using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRConsoleApp {
    internal class Program {
        public static void Main(string[] args) 
        {
            var hubConnection = new HubConnectionBuilder()
            .WithUrl("http://127.0.0.1:5000/SomeHub")
            .Build();

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var encodedMsg = $"{user}: {message}";
                System.Console.WriteLine("RECEIVED STUFF FROM THE SERVER " + encodedMsg);
            });

            hubConnection.StartAsync().Wait();

            while (true){
                System.Console.WriteLine("Press Enter to send a message");
                Console.ReadLine();
                hubConnection.SendAsync("SendMessage", "bla", "ha").Wait();
            }
        }
    }
}