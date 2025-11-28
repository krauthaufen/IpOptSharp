open System
open Aardvark.Base
open IpOptSharp
open Microsoft.FSharp.NativeInterop

#nowarn "9"
/// Example: 3D points connected by edges with geometric constraints
/// We have 4 points forming edges, with constraints:
/// - Edge 0-1 perpendicular to edge 1-2
/// - Edge 2-3 parallel to edge 0-1
/// - Minimize distance from target positions (regression-like cost)
//
// type Point3D = { X: float; Y: float; Z: float }
//
// module Vector3D =
//     let dot (a: Point3D) (b: Point3D) =
//         a.X * b.X + a.Y * b.Y + a.Z * b.Z
//     
//     let cross (a: Point3D) (b: Point3D) =
//         { X = a.Y * b.Z - a.Z * b.Y
//           Y = a.Z * b.X - a.X * b.Z
//           Z = a.X * b.Y - a.Y * b.X }
//     
//     let subtract (a: Point3D) (b: Point3D) =
//         { X = a.X - b.X; Y = a.Y - b.Y; Z = a.Z - b.Z }
//     
//     let normSq (v: Point3D) =
//         v.X * v.X + v.Y * v.Y + v.Z * v.Z

[<EntryPoint>]
let main argv =
    Aardvark.Init()
    
    IpOpt.test()
    exit 0
    
    // Target positions (we want to stay close to these)
    let targetPoints = 
        [| V3d(0.0, 0.0, 0.0)
           V3d(1.0, 0.0, 0.0)
           V3d(1.0, 1.0, 0.0)
           V3d(2.0, 1.0, 0.0) |]
    
    let numPoints = 4
    let n = numPoints * 3  // 12 variables (x,y,z for each point)
    
    // Objective: minimize squared distance from target positions
    let objective (x: nativeptr<float>) (cnt : int) (newX : bool) =
        let cnt = cnt / 3
        let x = NativePtr.ofNativeInt<V3d> (NativePtr.toNativeInt x)
        let mutable sumSq = 0.0
        for i in 0 .. cnt - 1 do
            let target = targetPoints.[i]
            let current = NativePtr.get x i
            sumSq <- sumSq + Vec.distanceSquared current target
        
        sumSq
       
    
    // Objective gradient
    let objectiveGradient (x: nativeptr<float>) (cnt : int) (newX : bool) (grad : nativeptr<float>)  =
        let cnt = cnt / 3
        let x = NativePtr.ofNativeInt<V3d> (NativePtr.toNativeInt x)
        let grad = NativePtr.ofNativeInt<V3d> (NativePtr.toNativeInt grad)
        
        for i in 0 .. cnt - 1 do
            let target = targetPoints.[i]
            let current = NativePtr.get x i
            let g = 2.0 * (current - target)
            NativePtr.set grad i g
    
    // Constraints:
    // g[0]: edge01 · edge12 = 0 (perpendicular)
    // g[1-3]: edge23 × edge01 = [0,0,0] (parallel, 3 components)
    let m = 4  // 4 constraint equations
    
    let constraints (points: nativeptr<float>) (cnt : int) (newX : bool) (c : nativeptr<float>) =
        let points = NativePtr.ofNativeInt<V3d> (NativePtr.toNativeInt points)
        let edge01 = NativePtr.get points 1 - NativePtr.get points 0
        let edge12 = NativePtr.get points 2 - NativePtr.get points 1
        let edge23 = NativePtr.get points 3 - NativePtr.get points 2
        
        let perp = Vec.dot edge01 edge12
        let parallelVec = Vec.cross edge23 edge01

        NativePtr.set c 0 perp
        NativePtr.set c 1 parallelVec.X
        NativePtr.set c 2 parallelVec.Y
        NativePtr.set c 3 parallelVec.Z
        
    
    // Constraint Jacobian (sparse format: row, col, value)
    // This is complex - we need partial derivatives of each constraint w.r.t. each variable
    let constraintJacobian (points: nativeptr<float>) (cnt : int) (newX : bool) =
        let points = NativePtr.ofNativeInt<V3d> (NativePtr.toNativeInt points)
        // For perpendicularity constraint g[0] = edge01 · edge12
        // edge01 = p1 - p0, edge12 = p2 - p1
        // ∂g[0]/∂p0 = -edge12
        // ∂g[0]/∂p1 = edge12 - edge01
        // ∂g[0]/∂p2 = edge01
        
        let edge01 = NativePtr.get points 1 - NativePtr.get points 0
        let edge12 = NativePtr.get points 2 - NativePtr.get points 1
        let edge23 = NativePtr.get points 3 - NativePtr.get points 2
        
        let jacobian = ResizeArray<struct(int * int * float)>()
        
        // g[0] = edge01 · edge12 derivatives
        // ∂/∂p0
        jacobian.Add(0, 0, -edge12.X)
        jacobian.Add(0, 1, -edge12.Y)
        jacobian.Add(0, 2, -edge12.Z)
        // ∂/∂p1
        jacobian.Add(0, 3, edge12.X - edge01.X)
        jacobian.Add(0, 4, edge12.Y - edge01.Y)
        jacobian.Add(0, 5, edge12.Z - edge01.Z)
        // ∂/∂p2
        jacobian.Add(0, 6, edge01.X)
        jacobian.Add(0, 7, edge01.Y)
        jacobian.Add(0, 8, edge01.Z)
        
        // g[1:3] = edge23 × edge01 derivatives (cross product)
        // edge23 = p3 - p2, edge01 = p1 - p0
        // cross(edge23, edge01) = [e23_y*e01_z - e23_z*e01_y,
        //                          e23_z*e01_x - e23_x*e01_z,
        //                          e23_x*e01_y - e23_y*e01_x]

        // g[1] = edge23.Y * edge01.Z - edge23.Z * edge01.Y
        // ∂g[1]/∂p0 = [-edge23.Y, edge23.Z, 0]
        jacobian.Add(1, 1, edge23.Z)
        jacobian.Add(1, 2, -edge23.Y)
        // ∂g[1]/∂p1 = [edge23.Y, -edge23.Z, 0]
        jacobian.Add(1, 4, -edge23.Z)
        jacobian.Add(1, 5, edge23.Y)
        // ∂g[1]/∂p2 = [edge01.Y, -edge01.Z, 0]
        jacobian.Add(1, 7, -edge01.Z)
        jacobian.Add(1, 8, edge01.Y)
        // ∂g[1]/∂p3 = [-edge01.Y, edge01.Z, 0]
        jacobian.Add(1, 10, edge01.Z)
        jacobian.Add(1, 11, -edge01.Y)

        // g[2] = edge23.Z * edge01.X - edge23.X * edge01.Z
        // ∂g[2]/∂p0 = [edge23.Z, 0, -edge23.X]
        jacobian.Add(2, 0, -edge23.Z)
        jacobian.Add(2, 2, edge23.X)
        // ∂g[2]/∂p1 = [-edge23.Z, 0, edge23.X]
        jacobian.Add(2, 3, edge23.Z)
        jacobian.Add(2, 5, -edge23.X)
        // ∂g[2]/∂p2 = [-edge01.Z, 0, edge01.X]
        jacobian.Add(2, 6, -edge01.Z)
        jacobian.Add(2, 8, edge01.X)
        // ∂g[2]/∂p3 = [edge01.Z, 0, -edge01.X]
        jacobian.Add(2, 9, edge01.Z)
        jacobian.Add(2, 11, -edge01.X)

        // g[3] = edge23.X * edge01.Y - edge23.Y * edge01.X
        // ∂g[3]/∂p0 = [0, -edge23.X, edge23.Y]
        jacobian.Add(3, 0, edge23.Y)
        jacobian.Add(3, 1, -edge23.X)
        // ∂g[3]/∂p1 = [0, edge23.X, -edge23.Y]
        jacobian.Add(3, 3, -edge23.Y)
        jacobian.Add(3, 4, edge23.X)
        // ∂g[3]/∂p2 = [0, edge01.X, -edge01.Y]
        jacobian.Add(3, 6, edge01.Y)
        jacobian.Add(3, 7, -edge01.X)
        // ∂g[3]/∂p3 = [0, -edge01.X, edge01.Y]
        jacobian.Add(3, 9, -edge01.Y)
        jacobian.Add(3, 10, edge01.X)

        jacobian.ToArray()
    
    // Bounds on variables (reasonable limits)
    let xLower = Array.create n System.Double.NegativeInfinity
    let xUpper = Array.create n System.Double.PositiveInfinity
    
    // Constraint bounds (all equality constraints = 0)
    let gLower = Array.create m 0.0
    let gUpper = Array.create m 0.0
    
    // Create IPOPT problem
    use problem = new IpoptProblem(
        n, xLower, xUpper,
        m, gLower, gUpper,
        objective,
        objectiveGradient,
        constraints,
        constraintJacobian)
    
    // Set options
    problem.SetOption("print_level", 5)  // 0 = no output, 5 = verbose
    problem.SetOption("tol", 1e-7)
    problem.SetOption("max_iter", 1000)
    problem.SetOption("hessian_approximation", "limited-memory")
    
    // Initial guess (start from a non-optimal configuration)
    let initialPoints =
        [| V3d(0.7, 0.5, 0.5)
           V3d(2.5, 0.5, 0.5)
           V3d(1.5, 1.5, 0.5)
           V3d(2.5, 1.5, 0.5) |]
        
    let constraintsArr (arr : V3d[]) =
        let res = Array.zeroCreate<float> m
        use cPtr = fixed res
        use ptr = fixed arr
        constraints (NativePtr.toNativeInt ptr |> NativePtr.ofNativeInt<float>) (arr.Length * 3) true cPtr
        res
        
    let objectiveArr (arr : V3d[]) =
        use ptr = fixed arr
        objective (NativePtr.toNativeInt ptr |> NativePtr.ofNativeInt<float>) (arr.Length * 3) true
        
    printfn "Starting optimization..."
    printfn "Initial objective: %f" (objectiveArr initialPoints)
    printfn "Initial constraints: %A" (constraintsArr initialPoints)
    printfn ""
    
    let x0 = initialPoints |> Array.collect (fun p -> [| float p.X; float p.Y; float p.Z |])
    
    // Solve
    let status, xSol, objVal = problem.Solve(x0)
    let sol =
        let arr = Array.zeroCreate<V3d> (n / 3)
        use ptr = fixed arr
        use xPtr = fixed xSol
        
        let src = System.Span<byte>(NativePtr.toVoidPtr xPtr, n * sizeof<float>)
        let dst = System.Span<byte>(NativePtr.toVoidPtr ptr, n * sizeof<float>)
        src.CopyTo dst
        arr
    
    
    printfn "\nOptimization complete!"
    printfn "Status: %A" status
    printfn "Final objective: %f" objVal
    printfn "Final constraints: %A" (constraintsArr sol)
    printfn ""
    
    // Print solution
    printfn "Solution points:"
    sol |> Array.iteri (fun i p ->
        printfn "  Point %d: (%.4f, %.4f, %.4f)" i p.X p.Y p.Z)
    
    // Verify constraints
    let edge01 = sol.[1] - sol.[0]
    let edge12 = sol.[2] - sol.[1]
    let edge23 = sol.[3] - sol.[2]
    
    printfn "\nVerification:"
    printfn "  Edge 0->1: (%.4f, %.4f, %.4f)" edge01.X edge01.Y edge01.Z
    printfn "  Edge 1->2: (%.4f, %.4f, %.4f)" edge12.X edge12.Y edge12.Z
    printfn "  Edge 2->3: (%.4f, %.4f, %.4f)" edge23.X edge23.Y edge23.Z
    printfn "  Dot(edge01, edge12) = %.6f (should be ~0)" (Vec.dot edge01 edge12)
    printfn "  Cross(edge23, edge01) = (%.6f, %.6f, %.6f) (should be ~0,0,0)" 
        (Vec.cross edge23 edge01).X
        (Vec.cross edge23 edge01).Y
        (Vec.cross edge23 edge01).Z
    
    0
