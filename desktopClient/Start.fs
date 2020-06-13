namespace desktopClient
open webapiServer.Controllers
open Elmish
open Microsoft.AspNetCore.SignalR.Client
open System.Net.Http
open Newtonsoft.Json

module Start = 
    
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout

    type Message = 
        | StartGame
        | ObserveGame
        | TestMessage of Response
        | BoardMsg of GameBoard.Msg
    
    type State = 
        | Empty of HubConnection
        | GameInProgress of Response 


    let init hubConnection = Empty hubConnection, Cmd.none

    let update msg (state : State) = 
        match msg with 
            | ObserveGame -> 
                let client = new HttpClient()
                let requestUrl = GameBoard.gameUrl + "game"
                
                let requestTask = client.GetStringAsync(requestUrl)
                requestTask.Wait()
                let response = requestTask.Result
                let responseTyped = JsonConvert.DeserializeObject<Response> response

                match state with 
                    | Empty conn -> 
                        conn.StartAsync() |> ignore
                    | GameInProgress state -> failwith "he?!"
                    
                (GameInProgress responseTyped, Cmd.none)

            | TestMessage response -> 
                printfn "test message received"
                (GameInProgress (GameBoard.update (GameBoard.NewStateFromServer response) response)), Cmd.none

            | StartGame -> 
                match state with 
                | Empty conn -> 
                    conn.StartAsync() |> ignore
                    (GameInProgress GameBoard.init), Cmd.none
                | GameInProgress state -> failwith "he?!"

            | BoardMsg gameMsg -> 
                match state with 
                | Empty _ -> failwith "he?!"
                | GameInProgress state -> (GameInProgress (GameBoard.update gameMsg state)), Cmd.none

    let view state dispatch = 
        match state with 
        | Empty -> 
            StackPanel.create [ 
                StackPanel.width 300.0
                StackPanel.verticalAlignment VerticalAlignment.Center
                StackPanel.children [
                    Button.create [
                        Button.content "Start new game"
                        Button.onClick (fun _ -> dispatch StartGame)
                        Button.height 100.0
                        Button.width 200.0
                        ]
                    Button.create [
                        Button.content "Connect to existing"
                        Button.onClick (fun _ -> dispatch ObserveGame)
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