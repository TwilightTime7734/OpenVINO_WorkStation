using static WorkStationC_.AppFiles;

namespace WorkStationC_;

internal static class OvmsPull
{
    internal const string OvmsSourceFormatExport = "HF export";

    internal const string OvmsSourceFormatGguf = "GGUF";

    internal sealed class OvmsPullDefaults
    {
        public string OvmsExecutable { get; set; } = string.Empty;
        public string PythonRuntimePath { get; set; } = string.Empty;
        public string ExportToolsPath { get; set; } = string.Empty;
        public string SourceFormat { get; set; } = string.Empty;
        public string Task { get; set; } = "text_generation";
        public string PipelineType { get; set; } = "VLM";
        public string SourceModel { get; set; } = string.Empty;
        public string GgufFilename { get; set; } = string.Empty;
        public string ModelRepositoryPath { get; set; } = string.Empty;
        public string WeightFormat { get; set; } = "int8";
        public string LogPath { get; set; } = "C:\\Example\\OpenVINO\\qwen35-4b-pull-global.log";
        public bool OverwriteModels { get; set; } = true;
        public bool DebugLogging { get; set; } = true;
        public List<string> SourceFormats { get; set; } = new();
        public List<string> Tasks { get; set; } = new();
        public List<string> PipelineTypes { get; set; } = new();
        public List<string> SourceModels { get; set; } = new();
        public List<string> WeightFormats { get; set; } = new();
        public List<OvmsPullCommandBlock> Commands { get; set; } = new();

        public List<OvmsVersionExpectation> Versions { get; set; } = new();
    }

    internal sealed class OvmsPullCommandBlock
    {
        public string OvmsExecutable { get; set; } = string.Empty;
        public string SourceFormat { get; set; } = string.Empty;
        public string Task { get; set; } = string.Empty;
        public string PipelineType { get; set; } = string.Empty;
        public string SourceModel { get; set; } = string.Empty;
        public string GgufFilename { get; set; } = string.Empty;
        public string ModelRepositoryPath { get; set; } = string.Empty;
        public string WeightFormat { get; set; } = string.Empty;
        public string LogLevel { get; set; } = string.Empty;
        public string LogPath { get; set; } = string.Empty;
        public bool OverwriteModels { get; set; }
        public bool HasArguments =>
            !string.IsNullOrWhiteSpace(Task)
            || !string.IsNullOrWhiteSpace(PipelineType)
            || !string.IsNullOrWhiteSpace(SourceModel)
            || !string.IsNullOrWhiteSpace(GgufFilename)
            || !string.IsNullOrWhiteSpace(ModelRepositoryPath)
            || !string.IsNullOrWhiteSpace(WeightFormat)
            || !string.IsNullOrWhiteSpace(LogPath)
            || OverwriteModels;
    }

    internal sealed class OvmsPullPlan
    {
        public required string OvmsExecutable { get; init; }
        public required string PythonRuntimePath { get; init; }
        public required string ExportToolsPath { get; init; }
        public required string ModelRepositoryPath { get; init; }
        public required string SourceModel { get; init; }
        public string GgufFilename { get; init; } = string.Empty;
        public bool IsGgufPull { get; init; }
        public required string ExpectedModelFolder { get; init; }
        public string LogPath { get; init; } = string.Empty;
        public required IReadOnlyList<string> Arguments { get; init; }
    }

    internal sealed record OvmsVersionExpectation(
        string Name,
        string ExpectedVersion,
        bool Required,
        bool Strict);

    internal sealed class OvmsStackCheckResult
    {
        public bool CanRunPull { get; set; } = true;
        public bool HasWarnings { get; set; }
        public string ExportPythonPath { get; set; } = string.Empty;
        public List<string> Lines { get; } = new();
        public List<string> RepairCommands { get; } = new();
    }

