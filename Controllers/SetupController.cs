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

    <button onclick='loadBoard()'>
        Load Board Columns
    </button>

    <h2>Monday Context</h2>

    <pre id='result'></pre>

    <h2>Board Columns</h2>

    <pre id='columns'></pre>

    <script>

        const monday = window.mondaySdk();

        let boardId = null;

        monday.listen('context', function(res)
        {
            boardId = res.data.boardId;

            document.getElementById('result').innerText =
                JSON.stringify(res.data, null, 2);

            console.log(res.data);
        });

        async function loadBoard()
        {
            if(boardId == null)
            {
                alert('Board ID not loaded yet');
                return;
            }

            const response =
                await fetch('/test-columns/' + boardId);

            const data = await response.json();

            document.getElementById('columns').innerText =
                JSON.stringify(
                    data.data.boards[0].columns,
                    null,
                    2
                );
        }

    </script>

</body>

</html>",
                ContentType = "text/html"
            };
        }
    }
}