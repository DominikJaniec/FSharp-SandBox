namespace TwitterTeamTreesUpdates

open System
open System.IO
open FSharp.Data


type private TweetRaw = JsonProvider<"""
{
    "_info": [
        "Some information lines",
        "as it is an example of serialized",
        "tweets by SeleniumViaCanopy idea project"
    ],
    "tweets": [
        {
            "parsingTime": 4057342,
            "selectingTime": 298053,
            "tweet": {
                "tweetUrl": "https://twitter.com/{userHandle}/status/{tweetId}",
                "twitter": "@{userHandle}",
                "timestamp": "2020-01-26T12:34:56.0000000Z",
                "content": " Very long\r\n tweet with \r\n \r\n some lines...",
                "replies": 3,
                "retweets": 5,
                "favorites": 7
            }
        }
    ]
}
    """>

type Tweet =
    { url: string
    ; twitter: string
    ; timestamp: DateTime
    ; contentLines: string list
    }


module Tweet =

    let private fromRaw (tweetRaw: TweetRaw.Tweet) =
        let asLines (value: string) =
            value.Split('\n')
            |> Array.map (fun l -> l.Trim())
            |> List.ofArray

        let tweet = tweetRaw.Tweet
        { url = tweet.TweetUrl
        ; twitter = tweet.Twitter
        ; timestamp = tweet.Timestamp.DateTime
        ; contentLines = tweet.Content |> asLines
        }

    let private ensureCorrectlyLoaded (root: TweetRaw.Root) =
        match (root.Info, root.Tweets) with
        | ([||], [||]) -> failwith "Looks like no data was parsed / loaded :("
        | _ -> ()

    let private describeLoaded (root: TweetRaw.Root) =
        root.Tweets.Length |> printfn "Loaded %i Tweets described as:"
        root.Info |> Array.iter (fun li -> printfn "  * %s" li)

    let private parseTweets (json: string) =
        let data = TweetRaw.Parse json
        ensureCorrectlyLoaded data
        describeLoaded data
        data.Tweets


    let loadFrom (file: string) =
        let filePath = Path.GetFullPath file
        let content = File.ReadAllText filePath
        printfn "Loading %i bytes form '%s' as Tweets' JSON..."
            (content.Length) filePath

        parseTweets content
        |> Seq.map fromRaw
        |> List.ofSeq
