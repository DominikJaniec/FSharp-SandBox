module Secrets =
    type Entry =
        { file: string
        ; pwd: string
        }

    let ``.pfx`` =
        [ { file = "domins.pfx"; pwd = "minik" }
        ]
