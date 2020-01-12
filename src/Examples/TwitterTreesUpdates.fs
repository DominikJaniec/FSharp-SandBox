module TwitterTreesUpdates

open System
open canopy.classic
open Continuum.Gatherer.Core


let private executionIdentity = "TwitterTreesUpdates"

let private tweetsPage = "https://twitter.com/TreesUpdates"
let private tweeterUserName = "TeamTrees Updates"


let lastDays (lastDays: int) (context: Executor.Context) =

    let log message =
        context.log message

    let screenshotAs description =
        context.screenshot executionIdentity description

    let waitForNextStep () =
        sleep 1
        ()


    let ensureExpectedUserPage() =
        log "Ensuring opened page is expected."

        let headingSelector = ".ProfileHeaderCard-nameLink"
        let headings = elements headingSelector

        (List.length headings, headingSelector)
        ||> sprintf "Found %d elements matching selector: '%s'."
        |> log

        read headingSelector
        |> sprintf "Text content of the first one: '%s'."
        |> log

        headingSelector == tweeterUserName
        highlight headingSelector

        screenshotAs "expected Twitter profile"
        waitForNextStep()


    let ensureExpectedTweetsTab() =
        log "Ensuring activated tab is expected."

        let tabSelector = "[data-element-term=tweets_toggle].is-active"

        displayed tabSelector
        highlight tabSelector

        let textSelector = "[aria-hidden=true]"
        (element tabSelector |> elementWithin textSelector).Text
        |> sprintf "Expected tab is active displaying: '%s'."
        |> log

        screenshotAs "expected Tweets tab"
        waitForNextStep()


    let openTwitterPage() =
        log <| sprintf "Opening page: '%s'..." tweetsPage

        url tweetsPage
        screenshotAs "page opened"
        waitForNextStep()

        ensureExpectedUserPage()
        ensureExpectedTweetsTab()

        log "Opened at Twitter page."
        waitForNextStep()


    let getTweetsUntil (pastLimit: DateTime) =
        Tools.asTimestamp' pastLimit
        |> sprintf "Looking for Tweets not older than: %s."
        |> log

        let batchLimit = 7 // TODO: make this infinite source
        let tweetSelector = "[data-item-type='tweet']:not(.js-pinned)"
        seq {
            for i in 0 .. batchLimit do
                let element = nth i tweetSelector
                let id = "#" + element.GetAttribute("id")

                // TODO: scroll to it, so highlighting is visible
                highlight id
                log <| sprintf "Retrieving #%d tweet: '%s'." (i+1) id

                waitForNextStep()
                yield element
        }


    fun _ ->
        let tweetsLimit = 3
        let pastLimit =
            TimeSpan.FromDays(float lastDays)
            |> DateTime.UtcNow.Subtract

        openTwitterPage()
        getTweetsUntil pastLimit
        |> Seq.take tweetsLimit
        |> Seq.iter (fun el ->
            // TODO: use it!
            log el.Text
        )

        ()
