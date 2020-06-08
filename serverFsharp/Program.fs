open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.SignalR
open System.Threading
open Corelib.Game
open Newtonsoft.Json

type SomeHub() = 
    inherit Hub() // inherit from Hub<a'> I think I will get type safety - https://www.codesuji.com/2019/02/19/Building-Game-with-SignalR-and-F/

    let mutable board = None
    let mutable availableActions = None

    member x.StartGame () = 
        printfn "received a request to start a new game"
        let (b, actionResult) = startGame()
        board <- Some b
        availableActions <- match actionResult with 
                                | GameInProgress (actions, player) -> Some actions
                                | GameWon player -> None
                                | GameTied -> None

        let actionIds = seq [1..9] |> Array.ofSeq
        // tell clients about the new game
        let serializedBoard = JsonConvert.SerializeObject b
        x.Clients.All.SendAsync("GameStarted", serializedBoard, actionIds) |> ignore

    member x.MakeAction (actionId:int) = 
        printfn "received a request to make a move %i" actionId

        // TODO looks like I get a new instance of a Hub for each request. I need to share state somehow
        match availableActions with 
        | None -> ()
        | Some actions -> let (action, cellPosition) = List.head actions
                          let (b, actionResult) = action()
                          board <- Some b
                          availableActions <- match actionResult with 
                                                    | GameInProgress (actions, player) -> Some actions
                                                    | GameWon player -> None
                                                    | GameTied -> None
        
        let serializedBoard = JsonConvert.SerializeObject board.Value
        let actionIds = seq [1..9] |> Array.ofSeq

        x.Clients.All.SendAsync("GameStarted", serializedBoard, actionIds) |> ignore

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

    host.Run()
    
    0 // return an integer exit code
