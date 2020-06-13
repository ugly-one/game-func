
module Game 

    open System
    open Microsoft.AspNetCore.SignalR;
    open Corelib.Game

    type GameHub() =
        inherit Hub()

    type ActionResultWithMap = 
        | GameInProgress of Map<CellPosition,Action>
        | GameWon of Player
        | GameTied

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
