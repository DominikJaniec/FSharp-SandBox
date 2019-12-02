namespace Continuum.Gatherer


type GatheringError =
    | UnknownError of string * exn option

type IGathering<'T> =
    interface end

module Gatherer =

    let prepare<'T> (config: obj)
        : IGathering<'T> =
        failwithf "Not implemented for: %A" obj

    let execute<'T> (gathering: IGathering<'T>)
        : Result<'T, GatheringError> =
        failwithf "Not implemented for: %A" gathering
