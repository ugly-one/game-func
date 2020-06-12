namespace desktopClient
open System.Net.Http
open System

module Cell =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open Corelib.Game
    open UI

    let update () state =
        state

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
            Button.onClick ((fun a -> dispatch ()), Always)
            Button.content cellContent ]