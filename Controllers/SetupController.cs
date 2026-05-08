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
        .grid { display:grid; grid-template-columns:1fr 1fr 1fr; gap:15px; }
        .item { padding:8px; border-bottom:1px solid #eee; font-size:13px; }
        select, input, textarea { width:100%; padding:8px; box-sizing:border-box; margin-top:4px; }
        button { padding:8px 14px; cursor:pointer; margin-top:8px; }
        .mirror { color:#b45309; font-weight:bold; }
        .relation { color:#2563eb; font-weight:bold; }
        .normal { color:#111827; }
        .mapping-row { display:grid; grid-template-columns:1fr 1fr 80px; gap:10px; margin-bottom:8px; }
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
    <h2>Create AI Task</h2>

    <h3>1. Trigger</h3>

    <label>Trigger Type</label>
    <select id='triggerType'></select>

    <br><br>

    <label>Trigger Column</label>
    <select id='triggerColumn'></select>

    <h3>2. Input</h3>

    <label>Input Source Column</label>
    <select id='inputColumn'></select>

    <br><br>

    <label>Input Type</label>
    <select id='inputType'>
        <option value='file'>Uploaded File / Document</option>
        <option value='text'>Text Column</option>
        <option value='update'>Item Updates</option>
        <option value='mirror'>Mirror / Connected Data</option>
    </select>

    <h3>3. AI Task</h3>

    <label>Task Name</label>
    <input id='taskName' placeholder='Read car registration document'>

    <br><br>

    <label>AI Instruction</label>
    <textarea id='aiInstruction' style='height:120px' placeholder='Read the uploaded document. Extract VIN, registration number, registration date, expiry date and owner. Return clean JSON.'></textarea>

    <h3>4. Conditions</h3>

    <label>Condition Type</label>
    <select id='conditionType'></select>

    <br><br>

    <label>Condition Column</label>
    <select id='conditionColumn'></select>

    <br><br>

    <label>Condition Value</label>
    <input id='conditionValue' placeholder='Example: Open / Done / Empty / > 10'>

    <h3>5. Output Mapping</h3>

    <p>Map AI result fields to Monday columns.</p>

    <div id='mappingRows'></div>

    <button onclick='addMappingRow()'>+ Add Mapping</button>

    <br><br>

    <button onclick='prepareTask()'>Prepare AI Task</button>

    <pre id='taskResult'></pre>
</div>

<div class='box'>
    <h2>Board Structure</h2>

    <div class='grid'>
        <div>
            <h3>Direct Board Columns</h3>
            <div id='directColumns'>Click Load Board Columns</div>
        </div>

        <div>
            <h3>Mirror Columns</h3>
            <div id='mirrorColumns'>Click Load Board Columns</div>
        </div>

        <div>
            <h3>Connected Board Columns</h3>
            <div id='relationColumns'>Click Load Board Columns</div>
        </div>
    </div>
</div>

<pre id='contextHidden' style='display:none'></pre>

<script>
    const monday = window.mondaySdk();

    let boardId = null;
    let allColumns = [];

    const triggerTypes = [
        'When item created',
        'When column changes',
        'When status changes',
        'When date arrives',
        'When file uploaded',
        'When update added',
        'Every time period',
        'Manual AI Run',
        'When subitem created'
    ];

    const conditionTypes = [
        'No condition',
        'If status is something',
        'If column is empty',
        'If number meets condition',
        'If item is in group',
        'If person is someone',
        'If dropdown meets condition',
        'If date is before',
        'If mirror column contains value'
    ];

    monday.listen('context', function(res) {
        boardId = res.data.boardId;
        document.getElementById('boardIdText').innerText = boardId;
        document.getElementById('contextHidden').innerText = JSON.stringify(res.data, null, 2);
    });

    fillStaticDropdowns();

    function fillStaticDropdowns() {
        const trigger = document.getElementById('triggerType');
        const condition = document.getElementById('conditionType');

        triggerTypes.forEach(t => {
            trigger.innerHTML += `<option value='${t}'>${t}</option>`;
        });

        conditionTypes.forEach(c => {
            condition.innerHTML += `<option value='${c}'>${c}</option>`;
        });
    }

    async function loadBoard() {
        if (boardId == null) {
            alert('Board ID not loaded yet');
            return;
        }

        const response = await fetch('/test-columns/' + boardId);
        const data = await response.json();

        allColumns = data.data.boards[0].columns;

        renderColumnGroups(allColumns);
        fillColumnDropdowns(allColumns);
        addMappingRow();
    }

    function renderColumnGroups(columns) {
        const direct = columns.filter(c =>
            c.type !== 'mirror' &&
            c.type !== 'board_relation'
        );

        const mirrors = columns.filter(c => c.type === 'mirror');
        const relations = columns.filter(c => c.type === 'board_relation');

        document.getElementById('directColumns').innerHTML = renderColumnList(direct, 'normal');
        document.getElementById('mirrorColumns').innerHTML = renderColumnList(mirrors, 'mirror');
        document.getElementById('relationColumns').innerHTML = renderColumnList(relations, 'relation');
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

    function fillColumnDropdowns(columns) {
        fillSelect('triggerColumn', columns, true);
        fillSelect('inputColumn', columns, true);
        fillSelect('conditionColumn', columns, true);
    }

    function fillSelect(selectId, columns, includeMirrors) {
        const select = document.getElementById(selectId);
        select.innerHTML = '';

        columns.forEach(col => {
            if (!includeMirrors && col.type === 'mirror') {
                return;
            }

            select.innerHTML += `
                <option value='${col.id}'>
                    ${col.title} (${col.type}) - ${col.id}
                </option>
            `;
        });
    }

    function addMappingRow() {
        const container = document.getElementById('mappingRows');

        const row = document.createElement('div');
        row.className = 'mapping-row';

        const aiField = document.createElement('input');
        aiField.placeholder = 'AI field, example: vin';

        const mondayColumn = document.createElement('select');

        allColumns.forEach(col => {
            if (col.type !== 'mirror') {
                const option = document.createElement('option');
                option.value = col.id;
                option.text = `${col.title} (${col.type}) - ${col.id}`;
                mondayColumn.appendChild(option);
            }
        });

        const removeButton = document.createElement('button');
        removeButton.innerText = 'Remove';
        removeButton.onclick = function() {
            row.remove();
        };

        row.appendChild(aiField);
        row.appendChild(mondayColumn);
        row.appendChild(removeButton);

        container.appendChild(row);
    }

    function prepareTask() {
        const mappings = [];

        document.querySelectorAll('.mapping-row').forEach(row => {
            const inputs = row.querySelectorAll('input, select');

            mappings.push({
                aiField: inputs[0].value,
                mondayColumnId: inputs[1].value
            });
        });

        const task = {
            boardId: boardId,
            taskName: document.getElementById('taskName').value,
            trigger: {
                type: document.getElementById('triggerType').value,
                columnId: document.getElementById('triggerColumn').value
            },
            input: {
                type: document.getElementById('inputType').value,
                columnId: document.getElementById('inputColumn').value
            },
            condition: {
                type: document.getElementById('conditionType').value,
                columnId: document.getElementById('conditionColumn').value,
                value: document.getElementById('conditionValue').value
            },
            ai: {
                provider: 'Claude',
                instruction: document.getElementById('aiInstruction').value
            },
            outputs: mappings
        };

        document.getElementById('taskResult').innerText =
            JSON.stringify(task, null, 2);

        alert('AI task prepared. Next step is saving this task to the backend.');
    }
</script>

</body>
</html>",
                ContentType = "text/html"
            };
        }
    }
}