namespace TwitterTeamTreesUpdates

open System
open System.IO


type private TweetRaw = { json: string } with
    static member Sample = """
{
    "_info": [
        "Some information lines",
        "as it is an example of serialized"
        "tweets by SeleniumViaCanopy idea project"
    ],
    "tweets": [
        {
            "parsingTime": 4057342,
            "selectingTime": 298053,
            "tweet": {
                "tweetUrl": "https://twitter.com/{userHandle}/status/{tweetId}",
                "twitter": "@{userHandle}",
                "timestamp": "2020-01-26T17:55:55.0000000Z",
                "content": " Very long\r\n tweet with \r\n \r\n some lines...",
                "replies": 3,
                "retweets": 5,
                "favorites": 7
            }
        }
    ]
}
    """

type Tweet =
    { url: string
    ; twitter: string
    ; timestamp: DateTime
    ; contentLines: string list
    }


module Tweet =

    let loadFrom (file: string) : Tweet list =
        let size = FileInfo(file).Length

        failwithf "TODO! loaded '%s' file of %i bytes." file size
