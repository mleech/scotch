namespace Scotch

open System
open System.Net.Http
open Scotch.Helpers

type ReplayingHandler(innerHandler:HttpMessageHandler, cassettePath:string) =
    inherit DelegatingHandler(innerHandler)

    new(cassettePath:string) = new ReplayingHandler(new HttpClientHandler(), cassettePath)

    override __.SendAsync (request:HttpRequestMessage, cancellationToken:Threading.CancellationToken) =

        let readCassetteWorkflow = async {
            let interactions = Cassette.ReadCassette cassettePath
            let! receivedRequest = toRequestAsync request

            // TODO: Handle request not found
            let matchedInteraction = Seq.find (fun i -> requestsMatch receivedRequest i.Request) interactions
            let matchedResponse = matchedInteraction.Response
            let responseMessage = matchedResponse.ToHttpResponseMessage()
            return responseMessage
        }

        Async.StartAsTask readCassetteWorkflow
