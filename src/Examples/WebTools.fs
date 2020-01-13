namespace Continuum.Gatherer.Selenium

open OpenQA.Selenium
open canopy.classic


module WebTools =

    let execute (script: string) (args: obj list) =
        let executor = browser :?> IJavaScriptExecutor
        let args = args |> Array.ofList
        executor.ExecuteScript(script, args)


    let scrollIntoView (element: IWebElement) =
        // alignToTop:true => scrollIntoViewOptions: {block: "start", inline: "nearest"}
        // alignToTop:false => scrollIntoViewOptions: {block: "end", inline: "nearest"}
        execute "arguments[0].scrollIntoView(false);" [ element ]
        |> ignore


    let highlightIt (element: IWebElement) =
        let id = "#" + element.GetAttribute("id")
        // TODO: implement other finders as first-match

        scrollIntoView element
        highlight id
