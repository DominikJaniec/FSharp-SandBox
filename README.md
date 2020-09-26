# FSharp-SandBox

Well, do something funky! Or just try some F# idea :)

## How to Start

One need the [`dotnet` 🔗](https://dotnet.microsoft.com/) available, then just call:

```cli
> dotnet tool restore
> dotnet fake -s build
```

## How to Run

To show what ideas' targets are available, execute:

```cli
> dotnet fake -s build --list
```

To execute chosen idea by its target name:

```cli
> dotnet fake -s build -t <ChosenTargetName>
```

----

## Ideas

* [SeleniumViaCanopy](./src/SeleniumViaCanopy/)
* [TwitterTeamTreesUpdates](./src/TwitterTeamTreesUpdates/)