    internal sealed class OvmsCommandResult
    {
        public int ExitCode { get; init; }
        public bool TimedOut { get; init; }
        public string StandardOutput { get; init; } = string.Empty;
        public string StandardError { get; init; } = string.Empty;
        public string StartError { get; init; } = string.Empty;
        public bool Started => string.IsNullOrWhiteSpace(StartError);
        public bool Succeeded => Started && !TimedOut && ExitCode == 0;
    }

    internal static OvmsPullDefaults LoadSettings()
    {
        string path = Path.Combine(GetProjectDirectory(), "ovms-settings.json");
        return System.Text.Json.JsonSerializer.Deserialize<OvmsPullDefaults>(File.ReadAllText(path))
            ?? throw new InvalidDataException("OVMS settings are empty.");
    }

    internal static async Task<OvmsStackCheckResult> CheckOvmsPullVersionStackAsync(OvmsPullPlan plan)
    {
        OvmsStackCheckResult result = new();
        IReadOnlyList<string> pathEntries = BuildOvmsPathPrefixEntries(plan);
        IReadOnlyList<OvmsVersionExpectation> expectations = LoadVersionExpectations();
        Dictionary<string, OvmsVersionExpectation> expectationsByName = expectations
            .ToDictionary(expectation => NormalizePythonPackageName(expectation.Name), StringComparer.OrdinalIgnoreCase);

        result.Lines.Add(string.Empty);
        result.Lines.Add("OVMS export stack check");
        result.Lines.Add("------------------------------------------------------------");
        result.Lines.Add($"OVMS program: {plan.OvmsExecutable}");
        result.Lines.Add($"Bundled Python DLL folder: {plan.PythonRuntimePath}");
        result.Lines.Add(plan.IsGgufPull
            ? "Source type: GGUF direct pull; Optimum/Transformers export packages are not used."
            : "Source type: Hugging Face export through OVMS/Optimum.");

        if (!File.Exists(plan.OvmsExecutable))
        {
            AddStackFailure(result, $"OVMS program not found: {plan.OvmsExecutable}");
        }

        if (!Directory.Exists(plan.PythonRuntimePath))
        {
            AddStackFailure(result, $"Bundled Python DLL folder not found: {plan.PythonRuntimePath}");
        }
        else
        {
            string pythonDll = Path.Combine(plan.PythonRuntimePath, "python312.dll");
            if (File.Exists(pythonDll))
            {
                result.Lines.Add($"OK: OVMS Python DLL found: {pythonDll}");
            }
            else
            {
                AddStackFailure(result, $"Expected OVMS Python DLL was not found: {pythonDll}");
            }
        }

        if (File.Exists(plan.OvmsExecutable) && Directory.Exists(plan.PythonRuntimePath))
        {
            OvmsCommandResult ovmsVersion = await RunOvmsCommandAsync(
                plan.OvmsExecutable,
                "--version",
                Path.GetDirectoryName(plan.OvmsExecutable) ?? GetProjectDirectory(),
                new[] { plan.PythonRuntimePath },
                timeoutMilliseconds: 15000);
            AppendCommandCheckOutput(result, "ovms --version", ovmsVersion);

            if (ovmsVersion.Succeeded
                && expectationsByName.TryGetValue("openvino", out OvmsVersionExpectation? openvinoExpectation)
                && !string.IsNullOrWhiteSpace(openvinoExpectation.ExpectedVersion)
                && !CombineCommandOutput(ovmsVersion).Contains(openvinoExpectation.ExpectedVersion, StringComparison.OrdinalIgnoreCase))
            {
                AddStackWarning(result, $"OVMS version output did not include expected OpenVINO version {openvinoExpectation.ExpectedVersion}. Confirm this OVMS install matches the working C:\\Example\\OpenVINO package.");
            }
        }

        if (plan.IsGgufPull)
        {
            result.Lines.Add("Skipped: GGUF pull does not need global optimum-cli, Transformers, or Optimum Intel.");
            FinishOvmsStackCheck(result, string.Empty, expectations);

            return result;
        }

        result.Lines.Add($"Export tools folder: {plan.ExportToolsPath}");
        string? exportPython = FindOvmsExportPythonPath(pathEntries);
        if (string.IsNullOrWhiteSpace(exportPython))
        {
            AddStackFailure(result, "Export Python was not found. The export tools folder should point at the global Python Scripts folder used by the successful conversion.");
        }
        else
        {
            result.ExportPythonPath = exportPython;
            result.Lines.Add($"Export Python: {exportPython}");
        }

        string? optimumCli = FindExecutableOnPath("optimum-cli.exe", pathEntries);
        if (string.IsNullOrWhiteSpace(optimumCli))
        {
            AddStackFailure(result, "optimum-cli.exe was not found on the generated PATH.");
        }
        else
        {
            result.Lines.Add($"OK: optimum-cli.exe: {optimumCli}");
        }

        if (string.IsNullOrWhiteSpace(exportPython))
        {
            FinishOvmsStackCheck(result, exportPython ?? string.Empty, expectations);

            return result;
        }

        OvmsCommandResult pythonVersion = await RunOvmsCommandAsync(
            exportPython,
            "--version",
            Path.GetDirectoryName(exportPython) ?? GetProjectDirectory(),
            pathEntries,
            timeoutMilliseconds: 15000);
        AppendCommandCheckOutput(result, "python --version", pythonVersion);

        string actualPythonVersion = ParsePythonVersion(CombineCommandOutput(pythonVersion));
        if (expectationsByName.TryGetValue("python", out OvmsVersionExpectation? pythonExpectation))
        {
            CompareVersion(result, pythonExpectation, actualPythonVersion);
        }

        OvmsCommandResult pipShow = await RunOvmsCommandAsync(
            exportPython,
            "-m pip show transformers optimum-intel optimum openvino openvino-genai huggingface-hub accelerate",
            Path.GetDirectoryName(exportPython) ?? GetProjectDirectory(),
            pathEntries,
            timeoutMilliseconds: 30000);
        AppendCommandCheckOutput(result, "python -m pip show ...", pipShow);

        Dictionary<string, string> packageVersions = ParsePipShowVersions(CombineCommandOutput(pipShow));
        foreach (OvmsVersionExpectation expectation in expectations.Where(expectation => !NormalizePythonPackageName(expectation.Name).Equals("python", StringComparison.OrdinalIgnoreCase)))
        {
            string normalizedName = NormalizePythonPackageName(expectation.Name);
            packageVersions.TryGetValue(normalizedName, out string? actualVersion);
            CompareVersion(result, expectation, actualVersion ?? string.Empty);
        }

        foreach (string observedPackage in new[] { "openvino-genai", "accelerate", "huggingface-hub" })
        {
            if (packageVersions.TryGetValue(observedPackage, out string? observedVersion))
            {
                result.Lines.Add($"Observed: {observedPackage} {observedVersion}");
            }
        }

        FinishOvmsStackCheck(result, exportPython, expectations);

        return result;
    }

