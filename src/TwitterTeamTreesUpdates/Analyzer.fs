namespace TwitterTeamTreesUpdates

open System
open Continuum.Common


module Analyzer =

    let tweetsSourceFile = "Tweets-TeamTrees-Updates.json"

    type private ParseFold =
        { parsed: (DateTime * TeamTrees) list
        ; errors: (string * Tweet) list
        } with
            static member Zero =
                { parsed = []
                ; errors = []
                }

    let private asTeamTrees (state: ParseFold) tweet =
        match TeamTreesParser.from tweet with
        | Ok update ->
            let result = (tweet.timestamp, update)
            { state with parsed = result :: state.parsed }

        | Error msg ->
            let result = (msg, tweet)
            { state with errors = result :: state.errors }

    let private toTeamTreesUpdates (tweets: Tweet list) =
        tweets.Length
        |> printfn "Parsing %i Tweets as TeamTrees' Updates..."

        let result =
            tweets |> List.fold asTeamTrees ParseFold.Zero

        (result.parsed.Length, result.errors.Length)
        ||> printfn "Successfully parsed %i Tweets, and got %i errors:"
        result.errors |> List.iteri (fun i (message, tweet) ->
            let tweetStamp = Time.asStamp' tweet.timestamp
            let tweetInfo = sprintf "%s %s" tweetStamp tweet.url
            printfn "%4i. %s | %s" (i + 1) message tweetInfo
            tweet.contentLines |> List.iter (printfn "\t%s")
        )

        result.parsed


    let analyzeTeamTrees (tweetsSource: string) =
        Tweet.loadFrom tweetsSource
        |> toTeamTreesUpdates
        |> ignore

        failwith "Let's analyze those events!"
        ()
