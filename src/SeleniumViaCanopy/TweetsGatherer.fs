module TweetsGatherer

open System
open FSharp.Json
open OpenQA.Selenium
open canopy.classic
open Continuum.Common
open SeleniumViaCanopy.Tools


type Config =
    { executionIdentity: string
    ; twitterDisplayName: string
    ; twitterPageUrl: string
    }

type Context =
    { executor: Executor.Context
    ; config: Config
    }


type private Twitter =
    { id: string
    ; handle: string
    ; fullName: string
    }

type private Statistics =
    { retweets: int
    ; replies: int
    ; favorites: int
    }

type private Tweet =
    { id: string
    ; by: Twitter
    ; at: DateTime
    ; url: string
    ; stats: Statistics
    ; content: IWebElement
    ; stringified: string
    }


module private StatisticsParser =
    let selector = ".ProfileTweet-action"

    let statisticsZero =
        { replies = 0
        ; retweets = 0
        ; favorites = 0
        }

    let from (elements: IWebElement list) =

        let set (stats: Statistics) (action: IWebElement) =
            let actionIs name = action |> Element.hasClass name
            let count =
                let el = action |> elementWithin ".ProfileTweet-actionCount"
                match el.Text |> String.IsNullOrWhiteSpace with
                | false -> int el.Text
                | true -> 0

            if not <| actionIs "ProfileTweet-action"
                then failwithf "Element does not have expected class."

            else if actionIs "ProfileTweet-action--reply"
                then { stats with replies = count }

            else if actionIs "ProfileTweet-action--retweet"
                then { stats with retweets = count }

            else if actionIs "ProfileTweet-action--favorite"
                then { stats with favorites = count }

            else
                Element.attrRaw "class" action
                |> failwithf "Unknown action with class: '%s'."

        elements |> Seq.fold set statisticsZero


module private TweetParser =
    let selector = "[data-item-type='tweet']"

    let from (element: IWebElement) =

        let getBy cssSelector =
            elementWithin cssSelector element

        let attrBy cssSelector name =
            getBy cssSelector
            |> Element.attr name

        let attr name =
            attrBy ".tweet" name

        let twitter =
            { id = attr "data-user-id"
            ; handle = "@" + (attr "data-screen-name")
            ; fullName = attr "data-name"
            }

        let tweetedAt =
            let timestampSelector = ".stream-item-header .tweet-timestamp span"
            let time = attrBy timestampSelector "data-time" |> float
            DateTime.UnixEpoch.AddSeconds(time)

        let tweetLink =
            "https://twitter.com" + (attr "data-permalink-path")

        let statistics =
            let footer = getBy ".stream-item-footer "
            elementsWithin StatisticsParser.selector footer
            |> StatisticsParser.from

        let contentElement =
            getBy ".js-tweet-text-container"

        { id = attr "data-tweet-id"
        ; by = twitter
        ; at = tweetedAt
        ; url = tweetLink
        ; stats = statistics
        ; content = contentElement
        ; stringified = element.Text
        }


type TweetData =
    { tweetUrl: string
    ; twitter: string
    ; timestamp: DateTime
    ; content: string
    ; replies: int
    ; retweets: int
    ; favorites: int
    }

type TweetItem =
    { tweetData: TweetData
    ; gatheredAt: DateTime
    ; selecting: TimeSpan
    ; parsing: TimeSpan
    }

