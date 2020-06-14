namespace desktopClient

open Elmish
open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Input
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts
open Microsoft.AspNetCore.SignalR.Client
open Newtonsoft.Json
open GameBoard
open Corelib.Game

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "desktopClient"
        base.Width <- 400.0
        base.Height <- 400.0
        
        let connection = 
            (HubConnectionBuilder())
                .WithUrl("http://localhost:5000/gameHub")
                .Build()

        connection.StartAsync() |> ignore

        let something state =

            let sub (dispatch: Start.Message -> unit) =
                let invoke message = 
                    printfn "received update from server"
                    let board = JsonConvert.DeserializeObject<Board> message
                    UI.printBoardWithEmptyFieldsAndPlayers board
                    // dispatch (Start.UpdateFromServer a)

                connection.On<string>("Board", fun s -> invoke s )  |> ignore
                
            Cmd.ofSub sub
        
        let test () = connection.SendAsync("Test") |> ignore
        let connect () = connection.SendAsync("Connect") |> ignore
        let move () = connection.SendAsync("Move", 2, 2) |> ignore
        
        Elmish.Program.mkProgram (fun () -> Start.init ) (Start.update test connect move) Start.view
        |> Program.withHost this
        |> Program.withSubscription something
        |> Program.run

        
type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
        this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)