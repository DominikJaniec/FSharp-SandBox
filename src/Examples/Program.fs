open canopy.classic

[<Literal>]
let EXIT_SUCCESS = 0


[<EntryPoint>]
let main argv =

    let log =
        { new Executor.ILog with
            member __.Info (message: string): unit =
                printfn "# Log: %s" message
        }

    Executor.executeWith
        { register = Examples.canopyDemo
        // { register = Examples.allDemos
        ; browser = firefox
        ; leftBrowserOpen = false
        ; log = log
        }

    EXIT_SUCCESS
