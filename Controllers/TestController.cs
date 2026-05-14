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
    public async Task<IActionResult> Test(long boardId)
    {
        var token = Environment.GetEnvironmentVariable("MONDAY_API_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            return StatusCode(500, new
            {
                error = "MONDAY_API_TOKEN is missing on the server environment."
            });
        }

        try
        {
            var result = await _monday.GetBoardColumns(boardId, token);
            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message
            });
        }
    }

    [HttpGet("/test-items/{boardId}")]
    public async Task<IActionResult> GetItems(long boardId)
    {
        var token = Environment.GetEnvironmentVariable("MONDAY_API_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            return StatusCode(500, new
            {
                error = "MONDAY_API_TOKEN is missing on the server environment."
            });
        }

        try
        {
            var result = await _monday.GetBoardItems(boardId, token);
            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message
            });
        }
    }
}