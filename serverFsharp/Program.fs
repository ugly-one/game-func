open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.SignalR

type SomeHub() = 
    inherit Hub() // inherit from Hub<a'> I think I will get type safety - https://www.codesuji.com/2019/02/19/Building-Game-with-SignalR-and-F/

    member x.SendMessage arg1 arg2 = 
        printfn "%s %s" arg1 arg2
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
    printfn "Hello World from F#!"
    let host =
        WebHostBuilder()
          .UseKestrel()
          .UseStartup<Startup>()
          .Build()

    host.Run()
  
    0 // return an integer exit code
