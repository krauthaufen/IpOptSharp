namespace IpOptSharp

open System
open System.Runtime.InteropServices
open Aardvark.Base.CodeFragment
open Microsoft.FSharp.NativeInterop

#nowarn "9"
/// IPOPT return codes
type IpoptApplicationReturnStatus =
    | Solve_Succeeded = 0
    | Solved_To_Acceptable_Level = 1
    | Infeasible_Problem_Detected = 2
    | Search_Direction_Becomes_Too_Small = 3
    | Diverging_Iterates = 4
    | User_Requested_Stop = 5
    | Feasible_Point_Found = 6
    | Maximum_Iterations_Exceeded = -1
    | Restoration_Failed = -2
    | Error_In_Step_Computation = -3
    | Maximum_CpuTime_Exceeded = -4
    | Not_Enough_Degrees_Of_Freedom = -10
    | Invalid_Problem_Definition = -11
    | Invalid_Option = -12
    | Invalid_Number_Detected = -13
    | Unrecoverable_Exception = -100
    | NonIpopt_Exception_Thrown = -101
    | Insufficient_Memory = -102
    | Internal_Error = -199

/// Callback delegates matching IPOPT's C interface
[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type EvalF = delegate of int * nativeint * bool * nativeint -> bool

[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type EvalGradF = delegate of int * nativeint * bool * nativeint -> bool

[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type EvalG = delegate of int * nativeint * bool * int * nativeint -> bool

[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type EvalJacG = delegate of int * nativeint * bool * int * int * nativeint * nativeint * nativeint -> bool

[<UnmanagedFunctionPointer(CallingConvention.Cdecl)>]
type EvalH = delegate of int * nativeint * bool * float * int * nativeint * bool * int * nativeint * nativeint * nativeint -> bool

/// P/Invoke declarations for IPOPT C interface
module IpoptNative =
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl)>]
    extern nativeint CreateIpoptProblem(
        int n,
        nativeint x_L,
        nativeint x_U,
        int m,
        nativeint g_L,
        nativeint g_U,
        int nele_jac,
        int nele_hess,
        int index_style,
        EvalF eval_f,
        EvalG eval_g,
        EvalGradF eval_grad_f,
        EvalJacG eval_jac_g,
        EvalH eval_h)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl)>]
    extern void FreeIpoptProblem(nativeint ipopt_problem)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl)>]
    extern int AddIpoptStrOption(nativeint ipopt_problem, string keyword, string value)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl)>]
    extern int AddIpoptNumOption(nativeint ipopt_problem, string keyword, double value)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl)>]
    extern int AddIpoptIntOption(nativeint ipopt_problem, string keyword, int value)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl)>]
    extern int IpoptSolve(
        nativeint ipopt_problem,
        nativeint x,
        nativeint g,
        nativeint obj_val,
        nativeint mult_g,
        nativeint mult_x_L,
        nativeint mult_x_U,
        nativeint user_data)

