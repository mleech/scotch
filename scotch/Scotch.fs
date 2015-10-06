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

    let getHeaders (headers: Headers.HttpHeaders) =
        headers |> Seq.map (fun h -> {Key = h.Key; Value = String.Join(",", h.Value)}) |> Seq.toArray

    let persistCassetteToFile (filePath:string, interactions:HttpInteraction list) =
        let serializedInteraction = toJSON interactions
        File.WriteAllText (filePath, serializedInteraction.ToString())

    type RecordingHandler(filePath:string, innerHandler:HttpMessageHandler) =
        inherit DelegatingHandler(innerHandler)

        let mutable interactions = []
        let mutable tasks = []

        override handler.SendAsync (request:HttpRequestMessage, cancellationToken:Threading.CancellationToken) =
            let baseResult = base.SendAsync(request, cancellationToken)
            let workflow = async {
                let! requestBody =
                    match request.Content with
                    | null -> async {return ""}
                    | _ -> request.Content.ReadAsStringAsync() |> Async.AwaitTask

                let interactionRequest =
                    {Method = request.Method.ToString()
                     URI = request.RequestUri.ToString()
                     Body = requestBody
                     Headers = getHeaders request.Headers}

                let! httpResponse = baseResult |> Async.AwaitTask

                let status = {Code = httpResponse.StatusCode; Message = httpResponse.ReasonPhrase}
                let httpVersion = httpResponse.Version;
                let! responseBody = httpResponse.Content.ReadAsStringAsync() |> Async.AwaitTask
                let responseHeaders = getHeaders httpResponse.Headers
                let interactionResponse = {Status = status; Headers = responseHeaders; Body = responseBody; HttpVersion = httpVersion}

                interactions <- List.Cons({Request = interactionRequest; Response = interactionResponse; RecordedAt = DateTimeOffset.Now}, interactions)

                return httpResponse
            }

            let task = Async.StartAsTask workflow
            tasks <- List.Cons(task, tasks)
            task

        override handler.Dispose (disposing:bool) =
            if disposing then
                Task.WaitAll [| for t in tasks -> t :> Task |]
                persistCassetteToFile (filePath, List.rev interactions)

            base.Dispose(disposing)

    let makeHttpCallAsync () =
        async {
            let clientHandler = new HttpClientHandler()
            use recordingHandler = new RecordingHandler("C:/Users/Martin/dev/testing123.json", clientHandler)
            let httpClient = new HttpClient(recordingHandler)
            httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/1") |> Async.AwaitTask |> ignore
            httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/2") |> Async.AwaitTask |> ignore
            httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/3") |> Async.AwaitTask |> ignore
            return ()
        }
