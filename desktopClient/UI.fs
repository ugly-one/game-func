namespace desktopClient
open Corelib.Game

module UI =

    let getPlayerRepresentation player = 
        match player with 
        | X -> "X"
        | Y -> "O"

    let toOneString (s1,s2) = s1 + "," + s2

    let getPositionRepresentatin (hor, ver) = 
        let horString = match hor with 
                        | Left -> "0"
                        | HCenter -> "1"
                        | Right -> "2"
        let verString = match ver with
                        | Top -> "0"
                        | VCenter -> "1"
                        | Bottom -> "2"
        (horString, verString)

    let printBoard state getStringForEmptyField getStringForOccupied = 
        let cellToString cell = 
            match cell.State with 
            | Empty -> getStringForEmptyField cell.Pos
            | Occupied player -> getStringForOccupied player

        let printRow cells = 
            cells |> List.map cellToString |> List.reduce (fun s1 s2 -> s1 + " | " + s2) |> printfn " %s "

        let topCells = state |> List.filter (fun cell -> snd cell.Pos = Top)
        let middleCells = state |> List.filter (fun cell -> snd cell.Pos = VCenter)
        let bottomCells = state |> List.filter (fun cell -> snd cell.Pos = Bottom)

        let printSeparator () = printfn "———┼———┼———"
        printRow topCells
        printSeparator ()
        printRow middleCells
        printSeparator ()
        printRow bottomCells
        printfn "\n"

    let printBoardWithEmptyFieldsAndPlayers board = 
        printBoard board (fun _ -> " ") getPlayerRepresentation