open System.IO
open Scotch
open Fleece

[<EntryPoint>]
let main argv =
    let interaction = Scotch.getHttpInteractionAsync "http://jsonplaceholder.typicode.com/posts/1" |> Async.RunSynchronously
    let serializedInteraction = toJSON interaction
    File.WriteAllText ("C:/Users/Martin/dev/testing123.txt", serializedInteraction.ToString())
    0