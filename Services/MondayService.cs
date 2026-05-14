using System.Text;
using System.Text.Json;
using System.Linq;

namespace MondayClaudeAI.Services;

public class MondayService
{
    private readonly HttpClient _http;

    public MondayService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetBoardColumns(long boardId, string token)
    {
        var query = $@"
        {{
            boards(ids: {boardId}) {{
                columns {{
                    id
                    title
                    type
                    settings
                    settings_str
                }}
            }}
        }}";

        var body = JsonSerializer.Serialize(new
        {
            query
        });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.monday.com/v2"
        );

        request.Headers.Add("Authorization", token);

        request.Content = new StringContent(
            body,
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(request);

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetBoardItems(long boardId, string token)
    {
        var query = $@"
        {{
            boards(ids: {boardId}) {{
                items_page(limit: 50) {{
                    items {{
                        id
                        name
                        column_values {{
                            id
                            type
                            text
                            value
                            ... on FormulaValue {{
                                display_value
                            }}
                            ... on MirrorValue {{
                                display_value
                            }}
                            ... on BoardRelationValue {{
                                display_value
                                linked_items {{
                                    id
                                    name
                                }}
                            }}
                        }}
                    }}
                }}
            }}
        }}";

        var body = JsonSerializer.Serialize(new { query });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.monday.com/v2");
        request.Headers.Add("Authorization", token);
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetOriginValues(long boardId, IEnumerable<long> itemIds, IEnumerable<string> columnIds, string token)
    {
        var safeItemIds = itemIds?.Distinct().ToArray() ?? Array.Empty<long>();
        var safeColumnIds = columnIds?
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? Array.Empty<string>();

        var itemList = string.Join(",", safeItemIds);
        var colList = string.Join(",", safeColumnIds.Select(id => $"\"{id.Replace("\\", "\\\\").Replace("\"", "\\\"")}\""));

        var query = $@"
        {{
            boards(ids: {boardId}) {{
                id
                items(ids: [{itemList}]) {{
                    id
                    name
                    column_values(ids: [{colList}]) {{
                        id
                        type
                        text
                        value
                        ... on FormulaValue {{
                            display_value
                        }}
                        ... on MirrorValue {{
                            display_value
                        }}
                        ... on BoardRelationValue {{
                            display_value
                            linked_items {{
                                id
                                name
                            }}
                        }}
                    }}
                }}
            }}
        }}";

        var body = JsonSerializer.Serialize(new { query });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.monday.com/v2");
        request.Headers.Add("Authorization", token);
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    public sealed class OriginResolvedValue
    {
        public long BoardId { get; set; }
        public long ItemId { get; set; }
        public string ColumnId { get; set; } = string.Empty;
        public string ColumnTitle { get; set; } = string.Empty;
        public string ColumnType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public List<ResolutionHop> Trail { get; set; } = new();
    }

    public sealed class ResolutionHop
    {
        public long BoardId { get; set; }
        public long ItemId { get; set; }
        public string ColumnId { get; set; } = string.Empty;
        public string ColumnTitle { get; set; } = string.Empty;
        public string ColumnType { get; set; } = string.Empty;
    }

    public sealed class MirrorColumnResolved
    {
        public string MirrorColumnId { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public List<OriginResolvedValue> Origins { get; set; } = new();
    }

    public sealed class MirrorResolutionResult
    {
        public long BoardId { get; set; }
        public long ItemId { get; set; }
        public List<MirrorColumnResolved> Mirrors { get; set; } = new();
    }

    private sealed class ColumnMeta
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string SettingsStr { get; set; } = string.Empty;
        public string SettingsJson { get; set; } = string.Empty;
    }

    private sealed class ItemColumnValue
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public List<long> LinkedItemIds { get; set; } = new();
    }

    private sealed class ItemData
    {
        public long Id { get; set; }
        public Dictionary<string, ItemColumnValue> Columns { get; set; } = new(StringComparer.Ordinal);
    }

