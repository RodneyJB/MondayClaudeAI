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
        body { font-family: Arial; padding: 20px; background:#f7f8fa; }
        .box { background:white; border:1px solid #ddd; border-radius:8px; padding:15px; margin-top:15px; }
        .grid { display:grid; grid-template-columns: 1fr 1fr 1fr; gap:15px; }
        .column-card { background:#fff; border:1px solid #ddd; border-radius:8px; padding:12px; }
        .item { padding:8px; border-bottom:1px solid #eee; font-size:13px; }
        .normal { color:#111827; }
        .mirror { color:#b45309; font-weight:bold; }
        .relation { color:#2563eb; font-weight:bold; }
        select, input, textarea { width:100%; padding:8px; box-sizing:border-box; }
        button { padding:8px 14px; cursor:pointer; }
    </style>
</head>

<body>

<h1>Claude AI Setup</h1>

<div class='box'>
    <strong>Current Board ID:</strong>
    <span id='boardIdText'>Loading...</span>
    <br><br>
    <button onclick='loadBoard()'>Load Board Columns</button>
</div>

<div class='box'>
    <h2>Board Structure</h2>

    <div class='grid'>
        <div class='column-card'>
            <h3>Direct Board Columns</h3>
            <div id='directColumns'>Click Load Board Columns</div>
        </div>

        <div class='column-card'>
            <h3>Mirror Columns</h3>
            <div id='mirrorColumns'>Click Load Board Columns</div>
        </div>

        <div class='column-card'>
            <h3>Connected Board Columns</h3>
            <div id='relationColumns'>Click Load Board Columns</div>
        </div>
    </div>
</div>

<div class='box'>
    <h2>Create AI Task</h2>

    <label>Task Name</label><br>
    <input id='taskName' placeholder='Read registration document'><br><br>

    <label>Trigger / Source Column</label><br>
    <select id='sourceColumn'></select><br><br>

    <label>AI Instruction</label><br>
    <textarea id='aiInstruction' style='height:100px' placeholder='Read the uploaded document and extract registration number and expiry date'></textarea><br><br>

    <label>Output Column</label><br>
    <select id='outputColumn'></select><br><br>

    <button onclick='saveTask()'>Save AI Task</button>

    <pre id='taskResult'></pre>
</div>

<pre id='contextHidden' style='display:none'></pre>

<script>
    const monday = window.mondaySdk();

    let boardId = null;
    let allColumns = [];

    monday.listen('context', function(res) {
        boardId = res.data.boardId;
        document.getElementById('boardIdText').innerText = boardId;
        document.getElementById('contextHidden').innerText = JSON.stringify(res.data, null, 2);
    });

    async function loadBoard() {
        if (boardId == null) {
            alert('Board ID not loaded yet');
            return;
        }

        const response = await fetch('/test-columns/' + boardId);
        const data = await response.json();

        allColumns = data.data.boards[0].columns;

        renderColumnGroups(allColumns);
        fillDropdowns(allColumns);
    }

    function renderColumnGroups(columns) {
        const direct = columns.filter(c =>
            c.type !== 'mirror' &&
            c.type !== 'board_relation'
        );

        const mirrors = columns.filter(c => c.type === 'mirror');

        const relations = columns.filter(c => c.type === 'board_relation');

        document.getElementById('directColumns').innerHTML =
            renderColumnList(direct, 'normal');

        document.getElementById('mirrorColumns').innerHTML =
            renderColumnList(mirrors, 'mirror');

        document.getElementById('relationColumns').innerHTML =
            renderColumnList(relations, 'relation');
    }

    function renderColumnList(columns, css) {
        if (columns.length === 0) {
            return '<em>No columns found</em>';
        }

        let html = '';

        columns.forEach(col => {
            html += `
                <div class='item ${css}'>
                    <strong>${col.title}</strong><br>
                    ID: ${col.id}<br>
                    Type: ${col.type}
                </div>
            `;
        });

        return html;
    }

    function fillDropdowns(columns) {
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

            if (col.type !== 'mirror') {
                output.innerHTML += `
                    <option value='${col.id}'>
                        ${col.title} (${col.type}) - ${col.id}
                    </option>
                `;
            }
        });
    }

    function saveTask() {
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