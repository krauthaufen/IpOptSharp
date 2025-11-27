namespace IpOptSharp

open System
open System.Runtime.InteropServices
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
