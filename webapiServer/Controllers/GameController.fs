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

[<ApiController>]
[<Route("[controller]")>]
type GameController (gameCache : GameCache, hub : IHubContext<GameHub>) =
    inherit ControllerBase()

    let hub = hub
    let mutable gameCache = gameCache

    [<HttpGet("start")>]
    member __.Get()  =
        let (board, actionResult) = startGame()
        let guid = Guid.NewGuid()
        gameCache.Update board actionResult guid
        let test = (board, guid)
        hub.Clients.All.SendAsync("Test", JsonConvert.SerializeObject board) |> ignore
        JsonConvert.SerializeObject test

    [<HttpGet("move/{playerGuid}/{id}", Name="ha")>]
    member __.Move (playerGuid: Guid) (id:int) = 
        printfn "%i" id
        let actionResult = gameCache.GetLastActionResult
        match actionResult with 
        | GameWon p -> "the game has been won - nothing to move"
        | GameTied -> "game ended with a draw - nothing to move"
        | GameInProgress (actions, player) -> 
            let actionsArray = Array.ofList actions
            let (action, position) : (Action * CellPosition) = actionsArray.[id]
            let (board, actionResult) = action()
            gameCache.Update board actionResult playerGuid // TODO it shouldn't be necessary to provide GUID now
            hub.Clients.All.SendAsync("Test", JsonConvert.SerializeObject board) |> ignore
            JsonConvert.SerializeObject board