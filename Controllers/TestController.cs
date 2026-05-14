using Microsoft.AspNetCore.Mvc;
using MondayClaudeAI.Services;

namespace MondayClaudeAI.Controllers;

[ApiController]
public class TestController : ControllerBase
{
    private readonly MondayService _monday;

    public class OriginValuesRequest
    {
        public long BoardId { get; set; }
        public List<long> ItemIds { get; set; } = new();
        public List<string> ColumnIds { get; set; } = new();
    }

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

    [HttpPost("/test-origin-values")]
    public async Task<IActionResult> GetOriginValues([FromBody] OriginValuesRequest request)
    {
        var token = Environment.GetEnvironmentVariable("MONDAY_API_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            return StatusCode(500, new
            {
                error = "MONDAY_API_TOKEN is missing on the server environment."
            });
        }

        if (request == null || request.BoardId <= 0)
        {
            return BadRequest(new { error = "boardId is required." });
        }

        if (request.ItemIds == null || request.ItemIds.Count == 0)
        {
            return BadRequest(new { error = "itemIds is required." });
        }

        if (request.ColumnIds == null || request.ColumnIds.Count == 0)
        {
            return BadRequest(new { error = "columnIds is required." });
        }

        try
        {
            var result = await _monday.GetOriginValues(request.BoardId, request.ItemIds, request.ColumnIds, token);
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