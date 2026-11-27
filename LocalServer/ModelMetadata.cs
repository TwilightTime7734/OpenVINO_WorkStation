using static WorkStationC_.AppFiles;
using static WorkStationC_.ModelFiles;

namespace WorkStationC_;

internal static class ModelMetadata
{

    internal const int DefaultLocalApiContextWindowTokens = 32768;

    internal const int MaxAdvertisedLocalApiContextWindowTokens = 1048576;

    internal static string BuildCurrentLoadedLocalApiModelName(string selectedFolder, string runtimePath)
    {
        string candidate = !string.IsNullOrWhiteSpace(selectedFolder)
            ? Path.GetFileName(selectedFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            : string.Empty;
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = Directory.Exists(runtimePath)
                ? Path.GetFileName(runtimePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                : Path.GetFileNameWithoutExtension(runtimePath);
        }

        return NormalizeLocalApiModelName(candidate);
    }

    internal static LocalModelApiModel Describe(RuntimeModelSelection source, ModelConfigClassification classification)
    {
        string selectedFolder = source.SelectedFolder;
        string runtimePath = source.RuntimePath;
        ModelFileKind kind = source.Kind;
        string name = BuildCurrentLoadedLocalApiModelName(selectedFolder, runtimePath);
        DateTimeOffset modifiedAtUtc = GetLocalApiModifiedTime(runtimePath);
        long sizeBytes = EstimateLocalApiModelSize(runtimePath);
        bool supportsChat = kind is ModelFileKind.OpenVinoGenAiDirectory or ModelFileKind.Gguf
            && classification.AcceptsText;
        bool supportsImageInput = kind == ModelFileKind.OpenVinoGenAiDirectory
            && classification.AcceptsImage;

        return new LocalModelApiModel
        {
            Name = name,
            Source = "Workstation",
            RuntimePath = runtimePath,
            Kind = DescribeRuntimeModelKind(kind),
            Format = DetectLocalApiFormat(kind, runtimePath),
            Family = DetectLocalApiFamily(runtimePath),
            QuantizationLevel = DetectLocalApiQuantization(runtimePath),
            ContextWindowTokens = DetectLocalApiContextWindow(selectedFolder, runtimePath, kind),
            SupportsChat = supportsChat,
            SupportsImageInput = supportsImageInput,
            Loaded = true,
            ModifiedAtUtc = modifiedAtUtc,
            SizeBytes = sizeBytes,
            Digest = LocalModelApiServer.CreateDigest(runtimePath, sizeBytes, modifiedAtUtc),
        };
    }

    internal static string NormalizeLocalApiModelName(string value)
    {
        string normalized = value.Trim()
            .Replace('\\', '/')
            .Replace(" ", "-", StringComparison.Ordinal);
        return string.IsNullOrWhiteSpace(normalized) ? "model" : normalized;
    }

    internal static DateTimeOffset GetLocalApiModifiedTime(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero);
            }

