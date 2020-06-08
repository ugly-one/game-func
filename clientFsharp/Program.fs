open System
open Microsoft.AspNetCore.SignalR.Client
open UI
open Corelib.Game
open Newtonsoft.Json

[<EntryPoint>]
let main argv =
    let connection = 
        (HubConnectionBuilder())
            .WithUrl("http://localhost:5000/SomeHub")
            .Build()

    connection.On<string, int[]>("GameStarted", fun boardJson ids -> JsonConvert.DeserializeObject<Board> boardJson |> UI.printBoardWithEmptyFieldsAndPlayers ) |> ignore
    connection.StartAsync().Wait()

    printfn "press enter to start the game"
    let input = Console.ReadLine()
    connection.SendAsync("StartGame").Wait()
    while true do 
        printfn "press enter to make an action"
        Console.ReadLine() |> ignore
        connection.SendAsync("MakeAction", 4).Wait()

    0 // return an integer exit code

