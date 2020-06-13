namespace desktopClient
open System.Net.Http
open System
open Avalonia.FuncUI.Helpers
open Microsoft.AspNetCore.SignalR.Client

module GameBoard =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open Corelib.Game
    open UI
    open Newtonsoft.Json
    open webapiServer.Controllers

    let gameUrl = "http://localhost:5000/Game/"  

    let initNewGame () = 
        let client = new HttpClient()
        let requestTask = client.GetStringAsync(gameUrl + "start")
        requestTask.Wait()
        let response = requestTask.Result
        printfn "%s" response
        JsonConvert.DeserializeObject<Response> response

    let initJoinGame () = 
        let client = new HttpClient()
        let requestTask = client.GetStringAsync(gameUrl + "game")
        requestTask.Wait()
        let response = requestTask.Result
        printfn "%s" response
        JsonConvert.DeserializeObject<Response> response

    type Msg = 
        | CellClicked of string

    let update (msg: Msg) state : Response =
        match msg with
        | CellClicked (positionString) -> 
            printfn "button clicked"
            let client = new HttpClient()
            let requestUrl = gameUrl + "move/" + positionString
            client.GetStringAsync(requestUrl) |> ignore
            state

    let view (state: Response) (dispatch) =
        let getDispatchFunction cell  = 
            let map =  Map.ofList state.Actions
            match Map.tryFind cell.Pos map with 
            | None -> ()
            | Some guid -> dispatch (CellClicked guid)
        
        let cells = 
            state.Board |> List.map (fun cell -> Cell.view cell (fun _ -> getDispatchFunction cell )) |> List.map generalize

        Grid.create [ 
              Grid.rowDefinitions (RowDefinitions("50,50,50"))
              Grid.columnDefinitions (ColumnDefinitions("50,50,50"))
              Grid.horizontalAlignment HorizontalAlignment.Stretch
              Grid.verticalAlignment VerticalAlignment.Stretch
              Grid.children [ yield! cells ]]