open Fleece
open Fleece.Operators
open System
open System.IO
open System.Json
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Scotch

let recordLiveHttpCallsAsync () =
    async {
        let cassettePath = "C:/Users/Martin/dev/liveCalls.json"
        let clientHandler = new HttpClientHandler()
        use recordingHandler = new Scotch.RecordingHandler(clientHandler, cassettePath)
        use httpClient = new HttpClient(recordingHandler)
        let! res1 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/1") |> Async.AwaitTask
        let! res2 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/2") |> Async.AwaitTask
        let! res3 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/3") |> Async.AwaitTask
        return ()
    }

let replayRecordedCallsAsync () =
    async {
        let originalCassettePath = "C:/Users/Martin/dev/liveCalls.json"
        let replayedCassettePath = "C:/Users/Martin/dev/replayedCalls.json"
        let clientHandler = new HttpClientHandler()
        use replayHandler = new Scotch.RecordingHandler(clientHandler, originalCassettePath)
        use recordingHandler = new Scotch.RecordingHandler(replayHandler, replayedCassettePath)
        use httpClient = new HttpClient(recordingHandler)
        let! res1 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/1") |> Async.AwaitTask
        let! res2 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/2") |> Async.AwaitTask
        let! res3 = httpClient.GetAsync("http://jsonplaceholder.typicode.com/posts/3") |> Async.AwaitTask
        return ()
    }

[<EntryPoint>]
let main argv =
    recordLiveHttpCallsAsync () |> Async.RunSynchronously
    replayRecordedCallsAsync () |> Async.RunSynchronously
    0