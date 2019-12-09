namespace Continuum.Gatherer.Core

open System


module Tools =

    type TimestampConfig =
        { separator: char
        ; skipLast: uint32
        }

    let asTimestampWith (config: TimestampConfig) (value: DateTime) =

        let fullFormat =
            let date = "yyyy-MM-dd"
            let time = "HH.mm.ss.fff"
            let sep = Char.ToString(config.separator)
            String.concat "" [ date; sep; time ]

        let format =
            let skip =
                match config.skipLast with
                // Skips: yyyy-MM-dd HH.mm.ss.fff
                | 5u -> 7 //              .ss.fff
                | 3u -> 4 //                 .fff
                // TODO: Skip other tails
                | _ -> 0

            let length = (String.length fullFormat) - skip
            fullFormat.Substring(0, length)

        value.ToString(format)


    let asTimestamp (value: DateTime) =
        asTimestampWith { separator = '+'; skipLast = 0u } value

    let asTimestamp' (value: DateTime) =
        asTimestampWith { separator = ' '; skipLast = 5u } value
