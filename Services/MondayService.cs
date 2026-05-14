using System.Text;
using System.Text.Json;

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
                            text
                            value
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
}