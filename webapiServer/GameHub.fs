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
        | BothNotConnected -> Ok (X, PlayerXConnected connectionId)
        | PlayerXConnected playerX -> 
            if playerX = connectionId then Error "you are already connected" else Ok ( Y, BothPlayersConnected {X = playerX; Y = connectionId})
        | BothPlayersConnected b -> Error "Both players are already connected, no need for more"

    let isNextPlayer ((actions, player) : ((Action*CellPosition) list * Player)) player2 = 
        player = player2

    let swap list = list |> List.map (fun (a,b) -> (b,a))
    let convertToMapOfPositionToAction list = list |> List.map (fun (a,b) -> (b,a)) |> Map.ofList
    
    type AvailableActions = (CellPosition) list

    type Game() =
        let mutable boardAndGameState = startGame ()
        let mutable playersConnections = BothNotConnected

        member _.GetOtherPlayerConnectionId connectionId = 
            match playersConnections with
            | BothNotConnected -> None
            | PlayerXConnected id -> if connectionId = id then None else Some id
            | BothPlayersConnected ids -> if ids.X = connectionId then Some ids.Y else Some ids.X

        member _.GetAvailableActions connectionId = 
            let (_, gameState) = boardAndGameState
            match gameState with 
            | GameWon _ -> List.empty
            | GameTied -> List.empty
            | GameInProgress (actions, player) -> 
                match playersConnections with 
                | BothNotConnected -> List.empty
                | PlayerXConnected conn -> if conn = connectionId && player = X then List.map (fun (a,c) -> c) actions else List.empty
                | BothPlayersConnected connections -> 
                    if (connectionId = connections.X && player = X) || (connectionId = connections.Y && player = Y)
                    then List.map (fun (a,c) -> c) actions
                    else List.empty

        member _.GetBoard () = 
            let (board, _) = boardAndGameState
            JsonConvert.SerializeObject board

        member _.AddPlayer connectionId =
            let newPlayersConnections = addPlayer playersConnections connectionId
            match newPlayersConnections with 
            | Error msg -> 
                printfn "%s" msg
                None
            | Ok (player, connections) -> 
                playersConnections <- connections
                let (Connection connectionAsString) = connectionId
                printfn "player %s added to the game" connectionAsString
                Some player

        member _.Move posX posY connectionId = 
            let (board, gameState) = boardAndGameState
            match gameState with 
            | GameWon _ -> printfn "game is done"
            | GameTied -> printfn "game is done"
            | GameInProgress (actions, nextPlayer) -> 
                let maybePlayer = getPlayer playersConnections connectionId
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
            let connection = Connection x.Context.ConnectionId
            let player = game.AddPlayer connection
            match player with 
            | None -> ()
            | Some p -> 
                let actions = game.GetAvailableActions connection
                x.Clients.Caller.SendAsync("PlayerAssigned", JsonConvert.SerializeObject p) |> ignore
                x.Clients.Caller.SendAsync("GameChanged", game.GetBoard(), JsonConvert.SerializeObject actions) |> ignore

        member x.Move posX posY = 
            printfn "Move request: %i %i" posX posY
            let connection = Connection x.Context.ConnectionId
            game.Move posX posY connection
            let board = game.GetBoard()
            let actionsForCurrentPlayer = game.GetAvailableActions connection
            x.Clients.Caller.SendAsync("GameChanged", board, JsonConvert.SerializeObject actionsForCurrentPlayer) |> ignore

            // send update to the other player (if connected)
            let nextPlayerConnectionId = game.GetOtherPlayerConnectionId connection
            match nextPlayerConnectionId with
            | None -> ()
            | Some nextPlayerConnection -> 
                let actionsForOtherPlayer = game.GetAvailableActions nextPlayerConnection
                let (Connection connectionAsString) = nextPlayerConnection
                let client = x.Clients.Client connectionAsString
                client.SendAsync("GameChanged", board, JsonConvert.SerializeObject actionsForOtherPlayer) |> ignore 
        


