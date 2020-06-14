namespace desktopClient
open Avalonia

module Cell =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open Corelib.Game
    open UI

    let update () state =
        state

    type CellMessage = 
    | CellClicked of CellPosition

    let view cell dispatch =
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
            Button.borderThickness (Thickness 2.0)
            Button.borderBrush "White"
            Button.onClick ((fun a -> dispatch (CellClicked (cell.Pos))), Always)
            Button.content cellContent ]