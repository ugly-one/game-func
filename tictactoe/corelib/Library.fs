namespace Corelib

module Game =

    type HorizontalPos = Left | HCenter | Right
    type VerticalPos = Top | VCenter | Bottom
    type CellPosition = HorizontalPos * VerticalPos
    type Player = X | Y
    type CellState = 
        | Empty
        | Occupied of Player 

    type Cell = {
        Pos: CellPosition
        State: CellState
    }
    
    type Board = Cell list
    
    type Action = unit -> Board * ActionResult
    and ActionResult = 
    | GameInProgress of ((Action*CellPosition) list * Player)
    | GameWon of Player
    | GameTied

    let allHorizontalPositions = [Left; HCenter; Right]
    let allVerticalPositions = [Top; VCenter; Bottom]

    let getInitialBoard = 
        let allPositions = [ 
            for hor in allHorizontalPositions do 
            for ver in allVerticalPositions do 
                yield (hor, ver)
            ]

        let emptyCells = allPositions |> List.map (fun pos -> {Pos = pos; State = Empty})

        emptyCells

    let updateBoard cell board = 
        let replaceOldCell oldCell = 
            if oldCell.Pos = cell.Pos then cell else oldCell
        
        board |> List.map replaceOldCell

    let otherPlayer player = 
        match player with 
        | X -> Y
        | Y -> X

    let getAvailablePositions board = 
        board |> List.filter (fun cell -> cell.State = Empty) |> List.map (fun cell -> cell.Pos)

    // for each position this will apply given parameters into given function 
    // and will return a collection of functions that are ready to be executed 
    let prepareActions f board player positions = 
        let actions = positions |> List.map (fun pos -> (fun () -> f board player pos),pos)
        (board, actions)

    type Line = CellPosition list

    let linesToCheck : Line list = 
        let topLine = [(Left,Top); (HCenter, Top); (Right,Top)]
        let middleHorizontalLine = [(Left,VCenter); (HCenter, VCenter); (Right,VCenter)]
        let bottomLine = [(Left,Bottom); (HCenter, Bottom); (Right,Bottom)]
        let leftLine = [(Left,Top); (Left, VCenter); (Left,Bottom)]
        let middleVerticalLine = [(HCenter,Top); (HCenter, VCenter); (HCenter,Bottom)]
        let rightLine = [(Right,Top); (Right, VCenter); (Right,Bottom)]
        let cross1 = [(Left,Top); (HCenter, VCenter); (Right,Bottom)]
        let cross2 = [(Right,Top); (HCenter, VCenter); (Left,Bottom)]
        [topLine; middleHorizontalLine; bottomLine; leftLine; middleVerticalLine; rightLine; cross1; cross2]

    let getCellsOnLine board line = 
        board |> List.filter (fun cell -> List.contains cell.Pos line)

    let isOwnedBy listOfCells player = 
        List.forall (fun cell -> cell.State = Occupied(player)) listOfCells

    let isGameWonBy board player =
        let cellLinesToCheck = linesToCheck |> List.map (fun line -> getCellsOnLine board line)
        List.exists (fun line -> isOwnedBy line player) cellLinesToCheck

    // executes move by player on given cell and returns new board and action result
    let rec playAction board player cellPosition : Board*ActionResult = 
        let newCell = {Pos = cellPosition; State = Occupied(player)}
        let newBoard = updateBoard newCell board

        let availablePositions = getAvailablePositions newBoard
        if isGameWonBy newBoard player then (newBoard,GameWon(player))
        else if List.isEmpty availablePositions then (newBoard, GameTied)
        else 
            let otherPlayer = otherPlayer player
            let (b, availableActions) = prepareActions playAction newBoard otherPlayer availablePositions
            (newBoard, GameInProgress((availableActions, otherPlayer)))

    // initilizes game and provides initial actions
    let startGame () : Board * ActionResult = 
        let initialBoard = getInitialBoard
        let availablePositions = getAvailablePositions initialBoard

        let (_,availableActions) = prepareActions playAction initialBoard X availablePositions
        (initialBoard, GameInProgress((availableActions, X)))
