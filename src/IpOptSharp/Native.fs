namespace IpOptSharp.Native

open System
open System.Runtime.InteropServices
open System.Security

/// Callback delegates matching IPOPT's C interface
[<UnmanagedFunctionPointer(CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
type EvalF = delegate of int * nativeint * bool * nativeint -> bool

[<UnmanagedFunctionPointer(CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
type EvalGradF = delegate of int * nativeint * bool * nativeint -> bool

[<UnmanagedFunctionPointer(CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
type EvalG = delegate of int * nativeint * bool * int * nativeint -> bool

[<UnmanagedFunctionPointer(CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
type EvalJacG = delegate of int * nativeint * bool * int * int * nativeint * nativeint * nativeint -> bool

[<UnmanagedFunctionPointer(CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
type EvalH = delegate of int * nativeint * bool * float * int * nativeint * bool * int * nativeint * nativeint * nativeint -> bool

/// P/Invoke declarations for IPOPT C interface
module IpoptNative =
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
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
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
    extern void FreeIpoptProblem(nativeint ipopt_problem)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
    extern int AddIpoptStrOption(nativeint ipopt_problem, string keyword, string value)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
    extern int AddIpoptNumOption(nativeint ipopt_problem, string keyword, double value)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
    extern int AddIpoptIntOption(nativeint ipopt_problem, string keyword, int value)
    
    [<DllImport("ipopt", CallingConvention = CallingConvention.Cdecl); SuppressUnmanagedCodeSecurity>]
    extern int IpoptSolve(
        nativeint ipopt_problem,
        nativeint x,
        nativeint g,
        nativeint obj_val,
        nativeint mult_g,
        nativeint mult_x_L,
        nativeint mult_x_U,
        nativeint user_data)
