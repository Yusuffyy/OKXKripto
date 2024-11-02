using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class WebSocketService
{
    private readonly ClientWebSocket _client;
    private readonly IHubContext<PriceHub> _hubContext;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public WebSocketService(IHubContext<PriceHub> hubContext)
    {
        _client = new ClientWebSocket();
        _hubContext = hubContext;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task ConnectAsync()
    {
        string uri = "wss://ws.okx.com:8443/ws/v5/public";
        try
        {
            await _client.ConnectAsync(new Uri(uri), CancellationToken.None);
            Console.WriteLine("WebSocket bağlantısı sağlandı.");
            _ = ReceiveMessagesAsync(); // Mesajları almak için arka planda çalışacak
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bağlantı hatası: {ex.Message}");
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];
        while (_client.State == WebSocketState.Open)
        {
            try
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Mesaj alındı: {message}");
                    await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mesaj alma hatası: {ex.Message}");
            }
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_client.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Mesaj gönderildi: {message}");
        }
        else
        {
            Console.WriteLine("WebSocket bağlantısı açık değil.");
        }
    }

    public async Task DisconnectAsync()
    {
        if (_client.State == WebSocketState.Open)
        {
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
            Console.WriteLine("WebSocket bağlantısı kapatıldı.");
        }
    }

    public void StopReceivingMessages()
    {
        _cancellationTokenSource.Cancel(); // Mesaj alımını durdur
        DisconnectAsync(); // Bağlantıyı kapat
    }
}
