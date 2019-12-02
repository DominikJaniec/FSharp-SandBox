module Tests

open Expecto
open Continuum.Gatherer

[<Tests>]
let tests =
  testList "samples" [
    testCase "universe exists" <| fun _ ->
      let subject = true
      Expect.isTrue subject "I compute, therefore I am."

    testCase "should fail" <| fun _ ->
      let subject = false
      Expect.isTrue subject "I should fail because the subject is false."

    testCase "let's start" <| fun _ ->
      let result =
        Gatherer.prepare null
        |> Gatherer.execute

      Expect.isOk result "Should be OK"
  ]
