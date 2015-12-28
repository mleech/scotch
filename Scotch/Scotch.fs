namespace Scotch

open System
open System.Net.Http

type ScotchMode =
    | None = 0
    | Recording = 1
    | Replaying = 2

module Scotch =
    let GetHandler (innerHandler:HttpMessageHandler, cassettePath:string, mode:ScotchMode) =
        match mode with
        | ScotchMode.Recording -> new RecordingHandler(innerHandler, cassettePath) :> HttpMessageHandler
        | ScotchMode.Replaying -> new ReplayingHandler(innerHandler, cassettePath) :> HttpMessageHandler
        | _ -> innerHandler


