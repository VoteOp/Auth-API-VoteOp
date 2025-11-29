using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace VoteOp.AuthApi.Utils;

public static class HttpRequestExtensions
{
    public static async Task<T?> ReadFromJsonAsync<T>(this HttpRequestData req)
    {
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(body)) return default;

        return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}