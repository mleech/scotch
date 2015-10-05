open System
open System.IO
open System.Net
open System.Net.Http
open Nessos.FsPickler.Json

type Header =
    {Key : string
     Value : string}

type Request =
    {Method : string
     URI : string
     Body : string
     Headers : List<Header>}

type Status =
    {Code : HttpStatusCode
     Message : string}

type Response =
    {Status : Status
     Headers : List<Header>
     Body : string
     HttpVersion : Version}

type HttpInteraction =
    {Request : Request
     Response : Response
     RecordedAt : DateTimeOffset}

let getHeaders (headers: Headers.HttpHeaders) =
    headers |> Seq.map (fun h -> {Key = h.Key; Value = String.Join(",", h.Value)})

let getHttpInteractionAsync (url: string) =
    async {
        let request = {Method = HttpMethod.Get.ToString(); URI = url; Body = ""; Headers = List.Empty}

        let httpClient = new HttpClient()
        let! httpResponse= httpClient.GetAsync(url) |> Async.AwaitTask

        let status = {Code = httpResponse.StatusCode; Message = httpResponse.ReasonPhrase}
        let httpVersion = httpResponse.Version;
        let! body = httpResponse.Content.ReadAsStringAsync() |> Async.AwaitTask
        let responseHeaders = getHeaders httpResponse.Headers |> Seq.toList
        let response = {Status = status; Headers = responseHeaders; Body = body; HttpVersion = httpVersion}

        return {Request = request; Response = response; RecordedAt = DateTimeOffset.Now}
    }

[<EntryPoint>]
let main argv =
    let interaction = getHttpInteractionAsync "http://jsonplaceholder.typicode.com/posts/1" |> Async.RunSynchronously
    let jsonSerializer = FsPickler.CreateJsonSerializer(indent = true, omitHeader = true)
    let serializedInteraction = jsonSerializer.Pickle interaction
    File.WriteAllBytes ("C:/Users/Martin/dev/testing123.txt", serializedInteraction)
    0