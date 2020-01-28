open System
open Continuum.Common
open TwitterTeamTreesUpdates

[<Literal>]
let EXIT_SUCCESS = 0


[<EntryPoint>]
let main argv =

    Runtime.fixCurrentDirectory()

    UpdatesLoader.tweetsSourceFile
    |> UpdatesLoader.loadUpdates
    |> Analyzer.analyze

    EXIT_SUCCESS
