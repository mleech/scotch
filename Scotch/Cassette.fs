namespace Scotch

open Fleece
open System
open System.IO
open System.Net
open System.Net.Http

module Cassette =
    let locker = new Object()

    let ReadCassette (cassettePath: string) : HttpInteraction [] =
        if (not (File.Exists(cassettePath))) then
            [||]
        else
            let jsonString = File.ReadAllText(cassettePath)
            let cassetteParseResult = parseJSON jsonString

            let interactions =
                match cassetteParseResult with
                | Success x -> x
                | Failure y -> failwith (sprintf "Error parsing the cassette file, Error: %A" y)

            interactions

    let WriteCassette cassettePath (httpInteractions: HttpInteraction []) =
            let serializedInteraction = toJSON (Seq.toList httpInteractions)
            File.WriteAllText (cassettePath, serializedInteraction.ToString())

    let UpdateInteraction cassettePath httpInteraction =
        lock locker ( fun () ->
            let existingInteractions = ReadCassette cassettePath
            let matchingIndex = Array.FindIndex (existingInteractions, fun i -> Helpers.requestsMatch httpInteraction.Request i.Request) 
            let newInteractions =
                match matchingIndex with
                | -1 -> Array.append existingInteractions [|httpInteraction|]
                | _ -> existingInteractions
                       |> Array.mapi  (fun index item -> if (index = matchingIndex) then httpInteraction else item)

            WriteCassette cassettePath newInteractions
       )
