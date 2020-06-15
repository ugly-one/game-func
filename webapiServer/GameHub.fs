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

    let swap list = list |> List.map (fun (a,b) -> (b,a))
    let convertToMapOfPositionToAction list = list |> List.map (fun (a,b) -> (b,a)) |> Map.ofList
    
    type AvailableActions = (CellPosition) list

    type Game() =
        let mutable boardAndGameState = startGame ()
        let mutable playersConnections = BothNotConnected

        member _.GetNextPlayerConnectionId () = 
            let (_, gameState) = boardAndGameState
            match gameState with 
            | GameWon _ -> None
            | GameTied -> None
            | GameInProgress (actions, player) -> 
                match playersConnections with
                | BothNotConnected -> None
                | PlayerXConnected id -> if player = X then Some id else None
                | BothPlayersConnected ids -> if player = X then Some ids.X else Some ids.Y

        member _.GetAvailableActions () = 
            let (_, gameState) = boardAndGameState
            let actionsList = 
                match gameState with 
                | GameWon _ -> List.empty
                | GameTied -> List.empty
                | GameInProgress (actions, player) -> 
                    List.map (fun (a,c) -> c) actions

            JsonConvert.SerializeObject actionsList

        member _.GetBoard () = 
            let (board, _) = boardAndGameState
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
            // TODO I should return available actions only for that connection/player
            x.Clients.Caller.SendAsync("GameChanged", game.GetBoard(), game.GetAvailableActions ()) |> ignore

        member x.Move posX posY = 
            printfn "Move request: %i %i" posX posY
            game.Move posX posY x.Context.ConnectionId
            let board = game.GetBoard()
            let actions = game.GetAvailableActions ()
            // TODO getAvailableActions should take player and return actions for that player
            // I shouldn't know here that the caller won't have any actions now
            x.Clients.Caller.SendAsync("GameChanged", board, JsonConvert.SerializeObject list.Empty) |> ignore

            // send update to the other player
            let nextPlayerConnectionId = game.GetNextPlayerConnectionId ()
            match nextPlayerConnectionId with
            | None -> ()
            | Some id -> 
                match id with // I wonder if there is a nicer way to extract id as a string from Connection
                | Connection id_ -> 
                    let client = x.Clients.Client id_
                    client.SendAsync("GameChanged", board, actions) |> ignore 
            


