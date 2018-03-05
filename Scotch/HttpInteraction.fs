namespace Scotch

open System
open System.Text
open System.Net
open System.Net.Http
open System.Collections.Generic

type HeaderDictionary = IDictionary<string, string>

type Request =
    { Method : string
      URI : string
      RequestHeaders : HeaderDictionary
      ContentHeaders : HeaderDictionary
      Body : string }

type Status =
    { Code : HttpStatusCode
      Message : string }

type Response =
    { Status : Status
      ResponseHeaders : HeaderDictionary
      ContentHeaders : HeaderDictionary
      Body : string
      HttpVersion : Version } with
    member this.ToHttpResponseMessage () =
        let result = new HttpResponseMessage(this.Status.Code)
        result.ReasonPhrase <- this.Status.Message
        result.Version <- this.HttpVersion
        for h in this.ResponseHeaders do result.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString()) |> ignore
        let content = new ByteArrayContent(Encoding.UTF8.GetBytes(this.Body))
        for h in this.ContentHeaders do content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString()) |> ignore
        result.Content <- content
        result

type HttpInteraction =
    { Request : Request
      Response : Response
      RecordedAt : DateTimeOffset }

module Helpers =
    let toHeaders (headers: Headers.HttpHeaders) =
        headers
        |> Seq.map (fun h -> (h.Key, String.Join(",", h.Value)))
        |> dict

    let toStringAsync (content:HttpContent) =
        match content with
        | null -> async {return ""}
        | _ -> content.ReadAsStringAsync() |> Async.AwaitTask

    let toContentHeaders (content:HttpContent) =
        match content with
        | null -> [||] |> dict
        | _ -> toHeaders content.Headers

    let toRequestAsync (request: HttpRequestMessage) =
        async {
            let! requestBody = toStringAsync request.Content
            return
                {Method = request.Method.ToString()
                 URI = request.RequestUri.ToString()
                 RequestHeaders = toHeaders request.Headers
                 ContentHeaders = toContentHeaders request.Content
                 Body = requestBody
                 }
        }

    let toResponseAsync (response: HttpResponseMessage) =
        async {
            let! responseBody = toStringAsync response.Content
            return
                {Status = {Code = response.StatusCode; Message = response.ReasonPhrase}
                 ResponseHeaders = toHeaders response.Headers
                 ContentHeaders = toContentHeaders response.Content
                 Body = responseBody
                 HttpVersion = response.Version}
        }

    let requestsMatch receivedRequest recordedRequest =
        receivedRequest.Method.Equals(recordedRequest.Method, StringComparison.InvariantCultureIgnoreCase)
        && receivedRequest.URI.Equals(recordedRequest.URI, StringComparison.InvariantCultureIgnoreCase)
