namespace desktopClient
open System.Net.Http
open System
open Avalonia.FuncUI.Helpers

module GameBoard =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open Corelib.Game
    open UI
    open Newtonsoft.Json
    open webapiServer.Controllers

    let gameUrl = "http://localhost:5000/Game/"  

    let init = 
        let client = new HttpClient()
        let requestTask = client.GetStringAsync(gameUrl + "start")
        requestTask.Wait()
        let response = requestTask.Result
        printfn "%s" response
        JsonConvert.DeserializeObject<Response> response

    type Msg = 
        | Move of string

    let update (msg: Msg) : Response =
        match msg with
        | Move (positionString) -> 
            printfn "button clicked"
            let client = new HttpClient()
            let requestUrl = gameUrl + "move/" + positionString
            
            let requestTask = client.GetStringAsync(requestUrl)
            requestTask.Wait()
            let response = requestTask.Result
            let responseTyped = JsonConvert.DeserializeObject<Response> response

            // update all cells, in theory I could update only the 
            responseTyped.Board |> List.map (Cell.update ()) |> ignore
            responseTyped


    let view (state: Response) (dispatch) =
        let a cell  = 
            let map =  Map.ofList state.Actions
            match Map.tryFind cell.Pos map with 
            | None -> ()
            | Some guid -> dispatch (Move guid)
        
        let cells = 
            state.Board |> List.map (fun cell -> Cell.view cell (fun _ -> a cell )) |> List.map generalize

        Grid.create
            [ 
              Grid.rowDefinitions (RowDefinitions("50,50,50"))
              Grid.columnDefinitions (ColumnDefinitions("50,50,50"))
              Grid.horizontalAlignment HorizontalAlignment.Stretch
              Grid.verticalAlignment VerticalAlignment.Stretch
              Grid.children [ yield! cells ]
            ]