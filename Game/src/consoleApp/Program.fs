open System
open Corelib.Game

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


let rec printBoardAndMakeFirstAvailableAction board actionResult = 
    printBoardWithEmptyFieldsAndPlayers board

    match actionResult with 
    | GameWon player -> printfn "PLAYER %s won" (getPlayerRepresentation player)
    | GameTied -> printfn "DRAW"
    | GameInProgress (actions, player) -> 
        
        // show the options
        printfn "Player %s Where would you like to play? \n" (getPlayerRepresentation player)
        let positions = List.map (fun (_, pos) -> pos) actions
        let indexes = seq {1 .. List.length positions} 
        let indexedActions = Seq.zip indexes actions |> Map.ofSeq
        let indexBasedOnPosition = Seq.zip positions indexes |> Map.ofSeq
        printBoard board (fun position -> (Map.find position indexBasedOnPosition) |> string) (fun _ -> " ")
        
        // choose the option
        let choice = Console.ReadLine() |> int

        // execute the option
        let (actionToPlay, cellPosition) = Map.find choice indexedActions
        let (x,y) = (getPositionRepresentatin cellPosition)
        printfn "PLAYING %s %s \n" x y
        let (newBoard, newResult) = actionToPlay()
        printBoardAndMakeFirstAvailableAction newBoard newResult


[<EntryPoint>]
let main argv =
    let (board, actionResult) = startGame()
    printBoardAndMakeFirstAvailableAction board actionResult
    0
