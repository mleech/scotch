namespace Scotch

open Fleece
open System
open System.IO
open System.Net
open System.Net.Http

module Cassette =
    let locker = new Object()

    let readCassette (cassettePath: string) : HttpInteraction list =
        if (not (File.Exists(cassettePath))) then
            []
        else
            let jsonString = File.ReadAllText(cassettePath)
            let cassetteParseResult = parseJSON jsonString

            let interactions =
                match cassetteParseResult with
                | Success x -> x
                | Failure y -> failwith (sprintf "Error parsing the cassette file, Error: %A" y)

            interactions

    let writeCassette cassettePath (httpInteractions: HttpInteraction list) =
        let serializedInteraction = toJSON (List.rev httpInteractions)
        File.WriteAllText (cassettePath, serializedInteraction.ToString())

    let updateInteraction cassettePath httpInteraction =
        lock locker ( fun () ->
            let existingInteractions = readCassette cassettePath
            let newInteractions =
                match existingInteractions with
                | [] -> [httpInteraction]
                | _ -> existingInteractions
                       |> List.map  (fun i -> if (Helpers.requestsMatch httpInteraction.Request i.Request) then httpInteraction else i)

            writeCassette cassettePath newInteractions
       )
