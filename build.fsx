open Fake.IO
#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
//"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet


module GathererBuild =

  let private paths =
    let src name =
      Path.combine "src" name

    {| Gatherer = src "Gatherer"
    |}


  let execute (param: TargetParameter) =
    Trace.log " --- Building the app --- "
    DotNet.build id paths.Gatherer
    ()


// *** Define Targets ***

Target.create "Clean" <| fun _ ->
  Trace.log " --- Cleaning stuff --- "

Target.create "Build"
  GathererBuild.execute

Target.create "ReBuild" <| fun _ ->
  Trace.log " --- Executing ReBuild ---"


// *** Define Dependencies ***

"Clean" ?=> "Build"
"Clean" ==> "ReBuild"
"Build" ==> "ReBuild"

// *** Start Build ***
Target.runOrDefault "Build"
