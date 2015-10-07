open Fleece
open Fleece.Operators
open System
open System.IO
open System.Json
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Scotch

let makeHttpCallAsync () =
    async {
        let cassettePath = "C:/Users/Martin/dev/testing123.json"
        let clientHandler = new HttpClientHandler()
        use recordingHandler = new Scotch.RecordingHandler(clientHandler, cassettePath)
        let httpClient = new HttpClient(recordingHandler)
        let! res1 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/1") |> Async.AwaitTask
        let! res2 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/2") |> Async.AwaitTask
        let! res3 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/3") |> Async.AwaitTask
        return ()
    }

[<EntryPoint>]
let main argv =
    makeHttpCallAsync () |> Async.RunSynchronously
    0