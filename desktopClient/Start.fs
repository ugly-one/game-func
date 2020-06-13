namespace desktopClient
open webapiServer.Controllers
open Elmish
open Microsoft.AspNetCore.SignalR.Client

module Start = 
    
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout

    type Message = 
        | Start
        | JoinExisting
        | UpdateFromServer of GameStateResponse
        | BoardMsg of GameBoard.Msg
        | SendTestMessage
    
    type State = 
        | Empty of HubConnection
        | GameInProgress of GameStateResponse 

    let init hubConnection = Empty hubConnection, Cmd.none

    let update msg (state : State) = 
        
        match state with 
        | Empty hubConnection -> hubConnection.StartAsync() |> ignore
        | GameInProgress -> ()
        
        match msg with 
        | JoinExisting -> (GameInProgress (GameBoard.initJoinGame ())), Cmd.none
        | UpdateFromServer response -> GameInProgress response, Cmd.none
        | Start -> (GameInProgress (GameBoard.initNewGame())), Cmd.none
        | BoardMsg gameMsg ->
            match state with 
            | Empty _ -> failwith "he?!"
            | GameInProgress state -> (GameInProgress (GameBoard.update gameMsg state)), Cmd.none
        | SendTestMessage -> 
            match state with 
            | Empty hubConnection -> 
                hubConnection.StartAsync() |> ignore
                hubConnection.SendAsync("Test") |> ignore
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