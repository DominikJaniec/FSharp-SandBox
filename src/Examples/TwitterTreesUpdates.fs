module TwitterTreesUpdates

open System
open System.IO
open canopy.classic


let private tweetsPage = "https://twitter.com/TreesUpdates"
let private tweeterUserName = "TeamTrees Updates"


let lastDays (lastDays: int) (log: Executor.Log) =

    let takeScreenshotAs description =
        let directory = Path.Combine("results", "TwitterTreesUpdates")
        let timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd+HH.mm.ss.fff")
        let filename = String.concat "" [ timestamp; " "; description ]
        let data = screenshot directory filename

        (Array.length data, filename)
        ||> sprintf "Screenshot taken and saved (%d bytes) in file: '%s'."
        |> log


    let ensureExpectedUserPage () =
        log "Ensuring opened page is expected."

        let headingSelector = ".ProfileHeaderCard-nameLink"
        let headings = elements headingSelector

        ( List.length headings, headingSelector )
        ||> sprintf "Found %d elements matching selector: '%s'."
        |> log

        read headingSelector
        |> sprintf "Text content of the first one: '%s'."
        |> log

        headingSelector == tweeterUserName
        highlight headingSelector

        takeScreenshotAs "expected Twitter profile"


    let ensureExpectedTweetsTab () =
        log "Ensuring activated tab is expected."

        let tabSelector = "[data-element-term=tweets_toggle].is-active"

        displayed tabSelector
        highlight tabSelector

        let textSelector = "[aria-hidden=true]"
        (element tabSelector |> elementWithin textSelector).Text
        |> sprintf "Expected tab is active displaying: '%s'."
        |> log

        takeScreenshotAs "expected Tweets tab"


    let openTwitterPage () =
        log <| sprintf "Opening page: '%s'..." tweetsPage

        url tweetsPage
        takeScreenshotAs "page opened"

        ensureExpectedUserPage ()
        ensureExpectedTweetsTab ()

        log "Opened at Twitter page."


    fun _ ->

        openTwitterPage ()

        ()
