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
                let invoke board actions = 
                    printfn "received update from server %s" actions
                    let board = JsonConvert.DeserializeObject<Board> board
                    UI.printBoardWithEmptyFieldsAndPlayers board

                    let actions = JsonConvert.DeserializeObject<list<CellPosition>> actions

                    let moves = actions |> List.map (fun cellPos -> 
                        cellPos , fun () -> 
                                    let (h, v) = Corelib.Game.toInts cellPos
                                    connection.SendAsync("Move", h, v) |> ignore) |> Map.ofList

                    dispatch (Start.UpdateFromServer (board, actions))

                connection.On<string, string>("GameChanged", fun board actions -> invoke board actions )  |> ignore
                
            Cmd.ofSub sub
        
        let test () = connection.SendAsync("Test") |> ignore
        let connect () = connection.SendAsync("Connect") |> ignore
        let move (h: int) (v:int) = connection.SendAsync("Move", h, v) |> ignore
        
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