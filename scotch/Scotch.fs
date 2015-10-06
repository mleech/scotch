namespace Scotch

module Scotch =

    open Fleece
    open Fleece.Operators
    open System
    open System.IO
    open System.Json
    open System.Net
    open System.Net.Http
    open System.Threading.Tasks

    type Header =
        {Key : string
         Value : string}

    type Request =
        {Method : string
         URI : string
         Body : string
         Headers : Header array}

    type Status =
        {Code : HttpStatusCode
         Message : string}

    type Response =
        {Status : Status
         Headers : Header array
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
                "body" .= x.Body
                "headers" .= x.Headers
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
                "headers" .= x.Headers
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

    let convertRequestAsync (request: HttpRequestMessage) =
        async {
            let! requestBody = getContentAsStringAsync request.Content
            return
                {Method = request.Method.ToString()
                 URI = request.RequestUri.ToString()
                 Body = requestBody
                 Headers = convertHeaders request.Headers}
        }

    let convertResponseAsync (response: HttpResponseMessage) =
        async {
            let! responseBody = getContentAsStringAsync response.Content
            return
                {Status = {Code = response.StatusCode; Message = response.ReasonPhrase}
                 Headers = convertHeaders response.Headers
                 Body = responseBody
                 HttpVersion = response.Version}
        }

    let persistCassetteToFile (filePath:string, interactions:HttpInteraction list) =
        let serializedInteraction = toJSON interactions
        File.WriteAllText (filePath, serializedInteraction.ToString())
