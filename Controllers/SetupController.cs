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
        * { box-sizing: border-box; }
        body { font-family: Arial, sans-serif; margin:0; padding:0; background:#f0f2f5; display:flex; height:100vh; overflow:hidden; }

        /* ── Left Sidebar ── */
        #sidebar {
            width: 240px;
            min-width: 240px;
            background: #1e2a3a;
            color: #c9d6e3;
            display: flex;
            flex-direction: column;
            height: 100vh;
            overflow: hidden;
        }
        #sidebar .sidebar-header {
            padding: 18px 16px 12px;
            border-bottom: 1px solid #2e3f52;
        }
        #sidebar .sidebar-header h2 {
            margin: 0 0 4px 0;
            font-size: 15px;
            color: #fff;
            font-weight: 700;
            letter-spacing: 0.3px;
        }
        #sidebar .sidebar-header p {
            margin: 0;
            font-size: 11px;
            color: #6b8099;
        }
        #sidebar .new-btn {
            margin: 12px 16px;
            padding: 9px 14px;
            background: #2563eb;
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-size: 13px;
            font-weight: 600;
            width: calc(100% - 32px);
            text-align: left;
            display: flex;
            align-items: center;
            gap: 8px;
            transition: background 0.15s;
        }
        #sidebar .new-btn:hover { background: #1d4ed8; }
        #sidebar .section-label {
            padding: 8px 16px 4px;
            font-size: 10px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 1px;
            color: #4a6580;
        }
        #setup-list {
            flex: 1;
            overflow-y: auto;
            padding: 4px 8px 16px;
        }
        .setup-item {
            padding: 10px 10px;
            border-radius: 6px;
            cursor: pointer;
            margin-bottom: 2px;
            transition: background 0.12s;
            display: flex;
            align-items: flex-start;
            gap: 10px;
        }
        .setup-item:hover { background: #2e3f52; }
        .setup-item.active { background: #2563eb22; border-left: 3px solid #2563eb; padding-left: 7px; }
        .setup-item .setup-icon { font-size: 16px; margin-top: 1px; flex-shrink: 0; }
        .setup-item .setup-text .setup-name { font-size: 13px; color: #dde5f0; font-weight: 600; }
        .setup-item .setup-text .setup-meta { font-size: 11px; color: #4a6580; margin-top: 2px; }
        .setup-empty { padding: 16px 10px; font-size: 12px; color: #4a6580; font-style: italic; }

        /* ── Main Content ── */
        #main {
            flex: 1;
            display: flex;
            flex-direction: column;
            overflow: hidden;
        }
        #topbar {
            background: white;
            border-bottom: 1px solid #e2e8f0;
            padding: 12px 20px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
        }
        #topbar h1 { margin: 0; font-size: 18px; color: #1e2a3a; font-weight: 700; }
        #topbar .board-info { font-size: 12px; color: #64748b; display: flex; align-items: center; gap: 6px; }
        #topbar .board-info strong { color: #1e2a3a; }
        #topbar .top-actions { display: flex; gap: 8px; }

        #content-area {
            flex: 1;
            overflow-y: auto;
            padding: 20px;
        }

        /* ── Boxes ── */
        .box { background:white; border:1px solid #e2e8f0; border-radius:10px; padding:20px; margin-bottom:16px; }
        .box h2 { margin:0 0 16px 0; font-size:16px; color:#1e2a3a; font-weight:700; border-bottom:1px solid #f1f5f9; padding-bottom:10px; }
        .box h3 { font-size:13px; color:#374151; margin:16px 0 6px 0; font-weight:700; }

        /* ── Form elements ── */
        select, input, textarea { width:100%; padding:8px 10px; box-sizing:border-box; margin-top:4px; border:1px solid #d1d5db; border-radius:6px; font-size:13px; color:#1e2a3a; background:#fff; }
        select:focus, input:focus, textarea:focus { outline:none; border-color:#2563eb; box-shadow: 0 0 0 3px #2563eb18; }
        label { font-size:12px; font-weight:600; color:#6b7280; text-transform:uppercase; letter-spacing:0.4px; }

        /* ── Buttons ── */
        button { padding:8px 16px; cursor:pointer; border:none; border-radius:6px; font-size:13px; font-weight:600; background:#f1f5f9; color:#374151; transition: all 0.15s; }
        button:hover { background:#e2e8f0; }
        .btn-primary { background:#2563eb; color:white; }
        .btn-primary:hover { background:#1d4ed8; }
        .btn-success { background:#16a34a; color:white; }
        .btn-success:hover { background:#15803d; }

        /* ── Layout helpers ── */
        .grid { display:grid; grid-template-columns:1fr 1fr 1fr; gap:15px; }
        .two-col { display:grid; grid-template-columns:1fr 1fr; gap:15px; }
        .mapping-row { display:grid; grid-template-columns:1fr 1fr 80px; gap:10px; margin-bottom:8px; align-items:end; }

        /* ── Column items ── */
        .item { padding:8px 10px; border-bottom:1px solid #f1f5f9; font-size:13px; }
        .mirror { color:#b45309; }
        .relation { color:#2563eb; }
        .normal { color:#111827; }
        .type-badge { font-size:11px; color:#9ca3af; margin-top:3px; }
        .empty-value { color:#9ca3af; }

        /* ── Sample table ── */
        .sample-table { width:100%; border-collapse:collapse; font-size:12px; margin-top:10px; }
        .sample-table th { background:#f8fafc; text-align:left; padding:6px 8px; border:1px solid #e2e8f0; font-size:11px; color:#6b7280; text-transform:uppercase; letter-spacing:0.4px; }
        .sample-table td { padding:6px 8px; border:1px solid #f1f5f9; vertical-align:top; max-width:180px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
        .sample-table tr:hover td { background:#f8fafc; }
        .selected-row td { background:#eff6ff !important; }

        /* ── Status badge ── */
        .badge { display:inline-block; padding:2px 8px; border-radius:12px; font-size:11px; font-weight:600; }
        .badge-blue { background:#dbeafe; color:#1d4ed8; }
        .badge-green { background:#dcfce7; color:#15803d; }
    </style>
</head>
<body>

<!-- LEFT SIDEBAR -->
<div id='sidebar'>
    <div class='sidebar-header'>
        <h2>🤖 Claude AI</h2>
        <p>Monday.com Automations</p>
    </div>

    <button class='new-btn' onclick='newSetup()'>
        ＋ New Setup
    </button>

    <div class='section-label'>Saved Setups</div>

    <div id='setup-list'>
        <div class='setup-empty' id='setup-empty-msg'>No setups yet. Click New Setup to get started.</div>
    </div>
</div>

<!-- MAIN CONTENT -->
<div id='main'>

    <!-- TOP BAR -->
    <div id='topbar'>
        <h1 id='page-title'>New AI Task</h1>
        <div class='board-info' style='display:flex; align-items:center; gap:8px; flex-wrap:wrap;'>
            Board ID: <strong id='boardIdText'>Loading...</strong>
            <input id='manualBoardId' style='display:none;'>
        </div>
    </div>

    <!-- SCROLLABLE CONTENT -->
    <div id='content-area'>

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

    <div style='display:flex; gap:10px; margin-top:16px; align-items:center; flex-wrap:wrap;'>
        <button class='btn-success' onclick='saveSetup()'>💾 Save Setup</button>
        <button class='btn-primary' onclick='prepareTask()'>▶ Prepare AI Task</button>
        <button onclick='newSetup()' style='margin-left:auto;'>✕ Clear Form</button>
    </div>

    <pre id='taskResult' style='background:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:12px; font-size:12px; margin-top:12px; display:none;'></pre>
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
            <h3>Connected Board Columns</h3>
            <div id='relationColumns'>Click Load Board Columns</div>
        </div>

        <div>
            <h3>Mirror Columns</h3>
            <div id='mirrorColumns'>Click Load Board Columns</div>
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

    </div><!-- /content-area -->
</div><!-- /main -->

<pre id='contextHidden' style='display:none'></pre>

<script>
    const monday = window.mondaySdk();

    let boardId = null;
    let allColumns = [];
    let savedSetups = JSON.parse(localStorage.getItem('claudeAiSetups') || '[]');
    let editingSetupId = null;

    renderSidebarList();

    function renderSidebarList() {
        const list = document.getElementById('setup-list');
        const emptyMsg = document.getElementById('setup-empty-msg');

        const existing = list.querySelectorAll('.setup-item');
        existing.forEach(e => e.remove());

        if (savedSetups.length === 0) {
            emptyMsg.style.display = 'block';
            return;
        }
        emptyMsg.style.display = 'none';

        savedSetups.forEach(s => {
            const el = document.createElement('div');
            el.className = 'setup-item' + (s.id === editingSetupId ? ' active' : '');
            el.dataset.id = s.id;
            el.onclick = () => loadSetupIntoForm(s.id);
            el.innerHTML = `
                <div class='setup-icon'>🤖</div>
                <div class='setup-text'>
                    <div class='setup-name'>${escapeHtml(s.taskName || 'Untitled')}</div>
                    <div class='setup-meta'>${escapeHtml(s.trigger ? s.trigger.type : '')} &bull; ${s.outputs ? s.outputs.length : 0} output(s)</div>
                </div>
            `;
            list.appendChild(el);
        });
    }

    function saveSetup() {
        const mappings = [];
        document.querySelectorAll('.mapping-row').forEach(row => {
            const inputs = row.querySelectorAll('input, select');
            if (inputs[0].value.trim()) {
                mappings.push({ aiField: inputs[0].value, mondayColumnId: inputs[1].value });
            }
        });

        const taskName = document.getElementById('taskName').value.trim();
        if (!taskName) { alert('Please enter a Task Name before saving.'); return; }

        const setup = {
            id: editingSetupId || ('setup-' + Date.now()),
            boardId: boardId,
            taskName: taskName,
            trigger: { type: document.getElementById('triggerType').value, columnId: document.getElementById('triggerColumn').value },
            input: { type: document.getElementById('inputType').value, columnId: document.getElementById('inputColumn').value },
            condition: { type: document.getElementById('conditionType').value, columnId: document.getElementById('conditionColumn').value, value: document.getElementById('conditionValue').value },
            ai: { provider: 'Claude', instruction: document.getElementById('aiInstruction').value },
            outputs: mappings,
            savedAt: new Date().toLocaleString()
        };

        if (editingSetupId) {
            const idx = savedSetups.findIndex(s => s.id === editingSetupId);
            if (idx >= 0) savedSetups[idx] = setup;
        } else {
            savedSetups.unshift(setup);
        }

        editingSetupId = setup.id;
        localStorage.setItem('claudeAiSetups', JSON.stringify(savedSetups));
        renderSidebarList();
        document.getElementById('page-title').innerText = setup.taskName;
        alert('Setup saved!');
    }

    function loadSetupIntoForm(id) {
        const s = savedSetups.find(x => x.id === id);
        if (!s) return;

        editingSetupId = id;
        document.getElementById('page-title').innerText = s.taskName || 'AI Task';
        document.getElementById('taskName').value = s.taskName || '';
        document.getElementById('aiInstruction').value = s.ai ? s.ai.instruction : '';
        document.getElementById('conditionValue').value = s.condition ? s.condition.value : '';

        if (s.trigger) {
            const tt = document.getElementById('triggerType');
            if (tt) setSelectValue(tt, s.trigger.type);
        }
        if (s.input) {
            const it = document.getElementById('inputType');
            if (it) setSelectValue(it, s.input.type);
        }
        if (s.condition) {
            const ct = document.getElementById('conditionType');
            if (ct) setSelectValue(ct, s.condition.type);
        }

        document.getElementById('mappingRows').innerHTML = '';
        if (s.outputs && s.outputs.length > 0) {
            s.outputs.forEach(m => addMappingRow(m.aiField, m.mondayColumnId));
        } else {
            addMappingRow();
        }

        renderSidebarList();
        document.getElementById('content-area').scrollTo({ top: 0, behavior: 'smooth' });
    }

    function setSelectValue(select, val) {
        for (let i = 0; i < select.options.length; i++) {
            if (select.options[i].value === val) { select.selectedIndex = i; return; }
        }
    }

    function newSetup() {
        editingSetupId = null;
        document.getElementById('page-title').innerText = 'New AI Task';
        document.getElementById('taskName').value = '';
        document.getElementById('aiInstruction').value = '';
        document.getElementById('conditionValue').value = '';
        document.getElementById('mappingRows').innerHTML = '';
        document.getElementById('taskResult').style.display = 'none';
        addMappingRow();
        renderSidebarList();
        document.getElementById('content-area').scrollTo({ top: 0, behavior: 'smooth' });
    }

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

    function normalizeBoardId(value) {
        if (value == null) return null;
        var str = String(value).trim();
        if (!str) return null;
        var digits = str.replace(/[^0-9]/g, '');
        return digits || null;
    }

    function extractBoardId(payload) {
        if (!payload) return null;

        // common monday sdk shapes
        var id =
            payload.boardId ||
            payload.board_id ||
            (payload.board && payload.board.id) ||
            (payload.boards && payload.boards[0] && payload.boards[0].id) ||
            (payload.boardIds && payload.boardIds[0]) ||
            (payload.board_ids && payload.board_ids[0]) ||
            null;

        if (id) return normalizeBoardId(id);

        // sometimes context is nested in payload.data
        if (payload.data && payload.data !== payload) {
            return extractBoardId(payload.data);
        }

        return null;
    }

    function extractBoardIdFromUrl() {
        try {
            var params = new URLSearchParams(window.location.search);

            var direct =
                params.get('boardId') ||
                params.get('board_id') ||
                params.get('boardIds') ||
                params.get('board_ids');

            if (direct) {
                // handle list-like values too
                var first = String(direct).split(',')[0];
                var normalized = normalizeBoardId(first);
                if (normalized) return normalized;
            }

            // monday often sends encoded context in query param
            var encodedContext = params.get('context');
            if (encodedContext) {
                try {
                    var decoded = atob(encodedContext);
                    var obj = JSON.parse(decoded);
                    var fromContext = extractBoardId(obj);
                    if (fromContext) return fromContext;
                } catch (e) {
                    // ignore decode failures
                }
            }
        } catch (e) {
            // ignore url parsing failures
        }

        return null;
    }

    function applyBoardId(id) {
        var normalized = normalizeBoardId(id);
        if (!normalized || normalized === boardId) return;
        boardId = normalized;
        document.getElementById('boardIdText').innerText = boardId;
        document.getElementById('manualBoardId').value = boardId;
        loadBoard();
    }

    // monday.listen fires when context is ready inside the iframe
    monday.listen('context', function(res) {
        var id = extractBoardId(res);
        applyBoardId(id);
        document.getElementById('contextHidden').innerText = JSON.stringify(res, null, 2);
    });

    // Also poll monday.get('context') with retries - SDK sometimes needs a moment
    function pollContext(attempts) {
        monday.get('context').then(function(res) {
            var id = extractBoardId(res);
            if (id) {
                applyBoardId(id);
            } else if (attempts > 0) {
                setTimeout(function() { pollContext(attempts - 1); }, 800);
            } else {
                document.getElementById('boardIdText').innerText = 'Not detected - enter ID manually';
            }
            document.getElementById('contextHidden').innerText = JSON.stringify(res, null, 2);
        }).catch(function() {
            if (attempts > 0) setTimeout(function() { pollContext(attempts - 1); }, 800);
        });
    }

    // immediate URL fallback for cases where sdk context is delayed/missing
    var urlBoardId = extractBoardIdFromUrl();
    if (urlBoardId) {
        applyBoardId(urlBoardId);
    }

    // Start polling after a short delay to let the SDK initialise
    setTimeout(function() { pollContext(10); }, 500);

    fillStaticDropdowns();
    addMappingRow();

    function fillStaticDropdowns() {
        var trigger = document.getElementById('triggerType');
        var condition = document.getElementById('conditionType');

        triggerTypes.forEach(function(t) {
            var opt = document.createElement('option');
            opt.value = t;
            opt.text = t;
            trigger.appendChild(opt);
        });

        conditionTypes.forEach(function(c) {
            var opt = document.createElement('option');
            opt.value = c;
            opt.text = c;
            condition.appendChild(opt);
        });
    }

    let sampleItems = [];
    let selectedSampleItem = null;

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
        const raw = await response.text();
        let data;

        try {
            data = JSON.parse(raw);
        } catch {
            alert('Item load failed: invalid server response');
            return;
        }

        if (!response.ok) {
            const serverError = (data && (data.error || data.message)) || raw;
            alert('Item load failed: ' + serverError);
            return;
        }

        if (data.errors && data.errors.length > 0) {
            alert('Monday API error: ' + (data.errors[0].message || 'Unknown error'));
            return;
        }

        const board = data && data.data && data.data.boards && data.data.boards[0];
        if (!board || !board.items_page || !board.items_page.items) {
            alert('No item data returned for this board.');
            return;
        }

        sampleItems = board.items_page.items;

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

        // auto-select first item so values show in board columns immediately
        if (sampleItems.length > 0) {
            selector.value = '0';
            selectSampleItem(0);
        }
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
        if (!item) return;

        selectedSampleItem = item;

        // highlight selected row
        document.querySelectorAll('#sampleBody tr').forEach((r, i) => {
            r.classList.toggle('selected-row', i === idx);
        });

        // keep dropdown in sync
        const selector = document.getElementById('itemSelector');
        if (selector) selector.value = idx;

        // re-render column groups with selected item values
        renderColumnGroups(allColumns);
    }

    function manualLoad() {
        var val = document.getElementById('manualBoardId').value.trim();
        if (val) {
            boardId = val;
            document.getElementById('boardIdText').innerText = boardId;
        }
        loadBoard();
    }

    async function loadBoard() {
        if (boardId == null) {
            var val = document.getElementById('manualBoardId').value.trim();
            if (val) { boardId = val; document.getElementById('boardIdText').innerText = boardId; }
            else { document.getElementById('boardIdText').innerText = 'No board ID - enter one above'; return; }
        }

        const btn = document.querySelector('#topbar button');
        if (btn) { btn.innerText = '⏳ Loading...'; btn.disabled = true; }

        try {
            const response = await fetch('/test-columns/' + boardId);
            const raw = await response.text();
            let data;

            try {
                data = JSON.parse(raw);
            } catch {
                document.getElementById('boardIdText').innerText = 'Invalid server response';
                if (btn) { btn.innerText = '↺ Load Board'; btn.disabled = false; }
                alert('Board load failed: invalid JSON response from server.');
                return;
            }

            if (!response.ok) {
                const serverError = (data && (data.error || data.message)) || raw;
                document.getElementById('boardIdText').innerText = 'Board load failed';
                if (btn) { btn.innerText = '↺ Load Board'; btn.disabled = false; }
                alert('Board load failed: ' + serverError);
                return;
            }

            if (data.errors && data.errors.length > 0) {
                document.getElementById('boardIdText').innerText = 'Monday API error';
                if (btn) { btn.innerText = '↺ Load Board'; btn.disabled = false; }
                alert('Monday API error: ' + (data.errors[0].message || 'Unknown error'));
                return;
            }

            const board = data && data.data && data.data.boards && data.data.boards[0];
            if (!board || !board.columns) {
                document.getElementById('boardIdText').innerText = 'No board data returned';
                if (btn) { btn.innerText = '↺ Load Board'; btn.disabled = false; }
                alert('No board data returned. Check board ID and token permissions.');
                return;
            }

            allColumns = board.columns;

            renderColumnGroups(allColumns);
            fillColumnDropdowns(allColumns);

            // only add a mapping row if none exist yet
            if (document.getElementById('mappingRows').children.length === 0) {
                addMappingRow();
            }

            if (btn) { btn.innerText = '✓ Board Loaded'; btn.disabled = false; }
        } catch(e) {
            console.error(e);
            if (btn) { btn.innerText = '↺ Load Board'; btn.disabled = false; }
        }
    }

    function renderColumnGroups(columns) {
        const direct = columns.filter(c =>
            c.type !== 'mirror' &&
            c.type !== 'board_relation'
        );

        const mirrors = columns.filter(c => c.type === 'mirror');
        const relations = columns.filter(c => c.type === 'board_relation');

        document.getElementById('directColumns').innerHTML = renderColumnList(direct, 'normal', selectedSampleItem);
        document.getElementById('mirrorColumns').innerHTML = renderColumnList(mirrors, 'mirror', selectedSampleItem);
        document.getElementById('relationColumns').innerHTML = renderColumnList(relations, 'relation', selectedSampleItem);
    }

    function escapeHtml(value) {
        return (value || '')
            .toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/'/g, '&#39;');
    }

    function readDynamicValue(obj) {
        if (obj == null) return '';

        if (typeof obj === 'string' || typeof obj === 'number' || typeof obj === 'boolean') {
            return String(obj);
        }

        if (Array.isArray(obj)) {
            return obj.map(readDynamicValue).filter(v => v !== '').join(', ');
        }

        if (typeof obj === 'object') {
            // common monday value shapes
            if (typeof obj.display_value === 'string' && obj.display_value.trim() !== '') return obj.display_value;
            if (typeof obj.text === 'string' && obj.text.trim() !== '') return obj.text;
            if (typeof obj.name === 'string' && obj.name.trim() !== '') return obj.name;
            if (typeof obj.title === 'string' && obj.title.trim() !== '') return obj.title;
            if (typeof obj.email === 'string' && obj.email.trim() !== '') return obj.email;
            if (typeof obj.phone === 'string' && obj.phone.trim() !== '') return obj.phone;
            if (typeof obj.date === 'string' && obj.date.trim() !== '') return obj.date;
            if (typeof obj.url === 'string' && obj.url.trim() !== '') return obj.url;

            if (obj.label) {
                const labelValue = readDynamicValue(obj.label);
                if (labelValue) return labelValue;
            }

            if (obj.labels) {
                const labelsValue = readDynamicValue(obj.labels);
                if (labelsValue) return labelsValue;
            }

            if (obj.personsAndTeams) {
                const peopleValue = readDynamicValue(obj.personsAndTeams);
                if (peopleValue) return peopleValue;
            }

            return JSON.stringify(obj);
        }

        return '';
    }

    function getColumnDisplayValue(item, col) {
        if (!item) return '';
        if (col.id === 'name') return item.name || '';

        const cv = item.column_values.find(v => v.id === col.id);
        if (!cv) return '';

        if (cv.text && cv.text.trim() !== '') {
            return cv.text;
        }

        if (!cv.value) return '';

        try {
            const parsed = JSON.parse(cv.value);
            return readDynamicValue(parsed);
        } catch {
            return cv.value;
        }
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

function renderColumnList(columns, css, selectedItem) {
    if (columns.length === 0) {
        return '<em>No columns found</em>';
    }

    let html = '';

    columns.forEach(col => {
        const value = getColumnDisplayValue(selectedItem, col);
        const safeValue = escapeHtml(value);

        html += `
            <div class='item ${css}' id='col-item-${col.id}'>
                <strong>${col.title}</strong>
                <div id='col-val-${col.id}' title='${safeValue}' style='margin-top:4px;font-size:12px;color:#374151;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;'>${safeValue || '<span class=""empty-value"">(empty)</span>'}</div>
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

    function addMappingRow(prefillAiField, prefillColumnId) {
        const container = document.getElementById('mappingRows');

        const row = document.createElement('div');
        row.className = 'mapping-row';

        const aiField = document.createElement('input');
        aiField.placeholder = 'AI field, example: vin';
        if (prefillAiField) aiField.value = prefillAiField;

        const mondayColumn = document.createElement('select');

        allColumns.forEach(col => {
            if (col.type !== 'mirror') {
                const option = document.createElement('option');
                option.value = col.id;
                option.text = `${getTypeIcon(col.type)} ${col.title}`;
                mondayColumn.appendChild(option);
            }
        });

        if (prefillColumnId) setSelectValue(mondayColumn, prefillColumnId);

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
            mappings.push({ aiField: inputs[0].value, mondayColumnId: inputs[1].value });
        });

        const task = {
            boardId: boardId,
            taskName: document.getElementById('taskName').value,
            trigger: { type: document.getElementById('triggerType').value, columnId: document.getElementById('triggerColumn').value },
            input: { type: document.getElementById('inputType').value, columnId: document.getElementById('inputColumn').value },
            condition: { type: document.getElementById('conditionType').value, columnId: document.getElementById('conditionColumn').value, value: document.getElementById('conditionValue').value },
            ai: { provider: 'Claude', instruction: document.getElementById('aiInstruction').value },
            outputs: mappings
        };

        const pre = document.getElementById('taskResult');
        pre.innerText = JSON.stringify(task, null, 2);
        pre.style.display = 'block';
    }
</script>

</body>
</html>",
                ContentType = "text/html; charset=utf-8"
            };
        }
    }
}