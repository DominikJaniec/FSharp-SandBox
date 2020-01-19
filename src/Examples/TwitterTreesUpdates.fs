module TwitterTreesUpdates

open System
open System.Text.RegularExpressions
open FSharp.Json
open OpenQA.Selenium
open canopy.classic
open Continuum.Gatherer.Core
open Continuum.Gatherer.Selenium


let private executionIdentity = "TwitterTreesUpdates"

let private tweetsPage = "https://twitter.com/TreesUpdates"
let private tweeterUserName = "TeamTrees Updates"


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


type TweetContent =
    { timestamp: DateTime
    ; tweetUrl: string
    ; twitter: string
    ; content: string
    }

module private TweetContent =
    let from (tweet: Tweet) =
        { timestamp = tweet.at
        ; tweetUrl = tweet.url
        ; twitter = tweet.by.handle
        ; content = tweet.content.Text
        }

    let serialize (storage: Executor.IStorage) (tweets: TweetContent list) =
        let serialized = Json.serialize tweets

        ("tweets.json", Array.singleton serialized)
        ||> storage.SaveFileAs executionIdentity

    let deserialize (storage: Executor.IStorage) (filename: string) =
        storage.LoadFileAs executionIdentity filename
        |> Json.deserialize<TweetContent list>


type private TreesExtra =
    { avg: int
    ; target: decimal
    ; goalEta: string
    } with
        static member Zero =
            { avg = 0
            ; target = 0m
            ; goalEta = "unknown"
            }

type private TreesEvent =
    { count: int
    ; delta: int
    ; timestamp: DateTime
    ; extra: TreesExtra option
    } with
        static member Zero =
            { count = 0
            ; delta = 0
            ; timestamp = DateTime.MinValue
            ; extra = None
            }


module private TreesEventParser =

    let private parseAs parser ((source: Match), (group: int)) =
        // Note: Group at 0-index represents whole regex match.
        source.Groups.[group + 1].Value
        |> parser

    let private asStr =
        parseAs (fun v -> v.Trim())

    let private asInt groupMatch =
        (asStr groupMatch).Replace(",", String.Empty)
        |> int

    let private asDec groupMatch =
        (asStr groupMatch).Trim('%')
        |> decimal


    let private matchLike pattern action =
        fun input ->
            let rules = RegexOptions.IgnoreCase ||| RegexOptions.CultureInvariant
            let result = Regex.Match(input, pattern, rules)
            match result.Success with
            | true -> action result |> Some
            | false -> None


    type private MatchSetter = TreesEvent -> TreesEvent
    type private LineMatcher = string -> MatchSetter option

    let private makeTweetMatcher (lineMatchers: LineMatcher list) =
        fun (tweetLines: string list) ->
            match lineMatchers.Length = tweetLines.Length with
            | false -> None
            | true ->
                let matchSetters =
                    Seq.zip lineMatchers tweetLines
                    |> Seq.map (fun (matcher, line) -> matcher line)
                    |> List.ofSeq

                match matchSetters |> List.forall Option.isSome with
                | false -> None
                | true ->
                    let applyAt event (setter: MatchSetter option)
                        = setter.Value event

                    matchSetters
                    |> Seq.fold applyAt TreesEvent.Zero
                    |> Some


    let private tweetMatcherExtra =
        (* Example Tweet:
            Current #TeamTrees count: 20,342,361!
            Up by 3694 in the last hour.
            Hourly average is 5689. Expected to reach goal in -13 days!
            100.17% of the way there!
        *)

        let setExtraOf event setter =
            let extra = event.extra |> Option.defaultValue TreesExtra.Zero
            { event with extra = Some <| setter extra }

        makeTweetMatcher
            [ matchLike @"count: (\S+?)!"
                <| fun result event ->
                    { event with count = (result, 0) |> asInt }

            ; matchLike @"up by (\d+)"
                <| fun result event ->
                    { event with delta = (result, 0) |> asInt }

            ; matchLike @"average is (\d+).+goal in (\S+)"
                <| fun result event ->
                    let avg = (result, 0) |> asInt
                    let goalEta = (result, 1) |> asStr
                    setExtraOf event
                        <| fun extra ->
                            { extra with avg = avg; goalEta = goalEta }

            ; matchLike @"(\d+\.\d+%) of the way"
                <| fun result event ->
                    let target = (result, 0) |> asDec
                    setExtraOf event
                        <| fun extra ->
                            { extra with target = target }
            ]


    let from (tweet: TweetContent) =

        let notEmptyLine line =
            not <| String.IsNullOrWhiteSpace(line)

        let lines =
            tweet.content.Split('\n')
            |> Seq.map (fun x -> x.Trim())
            |> Seq.where notEmptyLine
            |> List.ofSeq

        let matched =
            [ tweetMatcherExtra ]
            |> Seq.tryPick (fun matcher -> matcher lines)

        match matched with
        | None -> Error ("Unknown format", tweet)
        | Some treesEvent ->
            { treesEvent with timestamp = tweet.timestamp }
            |> Ok


let lastDays (lastDays: int) (context: Executor.Context) =

    let log message =
        context.log.Info message

    let screenshotAs description =
        context.storage.ScreenshotAs executionIdentity description

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
        headerSelector == tweeterUserName

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
        log <| sprintf "Opening page: '%s'..." tweetsPage

        url tweetsPage
        screenshotAs "page opened"
        waitForNextStep()

        ensureExpectedUserPage()
        ensureExpectedTweetsTab()

        log "Opened at valid and expected Twitter page."
        waitForNextStep()


    let getTweetsUntil (pastLimit: DateTime) =
        Tools.asTimestamp' pastLimit
        |> sprintf "Looking for Tweets not older than: %s."
        |> log

        let batchLimit = 1 // TODO: make this limited by `pastLimit`
        let tweetSelector = TweetParser.selector + ":not(.js-pinned)"
        seq {
            for i in 0 .. batchLimit do
                let element = nth i tweetSelector
                WebTools.highlightIt element

                log <| sprintf "Parsing as Tweet: %A..." element
                yield TweetParser.from element
        }


    let gatherTweets() =
        let pastLimit =
            TimeSpan.FromDays(float lastDays)
            |> DateTime.UtcNow.Subtract

        let tweets =
            getTweetsUntil pastLimit
            |> Seq.map TweetContent.from
            |> List.ofSeq

        log <| sprintf "Serializing %d tweets..." tweets.Length
        tweets |> TweetContent.serialize context.storage
        tweets


    let loadTweetsFrom (filename: string) =
        log <| sprintf "Loading and deserializing tweets from '%s' file." filename
        TweetContent.deserialize context.storage filename


    fun _ ->
        openTwitterPage()
        gatherTweets()
        // loadTweetsFrom "tweets.json"
        |> Seq.map TreesEventParser.from
        |> Seq.iter (fun result ->
            log <| sprintf "Got result:\n%A" result

            // TODO: use it somehow!
            waitForNextStep()
        )
