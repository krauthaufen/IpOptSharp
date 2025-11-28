namespace IpOptSharp

/// IPOPT return codes
type IpOptStatus =
    | Success = 0
    | Solved = 1
    | InfeasibleProblemDetected = 2
    | SearchDirectionBecomesTooSmall = 3
    | DivergingIterates = 4
    | UserRequestedStop = 5
    | FeasiblePointFound = 6
    | MaximumIterationsExceeded = -1
    | RestorationFailed = -2
    | ErrorInStepComputation = -3
    | MaximumCpuTimeExceeded = -4
    | NotEnoughDegreesOfFreedom = -10
    | InvalidProblemDefinition = -11
    | InvalidOption = -12
    | InvalidNumberDetected = -13
    | UnrecoverableException = -100
    | NonIpoptExceptionThrown = -101
    | InsufficientMemory = -102
    | InternalError = -199
  
[<RequireQualifiedAccess>]
type IpOptOption =
    | MaxIterations of int
    | Tolerance of float
    | PrintLevel of int
    | CpuTimeLimit of float
    | LinearSolver of string
    | DerivativeTest of string
    | HessianApproximation of string
    | MuStrategy of string
    | OutputFile of string
    | Int of string * int
    | Float of string * float
    | String of string * string
    