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

    /// <summary>
    /// Recursively resolves a mirror column value to its true origin by following the chain.
    /// Handles mirrors of mirrors until reaching a non-mirror column.
    /// </summary>
    public async Task<(string value, string columnType, string finalBoardId, string finalColumnId)> ResolveToOriginValue(
        long boardId,
        long itemId,
        string columnId,
        Dictionary<string, object>? columnSettings,
        string token,
        HashSet<string>? visitedBoards = null)
    {
        visitedBoards ??= new HashSet<string>();

        var boardKey = $"{boardId}:{columnId}";
        if (visitedBoards.Contains(boardKey))
        {
            // Circular reference detected
            return (string.Empty, "error", boardId.ToString(), columnId);
        }
        visitedBoards.Add(boardKey);

        // Fetch the column metadata from this board
        var columnsJson = await GetBoardColumns(boardId, token);
        var columnsDoc = JsonDocument.Parse(columnsJson);

        var column = columnsDoc.RootElement
            .GetProperty("data")
            .GetProperty("boards")[0]
            .GetProperty("columns")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("id").GetString() == columnId);

        if (column.ValueKind == JsonValueKind.Undefined)
        {
            return (string.Empty, "not_found", boardId.ToString(), columnId);
        }

        var colType = column.GetProperty("type").GetString() ?? "unknown";

        // If this is not a mirror, fetch and return its value
        if (colType != "mirror")
        {
            var itemsJson = await GetOriginValues(boardId, new[] { itemId }, new[] { columnId }, token);
            var itemsDoc = JsonDocument.Parse(itemsJson);

            try
            {
                var item = itemsDoc.RootElement
                    .GetProperty("data")
                    .GetProperty("boards")[0]
                    .GetProperty("items")[0];

                var colValue = item.GetProperty("column_values")[0];
                var displayValue = colValue.TryGetProperty("display_value", out var dv) ? dv.GetString() : null;
                var textValue = colValue.TryGetProperty("text", out var tv) ? tv.GetString() : null;
                var value = displayValue ?? textValue ?? string.Empty;

                return (value, colType, boardId.ToString(), columnId);
            }
            catch
            {
                return (string.Empty, colType, boardId.ToString(), columnId);
            }
        }

        // This IS a mirror - extract origin specs from settings
        if (columnSettings == null)
        {
            var settingsStr = column.TryGetProperty("settings_str", out var ss) ? ss.GetString() : null;
            if (!string.IsNullOrEmpty(settingsStr))
            {
                try
                {
                    columnSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(settingsStr);
                }
                catch { }
            }
        }

        // Extract displayed_linked_columns (origin board/column refs)
        if (columnSettings?.TryGetValue("displayed_linked_columns", out var dlc) == true &&
            dlc is JsonElement dlcElement &&
            dlcElement.ValueKind == JsonValueKind.Array)
        {
            var dlcArray = dlcElement.EnumerateArray().ToList();
            if (dlcArray.Count > 0)
            {
                var dlcObj = dlcArray[0];
                if (dlcObj.TryGetProperty("board_id", out var bid) && long.TryParse(bid.GetString(), out var originBoardId))
                {
                    if (dlcObj.TryGetProperty("column_ids", out var cids) && cids.ValueKind == JsonValueKind.Array)
                    {
                        var columnIdsArray = cids.EnumerateArray().ToList();
                        if (columnIdsArray.Count > 0)
                        {
                            var originColumnId = columnIdsArray[0].GetString() ?? string.Empty;

                            // Recursively resolve from the origin board
                            return await ResolveToOriginValue(originBoardId, itemId, originColumnId, null, token, visitedBoards);
                        }
                    }
                }
            }
        }

        // Fallback: return empty if we can't resolve
        return (string.Empty, colType, boardId.ToString(), columnId);
    }
}