open System
open Microsoft.AspNetCore.SignalR.Client
open UI
open Corelib.Game
open Newtonsoft.Json

[<EntryPoint>]
let main argv =
    let connection = 
        (HubConnectionBuilder())
            .WithUrl("http://localhost:5000/GameHub")
            .Build()

    let f (json:string) = 
        // printfn "RECEIVED %s" json
        let a = JsonConvert.DeserializeObject<Board> json
        // printfn "NOT CRASHED"
        printBoardWithEmptyFieldsAndPlayers a
        ()

    connection.On<string>("Test", fun s -> f s) |> ignore
    connection.StartAsync().Wait()

    Console.WriteLine("connected")
    while true do 
        Console.ReadLine() |> ignore
        // connection.SendAsync("MakeAction", 4).Wait()

    0 // return an integer exit code

