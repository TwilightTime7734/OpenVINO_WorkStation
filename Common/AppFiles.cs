

namespace WorkStationC_;

internal static class AppFiles
{

    internal static string FormatBytes(ulong bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int unitIndex = 0;

        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return unitIndex == 0 ? $"{bytes:N0} {units[unitIndex]}" : $"{value:N1} {units[unitIndex]}";
    }

    internal static string FirstText(params string[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    internal static string CollapseNestedMainFolderSelection(string selectedPath, string modelRoot)
    {
        string selected = Path.GetFullPath(selectedPath);
        string root = NormalizeModelRoot(modelRoot);
        if (!IsPathUnder(selected, root))
        {
            return selected;
        }

        DirectoryInfo? cursor = new(selected);
        while (cursor is not null && IsPathUnder(cursor.FullName, root))
        {
            if (cursor.Name.Equals("main", StringComparison.OrdinalIgnoreCase)
                && cursor.Parent is not null
                && !AreSameDirectory(cursor.Parent.FullName, root))
            {
                return cursor.Parent.FullName;
            }

            if (AreSameDirectory(cursor.FullName, root))
            {
                break;
            }

            cursor = cursor.Parent;
        }

        return selected;
    }

    internal static bool AreSameDirectory(string firstPath, string secondPath)
    {
        return NormalizeDirectoryPathForComparison(firstPath)
            .Equals(NormalizeDirectoryPathForComparison(secondPath), StringComparison.OrdinalIgnoreCase);
    }

    internal static string GetModelRoot()
    {
        string modelRoot = GetConfiguredModelRoot();
        EnsureModelRootFolders(modelRoot);
        return modelRoot;
    }

    internal static string GetConfiguredModelRoot()
    {
        string path = GetWorkbenchConfigPath();
        string? value = File.Exists(path) ? File.ReadLines(path)
            .Select(line => line.Split('=', 2, StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2 && parts[0].Equals("modelRoot", StringComparison.OrdinalIgnoreCase))
            .Select(parts => parts[1]).LastOrDefault() : null;
        return string.IsNullOrWhiteSpace(value) ? GetDefaultModelRoot() : NormalizeModelRoot(value);
    }

    internal static string GetDefaultModelRoot()
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string baseFolder = string.IsNullOrWhiteSpace(documents)
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            : documents;

        return Path.Combine(baseFolder, "OpenVINO Workbench Models");
    }

    internal static string GetWorkbenchConfigPath()
    {
        return Path.Combine(GetProjectDirectory(), "workbench.cfg");
    }

    internal static string NormalizeModelRoot(string modelRoot)
    {
        if (string.IsNullOrWhiteSpace(modelRoot))
        {
            return GetDefaultModelRoot();
        }

        string expanded = Environment.ExpandEnvironmentVariables(modelRoot.Trim());
        return Path.GetFullPath(expanded);
    }

    internal static void SaveModelRoot(string modelRoot)
    {
        string path = GetWorkbenchConfigPath();
        var lines = File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string>();
        lines.RemoveAll(line => line.Split('=', 2)[0].Trim().Equals("modelRoot", StringComparison.OrdinalIgnoreCase));
        lines.Add($"modelRoot={NormalizeModelRoot(modelRoot)}");
        WriteTextAtomically(path, string.Join(Environment.NewLine, lines) + Environment.NewLine);
    }

    internal static void EnsureModelRootFolders(string modelRoot)
    {
        string root = NormalizeModelRoot(modelRoot);
        Directory.CreateDirectory(root);
    }

    internal static string GetProjectDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WorkStationC#.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? AppContext.BaseDirectory;
    }

    internal static bool TryNormalizeDownloadWebAddress(string text, out Uri? uri, out string error)
    {
        uri = null;
        string urlText = text.Trim();
        if (string.IsNullOrWhiteSpace(urlText))
        {
            error = "enter a web address first.";
            return false;
        }

        if (!urlText.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !urlText.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            urlText = $"https://{urlText.TrimStart('/')}";
        }

        if (!Uri.TryCreate(urlText, UriKind.Absolute, out Uri? parsed)
            || (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(parsed.Host))
        {
            error = "enter a normal web address, like https://huggingface.co/Qwen/Qwen3-Coder-Next-FP8/tree/main.";
            return false;
        }

        uri = parsed;
        error = string.Empty;
        return true;
    }

    internal static string NormalizeTextFileLineEndings(string text)
    {
        string normalized = text.Replace("\r\n", "\r\n").Replace("\r", "\r\n").Replace("\r\n", Environment.NewLine);
        return normalized.EndsWith(Environment.NewLine, StringComparison.Ordinal)
            ? normalized
            : normalized + Environment.NewLine;
    }

    internal static string BuildModelFileDownloadPath(string repoId, string fileName, string revision)
    {
        string fullRoot = Path.GetFullPath(GetModelRoot());
        string[] fileSegments = fileName
            .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(SanitizePathSegment)
            .ToArray();

        if (fileSegments.Length == 0)
        {
            throw new InvalidOperationException("The selected file name is empty.");
        }

        string downloadDirectory = BuildModelDownloadDirectory(repoId, revision);
        if (!IsPathUnder(downloadDirectory, fullRoot))
        {
            throw new InvalidOperationException("The repository path resolved outside the model library.");
        }

        string targetPath = Path.Combine(new[] { downloadDirectory }.Concat(fileSegments).ToArray());
        string fullTarget = Path.GetFullPath(targetPath);

        if (!IsPathUnder(fullTarget, fullRoot))
        {
            throw new InvalidOperationException("The selected file path resolved outside the model library.");
        }

        return fullTarget;
    }

    internal static string BuildModelDownloadDirectory(string repoId, string revision)
    {
        string modelFolderName = GetDownloadModelFolderName(repoId);
        return Path.GetFullPath(Path.Combine(GetModelRoot(), modelFolderName, "main"));
    }

    internal static string GetDownloadModelFolderName(string modelId)
    {
        string modelName = modelId
            .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault() ?? modelId;
        return SanitizePathSegment(modelName);
    }

    internal static string BuildDirectWebFileDownloadPath(Uri uri, string fileName)
    {
        string modelName = BuildDirectWebFileModelFolderName(uri, fileName);
        return Path.GetFullPath(Path.Combine(GetModelRoot(), modelName, "main", SanitizePathSegment(fileName)));
    }

    internal static string BuildDirectWebFileModelFolderName(Uri uri, string fileName)
    {
        string fileStem = Path.GetFileNameWithoutExtension(fileName);
        string[] pathSegments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.UnescapeDataString)
            .ToArray();
        string parentSegment = pathSegments.Length >= 2 ? pathSegments[^2] : string.Empty;
        string candidate = IsGenericDirectModelFileStem(fileStem) && !string.IsNullOrWhiteSpace(parentSegment)
            ? parentSegment
            : fileStem;

        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host[4..] : uri.Host;
        }

        return SanitizePathSegment(candidate);
    }

