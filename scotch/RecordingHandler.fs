namespace Scotch

open Fleece
open System
open System.IO
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Scotch.Helpers

type RecordingHandler(innerHandler:HttpMessageHandler, cassettePath:string) =
    inherit DelegatingHandler(innerHandler)

    let mutable interactions = []
    let mutable tasks = []

    override handler.SendAsync (request:HttpRequestMessage, cancellationToken:Threading.CancellationToken) =
        let baseResult = base.SendAsync(request, cancellationToken)
        let workflow = async {
            let! response = baseResult |> Async.AwaitTask
            let! interactionRequest = toRequestAsync request
            let! interactionResponse = toResponseAsync response
            let httpInteraction =
                {Request = interactionRequest
                 Response = interactionResponse
                 RecordedAt = DateTimeOffset.Now}
            interactions <- httpInteraction :: interactions
            return response
        }

        let task = Async.StartAsTask workflow
        tasks <- task :: tasks
        task

    override handler.Dispose (disposing:bool) =
        if disposing then
            Task.WaitAll [| for t in tasks -> t :> Task |]
            let serializedInteraction = toJSON (List.rev interactions)
            File.WriteAllText (cassettePath, serializedInteraction.ToString())

        base.Dispose(disposing)

