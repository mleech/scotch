namespace Scotch

open System
open System.Net.Http
open Scotch.Helpers

type RecordingHandler(innerHandler:HttpMessageHandler, cassettePath:string) =
    inherit DelegatingHandler(innerHandler)

    new(cassettePath:string) = new RecordingHandler(new HttpClientHandler(), cassettePath)

    override __.SendAsync (request:HttpRequestMessage, cancellationToken:Threading.CancellationToken) =

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
