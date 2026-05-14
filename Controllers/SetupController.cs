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
        .two-col { display:grid; grid-template-columns:1fr 1fr; gap:15px; }
        .type-badge { font-size:12px; color:#555; }
        .sample-table { width:100%; border-collapse:collapse; font-size:13px; margin-top:10px; }
        .sample-table th { background:#f1f5f9; text-align:left; padding:6px 8px; border:1px solid #ddd; }
        .sample-table td { padding:6px 8px; border:1px solid #eee; vertical-align:top; max-width:200px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
        .sample-table tr:hover td { background:#f8fafc; }
        .selected-row td { background:#eff6ff !important; }
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

<div class='two-col'>
    <div>
        <label>Trigger Type</label>
        <select id='triggerType'></select>
    </div>

    <div>
        <label>Trigger Column</label>
        <select id='triggerColumn'></select>
    </div>
</div>

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
    <div style='display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:10px;'>
        <h2 style='margin:0;'>Board Structure</h2>
        <div style='display:flex; align-items:center; gap:10px; flex-wrap:wrap;'>
            <button onclick='loadSampleItems()'>Load Sample Items</button>
            <div id='itemSelectorWrap' style='display:none; align-items:center; gap:8px;'>
                <label style='font-size:13px; color:#555; margin:0;'>Select Item:</label>
                <select id='itemSelector' onchange='selectSampleItemByIndex(this.value)' style='width:auto; min-width:180px; padding:6px 8px; margin:0;'>
                    <option value=''>-- Choose an item --</option>
                </select>
            </div>
        </div>
    </div>

    <div class='grid' style='margin-top:15px;'>
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

    <div id='sampleBox' style='display:none; margin-top:15px; border-top:1px solid #eee; padding-top:15px;'>
        <p style='margin:0 0 8px 0; color:#555; font-size:13px;'>Click a row to populate column values.</p>
        <div style='overflow-x:auto'>
            <table class='sample-table' id='sampleTable'>
                <thead id='sampleHead'></thead>
                <tbody id='sampleBody'></tbody>
            </table>
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

    let sampleItems = [];

    async function loadSampleItems() {
        if (boardId == null) {
            alert('Board ID not loaded yet');
            return;
        }

        // load columns too if not already loaded
        if (allColumns.length === 0) {
            await loadBoard();
        }

        const response = await fetch('/test-items/' + boardId);
        const data = await response.json();

        sampleItems = data.data.boards[0].items_page.items;

        renderSampleTable(sampleItems);
        document.getElementById('sampleBox').style.display = 'block';

        // populate item selector dropdown
        const selector = document.getElementById('itemSelector');
        selector.innerHTML = '<option value="">-- Choose an item --</option>';
        sampleItems.forEach((item, idx) => {
            const opt = document.createElement('option');
            opt.value = idx;
            opt.text = item.name;
            selector.appendChild(opt);
        });
        const wrap = document.getElementById('itemSelectorWrap');
        wrap.style.display = 'flex';
    }

    function selectSampleItemByIndex(idxStr) {
        if (idxStr === '') return;
        const idx = parseInt(idxStr, 10);
        selectSampleItem(idx);

        // scroll to sample table
        document.getElementById('sampleBox').scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function renderSampleTable(items) {
        if (items.length === 0) {
            document.getElementById('sampleBody').innerHTML = '<tr><td>No items found</td></tr>';
            return;
        }

        // Build header from columns (show first 8 non-mirror columns for readability)
        const visibleCols = allColumns
            .filter(c => c.type !== 'mirror' && c.type !== 'board_relation')
            .slice(0, 8);

        let headHtml = '<tr><th>#</th><th>Name</th>';
        visibleCols.forEach(c => {
            headHtml += `<th>${getTypeIcon(c.type)} ${c.title}</th>`;
        });
        headHtml += '</tr>';
        document.getElementById('sampleHead').innerHTML = headHtml;

        let bodyHtml = '';
        items.forEach((item, idx) => {
            bodyHtml += `<tr onclick='selectSampleItem(${idx})' style='cursor:pointer'>`;
            bodyHtml += `<td>${idx + 1}</td><td><strong>${item.name}</strong></td>`;
            visibleCols.forEach(c => {
                const cv = item.column_values.find(v => v.id === c.id);
                const val = cv ? (cv.text || '') : '';
                bodyHtml += `<td title='${val.split(String.fromCharCode(39)).join(""&apos;"")}'>${val}</td>`;
            });
            bodyHtml += '</tr>';
        });
        document.getElementById('sampleBody').innerHTML = bodyHtml;
    }

    function selectSampleItem(idx) {
        const item = sampleItems[idx];

        // highlight selected row
        document.querySelectorAll('#sampleBody tr').forEach((r, i) => {
            r.classList.toggle('selected-row', i === idx);
        });

        // keep dropdown in sync
        const selector = document.getElementById('itemSelector');
        if (selector) selector.value = idx;

        // populate column values into the board structure list
        allColumns.forEach(col => {
            const el = document.getElementById('col-val-' + col.id);
            if (!el) return;
            const cv = item.column_values.find(v => v.id === col.id);
            const val = cv ? (cv.text || '') : '';
            el.innerText = val;
            el.title = val;
        });

        // also show the name
        const nameEl = document.getElementById('col-val-name');
        if (nameEl) nameEl.innerText = item.name;
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

    function getTypeIcon(type) {
    const icons = {
        name: '🔤',
        text: '🔤',
        long_text: '📝',
        numbers: '🔢',
        numeric: '🔢',
        date: '📅',
        timeline: '📆',
        hour: '⏰',
        time_tracking: '⏱️',
        status: '🟦',
        color: '🟦',
        dropdown: '⬇️',
        people: '👤',
        person: '👤',
        file: '📎',
        files: '📎',
        mirror: '🪞',
        board_relation: '🔗',
        connect_boards: '🔗🔗',
        email: '✉️',
        phone: '☎️',
        location: '📍',
        link: '🔗',
        checkbox: '☑️',
        formula: '🧮',
        item_id: '🆔',
        subtasks: '📌',
        auto_number: '#️⃣',
        country: '🌍',
        rating: '⭐',
        vote: '🗳️',
        week: '📅',
        world_clock: '🌐'
    };

    return icons[type] || '▫️';
}

function renderColumnList(columns, css) {
    if (columns.length === 0) {
        return '<em>No columns found</em>';
    }

    let html = '';

    columns.forEach(col => {
        html += `
            <div class='item ${css}' id='col-item-${col.id}'>
                <strong>${col.title}</strong>
                <span id='col-val-${col.id}' style='float:right;font-size:12px;color:#374151;max-width:55%;text-align:right;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;'></span><br>
                <span class='type-badge'>
                    ${getTypeIcon(col.type)} ${col.type}
                </span>
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
                    ${getTypeIcon(col.type)} ${col.title}
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
                option.text = `${getTypeIcon(col.type)} ${col.title}`;
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
                ContentType = "text/html; charset=utf-8"
            };
        }
    }
}