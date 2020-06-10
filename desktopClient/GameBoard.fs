namespace desktopClient
open System.Net.Http
open System

module GameBoard =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open Corelib.Game
    open UI
    open Newtonsoft.Json
    open webapiServer.Controllers

    let getInitialBoard () = 
        let client = new HttpClient()
        let requestTask = client.GetStringAsync("http://localhost:5000/Game/start")
        requestTask.Wait()
        let response = requestTask.Result
        printfn "%s" response
        JsonConvert.DeserializeObject<Response> response

    let init = 
        getInitialBoard ()

    type Msg = 
        | StartGame
        | Move of string

    let update (msg: Msg) (state: Response) : Response =
        match msg with
        | StartGame -> 
            printfn "start clicked - doing nothing"
            state
        | Move positionString -> 
            printfn "button clicked"
            let client = new HttpClient()
            let requestUrl = "http://localhost:5000/Game/move/" + positionString

            printfn "%s" requestUrl
            
            let requestTask = client.GetStringAsync(requestUrl)
            requestTask.Wait()
            let response = requestTask.Result
            printfn "%s" response
            JsonConvert.DeserializeObject<Response> response
    
    let cellView (cell:Cell) dispatchMsg = 
        let convertPosToNumber (hPos,vPos) = 
            let vPosNr = match vPos with 
                            | Top -> 0
                            | VCenter -> 1
                            | Bottom -> 2
            let hPosNr = match hPos with 
                            | Left -> 0
                            | HCenter -> 1
                            | Right -> 2
            (vPosNr, hPosNr)
    
        let (xPos, yPos) = cell.Pos |> convertPosToNumber
        let cellContent = match cell.State with 
                            | Empty -> ""
                            | Occupied player -> getPlayerRepresentation player

        Button.create
          [ Button.row xPos
            Button.column yPos
            Button.onClick (fun a -> dispatchMsg() )
            Button.content cellContent ]
    
    let test (response: Response)  (cell:Cell) dispatch =
        let map =  Map.ofList response.Actions
        match Map.tryFind cell.Pos map with 
        | None -> fun () -> ()
        | Some guid -> fun () -> dispatch (Move guid)

    let view (state: Response) (dispatch) =
        DockPanel.create [
            DockPanel.children [
                Grid.create
                    [ 
                      Grid.rowDefinitions (RowDefinitions("50,50,50"))
                      Grid.columnDefinitions (ColumnDefinitions("50,50,50"))
                      Grid.horizontalAlignment HorizontalAlignment.Stretch
                      Grid.verticalAlignment VerticalAlignment.Stretch
                      Grid.children [ for cell in state.Board do yield cellView cell (test state cell dispatch)]
                    ]
            ]
        ]       