/// High-level F# interface for IPOPT
type IpoptProblem(
    n: int,
    xLower: float[],
    xUpper: float[],
    m: int,
    gLower: float[],
    gUpper: float[],
    objective: nativeptr<float> -> int -> bool -> float,
    objectiveGradient: nativeptr<float> -> int -> bool -> nativeptr<float> -> unit,
    constraints: nativeptr<float> -> int -> bool -> nativeptr<float> -> unit,
    constraintJacobian: nativeptr<float> -> int -> bool -> struct(int * int * float)[]) =
    
    let mutable problemHandle = nativeint 0
    let mutable disposed = false
    
    // Keep delegates alive to prevent garbage collection
    let mutable evalFDelegate: EvalF option = None
    let mutable evalGradFDelegate: EvalGradF option = None
    let mutable evalGDelegate: EvalG option = None
    let mutable evalJacGDelegate: EvalJacG option = None

    do
        // Create callback for objective function
        let evalF = EvalF(fun n x newX objValue ->
            try
                let f = objective (NativePtr.ofNativeInt x) n newX
                NativePtr.write (NativePtr.ofNativeInt objValue) f
                true
            with _ ->
                false
        )
        evalFDelegate <- Some evalF
        
        // Create callback for objective gradient
        let evalGradF = EvalGradF(fun n x newX grad ->
            try
                let gradArr = objectiveGradient (NativePtr.ofNativeInt x) n newX (NativePtr.ofNativeInt grad)
                true
            with _ -> false
        )
        evalGradFDelegate <- Some evalGradF
        
        // Create callback for constraints
        let evalG = EvalG(fun n x newX m g ->
            try
                constraints (NativePtr.ofNativeInt x) n newX (NativePtr.ofNativeInt g)
                true
            with _ ->
                false
        )
        evalGDelegate <- Some evalG
        
        // Create callback for constraint Jacobian
        let evalJacG = EvalJacG(fun n x newX m neleJac iRow jCol values ->
            try
                if values = 0n then
                    // Return structure
                    
                    let jac = 
                        if x = 0n then
                            let arr = Array.zeroCreate<float> n
                            use ptr = fixed arr
                            constraintJacobian ptr n true
                        else
                            constraintJacobian (NativePtr.ofNativeInt x) n newX
                    
                    let pRows = NativePtr.ofNativeInt iRow
                    let pCols = NativePtr.ofNativeInt jCol
                    // let rows = Array.zeroCreate n
                    // let cols = Array.zeroCreate n
                    for i in 0 .. jac.Length - 1 do
                        let struct(row, col, _) = jac.[i]
                        NativePtr.set pRows i row
                        NativePtr.set pCols i col
                else
                    // Return structure
                    let jac = constraintJacobian (NativePtr.ofNativeInt x) n newX
                    
                    let pValues = NativePtr.ofNativeInt values
                    // let rows = Array.zeroCreate n
                    // let cols = Array.zeroCreate n
                    for i in 0 .. jac.Length - 1 do
                        let struct(_, _, v) = jac.[i]
                        NativePtr.set pValues i v
                true
            with _ -> false
        )
        evalJacGDelegate <- Some evalJacG
        
        // Dummy Hessian callback (use limited-memory approximation)
        let evalH = EvalH(fun _ _ _ _ _ _ _ _ _ _ _ -> true)
        
        // Calculate Jacobian sparsity pattern
        let jacPattern =
            let empty = Array.zeroCreate n
            use ptr = fixed empty
            constraintJacobian ptr n true
        let neleJac =
            jacPattern.Length
        
        // Pin arrays for P/Invoke
        let xLowerHandle = GCHandle.Alloc(xLower, GCHandleType.Pinned)
        let xUpperHandle = GCHandle.Alloc(xUpper, GCHandleType.Pinned)
        let gLowerHandle = GCHandle.Alloc(gLower, GCHandleType.Pinned)
        let gUpperHandle = GCHandle.Alloc(gUpper, GCHandleType.Pinned)
        
        try
            problemHandle <- IpoptNative.CreateIpoptProblem(
                n,
                xLowerHandle.AddrOfPinnedObject(),
                xUpperHandle.AddrOfPinnedObject(),
                m,
                gLowerHandle.AddrOfPinnedObject(),
                gUpperHandle.AddrOfPinnedObject(),
                neleJac,
                0, // Use limited-memory Hessian approximation
                0, // C-style indexing
                evalF,
                evalG,
                evalGradF,
                evalJacG,
                evalH)
            
            if problemHandle = nativeint 0 then
                failwith "Failed to create IPOPT problem"
        finally
            xLowerHandle.Free()
            xUpperHandle.Free()
            gLowerHandle.Free()
            gUpperHandle.Free()
    
    member this.SetOption(name: string, value: float) =
        IpoptNative.AddIpoptNumOption(problemHandle, name, value) |> ignore
    
    member this.SetOption(name: string, value: int) =
        IpoptNative.AddIpoptIntOption(problemHandle, name, value) |> ignore
    
    member this.SetOption(name: string, value: string) =
        IpoptNative.AddIpoptStrOption(problemHandle, name, value) |> ignore
    
    member this.Solve(x0: float[]) =
        let x = Array.copy x0
        let g = Array.zeroCreate<float> m
        let objVal = Array.zeroCreate<float> 1
        let multG = Array.zeroCreate<float> m
        let multXL = Array.zeroCreate<float> n
        let multXU = Array.zeroCreate<float> n
        
        let xHandle = GCHandle.Alloc(x, GCHandleType.Pinned)
        let gHandle = GCHandle.Alloc(g, GCHandleType.Pinned)
        let objValHandle = GCHandle.Alloc(objVal, GCHandleType.Pinned)
        let multGHandle = GCHandle.Alloc(multG, GCHandleType.Pinned)
        let multXLHandle = GCHandle.Alloc(multXL, GCHandleType.Pinned)
        let multXUHandle = GCHandle.Alloc(multXU, GCHandleType.Pinned)
        
        try
            let status = IpoptNative.IpoptSolve(
                problemHandle,
                xHandle.AddrOfPinnedObject(),
                gHandle.AddrOfPinnedObject(),
                objValHandle.AddrOfPinnedObject(),
                multGHandle.AddrOfPinnedObject(),
                multXLHandle.AddrOfPinnedObject(),
                multXUHandle.AddrOfPinnedObject(),
                nativeint 0)
            
            let returnStatus = enum<IpoptApplicationReturnStatus>(status)
            (returnStatus, x, objVal.[0])
        finally
            xHandle.Free()
            gHandle.Free()
            objValHandle.Free()
            multGHandle.Free()
            multXLHandle.Free()
            multXUHandle.Free()
    
    interface IDisposable with
        member this.Dispose() =
            if not disposed then
                if problemHandle <> nativeint 0 then
                    IpoptNative.FreeIpoptProblem(problemHandle)
                    problemHandle <- nativeint 0
                disposed <- true

    // n: int,
    // xLower: float[],
    // xUpper: float[],
    // m: int,
    // gLower: float[],
    // gUpper: float[],
    // objective: nativeptr<float> -> int -> bool -> float,
    // objectiveGradient: nativeptr<float> -> int -> bool -> nativeptr<float> -> unit,
    // constraints: nativeptr<float> -> int -> bool -> nativeptr<float> -> unit,
    // constraintJacobian: nativeptr<float> -> int -> bool -> struct(int * int * float)[]) =
    
