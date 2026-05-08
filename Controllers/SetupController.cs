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
<head>
    <script src='https://cdn.jsdelivr.net/npm/monday-sdk-js/dist/main.js'></script>
</head>

<body style='font-family:Arial;padding:20px'>

    <h1>Claude AI Setup</h1>

    <button onclick='alert(""Setup Started"")'>
        Setup Board
    </button>

    <h2>Monday Context</h2>

    <pre id='result'></pre>

    <script>
        const monday = window.mondaySdk();

        monday.listen('context', function(res) {

            document.getElementById('result').innerText =
                JSON.stringify(res.data, null, 2);

            console.log(res.data);
        });
    </script>

</body>
</html>",
                ContentType = "text/html"
            };
        }
    }
}