using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace server
{
    public class SomeHub : Hub{
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}