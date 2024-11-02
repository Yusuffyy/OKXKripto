using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using OKXKripto.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Register SignalR
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserService, UserService>();
// If using OkxWebSocketService, uncomment the following line:
// builder.Services.AddSingleton<OkxWebSocketService>();
builder.Services.AddSingleton<WebSocketService>(); // WebSocket service registration
builder.Services.AddSingleton<PriceHub>();
builder.Services.AddHttpClient();
// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("BuradaCokGizliBirAnahtarKeyYazili")), // Use a secure key
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:5173") // Ýstemci URL'si
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // Bu ayar ile kimlik bilgileri (cookies, http authentication) gönderilebilir.
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowSpecificOrigins"); // Ensure CORS is used after routing and before endpoints

app.MapControllers();

app.MapHub<PriceHub>("/priceHub");
app.Run();
