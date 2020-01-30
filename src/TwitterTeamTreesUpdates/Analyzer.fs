namespace TwitterTeamTreesUpdates

open System
open XPlot.Plotly
open Continuum.Common


module Analyzer =

    type private Event =
        { timestamp: DateTime
        ; treesCount: int
        ; delta: int
        } with
            static member Zero =
                { timestamp = DateTime.MinValue
                ; treesCount = 0
                ; delta = 0
                }

    module private Event =

        let private resolve (prev: Event) (next: TeamTreesUpdate) =
            let delta = next.count - prev.treesCount
            { timestamp = next.timestamp
            ; treesCount = next.count
            ; delta = delta
            }

        let private skipTooFrequent updates =
            let frequency = TimeSpan.FromMinutes 45.
            let tooFrequent (cur: TeamTreesUpdate, next: TeamTreesUpdate) =
                next.timestamp.Subtract(cur.timestamp) < frequency

            updates
            |> Seq.pairwise
            |> Seq.where (not << tooFrequent)
            |> Seq.map fst

        let headers =
            [ "+-----------------------+-------------+------------+"
            ; "|  Tweeted at Timestamp | Trees Count |      Delta |"
            ; "+-----------------------+-------------+------------+"
            ]

        let asRow i (event: Event) =
            sprintf "| %3i. %s | %11i | +%9i |" (i + 1)
                (Time.asStamp' event.timestamp)
                event.treesCount
                event.delta

        let from (updates: TeamTreesUpdate list) =
            // 1st: Insignificant `Event.Zero`
            // 2nd: Has got false `Delta`
            let skipCount = 2

            updates
            |> skipTooFrequent
            |> Seq.scan resolve Event.Zero
            |> Seq.skip skipCount
            |> List.ofSeq


    module private Plotter =

        type private Series =
            { stamps: DateTime list
            ; counts: int list
            ; deltas: int list
            } with
                static member Zero =
                    { stamps = []
                    ; counts = []
                    ; deltas = []
                    }

        let private seriesOf (events: Event list) =
            let folder (event: Event) (series: Series) =
                { series with
                    stamps = event.timestamp :: series.stamps
                    counts = event.treesCount :: series.counts
                    deltas = event.delta:: series.deltas
                }

            Seq.foldBack folder events Series.Zero

        let private goalSeriesFor (events: Event list) =
            List.replicate (events.Length)
                (20 * 1000 * 1000)

        let private titles =
            {| graph = "Progress of #TeamTrees"
            ;  count = "Total trees count"
            ;  delta = "Hourly increase"
            ;  goal = "Goal 20M trees"
            |}

        let plot (events: Event list) =
            let series = seriesOf events
            let goal = goalSeriesFor events

            let layout =
                Layout
                    ( title = titles.graph
                    , showlegend = true
                    , legend = Legend
                        ( orientation = "h"
                        )
                    , yaxis = Yaxis
                        ( title = titles.count
                        )
                    , yaxis2 = Yaxis
                        ( title = titles.delta
                        , overlaying = "y"
                        , side = "right"
                        , range = [ 0; 24 * 1000 ]
                        )
                    )

            let deltas =
                Scatter
                    ( name = titles.delta
                    , x = series.stamps
                    , y = series.deltas
                    , yaxis = "y2"
                    , fill = "tozeroy" // fill down to xaxis
                    , fillcolor = "greenyellow"
                    , line = Line
                        ( color = "darkgreen"
                        )
                    )
                :> Trace

            let trees =
                Bar
                    ( name = titles.count
                    , x = series.stamps
                    , y = series.counts
                    , marker = Marker
                        ( color = "olive"
                        )
                    )
                :> Trace

            let goal =
                Scatter
                    ( name = titles.goal
                    , x = series.stamps
                    , y = goal
                    , line = Line
                        ( color = "gold"
                        , width = 4
                        )
                    )
                :> Trace

            [ deltas; trees; goal ]
            |> Chart.Plot
            |> Chart.WithLayout layout
            |> Chart.WithHeight 700
            |> Chart.WithWidth 1500
            |> Chart.Show


    let private printAsTable (events: Event list) =
        let printLines (lines: string seq) =
            lines |> Seq.iter (printfn "%s")

        printfn "Found %i significant events:" events.Length
        printLines Event.headers
        Seq.mapi Event.asRow events
        |> printLines


    let analyze (updates: TeamTreesUpdate list) =
        let events =
            Event.from updates

        events |> printAsTable
        events |> Plotter.plot
