namespace Continuum.Gatherer.Core

open System


module Tools =

    type TimestampConfig =
        { dateFormat: string
        ; timeFormat: string
        ; separator: string
        ; skipLast: uint32
        ; skip: uint32 -> string -> string
        }

    let defaultTimestamp =
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


    let asTimestampWith (config: TimestampConfig) (value: DateTime) =
        let timestampFormat =
            config.dateFormat + config.separator + config.timeFormat

        value.ToString(timestampFormat)
        |> config.skip config.skipLast

    let asTimestamp (value: DateTime) =
        asTimestampWith defaultTimestamp value

    let asTimestamp' (value: DateTime) =
        asTimestampWith
            { defaultTimestamp
                with separator = " "
                    ; skipLast = 5u
            }
            value
