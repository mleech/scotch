namespace Scotch

open System
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Scotch

type RecordingHandler(filePath:string, innerHandler:HttpMessageHandler) =
    inherit DelegatingHandler(innerHandler)

    let mutable interactions = []
    let mutable tasks = []

    override handler.SendAsync (request:HttpRequestMessage, cancellationToken:Threading.CancellationToken) =
        let baseResult = base.SendAsync(request, cancellationToken)
        let workflow = async {
            let! response = baseResult |> Async.AwaitTask
            let! interactionRequest = convertRequestAsync request
            let! interactionResponse = convertResponseAsync response
            let httpInteraction =
                {Request = interactionRequest
                 Response = interactionResponse
                 RecordedAt = DateTimeOffset.Now}
            interactions <- List.Cons(httpInteraction, interactions)
            return response
        }

        let task = Async.StartAsTask workflow
        tasks <- List.Cons(task, tasks)
        task

    override handler.Dispose (disposing:bool) =
        if disposing then
            Task.WaitAll [| for t in tasks -> t :> Task |]
            persistCassetteToFile (filePath, List.rev interactions)

        base.Dispose(disposing)

