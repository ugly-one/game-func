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
open webapiServer.Something

type GameStateResponse = {
    Board: Board
    Actions: (CellPosition*string) list
}

[<ApiController>]
[<Route("[controller]")>]
type GameController (gameCache : GameCache, hub : GameHub2) =
    inherit ControllerBase()

    let updateCacheAndSendUpdate board actionResult clientId = 
        let (map, nextPlayer) = (covertToMap actionResult)
        gameCache.Update board map 
        let (actions, board) = convertToSerializableResponse map board
        let serializedResponse = JsonConvert.SerializeObject {Board = board; Actions = actions}
        hub.SendToAll serializedResponse |> ignore
        serializedResponse

    [<HttpGet("game")>]
    member __.GetGame() =
        printfn "providing the state and all available actions - no signalR update"
        let actionResult = gameCache.GetLastActionResult
        let board = gameCache.GetBoard
        convertToSerializableResponse actionResult board

    [<HttpGet("start")>]
    member __.Get()  =
        printfn "starting a new game"
        printfn "connectio id from controller %s" hub.GetConnectionId
        let (board, actionResult) = startGame ()
        updateCacheAndSendUpdate board actionResult hub.GetConnectionId

    [<HttpGet("move/{hp}/{vp}")>]
    member __.Move (hp: int) (vp : int) = 
        printfn "horizontal pos: %i verticalPos: %i - clientid: %s" hp vp hub.GetConnectionId 
        let actionResult = gameCache.GetLastActionResult
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
                updateCacheAndSendUpdate board actionResult hub.GetConnectionId
