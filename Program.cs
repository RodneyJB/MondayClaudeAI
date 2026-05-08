using Microsoft.AspNetCore.Mvc;
using MondayClaudeAI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapControllers();
builder.Services.AddHttpClient<MondayService>();

app.Run();