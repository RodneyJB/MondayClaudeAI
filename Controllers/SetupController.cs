using Microsoft.AspNetCore.Mvc;

namespace MondayClaudeAI.Controllers;

[ApiController]
[Route("")]
public class SetupController : Controller
{
    [HttpGet("setup")]
    public IActionResult Setup()
    {
        var html = @"
        <html>
        <body style='font-family:Arial;padding:40px'>
            <h1>Claude AI Setup</h1>

            <button onclick='loadBoard()'>
                Load Board Info
            </button>

            <pre id='result'></pre>

            <script>
                async function loadBoard()
                {
                    const res = await fetch('/api/test');
                    const data = await res.text();

                    document.getElementById('result').innerText = data;
                }
            </script>
        </body>
        </html>";

        return Content(html, "text/html");
    }

    [HttpGet("api/test")]
    public IActionResult Test()
    {
        return Ok("Setup page working");
    }
}