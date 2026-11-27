using static WorkStationC_.AppFiles;

namespace WorkStationC_;

internal static class ModelFiles
{

    internal sealed record RuntimeModelSelection(string SelectedFolder, string RuntimePath, ModelFileKind Kind);

    internal sealed record ModelConfigClassification(
        ModelRunPath RunPath,
        bool AcceptsText,
        bool AcceptsImage,
        bool ReturnsImage,
        IReadOnlyList<string> Evidence);

    internal static ModelConfigClassification ClassifyModelRunPath(string selectedFolder, string runtimePath, ModelFileKind kind)
    {
        List<string> evidence = new();
        string modelDirectory = GetModelConfigDirectory(selectedFolder, runtimePath, kind);
        ModelConfigSnapshot snapshot = ReadModelConfigSnapshot(modelDirectory);
        evidence.AddRange(snapshot.Evidence);

        if (kind == ModelFileKind.Gguf)
        {
            evidence.Add("GGUF file");
            return new ModelConfigClassification(ModelRunPath.TextGeneration, true, false, false, evidence);
        }

        if (snapshot.HasDiffusionModelIndex)
        {
            ModelRunPath diffusionPath = snapshot.ModelIndexClass.Contains("image-to-image", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelIndexClass.Contains("img2img", StringComparison.OrdinalIgnoreCase)
                ? ModelRunPath.ImageToImage
                : ModelRunPath.TextToImage;
            evidence.Add(string.IsNullOrWhiteSpace(snapshot.ModelIndexClass)
                ? "model_index.json"
                : $"model_index.json {_classNameLabel(snapshot.ModelIndexClass)}");
            return new ModelConfigClassification(diffusionPath, true, diffusionPath == ModelRunPath.ImageToImage, true, evidence);
        }

        if (kind == ModelFileKind.OpenVinoGenAiDirectory)
        {
            if (IsVisionLanguageConfig(snapshot, modelDirectory))
            {
                return new ModelConfigClassification(ModelRunPath.VisionLanguage, true, true, false, evidence);
            }

            if (IsTextEmbeddingConfig(snapshot, modelDirectory))
            {
                return new ModelConfigClassification(ModelRunPath.TextEmbedding, true, false, false, evidence);
            }

            return new ModelConfigClassification(ModelRunPath.TextGeneration, true, false, false, evidence);
        }

        if (IsVisionLanguageConfig(snapshot, modelDirectory))
        {
            return new ModelConfigClassification(ModelRunPath.VisionLanguage, true, true, false, evidence);
        }

        if (IsObjectDetectionConfig(snapshot))
        {
            return new ModelConfigClassification(ModelRunPath.ObjectDetection, false, true, false, evidence);
        }

        if (IsImageClassificationConfig(snapshot))
        {
            return new ModelConfigClassification(ModelRunPath.ImageClassification, false, true, false, evidence);
        }

        if (IsSpeechConfig(snapshot))
        {
            return new ModelConfigClassification(ModelRunPath.SpeechRecognition, false, false, false, evidence);
        }

        if (IsTextEmbeddingConfig(snapshot, modelDirectory))
        {
            return new ModelConfigClassification(ModelRunPath.TextEmbedding, true, false, false, evidence);
        }

        if (kind is ModelFileKind.OpenVinoIr or ModelFileKind.OpenVinoCoreReadable)
        {
            evidence.Add($"{Path.GetExtension(runtimePath).TrimStart('.')} model file");
            return new ModelConfigClassification(ModelRunPath.OpenVinoCore, false, false, false, evidence);
        }

        return new ModelConfigClassification(ModelRunPath.Unknown, false, false, false, evidence);

        static string _classNameLabel(string value) => $"class {value}";
    }

    internal sealed record ModelConfigSnapshot(
        string ModelType,
        IReadOnlyList<string> Architectures,
        bool HasVisionConfig,
        bool HasTextConfig,
        bool LanguageModelOnly,
        bool HasDiffusionModelIndex,
        string ModelIndexClass,
        IReadOnlyList<string> ModelIndexKeys,
        IReadOnlyList<string> Evidence);

    internal static ModelConfigSnapshot ReadModelConfigSnapshot(string modelDirectory)
    {
        List<string> evidence = new();
        string modelType = string.Empty;
        List<string> architectures = new();
        bool hasVisionConfig = false;
        bool hasTextConfig = false;
        bool languageModelOnly = false;
        bool hasDiffusionModelIndex = false;
        string modelIndexClass = string.Empty;
        List<string> modelIndexKeys = new();

        string configPath = Path.Combine(modelDirectory, "config.json");
        if (File.Exists(configPath))
        {
            try
            {
                using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(configPath));
                System.Text.Json.JsonElement root = document.RootElement;
                modelType = TryReadJsonString(root, "model_type") ?? string.Empty;
                architectures.AddRange(TryReadJsonStringArray(root, "architectures"));
                hasVisionConfig = root.TryGetProperty("vision_config", out _);
                hasTextConfig = root.TryGetProperty("text_config", out _);
                languageModelOnly = TryReadJsonBool(root, "language_model_only") == true;
                if (!string.IsNullOrWhiteSpace(modelType))
                {
                    evidence.Add($"config.json model_type={modelType}");
                }

                if (architectures.Count > 0)
                {
                    evidence.Add($"config.json architectures={string.Join(",", architectures)}");
                }
            }
            catch (Exception ex)
            {
                evidence.Add($"config.json unreadable: {ex.Message}");
            }
        }

        string modelIndexPath = Path.Combine(modelDirectory, "model_index.json");
        if (File.Exists(modelIndexPath))
        {
            hasDiffusionModelIndex = true;
            try
            {
                using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(modelIndexPath));
                System.Text.Json.JsonElement root = document.RootElement;
                modelIndexClass = TryReadJsonString(root, "_class_name") ?? string.Empty;
                modelIndexKeys.AddRange(root.EnumerateObject().Select(property => property.Name));
                evidence.Add(string.IsNullOrWhiteSpace(modelIndexClass)
                    ? "model_index.json"
                    : $"model_index.json _class_name={modelIndexClass}");
            }
            catch (Exception ex)
            {
                evidence.Add($"model_index.json unreadable: {ex.Message}");
            }
        }

