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

let core =
  {| Help = "SandBox-Help"
  ;  Init = "SandBox-Init"
  ;  Build = "SandBox-Build"
  ;  Continuum = "Continuum.Common"
  |}

let ideas =
  {| SeleniumViaCanopy = "SeleniumViaCanopy"
  ;  TwitterTeamTreesUpdates = "TwitterTeamTreesUpdates"
  |}


Target.create core.Help <| fun _ ->
  logHeader "FSharp-SandBox by Dominik Janiec"
  Target.listAvailable()

Target.create core.Init <| fun _ ->
  logHeader "Initializing FSharp-SandBox"
  ("paket", "--silent restore")
  ||> CreateProcess.fromRawCommandLine
  |> Proc.run
  |> ignore

Target.create core.Continuum <| fun _ ->
  SandBox.buildProject core.Continuum

Target.create core.Build <| fun _ ->
  logHeader "Executing SandBox Build"
  [ ideas.SeleniumViaCanopy
  ; ideas.TwitterTeamTreesUpdates
  ] |> List.iter SandBox.buildProject


Target.create ideas.SeleniumViaCanopy <| fun _ ->
  SandBox.executeProject ideas.SeleniumViaCanopy

Target.create ideas.TwitterTeamTreesUpdates <| fun _ ->
  SandBox.executeProject ideas.TwitterTeamTreesUpdates

// *** Define Dependencies ***

core.Init ==> core.Build
core.Init ==> core.Continuum
core.Continuum ==> core.Build
core.Continuum ==> ideas.SeleniumViaCanopy
core.Continuum ==> ideas.TwitterTeamTreesUpdates


// *** Start Build ***
Target.runOrDefault core.Help