open Aardvark.Base
    
type IpOptVariable =
    abstract NumberOfDoubles : int
    abstract CopyTo : nativeptr<float> -> unit
    abstract ReadFrom : nativeptr<float> -> unit
    
[<AbstractClass>]
type IpOptVariable<'T>(numberOfDoubles : int) =
    abstract member CopyTo : nativeptr<float> -> unit
    abstract member ReadFrom : nativeptr<float> -> unit
    abstract member Value : 'T
    
    interface IpOptVariable with
        member x.NumberOfDoubles = numberOfDoubles
        member x.CopyTo(ptr: nativeptr<float>) = x.CopyTo(ptr)
        member x.ReadFrom(ptr: nativeptr<float>) = x.ReadFrom(ptr)
        
    
type IpOptProblem =
    {
        Variables : IpOptVariable[]
        Objective : IpOptVariable[] -> scalar
    }

module IpOpt =
    
    [<Struct>]
    type Constraint = Constraint of scalar * float * float
    
    
    type IpOptBuilderState =
        {
            UpdateScalars : list<nativeptr<float> -> unit>
            UpdateInput : list<nativeptr<float> -> unit>
            WriteTo : list<nativeptr<float> -> unit>
            VariableOffset : int
            Objective : scalar
            Constraints : list<Constraint[]>
            ConstraintCount : int
            CachedArrays : System.Collections.Generic.Dictionary<obj, obj>
        }
    
    type BB = IpOptBuilderState -> IpOptBuilderState
    
    type ConstraintCreator() =
        static member CreateConstraint(f : scalar, l : float, h : float) =
            Constraint(f, l, h)
    
        static member CreateConstraint(v : V2s, l : float, h : float) =
            [|
                Constraint(v.X, l, h)
                Constraint(v.Y, l, h)
            |]
    let inline constaintAux< ^a, ^b, ^r when (^a or ^b) : (static member CreateConstraint : ^b * float * float -> ^r)> (d : ^a) (f : ^b) (l : float) (h : float) =
        ((^a or ^b) : (static member CreateConstraint : ^b * float * float -> ^r) (f, l, h))
    
    let inline constr f l h = constaintAux Unchecked.defaultof<ConstraintCreator> f l h
    
    let inline equal a b =
        constr (a - b) 0.0 0.0
        
    let inline greaterEqual a b =
        constr (a - b) 0.0 System.Double.PositiveInfinity
    
    let inline lessEqual a b =
        constr (b - a) 0.0 System.Double.PositiveInfinity
    
    let inline range a l h =
        constr ((a - l) / (h - l)) 0.0 1.0
    
    [<Struct>]
    type Objective = Objective of scalar
    
    let inline minimize a = Objective a
    let inline maximize a = Objective -a
    
    type IpOptBuilder() =
        
        member inline x.Bind(value : float[], [<InlineIfLambda>] cont : scalar[] -> BB) : BB =
            fun state ->
                match state.CachedArrays.TryGetValue value with
                | (true, (:? array<scalar> as cached)) ->
                    cont cached state
                | _ ->
                    let sValue =
                        value |> Array.mapi (fun i v ->
                            scalar.Variable(state.VariableOffset + i, v)
                        )
                        
                    let updateScalars(src : nativeptr<float>) =
                        for i in 0 .. value.Length - 1 do
                            sValue.[i].Value <- NativePtr.get src (state.VariableOffset + i)
                        
                    let updateInput(src : nativeptr<float>) =
                        for i in 0 .. value.Length - 1 do
                            value.[i] <- NativePtr.get src (state.VariableOffset + i)
                        
                    let writeTo (dst : nativeptr<float>) =
                        for i in 0 .. value.Length - 1 do
                            NativePtr.set dst (state.VariableOffset + i) value.[i]
                        
                    state.CachedArrays.[value] <- sValue :> System.Array
                    cont sValue {
                        state with
                            UpdateScalars = updateScalars :: state.UpdateScalars
                            UpdateInput = updateInput :: state.UpdateInput
                            WriteTo = writeTo :: state.WriteTo
                            VariableOffset = state.VariableOffset + value.Length
                    }
        
        member inline x.Bind(value : V2d[], [<InlineIfLambda>] cont : V2s[] -> BB) : BB =
            fun state ->
                match state.CachedArrays.TryGetValue value with
                | (true, (:? array<V2s> as cached)) ->
                    cont cached state
                | _ ->
                    let sValue =
                        value |> Array.mapi (fun i v ->
                            let bi = 2*i
                            V2s(
                                scalar.Variable(state.VariableOffset + bi, v.X),
                                scalar.Variable(state.VariableOffset + bi + 1, v.Y)
                            )
                        )
                        
                    let updateScalars(src : nativeptr<float>) =
                        let mutable si = state.VariableOffset
                        for i in 0 .. value.Length - 1 do
                            let x = NativePtr.get src si
                            si <- si + 1
                            let y = NativePtr.get src si
                            si <- si + 1
                            sValue.[i].X.Value <- x
                            sValue.[i].Y.Value <- y
                        
                    let updateInput(src : nativeptr<float>) =
                        let mutable si = state.VariableOffset
                        for i in 0 .. value.Length - 1 do
                            let x = NativePtr.get src si
                            si <- si + 1
                            let y = NativePtr.get src si
                            si <- si + 1
                            value.[i] <- V2d(x,y)
                        
                    let writeTo (dst : nativeptr<float>) =
                        let mutable di = state.VariableOffset
                        for i in 0 .. value.Length - 1 do
                            NativePtr.set dst di value.[i].X
                            di <- di + 1
                            NativePtr.set dst di value.[i].Y
                            di <- di + 1
                        
                    state.CachedArrays.[value] <- sValue :> System.Array
                    cont sValue {
                        state with
                            UpdateScalars = updateScalars :: state.UpdateScalars
                            UpdateInput = updateInput :: state.UpdateInput
                            WriteTo = writeTo :: state.WriteTo
                            VariableOffset = state.VariableOffset + 2*value.Length
                    }
        
        member inline x.Bind(value : ref<float>, [<InlineIfLambda>] cont : scalar -> BB) : BB =
            fun state ->
                match state.CachedArrays.TryGetValue value with
                | (true, (:? ref<scalar> as cached)) ->
                    cont cached.Value state
                | _ ->
                    let sValue =
                        ref (scalar.Variable(state.VariableOffset, value.Value))
                        
                    let updateScalars(src : nativeptr<float>) =
                        sValue.contents.Value <- NativePtr.get src state.VariableOffset
                        
                    let updateInput(src : nativeptr<float>) =
                        value.Value <- NativePtr.get src state.VariableOffset
                        
                    let writeTo (dst : nativeptr<float>) =
                        NativePtr.set dst state.VariableOffset value.Value
                        
                    state.CachedArrays.[value] <- sValue
                    cont sValue.Value {
                        state with
                            UpdateScalars = updateScalars :: state.UpdateScalars
                            UpdateInput = updateInput :: state.UpdateInput
                            WriteTo = writeTo :: state.WriteTo
                            VariableOffset = state.VariableOffset + 1
                    }
        
        member inline x.Bind(value : ref<V2d>, [<InlineIfLambda>] cont : V2s -> BB) : BB =
            fun state ->
                match state.CachedArrays.TryGetValue value with
                | (true, (:? ref<V2s> as cached)) ->
                    cont cached.Value state
                | _ ->
                    let sValue =
                        let v = value.Value
                        ref (
                            V2s(
                                scalar.Variable(state.VariableOffset, v.X),
                                scalar.Variable(state.VariableOffset + 1, v.Y)
                            )
                        )
                        
                    let updateScalars(src : nativeptr<float>) =
                        let mutable si = state.VariableOffset
                        let x = NativePtr.get src si
                        si <- si + 1
                        let y = NativePtr.get src si
                        si <- si + 1
                        sValue.contents.X.Value <- x
                        sValue.contents.Y.Value <- y
                        
                    let updateInput(src : nativeptr<float>) =
                        let mutable si = state.VariableOffset
                        let x = NativePtr.get src si
                        si <- si + 1
                        let y = NativePtr.get src si
                        si <- si + 1
                        value.Value <- V2d(x,y)
                        
                    let writeTo (dst : nativeptr<float>) =
                        let mutable di = state.VariableOffset
                        NativePtr.set dst di value.Value.X
                        di <- di + 1
                        NativePtr.set dst di value.Value.Y
                        di <- di + 1
                        
                    state.CachedArrays.[value] <- sValue
                    cont sValue.Value {
                        state with
                            UpdateScalars = updateScalars :: state.UpdateScalars
                            UpdateInput = updateInput :: state.UpdateInput
                            WriteTo = writeTo :: state.WriteTo
                            VariableOffset = state.VariableOffset + 2
                    }
        
    
        member inline x.Return(objective : scalar) : BB =
            fun state ->
                { state with Objective = objective }
    
        member inline x.Delay([<InlineIfLambda>] f : unit -> BB) : BB =
            fun state -> f() state
                
        member inline x.Yield (c : Constraint[]) : BB =
            fun state ->
                let newConstraints = c :: state.Constraints
                { state with Constraints = newConstraints; ConstraintCount = state.ConstraintCount + c.Length }
                
        member inline x.Yield (c : Constraint) : BB =
            fun state ->
                let newConstraints = [| c |] :: state.Constraints
                { state with Constraints = newConstraints; ConstraintCount = state.ConstraintCount + 1 }
                
        member inline x.Yield (Objective o) : BB =
            fun state -> { state with Objective = o }
                
        member inline x.Combine([<InlineIfLambda>] a : BB, [<InlineIfLambda>] b : BB) : BB =
            fun state -> b (a state)
           
        member inline x.Run([<InlineIfLambda>] f : BB) =
            let initialState = { CachedArrays = System.Collections.Generic.Dictionary(); VariableOffset = 0; UpdateScalars = []; UpdateInput = []; WriteTo = []; Objective = scalar.Zero; Constraints = []; ConstraintCount = 0 }
            let state = f initialState
            
            let mutable oCache = scalar.Zero
            let mutable cCache = Array.zeroCreate<Constraint> state.ConstraintCount
            
            //let evalState = { initialState with Evaluation = true }
            let updateCache (x : nativeint) =
                for update in state.UpdateScalars do
                    update (NativePtr.ofNativeInt x)
                   
                let state = f initialState
                let res = state.Objective
                let mutable oi = 0
                for cs in state.Constraints do
                    for c in cs do
                        cCache.[oi] <- c
                        oi <- oi + 1
                oCache <- res
                
            
            let objectiveFunc =
                EvalF (fun n x newX objValue ->
                    if newX then updateCache x
                    NativePtr.write (NativePtr.ofNativeInt objValue) oCache.Value
                    true
                )
            
            let objectiveGradientFunc =
                EvalGradF (fun n x newX grad ->
                    if newX then updateCache x
                    let gradPtr = NativePtr.ofNativeInt grad
                    for KeyValue(i, v) in oCache.Jacobian do
                        NativePtr.set gradPtr i v
                    true
                )
                
            let cLowerBounds = Array.zeroCreate<float> state.ConstraintCount
            let cUpperBounds = Array.zeroCreate<float> state.ConstraintCount
                
            let constraintsFunc =
                EvalG (fun n x newX m g ->
                    if newX then updateCache x
                    let gPtr = NativePtr.ofNativeInt g
                    
                    let mutable ci = 0
                    for Constraint(f, l, h) in cCache do
                        NativePtr.set gPtr ci f.Value
                        ci <- ci + 1
                        
                    true
                )
                
            let constraintJacobianFunc =
                EvalJacG (fun n x newX m neleJac iRow jCol values ->
                    if newX then updateCache x
                    if values = 0n then
                        // return structure
                        let pRows = NativePtr.ofNativeInt iRow
                        let pCols = NativePtr.ofNativeInt jCol
                        let mutable ji = 0
                        for ci in 0 .. cCache.Length - 1 do
                            let (Constraint(f, _, _)) = cCache.[ci]
                            for KeyValue(vi, _) in f.Jacobian do
                                NativePtr.set pRows ji ci
                                NativePtr.set pCols ji vi
                                ji <- ji + 1
                    else
                        // return values
                        let pValues = NativePtr.ofNativeInt values
                        let mutable ji = 0
                        for ci in 0 .. cCache.Length - 1 do
                            let (Constraint(f, _, _)) = cCache.[ci]
                            for KeyValue(_, v) in f.Jacobian do
                                NativePtr.set pValues ji v
                                ji <- ji + 1
                                
                    true
                )
                
            let numberOfJacobianEntries =
                let xs = Array.zeroCreate<float> state.VariableOffset
                use ptr = fixed xs
                updateCache (NativePtr.toNativeInt ptr)
                
                let mutable ci = 0
                let mutable cnt = 0
                for Constraint(f, l, h) in cCache do
                    cnt <- cnt + f.Jacobian.Count
                    cLowerBounds.[ci] <- l
                    cUpperBounds.[ci] <- h
                    ci <- ci + 1
                cnt
            
                
            let lowerBounds = Array.create state.VariableOffset System.Double.NegativeInfinity
            let upperBounds = Array.create state.VariableOffset System.Double.PositiveInfinity
                
            use pLower = fixed lowerBounds
            use pUpper = fixed upperBounds
            use pCLower = fixed cLowerBounds
            use pCUpper = fixed cUpperBounds
            let handle = 
                IpoptNative.CreateIpoptProblem(
                    state.VariableOffset,
                    NativePtr.toNativeInt pLower,
                    NativePtr.toNativeInt pUpper,
                    state.ConstraintCount,
                    NativePtr.toNativeInt pCLower,
                    NativePtr.toNativeInt pCUpper,
                    numberOfJacobianEntries,
                    0, 0,
                    objectiveFunc,
                    constraintsFunc,
                    objectiveGradientFunc,
                    constraintJacobianFunc,
                    EvalH(fun  _ _ _ _ _ _ _ _ _ _ _ -> false)
                )
            
            IpoptNative.AddIpoptIntOption(handle, "print_level", 5) |> ignore
            IpoptNative.AddIpoptNumOption(handle, "tol", 1e-7) |> ignore
            IpoptNative.AddIpoptIntOption(handle, "max_iter", 1000) |> ignore
            IpoptNative.AddIpoptStrOption(handle, "hessian_approximation", "limited-memory") |> ignore
            
            
            
            
            let m = state.ConstraintCount
            let n = state.VariableOffset
            
            let x = Array.zeroCreate<float> n
            use ptr = fixed x
            for w in state.WriteTo do  w ptr
            
            
            
            let g = Array.zeroCreate<float> m
            let objVal = Array.zeroCreate<float> 1
            let multG = Array.zeroCreate<float> m
            let multXL = Array.zeroCreate<float> n
            let multXU = Array.zeroCreate<float> n
            
            let xHandle = GCHandle.Alloc(x, GCHandleType.Pinned)
            let gHandle = GCHandle.Alloc(g, GCHandleType.Pinned)
            let objValHandle = GCHandle.Alloc(objVal, GCHandleType.Pinned)
            let multGHandle = GCHandle.Alloc(multG, GCHandleType.Pinned)
            let multXLHandle = GCHandle.Alloc(multXL, GCHandleType.Pinned)
            let multXUHandle = GCHandle.Alloc(multXU, GCHandleType.Pinned)
            
            try
                let status = IpoptNative.IpoptSolve(
                    handle,
                    xHandle.AddrOfPinnedObject(),
                    gHandle.AddrOfPinnedObject(),
                    objValHandle.AddrOfPinnedObject(),
                    multGHandle.AddrOfPinnedObject(),
                    multXLHandle.AddrOfPinnedObject(),
                    multXUHandle.AddrOfPinnedObject(),
                    nativeint 0)
                
                let returnStatus = enum<IpoptApplicationReturnStatus>(status)
                for r in state.UpdateInput do r ptr
                (returnStatus, objVal.[0])
            finally
                xHandle.Free()
                gHandle.Free()
                objValHandle.Free()
                multGHandle.Free()
                multXLHandle.Free()
                multXUHandle.Free()
            
    
    let optimize = IpOptBuilder()
    
    let inline (/<+) a (b, c) = range a b c
    let inline (?=) a b  = equal a b
    let inline (?>=) a b  = greaterEqual a b
    let inline (?<=) a b  = lessEqual a b
    
    let test () =
        let a = ref (V2d(1.0, 2.0))
        let b = ref 3.0
        let (status, objective) = 
            optimize {
                let! a = a
                let! b = b
                
                // objective
                minimize (sqr (a.X - a.Y * b))
                
                // constraints
                range b -0.1 0.1
                a.Length ?= 1.0
            }

        printfn "status: %A" status 
        printfn "obj:    %.4f" objective 
        printfn "a:      %s (length: %.4f)" (a.Value.ToString "0.0000") a.Value.Length
        printfn "b:      %.4f" b.Value       
        // status: Solve_Succeeded
        // obj:    0.0000
        // a:      [0.0744, 0.9972] (length: 1.0000)
        // b:      0.0746

    


