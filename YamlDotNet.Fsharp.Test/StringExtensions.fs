namespace YamlDotNet.Fsharp.Test

open System.Runtime.CompilerServices

[<Extension>]
type StringExtensions() =
    [<Extension>]
    static member NormalizeNewLines(x: string) =
        x.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine)

    [<Extension>]
    static member TrimNewLines(x: string) = x.TrimEnd('\r').TrimEnd('\n')

    [<Extension>]
    static member Clean(x: string) = x.NormalizeNewLines().TrimNewLines()
