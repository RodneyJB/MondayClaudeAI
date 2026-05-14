using Microsoft.AspNetCore.Mvc;
using MondayClaudeAI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<MondayService>();
builder.Services.AddHttpClient<AiService>();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapGet("/", () => Results.Ok(new { status = "ok", service = "MondayClaudeAI" }));
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));

app.MapControllers();


app.Run();