using Microsoft.AspNetCore.SignalR;
using System.Net.WebSockets;

namespace OKXKripto.BacgroundService
{
    public class WebSocketBackgroundService : IHostedService
    {
        private readonly WebSocketService _webSocketService;
        public WebSocketBackgroundService(IHubContext<PriceHub> hubContext)
        {
            _webSocketService = new WebSocketService(hubContext);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _webSocketService.ConnectAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _webSocketService.DisconnectAsync();
            return Task.CompletedTask;
        }
    }
}
