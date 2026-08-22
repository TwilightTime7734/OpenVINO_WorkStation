// Copyright (c) 2026 Guojin Yan
// Licensed under the Apache-2.0 License.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OpenVinoSharp.GenAI
{
    /// <summary>
    /// Applies runtime compatibility fixes required by Qwen3.5-family OpenVINO VLM
    /// exports when used through OpenVINO GenAI's Minja template engine.
    /// </summary>
    internal static class VlmModelCompatibility
    {
        private const string Qwen35ModelType = "qwen3_5";
        private const string AttentionBackendKey = "ATTENTION_BACKEND";
        private const string SdpaBackend = "SDPA";

        // Exact small Minja-compatible template verified against the Qwen3.5/Qwen3.8
        // OpenVINO VLM path.
        private const string CompatibilityTemplate =
            "<|im_start|>user\n" +
            "{{ messages[0].content }}<|im_end|>\n" +
            "<|im_start|>assistant\n";

        public static IDictionary<string, string>? Prepare(
            string modelsPath,
            IDictionary<string, string>? properties)
        {
            if (!IsQwen35Family(modelsPath))
                return properties;

            InstallCompatibilityTemplate(modelsPath);

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

        private static void InstallCompatibilityTemplate(string modelsPath)
        {
            ReplaceTemplateFile(Path.Combine(modelsPath, "chat_template.jinja"));

            // The tokenizer IR may embed its own chat_template in rt_info. That value
            // can override chat_template.jinja, so remove only that metadata element.
            // Do not rewrite the enormous encoded attribute value: stripping the small
            // rt_info node is safer and lets GenAI use the verified external template.
            RemoveEmbeddedTemplate(Path.Combine(modelsPath, "openvino_tokenizer.xml"));
            RemoveEmbeddedTemplate(Path.Combine(modelsPath, "openvino_detokenizer.xml"));
        }

        private static void ReplaceTemplateFile(string path)
        {
            if (!File.Exists(path))
                return;

            string current = File.ReadAllText(path);
            if (string.Equals(current, CompatibilityTemplate, StringComparison.Ordinal))
                return;

            SaveOriginalFile(path, current);
            File.WriteAllText(path, CompatibilityTemplate, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void RemoveEmbeddedTemplate(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                return;

            string xml = File.ReadAllText(xmlPath);
            const string startMarker = "<chat_template";

            int start = xml.IndexOf(startMarker, StringComparison.Ordinal);
            if (start < 0)
                return;

            int selfClosingEnd = xml.IndexOf("/>", start, StringComparison.Ordinal);
            if (selfClosingEnd < 0)
                return;

            int endExclusive = selfClosingEnd + 2;
            SaveOriginalFile(xmlPath, xml);
            string patched = string.Concat(xml.AsSpan(0, start), xml.AsSpan(endExclusive));
            File.WriteAllText(xmlPath, patched, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void SaveOriginalFile(string sourcePath, string content)
        {
            try
            {
                string directory = Path.Combine(
                    Path.GetDirectoryName(sourcePath) ?? string.Empty,
                    ".workstation-compat");
                Directory.CreateDirectory(directory);

                string fileName = Path.GetFileName(sourcePath) + ".original";
                string backupPath = Path.Combine(directory, fileName);
                if (!File.Exists(backupPath))
                    File.WriteAllText(backupPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }
            catch
            {
                // Backup failure should not prevent model loading.
            }
        }
    }
}
