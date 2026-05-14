using Microsoft.AspNetCore.Mvc;
using MondayClaudeAI.Services;

namespace MondayClaudeAI.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly AiService _ai;

    public class AiRunRequest
    {
        public string SystemPrompt { get; set; } = "";
        public string UserPrompt { get; set; } = "";
        public double Temperature { get; set; } = 0.2;
    }

    public AiController(AiService ai)
    {
        _ai = ai;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            configured = _ai.IsConfigured(),
            model = Environment.GetEnvironmentVariable("AI_MODEL") ?? "gpt-4o-mini"
        });
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] AiRunRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserPrompt))
        {
            return BadRequest(new { error = "userPrompt is required." });
        }

        try
        {
            var output = await _ai.RunPrompt(request.SystemPrompt, request.UserPrompt, request.Temperature);
            return Ok(new
            {
                success = true,
                output
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message
            });
        }
    }
}
