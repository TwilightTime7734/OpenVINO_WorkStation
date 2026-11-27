using System.Text.RegularExpressions;
using static WorkStationC_.AppFiles;

namespace WorkStationC_;

// OVMS uses the device written in a model's graph.pbtxt and ignores --target_device,
// so the graph file must name the device chosen on Workstation.
internal static partial class OvmsGraph
{
    [GeneratedRegex(@"(\b(?:target_)?device:\s*"")([^""]*)("")")]
    private static partial Regex DeviceSetting();

    internal static string GraphPath(string modelPath) => Path.Combine(modelPath, "graph.pbtxt");

    internal static IReadOnlyList<string> ReadDevices(string modelPath)
    {
        string path = GraphPath(modelPath);
        if (!File.Exists(path)) return [];
        return DeviceSetting().Matches(File.ReadAllText(path)).Select(match => match.Groups[2].Value).Distinct().ToArray();
    }

    // Returns the devices that were replaced, or an empty list when nothing changed.
    internal static IReadOnlyList<string> SetDevice(string modelPath, string device)
    {
        string path = GraphPath(modelPath);
        if (!File.Exists(path)) return [];
        string text = File.ReadAllText(path);
        string[] previous = DeviceSetting().Matches(text).Select(match => match.Groups[2].Value)
            .Where(value => !value.Equals(device, StringComparison.Ordinal)).Distinct().ToArray();
        if (previous.Length == 0) return [];
        WriteTextAtomically(path, DeviceSetting().Replace(text, match => match.Groups[1].Value + device + match.Groups[3].Value));
        return previous;
    }

    [GeneratedRegex(@"(\bcache_size:\s*)(\d+)")]
    private static partial Regex CacheSizeSetting();

    [GeneratedRegex(@"(\bplugin_config:\s*')(\{[^']*\})(')")]
    private static partial Regex PluginConfigSetting();

    // A cache_size of 0 lets the model's working memory (KV cache) grow without limit until it spills into
    // system memory, which makes every reply crawl. Write a fixed size, and u8 precision to halve its cost.
    // Returns a description of what changed, or null when the graph has nothing to set.
    internal static string? SetMemory(string modelPath, int cacheGigabytes, bool reducedPrecision)
    {
        string path = GraphPath(modelPath);
        if (!File.Exists(path)) return null;
        string original = File.ReadAllText(path);
        if (!CacheSizeSetting().IsMatch(original)) return null; // Not a text-generation graph.
        List<string> changes = [];
        string text = CacheSizeSetting().Replace(original, match =>
        {
            if (match.Groups[2].Value != cacheGigabytes.ToString()) changes.Add($"cache_size {match.Groups[2].Value} -> {cacheGigabytes}");
            return match.Groups[1].Value + cacheGigabytes;
        });
        if (reducedPrecision)
        {
            text = PluginConfigSetting().Replace(text, match =>
            {
                var config = System.Text.Json.Nodes.JsonNode.Parse(match.Groups[2].Value) as System.Text.Json.Nodes.JsonObject ?? [];
                if (config["KV_CACHE_PRECISION"]?.ToString() == "u8") return match.Value;
                config["KV_CACHE_PRECISION"] = "u8";
                changes.Add("KV_CACHE_PRECISION u8");
                return match.Groups[1].Value + config.ToJsonString() + match.Groups[3].Value;
            });
        }
        if (text != original) WriteTextAtomically(path, text);
        return changes.Count == 0 ? string.Empty : string.Join(", ", changes);
    }
}
