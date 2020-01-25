module Examples

open System
open canopy.runner.classic
open canopy.classic


let canopyTest () =

    // Defines tests with given description name
    "Takes canopy for a spin" &&& fun _ ->

        // Opens Browser at that page
        url "http://lefthandedgoat.github.io/canopy/testpages/"

        // Asserts that:
        // * the element with an id of 'welcome'
        // * has the text (value) equal to 'Welcome'
        "#welcome" == "Welcome"

        // Asserts that
        // * the value of the element on the left
        // * is equal to the value on the right
        "#firstName" == "John"

        // Changes the value of element with matched id
        "#firstName" << "Something Else"

        // Verifies another element's value via assertion
        "#button_clicked" == "button not clicked"

        // Clicks button found by selector
        click "#button"

        // Asserts change on UI
        "#button_clicked" == "button clicked"


let canopyDemo (_: Executor.Context) =
    canopyTest ()


let twitterTreesUpdates (context: Executor.Context) =
    // Date of first "official" YouTube announcement about #TeamTrees:
    // https://twitter.com/YouTube/status/1187806031520268288
    let limit = DateTime(2019, 10, 25 - 1)

    "Traverses all @TreesUpdates' tweets about #TeamTrees" &&&
        (TwitterTreesUpdates.tweetsUntil limit context)


let allDemos (context: Executor.Context) =

    // Defines basic test demos
    "Executor.Log test as demo" &&& fun _ ->
        context.log.Info "Executing test of logs"

    canopyDemo context

    // Registers other demos
    twitterTreesUpdates context
