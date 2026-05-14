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

    public class ResolveOriginRequest
    {
        public long BoardId { get; set; }
        public long ItemId { get; set; }
        public string ColumnId { get; set; } = string.Empty;
        public Dictionary<string, object>? ColumnSettings { get; set; }
    }

    public class ResolveOriginResponse
    {
        public string Value { get; set; } = string.Empty;
        public string ColumnType { get; set; } = string.Empty;
        public string FinalBoardId { get; set; } = string.Empty;
        public string FinalColumnId { get; set; } = string.Empty;
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

    [HttpPost("/resolve-origin-value")]
    public async Task<IActionResult> ResolveOriginValue([FromBody] ResolveOriginRequest request)
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

        if (request.ItemId <= 0)
        {
            return BadRequest(new { error = "itemId is required." });
        }

        if (string.IsNullOrWhiteSpace(request.ColumnId))
        {
            return BadRequest(new { error = "columnId is required." });
        }

        try
        {
            var (value, columnType, finalBoardId, finalColumnId) = await _monday.ResolveToOriginValue(
                request.BoardId,
                request.ItemId,
                request.ColumnId,
                request.ColumnSettings,
                token
            );

            return Ok(new ResolveOriginResponse
            {
                Value = value,
                ColumnType = columnType,
                FinalBoardId = finalBoardId,
                FinalColumnId = finalColumnId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message
            });
        }
    }

    [HttpGet("/resolve-mirror-columns/{boardId}/{itemId}")]
    public async Task<IActionResult> ResolveMirrorColumns(long boardId, long itemId)
    {
        var token = Environment.GetEnvironmentVariable("MONDAY_API_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            return StatusCode(500, new
            {
                error = "MONDAY_API_TOKEN is missing on the server environment."
            });
        }

        if (boardId <= 0)
        {
            return BadRequest(new { error = "boardId is required." });
        }

        if (itemId <= 0)
        {
            return BadRequest(new { error = "itemId is required." });
        }

        try
        {
            var resolved = await _monday.ResolveMirrorColumnsForItem(boardId, itemId, token);
            return Ok(resolved);
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