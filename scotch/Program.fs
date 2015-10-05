open System.IO
open Nessos.FsPickler.Json
open Scotch

[<EntryPoint>]
let main argv =
    let interaction = Scotch.getHttpInteractionAsync "http://jsonplaceholder.typicode.com/posts/1" |> Async.RunSynchronously
    let jsonSerializer = FsPickler.CreateJsonSerializer(indent = true, omitHeader = true)
    let serializedInteraction = jsonSerializer.Pickle interaction
    File.WriteAllBytes ("C:/Users/Martin/dev/testing123.txt", serializedInteraction)
    0