    private async Task<string> PostQuery(string query, string token)
    {
        var body = JsonSerializer.Serialize(new { query });
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.monday.com/v2");
        request.Headers.Add("Authorization", token);
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _http.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    private static bool IsMirrorLikeType(string? type)
        => string.Equals(type, "mirror", StringComparison.OrdinalIgnoreCase)
        || string.Equals(type, "lookup", StringComparison.OrdinalIgnoreCase);

    private static bool IsRelationLikeType(string? type)
        => string.Equals(type, "board_relation", StringComparison.OrdinalIgnoreCase)
        || string.Equals(type, "connect_boards", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSettingsJson(JsonElement col)
    {
        var settingsStr = col.TryGetProperty("settings_str", out var sEl) ? (sEl.GetString() ?? string.Empty) : string.Empty;
        if (!string.IsNullOrWhiteSpace(settingsStr)) return settingsStr;

        if (!col.TryGetProperty("settings", out var settingsEl) || settingsEl.ValueKind == JsonValueKind.Null || settingsEl.ValueKind == JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        if (settingsEl.ValueKind == JsonValueKind.String)
        {
            return settingsEl.GetString() ?? string.Empty;
        }

        return settingsEl.GetRawText();
    }

    private async Task<Dictionary<string, ColumnMeta>> GetBoardColumnMetaMap(long boardId, string token, Dictionary<long, Dictionary<string, ColumnMeta>> cache)
    {
        if (cache.TryGetValue(boardId, out var cached))
        {
            return cached;
        }

        var raw = await GetBoardColumns(boardId, token);
        using var doc = JsonDocument.Parse(raw);

        var result = new Dictionary<string, ColumnMeta>(StringComparer.Ordinal);

        if (!doc.RootElement.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("boards", out var boards) ||
            boards.ValueKind != JsonValueKind.Array ||
            boards.GetArrayLength() == 0)
        {
            cache[boardId] = result;
            return result;
        }

        var cols = boards[0].TryGetProperty("columns", out var c) ? c : default;
        if (cols.ValueKind == JsonValueKind.Array)
        {
            foreach (var col in cols.EnumerateArray())
            {
                var id = col.TryGetProperty("id", out var idEl) ? (idEl.GetString() ?? string.Empty) : string.Empty;
                if (string.IsNullOrWhiteSpace(id)) continue;

                result[id] = new ColumnMeta
                {
                    Id = id,
                    Title = col.TryGetProperty("title", out var titleEl) ? (titleEl.GetString() ?? string.Empty) : string.Empty,
                    Type = col.TryGetProperty("type", out var tEl) ? (tEl.GetString() ?? string.Empty) : string.Empty,
                    SettingsStr = col.TryGetProperty("settings_str", out var sEl) ? (sEl.GetString() ?? string.Empty) : string.Empty,
                    SettingsJson = NormalizeSettingsJson(col)
                };
            }
        }

        cache[boardId] = result;
        return result;
    }

    private async Task<ItemData?> GetItemData(long boardId, long itemId, string token, Dictionary<string, ItemData> cache)
    {
        var key = $"{boardId}:{itemId}";
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var query = $@"
        {{
            boards(ids: {boardId}) {{
                items(ids: [{itemId}]) {{
                    id
                    column_values {{
                        id
                        type
                        text
                        value
                        ... on FormulaValue {{
                            display_value
                        }}
                        ... on MirrorValue {{
                            display_value
                        }}
                        ... on BoardRelationValue {{
                            display_value
                            linked_items {{
                                id
                            }}
                        }}
                    }}
                }}
            }}
        }}";

        var raw = await PostQuery(query, token);
        using var doc = JsonDocument.Parse(raw);

        if (!doc.RootElement.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("boards", out var boards) ||
            boards.ValueKind != JsonValueKind.Array ||
            boards.GetArrayLength() == 0)
        {
            cache[key] = null!;
            return null;
        }

        var items = boards[0].TryGetProperty("items", out var itemsEl) ? itemsEl : default;
        if (items.ValueKind != JsonValueKind.Array || items.GetArrayLength() == 0)
        {
            cache[key] = null!;
            return null;
        }

        var item = items[0];
        var parsed = new ItemData
        {
            Id = long.TryParse(item.GetProperty("id").GetString(), out var iid) ? iid : itemId,
            Columns = new Dictionary<string, ItemColumnValue>(StringComparer.Ordinal)
        };

        var cvs = item.TryGetProperty("column_values", out var cvEl) ? cvEl : default;
        if (cvs.ValueKind == JsonValueKind.Array)
        {
            foreach (var cv in cvs.EnumerateArray())
            {
                var id = cv.TryGetProperty("id", out var idEl) ? (idEl.GetString() ?? string.Empty) : string.Empty;
                if (string.IsNullOrWhiteSpace(id)) continue;

                var col = new ItemColumnValue
                {
                    Id = id,
                    Type = cv.TryGetProperty("type", out var tEl) ? (tEl.GetString() ?? string.Empty) : string.Empty,
                    Text = cv.TryGetProperty("text", out var txtEl) ? (txtEl.GetString() ?? string.Empty) : string.Empty,
                    DisplayValue = cv.TryGetProperty("display_value", out var dvEl) ? (dvEl.GetString() ?? string.Empty) : string.Empty,
                    Value = cv.TryGetProperty("value", out var vEl) ? (vEl.GetString() ?? string.Empty) : string.Empty,
                    LinkedItemIds = new List<long>()
                };

                if (cv.TryGetProperty("linked_items", out var linked) && linked.ValueKind == JsonValueKind.Array)
                {
                    foreach (var li in linked.EnumerateArray())
                    {
                        var sid = li.TryGetProperty("id", out var liIdEl) ? liIdEl.GetString() : null;
                        if (long.TryParse(sid, out var lid))
                        {
                            col.LinkedItemIds.Add(lid);
                        }
                    }
                }

                if (col.LinkedItemIds.Count == 0 && !string.IsNullOrWhiteSpace(col.Value))
                {
                    try
                    {
                        using var valueDoc = JsonDocument.Parse(col.Value);
                        if (valueDoc.RootElement.TryGetProperty("linkedPulseIds", out var linkedPulseIds) && linkedPulseIds.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var lp in linkedPulseIds.EnumerateArray())
                            {
                                if (lp.ValueKind == JsonValueKind.Object && lp.TryGetProperty("linkedPulseId", out var lpidObj))
                                {
                                    var rawId = lpidObj.ToString();
                                    if (long.TryParse(rawId, out var lid)) col.LinkedItemIds.Add(lid);
                                }
                                else
                                {
                                    var rawId = lp.ToString();
                                    if (long.TryParse(rawId, out var lid)) col.LinkedItemIds.Add(lid);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignore malformed relation payloads
                    }
                }

                parsed.Columns[id] = col;
            }
        }

        cache[key] = parsed;
        return parsed;
    }

    private static string? ExtractParentRelationIdFromSettings(string settingsJson, HashSet<string> relationIdSet)
    {
        if (string.IsNullOrWhiteSpace(settingsJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(settingsJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("relation_column", out var relColObj) && relColObj.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in relColObj.EnumerateObject())
                {
                    if (relationIdSet.Contains(p.Name)) return p.Name;
                }
            }

            string? Walk(JsonElement node)
            {
                if (node.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in node.EnumerateObject())
                    {
                        var k = prop.Name.ToLowerInvariant();
                        if ((k.Contains("relation") && k.Contains("column") && k.Contains("id")) ||
                            k == "boardrelationcolumnid" ||
                            k == "relation_column_id" ||
                            k == "relationcolumnid")
                        {
                            var candidate = prop.Value.ToString();
                            if (relationIdSet.Contains(candidate)) return candidate;
                        }

                        var nested = Walk(prop.Value);
                        if (!string.IsNullOrWhiteSpace(nested)) return nested;
                    }
                }
                else if (node.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in node.EnumerateArray())
                    {
                        var nested = Walk(el);
                        if (!string.IsNullOrWhiteSpace(nested)) return nested;
                    }
                }

                return null;
            }

            return Walk(root);
        }
        catch
        {
            return null;
        }
    }

    private sealed class OriginSpec
    {
        public long BoardId { get; set; }
        public List<string> ColumnIds { get; set; } = new();
    }

    private static List<OriginSpec> ExtractOriginSpecsFromSettings(string settingsJson)
    {
        var result = new List<OriginSpec>();
        if (string.IsNullOrWhiteSpace(settingsJson)) return result;

        try
        {
            using var doc = JsonDocument.Parse(settingsJson);
            var root = doc.RootElement;
            if (!root.TryGetProperty("displayed_linked_columns", out var list) || list.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var node in list.EnumerateArray())
            {
                long boardId = 0;
                if (node.TryGetProperty("board_id", out var bid))
                {
                    var bidRaw = bid.ToString();
                    long.TryParse(bidRaw, out boardId);
                }
                if (boardId <= 0) continue;

                var colIds = new List<string>();
                if (node.TryGetProperty("column_ids", out var cids) && cids.ValueKind == JsonValueKind.Array)
                {
                    foreach (var cid in cids.EnumerateArray())
                    {
                        var colId = cid.GetString() ?? cid.ToString();
                        if (!string.IsNullOrWhiteSpace(colId)) colIds.Add(colId);
                    }
                }

                if (colIds.Count > 0)
                {
                    result.Add(new OriginSpec { BoardId = boardId, ColumnIds = colIds.Distinct(StringComparer.Ordinal).ToList() });
                }
            }
        }
        catch
        {
            // ignore invalid settings JSON
        }

        return result;
    }

    private static HashSet<long> CollectBoardIdsFromSettingsJson(string settingsJson)
    {
        var result = new HashSet<long>();
        if (string.IsNullOrWhiteSpace(settingsJson)) return result;

        try
        {
            using var doc = JsonDocument.Parse(settingsJson);

            void PushBoardId(JsonElement value)
            {
                if (value.ValueKind == JsonValueKind.Number)
                {
                    if (value.TryGetInt64(out var n) && n > 0) result.Add(n);
                    return;
                }

                if (value.ValueKind == JsonValueKind.String)
                {
                    if (long.TryParse(value.GetString(), out var n) && n > 0) result.Add(n);
                    return;
                }

                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in value.EnumerateArray()) PushBoardId(el);
                }
            }

            void Walk(JsonElement node)
            {
                if (node.ValueKind == JsonValueKind.Object)
                {
                    foreach (var p in node.EnumerateObject())
                    {
                        var k = p.Name.ToLowerInvariant();
                        if (k == "boardid" || k == "boardids" || k == "linkedboardids" || (k.Contains("board") && k.Contains("id")))
                        {
                            PushBoardId(p.Value);
                        }

                        Walk(p.Value);
                    }
                }
                else if (node.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in node.EnumerateArray()) Walk(el);
                }
            }

            Walk(doc.RootElement);
        }
        catch
        {
            // ignore malformed settings
        }

        return result;
    }

