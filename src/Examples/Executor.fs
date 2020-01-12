module Executor

open System
open System.IO
open canopy.runner.classic
open canopy.classic
open canopy.types
open Continuum.Gatherer.Core


type Log =
    string -> unit

type Screenshot =
    string -> string -> unit

type Context =
    { log: Log
    ; screenshot: Screenshot
    }

type Parameters =
    { register: Context -> unit
    ; browser: BrowserStartMode
    ; leftBrowserOpen: bool
    ; log: Log
    }


let private makeContextFrom (param: Parameters) =

    let takeScreenshot identity description =
        let timestamp = Tools.asTimestamp DateTime.UtcNow
        let filename = timestamp + " " + description
        let directory = Path.Combine("results", identity)
        let data = screenshot directory filename

        (Array.length data, directory, filename)
        |||> sprintf "Screenshot taken and saved (%d bytes) in file: '%s|%s'."
        |> param.log

    { log = param.log
    ; screenshot = takeScreenshot
    }


let executeWith (param: Parameters) =

    // Starts instance of given testing Browser
    start param.browser

    // Registers and executes tests defined above
    makeContextFrom param
    |> param.register

    run()

    // Allows to take a look at Browser last state
    if param.leftBrowserOpen then
        printfn "Press [enter] to exit the %A" param.browser
        Console.ReadLine() |> ignore

    // Ends sessions
    quit()
