
module Game 

    open System
    open Microsoft.AspNetCore.SignalR;
    open Corelib.Game

    type GameHub() =
        inherit Hub()

    type ActionResultWithGuid = 
        | GameInProgress of Map<Guid,Action*CellPosition>
        | GameWon of Player
        | GameTied

    type GameCache() =

        let mutable board: Option<Board> = None
        let mutable lastActionResult: Option<ActionResultWithGuid> = None

        member this.Update _board _lastActionResult = 
            board <- Some _board
            lastActionResult <- Some _lastActionResult

        member this.GetLastActionResult = 
            match lastActionResult with 
            | Some result -> result
            | None -> failwith "forgot to start the game?"
