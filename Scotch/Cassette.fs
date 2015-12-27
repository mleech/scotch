namespace Scotch

open Fleece
open System
open System.IO
open System.Net
open System.Net.Http

module Cassette =
    let locker = new Object()

    let ReadCassette (cassettePath: string) : HttpInteraction seq =
        if (not (File.Exists(cassettePath))) then
            Seq.empty
        else
            let jsonString = File.ReadAllText(cassettePath)
            let cassetteParseResult = parseJSON jsonString

            let interactions =
                match cassetteParseResult with
                | Success x -> List.toSeq x
                | Failure y -> failwith (sprintf "Error parsing the cassette file, Error: %A" y)

            interactions

    let WriteCassette cassettePath (httpInteractions: HttpInteraction seq) =
            let serializedInteraction = toJSON (Seq.toList httpInteractions)
            File.WriteAllText (cassettePath, serializedInteraction.ToString())

    let UpdateInteraction cassettePath httpInteraction =
        lock locker ( fun () ->
            let existingInteractions = ReadCassette cassettePath
            let matchingIndex = existingInteractions
                                |> Seq.tryFindIndex (fun i -> Helpers.requestsMatch httpInteraction.Request i.Request)

            let newInteractions =
                match matchingIndex with
                | None -> Seq.append existingInteractions [|httpInteraction|]
                | Some i -> existingInteractions
                            |> Seq.mapi  (fun index item -> if (index = i) then httpInteraction else item)

            WriteCassette cassettePath newInteractions
       )
