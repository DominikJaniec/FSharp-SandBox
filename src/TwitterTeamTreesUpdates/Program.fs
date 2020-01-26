open System
open Continuum.Common
open System.IO

[<Literal>]
let EXIT_SUCCESS = 0


[<EntryPoint>]
let main argv =

    Runtime.fixCurrentDirectory()

    printfn "All available files:"
    Directory.EnumerateFiles "."
    |> Seq.iteri (fun i file ->
        printfn "%3i. '%s'" (i + 1) file
    )

    EXIT_SUCCESS
