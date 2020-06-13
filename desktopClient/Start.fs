namespace desktopClient
open webapiServer.Controllers
open Elmish
open Microsoft.AspNetCore.SignalR.Client

module Start = 
    
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout

    type Message = 
        | StartGame
        | TestMessage
        | BoardMsg of GameBoard.Msg
    
    type State = 
        | Empty 
        | GameInProgress of Response 


    let init = Empty, Cmd.none

    let update msg (state : State) = 
        match msg with 
            | TestMessage -> 
                printfn "test message received"
                state, Cmd.none
            | StartGame -> (GameInProgress GameBoard.init), Cmd.none
            | BoardMsg gameMsg -> 
                match state with 
                | Empty -> failwith "he?!"
                | GameInProgress state -> (GameInProgress (GameBoard.update gameMsg state)), Cmd.none

    let view state dispatch = 
        match state with 
        | Empty -> 
            DockPanel.create [ 
                DockPanel.width 300.0
                DockPanel.children [
                    Button.create [
                        Button.dock Dock.Left
                        Button.content "Start new game"
                        Button.onClick (fun _ -> dispatch StartGame)
                        Button.height 100.0
                        Button.width 200.0
                        ]]]
        | GameInProgress gameState -> 
            let gameBoard = GameBoard.view gameState (fun boardMsg -> (dispatch (BoardMsg boardMsg)))
            DockPanel.create [ 
                DockPanel.children [ gameBoard ]
                DockPanel.horizontalAlignment HorizontalAlignment.Center
                DockPanel.verticalAlignment VerticalAlignment.Center
                ]