namespace webapiServer
open Game
open Corelib.Game

module Something =
    
    let covertToMap actionResult : (ActionResultWithMap* Player option) = 
        let convertToMap actions = (List.map (fun (action,pos) -> (pos,action)) actions) |> Map.ofList
        match actionResult with
                | GameInProgress (actions, player) -> (ActionResultWithMap.GameInProgress (convertToMap actions), Some player)
                | GameWon p -> (ActionResultWithMap.GameWon p, None)
                | GameTied -> (ActionResultWithMap.GameTied, None)

    let getPositionRepresentation (hor, ver) = 
        let horString = match hor with 
                        | Left -> "0"
                        | HCenter -> "1"
                        | Right -> "2"
        let verString = match ver with
                        | Top -> "0"
                        | VCenter -> "1"
                        | Bottom -> "2"
        (horString, verString)

    
    let createHPosition pos = 
        if pos = 0 then Left 
        elif pos = 1 then HCenter 
        elif pos = 2 then Right 
        else Right


    let createVPosition pos = 
        if pos = 0 then Top
        elif pos = 1 then VCenter 
        elif pos = 2 then Bottom 
        else Bottom

    let createPosition (hp, vp) =
        (createHPosition hp, createVPosition vp)

    let convertToResponseActions actionResult = 
        let positionToString (hPos,vPos) =
            let (s1,s2) = getPositionRepresentation (hPos,vPos)
            s1 + "/" + s2

        let actions = match actionResult with
                        | ActionResultWithMap.GameInProgress map -> map |> Map.toList |> List.map fst |> List.map (fun pos -> (pos, positionToString pos))
                        | ActionResultWithMap.GameWon p -> List.empty
                        | ActionResultWithMap.GameTied -> List.empty
        actions
        