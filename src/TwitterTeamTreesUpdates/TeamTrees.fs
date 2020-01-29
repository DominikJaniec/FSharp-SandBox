namespace TwitterTeamTreesUpdates

open System
open System.Text.RegularExpressions


type TeamTreesUpdate =
    { timestamp: DateTime
    ; count: int
    ; delta: int
    ; avg: int option
    ; target: decimal option
    ; goalEta: string option
    } with
        static member Zero =
            { timestamp = DateTime.MinValue
            ; count = 0
            ; delta = 0
            ; avg = None
            ; target = None
            ; goalEta = None
            }


module TeamTreesParser =

    type private MatchedGroup =
        Match * int

    let private parseAs parser (matchedGroup: MatchedGroup) =
        // Note: Group at 0-index represents whole regex match.
        let (sourceMatch, groupPosition) = matchedGroup
        sourceMatch.Groups.[groupPosition + 1].Value
        |> parser

    let private asJust =
        parseAs id

    let private asStr =
        parseAs (fun v -> v.Trim())

    let private asInt matchedGroup =
        (asStr matchedGroup).Replace(",", String.Empty)
        |> int

    let private asDec matchedGroup =
        (asStr matchedGroup).Trim('%')
        |> decimal

    let private matchLike pattern action =
        fun input ->
            let rules = RegexOptions.IgnoreCase ||| RegexOptions.CultureInvariant
            let result = Regex.Match(input, pattern, rules)
            match result.Success with
            | true -> action result |> Some
            | false -> None


    type private MatchSetter = TeamTreesUpdate -> TeamTreesUpdate
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

                    let state = TeamTreesUpdate.Zero
                    matchSetters
                    |> Seq.fold applyAt state
                    |> Some

    let private matchersSimple : LineMatcher list =
        (* Example Tweet:
            Current #TeamTrees count: 16,364,940!
            Up by 1284 in the last hour|tweet.
        *)

        [ matchLike @"#TeamTrees count: ([0-9,]+)!?"
            <| fun result event ->
                let count = (result, 0) |> asInt
                { event with count = count }

        ; matchLike @"up by (\d+)"
            <| fun result event ->
                let delta = (result, 0) |> asInt
                { event with delta = delta }
        ]

    let private matchersExtended : LineMatcher list =
        (* Example Tweet:
            Current #TeamTrees count: 20,342,361!
            Up by 3694 in the last hour.
            Hourly average is NaN. Expected to reach goal in -13 days!
            100.17% of the way there!
        *)

        let asNoneOr parser matchedGroup =
            match matchedGroup |> asJust with
            | "NaN" -> None
            | _ ->
                matchedGroup
                |> parser
                |> Some

        List.append matchersSimple
            [ matchLike @"average is (NaN|\d+)\..+ goal in (NaN|\S+)"
                <| fun result event ->
                    let avg = (result, 0) |> asNoneOr asInt
                    let goalEta = (result, 1) |> asNoneOr asStr
                    { event with avg = avg; goalEta = goalEta }

            ; matchLike @"(\d+(\.\d+)?%) of the way"
                <| fun result event ->
                    let target = (result, 0) |> asDec
                    { event with target = Some target }
            ]


    let from (tweet: Tweet) =

        let notEmptyLine line =
            not <| String.IsNullOrWhiteSpace(line)

        let lines =
            tweet.contentLines
            |> List.where notEmptyLine

        let matched =
            [ matchersSimple; matchersExtended ]
            |> Seq.map makeTweetMatcher
            |> Seq.tryPick (fun matcher -> matcher lines)

        match matched with
        | None -> Error "Unknown format!"
        | Some treesEvent ->
            let stamp = tweet.timestamp
            { treesEvent with timestamp = stamp }
            |> Ok
