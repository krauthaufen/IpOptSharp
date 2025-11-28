namespace IpOptSharp

open System
open System.Runtime.InteropServices
open Aardvark.Base.CodeFragment
open Microsoft.FSharp.NativeInterop
open Aardvark.Base
open IpOptSharp.Native

#nowarn "9"
#nowarn "1337"

module IpOptBuilderImplementation =
    
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
    
    type Builder = IpOptBuilderState -> IpOptBuilderState
    
    type ConstraintCreator() =
        static member CreateConstraint(f : scalar, l : float, h : float) =
            Constraint(f, l, h)
    
        static member CreateConstraint(v : V2s, l : float, h : float) =
            [|
                Constraint(v.X, l, h)
                Constraint(v.Y, l, h)
            |]
        static member CreateConstraint(v : V3s, l : float, h : float) =
            [|
                Constraint(v.X, l, h)
                Constraint(v.Y, l, h)
                Constraint(v.Z, l, h)
            |]
        static member CreateConstraint(v : V4s, l : float, h : float) =
            [|
                Constraint(v.X, l, h)
                Constraint(v.Y, l, h)
                Constraint(v.Z, l, h)
                Constraint(v.W, l, h)
            |]
        static member CreateConstraint(v : M22s, l : float, h : float) =
            [|
                Constraint(v.M00, l, h)
                Constraint(v.M01, l, h)
                Constraint(v.M10, l, h)
                Constraint(v.M11, l, h)
            |]
        
        static member CreateConstraint(v : M33s, l : float, h : float) =
            [|
                Constraint(v.M00, l, h)
                Constraint(v.M01, l, h)
                Constraint(v.M02, l, h)
                Constraint(v.M10, l, h)
                Constraint(v.M11, l, h)
                Constraint(v.M12, l, h)
                Constraint(v.M20, l, h)
                Constraint(v.M21, l, h)
                Constraint(v.M22, l, h)
            |]
        
        static member CreateConstraint(v : M44s, l : float, h : float) =
            [|
                Constraint(v.M00, l, h)
                Constraint(v.M01, l, h)
                Constraint(v.M02, l, h)
                Constraint(v.M03, l, h)
                Constraint(v.M10, l, h)
                Constraint(v.M11, l, h)
                Constraint(v.M12, l, h)
                Constraint(v.M13, l, h)
                Constraint(v.M20, l, h)
                Constraint(v.M21, l, h)
                Constraint(v.M22, l, h)
                Constraint(v.M23, l, h)
                Constraint(v.M30, l, h)
                Constraint(v.M31, l, h)
                Constraint(v.M32, l, h)
                Constraint(v.M33, l, h)
            |]
        
    let inline constaintAux< ^a, ^b, ^r when (^a or ^b) : (static member CreateConstraint : ^b * float * float -> ^r)> (d : ^a) (f : ^b) (l : float) (h : float) =
        ((^a or ^b) : (static member CreateConstraint : ^b * float * float -> ^r) (f, l, h))
    
    let inline constr f l h = constaintAux Unchecked.defaultof<ConstraintCreator> f l h
    
    type Objective = Objective of scalar
    
    let inline private pin (arr : 'a[]) ([<InlineIfLambda>] action : nativeptr<'a> -> 'r) =
        let gc = GCHandle.Alloc(arr, GCHandleType.Pinned)
        try action (NativePtr.ofNativeInt (gc.AddrOfPinnedObject()))
        finally gc.Free()
    
    type IpOptBuilder() =
        
        member inline x.Return(objective : scalar) : Builder =
            fun state ->
                { state with Objective = objective }
    
        member inline x.Delay([<InlineIfLambda>] f : unit -> Builder) : Builder =
            fun state -> f() state
                
        member inline x.Yield (c : Constraint[]) : Builder =
            fun state ->
                let newConstraints = c :: state.Constraints
                { state with Constraints = newConstraints; ConstraintCount = state.ConstraintCount + c.Length }
                
        member inline x.Yield (c : Constraint) : Builder =
            fun state ->
                let newConstraints = [| c |] :: state.Constraints
                { state with Constraints = newConstraints; ConstraintCount = state.ConstraintCount + 1 }
                
        member inline x.Yield (Objective o) : Builder =
            fun state -> { state with Objective = o }
                
        member inline x.Combine([<InlineIfLambda>] a : Builder, [<InlineIfLambda>] b : Builder) : Builder =
            fun state -> b (a state)
           
        member inline x.Run([<InlineIfLambda>] f : Builder) =
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
                
            let values = Array.zeroCreate<float> state.VariableOffset
            
            pin values <| fun ptr ->
                
                let m = state.ConstraintCount
                let n = state.VariableOffset
                
                
                let lowerBounds = Array.create n System.Double.NegativeInfinity
                let upperBounds = Array.create n System.Double.PositiveInfinity
                // pinner {
                //     let! ptr = values
                //     let! pLower = lowerBounds
                //     let! pUpper = upperBounds
                //     let! pCLower = cLowerBounds
                //     let! pCUpper = cUpperBounds
                //     
                //     
                //     do
                // }
                
                let handle = 
                    pin lowerBounds <| fun pLower ->
                        pin upperBounds <| fun pUpper ->
                            pin cLowerBounds <| fun pCLower ->
                                pin cUpperBounds <| fun pCUpper ->
                                    let numberOfJacobianEntries =
                                        updateCache (NativePtr.toNativeInt ptr)
                                        let mutable ci = 0
                                        let mutable cnt = 0
                                        for Constraint(f, l, h) in cCache do
                                            cnt <- cnt + f.Jacobian.Count
                                            cLowerBounds.[ci] <- l
                                            cUpperBounds.[ci] <- h
                                            ci <- ci + 1
                                        cnt
                                    
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
                                    
                                    
                                    for w in state.WriteTo do w ptr
                                    handle
                                
                if handle <> 0n then
                    let m = state.ConstraintCount
                    let n = state.VariableOffset
                    
                    
                    
                    
                    let g = Array.zeroCreate<float> m
                    let objVal = Array.zeroCreate<float> 1
                    let multG = Array.zeroCreate<float> m
                    let multXL = Array.zeroCreate<float> n
                    let multXU = Array.zeroCreate<float> n
                    
                    let gHandle = GCHandle.Alloc(g, GCHandleType.Pinned)
                    let objValHandle = GCHandle.Alloc(objVal, GCHandleType.Pinned)
                    let multGHandle = GCHandle.Alloc(multG, GCHandleType.Pinned)
                    let multXLHandle = GCHandle.Alloc(multXL, GCHandleType.Pinned)
                    let multXUHandle = GCHandle.Alloc(multXU, GCHandleType.Pinned)
                    
                    try
                        let status = IpoptNative.IpoptSolve(
                            handle,
                            NativePtr.toNativeInt ptr,
                            gHandle.AddrOfPinnedObject(),
                            objValHandle.AddrOfPinnedObject(),
                            multGHandle.AddrOfPinnedObject(),
                            multXLHandle.AddrOfPinnedObject(),
                            multXUHandle.AddrOfPinnedObject(),
                            nativeint 0)
                        
                        let returnStatus = enum<IpOptStatus>(status)
                        for r in state.UpdateInput do r ptr
                        (returnStatus, objVal.[0])
                    finally
                        gHandle.Free()
                        objValHandle.Free()
                        multGHandle.Free()
                        multXLHandle.Free()
                        multXUHandle.Free()
                else
                    (IpOptStatus.InternalError, System.Double.PositiveInfinity)
         
[<AutoOpen>]
module IpOptBuilder =
    open IpOptBuilderImplementation
    
    let inline minimize a =
        Objective a
        
    let inline maximize a =
        Objective -a
    
    let inline equal a b =
        constr (a - b) 0.0 0.0
        
    let inline greaterEqual a b =
        constr (a - b) 0.0 System.Double.PositiveInfinity
    
    let inline lessEqual a b =
        constr (b - a) 0.0 System.Double.PositiveInfinity
    
    let inline range a l h =
        constr ((a - l) / (h - l)) 0.0 1.0
    
    let ipopt = IpOptBuilder()
    
    

