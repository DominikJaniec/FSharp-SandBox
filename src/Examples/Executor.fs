module Executor

open canopy.runner.classic
open canopy.classic
open canopy.types


type Log =
    string -> unit

type Parameters =
    { browser: BrowserStartMode
    ; leftBrowserOpen: bool
    ; register: Log -> unit
    ; log: Log
    }


let executeWith (param: Parameters) =

    // Starts instance of given testing Browser
    start param.browser

    // Registers and executes tests defined above
    param.register param.log
    run()

    // Allows to take a look at Browser recent state
    if param.leftBrowserOpen then
        printfn "Press [enter] to exit the %A" param.browser
        System.Console.ReadLine() |> ignore

    // Ends sessions
    quit()

    ()

