open Continuum.Common
open canopy.classic

[<Literal>]
let EXIT_SUCCESS = 0


[<EntryPoint>]
let main argv =

    Runtime.fixCurrentDirectory()

    let log =
        { new Executor.ILog with
            member __.Info (message: string): unit =
                printfn "# Log: %s" message
        }

    Executor.executeWith
        { register = Examples.allDemos
        ; browser = firefox
        ; leftBrowserOpen = false
        ; log = log
        }

    EXIT_SUCCESS
