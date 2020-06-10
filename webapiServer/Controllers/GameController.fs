namespace webapiServer.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Game
open Microsoft.AspNetCore.SignalR
open Corelib.Game
open Newtonsoft.Json

type Response = {
    Board: Board
    Actions: (Guid * CellPosition) list
}

[<ApiController>]
[<Route("[controller]")>]
type GameController (gameCache : GameCache, hub : IHubContext<GameHub>) =
    inherit ControllerBase()

    let hub = hub
    let mutable gameCache = gameCache

    let addGuid actionResult : ActionResultWithGuid = 
        
        let createMapWithGuid (actions: (Action * CellPosition) list)  : Map<Guid,(Action*CellPosition)> = 
            let guids = Seq.initInfinite (fun _ -> Guid.NewGuid()) |> Seq.take actions.Length |> List.ofSeq
            let zippedList = List.zip guids actions
            Map.ofList zippedList

        match actionResult with
                | GameInProgress (actions,player) -> ActionResultWithGuid.GameInProgress (createMapWithGuid (actions))
                | GameWon p -> ActionResultWithGuid.GameWon p
                | GameTied -> ActionResultWithGuid.GameTied

    [<HttpGet("start")>]
    member __.Get()  =
        let (board, actionResult) = startGame()

        let actionResultWithGuid = addGuid actionResult
        
        gameCache.Update board actionResultWithGuid

        let actions = match actionResultWithGuid with
                        | ActionResultWithGuid.GameInProgress map -> map |> Map.map (fun key (action,cellPos) -> (key,cellPos) ) |> Map.toList |> List.map snd
                        | ActionResultWithGuid.GameWon p -> List.empty
                        | ActionResultWithGuid.GameTied -> List.empty
        
        hub.Clients.All.SendAsync("Test", JsonConvert.SerializeObject board) |> ignore
        JsonConvert.SerializeObject {Board = board; Actions = actions}

    [<HttpGet("move/{actionGuid}")>]
    member __.Move (actionGuid: Guid) = 
        printfn "%s" (actionGuid.ToString())
        let actionResult : ActionResultWithGuid = gameCache.GetLastActionResult
        match actionResult with 
        | ActionResultWithGuid.GameWon p -> "the game has been won - nothing to move"
        | ActionResultWithGuid.GameTied -> "game ended with a draw - nothing to move"
        | ActionResultWithGuid.GameInProgress actions -> 
            let actionOption = Map.tryFind actionGuid actions
            match actionOption with 
            | None -> "This action is not available"
            | Some (action, cellPos) -> 
                let (board, actionResult) = action()
                let actionResultWithGuid = addGuid actionResult
                gameCache.Update board actionResultWithGuid
                let actions = match actionResultWithGuid with
                                | ActionResultWithGuid.GameInProgress map -> map |> Map.map (fun key (action,cellPos) -> (key,cellPos) ) |> Map.toList |> List.map snd
                                | ActionResultWithGuid.GameWon p -> List.empty
                                | ActionResultWithGuid.GameTied -> List.empty
                
                hub.Clients.All.SendAsync("Test", JsonConvert.SerializeObject board) |> ignore
                JsonConvert.SerializeObject {Board = board; Actions = actions}
