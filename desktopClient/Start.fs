namespace desktopClient
open Elmish
open Microsoft.AspNetCore.SignalR.Client
open Corelib.Game

module Start = 
    
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open GameBoard
    type Message = 
        | Start
        | JoinExisting
        | UpdateFromServer of Board * CellPosition list
        | BoardMsg of Cell.CellMessage
        | SendTestMessage
    
    type State = 
        | Empty 
        | GameInProgress of Board * CellPosition list

    let init = Empty, Cmd.none

    let update testConnection connectToGame move msg (state : State)  = 
        
        match msg with 
        | JoinExisting -> 
            connectToGame ()
            state, Cmd.none
        | UpdateFromServer (board, clickableCells) -> GameInProgress  (board, clickableCells), Cmd.none
        | Start -> 
            connectToGame ()
            state, Cmd.none
        | BoardMsg gameMsg ->
            match state with 
            | Empty _ -> failwith "he?!"
            | GameInProgress  (board, clickableCells) -> 
                let a = gameMsg
                match a with 
                | Cell.CellClicked pos -> 
                    if List.contains pos clickableCells then 
                        let (h, v) = Corelib.Game.toInts pos
                        move h v
                    else ()
                state, Cmd.none
                // (GameInProgress (GameBoard.update gameMsg  (board, clickableCells))), Cmd.none
        | SendTestMessage -> 
            match state with 
            | Empty -> 
                testConnection ()
                connectToGame ()
                move 1 2
                state, Cmd.none
            | GameInProgress -> state, Cmd.none
        

    let view state dispatch = 
        match state with 
        | Empty -> 
            StackPanel.create [ 
                StackPanel.width 300.0
                StackPanel.verticalAlignment VerticalAlignment.Center
                StackPanel.children [
                    Button.create [
                        Button.content "Start new game"
                        Button.onClick (fun _ -> dispatch Start)
                        Button.height 100.0
                        Button.width 200.0
                        ]
                    Button.create [
                        Button.content "Connect to existing"
                        Button.onClick (fun _ -> dispatch JoinExisting)
                        Button.height 100.0
                        Button.width 200.0
                        ] 
                    Button.create [
                        Button.content "Send message to SignalR"
                        Button.onClick (fun _ -> dispatch SendTestMessage)
                        Button.height 100.0
                        Button.width 200.0
                        ] 
                       ]]
        | GameInProgress  (board, clickableCells) -> 
            let gameBoard = GameBoard.view board  (fun (cellMsg) -> dispatch (BoardMsg cellMsg))
            StackPanel.create [ 
                StackPanel.children [ gameBoard ]
                StackPanel.horizontalAlignment HorizontalAlignment.Center
                StackPanel.verticalAlignment VerticalAlignment.Center
                ]