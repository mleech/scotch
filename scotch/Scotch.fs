namespace Scotch

module Scotch =

    open Fleece
    open Fleece.Operators
    open System
    open System.IO
    open System.Json
    open System.Net
    open System.Net.Http

    type Header =
        {Key : string
         Value : string}

    type Request =
        {Method : string
         URI : string
         RequestHeaders : Header array
         ContentHeaders : Header array
         Body : string}

    type Status =
        {Code : HttpStatusCode
         Message : string}

    type Response =
        {Status : Status
         ResponseHeaders : Header array
         ContentHeaders : Header array
         Body : string
         HttpVersion : Version}

    type HttpInteraction =
        {Request : Request
         Response : Response
         RecordedAt : DateTimeOffset}

    type Header with
        static member ToJSON (x: Header) =
            jobj [
                x.Key .= x.Value
            ]

    type Request with
        static member ToJSON (x: Request) =
            jobj [
                "method" .= x.Method
                "uri" .= x.URI
                "requestHeaders" .= x.RequestHeaders
                "contentHeaders" .= x.ContentHeaders
                "body" .= x.Body
            ]

    type Status with
        static member ToJSON (x: Status) =
            jobj [
                "code" .= int x.Code
                "message" .= x.Message
            ]

    type Response with
        static member ToJSON (x: Response) =
            jobj [
                "status" .= x.Status
                "responseHeaders" .= x.ResponseHeaders
                "contentHeaders" .= x.ContentHeaders
                "body" .= x.Body
                "httpVersion" .= x.HttpVersion.ToString()
            ]

    type HttpInteraction with
        static member ToJSON (x: HttpInteraction) =
            jobj [
                "request" .= x.Request
                "response" .= x.Response
                "recordedAt" .= x.RecordedAt
            ]

    let convertHeaders (headers: Headers.HttpHeaders) =
        headers
        |> Seq.map (fun h -> {Key = h.Key; Value = String.Join(",", h.Value)})
        |> Seq.toArray

    let getContentAsStringAsync (content:HttpContent) =
        match content with
        | null -> async {return ""}
        | _ -> content.ReadAsStringAsync() |> Async.AwaitTask

    let getContentHeaders (content:HttpContent) =
        match content with
        | null -> [||]
        | _ -> convertHeaders content.Headers

    let convertRequestAsync (request: HttpRequestMessage) =
        async {
            let! requestBody = getContentAsStringAsync request.Content
            return
                {Method = request.Method.ToString()
                 URI = request.RequestUri.ToString()
                 RequestHeaders = convertHeaders request.Headers
                 ContentHeaders = getContentHeaders request.Content
                 Body = requestBody
                 }
        }

    let convertResponseAsync (response: HttpResponseMessage) =
        async {
            let! responseBody = getContentAsStringAsync response.Content
            return
                {Status = {Code = response.StatusCode; Message = response.ReasonPhrase}
                 ResponseHeaders = convertHeaders response.Headers
                 ContentHeaders = getContentHeaders response.Content
                 Body = responseBody
                 HttpVersion = response.Version}
        }

    let persistCassetteToFile (filePath:string, interactions:HttpInteraction list) =
        let serializedInteraction = toJSON interactions
        File.WriteAllText (filePath, serializedInteraction.ToString())
