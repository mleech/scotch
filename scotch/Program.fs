open System.IO
open Scotch
open Fleece

[<EntryPoint>]
let main argv =
    Scotch.makeHttpCallAsync () |> Async.RunSynchronously
    0