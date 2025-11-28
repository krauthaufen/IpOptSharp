# IpOptSharp

A F# wrapper for IPOPT (Interior Point OPTimizer) that provides a fluent computation expression API for defining constrained nonlinear optimization problems with automatic differentiation.

## Example

Find the triangle inscribed in a unit circle with maximum perimeter:

```fsharp
open IpOptSharp
open Aardvark.Base

let a = ref (V2d(-1.0, -1.0))
let b = ref (V2d(1.0, -1.0))
let c = ref (V2d(0.0, 1.0))

let (status, objective) =
    ipopt {
        let! a = a
        let! b = b
        let! c = c

        // maximize the circumference
        let circumference =
            Vec.length (b - a) +
            Vec.length (c - b) +
            Vec.length (a - c)
        IpOpt.Maximize circumference

        // constrain all vertices to the unit circle
        IpOpt.Equal(a.Length, 1.0)
        IpOpt.Equal(b.Length, 1.0)
        IpOpt.Equal(c.Length, 1.0)
    }

// status: Success
// objective: 5.1962 (3 * sqrt(3))
// Result: Equilateral triangle with 60° angles
```

## Features

- **Computation Expression Builder**: Define optimization problems using F# computation expressions
- **Automatic Differentiation**: Gradients and Jacobians computed automatically via the scalar type system
- **Type-Safe**: Supports floats, vectors (V2d, V3d, V4d), matrices (M22d, M33d, M44d), and transformations (Rot3d, Euclidean3d)
- **Arrays and References**: Bind optimization variables as either mutable references or arrays

## API

### Objectives
```fsharp
IpOpt.Minimize expr
IpOpt.Maximize expr
```

### Constraints
```fsharp
IpOpt.Equal(expr, value)           // expr = value
IpOpt.LessOrEqual(expr, value)     // expr <= value
IpOpt.GreaterOrEqual(expr, value)  // expr >= value
IpOpt.Range(expr, min, max)      // min <= expr <= max
// ... and more geometric constraints
```

### Solver Options
```fsharp
IpOpt.MaxIterations 1000
IpOpt.Tolerance 1e-8
IpOpt.PrintLevel 5
IpOpt.LinearSolver "mumps"
// ... and many more IPOPT options
```
