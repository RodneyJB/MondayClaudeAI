using Microsoft.AspNetCore.Mvc;

namespace MondayClaudeAI.Controllers
{
    [ApiController]
    public class SetupController : Controller
    {
        [HttpGet("/setup")]
        public ContentResult Setup()
        {
            return new ContentResult
            {
                Content = @"
                <html>
                <body style='font-family:Arial;padding:20px'>
                    <h1>Claude AI Setup</h1>

                    <button onclick='alert(""Setup Started"")'>
                        Setup Board
                    </button>
                </body>
                </html>",
                ContentType = "text/html"
            };
        }
    }
}