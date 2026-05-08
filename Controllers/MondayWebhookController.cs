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
    public async Task<IActionResult> Webhook([FromBody] dynamic body)
    {
        // Monday verification challenge
        if (body.challenge != null)
        {
            return Ok(new
            {
                challenge = body.challenge
            });
        }

        try
        {
            Console.WriteLine(body);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Ok();
        }
    }
}