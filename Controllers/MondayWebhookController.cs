using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MondayClaudeAI.Controllers;

[ApiController]
[Route("api/monday")]
public class MondayWebhookController : ControllerBase
{
    [HttpGet]
    public IActionResult Test()
    {
        return Ok("Monday Claude AI Running");
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] JsonElement body)
    {
        try
        {
            Console.WriteLine(body.ToString());

            // Monday challenge check
            if (body.TryGetProperty("challenge", out JsonElement challenge))
            {
                return Ok(new
                {
                    challenge = challenge.GetString()
                });
            }

            return Ok(new
            {
                success = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR:");
            Console.WriteLine(ex.ToString());

            return Ok(new
            {
                success = false,
                error = ex.Message
            });
        }
    }
}