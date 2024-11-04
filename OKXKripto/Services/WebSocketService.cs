using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OKXKripto.Models;

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
            await SendMessage("BTC-USDT");
            await SendMessage("ETH-USDT");
            await SendMessage("XRP-USDT");
            await SendMessage("BNB-USDT");
            await SendMessage("SOL-USDT");
            _ = ReceiveMessagesAsync(); // Mesajları almak için arka planda çalışacak
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bağlantı hatası: {ex.Message}");
        }
    }
    public async Task SendMessage(string currency)
    {
        var subscribeMessage = new
        {
            op = "subscribe",
            args = new[]
            {
                new {channel = "tickers", instId= currency }
            }
        };
        var messageJson = JsonConvert.SerializeObject(subscribeMessage);
        var messageBytes = Encoding.UTF8.GetBytes(messageJson);
        await _client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];
        while (_client.State == WebSocketState.Open)
        {
            try
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var OKXObject = JsonConvert.DeserializeObject<CurrencyModel.OkxTickerResponse>(message);

                if (OKXObject != null && OKXObject.Data != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", OKXObject.Data[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mesaj alma hatası: {ex.Message}");
            }
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
