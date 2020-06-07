using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Configuring the web host");

            var host = new WebHostBuilder()
            .UseKestrel()
            .UseStartup<Startup>()
            .Build();

            var task = Task.Factory.StartNew(() => host.Run());
            Thread.Sleep(2000); // :)

            Console.WriteLine("web host is running");
            System.Console.WriteLine("Configuring the client...");

            var hubConnection = new HubConnectionBuilder()
            .WithUrl("http://127.0.0.1:5000" + HubConfiguration.Name)
            .Build();

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var encodedMsg = $"{user}: {message}";
                System.Console.WriteLine("RECEIVED STUFF FROM THE SERVER " + encodedMsg);
            });

            hubConnection.StartAsync().Wait();

            while (true){
                System.Console.Write("Enter message to send: ");
                var message = Console.ReadLine();
                hubConnection.SendAsync("SendMessage", "bla", message).Wait();
            }
        }
    }
}