module TweetItem =

    let dataOf (item: TweetItem) =
        item.tweetData

    let serializeAs (description: string) (context: Context) (tweets: TweetItem list) =
        let infoLines =
            let twitter = context.config.twitterDisplayName
            let page = context.config.twitterPageUrl
            [ "Tweets gathered with FSharp-SandBox SeleniumViaCanopy idea project"
            ; sprintf "From user: %s through %s" twitter page
            ; sprintf "Loaded at: %s" (Time.asStamp' Time.Now)
            ]

        let tweetsResult =
            tweets |> List.map (fun tweet ->
                {| tweet = tweet.tweetData
                ;  selectingTime = tweet.selecting.Ticks
                ;  parsingTime = tweet.parsing.Ticks
                |}
            )

        let serializedLines =
            {| _info = infoLines
            ;  tweets = tweetsResult
            |}
            |> Json.serialize
            |> Array.singleton

        let filename = sprintf "tweets %s.json" description
        let storage = context.executor.storage
        let identity = context.config.executionIdentity

        storage.SaveFileAs identity filename serializedLines


let tweetsUntil (tweetsLimit: DateTime) (context: Context) =

    let twitterDisplayName =
        context.config.twitterDisplayName

    let twitterPageUrl =
        context.config.twitterPageUrl

    let log message =
        context.executor.log.Info message

    let screenshotAs description =
        let storage = context.executor.storage
        let identity = context.config.executionIdentity
        storage.ScreenshotAs identity description

    let waitForNextStep () =
        // sleep 1
        ()


    let ensureExpectedUserPage() =
        log "Ensuring opened page is expected."

        let headerSelector = ".ProfileHeaderCard-nameLink"
        highlight headerSelector

        let headers = elements headerSelector
        (List.length headers, headerSelector)
        ||> sprintf "Found %d elements matching selector: '%s'."
        |> log

        read headerSelector
        |> sprintf "Text content of the first one: '%s'."
        |> log

        screenshotAs "expected Twitter profile"
        headerSelector == twitterDisplayName

        waitForNextStep()


    let ensureExpectedTweetsTab() =
        log "Ensuring activated tab is expected."

        let tabSelector = "[data-element-term=tweets_toggle].is-active"
        highlight tabSelector

        screenshotAs "expected Tweets tab"
        displayed tabSelector

        let textSelector = "[aria-hidden=true]"
        (element tabSelector |> elementWithin textSelector).Text
        |> sprintf "Expected tab is active displaying: '%s'."
        |> log

        waitForNextStep()


    let openTwitterPage() =
        log <| sprintf "Opening page: '%s'..." twitterPageUrl

        url twitterPageUrl
        screenshotAs "page opened"
        waitForNextStep()

        ensureExpectedUserPage()
        ensureExpectedTweetsTab()

        log "Opened at valid and expected Twitter page."
        waitForNextStep()


    let getTweetsUntilLimit() =
        let tweetSelector = TweetParser.selector + ":not(.js-pinned)"

        let prefixIter i =
            i + 1 |> sprintf "%05d"

        let selectTweetAt index =
            try
                // Note: Selecting via `nth` keeps getting slower
                //       for older (higher i's) Tweets.
                // Pleas be patient...
                let element = nth index tweetSelector
                WebTools.highlightIt element
                element |> Some

            with ex ->
                [ sprintf "%s. Got exception when selecting tweet:" <| prefixIter index
                ; ex.ToString()
                ; "No more tweets will be loaded."
                ] |> Seq.iter log
                None

        let parseTweetAt index element =
            (prefixIter index, element)
            ||> sprintf "%s. Parsing as Tweet: %A..."
            |> log

            TweetParser.from element

        let identity (tweet: Tweet) =
            Time.asStamp' tweet.at
            |> sprintf "%s at: %s" tweet.id

        let getTweetAt index =
            let (selected, selecting) =
                Time.watchThat <| fun () ->
                    selectTweetAt index

            match selected with
            | None -> None
            | Some element ->
                let (tweet, parsing) =
                    Time.watchThat <| fun () ->
                        parseTweetAt index element

                (identity tweet, Time.sec00 selecting, Time.sec00 parsing)
                |||> sprintf "Got tweet %s - selected: %s, parsed: %s."
                |> log

                let tweetItem =
                    { tweetData =
                        { tweetUrl = tweet.url
                        ; twitter = tweet.by.handle
                        ; timestamp = tweet.at
                        ; content = tweet.content.Text
                        ; replies = tweet.stats.replies
                        ; retweets = tweet.stats.retweets
                        ; favorites = tweet.stats.favorites
                        }
                    ; gatheredAt = Time.Now
                    ; selecting = selecting
                    ; parsing = parsing
                    }

                waitForNextStep()
                tweetItem |> Some

        let stillFresh =
            function
            | None -> false
            | Some (tweet: TweetItem) ->
                tweet.tweetData.timestamp > tweetsLimit

        Time.asStamp' tweetsLimit
        |> sprintf "Looking for Tweets not older than: %s."
        |> log

        Seq.initInfinite getTweetAt
        |> Seq.takeWhile stillFresh
        |> Seq.choose id // <| fun x -> x


    let gatherTweets() =
        let batchSize = 33

        let tweetsBatches =
            getTweetsUntilLimit()
            |> Seq.chunkBySize batchSize

        let serializeAs (description: string) (tweets: TweetItem list) =
            log <| sprintf "Serializing %d tweets..." tweets.Length
            tweets |> TweetItem.serializeAs description context
            tweets

        let serializeBatch i batch =
            let batch = List.ofArray batch
            match i = 0 && batch.Length < batchSize with
            | false ->
                let n = sprintf "b.%03d" (i + 1)
                serializeAs n batch
            | true ->
                batch

        let (tweetsItems, gathering) =
            Time.watchThat <| fun () ->
                tweetsBatches
                |> Seq.mapi serializeBatch
                |> Seq.concat
                |> List.ofSeq

        let (tweets, serializing) =
            Time.watchThat <| fun () ->
                tweetsItems
                |> serializeAs "- all"
                |> List.map TweetItem.dataOf

        (List.length tweets, Time.minSec00 gathering, Time.sec00 serializing)
        |||> sprintf "Gathered %d tweets, within: %s, serialized: %s."
        |> log

        tweets


    fun _ ->
        // Note: Since some time, Twitter is A/B testing a new styles system.
        //       It does not use "human-readable" CSS' classes, thus this module
        //       will not work sometimes or at all in a near future :(
        openTwitterPage()
        gatherTweets()
        |> Seq.iter (fun tweet ->
            sprintf "Got tweet: %A" tweet
            |> log

            waitForNextStep()
        )


let tweetsLastDays (lastDays: int) (context: Context) =
    let limit =
        TimeSpan.FromDays(float lastDays)
        |> DateTime.UtcNow.Subtract

    tweetsUntil limit context
