module Examples

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


let canopyDemo (_: Executor.Log) =
    canopyTest ()


let twitterTreesUpdates (log: Executor.Log) =
    "Traverses last 7 days of @TreesUpdates' tweets" &&&
        (TwitterTreesUpdates.lastDays 7 log)


let allDemos (log: Executor.Log) =

    // Defines basic test demos
    "Executor.Log test as demo" &&& fun _ ->
        log "Executing test of logs"

    canopyDemo log

    // Registers other demos
    twitterTreesUpdates log
