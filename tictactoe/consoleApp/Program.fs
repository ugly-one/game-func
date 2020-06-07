open System
open Corelib.Game
open UI


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
