// Copyright (c) 2026 Guojin Yan
// Licensed under the Apache-2.0 License.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        // This is the exact small Minja-compatible template verified against the
        // Qwen3.5/Qwen3.8 OpenVINO VLM path. The VLM pipeline normalizes image/video
        // placeholders into the user content before applying the chat template, so
        // the compatibility template preserves normal text and vision prompts while
        // avoiding unsupported Jinja tests used by the exported HF template.
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

            // Qwen3.5-family models currently require the regular SDPA VLM backend.
            // The default Paged-Attention/continuous-batching backend does not support
            // the chat-template path used by this compatibility mode.
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
            // Keep every template source coherent. OpenVINO GenAI can read the normal
            // chat_template.jinja file, but the tokenizer IR may contain its own
            // chat_template rt_info value which takes precedence during setup.
            ReplaceTemplateFile(Path.Combine(modelsPath, "chat_template.jinja"));
            ReplaceEmbeddedTemplate(Path.Combine(modelsPath, "openvino_tokenizer.xml"));
            ReplaceEmbeddedTemplate(Path.Combine(modelsPath, "openvino_detokenizer.xml"));
        }

        private static void ReplaceTemplateFile(string path)
        {
            if (!File.Exists(path))
                return;

            string current = File.ReadAllText(path);
            if (string.Equals(current, CompatibilityTemplate, StringComparison.Ordinal))
                return;

            SaveOriginalTemplate(path, current);
            File.WriteAllText(path, CompatibilityTemplate, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void ReplaceEmbeddedTemplate(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                return;

            string xml = File.ReadAllText(xmlPath);
            const string tag = "<chat_template";
            const string valueMarker = "value=\"";

            int tagStart = xml.IndexOf(tag, StringComparison.Ordinal);
            if (tagStart < 0)
                return;

            int markerStart = xml.IndexOf(valueMarker, tagStart, StringComparison.Ordinal);
            if (markerStart < 0)
                return;

            int valueStart = markerStart + valueMarker.Length;
            int valueEnd = xml.IndexOf('\"', valueStart);
            if (valueEnd < 0)
                return;

            string encodedTemplate = xml.Substring(valueStart, valueEnd - valueStart);
            string currentTemplate = WebUtility.HtmlDecode(encodedTemplate);
            if (string.Equals(currentTemplate, CompatibilityTemplate, StringComparison.Ordinal))
                return;

            SaveOriginalTemplate(xmlPath, currentTemplate);

            string patchedXml = string.Concat(
                xml.AsSpan(0, valueStart),
                EncodeXmlAttribute(CompatibilityTemplate),
                xml.AsSpan(valueEnd));

            File.WriteAllText(xmlPath, patchedXml, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static string EncodeXmlAttribute(string value)
        {
            // Preserve newlines explicitly because XML attribute whitespace is normalized.
            return value
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("'", "&apos;", StringComparison.Ordinal)
                .Replace("\r\n", "&#10;", StringComparison.Ordinal)
                .Replace("\n", "&#10;", StringComparison.Ordinal)
                .Replace("\r", "&#10;", StringComparison.Ordinal);
        }

        private static void SaveOriginalTemplate(string sourcePath, string template)
        {
            try
            {
                string directory = Path.Combine(
                    Path.GetDirectoryName(sourcePath) ?? string.Empty,
                    ".workstation-compat");
                Directory.CreateDirectory(directory);

                string fileName = Path.GetFileName(sourcePath) + ".original-chat-template.jinja";
                string backupPath = Path.Combine(directory, fileName);
                if (!File.Exists(backupPath))
                    File.WriteAllText(backupPath, template, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }
            catch
            {
                // Backup failure should not prevent model loading. The compatibility
                // edit itself remains narrowly scoped to chat-template metadata.
            }
        }
    }
}
