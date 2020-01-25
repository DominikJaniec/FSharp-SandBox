namespace SeleniumViaCanopy.Tools

open OpenQA.Selenium
open canopy.classic


module WebTools =

    let execute (script: string) (args: obj list) =
        let executor = browser :?> IJavaScriptExecutor
        let args = args |> Array.ofList
        executor.ExecuteScript(script, args)


    let scrollIntoView (element: IWebElement) =
        let script =
            // "true" // or => "{ block: 'start', inline: 'nearest' }"
            // "false" // or => "{ block: 'end', inline: 'nearest' }"
            "{ block: 'center', inline: 'nearest' }"
            |> sprintf "arguments[0].scrollIntoView(%s);"

        execute script [ element ]
        |> ignore


    let highlightIt (element: IWebElement) =
        let id = "#" + element.GetAttribute("id")
        // TODO: implement other finders as first-match

        scrollIntoView element
        highlight id
