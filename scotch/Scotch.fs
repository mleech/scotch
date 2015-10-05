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
         Body : string
         Headers : Header list}

    type Status =
        {Code : HttpStatusCode
         Message : string}

    type Response =
        {Status : Status
         Headers : Header list
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

    let getHeaders (headers: Headers.HttpHeaders) =
        headers |> Seq.map (fun h -> {Key = h.Key; Value = String.Join(",", h.Value)})

    let getHttpInteractionAsync (url: string) =
        async {
            let request = {Method = HttpMethod.Get.ToString(); URI = url; Body = ""; Headers = []}

            let httpClient = new HttpClient()
            let! httpResponse= httpClient.GetAsync(url) |> Async.AwaitTask

            let status = {Code = httpResponse.StatusCode; Message = httpResponse.ReasonPhrase}
            let httpVersion = httpResponse.Version;
            let! body = httpResponse.Content.ReadAsStringAsync() |> Async.AwaitTask
            let responseHeaders = getHeaders httpResponse.Headers |> Seq.toList
            let response = {Status = status; Headers = responseHeaders; Body = body; HttpVersion = httpVersion}

            return {Request = request; Response = response; RecordedAt = DateTimeOffset.Now}
        }
