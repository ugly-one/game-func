open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.SignalR
open Microsoft.AspNetCore.SignalR.Client
open System.Threading

type SomeHub() = 
    inherit Hub() // inherit from Hub<a'> I think I will get type safety - https://www.codesuji.com/2019/02/19/Building-Game-with-SignalR-and-F/

    member x.SendMessage arg1 arg2 = 
        printfn "Server: Someone send me a message: %s %s, let me send it to all clients" arg1 arg2
        x.Clients.All.SendAsync("ReceiveMessage", "from server", arg2) |> ignore

type Startup() =
    member __.ConfigureServices(services: IServiceCollection) =
        services.AddSignalR() |> ignore

    member __.Configure (app : IApplicationBuilder) =
        app.UseRouting() |> ignore
        app.UseEndpoints( fun builder -> builder.MapHub<SomeHub>("SomeHub") |> ignore ) |> ignore
        ()

[<EntryPoint>]
let main argv =
    let host =
        WebHostBuilder()
          .UseKestrel()
          .UseStartup<Startup>()
          .Build()

    async { host.Run() } |> Async.Start |> ignore // just start the server and move on with our live :) we have stuff to do below

    Thread.Sleep(2000); // let's give the server a bit of time to stand up :)

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
