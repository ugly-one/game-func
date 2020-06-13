
module Game 

    open System
    open Microsoft.AspNetCore.SignalR;
    open Corelib.Game

    type ActionResultWithMap = 
        | GameInProgress of Map<CellPosition,Action>
        | GameWon of Player
        | GameTied
        
    type GameHub() =
        inherit Hub()

        member x.Test () =
            let connectionId = x.Context.ConnectionId
            printfn "CONNECTION ID %s" connectionId
        member x.Internal () =
            x.Context.ConnectionId

    type GameHub2 (hubContext : IHubContext<GameHub>, gameHub : GameHub) = 
        member x.GetConnectionId = 
            gameHub.Context.ConnectionId

        member x.SendToAll (object : string) = 
            gameHub.Clients.All.SendAsync("Test2", object)

    type GameCache() =

        let mutable board: Option<Board> = None
        let mutable lastActionResult: Option<ActionResultWithMap> = None

        member this.Update _board _lastActionResult = 
            board <- Some _board
            lastActionResult <- Some _lastActionResult

        member this.GetBoard = board.Value
        member this.GetLastActionResult = 
            match lastActionResult with 
            | Some result -> result
            | None -> failwith "forgot to start the game?"
