namespace Scotch

open Fleece
open System
open System.IO
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Scotch

type ReplayingHandler(innerHandler:HttpMessageHandler, cassettePath:string) =
    inherit DelegatingHandler(innerHandler)

    let jsonString = File.ReadAllText(cassettePath)
    let cassetteParseResult = parseJSON jsonString

    override handler.SendAsync (request:HttpRequestMessage, cancellationToken:Threading.CancellationToken) =
        let interactions =
            match cassetteParseResult with
            | Success x -> x
            | Failure y -> failwith (sprintf "Error parsing the cassette file, Error: %A" y)

        let requestsMatch receivedRequest recordedRequest =
            receivedRequest.Method.Equals(recordedRequest.Method, StringComparison.InvariantCultureIgnoreCase)
            && receivedRequest.URI.Equals(recordedRequest.URI, StringComparison.InvariantCultureIgnoreCase)

        let workflow = async {
            let! receivedRequest = Scotch.toRequestAsync request

            // TODO: Handle request not found
            let matchedInteraction = List.find (fun i -> requestsMatch receivedRequest i.Request) interactions
            let matchedResponse = matchedInteraction.Response
            let responseMessage = Scotch.toHttpResponseMessage matchedResponse
            return responseMessage
        }

        Async.StartAsTask workflow