    private static string? InferParentRelationId(
        Dictionary<string, ColumnMeta> metaMap,
        ItemData item,
        List<OriginSpec> originSpecs)
    {
        var originBoardIds = originSpecs.Select(x => x.BoardId).Where(x => x > 0).ToHashSet();

        var candidates = metaMap.Values
            .Where(x => IsRelationLikeType(x.Type))
            .Where(x => item.Columns.TryGetValue(x.Id, out var cv) && cv.LinkedItemIds.Count > 0)
            .ToList();

        if (candidates.Count == 0) return null;

        if (originBoardIds.Count > 0)
        {
            foreach (var c in candidates)
            {
                var targetBoards = CollectBoardIdsFromSettingsJson(c.SettingsJson);
                if (targetBoards.Count > 0 && targetBoards.Overlaps(originBoardIds))
                {
                    return c.Id;
                }
            }
        }

        if (candidates.Count == 1)
        {
            return candidates[0].Id;
        }

        return candidates
            .OrderByDescending(c => item.Columns.TryGetValue(c.Id, out var cv) ? cv.LinkedItemIds.Count : 0)
            .Select(c => c.Id)
            .FirstOrDefault();
    }

    private static string PickBestValue(ItemColumnValue? cv)
    {
        if (cv == null) return string.Empty;

        if (!string.IsNullOrWhiteSpace(cv.DisplayValue) && !string.Equals(cv.DisplayValue.Trim(), "null", StringComparison.OrdinalIgnoreCase))
            return cv.DisplayValue.Trim();

        if (!string.IsNullOrWhiteSpace(cv.Text) && !string.Equals(cv.Text.Trim(), "null", StringComparison.OrdinalIgnoreCase))
            return cv.Text.Trim();

        return string.Empty;
    }

