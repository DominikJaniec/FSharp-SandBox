namespace TwitterTeamTreesUpdates

open System
open System.Text.RegularExpressions


type Extra =
    { avg: int
    ; target: decimal
    ; goalEta: string
    } with
        static member Zero =
            { avg = 0
            ; target = 0m
            ; goalEta = "unknown"
            }

type TeamTrees =
    { count: int
    ; delta: int
    ; timestamp: DateTime
    ; extra: Extra option
    } with
        static member Zero =
            { count = 0
            ; delta = 0
            ; timestamp = DateTime.MinValue
            ; extra = None
            }


module TeamTreesParser =

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


    type private MatchSetter = TeamTrees -> TeamTrees
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
                    |> Seq.fold applyAt TeamTrees.Zero
                    |> Some


    let private tweetMatcherExtra =
        (* Example Tweet:
            Current #TeamTrees count: 20,342,361!
            Up by 3694 in the last hour.
            Hourly average is 5689. Expected to reach goal in -13 days!
            100.17% of the way there!
        *)

        let setExtraOf event setter =
            let extra = event.extra |> Option.defaultValue Extra.Zero
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


    let from (tweet: Tweet) =

        let notEmptyLine line =
            not <| String.IsNullOrWhiteSpace(line)

        let lines =
            tweet.contentLines
            |> List.where notEmptyLine

        let matched =
            [ tweetMatcherExtra ]
            |> Seq.tryPick (fun matcher -> matcher lines)

        match matched with
        | None -> Error "Unknown format"
        | Some treesEvent ->
            { treesEvent with timestamp = tweet.timestamp }
            |> Ok