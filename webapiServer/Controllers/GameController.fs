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
    Actions: (CellPosition*string) list
}

[<ApiController>]
[<Route("[controller]")>]
type GameController (gameCache : GameCache, hub : IHubContext<GameHub>) =
    inherit ControllerBase()

    let hub = hub
    let gameCache = gameCache


    let covertToMap actionResult : ActionResultWithMap = 
        let a actions = (List.map (fun (action,pos) -> (pos,action)) actions) |> Map.ofList
        match actionResult with
                | GameInProgress (actions,player) -> ActionResultWithMap.GameInProgress (a actions)
                | GameWon p -> ActionResultWithMap.GameWon p
                | GameTied -> ActionResultWithMap.GameTied

    let getPositionRepresentatin (hor, ver) = 
        let horString = match hor with 
                        | Left -> "0"
                        | HCenter -> "1"
                        | Right -> "2"
        let verString = match ver with
                        | Top -> "0"
                        | VCenter -> "1"
                        | Bottom -> "2"
        (horString, verString)

    
    let createHPosition pos = 
        if pos = 0 then Left 
        elif pos = 1 then HCenter 
        elif pos = 2 then Right 
        else Right


    let createVPosition pos = 
        if pos = 0 then Top
        elif pos = 1 then VCenter 
        elif pos = 2 then Bottom 
        else Bottom

    let createPosition (hp, vp) =
        (createHPosition hp, createVPosition vp)


    let getResponse actionResult board = 
        let positionToString (hPos,vPos) =
            let (s1,s2) = getPositionRepresentatin (hPos,vPos)
            s1 + "/" + s2

        let actions = match actionResult with
                        | ActionResultWithMap.GameInProgress map -> map |> Map.toList |> List.map fst |> List.map (fun pos -> (pos, positionToString pos))
                        | ActionResultWithMap.GameWon p -> List.empty
                        | ActionResultWithMap.GameTied -> List.empty
        
        {Board = board; Actions = actions}
        
    let updateCacheAndSendUpdate board actionResult = 
        let map = (covertToMap actionResult)
        gameCache.Update board map
        let response = getResponse map board
        let serializedResponse = JsonConvert.SerializeObject response
        hub.Clients.All.SendAsync("Test2", serializedResponse) |> ignore
        serializedResponse

    [<HttpGet("game")>]
    member __.GetGame() =
        let actionResult : ActionResultWithMap = gameCache.GetLastActionResult
        let board = gameCache.GetBoard
        JsonConvert.SerializeObject (getResponse actionResult board)

    [<HttpGet("start")>]
    member __.Get()  =
        let (board, actionResult) = startGame()
        updateCacheAndSendUpdate board actionResult

    [<HttpGet("move/{hp}/{vp}")>]
    member __.Move (hp: int) (vp : int) = 
        printfn "horizontal pos: %i verticalPos: %i" hp vp 
        let actionResult : ActionResultWithMap = gameCache.GetLastActionResult
        match actionResult with 
        | ActionResultWithMap.GameWon p -> "the game has been won - nothing to move"
        | ActionResultWithMap.GameTied -> "game ended with a draw - nothing to move"
        | ActionResultWithMap.GameInProgress actions -> 
            let position = createPosition (hp,vp)
            let actionOption = Map.tryFind position actions
            match actionOption with 
            | None -> "This action is not available"
            | Some action -> 
                let (board, actionResult) = action()
                updateCacheAndSendUpdate board actionResult
