#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
//"

open Fake.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet


let logHeader header =
  sprintf " --- %s ---" header
  |> Trace.log


module SandBox =

  let projectPath name =
    Path.combine "src" name

  let projectFilePath name =
    let proj = sprintf "%s.fsproj" name
    Path.combine (projectPath name) proj

  let executeProject name =
    let path = projectFilePath name
    sprintf "Executing '%s' project" path
    |> logHeader

    DotNet.exec id "run" ("--project " + path)
    |> ignore

  let buildProject name =
    let path = projectFilePath name
    sprintf "Building '%s' project" path
    |> logHeader

    DotNet.build id path


// *** Define Targets ***

let sandbox =
  {| Help = "SandBox-Help"
     Init = "SandBox-Init"
  ;  Build = "SandBox-Build"
  |}

let idea =
  {| SeleniumViaCanopy = "SeleniumViaCanopy"
  |}


Target.create sandbox.Help <| fun _ ->
  logHeader "FSharp-SandBox by Dominik Janiec"
  Target.listAvailable()

Target.create sandbox.Init <| fun _ ->
  logHeader "Initializing FSharp-SandBox"
  ("paket", "--silent restore")
  ||> CreateProcess.fromRawCommandLine
  |> Proc.run
  |> ignore

Target.create sandbox.Build <| fun _ ->
  logHeader "Executing SandBox Build"
  [ idea.SeleniumViaCanopy ]
  |> List.iter SandBox.buildProject


Target.create idea.SeleniumViaCanopy <| fun _ ->
  SandBox.executeProject idea.SeleniumViaCanopy


// *** Define Dependencies ***

sandbox.Init ==> sandbox.Build
sandbox.Init ==> idea.SeleniumViaCanopy


// *** Start Build ***
Target.runOrDefault sandbox.Help