    private async Task<List<OriginResolvedValue>> ResolveColumnChainInternal(
        long boardId,
        long itemId,
        string columnId,
        string token,
        Dictionary<long, Dictionary<string, ColumnMeta>> columnCache,
        Dictionary<string, ItemData> itemCache,
        HashSet<string> path,
        List<ResolutionHop> trail)
    {
        var nodeKey = $"{boardId}:{itemId}:{columnId}";
        if (path.Contains(nodeKey))
        {
            return new List<OriginResolvedValue>();
        }

        path.Add(nodeKey);
        try
        {
            var metaMap = await GetBoardColumnMetaMap(boardId, token, columnCache);
            if (!metaMap.TryGetValue(columnId, out var meta))
            {
                return new List<OriginResolvedValue>();
            }

            var currentTrail = new List<ResolutionHop>(trail)
            {
                new ResolutionHop
                {
                    BoardId = boardId,
                    ItemId = itemId,
                    ColumnId = columnId,
                    ColumnTitle = meta.Title,
                    ColumnType = meta.Type
                }
            };

            var item = await GetItemData(boardId, itemId, token, itemCache);
            if (item == null)
            {
                return new List<OriginResolvedValue>();
            }

            if (!IsMirrorLikeType(meta.Type))
            {
                item.Columns.TryGetValue(columnId, out var cv);
                return new List<OriginResolvedValue>
                {
                    new OriginResolvedValue
                    {
                        BoardId = boardId,
                        ItemId = itemId,
                        ColumnId = columnId,
                        ColumnTitle = meta.Title,
                        ColumnType = meta.Type,
                        Value = PickBestValue(cv),
                        Trail = currentTrail
                    }
                };
            }

            var relationIds = metaMap.Values
                .Where(x => IsRelationLikeType(x.Type))
                .Select(x => x.Id)
                .ToHashSet(StringComparer.Ordinal);

            var parentRelationId = ExtractParentRelationIdFromSettings(meta.SettingsJson, relationIds);
            var originSpecs = ExtractOriginSpecsFromSettings(meta.SettingsJson);

            if (string.IsNullOrWhiteSpace(parentRelationId))
            {
                parentRelationId = InferParentRelationId(metaMap, item, originSpecs);
            }

            if (string.IsNullOrWhiteSpace(parentRelationId) || originSpecs.Count == 0)
            {
                item.Columns.TryGetValue(columnId, out var mirrorCv);
                return new List<OriginResolvedValue>
                {
                    new OriginResolvedValue
                    {
                        BoardId = boardId,
                        ItemId = itemId,
                        ColumnId = columnId,
                        ColumnTitle = meta.Title,
                        ColumnType = meta.Type,
                        Value = PickBestValue(mirrorCv),
                        Trail = currentTrail
                    }
                };
            }

            if (!item.Columns.TryGetValue(parentRelationId, out var relCv) || relCv.LinkedItemIds.Count == 0)
            {
                item.Columns.TryGetValue(columnId, out var mirrorCv);
                return new List<OriginResolvedValue>
                {
                    new OriginResolvedValue
                    {
                        BoardId = boardId,
                        ItemId = itemId,
                        ColumnId = columnId,
                        ColumnTitle = meta.Title,
                        ColumnType = meta.Type,
                        Value = PickBestValue(mirrorCv),
                        Trail = currentTrail
                    }
                };
            }

            var resolved = new List<OriginResolvedValue>();
            foreach (var spec in originSpecs)
            {
                foreach (var linkedItemId in relCv.LinkedItemIds.Distinct())
                {
                    foreach (var nextColId in spec.ColumnIds.Distinct(StringComparer.Ordinal))
                    {
                        var branch = await ResolveColumnChainInternal(
                            spec.BoardId,
                            linkedItemId,
                            nextColId,
                            token,
                            columnCache,
                            itemCache,
                            path,
                            currentTrail);

                        if (branch.Count > 0)
                        {
                            resolved.AddRange(branch);
                        }
                    }
                }
            }

            if (resolved.Count == 0)
            {
                item.Columns.TryGetValue(columnId, out var mirrorCv);
                resolved.Add(new OriginResolvedValue
                {
                    BoardId = boardId,
                    ItemId = itemId,
                    ColumnId = columnId,
                    ColumnTitle = meta.Title,
                    ColumnType = meta.Type,
                    Value = PickBestValue(mirrorCv),
                    Trail = currentTrail
                });
            }

            return resolved;
        }
        finally
        {
            path.Remove(nodeKey);
        }
    }

