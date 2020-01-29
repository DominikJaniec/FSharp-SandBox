namespace TwitterTeamTreesUpdates

open System
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

        let headers =
            [ "  Tweeted at Timestamp | Trees Count |      Delta |"
            ; "-----------------------+-------------+------------+"
            ]

        let asRow i (event: Event) =
            sprintf "%4i. %s | %11i | +%9i |" (i + 1)
                (Time.asStamp' event.timestamp)
                event.treesCount
                event.delta

        let from (updates: TeamTreesUpdate list) =
            updates
            |> Seq.scan resolve Event.Zero
            |> Seq.skip 1
            |> List.ofSeq


    let analyze (updates: TeamTreesUpdate list) =
        let printLines (lines: string seq) =
            lines |> Seq.iter (printfn "%s")

        let events = Event.from updates

        printLines Event.headers
        Seq.take 421 events
        |> Seq.mapi Event.asRow
        |> printLines

        failwith "Let's analyze those events!"
        ()
