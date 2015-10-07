namespace Scotch

module Scotch =

    open Fleece
    open Fleece.Operators
    open FSharpPlus
    open FSharp.Data
    open System
    open System.IO
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
        static member FromJSON (_: Header) =
            function
            | JObject o ->
                monad {
                    let key = Seq.head o.Keys
                    let value = string (Seq.head o.Values)

                    return {Header.Key = key; Value = value}
                }
            | x -> Failure (sprintf "Expected header, found %A" x)

    type Request with
        static member ToJSON (x: Request) =
            jobj [
                "method" .= x.Method
                "uri" .= x.URI
                "requestHeaders" .= x.RequestHeaders
                "contentHeaders" .= x.ContentHeaders
                "body" .= x.Body
            ]
        static member FromJSON (_: Request) =
            function
            | JObject o ->
                monad {
                    let! httpMethod = o .@ "method"
                    let! uri = o .@ "uri"
                    let! requestHeaders = o .@ "requestHeaders"
                    let! contentHeaders = o .@ "contentHeaders"
                    let! body = o .@ "body"
                    return {
                        Request.Method = httpMethod
                        URI = uri
                        RequestHeaders = requestHeaders
                        ContentHeaders = contentHeaders
                        Body = body
                    }
                }
            | x -> Failure (sprintf "Expected request, found %A" x)

    type Status with
        static member ToJSON (x: Status) =
            jobj [
                "code" .= string (int x.Code)
                "message" .= x.Message
            ]
        static member FromJSON (_: Status) =
            function
            | JObject o ->
                monad {
                    let! code = o .@ "code"
                    let! message = o .@ "message"
                    return {
                        Status.Code = Enum.Parse(typedefof<HttpStatusCode>, code) :?> HttpStatusCode
                        Message = message
                    }
                }
            | x -> Failure (sprintf "Expected status, found %A" x)

    type Response with
        static member ToJSON (x: Response) =
            jobj [
                "status" .= x.Status
                "responseHeaders" .= x.ResponseHeaders
                "contentHeaders" .= x.ContentHeaders
                "body" .= x.Body
                "httpVersion" .= x.HttpVersion.ToString()
            ]
        static member FromJSON (_: Response) =
            function
            | JObject o ->
                monad {
                    let! status = o .@ "status"
                    let! responseHeaders = o .@ "responseHeaders"
                    let! contentHeaders = o .@ "contentHeaders"
                    let! body = o .@ "body"
                    let! httpVersion = o .@ "httpVersion"
                    return {
                        Response.Status = status
                        ResponseHeaders = responseHeaders
                        ContentHeaders = contentHeaders
                        Body = body
                        HttpVersion = Version.Parse(httpVersion)
                    }
                }
            | x -> Failure (sprintf "Expected response, found %A" x)

    type HttpInteraction with
        static member ToJSON (x: HttpInteraction) =
            jobj [
                "request" .= x.Request
                "response" .= x.Response
                "recordedAt" .= x.RecordedAt
            ]
        static member FromJSON (_: HttpInteraction) =
            function
            | JObject o ->
                monad {
                    let! request = o .@ "request"
                    let! response = o .@ "response"
                    let! recordedAt = o .@ "recordedAt"
                    return {
                        HttpInteraction.Request = request
                        Response = response
                        RecordedAt = recordedAt
                    }
                }
            | x -> Failure (sprintf "Expected response, found %A" x)

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

    let httpResponseMessage(response: Response) =
        let result = new HttpResponseMessage(response.Status.Code)
        result.ReasonPhrase <- response.Status.Message
        result.Version <- response.HttpVersion
        for h in response.ResponseHeaders do result.Headers.TryAddWithoutValidation(h.Key, h.Value) |> ignore
        let content = new StringContent(response.Body)
        for h in response.ContentHeaders do content.Headers.TryAddWithoutValidation(h.Key, h.Value) |> ignore
        result.Content <- content
        result

    let persistCassetteToFile (cassettePath:string, interactions:HttpInteraction list) =
        let serializedInteraction = toJSON interactions
        File.WriteAllText (cassettePath, serializedInteraction.ToString())

    let readCassetteFromFile (cassettePath:string) : HttpInteraction list ParseResult  =
        let jsonString = File.ReadAllText(cassettePath)
        parseJSON jsonString