    public async Task<MirrorResolutionResult> ResolveMirrorColumnsForItem(long boardId, long itemId, string token)
    {
        var columnCache = new Dictionary<long, Dictionary<string, ColumnMeta>>();
        var itemCache = new Dictionary<string, ItemData>(StringComparer.Ordinal);
        var result = new MirrorResolutionResult { BoardId = boardId, ItemId = itemId };

        var rootColumns = await GetBoardColumnMetaMap(boardId, token, columnCache);
        var rootItem = await GetItemData(boardId, itemId, token, itemCache);
        if (rootItem == null)
        {
            return result;
        }

        var relationIds = rootColumns.Values
            .Where(x => IsRelationLikeType(x.Type))
            .Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var mirrorCols = rootColumns.Values
            .Where(x => IsMirrorLikeType(x.Type))
            .ToList();

        foreach (var mirror in mirrorCols)
        {
            var mirrorOut = new MirrorColumnResolved { MirrorColumnId = mirror.Id };

            var parentRelId = ExtractParentRelationIdFromSettings(mirror.SettingsJson, relationIds);
            var originSpecs = ExtractOriginSpecsFromSettings(mirror.SettingsJson);

            if (string.IsNullOrWhiteSpace(parentRelId))
            {
                parentRelId = InferParentRelationId(rootColumns, rootItem, originSpecs);
            }

            if (string.IsNullOrWhiteSpace(parentRelId) || originSpecs.Count == 0)
            {
                if (rootItem.Columns.TryGetValue(mirror.Id, out var fallbackCv))
                {
                    mirrorOut.Value = PickBestValue(fallbackCv);
                }
                result.Mirrors.Add(mirrorOut);
                continue;
            }

            if (!rootItem.Columns.TryGetValue(parentRelId, out var relCv) || relCv.LinkedItemIds.Count == 0)
            {
                if (rootItem.Columns.TryGetValue(mirror.Id, out var fallbackCv))
                {
                    mirrorOut.Value = PickBestValue(fallbackCv);
                }
                result.Mirrors.Add(mirrorOut);
                continue;
            }

            foreach (var spec in originSpecs)
            {
                foreach (var linkedItemId in relCv.LinkedItemIds.Distinct())
                {
                    foreach (var originColId in spec.ColumnIds.Distinct(StringComparer.Ordinal))
                    {
                        var origins = await ResolveColumnChainInternal(
                            spec.BoardId,
                            linkedItemId,
                            originColId,
                            token,
                            columnCache,
                            itemCache,
                            new HashSet<string>(StringComparer.Ordinal),
                            new List<ResolutionHop>());

                        mirrorOut.Origins.AddRange(origins);
                    }
                }
            }

            var finalValues = mirrorOut.Origins
                .Select(x => x.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v) && !string.Equals(v.Trim(), "null", StringComparison.OrdinalIgnoreCase))
                .Select(v => v.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            mirrorOut.Value = string.Join(", ", finalValues);
            result.Mirrors.Add(mirrorOut);
        }

        return result;
    }

    public async Task<(string value, string columnType, string finalBoardId, string finalColumnId)> ResolveToOriginValue(
        long boardId,
        long itemId,
        string columnId,
        Dictionary<string, object>? columnSettings,
        string token,
        HashSet<string>? visitedBoards = null)
    {
        var columnCache = new Dictionary<long, Dictionary<string, ColumnMeta>>();
        var itemCache = new Dictionary<string, ItemData>(StringComparer.Ordinal);
        var resolved = await ResolveColumnChainInternal(
            boardId,
            itemId,
            columnId,
            token,
            columnCache,
            itemCache,
            new HashSet<string>(StringComparer.Ordinal),
            new List<ResolutionHop>());

        var best = resolved
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value) && !string.Equals(x.Value.Trim(), "null", StringComparison.OrdinalIgnoreCase))
            ?? resolved.FirstOrDefault();

        if (best == null)
        {
            return (string.Empty, "unknown", boardId.ToString(), columnId);
        }

        return (best.Value ?? string.Empty, best.ColumnType ?? "unknown", best.BoardId.ToString(), best.ColumnId ?? columnId);
    }
}