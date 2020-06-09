
module Game 

    open System
    open Microsoft.AspNetCore.SignalR;
    open Corelib.Game

    type GameHub() =
        inherit Hub()

    type GameCache() =

        let mutable board: Option<Board> = None
        let mutable lastActionResult: Option<ActionResult> = None
        let mutable playerXGuid: Guid = Guid.Empty

        member this.Update _board _lastActionResult _playerXGuid = 
            board <- Some _board
            lastActionResult <- Some _lastActionResult
            playerXGuid <- _playerXGuid

        member this.GetLastActionResult = 
            match lastActionResult with 
            | Some result -> result
            | None -> failwith "forgot to start the game?"
