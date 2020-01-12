open canopy.classic

[<Literal>]
let EXIT_SUCCESS = 0


[<EntryPoint>]
let main argv =

    Executor.executeWith
        { register = Examples.canopyDemo
        // { register = Examples.allDemos
        ; browser = firefox
        ; leftBrowserOpen = false
        ; log = fun msg -> printfn "# Log: %s" msg
        }

    EXIT_SUCCESS
