namespace Scotch

open System
open System.Net.Http

type ScotchMode =
    | None = 0
    | Recording = 1
    | Replaying = 2

module HttpClients =
    let NewHttpClientWithHandler (innerHandler:HttpMessageHandler, cassettePath:string, mode:ScotchMode) =
        let handler =
            match mode with
                | ScotchMode.Recording -> new RecordingHandler(innerHandler, cassettePath) :> HttpMessageHandler
                | ScotchMode.Replaying -> new ReplayingHandler(innerHandler, cassettePath) :> HttpMessageHandler
                | _ -> innerHandler

        new HttpClient(handler);

    let NewHttpClient (cassettePath:string, mode:ScotchMode) =
        NewHttpClientWithHandler(new HttpClientHandler(), cassettePath, mode)
