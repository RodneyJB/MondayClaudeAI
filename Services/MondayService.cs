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
}