    internal static bool IsGenericDirectModelFileStem(string fileStem)
    {
        return fileStem.Equals("model", StringComparison.OrdinalIgnoreCase)
            || fileStem.Equals("openvino_model", StringComparison.OrdinalIgnoreCase)
            || fileStem.Equals("pytorch_model", StringComparison.OrdinalIgnoreCase)
            || fileStem.Equals("tf_model", StringComparison.OrdinalIgnoreCase)
            || fileStem.Equals("weights", StringComparison.OrdinalIgnoreCase)
            || fileStem.Equals("config", StringComparison.OrdinalIgnoreCase)
            || fileStem.Equals("tokenizer", StringComparison.OrdinalIgnoreCase);
    }

    internal static string SanitizePathSegment(string segment)
    {
        string sanitized = segment.Trim();
        if (sanitized is "." or "..")
        {
            throw new InvalidOperationException($"Unsafe relative path segment `{sanitized}` was rejected.");
        }

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        sanitized = sanitized.Replace(':', '_').TrimEnd(' ', '.');
        if (sanitized is "." or "..")
        {
            throw new InvalidOperationException("Unsafe relative path segment was rejected.");
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "_" : sanitized;
    }

    internal static void ValidateDownloadDestination(string targetPath)
    {
        string fullRoot = Path.GetFullPath(GetModelRoot());
        string fullTarget = Path.GetFullPath(targetPath);
        if (!IsPathUnder(fullTarget, fullRoot))
        {
            throw new InvalidOperationException("The download target resolved outside the model library.");
        }

        string? targetDirectory = Path.GetDirectoryName(fullTarget);
        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            throw new InvalidOperationException("The download target has no parent directory.");
        }

        string relativeDirectory = Path.GetRelativePath(fullRoot, targetDirectory);
        string currentDirectory = fullRoot;
        foreach (string segment in relativeDirectory.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
            StringSplitOptions.RemoveEmptyEntries))
        {
            currentDirectory = Path.Combine(currentDirectory, segment);
            if (Directory.Exists(currentDirectory)
                && (File.GetAttributes(currentDirectory) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException($"Download refused because `{currentDirectory}` is a symbolic link or junction.");
            }
        }

        if ((File.Exists(fullTarget) || Directory.Exists(fullTarget))
            && (File.GetAttributes(fullTarget) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidOperationException("Download refused because the target is a symbolic link or junction.");
        }
    }

    internal static string FormatCommandArguments(IEnumerable<string> arguments)
    {
        return string.Join(" ", arguments.Select(QuoteCommandArgument));
    }

    internal static string QuoteCommandArgument(string argument)
    {
        return argument.Any(char.IsWhiteSpace) || argument.Contains('"', StringComparison.Ordinal)
            ? $"\"{argument.Replace("\"", "\\\"", StringComparison.Ordinal)}\""
            : argument;
    }

    internal static void WriteTextAtomically(string path, string contents)
    {
        string fullPath = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException($"No parent directory exists for: {fullPath}");
        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.partial");
        try
        {
            File.WriteAllText(temporaryPath, contents);
            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    internal static bool IsPathUnder(string candidatePath, string rootPath)
    {
        string candidate = Path.GetFullPath(candidatePath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string root = Path.GetFullPath(rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    internal static string NormalizeDirectoryPathForComparison(string path)
    {
        return Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
    }

    internal static IEnumerable<string> SplitTextLines(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    internal static string QuoteCommandPromptArgument(string value)
    {
        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }

    internal static string ExpandPathText(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : Environment.ExpandEnvironmentVariables(value.Trim().Trim('"'));
    }

}
