#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
//"

open Fake.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet


module GathererTarget =

  let private paths =
    let proj name =
      sprintf "Continuum.%s.fsproj" name
      |> Path.combine name
      |> Path.combine "src"

    {| Gatherer = proj "Gatherer"
    ;  GathererTests = proj "Gatherer.Tests"
    |}


  let build (_: TargetParameter) =
    Trace.log " --- Building the app --- "
    DotNet.build id paths.Gatherer
    DotNet.build id paths.GathererTests
    ()


  let test (_: TargetParameter) =
    Trace.log " --- Executing tests --- "
    DotNet.test id paths.GathererTests


module ExamplesTarget =

  let private path =
    "Examples.fsproj"
    |> Path.combine "Examples"
    |> Path.combine "src"


  let execute (_: TargetParameter) =
    Trace.log " --- Executing examples --- "
    DotNet.exec id "run" ("--project " + path)
    |> ignore


// *** Define Targets ***

Target.create "Clean" <| fun _ ->
  Trace.log " --- Cleaning stuff --- "

Target.create "Build"
  GathererTarget.build

Target.create "ReBuild" <| fun _ ->
  Trace.log " --- Executing ReBuild --- "

Target.create "Test"
  GathererTarget.test

Target.create "Example"
  ExamplesTarget.execute

Target.create "Sample"
  ExamplesTarget.execute


// *** Define Dependencies ***

"Clean" ?=> "Build"
"Clean" ==> "ReBuild"
"Build" ==> "ReBuild"


// *** Start Build ***
Target.runOrDefault "Test"
