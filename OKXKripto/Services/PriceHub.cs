using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class PriceHub : Hub
{
    
   public async Task SendPriceUpdate(string message)
    {
        await Clients.All.SendAsync("ReceivePriceUpdate", message);
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Bir istemci bağlandı: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"Bir istemci ayrıldı: {Context.ConnectionId}");
        if (exception != null)
        {
            Console.WriteLine("Ayrılma nedeni: " + exception.Message);
        }
        return base.OnDisconnectedAsync(exception);
    }
    
}