    internal static void AddStackFailure(OvmsStackCheckResult result, string message)
    {
        result.CanRunPull = false;
        result.Lines.Add($"FAIL: {message}");
    }

    internal static void AddStackWarning(OvmsStackCheckResult result, string message)
    {
        result.HasWarnings = true;
        result.Lines.Add($"WARN: {message}");
    }

    internal static void CompareVersion(OvmsStackCheckResult result, OvmsVersionExpectation expectation, string actualVersion)
    {
        string normalizedName = NormalizePythonPackageName(expectation.Name);
        if (string.IsNullOrWhiteSpace(actualVersion))
        {
            if (expectation.Required)
            {
                AddStackFailure(result, $"{expectation.Name} is missing; expected {expectation.ExpectedVersion}.");
            }
            else
            {
                AddStackWarning(result, $"{expectation.Name} is not installed; expected/observed working version was {expectation.ExpectedVersion}.");
            }

            return;
        }

        if (!expectation.Strict || string.IsNullOrWhiteSpace(expectation.ExpectedVersion))
        {
            result.Lines.Add($"INFO: {expectation.Name} {actualVersion} installed.");
            return;
        }

        if (actualVersion.Equals(expectation.ExpectedVersion, StringComparison.OrdinalIgnoreCase))
        {
            result.Lines.Add($"OK: {expectation.Name} {actualVersion}");
            return;
        }

        string message = $"{expectation.Name} is {actualVersion}; expected {expectation.ExpectedVersion}.";
        if (expectation.Required)
        {
            AddStackFailure(result, message);
        }
        else
        {
            AddStackWarning(result, message);
        }

        _ = normalizedName;
    }

