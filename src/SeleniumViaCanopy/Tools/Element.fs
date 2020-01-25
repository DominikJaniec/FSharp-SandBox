namespace SeleniumViaCanopy.Tools

open System
open OpenQA.Selenium


module Element =

    let attrRaw (name: string) (el: IWebElement) =
        el.GetAttribute(name)

    let attr (name: string) (el: IWebElement) =
        let value = el |> attrRaw name
        match String.IsNullOrWhiteSpace(value) with
        | true -> failwithf "Missing value of '%s' attribute of: %A" name el
        | false -> value.Trim()


    let class' (el: IWebElement) =
        el |> attrRaw "class"

    let hasClass className (el: IWebElement) =
        (el |> class').Split(' ')
        |> Seq.contains className
