namespace TwitterTeamTreesUpdates

open Continuum.Common


module UpdatesLoader =

    let tweetsSourceFile = "Tweets-TeamTrees-Updates.json"

    type private ParseFold =
        { parsed: TeamTreesUpdate list
        ; errors: (string * Tweet) list
        } with
            static member Zero =
                { parsed = []
                ; errors = []
                }

    let private asTeamTrees (state: ParseFold) tweet =
        match TeamTreesParser.from tweet with
        | Ok update ->
            { state with parsed = update :: state.parsed }

        | Error msg ->
            let result = (msg, tweet)
            { state with errors = result :: state.errors }

    let private showResults (state: ParseFold) =
        let splitterLine() =
            String.replicate 69 "-"
            |> printfn "%s"

        let describedTweets (tweets: (string * Tweet) list) =
            tweets |> List.iteri (fun i (msg, tweet) ->
                let twitter = tweet.twitter
                let stamp = Time.asStamp' tweet.timestamp
                printfn "%4i. %s | %s %s" (i + 1) msg stamp twitter
                printfn "\t: %s" tweet.url
                tweet.contentLines
                |> List.iter (printfn "\t| %s")
            )

        splitterLine()
        printf "Successfully parsed %i Tweets" state.parsed.Length
        match state.errors.Length with
        | 0 -> printfn ", and no errors!"
        | length ->
            printfn ", but got %i errors:" length
            state.errors |> describedTweets
            splitterLine()

    let private toTeamTreesUpdates (tweets: Tweet list) =
        printfn "Parsing %i Tweets as TeamTrees' Updates..." tweets.Length

        let result =
            List.fold asTeamTrees
                ParseFold.Zero
                tweets

        showResults result
        result.parsed


    let loadUpdates (tweetsSource: string) =
        Tweet.loadFrom tweetsSource
        |> toTeamTreesUpdates
