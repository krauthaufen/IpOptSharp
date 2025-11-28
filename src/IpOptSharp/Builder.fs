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
        
    type ScalarAdapter<'a, 'b> =
        abstract DoubleCount : int
        abstract ReadScalar : nativeptr<float> * int -> 'b
        abstract ReadValue : nativeptr<float> * int -> 'a
        abstract WriteTo : nativeptr<float> * int * 'a -> unit
        abstract GetValue : 'b -> 'a
        abstract Variable : 'a * int -> 'b

    type ScalarAdapterInstances() =
        static member GetScalarAdapter (_dummy : float) =
            { new ScalarAdapter<float, scalar> with
                member x.DoubleCount = 1
                member x.ReadScalar(ptr, id) = scalar.Variable(id, NativePtr.get ptr id)
                member x.ReadValue(ptr, id) = NativePtr.get ptr id
                member x.WriteTo(ptr, id, value) = NativePtr.set ptr id value
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) = scalar.Variable(id, value)
            }
        
        static member GetScalarAdapter (_dummy : V2d) =
            { new ScalarAdapter<V2d, V2s> with
                member x.DoubleCount = 2
                member x.ReadScalar(ptr, id) = V2s(scalar.Variable(id, NativePtr.get ptr id), scalar.Variable(id+1, NativePtr.get ptr (id+1)))
                member x.ReadValue(ptr, id) = V2d(NativePtr.get ptr id, NativePtr.get ptr (id+1))
                member x.WriteTo(ptr, id, value) =
                    NativePtr.set ptr id value.X
                    NativePtr.set ptr (id+1) value.Y
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) = V2s(scalar.Variable(id, value.X), scalar.Variable(id+1, value.Y))
            }
        
        static member GetScalarAdapter (_dummy : V3d) =
            { new ScalarAdapter<V3d, V3s> with
                member x.DoubleCount = 3
                member x.ReadScalar(ptr, id) = V3s(scalar.Variable(id, NativePtr.get ptr id), scalar.Variable(id+1, NativePtr.get ptr (id+1)), scalar.Variable(id+2, NativePtr.get ptr (id+2)))
                member x.ReadValue(ptr, id) = V3d(NativePtr.get ptr id, NativePtr.get ptr (id+1), NativePtr.get ptr (id+2))
                member x.WriteTo(ptr, id, value) =
                    NativePtr.set ptr id value.X
                    NativePtr.set ptr (id+1) value.Y
                    NativePtr.set ptr (id+2) value.Z
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) = V3s(scalar.Variable(id, value.X), scalar.Variable(id+1, value.Y), scalar.Variable(id+2, value.Z))
            }
            
        static member GetScalarAdapter (_dummy : V4d) =
            { new ScalarAdapter<V4d, V4s> with
                member x.DoubleCount = 4
                member x.ReadScalar(ptr, id) =
                    V4s.CeresRead(ptr, id)
                member x.ReadValue(ptr, id) =
                    V4d(
                        NativePtr.get ptr id,
                        NativePtr.get ptr (id+1),
                        NativePtr.get ptr (id+2),
                        NativePtr.get ptr (id+3)
                    )
                member x.WriteTo(ptr, id, value) =
                    NativePtr.set ptr id value.X
                    NativePtr.set ptr (id+1) value.Y
                    NativePtr.set ptr (id+2) value.Z
                    NativePtr.set ptr (id+3) value.W
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) =
                    V4s(
                        scalar.Variable(id, value.X),
                        scalar.Variable(id+1, value.Y),
                        scalar.Variable(id+2, value.Z),
                        scalar.Variable(id+3, value.W)
                    )
                    
            }
            
        static member GetScalarAdapter (_dummy : M22d) =
            { new ScalarAdapter<M22d, M22s> with
                member x.DoubleCount = 4
                member x.ReadScalar(ptr, id) =
                    M22s.CeresRead(ptr, id)
                member x.ReadValue(ptr, id) =
                    M22d(
                        NativePtr.get ptr id,
                        NativePtr.get ptr (id+1),
                        NativePtr.get ptr (id+2),
                        NativePtr.get ptr (id+3)
                    )
                member x.WriteTo(ptr, id, value) =
                    NativePtr.set ptr id value.M00
                    NativePtr.set ptr (id+1) value.M01
                    NativePtr.set ptr (id+2) value.M10
                    NativePtr.set ptr (id+3) value.M11
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) =
                    M22s(
                        scalar.Variable(id, value.M00),
                        scalar.Variable(id+1, value.M01),
                        scalar.Variable(id+2, value.M10),
                        scalar.Variable(id+3, value.M11)
                    )
                    
            }
            
        static member GetScalarAdapter (_dummy : M33d) =
            { new ScalarAdapter<M33d, M33s> with
                member x.DoubleCount = 9
                member x.ReadScalar(ptr, id) =
                    M33s.CeresRead(ptr, id)
                member x.ReadValue(ptr, id) =
                    M33d(
                        NativePtr.get ptr id,
                        NativePtr.get ptr (id+1),
                        NativePtr.get ptr (id+2),
                        NativePtr.get ptr (id+3),
                        NativePtr.get ptr (id+4),
                        NativePtr.get ptr (id+5),
                        NativePtr.get ptr (id+6),
                        NativePtr.get ptr (id+7),
                        NativePtr.get ptr (id+8)
                    )
                member x.WriteTo(ptr, id, value) =
                    NativePtr.set ptr id value.M00
                    NativePtr.set ptr (id+1) value.M01
                    NativePtr.set ptr (id+2) value.M02
                    NativePtr.set ptr (id+3) value.M10
                    NativePtr.set ptr (id+4) value.M11
                    NativePtr.set ptr (id+5) value.M12
                    NativePtr.set ptr (id+6) value.M20
                    NativePtr.set ptr (id+7) value.M21
                    NativePtr.set ptr (id+8) value.M22
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) =
                    M33s(
                        scalar.Variable(id, value.M00),
                        scalar.Variable(id+1, value.M01),
                        scalar.Variable(id+2, value.M02),
                        scalar.Variable(id+3, value.M10),
                        scalar.Variable(id+4, value.M11),
                        scalar.Variable(id+5, value.M12),
                        scalar.Variable(id+6, value.M20),
                        scalar.Variable(id+7, value.M21),
                        scalar.Variable(id+8, value.M22)
                    )
                    
            }
            
        static member GetScalarAdapter (_dummy : M44d) =
            { new ScalarAdapter<M44d, M44s> with
                member x.DoubleCount = 16
                member x.ReadScalar(ptr, id) =
                    M44s.CeresRead(ptr, id)
                member x.ReadValue(ptr, id) =
                    M44d(
                        NativePtr.get ptr id,
                        NativePtr.get ptr (id+1),
                        NativePtr.get ptr (id+2),
                        NativePtr.get ptr (id+3),
                        NativePtr.get ptr (id+4),
                        NativePtr.get ptr (id+5),
                        NativePtr.get ptr (id+6),
                        NativePtr.get ptr (id+7),
                        NativePtr.get ptr (id+8),
                        NativePtr.get ptr (id+9),
                        NativePtr.get ptr (id+10),
                        NativePtr.get ptr (id+11),
                        NativePtr.get ptr (id+12),
                        NativePtr.get ptr (id+13),
                        NativePtr.get ptr (id+14),
                        NativePtr.get ptr (id+15)
                        
                    )
                member x.WriteTo(ptr, id, value) =
                    NativePtr.set ptr id value.M00
                    NativePtr.set ptr (id+1) value.M01
                    NativePtr.set ptr (id+2) value.M02
                    NativePtr.set ptr (id+3) value.M03
                    NativePtr.set ptr (id+4) value.M10
                    NativePtr.set ptr (id+5) value.M11
                    NativePtr.set ptr (id+6) value.M12
                    NativePtr.set ptr (id+7) value.M13
                    NativePtr.set ptr (id+8) value.M20
                    NativePtr.set ptr (id+9) value.M21
                    NativePtr.set ptr (id+10) value.M22
                    NativePtr.set ptr (id+11) value.M23
                    NativePtr.set ptr (id+12) value.M30
                    NativePtr.set ptr (id+13) value.M31
                    NativePtr.set ptr (id+14) value.M32
                    NativePtr.set ptr (id+15) value.M33
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) =
                    M44s(
                        scalar.Variable(id, value.M00),
                        scalar.Variable(id+1, value.M01),
                        scalar.Variable(id+2, value.M02),
                        scalar.Variable(id+3, value.M03),
                        scalar.Variable(id+4, value.M10),
                        scalar.Variable(id+5, value.M11),
                        scalar.Variable(id+6, value.M12),
                        scalar.Variable(id+7, value.M13),
                        scalar.Variable(id+8, value.M20),
                        scalar.Variable(id+9, value.M21),
                        scalar.Variable(id+10, value.M22),
                        scalar.Variable(id+11, value.M23),
                        scalar.Variable(id+12, value.M30),
                        scalar.Variable(id+13, value.M31),
                        scalar.Variable(id+14, value.M32),
                        scalar.Variable(id+15, value.M33)
                    )
                    
            }
            
        static member GetScalarAdapter (_dummy : Rot2d) =
            { new ScalarAdapter<Rot2d, Rot2s> with
                member x.DoubleCount = 1
                member x.ReadScalar(ptr, id) = Rot2s(scalar.Variable(id, NativePtr.get ptr id))
                member x.ReadValue(ptr, id) = Rot2d(NativePtr.get ptr id)
                member x.WriteTo(ptr, id, value) = NativePtr.set ptr id value.Angle
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) = Rot2s(scalar.Variable(id, value.Angle))
            }
        
        static member GetScalarAdapter (_dummy : Rot3d) =
            { new ScalarAdapter<Rot3d, Rot3s> with
                member x.DoubleCount = 3
                member x.ReadScalar(ptr, id) =
                    let x = scalar.Variable(id, NativePtr.get ptr id)
                    let y = scalar.Variable(id+1, NativePtr.get ptr (id+1))
                    let z = scalar.Variable(id+2, NativePtr.get ptr (id+2))
                    Rot3s(V3s(x, y, z))
                    
                member x.ReadValue(ptr, id) =
                    let x = NativePtr.get ptr id
                    let y = NativePtr.get ptr (id+1)
                    let z = NativePtr.get ptr (id+2)
                    Rot3d.FromAngleAxis(V3d(x, y, z))
                    
                member x.WriteTo(ptr, id, value) =
                    let aa = value.ToAngleAxis()
                    NativePtr.set ptr id aa.X
                    NativePtr.set ptr (id+1) aa.Y
                    NativePtr.set ptr (id+2) aa.Z
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) =
                    let aa = value.ToAngleAxis()
                    Rot3s(V3s(scalar.Variable(id, aa.X), scalar.Variable(id+1, aa.Y), scalar.Variable(id+2, aa.Z)))
            }
           
        static member GetScalarAdapter (_dummy : Euclidean2d) =
            { new ScalarAdapter<Euclidean2d, Euclidean2s> with
                member x.DoubleCount = 3
                member x.ReadScalar(ptr, id) =
                    let r = Rot2s(scalar.Variable(id, NativePtr.get ptr id))
                    let t = V2s(scalar.Variable(id+1, NativePtr.get ptr (id+1)), scalar.Variable(id+2, NativePtr.get ptr (id+2)))
                    Euclidean2s(r, t)
                    
                member x.ReadValue(ptr, id) =
                    let r = Rot2d(NativePtr.get ptr id)
                    let t = V2d(NativePtr.get ptr (id+1), NativePtr.get ptr (id+2))
                    Euclidean2d(r, t)
                    
                member x.WriteTo(ptr, id, value) =
                    NativePtr.set ptr id value.Rot.Angle
                    NativePtr.set ptr (id+1) value.Trans.X
                    NativePtr.set ptr (id+2) value.Trans.Y
                    
                member x.GetValue(s) = s.Value
                member x.Variable(value, id) =
                    Euclidean2s(
                        Rot2s(scalar.Variable(id, value.Rot.Angle)),
                        V2s(scalar.Variable(id+1, value.Trans.X), scalar.Variable(id+2, value.Trans.Y))
                    )
            }
         
        static member GetScalarAdapter (_dummy : Euclidean3d) =
            { new ScalarAdapter<Euclidean3d, Euclidean3s> with
                member x.DoubleCount = 6
                member x.ReadScalar(ptr, id) =
                    let rx = scalar.Variable(id, NativePtr.get ptr id)
                    let ry = scalar.Variable(id+1, NativePtr.get ptr (id+1))
                    let rz = scalar.Variable(id+2, NativePtr.get ptr (id+2))
                    let r = Rot3s(V3s(rx, ry, rz))
                    let t =
                        V3s(
                            scalar.Variable(id+3, NativePtr.get ptr (id+3)),
                            scalar.Variable(id+4, NativePtr.get ptr (id+4)),
                            scalar.Variable(id+5, NativePtr.get ptr (id+5))
                        )
                    Euclidean3s(r, t)
                    
                member x.ReadValue(ptr, id) =
                    let rx = NativePtr.get ptr id
                    let ry = NativePtr.get ptr (id+1)
                    let rz = NativePtr.get ptr (id+2)
                    
                    let r = Rot3d.FromAngleAxis(V3d(rx, ry, rz))
                    let t =
                        V3d(
                            NativePtr.get ptr (id+3),
                            NativePtr.get ptr (id+4),
                            NativePtr.get ptr (id+5)
                        )
                    Euclidean3d(r, t)
                    
                member x.WriteTo(ptr, id, value) =
                    let aa = value.Rot.ToAngleAxis()
                    NativePtr.set ptr id aa.X
                    NativePtr.set ptr (id+1) aa.Y
                    NativePtr.set ptr (id+2) aa.Z
                    NativePtr.set ptr (id+3) value.Trans.X
                    NativePtr.set ptr (id+4) value.Trans.Y
                    NativePtr.set ptr (id+5) value.Trans.Z
                    
                member x.GetValue(s) = s.Value
                
                member x.Variable(value, id) =
                    let aa = value.Rot.ToAngleAxis()
                    Euclidean3s(
                        Rot3s(
                            V3s(
                                scalar.Variable(id, aa.X),
                                scalar.Variable(id+1, aa.Y),
                                scalar.Variable(id+2, aa.Z)
                            )
                        ),
                        V3s(
                            scalar.Variable(id+3, value.Trans.X),
                            scalar.Variable(id+4, value.Trans.Y),
                            scalar.Variable(id+5, value.Trans.Z)
                        )
                    )
            }
         
        static member GetScalarAdapter (_dummy : Similarity2d) =
            { new ScalarAdapter<Similarity2d, Similarity2s> with
                member x.DoubleCount = 4
                member x.ReadScalar(ptr, id) =
                    let r = Rot2s(scalar.Variable(id, NativePtr.get ptr id))
                    let t = V2s(scalar.Variable(id+1, NativePtr.get ptr (id+1)), scalar.Variable(id+2, NativePtr.get ptr (id+2)))
                    let sqrtScale = scalar.Variable(id+3, NativePtr.get ptr (id+3))
                    Similarity2s(Euclidean2s(r, t), sqrtScale)
                    
                member x.ReadValue(ptr, id) =
                    let r = Rot2d(NativePtr.get ptr id)
                    let t = V2d(NativePtr.get ptr (id+1), NativePtr.get ptr (id+2))
                    let sqrtScale = NativePtr.get ptr (id+3)
                    Similarity2d(sqr sqrtScale, Euclidean2d(r, t))
                    
                member x.WriteTo(ptr, id, value) =
                    NativePtr.set ptr id value.Rot.Angle
                    NativePtr.set ptr (id+1) value.Trans.X
                    NativePtr.set ptr (id+2) value.Trans.Y
                    NativePtr.set ptr (id+3) (sqrt value.Scale)
                    
                member x.GetValue(s) =
                    Similarity2d(s.Scale.Value, s.EuclideanTransformation.Value)
                    
                member x.Variable(value, id) =
                    Similarity2s(
                        Euclidean2s(
                            Rot2s(scalar.Variable(id, value.Rot.Angle)),
                            V2s(scalar.Variable(id+1, value.Trans.X), scalar.Variable(id+2, value.Trans.Y))
                        ),
                        scalar.Variable(id+3, sqrt value.Scale)
                    )
            }
         
        static member GetScalarAdapter (_dummy : Similarity3d) =
            { new ScalarAdapter<Similarity3d, Similarity3s> with
                member x.DoubleCount = 7
                member x.ReadScalar(ptr, id) =
                    let rx = scalar.Variable(id, NativePtr.get ptr id)
                    let ry = scalar.Variable(id+1, NativePtr.get ptr (id+1))
                    let rz = scalar.Variable(id+2, NativePtr.get ptr (id+2))
                    let r = Rot3s(V3s(rx, ry, rz))
                    let t =
                        V3s(
                            scalar.Variable(id+3, NativePtr.get ptr (id+3)),
                            scalar.Variable(id+4, NativePtr.get ptr (id+4)),
                            scalar.Variable(id+5, NativePtr.get ptr (id+5))
                        )
                    let sqrtScale = scalar.Variable(id+6, NativePtr.get ptr (id+6))
                    Similarity3s(Euclidean3s(r, t), sqrtScale)
                    
                member x.ReadValue(ptr, id) =
                    let rx = NativePtr.get ptr id
                    let ry = NativePtr.get ptr (id+1)
                    let rz = NativePtr.get ptr (id+2)
                    
                    let r = Rot3d.FromAngleAxis(V3d(rx, ry, rz))
                    let t =
                        V3d(
                            NativePtr.get ptr (id+3),
                            NativePtr.get ptr (id+4),
                            NativePtr.get ptr (id+5)
                        )
                    let sqrtScale = NativePtr.get ptr (id+6)
                    Similarity3d(sqr sqrtScale, Euclidean3d(r, t))
                    
                member x.WriteTo(ptr, id, value) =
                    let aa = value.Rot.ToAngleAxis()
                    NativePtr.set ptr id aa.X
                    NativePtr.set ptr (id+1) aa.Y
                    NativePtr.set ptr (id+2) aa.Z
                    NativePtr.set ptr (id+3) value.Trans.X
                    NativePtr.set ptr (id+4) value.Trans.Y
                    NativePtr.set ptr (id+5) value.Trans.Z
                    NativePtr.set ptr (id+6) (sqrt value.Scale)
                    
                member x.GetValue(s) =
                    Similarity3d(s.Scale.Value, s.EuclideanTransformation.Value)
                
                member x.Variable(value, id) =
                    let aa = value.Rot.ToAngleAxis()
                    Similarity3s(
                        Euclidean3s(
                            Rot3s(
                                V3s(
                                    scalar.Variable(id, aa.X),
                                    scalar.Variable(id+1, aa.Y),
                                    scalar.Variable(id+2, aa.Z)
                                )
                            ),
                            V3s(
                                scalar.Variable(id+3, value.Trans.X),
                                scalar.Variable(id+4, value.Trans.Y),
                                scalar.Variable(id+5, value.Trans.Z)
                            )
                        ),
                        scalar.Variable(id+6, sqrt value.Scale)
                    )
            }
         
        
         
        // static member inline GetScalarAdapter (value : ^a[])  =
        //     let inner = scalarAdapterAux Unchecked.defaultof<ScalarAdapterInstances> value.[0]
        //     { new ScalarAdapter<^a[], _> with
        //         member x.DoubleCount = value.Length * inner.DoubleCount
        //         member x.ReadScalar(ptr, id) =
        //             let res = Array.zeroCreate value.Length
        //             let mutable id = id
        //             for i in 0 .. value.Length - 1 do
        //                 res.[i] <- inner.Variable(inner.ReadValue(ptr, id), id)
        //                 id <- id + inner.DoubleCount
        //             res
        //         member x.ReadValue(ptr, id) = failwith ""
        //         member x.WriteTo(ptr, id, value) = failwith ""
        //         member x.GetValue(s) =
        //             s |> Array.map inner.GetValue
        //         member x.Variable(value, id) = failwith ""
        //     }
           
    let inline scalarAdapterAux< ^a, ^b, ^c when (^a or ^b or ^c) : (static member GetScalarAdapter : ^a -> ScalarAdapter<^a, ^b>) > (_ : ^c) (dummy : ^a)  =
        ((^a or ^b or ^c) : (static member GetScalarAdapter : ^a -> ScalarAdapter<^a, ^b>) (dummy))
          
    let inline scalarAdapter dummy =
        scalarAdapterAux Unchecked.defaultof<ScalarAdapterInstances> dummy

        
    [<Struct>]
    type Constraint = Constraint of scalar * float * float
    
    type IpOptBuilderState =
        {
            Evaluation : bool
            UpdateScalars : list<nativeptr<float> -> unit>
            UpdateInput : list<nativeptr<float> -> unit>
            WriteTo : list<nativeptr<float> -> unit>
            VariableOffset : int
            Objective : scalar
            NegateObjective : bool
            Constraints : list<Constraint[]>
            ConstraintCount : int
            CachedArrays : System.Collections.Generic.Dictionary<obj, obj>
            Options : list<IpOptOption>
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
    
    [<Struct>]
    type Objective = Objective of min : bool * value : scalar
    
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
                
        member inline x.Yield (Objective(isMin, o)) : Builder =
            fun state -> { state with Objective = o; NegateObjective = not isMin }
                
                
        member inline x.Yield (o : IpOptOption) : Builder =
            fun state ->
                if state.Evaluation then state
                else { state with Options = o :: state.Options }
                
        member inline x.Combine([<InlineIfLambda>] a : Builder, [<InlineIfLambda>] b : Builder) : Builder =
            fun state -> b (a state)
           
        member inline x.Run([<InlineIfLambda>] f : Builder) =
            let initialState = { Evaluation = false; Options = []; NegateObjective = false; CachedArrays = System.Collections.Generic.Dictionary(); VariableOffset = 0; UpdateScalars = []; UpdateInput = []; WriteTo = []; Objective = scalar.Zero; Constraints = []; ConstraintCount = 0 }
            let state = f initialState
            
            
            let options = state.Options
            let mutable oCache = scalar.Zero
            let mutable cCache = Array.zeroCreate<Constraint> state.ConstraintCount
            
            //let evalState = { initialState with Evaluation = true }
            let updateCache (x : nativeint) =
                for update in state.UpdateScalars do
                    update (NativePtr.ofNativeInt x)
                   
                let state = f { initialState with Evaluation = true }
                let res = state.Objective
                let mutable oi = 0
                for cs in state.Constraints do
                    for c in cs do
                        cCache.[oi] <- c
                        oi <- oi + 1
                        
                if state.NegateObjective then
                    oCache <- -res
                else
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
                                    
                                    let tol = options |> List.tryPick (function IpOptOption.Tolerance t -> Some t | _ -> None) |> Option.defaultValue 1e-7
                                    let printLevel = options |> List.tryPick (function IpOptOption.PrintLevel l -> Some l | _ -> None) |> Option.defaultValue 0
                                    let maxIter = options |> List.tryPick (function IpOptOption.MaxIterations i -> Some i | _ -> None) |> Option.defaultValue 1000
                                    let hessianApproximation = options |> List.tryPick (function IpOptOption.HessianApproximation s -> Some s | _ -> None) |> Option.defaultValue "limited-memory"
                                    
                                    IpoptNative.AddIpoptIntOption(handle, "print_level", printLevel) |> ignore
                                    IpoptNative.AddIpoptNumOption(handle, "tol", tol) |> ignore
                                    IpoptNative.AddIpoptIntOption(handle, "max_iter", maxIter) |> ignore
                                    IpoptNative.AddIpoptStrOption(handle, "hessian_approximation", hessianApproximation) |> ignore
                                    
                                    for o in options do
                                        match o with
                                        | IpOptOption.Tolerance _ 
                                        | IpOptOption.PrintLevel _ 
                                        | IpOptOption.MaxIterations _ 
                                        | IpOptOption.HessianApproximation _ ->
                                            ()
                                            
                                        | IpOptOption.String(k, v) ->
                                            IpoptNative.AddIpoptStrOption(handle, k, v) |> ignore
                                        | IpOptOption.Float(k, v) ->
                                            IpoptNative.AddIpoptNumOption(handle, k, v) |> ignore
                                        | IpOptOption.Int(k, v) ->
                                            IpoptNative.AddIpoptIntOption(handle, k, v) |> ignore
                                        | IpOptOption.CpuTimeLimit v ->
                                            IpoptNative.AddIpoptNumOption(handle, "cpu_time_limit", v) |> ignore
                                        | IpOptOption.DerivativeTest test ->
                                            IpoptNative.AddIpoptStrOption(handle, "derivative_test", test) |> ignore
                                        | IpOptOption.LinearSolver solver ->
                                            IpoptNative.AddIpoptStrOption(handle, "linear_solver", solver) |> ignore
                                        | IpOptOption.MuStrategy strategy ->
                                            IpoptNative.AddIpoptStrOption(handle, "mu_strategy", strategy) |> ignore
                                        | IpOptOption.OutputFile file ->
                                            IpoptNative.AddIpoptStrOption(handle, "output_file", file) |> ignore
                                            
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
                        
                        
                        let obj = if state.NegateObjective then -objVal.[0] else objVal.[0]
                        (returnStatus, obj)
                    finally
                        gHandle.Free()
                        objValHandle.Free()
                        multGHandle.Free()
                        multXLHandle.Free()
                        multXUHandle.Free()
                else
                    (IpOptStatus.InternalError, System.Double.PositiveInfinity)
       
       
        member inline x.Bind (thing : ref<_>, action) =
            fun (state : IpOptBuilderState) ->
                let mutable arr = Unchecked.defaultof<_>
                let mutable runState = state
                match state.CachedArrays.TryGetValue thing with
                | (true, cached) ->
                    arr <- cached :?> _
                | _ ->
                    let ad = scalarAdapter thing.Value
                    
                    let sValue =
                        ref (ad.Variable(thing.Value, state.VariableOffset))
                        
                    let updateScalars(src : nativeptr<float>) =
                        sValue.Value <- ad.ReadScalar(src, state.VariableOffset)
                        
                    let updateInput(src : nativeptr<float>) =
                        thing.Value <- ad.ReadValue(src, state.VariableOffset)
                        
                    let writeTo (dst : nativeptr<float>) =
                        ad.WriteTo(dst, state.VariableOffset, thing.Value)
                        
                    state.CachedArrays.[thing] <- sValue
                    arr <- sValue
                    runState <- {
                        state with
                            UpdateScalars = updateScalars :: state.UpdateScalars
                            UpdateInput = updateInput :: state.UpdateInput
                            WriteTo = writeTo :: state.WriteTo
                            VariableOffset = state.VariableOffset + ad.DoubleCount
                    }
                action arr.Value runState
            
        member inline x.Bind (thing : array<_>, action) =
            fun (state : IpOptBuilderState) ->
                let mutable arr = Unchecked.defaultof<_>
                let mutable runState = state
                match state.CachedArrays.TryGetValue thing with
                | (true, cached) ->
                    arr <- cached :?> _
                | _ ->
                    let v = Array.tryHead thing |> Option.defaultValue Unchecked.defaultof<_>
                    let ad = scalarAdapter v
                    
                    let sValue =
                        let variables = Array.zeroCreate thing.Length
                        let mutable offset = state.VariableOffset
                        for i in 0 .. thing.Length - 1 do
                            variables.[i] <- ad.Variable(thing.[i], offset)
                            offset <- offset + ad.DoubleCount
                        variables
                        
                    let updateScalars(src : nativeptr<float>) =
                        let mutable offset = state.VariableOffset
                        for i in 0 .. thing.Length - 1 do
                            sValue.[i] <- ad.ReadScalar(src, offset)
                            offset <- offset + ad.DoubleCount
                        
                    let updateInput(src : nativeptr<float>) =
                        let mutable offset = state.VariableOffset
                        for i in 0 .. thing.Length - 1 do
                            thing.[i] <- ad.ReadValue(src, offset)
                            offset <- offset + ad.DoubleCount
                        
                    let writeTo (dst : nativeptr<float>) =
                        let mutable offset = state.VariableOffset
                        for i in 0 .. thing.Length - 1 do
                            ad.WriteTo(dst, state.VariableOffset, thing.[i])
                            offset <- offset + ad.DoubleCount
                        
                    state.CachedArrays.[thing] <- sValue
                    arr <- sValue
                    runState <- {
                        state with
                            UpdateScalars = updateScalars :: state.UpdateScalars
                            UpdateInput = updateInput :: state.UpdateInput
                            WriteTo = writeTo :: state.WriteTo
                            VariableOffset = state.VariableOffset + ad.DoubleCount * thing.Length
                    }
                action arr runState
       
       
open IpOptBuilderImplementation

[<AutoOpen>]
module IpOptBuilder =
    open IpOptBuilderImplementation
    
    let inline minimize a =
        Objective(true, a)
        
    let inline maximize a =
        Objective(false, a)
    
    let inline equal a b =
        constr (a - b) 0.0 0.0
        
    let inline greaterEqual a b =
        constr (a - b) 0.0 System.Double.PositiveInfinity
    
    let inline lessEqual a b =
        constr (b - a) 0.0 System.Double.PositiveInfinity
    
    let inline range a l h =
        constr ((a - l) / (h - l)) 0.0 1.0
    
    let ipopt = IpOptBuilder()
    
type IpOpt private() =
    static member MaxIterations(value : int) : IpOptOption =
        IpOptOption.MaxIterations value
    static member Tolerance(value : float) : IpOptOption =
        IpOptOption.Tolerance value
    static member PrintLevel(value : int) : IpOptOption =
        IpOptOption.PrintLevel value
    static member CpuTimeLimit(value : float) : IpOptOption =
        IpOptOption.CpuTimeLimit value
    static member LinearSolver(value : string) : IpOptOption =
        IpOptOption.LinearSolver value
    static member DerivativeTest(value : string) : IpOptOption =
        IpOptOption.DerivativeTest value
    static member HessianApproximation(value : string) : IpOptOption =
        IpOptOption.HessianApproximation value
    static member MuStrategy(value : string) : IpOptOption =
        IpOptOption.MuStrategy value
    static member OutputFile(value : string) : IpOptOption =
        IpOptOption.OutputFile value
    static member Int(key : string, value : int) : IpOptOption =
        IpOptOption.Int(key, value)
    static member Float(key : string, value : float) : IpOptOption =
        IpOptOption.Float(key, value)
    static member String(key : string, value : string) : IpOptOption =
        IpOptOption.String(key, value)
        
    
    static member inline Equal(a, b) = equal a b
    static member inline GreaterEqual(a, b) = greaterEqual a b
    static member inline LessEqual(a, b) = lessEqual a b
    static member inline Range(a, l, h) = range a l h
    
    
    static member inline Perpendicular(v1 : V2s, v2 : V2s) =
        [|
            Constraint(v1.X * v2.X + v1.Y * v2.Y, 0.0, 0.0)
        |]
        
    static member inline Perpendicular(v1 : V3s, v2 : V3s) =
        [|
            Constraint(v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z, 0.0, 0.0)
        |]
        
    static member inline Perpendicular(v1 : V4s, v2 : V4s) =
        [|
            Constraint(v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W, 0.0, 0.0)
        |]
    
    static member inline Parallel(v1 : V2s, v2 : V2s) =
        [|
            Constraint(v1.X * v2.Y - v1.Y * v2.X, 0.0, 0.0)
        |]
        
    static member inline Parallel(v1 : V3s, v2 : V3s) =
        [|
            Constraint(v1.Y * v2.Z - v1.Z * v2.Y, 0.0, 0.0)
            Constraint(v1.Z * v2.X - v1.X * v2.Z, 0.0, 0.0)
            Constraint(v1.X * v2.Y - v1.Y * v2.X, 0.0, 0.0)
        |]
        
    static member inline Parallel(v1 : V4s, v2 : V4s) =
        [|
            Constraint(v1.Y * v2.Z - v1.Z * v2.Y, 0.0, 0.0)
            Constraint(v1.Z * v2.W - v1.W * v2.Z, 0.0, 0.0)
            Constraint(v1.W * v2.X - v1.X * v2.W, 0.0, 0.0)
            Constraint(v1.X * v2.Y - v1.Y * v2.X, 0.0, 0.0)
        |]
    
    static member inline Orthogonal(m : M22s) =
        [|
            Constraint(m.M00 * m.M10 + m.M01 * m.M11, 0.0, 0.0)
        |]
        
    static member inline Orthogonal(m : M33s) =
        [|
            Constraint(m.M00 * m.M10 + m.M01 * m.M11 + m.M02 * m.M12, 0.0, 0.0)
            Constraint(m.M00 * m.M20 + m.M01 * m.M21 + m.M02 * m.M22, 0.0, 0.0)
            Constraint(m.M10 * m.M20 + m.M11 * m.M21 + m.M12 * m.M22, 0.0, 0.0)
        |]
    
    static member inline Orthonormal(m : M22s) =
        [|
            Constraint(sqr m.M00 + sqr m.M01, 1.0, 1.0)
            Constraint(sqr m.M10 + sqr m.M11, 1.0, 1.0)
            Constraint(m.M00 * m.M10 + m.M01 * m.M11, 0.0, 0.0)
        |]
    static member inline Orthonormal(m : M33s) =
        [|
            Constraint(sqr m.M00 + sqr m.M01 + sqr m.M02, 1.0, 1.0)
            Constraint(sqr m.M10 + sqr m.M11 + sqr m.M12, 1.0, 1.0)
            Constraint(sqr m.M20 + sqr m.M21 + sqr m.M22, 1.0, 1.0)
            Constraint(m.M00 * m.M10 + m.M01 * m.M11 + m.M02 * m.M12, 0.0, 0.0)
            Constraint(m.M00 * m.M20 + m.M01 * m.M21 + m.M02 * m.M22, 0.0, 0.0)
            Constraint(m.M10 * m.M20 + m.M11 * m.M21 + m.M12 * m.M22, 0.0, 0.0)
        |]
    
    
    static member inline Minimize(a) = minimize a
    static member inline Maximize(a) = maximize a

