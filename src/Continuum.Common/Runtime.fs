namespace Continuum.Common

open System.IO
open System.Reflection


module Runtime =

    /// <summary>
    /// DotNet CLI runs requested project within current directory of its call. Thus, if one wants to
    /// have easy access for files collocated with build artifacts, we need set that directory explicitly.
    /// </summary>
    let fixCurrentDirectory () =
        let callingSource =
            Assembly.GetCallingAssembly()

        callingSource.Location
        |> Path.GetDirectoryName
        |> Directory.SetCurrentDirectory
