using Microsoft.AspNetCore.Mvc;
using MondayClaudeAI.Services;

namespace MondayClaudeAI.Controllers;

[ApiController]
public class TestController : ControllerBase
{
    private readonly MondayService _monday;

    public TestController(MondayService monday)
    {
        _monday = monday;
    }

    [HttpGet("/test-columns/{boardId}")]
    public async Task<string> Test(long boardId)
    {
        var token = Environment.GetEnvironmentVariable("MONDAY_API_TOKEN");
        return await _monday.GetBoardColumns(boardId, token!);
    }

    [HttpGet("/test-items/{boardId}")]
    public async Task<string> GetItems(long boardId)
    {
        var token = Environment.GetEnvironmentVariable("MONDAY_API_TOKEN");
        return await _monday.GetBoardItems(boardId, token!);
    }
}