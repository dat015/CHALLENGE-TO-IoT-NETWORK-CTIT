using System;
using System.Text;
using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors; // Đảm bảo có namespace này
using PlantBlockchainApp.model; // Namespace chứa block_chain

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Đăng ký block_chain như một singleton
builder.Services.AddSingleton<block_chain>();

var app = builder.Build();

// Sử dụng CORS middleware
app.UseCors("AllowAll");

// Kênh để gửi dữ liệu real-time
var channel = Channel.CreateUnbounded<string>();

// Tạo dữ liệu ảo
var blockchain = app.Services.GetService<block_chain>();
var timer = new System.Timers.Timer(3000); // 3 giây/lần
timer.Elapsed += (sender, e) =>
{
    string data = $"{{ \"temp\": {Random.Shared.Next(15, 35)}, \"humidity\": {Random.Shared.Next(30, 90)}, \"light\": {Random.Shared.Next(100, 1000)}, \"soilMoisture\": {Random.Shared.Next(20, 80)} }}";
    blockchain.AddBlock(data);
    channel.Writer.TryWrite(data); // Gửi dữ liệu qua kênh
    Console.WriteLine($"Added data: {data}");
};
timer.Start();

// API endpoints
app.MapGet("/blocks", ([FromServices] block_chain bc) => bc.GetChain());
app.MapGet("/latest", ([FromServices] block_chain bc) => bc.GetLatestBlock());

// Endpoint SSE real-time
app.MapGet("/events", async (HttpContext context) =>
{
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");
    await context.Response.WriteAsync("data: Connected\n\n");
    await context.Response.Body.FlushAsync();

    await foreach (var data in channel.Reader.ReadAllAsync())
    {
        var message = $"data: {data}\n\n";
        await context.Response.WriteAsync(message);
        await context.Response.Body.FlushAsync();
    }
});

app.Run("http://localhost:5000");