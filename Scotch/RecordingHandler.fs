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

    new(cassettePath:string) = new RecordingHandler(new HttpClientHandler(), cassettePath)

    override handler.SendAsync (request:HttpRequestMessage, cancellationToken:Threading.CancellationToken) =

        let baseResult = base.SendAsync(request, cancellationToken)

        let updateCassetteWorkflow = async {
            let! response = baseResult |> Async.AwaitTask

            let! interactionRequest = toRequestAsync request
            let! interactionResponse = toResponseAsync response
            let httpInteraction =
                {Request = interactionRequest
                 Response = interactionResponse
                 RecordedAt = DateTimeOffset.Now}

            Cassette.UpdateInteraction cassettePath httpInteraction

            return response
        }

        Async.StartAsTask updateCassetteWorkflow
