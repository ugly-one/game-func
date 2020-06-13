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
open webapiServer.Controllers

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

        let something state =

            let sub (dispatch: Start.Message -> unit) =
                let invoke message = 
                    printfn "%s" message
                    printfn "received update from server"
                    let a = JsonConvert.DeserializeObject<GameStateResponse> message
                    UI.printBoardWithEmptyFieldsAndPlayers a.Board
                    dispatch (Start.UpdateFromServer a)

                connection.On<string>("Test2", fun s -> invoke s )  |> ignore
                
            Cmd.ofSub sub
        
        Elmish.Program.mkProgram (fun () -> Start.init connection) Start.update Start.view
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