            if (Directory.Exists(path))
            {
                return new DateTimeOffset(Directory.GetLastWriteTimeUtc(path), TimeSpan.Zero);
            }
        }
        catch
        {
        }

        return DateTimeOffset.UtcNow;
    }

    internal static long EstimateLocalApiModelSize(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                long size = new FileInfo(path).Length;
                string? binPath = FindMatchingBinPath(path);
                if (!string.IsNullOrWhiteSpace(binPath) && File.Exists(binPath))
                {
                    size += new FileInfo(binPath).Length;
                }

                return size;
            }

            if (Directory.Exists(path))
            {
                long total = 0;
                foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Take(5000))
                {
                    total += new FileInfo(file).Length;
                }

                return total;
            }
        }
        catch
        {
        }

        return 0;
    }

    internal static string DetectLocalApiFormat(ModelFileKind kind, string path)
    {
        return kind switch
        {
            ModelFileKind.OpenVinoGenAiDirectory => "openvino_genai",
            ModelFileKind.OpenVinoIr => "openvino_ir",
            ModelFileKind.OpenVinoCoreReadable => Path.GetExtension(path).TrimStart('.').ToLowerInvariant(),
            ModelFileKind.Gguf => "gguf",
            _ => "unknown",
        };
    }

    internal static string DetectLocalApiFamily(string path)
    {
        string name = Path.GetFileName(path);
        string directory = Path.GetFileName(Path.GetDirectoryName(path) ?? string.Empty);
        string text = $"{name} {directory}".ToLowerInvariant();
        if (text.Contains("qwen", StringComparison.OrdinalIgnoreCase))
        {
            return "qwen";
        }

        if (text.Contains("mobilenet", StringComparison.OrdinalIgnoreCase))
        {
            return "mobilenet";
        }

        if (text.Contains("llama", StringComparison.OrdinalIgnoreCase))
        {
            return "llama";
        }

        if (text.Contains("phi", StringComparison.OrdinalIgnoreCase))
        {
            return "phi";
        }

        return string.Empty;
    }

    internal static string DetectLocalApiQuantization(string path)
    {
        string text = path.ToLowerInvariant();
        string[] markers =
        {
            "int4",
            "int8",
            "fp16",
            "fp32",
            "q4",
            "q5",
            "q6",
            "q8",
        };

        return markers.FirstOrDefault(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }

    internal static int DetectLocalApiContextWindow(string selectedFolder, string runtimePath, ModelFileKind kind)
    {
        foreach (string configPath in EnumerateLocalApiContextConfigFiles(selectedFolder, runtimePath, kind))
        {
            int? contextWindow = TryReadContextWindowFromJson(configPath);
            if (contextWindow is > 0)
            {
                return contextWindow.Value;
            }
        }

        return DefaultLocalApiContextWindowTokens;
    }

    internal static IEnumerable<string> EnumerateLocalApiContextConfigFiles(string selectedFolder, string runtimePath, ModelFileKind kind)
    {
        string configDirectory = GetModelConfigDirectory(selectedFolder, runtimePath, kind);
        string[] fileNames =
        {
            "config.json",
            "generation_config.json",
            "tokenizer_config.json",
        };

        foreach (string directory in EnumerateDistinctExistingDirectories(configDirectory, selectedFolder, runtimePath))
        {
            foreach (string fileName in fileNames)
            {
                string path = Path.Combine(directory, fileName);
                if (File.Exists(path))
                {
                    yield return path;
                }
            }
        }
    }

    internal static IEnumerable<string> EnumerateDistinctExistingDirectories(params string[] paths)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in paths)
        {
            string? directory = Directory.Exists(path)
                ? path
                : File.Exists(path) ? Path.GetDirectoryName(path) : null;
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(directory);
            }
            catch
            {
                continue;
            }

            if (Directory.Exists(fullPath) && seen.Add(fullPath))
            {
                yield return fullPath;
            }
        }
    }

    internal static int? TryReadContextWindowFromJson(string configPath)
    {
        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(configPath));
            return TryReadContextWindowFromJsonElement(document.RootElement);
        }
        catch
        {
            return null;
        }
    }

    internal static int? TryReadContextWindowFromJsonElement(System.Text.Json.JsonElement element)
    {
        string[] directNames =
        {
            "max_model_len",
            "context_length",
            "context_window",
            "model_max_length",
            "max_position_embeddings",
            "max_sequence_length",
            "seq_length",
            "max_seq_len",
            "n_positions",
            "n_ctx",
            "num_ctx",
            "sliding_window",
        };

        foreach (string propertyName in directNames)
        {
            int? value = TryReadContextTokenCount(element, propertyName);
            if (value is > 0)
            {
                return value;
            }
        }

        string[] nestedNames =
        {
            "text_config",
            "llm_config",
            "language_config",
        };

        foreach (string propertyName in nestedNames)
        {
            if (element.TryGetProperty(propertyName, out System.Text.Json.JsonElement nested)
                && nested.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                int? value = TryReadContextWindowFromJsonElement(nested);
                if (value is > 0)
                {
                    return value;
                }
            }
        }

        return null;
    }

    internal static int? TryReadContextTokenCount(System.Text.Json.JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property))
        {
            return null;
        }

        long? value = property.ValueKind switch
        {
            System.Text.Json.JsonValueKind.Number when property.TryGetInt64(out long number) => number,
            System.Text.Json.JsonValueKind.String when long.TryParse(property.GetString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long number) => number,
            _ => null,
        };

        if (value is null
            || value <= 0
            || value > MaxAdvertisedLocalApiContextWindowTokens)
        {
            return null;
        }

        return (int)value.Value;
    }

}
