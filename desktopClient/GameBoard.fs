namespace desktopClient

module GameBoard =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout
    open Corelib.Game
    open UI

    let init = getInitialBoard

    type Msg = StartGame

    let update (msg: Msg) (state: Board) : Board =
        match msg with
        | StartGame -> 
            printfn "button clicked"
            state

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

    let cellView (cell:Cell) dispath = 
        let (xPos, yPos) = cell.Pos |> convertPosToNumber
        let cellContent = match cell.State with 
                            | Empty -> ""
                            | Occupied player -> getPlayerRepresentation player

        Button.create
          [ Button.row xPos
            Button.column yPos
            Button.onClick (fun a -> dispath StartGame )
            Button.content cellContent ]
   
    let view (state: Board) (dispatch) =
        DockPanel.create [
            DockPanel.children [
                Grid.create
                    [ 
                      Grid.rowDefinitions (RowDefinitions("50,50,50"))
                      Grid.columnDefinitions (ColumnDefinitions("50,50,50"))
                      Grid.horizontalAlignment HorizontalAlignment.Stretch
                      Grid.verticalAlignment VerticalAlignment.Stretch
                      Grid.children [ for cell in state do yield cellView cell dispatch]
                    ]
            ]
        ]       