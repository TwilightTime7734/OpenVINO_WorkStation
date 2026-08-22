// Copyright (c) 2026 Guojin Yan
// Licensed under the Apache-2.0 License.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenVinoSharp.GenAI
{
    /// <summary>
    /// Applies runtime compatibility properties required by Qwen3.5-family OpenVINO VLM exports.
    /// Model files are never modified here.
    /// </summary>
    internal static class VlmModelCompatibility
    {
        private const string Qwen35ModelType = "qwen3_5";
        private const string AttentionBackendKey = "ATTENTION_BACKEND";
        private const string SdpaBackend = "SDPA";

        public static IDictionary<string, string>? Prepare(
            string modelsPath,
            IDictionary<string, string>? properties)
        {
            if (!IsQwen35Family(modelsPath))
                return properties;

            Dictionary<string, string> effective = properties is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase);

            effective[AttentionBackendKey] = SdpaBackend;
            return effective;
        }

        private static bool IsQwen35Family(string modelsPath)
        {
            try
            {
                string configPath = Path.Combine(modelsPath, "config.json");
                if (!File.Exists(configPath))
                    return false;

                using JsonDocument document = JsonDocument.Parse(File.ReadAllText(configPath));
                JsonElement root = document.RootElement;

                if (HasModelType(root, Qwen35ModelType))
                    return true;

                if (root.TryGetProperty("text_config", out JsonElement textConfig)
                    && textConfig.ValueKind == JsonValueKind.Object
                    && HasModelType(textConfig, Qwen35ModelType))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool HasModelType(JsonElement element, string expected)
        {
            return element.TryGetProperty("model_type", out JsonElement modelType)
                && modelType.ValueKind == JsonValueKind.String
                && string.Equals(modelType.GetString(), expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
