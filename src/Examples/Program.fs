open canopy.classic

[<Literal>]
let EXIT_SUCCESS = 0


[<EntryPoint>]
let main argv =

    Executor.executeWith
        { browser = firefox
        ; leftBrowserOpen = false
        ; register = Examples.allDemos
        // ; register = Examples.canopyDemo
        ; log = fun msg -> printfn "# Log: %s" msg
        }

    EXIT_SUCCESS
