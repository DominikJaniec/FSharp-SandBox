namespace Continuum.Common

open System
open System.Diagnostics


type Time = class end with
    static member Now =
        DateTime.UtcNow

module Time =

    type StampConfig =
        { dateFormat: string
        ; timeFormat: string
        ; separator: string
        ; skipLast: uint32
        ; skip: uint32 -> string -> string
        } with
            static member Default =
                let skipImpl last formatted =
                    let length = String.length formatted
                    let skip =
                        match last with
                        // Skips: yyyy-MM-dd HH.mm.ss.fff
                        | 5u -> 7 //              .ss.fff
                        | 3u -> 4 //                 .fff
                        // TODO: Skip other tails
                        | _ -> 0

                    formatted.Substring(0, length - skip)

                { dateFormat = "yyyy-MM-dd"
                ; timeFormat = "HH.mm.ss.fff"
                ; separator = "+"
                ; skipLast = 0u
                ; skip = skipImpl
                }

    let asStampWith (config: StampConfig) (value: DateTime) =
        let timestampFormat =
            config.dateFormat
            + config.separator
            + config.timeFormat

        value.ToString(timestampFormat)
        |> config.skip config.skipLast

    let asStamp (value: DateTime) =
        asStampWith StampConfig.Default value

    let asStamp' (value: DateTime) =
        asStampWith
            { StampConfig.Default
                with separator = " "
                    ; skipLast = 5u
            }
            value


    let sec00 (time: TimeSpan) =
        sprintf "%.2f (sec)" time.TotalSeconds

    let minSec00 (time: TimeSpan) =
        let minutes = time.TotalMinutes |> int
        let seconds =
            time.TotalSeconds
            - (time.TotalSeconds |> Math.Floor)
            + (time.Seconds |> float)

        sprintf "%d:%.2f (min:sec)" minutes seconds


    let watchThat factory =
        let watch = Stopwatch.StartNew()
        let result = factory()
        (result, watch.Elapsed)
