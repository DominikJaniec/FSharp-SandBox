namespace TwitterTeamTreesUpdates


module Analyzer =

    let tweetsSourceFile = "Tweets-TeamTrees-Updates.json"

    let private toTeamTreesUpdates (tweets: Tweet list) =
        let parse tweet =
            match TeamTreesParser.from tweet with
            | Ok update -> printf "."; update
            | Error msg ->
                let header = [ "" ; sprintf "Got error '%s' while parsing Tweet:" msg ]
                let content = tweet.contentLines |> List.map (fun l -> "  | " + l)
                let message = List.concat [ header; content ]
                message |> List.iter (printfn "%s")
                failwith "Implement more formats!"

        tweets.Length
        |> printf "Parsing %i Tweets as TeamTrees' Updates.."

        tweets |> List.map parse

    let analyzeTeamTrees (tweetsSource: string) =
        let tweets =
            Tweet.loadFrom tweetsSource

        tweets |> toTeamTreesUpdates |> ignore
        failwith "Let's analyze those events!"
        ()
