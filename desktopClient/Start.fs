namespace desktopClient
open Elmish
open Microsoft.AspNetCore.SignalR.Client

module Start = 
    
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open GameBoard
    type Message = 
        | Start
        | JoinExisting
        | UpdateFromServer of GameStateResponse
        | BoardMsg of GameBoard.Msg
        | SendTestMessage
    
    type State = 
        | Empty 
        | GameInProgress of GameStateResponse 

    let init = Empty, Cmd.none

    let update testConnection connectToGame move msg (state : State)  = 
        
        match msg with 
        | JoinExisting -> (GameInProgress (GameBoard.initJoinGame ())), Cmd.none
        | UpdateFromServer response -> GameInProgress response, Cmd.none
        | Start -> 
            (GameInProgress (GameBoard.initNewGame())), Cmd.none
        | BoardMsg gameMsg ->
            match state with 
            | Empty _ -> failwith "he?!"
            | GameInProgress state -> (GameInProgress (GameBoard.update gameMsg state)), Cmd.none
        | SendTestMessage -> 
            match state with 
            | Empty -> 
                testConnection ()
                connectToGame ()
                move ()
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
        | GameInProgress gameState -> 
            let gameBoard = GameBoard.view gameState (fun boardMsg -> (dispatch (BoardMsg boardMsg)))
            StackPanel.create [ 
                StackPanel.children [ gameBoard ]
                StackPanel.horizontalAlignment HorizontalAlignment.Center
                StackPanel.verticalAlignment VerticalAlignment.Center
                ]