    internal static void AppendCommandCheckOutput(OvmsStackCheckResult result, string label, OvmsCommandResult commandResult)
    {
        if (!commandResult.Started)
        {
            AddStackFailure(result, $"{label} could not start: {commandResult.StartError}");
            return;
        }

        if (commandResult.TimedOut)
        {
            AddStackFailure(result, $"{label} timed out.");
            return;
        }

        if (commandResult.ExitCode != 0)
        {
            AddStackFailure(result, $"{label} exited with code {commandResult.ExitCode}.");
        }
        else
        {
            result.Lines.Add($"OK: {label}");
        }

        foreach (string line in GetFirstNonEmptyCommandLines(commandResult, maxLines: 8))
        {
            result.Lines.Add($"  {line}");
        }
    }

    internal static void FinishOvmsStackCheck(
        OvmsStackCheckResult result,
        string exportPythonPath,
        IReadOnlyList<OvmsVersionExpectation> expectations)
    {
        if (!result.CanRunPull)
        {
            result.Lines.Add("Result: not ready for OVMS export. Fix the items above before starting a non-GGUF pull.");
            foreach (string command in BuildOvmsStackRepairCommands(exportPythonPath, expectations))
            {
                result.RepairCommands.Add(command);
            }

            return;
        }

        result.Lines.Add(result.HasWarnings
            ? "Result: stack is usable, but review the warnings above."
            : "Result: stack matches the known-good OVMS export setup.");
    }

    internal static IReadOnlyList<string> BuildOvmsPullArguments(
        string task,
        string pipelineType,
        string sourceModel,
        string ggufFilename,
        string modelRepositoryPath,
        string weightFormat,
        string logPath,
        bool overwriteModels,
        bool debugLogging,
        bool isGgufPull)
    {
        List<string> arguments = new()
        {
            "--pull",
            "--task",
            task,
            "--source_model",
            sourceModel,
            "--model_repository_path",
            modelRepositoryPath,
        };

        if (isGgufPull)
        {
            arguments.Add("--gguf_filename");
            arguments.Add(ggufFilename);
        }
        else
        {
            arguments.Add("--pipeline_type");
            arguments.Add(pipelineType);
            arguments.Add("--weight-format");
            arguments.Add(weightFormat);
        }

        if (overwriteModels)
        {
            arguments.Add("--overwrite_models");
        }

        arguments.Add("--log_level");
        arguments.Add(debugLogging ? "DEBUG" : "INFO");

        if (!string.IsNullOrWhiteSpace(logPath))
        {
            arguments.Add("--log_path");
            arguments.Add(logPath);
        }

        return arguments;
    }

    internal static void ValidateOvmsPullPlan(OvmsPullPlan plan)
    {
        if (!File.Exists(plan.OvmsExecutable))
        {
            throw new FileNotFoundException("OVMS program was not found.", plan.OvmsExecutable);
        }

        if (!Directory.Exists(plan.PythonRuntimePath))
        {
            throw new DirectoryNotFoundException($"Python DLL folder was not found: {plan.PythonRuntimePath}");
        }

        if (plan.IsGgufPull)
        {
            return;
        }

        string[] exportToolPaths = SplitPathList(plan.ExportToolsPath).ToArray();
        if (exportToolPaths.Length == 0)
        {
            throw new DirectoryNotFoundException("No export tools folder was provided.");
        }

        foreach (string exportToolPath in exportToolPaths)
        {
            if (!Directory.Exists(exportToolPath))
            {
                throw new DirectoryNotFoundException($"Export tools folder was not found: {exportToolPath}");
            }
        }
    }

