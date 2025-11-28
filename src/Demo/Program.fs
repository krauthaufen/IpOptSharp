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
    Test.test()
    
    0
