
type VariableType =
    | Float
    | Vector of dim : int
    | Matrix of rows : int * cols : int
    

type InputType =
    | Array of VariableType
    | Ref of VariableType

    member x.VariableType =
        match x with
        | Array vt -> vt
        | Ref vt -> vt

let types =
    [
        for arr in [false; true] do
            for vt in [Float; Vector 2; Vector 3; Vector 4; Matrix (2,2); Matrix (3,3); Matrix (4,4)] do
                if arr then yield Array vt
                else yield Ref vt
    ]
 
let inputName (t : InputType) =
    match t with
    | Array vt ->
        match vt with
        | Float -> "float[]"
        | Vector dim -> $"V{dim}d[]"
        | Matrix (r, c) -> $"M{r}{c}d[]"
    | Ref vt ->
        match vt with
        | Float -> "ref<float>"
        | Vector dim -> $"ref<V{dim}d>"
        | Matrix (r, c) -> $"ref<M{r}{c}d>"
  
let cacheName (t : InputType) =
    match t with
    | Array vt ->
        match vt with
        | Float -> "array<scalar>"
        | Vector dim -> $"array<V{dim}s>"
        | Matrix (r, c) -> $"array<M{r}{c}s>"
    | Ref vt ->
        match vt with
        | Float -> "ref<scalar>"
        | Vector dim -> $"ref<V{dim}s>"
        | Matrix (r, c) -> $"ref<M{r}{c}s>"
   
let scalarName (t : InputType) =
    match t with
    | Array vt ->
        match vt with
        | Float -> "scalar[]"
        | Vector dim -> $"V{dim}s[]"
        | Matrix (r, c) -> $"M{r}{c}s[]"
    | Ref vt ->
        match vt with
        | Float -> "scalar"
        | Vector dim -> $"V{dim}s"
        | Matrix (r, c) -> $"M{r}{c}s"
 
 
let vecComponent (i : int) =
    match i with
    | 0 -> "X"
    | 1 -> "Y"
    | 2 -> "Z"
    | 3 -> "W"
    | _ -> failwith "invalid vector component"
    
let matComponent (r : int) (c : int) =
    $"M{r}{c}"
 
let read (t : InputType) (str : string) =
    match t with
    | Array _ -> str
    | Ref _ -> $"{str}.Value"
 
let baseFloatCount (t : VariableType) =
    match t with
    | Float -> 1
    | Vector dim -> dim
    | Matrix (r,c) -> r * c
 
let floatCount (t : InputType)=
    match t with
    | Array vt ->
        let c = baseFloatCount vt
        fun s -> sprintf "%s.Length * %d" s c
      
    | Ref vt ->
        let c = baseFloatCount vt
        fun _ -> sprintf "%d" c

let b = System.Text.StringBuilder()
let printfn fmt = Printf.kprintf (fun str -> b.AppendLine str |> ignore) fmt
 
printfn "namespace IpOptSharp"

printfn "open Aardvark.Base"
printfn "open Microsoft.FSharp.NativeInterop"
printfn "open System.Runtime.CompilerServices"
printfn "open IpOptSharp.IpOptBuilderImplementation"
printfn "#nowarn \"9\""
printfn ""
printfn $"[<AbstractClass; Sealed>]"
printfn "type IpOptBuilderExtensions private() = "
 