    internal static string WriteOvmsPullCommandScript(OvmsPullPlan plan)
    {
        string scriptPath = BuildOvmsPullCommandScriptPath(plan);
        Directory.CreateDirectory(Path.GetDirectoryName(scriptPath)
            ?? throw new InvalidOperationException("No parent folder exists for the OVMS command script."));

        IReadOnlyList<string> pathEntries = BuildOvmsPathPrefixEntries(plan);
        string pathPrefix = string.Join(Path.PathSeparator, pathEntries);
        string commandLine = BuildBatchCommandLine(plan.OvmsExecutable, plan.Arguments);
        string? exportPython = FindOvmsExportPythonPath(pathEntries);

        System.Text.StringBuilder builder = new();
        builder.AppendLine("@echo off");
        builder.AppendLine($"title OVMS Pull - {plan.SourceModel}");
        builder.AppendLine($"cd /d {QuoteBatchArgument(Path.GetDirectoryName(plan.OvmsExecutable) ?? GetProjectDirectory())}");
        builder.AppendLine($"set \"PATH={pathPrefix};%PATH%\"");
        builder.AppendLine("echo OVMS Pull");
        builder.AppendLine($"echo Source model: {plan.SourceModel}");
        if (plan.IsGgufPull)
        {
            builder.AppendLine($"echo GGUF file: {plan.GgufFilename}");
        }

        builder.AppendLine($"echo Model folder: {plan.ModelRepositoryPath}");
        builder.AppendLine($"echo Expected output: {plan.ExpectedModelFolder}");
        if (!string.IsNullOrWhiteSpace(plan.LogPath))
        {
            builder.AppendLine($"echo Log file: {plan.LogPath}");
        }

        builder.AppendLine("echo.");
        builder.AppendLine("echo PATH check:");
        builder.AppendLine("where python.exe");
        if (!plan.IsGgufPull)
        {
            builder.AppendLine("where optimum-cli.exe");
            if (!string.IsNullOrWhiteSpace(exportPython))
            {
                builder.AppendLine("echo.");
                builder.AppendLine("echo Export package versions:");
                builder.AppendLine($"{QuoteBatchArgument(exportPython)} --version");
                builder.AppendLine($"{QuoteBatchArgument(exportPython)} -m pip show transformers optimum-intel optimum openvino openvino-genai huggingface-hub accelerate");
            }
        }

        builder.AppendLine("echo.");
        builder.AppendLine("echo Running:");
        builder.AppendLine($"echo {commandLine}");
        builder.AppendLine("echo.");
        builder.AppendLine(commandLine);
        builder.AppendLine("set \"OVMS_EXIT=%ERRORLEVEL%\"");
        builder.AppendLine("echo.");
        builder.AppendLine("echo OVMS exited with code %OVMS_EXIT%.");
        builder.AppendLine($"if exist {QuoteBatchArgument(Path.Combine(plan.ExpectedModelFolder, "graph.pbtxt"))} echo Graph found: {Path.Combine(plan.ExpectedModelFolder, "graph.pbtxt")}");
        builder.AppendLine("echo.");
        builder.AppendLine("echo Close this Command Prompt when you are done.");
        builder.AppendLine("exit /b %OVMS_EXIT%");

        WriteTextAtomically(scriptPath, NormalizeTextFileLineEndings(builder.ToString()));
        return scriptPath;
    }

    internal static string BuildOvmsPullCommandScriptPath(OvmsPullPlan plan)
    {
        string folder = Path.Combine(Path.GetTempPath(), "OpenVINO-Workbench");
        string sourceName = SanitizePathSegment(plan.SourceModel.Replace('/', '-').Replace('\\', '-'));
        return Path.Combine(folder, $"ovms-pull-{sourceName}.cmd");
    }

