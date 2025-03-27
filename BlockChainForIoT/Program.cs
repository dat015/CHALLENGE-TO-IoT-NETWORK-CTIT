

using BlockChainForIoT.blockchain;
using BlockChainForIoT.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Blockchain>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blockchain for IoT API V1");
        c.RoutePrefix = "swagger"; // Truy cập Swagger tại /swagger
    });
}
app.UseAuthorization();
app.MapControllers();



app.Run();