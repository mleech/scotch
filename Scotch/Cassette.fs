namespace Scotch

open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Converters

module Cassette =
    let locker = new Object()

    let ReadCassette (cassettePath: string) : HttpInteraction seq =
        if (not (File.Exists(cassettePath))) then
            Seq.empty
        else
            let jsonString = File.ReadAllText(cassettePath)
            let cassetteParseResult = JsonConvert.DeserializeObject<List<HttpInteraction>> (jsonString, new VersionConverter())
            List.toSeq cassetteParseResult

    let WriteCassette cassettePath (httpInteractions: HttpInteraction seq) =
            let serializedInteraction = JsonConvert.SerializeObject (Seq.toList httpInteractions, Formatting.Indented, new VersionConverter())
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