    internal static IReadOnlyList<string> BuildOvmsPathPrefixEntries(OvmsPullPlan plan)
    {
        List<string> entries = new()
        {
            plan.PythonRuntimePath,
        };

        if (!plan.IsGgufPull)
        {
            foreach (string exportPath in SplitPathList(plan.ExportToolsPath))
            {
                entries.Add(exportPath);
                if (Path.GetFileName(exportPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                    .Equals("Scripts", StringComparison.OrdinalIgnoreCase)
                    && Directory.GetParent(exportPath) is DirectoryInfo parent)
                {
                    entries.Add(parent.FullName);
                }
            }
        }

        return entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .Select(entry => Path.GetFullPath(entry))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    internal static string? FindOvmsExportPythonPath(IEnumerable<string> pathEntries)
    {
        foreach (string pathEntry in pathEntries)
        {
            string fullEntry = Path.GetFullPath(pathEntry);
            string directPython = Path.Combine(fullEntry, "python.exe");
            if (File.Exists(directPython))
            {
                return directPython;
            }

            if (Path.GetFileName(fullEntry.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                .Equals("Scripts", StringComparison.OrdinalIgnoreCase)
                && Directory.GetParent(fullEntry) is DirectoryInfo parent)
            {
                string parentPython = Path.Combine(parent.FullName, "python.exe");
                if (File.Exists(parentPython))
                {
                    return parentPython;
                }
            }
        }

        return null;
    }

    internal static string? FindExecutableOnPath(string executableName, IEnumerable<string> pathEntries)
    {
        foreach (string pathEntry in pathEntries)
        {
            string fullEntry = Path.GetFullPath(pathEntry);
            string candidate = Path.Combine(fullEntry, executableName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    internal static IReadOnlyList<OvmsVersionExpectation> LoadVersionExpectations()
    {
        return LoadSettings().Versions;
    }

    internal static async Task<OvmsCommandResult> RunOvmsCommandAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        IEnumerable<string> pathPrefixEntries,
        int timeoutMilliseconds)
    {
        System.Diagnostics.ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = Directory.Exists(workingDirectory) ? workingDirectory : GetProjectDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        string pathPrefix = string.Join(
            Path.PathSeparator,
            pathPrefixEntries
                .Where(entry => !string.IsNullOrWhiteSpace(entry))
                .Select(entry => Path.GetFullPath(entry))
                .Distinct(StringComparer.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(pathPrefix))
        {
            string existingPath = startInfo.EnvironmentVariables["PATH"] ?? Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            startInfo.EnvironmentVariables["PATH"] = pathPrefix + Path.PathSeparator + existingPath;
        }

        using System.Diagnostics.Process process = new() { StartInfo = startInfo };
        try
        {
            if (!process.Start())
            {
                return new OvmsCommandResult { ExitCode = -1, StartError = "Process.Start returned false." };
            }
        }
        catch (Exception ex)
        {
            return new OvmsCommandResult { ExitCode = -1, StartError = ex.Message };
        }

        Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();
        Task waitTask = process.WaitForExitAsync();
        Task finishedTask = await Task.WhenAny(waitTask, Task.Delay(timeoutMilliseconds));
        bool timedOut = finishedTask != waitTask;
        if (timedOut)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // The process may have exited between the timeout and the kill attempt.
            }

            await process.WaitForExitAsync();
        }
        else
        {
            await waitTask;
        }

        string standardOutput = await standardOutputTask;
        string standardError = await standardErrorTask;
        return new OvmsCommandResult
        {
            ExitCode = timedOut ? -1 : process.ExitCode,
            TimedOut = timedOut,
            StandardOutput = standardOutput,
            StandardError = standardError,
        };
    }

    internal static string CombineCommandOutput(OvmsCommandResult commandResult)
    {
        return string.Join(
            Environment.NewLine,
            new[] { commandResult.StandardOutput, commandResult.StandardError }.Where(text => !string.IsNullOrWhiteSpace(text)));
    }

    internal static IEnumerable<string> GetFirstNonEmptyCommandLines(OvmsCommandResult commandResult, int maxLines)
    {
        return SplitTextLines(CombineCommandOutput(commandResult))
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(maxLines);
    }

    internal static string ParsePythonVersion(string commandOutput)
    {
        foreach (string rawLine in SplitTextLines(commandOutput))
        {
            string line = rawLine.Trim();
            if (line.StartsWith("Python ", StringComparison.OrdinalIgnoreCase))
            {
                return line["Python ".Length..].Trim();
            }
        }

        return string.Empty;
    }

    internal static Dictionary<string, string> ParsePipShowVersions(string commandOutput)
    {
        Dictionary<string, string> versions = new(StringComparer.OrdinalIgnoreCase);
        string currentName = string.Empty;
        string currentVersion = string.Empty;

        foreach (string rawLine in SplitTextLines(commandOutput))
        {
            string line = rawLine.Trim();
            if (line.StartsWith("Name:", StringComparison.OrdinalIgnoreCase))
            {
                StoreCurrentPackage();
                currentName = NormalizePythonPackageName(line["Name:".Length..].Trim());
                currentVersion = string.Empty;
                continue;
            }

            if (line.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))
            {
                currentVersion = line["Version:".Length..].Trim();
            }
        }

        StoreCurrentPackage();
        return versions;

        void StoreCurrentPackage()
        {
            if (!string.IsNullOrWhiteSpace(currentName) && !string.IsNullOrWhiteSpace(currentVersion))
            {
                versions[currentName] = currentVersion;
            }
        }
    }

    internal static string NormalizePythonPackageName(string name)
    {
        return name.Trim().Replace('_', '-').ToLowerInvariant();
    }

    internal static IReadOnlyList<string> BuildOvmsStackRepairCommands(
        string exportPythonPath,
        IReadOnlyList<OvmsVersionExpectation> expectations)
    {
        string python = string.IsNullOrWhiteSpace(exportPythonPath)
            ? "python"
            : QuoteCommandPromptArgument(exportPythonPath);
        Dictionary<string, OvmsVersionExpectation> expectationsByName = expectations
            .ToDictionary(expectation => NormalizePythonPackageName(expectation.Name), StringComparer.OrdinalIgnoreCase);
        string[] pipPackageOrder =
        {
            "transformers",
            "optimum",
            "openvino",
        };

        List<string> packages = new();
        foreach (string packageName in pipPackageOrder)
        {
            if (expectationsByName.TryGetValue(packageName, out OvmsVersionExpectation? expectation)
                && !string.IsNullOrWhiteSpace(expectation.ExpectedVersion))
            {
                packages.Add(QuoteCommandPromptArgument($"{packageName}=={expectation.ExpectedVersion}"));
            }
        }

        List<string> commands = new();
        if (packages.Count > 0)
        {
            commands.Add($"{python} -m pip install --upgrade --force-reinstall {string.Join(" ", packages)}");
        }

        commands.Add($"{python} -m pip install --upgrade --force-reinstall --no-deps \"https://github.com/huggingface/optimum-intel/archive/refs/heads/main.zip\"");
        return commands;
    }

    internal static IEnumerable<string> SplitPathList(string pathList)
    {
        return pathList
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => Environment.ExpandEnvironmentVariables(path.Trim().Trim('"')))
            .Where(path => !string.IsNullOrWhiteSpace(path));
    }

    internal static string BuildBatchCommandLine(string executable, IEnumerable<string> arguments)
    {
        return QuoteBatchArgument(executable) + " " + string.Join(" ", arguments.Select(QuoteBatchArgument));
    }

    internal static string QuoteBatchArgument(string value)
    {
        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    internal static string BuildExpectedOvmsModelFolder(string modelRepositoryPath, string sourceModel)
    {
        string path = Path.GetFullPath(modelRepositoryPath);
        string[] parts = sourceModel
            .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(SanitizePathSegment)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (parts.Length == 0)
        {
            return Path.Combine(path, "model");
        }

        foreach (string part in parts)
        {
            path = Path.Combine(path, part);
        }

        return path;
    }

}
