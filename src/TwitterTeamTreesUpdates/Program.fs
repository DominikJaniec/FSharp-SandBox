open System
open Continuum.Common
open TwitterTeamTreesUpdates

[<Literal>]
let EXIT_SUCCESS = 0


[<EntryPoint>]
let main argv =

    Runtime.fixCurrentDirectory()

    Analyzer.tweetsSourceFile
    |> Analyzer.analyzeTeamTrees

    EXIT_SUCCESS
