module GameHub 

    open System
    open Microsoft.AspNetCore.SignalR;
    open Corelib.Game
    open Something
    open Newtonsoft.Json
    
    type Connection = Connection of string
    type PlayersConnectionIds = {
        X : Connection
        Y : Connection
    }

    type PlayersConnections = 
    | BothNotConnected 
    | PlayerXConnected of Connection
    | BothPlayersConnected of PlayersConnectionIds
    
    let getPlayer connections connection = 
        match connections with 
        | BothPlayersConnected connections -> 
            if connections.X = connection 
            then Some Player.X
            else if connections.Y = connection then Some Player.Y
            else None
        | PlayerXConnected conn -> if conn = connection then Some Player.X else None
        | BothNotConnected -> None

    let addPlayer playersConnections connectionId =
        match playersConnections with 
        | BothNotConnected -> Ok (PlayerXConnected connectionId)
        | PlayerXConnected playerX -> 
            if playerX = connectionId then Error "you are already connected" else Ok (BothPlayersConnected {X = playerX; Y = connectionId})
        | BothPlayersConnected b -> Error "Both players are already connected, no need for more"

    let isNextPlayer ((actions, player) : ((Action*CellPosition) list * Player)) player2 = 
        player = player2

    let convertToMapOfPositionToAction list = list |> List.map (fun (a,b) -> (b,a)) |> Map.ofList
    type Game() =
        let mutable boardAndGameState = startGame ()
        let mutable playersConnections = BothNotConnected

        member _.GetBoard () = 
            let (board, gameState) = boardAndGameState
            JsonConvert.SerializeObject board

        member _.AddPlayer connectionId =
            let newPlayersConnections = addPlayer playersConnections (Connection connectionId)
            match newPlayersConnections with 
            | Error msg -> printfn "%s" msg
            | Ok connections -> 
                playersConnections <- connections
                printfn "player %s added to the game" connectionId

        member _.Move posX posY connectionId = 
            let (board, gameState) = boardAndGameState
            match gameState with 
            | GameWon _ -> printfn "game is done"
            | GameTied -> printfn "game is done"
            | GameInProgress (actions, nextPlayer) -> 
                let maybePlayer = getPlayer playersConnections (Connection connectionId)
                match maybePlayer with 
                | None -> printfn "not a registered player"
                | Some player -> 
                    if nextPlayer = player 
                    then
                        let cellPos = createPosition (posX,posY)
                        let actionsMap = convertToMapOfPositionToAction actions
                        let maybeAction = Map.tryFind cellPos actionsMap
                        match maybeAction with
                        | None -> printfn "trying to execute action that is not allowed"
                        | Some action -> 
                            printfn "performing requested action and updating the game"
                            boardAndGameState <- action ()
                    else printfn "you are not the next player to make a move"

    type GameHub (game : Game) =
        inherit Hub()

        member x.Test () =
            printfn "CONNECTION ID %s"  x.Context.ConnectionId

        member x.Connect () = 
            printfn "Connect request %s"  x.Context.ConnectionId
            game.AddPlayer x.Context.ConnectionId

        member x.Move posX posY = 
            printfn "Move request: %i %i" posX posY
            game.Move posX posY x.Context.ConnectionId
            let board = game.GetBoard()
            x.Clients.All.SendAsync("Board", board)
            


