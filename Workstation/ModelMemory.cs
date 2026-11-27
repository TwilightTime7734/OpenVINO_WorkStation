using System.Text.Json;

namespace WorkStationC_;

// Sizes the model's working memory (KV cache): how much GPU memory to reserve, and roughly how many
// tokens of conversation that holds, from the model's config.json.
internal static class ModelMemory
{
    // Kept free beside the weights and the cache: vision encoder, temporary buffers, driver use.
    private const ulong SafetyMarginBytes = 2UL << 30;
    private const int MaxCacheGigabytes = 16;

    // Whole gigabytes that fit after the weights and the safety margin; at least 1.
    internal static int CacheGigabytes(ulong gpuMemoryBytes, ulong weightBytes)
    {
        if (gpuMemoryBytes <= weightBytes + SafetyMarginBytes) return 1;
        ulong free = gpuMemoryBytes - weightBytes - SafetyMarginBytes;
        return (int)Math.Clamp(free >> 30, 1, MaxCacheGigabytes);
    }

    // Cache bytes for one token: keys and values for every full-attention layer.
    // Models such as Qwen3.5 use fixed-size linear-attention layers, which do not grow with the conversation.
    internal static long? BytesPerToken(string modelPath, bool reducedPrecision)
    {
        string config = Path.Combine(modelPath, "config.json");
        if (!File.Exists(config)) return null;
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(config));
            var text = json.RootElement.TryGetProperty("text_config", out var nested) ? nested : json.RootElement;
            int layers = Int(text, "num_hidden_layers");
            if (text.TryGetProperty("layer_types", out var types) && types.ValueKind == JsonValueKind.Array)
                layers = types.EnumerateArray().Count(type => type.GetString() == "full_attention");
            int heads = Int(text, "num_attention_heads");
            int kvHeads = Int(text, "num_key_value_heads");
            if (kvHeads == 0) kvHeads = heads;
            int headDim = Int(text, "head_dim");
            if (headDim == 0 && heads > 0) headDim = Int(text, "hidden_size") / heads;
            if (layers == 0 || kvHeads == 0 || headDim == 0) return null;
            return 2L * layers * kvHeads * headDim * (reducedPrecision ? 1 : 2);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static int? ContextTokens(string modelPath, int cacheGigabytes, bool reducedPrecision) =>
        BytesPerToken(modelPath, reducedPrecision) is { } perToken
            ? (int)Math.Min(int.MaxValue, ((long)cacheGigabytes << 30) / perToken)
            : null;

    private static int Int(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetInt32(out int number) ? number : 0;
}