for t in types do
    let bcnt = baseFloatCount t.VariableType
    
    let ctorName =
        match t.VariableType with
        | Float -> "scalar"
        | Vector dim -> $"V{dim}s"
        | Matrix (r,c) -> $"M{r}{c}s"
    
    let access i =
        match t.VariableType with
        | Float -> fun v -> $"{v}"
        | Vector dim -> fun v -> $"{v}.{vecComponent i}"
        | Matrix (r,c) -> fun v -> $"{v}.{matComponent (i / r) (i % r)}" 
    printfn $"    [<Extension>]"
    printfn $"    static member inline Bind(x : IpOptBuilder, value : {inputName t}, [<InlineIfLambda>] cont : {scalarName t} -> Builder) : Builder ="
    printfn $"        fun state ->"
    printfn $"            let mutable arr = Unchecked.defaultof<_>"
    printfn $"            let mutable runState = state"
    printfn $"            match state.CachedArrays.TryGetValue value with"
    printfn $"            | (true, (:? {cacheName t} as cached)) ->"
    printfn $"                arr <- cached"
    printfn $"            | _ ->"

    match t with
    | Array _ ->
        printfn $"                let sValue ="
        printfn $"                    value |> Array.mapi (fun i v ->"
        printfn $"                        let bi = {bcnt}*i"
        match t.VariableType with
        | Float ->
            printfn $"                        scalar.Variable(state.VariableOffset + bi, v)"
        | _ ->
            printfn $"                        {ctorName}("
            for i in 0 .. bcnt - 1 do
                let a = access i "v"
                let comma = if i < bcnt - 1 then "," else ""
                printfn $"                            scalar.Variable(state.VariableOffset + bi + {i}, {a}){comma}"
            printfn $"                        )"
        printfn $"                    )"
        printfn $"                    "
        printfn $"                let updateScalars(src : nativeptr<float>) ="
        printfn $"                    let mutable si = state.VariableOffset"
        printfn $"                    for i in 0 .. value.Length - 1 do"
        for i in 0 .. bcnt - 1 do
            printfn $"                        let x{i} = NativePtr.get src si"
            printfn $"                        si <- si + 1"
        for i in 0 .. bcnt - 1 do
            let accessPath =
                match t.VariableType with
                | Float -> ""
                | Vector _ -> $".{vecComponent i}"
                | Matrix (r,_) -> $".{matComponent (i / r) (i % r)}"
            printfn $"                        sValue.[i]{accessPath}.Value <- x{i}"
        printfn $"                    "
        printfn $"                let updateInput(src : nativeptr<float>) ="
        printfn $"                    let mutable si = state.VariableOffset"
        printfn $"                    for i in 0 .. value.Length - 1 do"
        for i in 0 .. bcnt - 1 do
            printfn $"                        let x{i} = NativePtr.get src si"
            printfn $"                        si <- si + 1"
        match t.VariableType with
        | Float ->
            printfn $"                        value.[i] <- x0"
        | Vector dim ->
            let args = [0..dim-1] |> List.map (fun i -> $"x{i}") |> String.concat ", "
            printfn $"                        value.[i] <- V{dim}d({args})"
        | Matrix (r,c) ->
            let args = [0..r*c-1] |> List.map (fun i -> $"x{i}") |> String.concat ", "
            printfn $"                        value.[i] <- M{r}{c}d({args})"
        printfn $"                    "
        printfn $"                let writeTo (dst : nativeptr<float>) ="
        printfn $"                    let mutable di = state.VariableOffset"
        printfn $"                    for i in 0 .. value.Length - 1 do"
        for i in 0 .. bcnt - 1 do
            let accessPath =
                match t.VariableType with
                | Float -> ""
                | Vector _ -> $".{vecComponent i}"
                | Matrix (r,_) -> $".{matComponent (i / r) (i % r)}"
            printfn $"                        NativePtr.set dst di value.[i]{accessPath}"
            printfn $"                        di <- di + 1"
        printfn $"                    "
        printfn $"                state.CachedArrays.[value] <- sValue"
        printfn $"                arr <- sValue"
        printfn $"                runState <- {{"
        printfn $"                    state with"
        printfn $"                        UpdateScalars = updateScalars :: state.UpdateScalars"
        printfn $"                        UpdateInput = updateInput :: state.UpdateInput"
        printfn $"                        WriteTo = writeTo :: state.WriteTo"
        printfn $"                        VariableOffset = state.VariableOffset + {bcnt}*value.Length"
        printfn $"                }}"
        printfn $"            cont arr runState"
    | Ref _ ->
        printfn $"                let sValue ="
        match t.VariableType with
        | Float ->
            let readValue = read t "value"
            printfn $"                    ref (scalar.Variable(state.VariableOffset, {readValue}))"
        | _ ->
            printfn $"                    ref ("
            printfn $"                        {ctorName}("
            for i in 0 .. bcnt - 1 do
                let a = access i (read t "value")
                let comma = if i < bcnt - 1 then "," else ""
                printfn $"                            scalar.Variable(state.VariableOffset + {i}, {a}){comma}"
            printfn $"                        )"
            printfn $"                    )"
        printfn $"                    "
        printfn $"                let updateScalars(src : nativeptr<float>) ="
        for i in 0 .. bcnt - 1 do
            let accessPath =
                match t.VariableType with
                | Float -> ""
                | Vector _ -> $".{vecComponent i}"
                | Matrix (r,_) -> $".{matComponent (i / r) (i % r)}"
            printfn $"                    sValue.contents{accessPath}.Value <- NativePtr.get src (state.VariableOffset + {i})"
        printfn $"                    "
        printfn $"                let updateInput(src : nativeptr<float>) ="
        match t.VariableType with
        | Float ->
            printfn $"                    value.Value <- NativePtr.get src state.VariableOffset"
        | Vector dim ->
            let args = [0..dim-1] |> List.map (fun i -> $"NativePtr.get src (state.VariableOffset + {i})") |> String.concat ", "
            printfn $"                    value.Value <- V{dim}d({args})"
        | Matrix (r,c) ->
            let args = [0..r*c-1] |> List.map (fun i -> $"NativePtr.get src (state.VariableOffset + {i})") |> String.concat ", "
            printfn $"                    value.Value <- M{r}{c}d({args})"
        printfn $"                    "
        printfn $"                let writeTo (dst : nativeptr<float>) ="
        for i in 0 .. bcnt - 1 do
            let accessPath =
                match t.VariableType with
                | Float -> ""
                | Vector _ -> $".{vecComponent i}"
                | Matrix (r,_) -> $".{matComponent (i / r) (i % r)}"
            let readValue = read t "value"
            printfn $"                    NativePtr.set dst (state.VariableOffset + {i}) {readValue}{accessPath}"
        printfn $"                    "
        printfn $"                state.CachedArrays.[value] <- sValue"
        printfn $"                arr <- sValue"
        printfn $"                runState <- {{"
        printfn $"                    state with"
        printfn $"                        UpdateScalars = updateScalars :: state.UpdateScalars"
        printfn $"                        UpdateInput = updateInput :: state.UpdateInput"
        printfn $"                        WriteTo = writeTo :: state.WriteTo"
        printfn $"                        VariableOffset = state.VariableOffset + {bcnt}"
        printfn $"                }}"
        printfn $"            cont arr.Value runState"

open System.IO
// Output the generated code
File.WriteAllText(Path.Combine(__SOURCE_DIRECTORY__, "Bind.fs"), b.ToString())
System.Console.WriteLine(b.ToString())
