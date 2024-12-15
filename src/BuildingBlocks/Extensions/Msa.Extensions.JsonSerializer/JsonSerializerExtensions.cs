using System.Text.Json;

namespace Msa.Extensions.JsonSerializer
{
    public static class JsonSerializerExtensions
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string Serialize<T>(this T obj) where T : class => System.Text.Json.JsonSerializer.Serialize(obj, JsonSerializerOptions);

        public static T? Deserialize<T>(this string value) where T : class => System.Text.Json.JsonSerializer.Deserialize<T>(value, JsonSerializerOptions);

    }
}
