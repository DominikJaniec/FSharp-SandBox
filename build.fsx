#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Paket
//"

open Fake.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet


let logAsHeader header =
  let headLine = sprintf @"   \_  %s  _/  " header
  let barLine =
    let len = String.length header
    System.String('=', len + 8)
    |> sprintf "==+%s+=="

  [ barLine; headLine; "" ]
  |> List.iter Trace.log


module SandBox =

  let projectPath name =
    Path.combine "src" name

  let projectFilePath name =
    let proj = sprintf "%s.fsproj" name
    Path.combine (projectPath name) proj

  let executeProject name =
    let path = projectFilePath name
    sprintf "Executing '%s' project" path
    |> logAsHeader

    DotNet.exec id "run" ("--project " + path)
    |> ignore

  let buildProject name =
    let path = projectFilePath name
    sprintf "Building '%s' project" path
    |> logAsHeader

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
  logAsHeader "FSharp-SandBox by Dominik Janiec"
  Target.listAvailable()
  [ ""
  ; "To run one of them, just execute command:"
  ; "> dotnet fake -s build -t <TargetName>"
  ; ""
  ; "----"
  ] |> List.iter Trace.log


Target.create core.Init <| fun _ ->
  logAsHeader "Initializing FSharp-SandBox"
  Paket.restore id

Target.create core.Continuum <| fun _ ->
  SandBox.buildProject core.Continuum

Target.create core.Build <| fun _ ->
  logAsHeader "Executing SandBox Build"
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
