namespace IpOptSharp
open Aardvark.Base
open Microsoft.FSharp.NativeInterop
open System.Runtime.CompilerServices
open IpOptSharp.IpOptBuilderImplementation
#nowarn "9"

[<AbstractClass; Sealed>]
type IpOptBuilderExtensions private() = 
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : ref<float>, [<InlineIfLambda>] cont : scalar -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? ref<scalar> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    ref (scalar.Variable(state.VariableOffset, value.Value))
                    
                let updateScalars(src : nativeptr<float>) =
                    sValue.contents.Value <- NativePtr.get src (state.VariableOffset + 0)
                    
                let updateInput(src : nativeptr<float>) =
                    value.Value <- NativePtr.get src state.VariableOffset
                    
                let writeTo (dst : nativeptr<float>) =
                    NativePtr.set dst (state.VariableOffset + 0) value.Value
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 1
                }
            cont arr.Value runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : ref<V2d>, [<InlineIfLambda>] cont : V2s -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? ref<V2s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    ref (
                        V2s(
                            scalar.Variable(state.VariableOffset + 0, value.Value.X),
                            scalar.Variable(state.VariableOffset + 1, value.Value.Y)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    sValue.contents.X.Value <- NativePtr.get src (state.VariableOffset + 0)
                    sValue.contents.Y.Value <- NativePtr.get src (state.VariableOffset + 1)
                    
                let updateInput(src : nativeptr<float>) =
                    value.Value <- V2d(NativePtr.get src (state.VariableOffset + 0), NativePtr.get src (state.VariableOffset + 1))
                    
                let writeTo (dst : nativeptr<float>) =
                    NativePtr.set dst (state.VariableOffset + 0) value.Value.X
                    NativePtr.set dst (state.VariableOffset + 1) value.Value.Y
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 2
                }
            cont arr.Value runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : ref<V3d>, [<InlineIfLambda>] cont : V3s -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? ref<V3s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    ref (
                        V3s(
                            scalar.Variable(state.VariableOffset + 0, value.Value.X),
                            scalar.Variable(state.VariableOffset + 1, value.Value.Y),
                            scalar.Variable(state.VariableOffset + 2, value.Value.Z)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    sValue.contents.X.Value <- NativePtr.get src (state.VariableOffset + 0)
                    sValue.contents.Y.Value <- NativePtr.get src (state.VariableOffset + 1)
                    sValue.contents.Z.Value <- NativePtr.get src (state.VariableOffset + 2)
                    
                let updateInput(src : nativeptr<float>) =
                    value.Value <- V3d(NativePtr.get src (state.VariableOffset + 0), NativePtr.get src (state.VariableOffset + 1), NativePtr.get src (state.VariableOffset + 2))
                    
                let writeTo (dst : nativeptr<float>) =
                    NativePtr.set dst (state.VariableOffset + 0) value.Value.X
                    NativePtr.set dst (state.VariableOffset + 1) value.Value.Y
                    NativePtr.set dst (state.VariableOffset + 2) value.Value.Z
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 3
                }
            cont arr.Value runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : ref<V4d>, [<InlineIfLambda>] cont : V4s -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? ref<V4s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    ref (
                        V4s(
                            scalar.Variable(state.VariableOffset + 0, value.Value.X),
                            scalar.Variable(state.VariableOffset + 1, value.Value.Y),
                            scalar.Variable(state.VariableOffset + 2, value.Value.Z),
                            scalar.Variable(state.VariableOffset + 3, value.Value.W)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    sValue.contents.X.Value <- NativePtr.get src (state.VariableOffset + 0)
                    sValue.contents.Y.Value <- NativePtr.get src (state.VariableOffset + 1)
                    sValue.contents.Z.Value <- NativePtr.get src (state.VariableOffset + 2)
                    sValue.contents.W.Value <- NativePtr.get src (state.VariableOffset + 3)
                    
                let updateInput(src : nativeptr<float>) =
                    value.Value <- V4d(NativePtr.get src (state.VariableOffset + 0), NativePtr.get src (state.VariableOffset + 1), NativePtr.get src (state.VariableOffset + 2), NativePtr.get src (state.VariableOffset + 3))
                    
                let writeTo (dst : nativeptr<float>) =
                    NativePtr.set dst (state.VariableOffset + 0) value.Value.X
                    NativePtr.set dst (state.VariableOffset + 1) value.Value.Y
                    NativePtr.set dst (state.VariableOffset + 2) value.Value.Z
                    NativePtr.set dst (state.VariableOffset + 3) value.Value.W
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 4
                }
            cont arr.Value runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : ref<M22d>, [<InlineIfLambda>] cont : M22s -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? ref<M22s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    ref (
                        M22s(
                            scalar.Variable(state.VariableOffset + 0, value.Value.M00),
                            scalar.Variable(state.VariableOffset + 1, value.Value.M01),
                            scalar.Variable(state.VariableOffset + 2, value.Value.M10),
                            scalar.Variable(state.VariableOffset + 3, value.Value.M11)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    sValue.contents.M00.Value <- NativePtr.get src (state.VariableOffset + 0)
                    sValue.contents.M01.Value <- NativePtr.get src (state.VariableOffset + 1)
                    sValue.contents.M10.Value <- NativePtr.get src (state.VariableOffset + 2)
                    sValue.contents.M11.Value <- NativePtr.get src (state.VariableOffset + 3)
                    
                let updateInput(src : nativeptr<float>) =
                    value.Value <- M22d(NativePtr.get src (state.VariableOffset + 0), NativePtr.get src (state.VariableOffset + 1), NativePtr.get src (state.VariableOffset + 2), NativePtr.get src (state.VariableOffset + 3))
                    
                let writeTo (dst : nativeptr<float>) =
                    NativePtr.set dst (state.VariableOffset + 0) value.Value.M00
                    NativePtr.set dst (state.VariableOffset + 1) value.Value.M01
                    NativePtr.set dst (state.VariableOffset + 2) value.Value.M10
                    NativePtr.set dst (state.VariableOffset + 3) value.Value.M11
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 4
                }
            cont arr.Value runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : ref<M33d>, [<InlineIfLambda>] cont : M33s -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? ref<M33s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    ref (
                        M33s(
                            scalar.Variable(state.VariableOffset + 0, value.Value.M00),
                            scalar.Variable(state.VariableOffset + 1, value.Value.M01),
                            scalar.Variable(state.VariableOffset + 2, value.Value.M02),
                            scalar.Variable(state.VariableOffset + 3, value.Value.M10),
                            scalar.Variable(state.VariableOffset + 4, value.Value.M11),
                            scalar.Variable(state.VariableOffset + 5, value.Value.M12),
                            scalar.Variable(state.VariableOffset + 6, value.Value.M20),
                            scalar.Variable(state.VariableOffset + 7, value.Value.M21),
                            scalar.Variable(state.VariableOffset + 8, value.Value.M22)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    sValue.contents.M00.Value <- NativePtr.get src (state.VariableOffset + 0)
                    sValue.contents.M01.Value <- NativePtr.get src (state.VariableOffset + 1)
                    sValue.contents.M02.Value <- NativePtr.get src (state.VariableOffset + 2)
                    sValue.contents.M10.Value <- NativePtr.get src (state.VariableOffset + 3)
                    sValue.contents.M11.Value <- NativePtr.get src (state.VariableOffset + 4)
                    sValue.contents.M12.Value <- NativePtr.get src (state.VariableOffset + 5)
                    sValue.contents.M20.Value <- NativePtr.get src (state.VariableOffset + 6)
                    sValue.contents.M21.Value <- NativePtr.get src (state.VariableOffset + 7)
                    sValue.contents.M22.Value <- NativePtr.get src (state.VariableOffset + 8)
                    
                let updateInput(src : nativeptr<float>) =
                    value.Value <- M33d(NativePtr.get src (state.VariableOffset + 0), NativePtr.get src (state.VariableOffset + 1), NativePtr.get src (state.VariableOffset + 2), NativePtr.get src (state.VariableOffset + 3), NativePtr.get src (state.VariableOffset + 4), NativePtr.get src (state.VariableOffset + 5), NativePtr.get src (state.VariableOffset + 6), NativePtr.get src (state.VariableOffset + 7), NativePtr.get src (state.VariableOffset + 8))
                    
                let writeTo (dst : nativeptr<float>) =
                    NativePtr.set dst (state.VariableOffset + 0) value.Value.M00
                    NativePtr.set dst (state.VariableOffset + 1) value.Value.M01
                    NativePtr.set dst (state.VariableOffset + 2) value.Value.M02
                    NativePtr.set dst (state.VariableOffset + 3) value.Value.M10
                    NativePtr.set dst (state.VariableOffset + 4) value.Value.M11
                    NativePtr.set dst (state.VariableOffset + 5) value.Value.M12
                    NativePtr.set dst (state.VariableOffset + 6) value.Value.M20
                    NativePtr.set dst (state.VariableOffset + 7) value.Value.M21
                    NativePtr.set dst (state.VariableOffset + 8) value.Value.M22
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 9
                }
            cont arr.Value runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : ref<M44d>, [<InlineIfLambda>] cont : M44s -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? ref<M44s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    ref (
                        M44s(
                            scalar.Variable(state.VariableOffset + 0, value.Value.M00),
                            scalar.Variable(state.VariableOffset + 1, value.Value.M01),
                            scalar.Variable(state.VariableOffset + 2, value.Value.M02),
                            scalar.Variable(state.VariableOffset + 3, value.Value.M03),
                            scalar.Variable(state.VariableOffset + 4, value.Value.M10),
                            scalar.Variable(state.VariableOffset + 5, value.Value.M11),
                            scalar.Variable(state.VariableOffset + 6, value.Value.M12),
                            scalar.Variable(state.VariableOffset + 7, value.Value.M13),
                            scalar.Variable(state.VariableOffset + 8, value.Value.M20),
                            scalar.Variable(state.VariableOffset + 9, value.Value.M21),
                            scalar.Variable(state.VariableOffset + 10, value.Value.M22),
                            scalar.Variable(state.VariableOffset + 11, value.Value.M23),
                            scalar.Variable(state.VariableOffset + 12, value.Value.M30),
                            scalar.Variable(state.VariableOffset + 13, value.Value.M31),
                            scalar.Variable(state.VariableOffset + 14, value.Value.M32),
                            scalar.Variable(state.VariableOffset + 15, value.Value.M33)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    sValue.contents.M00.Value <- NativePtr.get src (state.VariableOffset + 0)
                    sValue.contents.M01.Value <- NativePtr.get src (state.VariableOffset + 1)
                    sValue.contents.M02.Value <- NativePtr.get src (state.VariableOffset + 2)
                    sValue.contents.M03.Value <- NativePtr.get src (state.VariableOffset + 3)
                    sValue.contents.M10.Value <- NativePtr.get src (state.VariableOffset + 4)
                    sValue.contents.M11.Value <- NativePtr.get src (state.VariableOffset + 5)
                    sValue.contents.M12.Value <- NativePtr.get src (state.VariableOffset + 6)
                    sValue.contents.M13.Value <- NativePtr.get src (state.VariableOffset + 7)
                    sValue.contents.M20.Value <- NativePtr.get src (state.VariableOffset + 8)
                    sValue.contents.M21.Value <- NativePtr.get src (state.VariableOffset + 9)
                    sValue.contents.M22.Value <- NativePtr.get src (state.VariableOffset + 10)
                    sValue.contents.M23.Value <- NativePtr.get src (state.VariableOffset + 11)
                    sValue.contents.M30.Value <- NativePtr.get src (state.VariableOffset + 12)
                    sValue.contents.M31.Value <- NativePtr.get src (state.VariableOffset + 13)
                    sValue.contents.M32.Value <- NativePtr.get src (state.VariableOffset + 14)
                    sValue.contents.M33.Value <- NativePtr.get src (state.VariableOffset + 15)
                    
                let updateInput(src : nativeptr<float>) =
                    value.Value <- M44d(NativePtr.get src (state.VariableOffset + 0), NativePtr.get src (state.VariableOffset + 1), NativePtr.get src (state.VariableOffset + 2), NativePtr.get src (state.VariableOffset + 3), NativePtr.get src (state.VariableOffset + 4), NativePtr.get src (state.VariableOffset + 5), NativePtr.get src (state.VariableOffset + 6), NativePtr.get src (state.VariableOffset + 7), NativePtr.get src (state.VariableOffset + 8), NativePtr.get src (state.VariableOffset + 9), NativePtr.get src (state.VariableOffset + 10), NativePtr.get src (state.VariableOffset + 11), NativePtr.get src (state.VariableOffset + 12), NativePtr.get src (state.VariableOffset + 13), NativePtr.get src (state.VariableOffset + 14), NativePtr.get src (state.VariableOffset + 15))
                    
                let writeTo (dst : nativeptr<float>) =
                    NativePtr.set dst (state.VariableOffset + 0) value.Value.M00
                    NativePtr.set dst (state.VariableOffset + 1) value.Value.M01
                    NativePtr.set dst (state.VariableOffset + 2) value.Value.M02
                    NativePtr.set dst (state.VariableOffset + 3) value.Value.M03
                    NativePtr.set dst (state.VariableOffset + 4) value.Value.M10
                    NativePtr.set dst (state.VariableOffset + 5) value.Value.M11
                    NativePtr.set dst (state.VariableOffset + 6) value.Value.M12
                    NativePtr.set dst (state.VariableOffset + 7) value.Value.M13
                    NativePtr.set dst (state.VariableOffset + 8) value.Value.M20
                    NativePtr.set dst (state.VariableOffset + 9) value.Value.M21
                    NativePtr.set dst (state.VariableOffset + 10) value.Value.M22
                    NativePtr.set dst (state.VariableOffset + 11) value.Value.M23
                    NativePtr.set dst (state.VariableOffset + 12) value.Value.M30
                    NativePtr.set dst (state.VariableOffset + 13) value.Value.M31
                    NativePtr.set dst (state.VariableOffset + 14) value.Value.M32
                    NativePtr.set dst (state.VariableOffset + 15) value.Value.M33
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 16
                }
            cont arr.Value runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : float[], [<InlineIfLambda>] cont : scalar[] -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? array<scalar> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    value |> Array.mapi (fun i v ->
                        let bi = 1*i
                        scalar.Variable(state.VariableOffset + bi, v)
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        sValue.[i].Value <- x0
                    
                let updateInput(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        value.[i] <- x0
                    
                let writeTo (dst : nativeptr<float>) =
                    let mutable di = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        NativePtr.set dst di value.[i]
                        di <- di + 1
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 1*value.Length
                }
            cont arr runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : V2d[], [<InlineIfLambda>] cont : V2s[] -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? array<V2s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    value |> Array.mapi (fun i v ->
                        let bi = 2*i
                        V2s(
                            scalar.Variable(state.VariableOffset + bi + 0, v.X),
                            scalar.Variable(state.VariableOffset + bi + 1, v.Y)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        sValue.[i].X.Value <- x0
                        sValue.[i].Y.Value <- x1
                    
                let updateInput(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        value.[i] <- V2d(x0, x1)
                    
                let writeTo (dst : nativeptr<float>) =
                    let mutable di = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        NativePtr.set dst di value.[i].X
                        di <- di + 1
                        NativePtr.set dst di value.[i].Y
                        di <- di + 1
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 2*value.Length
                }
            cont arr runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : V3d[], [<InlineIfLambda>] cont : V3s[] -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? array<V3s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    value |> Array.mapi (fun i v ->
                        let bi = 3*i
                        V3s(
                            scalar.Variable(state.VariableOffset + bi + 0, v.X),
                            scalar.Variable(state.VariableOffset + bi + 1, v.Y),
                            scalar.Variable(state.VariableOffset + bi + 2, v.Z)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        sValue.[i].X.Value <- x0
                        sValue.[i].Y.Value <- x1
                        sValue.[i].Z.Value <- x2
                    
                let updateInput(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        value.[i] <- V3d(x0, x1, x2)
                    
                let writeTo (dst : nativeptr<float>) =
                    let mutable di = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        NativePtr.set dst di value.[i].X
                        di <- di + 1
                        NativePtr.set dst di value.[i].Y
                        di <- di + 1
                        NativePtr.set dst di value.[i].Z
                        di <- di + 1
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 3*value.Length
                }
            cont arr runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : V4d[], [<InlineIfLambda>] cont : V4s[] -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? array<V4s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    value |> Array.mapi (fun i v ->
                        let bi = 4*i
                        V4s(
                            scalar.Variable(state.VariableOffset + bi + 0, v.X),
                            scalar.Variable(state.VariableOffset + bi + 1, v.Y),
                            scalar.Variable(state.VariableOffset + bi + 2, v.Z),
                            scalar.Variable(state.VariableOffset + bi + 3, v.W)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        let x3 = NativePtr.get src si
                        si <- si + 1
                        sValue.[i].X.Value <- x0
                        sValue.[i].Y.Value <- x1
                        sValue.[i].Z.Value <- x2
                        sValue.[i].W.Value <- x3
                    
                let updateInput(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        let x3 = NativePtr.get src si
                        si <- si + 1
                        value.[i] <- V4d(x0, x1, x2, x3)
                    
                let writeTo (dst : nativeptr<float>) =
                    let mutable di = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        NativePtr.set dst di value.[i].X
                        di <- di + 1
                        NativePtr.set dst di value.[i].Y
                        di <- di + 1
                        NativePtr.set dst di value.[i].Z
                        di <- di + 1
                        NativePtr.set dst di value.[i].W
                        di <- di + 1
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 4*value.Length
                }
            cont arr runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : M22d[], [<InlineIfLambda>] cont : M22s[] -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? array<M22s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    value |> Array.mapi (fun i v ->
                        let bi = 4*i
                        M22s(
                            scalar.Variable(state.VariableOffset + bi + 0, v.M00),
                            scalar.Variable(state.VariableOffset + bi + 1, v.M01),
                            scalar.Variable(state.VariableOffset + bi + 2, v.M10),
                            scalar.Variable(state.VariableOffset + bi + 3, v.M11)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        let x3 = NativePtr.get src si
                        si <- si + 1
                        sValue.[i].M00.Value <- x0
                        sValue.[i].M01.Value <- x1
                        sValue.[i].M10.Value <- x2
                        sValue.[i].M11.Value <- x3
                    
                let updateInput(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        let x3 = NativePtr.get src si
                        si <- si + 1
                        value.[i] <- M22d(x0, x1, x2, x3)
                    
                let writeTo (dst : nativeptr<float>) =
                    let mutable di = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        NativePtr.set dst di value.[i].M00
                        di <- di + 1
                        NativePtr.set dst di value.[i].M01
                        di <- di + 1
                        NativePtr.set dst di value.[i].M10
                        di <- di + 1
                        NativePtr.set dst di value.[i].M11
                        di <- di + 1
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 4*value.Length
                }
            cont arr runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : M33d[], [<InlineIfLambda>] cont : M33s[] -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? array<M33s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    value |> Array.mapi (fun i v ->
                        let bi = 9*i
                        M33s(
                            scalar.Variable(state.VariableOffset + bi + 0, v.M00),
                            scalar.Variable(state.VariableOffset + bi + 1, v.M01),
                            scalar.Variable(state.VariableOffset + bi + 2, v.M02),
                            scalar.Variable(state.VariableOffset + bi + 3, v.M10),
                            scalar.Variable(state.VariableOffset + bi + 4, v.M11),
                            scalar.Variable(state.VariableOffset + bi + 5, v.M12),
                            scalar.Variable(state.VariableOffset + bi + 6, v.M20),
                            scalar.Variable(state.VariableOffset + bi + 7, v.M21),
                            scalar.Variable(state.VariableOffset + bi + 8, v.M22)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        let x3 = NativePtr.get src si
                        si <- si + 1
                        let x4 = NativePtr.get src si
                        si <- si + 1
                        let x5 = NativePtr.get src si
                        si <- si + 1
                        let x6 = NativePtr.get src si
                        si <- si + 1
                        let x7 = NativePtr.get src si
                        si <- si + 1
                        let x8 = NativePtr.get src si
                        si <- si + 1
                        sValue.[i].M00.Value <- x0
                        sValue.[i].M01.Value <- x1
                        sValue.[i].M02.Value <- x2
                        sValue.[i].M10.Value <- x3
                        sValue.[i].M11.Value <- x4
                        sValue.[i].M12.Value <- x5
                        sValue.[i].M20.Value <- x6
                        sValue.[i].M21.Value <- x7
                        sValue.[i].M22.Value <- x8
                    
                let updateInput(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        let x3 = NativePtr.get src si
                        si <- si + 1
                        let x4 = NativePtr.get src si
                        si <- si + 1
                        let x5 = NativePtr.get src si
                        si <- si + 1
                        let x6 = NativePtr.get src si
                        si <- si + 1
                        let x7 = NativePtr.get src si
                        si <- si + 1
                        let x8 = NativePtr.get src si
                        si <- si + 1
                        value.[i] <- M33d(x0, x1, x2, x3, x4, x5, x6, x7, x8)
                    
                let writeTo (dst : nativeptr<float>) =
                    let mutable di = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        NativePtr.set dst di value.[i].M00
                        di <- di + 1
                        NativePtr.set dst di value.[i].M01
                        di <- di + 1
                        NativePtr.set dst di value.[i].M02
                        di <- di + 1
                        NativePtr.set dst di value.[i].M10
                        di <- di + 1
                        NativePtr.set dst di value.[i].M11
                        di <- di + 1
                        NativePtr.set dst di value.[i].M12
                        di <- di + 1
                        NativePtr.set dst di value.[i].M20
                        di <- di + 1
                        NativePtr.set dst di value.[i].M21
                        di <- di + 1
                        NativePtr.set dst di value.[i].M22
                        di <- di + 1
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 9*value.Length
                }
            cont arr runState
    [<Extension>]
    static member inline Bind(x : IpOptBuilder, value : M44d[], [<InlineIfLambda>] cont : M44s[] -> Builder) : Builder =
        fun state ->
            let mutable arr = Unchecked.defaultof<_>
            let mutable runState = state
            match state.CachedArrays.TryGetValue value with
            | (true, (:? array<M44s> as cached)) ->
                arr <- cached
            | _ ->
                let sValue =
                    value |> Array.mapi (fun i v ->
                        let bi = 16*i
                        M44s(
                            scalar.Variable(state.VariableOffset + bi + 0, v.M00),
                            scalar.Variable(state.VariableOffset + bi + 1, v.M01),
                            scalar.Variable(state.VariableOffset + bi + 2, v.M02),
                            scalar.Variable(state.VariableOffset + bi + 3, v.M03),
                            scalar.Variable(state.VariableOffset + bi + 4, v.M10),
                            scalar.Variable(state.VariableOffset + bi + 5, v.M11),
                            scalar.Variable(state.VariableOffset + bi + 6, v.M12),
                            scalar.Variable(state.VariableOffset + bi + 7, v.M13),
                            scalar.Variable(state.VariableOffset + bi + 8, v.M20),
                            scalar.Variable(state.VariableOffset + bi + 9, v.M21),
                            scalar.Variable(state.VariableOffset + bi + 10, v.M22),
                            scalar.Variable(state.VariableOffset + bi + 11, v.M23),
                            scalar.Variable(state.VariableOffset + bi + 12, v.M30),
                            scalar.Variable(state.VariableOffset + bi + 13, v.M31),
                            scalar.Variable(state.VariableOffset + bi + 14, v.M32),
                            scalar.Variable(state.VariableOffset + bi + 15, v.M33)
                        )
                    )
                    
                let updateScalars(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        let x3 = NativePtr.get src si
                        si <- si + 1
                        let x4 = NativePtr.get src si
                        si <- si + 1
                        let x5 = NativePtr.get src si
                        si <- si + 1
                        let x6 = NativePtr.get src si
                        si <- si + 1
                        let x7 = NativePtr.get src si
                        si <- si + 1
                        let x8 = NativePtr.get src si
                        si <- si + 1
                        let x9 = NativePtr.get src si
                        si <- si + 1
                        let x10 = NativePtr.get src si
                        si <- si + 1
                        let x11 = NativePtr.get src si
                        si <- si + 1
                        let x12 = NativePtr.get src si
                        si <- si + 1
                        let x13 = NativePtr.get src si
                        si <- si + 1
                        let x14 = NativePtr.get src si
                        si <- si + 1
                        let x15 = NativePtr.get src si
                        si <- si + 1
                        sValue.[i].M00.Value <- x0
                        sValue.[i].M01.Value <- x1
                        sValue.[i].M02.Value <- x2
                        sValue.[i].M03.Value <- x3
                        sValue.[i].M10.Value <- x4
                        sValue.[i].M11.Value <- x5
                        sValue.[i].M12.Value <- x6
                        sValue.[i].M13.Value <- x7
                        sValue.[i].M20.Value <- x8
                        sValue.[i].M21.Value <- x9
                        sValue.[i].M22.Value <- x10
                        sValue.[i].M23.Value <- x11
                        sValue.[i].M30.Value <- x12
                        sValue.[i].M31.Value <- x13
                        sValue.[i].M32.Value <- x14
                        sValue.[i].M33.Value <- x15
                    
                let updateInput(src : nativeptr<float>) =
                    let mutable si = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        let x0 = NativePtr.get src si
                        si <- si + 1
                        let x1 = NativePtr.get src si
                        si <- si + 1
                        let x2 = NativePtr.get src si
                        si <- si + 1
                        let x3 = NativePtr.get src si
                        si <- si + 1
                        let x4 = NativePtr.get src si
                        si <- si + 1
                        let x5 = NativePtr.get src si
                        si <- si + 1
                        let x6 = NativePtr.get src si
                        si <- si + 1
                        let x7 = NativePtr.get src si
                        si <- si + 1
                        let x8 = NativePtr.get src si
                        si <- si + 1
                        let x9 = NativePtr.get src si
                        si <- si + 1
                        let x10 = NativePtr.get src si
                        si <- si + 1
                        let x11 = NativePtr.get src si
                        si <- si + 1
                        let x12 = NativePtr.get src si
                        si <- si + 1
                        let x13 = NativePtr.get src si
                        si <- si + 1
                        let x14 = NativePtr.get src si
                        si <- si + 1
                        let x15 = NativePtr.get src si
                        si <- si + 1
                        value.[i] <- M44d(x0, x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12, x13, x14, x15)
                    
                let writeTo (dst : nativeptr<float>) =
                    let mutable di = state.VariableOffset
                    for i in 0 .. value.Length - 1 do
                        NativePtr.set dst di value.[i].M00
                        di <- di + 1
                        NativePtr.set dst di value.[i].M01
                        di <- di + 1
                        NativePtr.set dst di value.[i].M02
                        di <- di + 1
                        NativePtr.set dst di value.[i].M03
                        di <- di + 1
                        NativePtr.set dst di value.[i].M10
                        di <- di + 1
                        NativePtr.set dst di value.[i].M11
                        di <- di + 1
                        NativePtr.set dst di value.[i].M12
                        di <- di + 1
                        NativePtr.set dst di value.[i].M13
                        di <- di + 1
                        NativePtr.set dst di value.[i].M20
                        di <- di + 1
                        NativePtr.set dst di value.[i].M21
                        di <- di + 1
                        NativePtr.set dst di value.[i].M22
                        di <- di + 1
                        NativePtr.set dst di value.[i].M23
                        di <- di + 1
                        NativePtr.set dst di value.[i].M30
                        di <- di + 1
                        NativePtr.set dst di value.[i].M31
                        di <- di + 1
                        NativePtr.set dst di value.[i].M32
                        di <- di + 1
                        NativePtr.set dst di value.[i].M33
                        di <- di + 1
                    
                state.CachedArrays.[value] <- sValue
                arr <- sValue
                runState <- {
                    state with
                        UpdateScalars = updateScalars :: state.UpdateScalars
                        UpdateInput = updateInput :: state.UpdateInput
                        WriteTo = writeTo :: state.WriteTo
                        VariableOffset = state.VariableOffset + 16*value.Length
                }
            cont arr runState
