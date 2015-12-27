namespace Scotch

open Fleece
open System
open System.IO
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Scotch.Helpers

type ReplayingHandler(innerHandler:HttpMessageHandler, cassettePath:string) =
    inherit DelegatingHandler(innerHandler)

    new(cassettePath:string) = new ReplayingHandler(new HttpClientHandler(), cassettePath)

    override handler.SendAsync (request:HttpRequestMessage, cancellationToken:Threading.CancellationToken) =
        let interactions = Cassette.ReadCassette cassettePath

        let readCassetteWorkflow = async {
            let! receivedRequest = toRequestAsync request

            // TODO: Handle request not found
            let matchedInteraction = Seq.find (fun i -> requestsMatch receivedRequest i.Request) interactions
            let matchedResponse = matchedInteraction.Response
            let responseMessage = matchedResponse.ToHttpResponseMessage()
            return responseMessage
        }

        Async.StartAsTask readCassetteWorkflow
