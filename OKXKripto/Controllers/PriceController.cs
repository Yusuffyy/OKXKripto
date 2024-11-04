using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OKXKripto.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class PriceController : ControllerBase
{
    private readonly WebSocketService _webSocketService;
    private readonly HttpClient _httpClient;

    public PriceController(WebSocketService webSocketService, HttpClient httpClient)
    {
        _webSocketService = webSocketService;
        _httpClient = httpClient;
    }
    
    [HttpGet("latest")]
    [Authorize]
    public async Task<IActionResult> GetLatestPrices()
    {
        // OKX API'den son fiyatları almak için gerekli kod
        try
        {
            var response = await _httpClient.GetAsync("https://www.okx.com/api/v5/market/ticker?instId=BTC-USDT");
            response.EnsureSuccessStatusCode(); // İstek başarılı değilse hata fırlatır

            var responseBody = await response.Content.ReadAsStringAsync();
            return Ok(responseBody); // JSON yanıtını döner
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"API isteği başarısız: {ex.Message}");
        }
    }
}
}
