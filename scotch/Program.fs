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
        let clientHandler = new HttpClientHandler()
        use recordingHandler = new Scotch.RecordingHandler("C:/Users/Martin/dev/testing123.json", clientHandler)
        let httpClient = new HttpClient(recordingHandler)
        httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/1") |> Async.AwaitTask |> ignore
        httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/2") |> Async.AwaitTask |> ignore
        httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/3") |> Async.AwaitTask |> ignore
        return ()
    }

[<EntryPoint>]
let main argv =
    makeHttpCallAsync () |> Async.RunSynchronously
    0