        if (File.Exists(Path.Combine(modelDirectory, "preprocessor_config.json")))
        {
            evidence.Add("preprocessor_config.json");
        }

        if (File.Exists(Path.Combine(modelDirectory, "processor_config.json")))
        {
            evidence.Add("processor_config.json");
        }

        if (File.Exists(Path.Combine(modelDirectory, "tokenizer_config.json")))
        {
            evidence.Add("tokenizer_config.json");
        }

        return new ModelConfigSnapshot(
            modelType,
            architectures,
            hasVisionConfig,
            hasTextConfig,
            languageModelOnly,
            hasDiffusionModelIndex,
            modelIndexClass,
            modelIndexKeys,
            evidence);
    }

    internal static string GetModelConfigDirectory(string selectedFolder, string runtimePath, ModelFileKind kind)
    {
        if (kind == ModelFileKind.OpenVinoGenAiDirectory && Directory.Exists(runtimePath))
        {
            return runtimePath;
        }

        if (Directory.Exists(selectedFolder))
        {
            string mainFolder = Path.Combine(selectedFolder, "main");
            if (File.Exists(Path.Combine(mainFolder, "config.json"))
                || File.Exists(Path.Combine(mainFolder, "model_index.json")))
            {
                return mainFolder;
            }

            if (File.Exists(Path.Combine(selectedFolder, "config.json"))
                || File.Exists(Path.Combine(selectedFolder, "model_index.json")))
            {
                return selectedFolder;
            }
        }

        string? runtimeDirectory = File.Exists(runtimePath)
            ? Path.GetDirectoryName(runtimePath)
            : Directory.Exists(runtimePath) ? runtimePath : null;
        return string.IsNullOrWhiteSpace(runtimeDirectory)
            ? selectedFolder
            : runtimeDirectory;
    }

    internal static bool IsVisionLanguageConfig(ModelConfigSnapshot snapshot, string modelDirectory)
    {
        return HasOpenVinoVlmLanguageModel(modelDirectory)
            || snapshot.HasVisionConfig && !snapshot.LanguageModelOnly
            || IsVisionLanguageModelType(snapshot.ModelType)
            || snapshot.Architectures.Any(architecture =>
                architecture.Contains("Vision", StringComparison.OrdinalIgnoreCase)
                || architecture.Contains("VL", StringComparison.OrdinalIgnoreCase)
                || architecture.Contains("ConditionalGeneration", StringComparison.OrdinalIgnoreCase)
                || architecture.Contains("ImageText", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsImageClassificationConfig(ModelConfigSnapshot snapshot)
    {
        return snapshot.Architectures.Any(architecture => architecture.Contains("ForImageClassification", StringComparison.OrdinalIgnoreCase))
            || snapshot.ModelType.Contains("vit", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("resnet", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("swin", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("convnext", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("mobilenet", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("efficientnet", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsObjectDetectionConfig(ModelConfigSnapshot snapshot)
    {
        return snapshot.Architectures.Any(architecture =>
                architecture.Contains("ForObjectDetection", StringComparison.OrdinalIgnoreCase)
                || architecture.Contains("Detection", StringComparison.OrdinalIgnoreCase))
            || snapshot.ModelType.Contains("detr", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("yolo", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("ssd", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsSpeechConfig(ModelConfigSnapshot snapshot)
    {
        return snapshot.ModelType.Contains("whisper", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("wav2vec", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("speech", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("audio", StringComparison.OrdinalIgnoreCase)
            || snapshot.Architectures.Any(architecture =>
                architecture.Contains("Speech", StringComparison.OrdinalIgnoreCase)
                || architecture.Contains("Audio", StringComparison.OrdinalIgnoreCase)
                || architecture.Contains("Whisper", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsTextEmbeddingConfig(ModelConfigSnapshot snapshot, string modelDirectory)
    {
        return File.Exists(Path.Combine(modelDirectory, "openvino_text_embeddings_model.xml"))
            || snapshot.ModelType.Contains("embedding", StringComparison.OrdinalIgnoreCase)
            || snapshot.ModelType.Contains("sentence", StringComparison.OrdinalIgnoreCase)
            || snapshot.Architectures.Any(architecture => architecture.Contains("Embedding", StringComparison.OrdinalIgnoreCase));
    }

    internal static string[] GetPreferredMainRuntimeFiles(string selectedFolder, IReadOnlyList<string> files)
    {
        string mainFolder = selectedFolder.EndsWith(
            $"{Path.DirectorySeparatorChar}main",
            StringComparison.OrdinalIgnoreCase)
            || selectedFolder.EndsWith(
                $"{Path.AltDirectorySeparatorChar}main",
                StringComparison.OrdinalIgnoreCase)
                ? selectedFolder
                : Path.Combine(selectedFolder, "main");

        if (!Directory.Exists(mainFolder))
        {
            return Array.Empty<string>();
        }

        string fullMainFolder = Path.GetFullPath(mainFolder);
        return files
            .Where(path => IsPathUnder(path, fullMainFolder))
            .ToArray();
    }

    internal static string? FindRuntimeFile(IEnumerable<string> files, ModelFileKind kind)
    {
        return files
            .Where(path => GetModelFileKind(path) == kind)
            .Where(path => kind != ModelFileKind.OpenVinoIr || !IsOpenVinoSupportXml(path))
            .OrderBy(path => Path.GetFileName(path).Equals("openvino_model.xml", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(path => Path.GetFileName(path).Equals("openvino_language_model.xml", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(path => path.Length)
            .FirstOrDefault();
    }

    internal static bool IsOpenVinoSupportXml(string path)
    {
        string fileName = Path.GetFileName(path);
        return fileName.Equals("openvino_tokenizer.xml", StringComparison.OrdinalIgnoreCase)
            || fileName.Equals("openvino_detokenizer.xml", StringComparison.OrdinalIgnoreCase)
            || fileName.Equals("openvino_text_embeddings_model.xml", StringComparison.OrdinalIgnoreCase)
            || fileName.StartsWith("openvino_vision_embeddings", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsOpenVinoGenAiDirectory(string directory)
    {
        bool hasLlmModel = File.Exists(Path.Combine(directory, "openvino_model.xml"))
            && File.Exists(Path.Combine(directory, "openvino_model.bin"));
        bool hasVlmLanguageModel = HasOpenVinoVlmLanguageModel(directory);
        if (!hasLlmModel && !hasVlmLanguageModel)
        {
            return false;
        }

        string[] names = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Cast<string>()
            .ToArray();
        bool hasTokenizer = names.Any(name => name.Contains("tokenizer", StringComparison.OrdinalIgnoreCase));
        bool hasConfiguration = names.Any(name => name.Equals("config.json", StringComparison.OrdinalIgnoreCase)
            || name.Equals("generation_config.json", StringComparison.OrdinalIgnoreCase));
        return hasTokenizer && hasConfiguration;
    }

    internal static bool HasOpenVinoVlmLanguageModel(string directory)
    {
        return File.Exists(Path.Combine(directory, "openvino_language_model.xml"))
            && File.Exists(Path.Combine(directory, "openvino_language_model.bin"));
    }

    internal static string DescribeRuntimeModelKind(ModelFileKind kind) => kind switch
    {
        ModelFileKind.OpenVinoGenAiDirectory => "OpenVINO GenAI model directory",
        ModelFileKind.Gguf => "GGUF model",
        ModelFileKind.OpenVinoIr => "OpenVINO IR model",
        ModelFileKind.OpenVinoCoreReadable => "OpenVINO Core-readable model",
        _ => "unsupported model",
    };

    internal static string DescribeModelRunPath(ModelRunPath path) => path switch
    {
        ModelRunPath.TextGeneration => "text generation",
        ModelRunPath.VisionLanguage => "vision-language text generation",
        ModelRunPath.TextToImage => "text-to-image generation",
        ModelRunPath.ImageToImage => "image-to-image generation",
        ModelRunPath.ImageClassification => "image classification",
        ModelRunPath.ObjectDetection => "object detection",
        ModelRunPath.SpeechRecognition => "speech recognition",
        ModelRunPath.TextEmbedding => "text embeddings",
        ModelRunPath.OpenVinoCore => "OpenVINO Core model",
        ModelRunPath.Unsupported => "unsupported",
        _ => "unknown",
    };

    internal static string TryReadString(System.Text.Json.JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
            && property.ValueKind == System.Text.Json.JsonValueKind.String
                ? property.GetString() ?? string.Empty
                : string.Empty;
    }

    internal static ModelFileKind GetModelFileKind(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".xml" => ModelFileKind.OpenVinoIr,
            ".bin" => ModelFileKind.OpenVinoWeights,
            ".onnx" or ".pb" or ".caffemodel" or ".tflite" or ".pdmodel" => ModelFileKind.OpenVinoCoreReadable,
            ".gguf" => ModelFileKind.Gguf,
            ".pt" or ".pth" => ModelFileKind.PyTorchSource,
            ".safetensors" or ".json" or ".model" or ".txt" or ".prototxt" => ModelFileKind.SupportFile,
            _ => ModelFileKind.Unknown,
        };
    }

    internal static string? TryReadJsonString(System.Text.Json.JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
            && property.ValueKind == System.Text.Json.JsonValueKind.String
            ? property.GetString()
            : null;
    }

    internal static bool? TryReadJsonBool(System.Text.Json.JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
            && (property.ValueKind == System.Text.Json.JsonValueKind.True || property.ValueKind == System.Text.Json.JsonValueKind.False)
            ? property.GetBoolean()
            : null;
    }

    internal static IReadOnlyList<string> TryReadJsonStringArray(System.Text.Json.JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
            || property.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return property.EnumerateArray()
            .Where(item => item.ValueKind == System.Text.Json.JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray();
    }

    internal static bool IsVisionLanguageModelType(string modelType)
    {
        return modelType.Equals("qwen2_vl", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("qwen2_5_vl", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("qwen3_vl", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("qwen3_5", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("qwen3_5_moe", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("llava", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("llava_next", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("llava_next_video", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("gemma3", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("gemma4", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("llama4", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("phi3_v", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("phi4mm", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("phi4_multimodal", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("minicpmv", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("minicpmo", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("smolvlm", StringComparison.OrdinalIgnoreCase)
            || modelType.Equals("internvl_chat", StringComparison.OrdinalIgnoreCase);
    }

    internal static string? FindMatchingBinPath(string modelPath)
    {
        string binPath = Path.ChangeExtension(modelPath, ".bin");
        return File.Exists(binPath) ? binPath : null;
    }

}
