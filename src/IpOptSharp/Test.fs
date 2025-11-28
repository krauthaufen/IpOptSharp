module IpOptSharp.Test

open IpOptSharp
open Aardvark.Base
open IpOptSharp

let test () =
    let a = ref (V2d(1.0, 2.0))
    let b = ref 3.0
    let (status, objective) = 
        ipopt {
            let! a = a
            let! b = b
            
            // objective
            minimize (sqr (a.X - a.Y * b))
            
            // constraints
            range b -0.1 0.1
            equal a.Length 1.0
        }

    printfn "status: %A" status 
    printfn "obj:    %.4f" objective 
    printfn "a:      %s (length: %.4f)" (a.Value.ToString "0.0000") a.Value.Length
    printfn "b:      %.4f" b.Value       
    // status: Success
    // obj:    0.0000
    // a:      [0.0744, 0.9972] (length: 1.0000)
    // b:      0.0746



