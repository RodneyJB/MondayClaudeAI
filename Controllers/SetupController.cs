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

    <style>
        body {
            font-family: Arial;
            padding: 20px;
            background: #f7f8fa;
        }

        h1 {
            margin-bottom: 10px;
        }

        button {
            padding: 8px 14px;
            margin: 5px 0;
            cursor: pointer;
        }

        .box {
            background: white;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 15px;
            margin-top: 15px;
        }

        .column {
            padding: 8px;
            border-bottom: 1px solid #eee;
        }

        .mirror {
            color: #b45309;
            font-weight: bold;
        }

        .relation {
            color: #2563eb;
            font-weight: bold;
        }

        .normal {
            color: #111827;
        }

        pre {
            white-space: pre-wrap;
            font-size: 12px;
        }
    </style>
</head>

<body>

    <h1>Claude AI Setup</h1>

    <div class='box'>
        <strong>Current Board ID:</strong>
        <span id='boardIdText'>Loading...</span>
        <br><br>

        <button onclick='loadBoard()'>
            Load Board Columns
        </button>
    </div>

    <div class='box'>
        <h2>Board Columns</h2>
        <div id='columns'>Click Load Board Columns</div>
    </div>

    <div class='box'>
        <h2>Mirror / Connected Columns</h2>
        <p>
            Mirror columns are read-only. To update them, the app must update the connected source item.
        </p>
        <div id='mirrorColumns'>No data loaded yet</div>
    </div>

    <div class='box'>
        <h2>Create AI Task</h2>

        <label>Task Name</label><br>
        <input id='taskName' style='width:100%;padding:8px' placeholder='Read registration document'><br><br>

        <label>Trigger / Source Column</label><br>
        <select id='sourceColumn' style='width:100%;padding:8px'></select><br><br>

        <label>AI Instruction</label><br>
        <textarea id='aiInstruction' style='width:100%;height:100px;padding:8px' placeholder='Read the uploaded document and extract registration number and expiry date'></textarea><br><br>

        <label>Output Column</label><br>
        <select id='outputColumn' style='width:100%;padding:8px'></select><br><br>

        <button onclick='saveTask()'>
            Save AI Task
        </button>

        <pre id='taskResult'></pre>
    </div>

    <pre id='contextHidden' style='display:none'></pre>

    <script>

        const monday = window.mondaySdk();

        let boardId = null;
        let allColumns = [];

        monday.listen('context', function(res)
        {
            boardId = res.data.boardId;

            document.getElementById('boardIdText').innerText = boardId;

            document.getElementById('contextHidden').innerText =
                JSON.stringify(res.data, null, 2);
        });

        async function loadBoard()
        {
            if(boardId == null)
            {
                alert('Board ID not loaded yet');
                return;
            }

            const response = await fetch('/test-columns/' + boardId);
            const data = await response.json();

            allColumns = data.data.boards[0].columns;

            renderColumns(allColumns);
            renderMirrorColumns(allColumns);
            fillDropdowns(allColumns);
        }

        function renderColumns(columns)
        {
            let html = '';

            columns.forEach(col => {

                let css = 'normal';

                if(col.type === 'mirror')
                {
                    css = 'mirror';
                }

                if(col.type === 'board_relation')
                {
                    css = 'relation';
                }

                html += `
                    <div class='column ${css}'>
                        <strong>${col.title}</strong><br>
                        ID: ${col.id}<br>
                        Type: ${col.type}
                    </div>
                `;
            });

            document.getElementById('columns').innerHTML = html;
        }

        function renderMirrorColumns(columns)
        {
            const filtered = columns.filter(c =>
                c.type === 'mirror' ||
                c.type === 'board_relation'
            );

            if(filtered.length === 0)
            {
                document.getElementById('mirrorColumns').innerHTML =
                    'No mirror or connected-board columns found.';
                return;
            }

            let html = '';

            filtered.forEach(col => {

                let note = '';

                if(col.type === 'mirror')
                {
                    note = 'Read only. Use connected board relation to update source item.';
                }

                if(col.type === 'board_relation')
                {
                    note = 'Connection column. Needed to find linked item and source board.';
                }

                html += `
                    <div class='column'>
                        <strong>${col.title}</strong><br>
                        ID: ${col.id}<br>
                        Type: ${col.type}<br>
                        Note: ${note}
                    </div>
                `;
            });

            document.getElementById('mirrorColumns').innerHTML = html;
        }

        function fillDropdowns(columns)
        {
            const source = document.getElementById('sourceColumn');
            const output = document.getElementById('outputColumn');

            source.innerHTML = '';
            output.innerHTML = '';

            columns.forEach(col => {

                source.innerHTML += `
                    <option value='${col.id}'>
                        ${col.title} (${col.type}) - ${col.id}
                    </option>
                `;

                if(col.type !== 'mirror')
                {
                    output.innerHTML += `
                        <option value='${col.id}'>
                            ${col.title} (${col.type}) - ${col.id}
                        </option>
                    `;
                }
            });
        }

        function saveTask()
        {
            const task = {
                boardId: boardId,
                taskName: document.getElementById('taskName').value,
                sourceColumnId: document.getElementById('sourceColumn').value,
                aiInstruction: document.getElementById('aiInstruction').value,
                outputColumnId: document.getElementById('outputColumn').value
            };

            document.getElementById('taskResult').innerText =
                JSON.stringify(task, null, 2);

            alert('Task prepared. Next step is saving this to backend/database.');
        }

    </script>

</body>

</html>",
                ContentType = "text/html"
            };
        }
    }
}