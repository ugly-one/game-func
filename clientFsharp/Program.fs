open System
open Microsoft.AspNetCore.SignalR.Client

[<EntryPoint>]
let main argv =
    let connection = 
        (HubConnectionBuilder())
            .WithUrl("http://localhost:5000/SomeHub")
            .Build()

    connection.On<string, string>("ReceiveMessage", fun arg1 arg2 -> printfn "received from server: %s %s" arg1 arg2) |> ignore
    connection.StartAsync().Wait()

    while true do 
        printfn "Enter your message"
        let input = Console.ReadLine()
        connection.SendAsync("SendMessage", "client1", input).Wait()

    0 // return an integer exit code

