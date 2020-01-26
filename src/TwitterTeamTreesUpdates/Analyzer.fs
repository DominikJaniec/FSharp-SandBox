namespace TwitterTeamTreesUpdates


module Analyzer =

    let tweetsSourceFile = "Tweets-TeamTrees-Updates.json"

    let analyzeTeamTrees (tweetsSource: string) =
        let tweets = Tweet.loadFrom tweetsSource

        failwithf "Let's analyze %i tweets!" tweets.Length
        ()
