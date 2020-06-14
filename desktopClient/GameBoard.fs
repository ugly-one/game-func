namespace desktopClient
open Avalonia.FuncUI.Helpers

module GameBoard =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open Corelib.Game

    let update msg state =
        state

    let view (state: Board) dispatch =
        let cells = 
            state |> List.map (fun cell -> Cell.view cell dispatch) |> List.map generalize

        Grid.create [ 
              Grid.rowDefinitions (RowDefinitions("50,50,50"))
              Grid.columnDefinitions (ColumnDefinitions("50,50,50"))
              Grid.horizontalAlignment HorizontalAlignment.Stretch
              Grid.verticalAlignment VerticalAlignment.Stretch
              Grid.children [ yield! cells ]]