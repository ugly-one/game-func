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

    type AvailableActions = (CellPosition) list

    type GameStateResponse = {
        Board: Board
        Actions: AvailableActions
    }


    let update msg state =
        state

    let view (state: Board) dispatch =
        // let getDispatchFunction cell  = 
        //     match Map.tryFind cell.Pos actionsMap with 
        //     | None -> fun () -> ()
        //     | Some dispatch -> dispatch
        
        let cells = 
            state |> List.map (fun cell -> Cell.view cell dispatch) |> List.map generalize

        Grid.create [ 
              Grid.rowDefinitions (RowDefinitions("50,50,50"))
              Grid.columnDefinitions (ColumnDefinitions("50,50,50"))
              Grid.horizontalAlignment HorizontalAlignment.Stretch
              Grid.verticalAlignment VerticalAlignment.Stretch
              Grid.children [ yield! cells ]]