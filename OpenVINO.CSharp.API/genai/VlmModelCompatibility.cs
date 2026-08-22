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
    /// Applies small runtime compatibility fixes required by newer Qwen3.5-family
    /// OpenVINO VLM exports when used with OpenVINO GenAI's Minja template engine.
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

            PatchQwen35Templates(modelsPath);

            Dictionary<string, string> effective = properties is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase);

            // Qwen3.5 currently needs the regular SDPA VLM backend. The default
            // Paged-Attention/continuous-batching backend does not support the
            // chat-template path used by these exports.
            if (!effective.ContainsKey(AttentionBackendKey))
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

        private static void PatchQwen35Templates(string modelsPath)
        {
            // Keep the normal exported template file consistent for tools that read it.
            PatchTemplateFile(Path.Combine(modelsPath, "chat_template.jinja"));

            // OpenVINO tokenizer IR stores its own chat_template in rt_info. That
            // embedded value overrides chat_template.jinja during tokenizer setup,
            // so it must be repaired too. Do both files to keep metadata coherent.
            PatchEmbeddedTemplate(Path.Combine(modelsPath, "openvino_tokenizer.xml"));
            PatchEmbeddedTemplate(Path.Combine(modelsPath, "openvino_detokenizer.xml"));
        }

        private static void PatchTemplateFile(string path)
        {
            if (!File.Exists(path))
                return;

            string original = File.ReadAllText(path);
            string patched = MakeMinjaCompatible(original);
            if (string.Equals(original, patched, StringComparison.Ordinal))
                return;

            SaveOriginalTemplate(path, original);
            File.WriteAllText(path, patched, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void PatchEmbeddedTemplate(string xmlPath)
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
            string originalTemplate = WebUtility.HtmlDecode(encodedTemplate);
            string patchedTemplate = MakeMinjaCompatible(originalTemplate);
            if (string.Equals(originalTemplate, patchedTemplate, StringComparison.Ordinal))
                return;

            SaveOriginalTemplate(xmlPath, originalTemplate);
            string patchedEncoded = EncodeXmlAttribute(patchedTemplate);
            string patchedXml = string.Concat(
                xml.AsSpan(0, valueStart),
                patchedEncoded,
                xml.AsSpan(valueEnd));

            File.WriteAllText(xmlPath, patchedXml, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static string MakeMinjaCompatible(string template)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            return template
                .Replace(
                    "enable_thinking is undefined or enable_thinking is true",
                    "enable_thinking|default(true)",
                    StringComparison.Ordinal)
                .Replace(
                    "preserve_thinking is undefined or preserve_thinking is true",
                    "preserve_thinking|default(true)",
                    StringComparison.Ordinal)
                .Replace(
                    "enable_thinking is defined and enable_thinking is false",
                    "not (enable_thinking|default(true))",
                    StringComparison.Ordinal)
                .Replace(
                    "tool_call.arguments is defined and tool_call.arguments != ''",
                    "(tool_call.arguments|default('')) != ''",
                    StringComparison.Ordinal)
                .Replace(
                    "tool_call.function is defined",
                    "tool_call.function|default(false)",
                    StringComparison.Ordinal)
                .Replace(
                    "content is none or content is undefined",
                    "content|default(none) is none",
                    StringComparison.Ordinal);
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
                // edit itself remains narrowly scoped to the chat_template metadata.
            }
        }
    }
}
