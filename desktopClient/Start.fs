namespace desktopClient
open webapiServer.Controllers

module Start = 
    
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout

    type Message = 
        | StartGame
        | BoardMsg of GameBoard.Msg
    
    type State = 
        | Empty 
        | GameInProgress of Response 

    let init = Empty

    let update msg (state : State) = 
        match msg with 
            | StartGame -> GameInProgress GameBoard.init
            | BoardMsg gameMsg -> 
                GameInProgress (GameBoard.update gameMsg)

    let view state dispatch = 
        
        match state with 
        | Empty -> 
            DockPanel.create [ DockPanel.children [
                Button.create [
                    Button.content "Start new game"
                    Button.onClick (fun _ -> dispatch StartGame)
                    Button.height 100.0
                    Button.width 200.0
                    ]
            ]]
        | GameInProgress gameState -> 
            let gameBoard = GameBoard.view gameState (fun boardMsg -> (dispatch (BoardMsg boardMsg)))
            DockPanel.create [ 
                DockPanel.children [ gameBoard ]
                DockPanel.horizontalAlignment HorizontalAlignment.Center
                DockPanel.verticalAlignment VerticalAlignment.Center
                ]