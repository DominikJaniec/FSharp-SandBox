module Executor

open System
open System.IO
open canopy.runner.classic
open canopy.classic
open canopy.types
open Continuum.Common


type ILog =
    abstract member Info : string -> unit

type IStorage =
    abstract member LoadFileAs : string -> string -> string
    abstract member SaveFileAs : string -> string -> string[] -> unit
    abstract member ScreenshotAs : string -> string -> unit

type Context =
    { log: ILog
    ; storage: IStorage
    }

type Parameters =
    { register: Context -> unit
    ; browser: BrowserStartMode
    ; leftBrowserOpen: bool
    ; log: ILog
    }


let private makeContextFrom (param: Parameters) =

    let directoryOf identity =
        Path.Combine("results", identity)

    let pathFor identity filename =
        let timestamp = Time.asStamp Time.Now
        let filename = timestamp + " " + filename
        let directory = directoryOf identity
        let combined = Path.Combine(directory, filename)
        (directory, filename, Path.GetFullPath combined)

    { log = param.log
    ; storage =
        { new IStorage with

            member __.LoadFileAs (identity: string) (filename: string) : string =
                let filePath =
                    let directory = directoryOf identity
                    Path.Combine(directory, filename)

                let content = File.ReadAllText(filePath)
                (content.Length, filePath)
                ||> sprintf "Loaded %d characters from file: '%s'."
                |> param.log.Info

                content

            member __.SaveFileAs (identity: string) (filename: string) (lines: string[]) : unit =
                let (_, _, filePath) = pathFor identity filename
                File.WriteAllLines(filePath, lines)

                (FileInfo(filePath).Length, filePath)
                ||> sprintf "All file lines saved (%d bytes) in file: '%s'."
                |> param.log.Info

            member __.ScreenshotAs (identity: string) (description: string) : unit =
                let (directory, filename, filePath) = pathFor identity description
                let data = screenshot directory filename

                (Array.length data, filePath)
                ||> sprintf "Screenshot taken and saved (%d bytes) in file: '%s'."
                |> param.log.Info
        }
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
