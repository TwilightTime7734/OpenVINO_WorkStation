using System.Diagnostics;
using static WorkStationC_.AppFiles;
using static WorkStationC_.OvmsPull;

namespace WorkStationC_;

internal sealed class OvmsTab(TextBox executable, TextBox python, TextBox exportTools,
    ComboBox sourceFormat, ComboBox task, ComboBox pipeline, ComboBox source, ComboBox weights,
    TextBox gguf, TextBox repository, TextBox log, CheckBox overwrite, CheckBox debug,
    Label status, Action<string> output)
{
    private OvmsPullDefaults? _settings;
    private bool _initializing;

    public void Initialize()
    {
        var settings = LoadSettings();
        _settings = settings;
        _initializing = true;
        executable.Text = settings.OvmsExecutable;
        python.Text = settings.PythonRuntimePath;
        exportTools.Text = settings.ExportToolsPath;
        Fill(sourceFormat, settings.SourceFormats, settings.SourceFormat);
        Fill(task, settings.Tasks, settings.Task);
        Fill(pipeline, settings.PipelineTypes, settings.PipelineType);
        Fill(weights, settings.WeightFormats, settings.WeightFormat);
        Fill(source, settings.SourceModels, settings.SourceModel);
        gguf.Text = settings.GgufFilename;
        repository.Text = settings.ModelRepositoryPath;
        log.Text = settings.LogPath;
        overwrite.Checked = settings.OverwriteModels;
        debug.Checked = settings.DebugLogging;
        _initializing = false;
    }

    private static void Fill(ComboBox control, IEnumerable<string> values, string selected)
    {
        control.Items.Clear();
        control.Items.AddRange(values.ToArray());
        control.Text = selected;
    }

    public void SelectPreset()
    {
        if (_initializing || _settings is null) return;
        var preset = _settings.Commands.FirstOrDefault(item => item.SourceModel.Equals(source.Text, StringComparison.OrdinalIgnoreCase));
        if (preset is null) return;
        SetIfPresent(executable, preset.OvmsExecutable);
        sourceFormat.Text = FirstText(preset.SourceFormat, preset.GgufFilename.Length > 0 ? OvmsSourceFormatGguf : OvmsSourceFormatExport);
        SetIfPresent(task, preset.Task);
        SetIfPresent(pipeline, preset.PipelineType);
        SetIfPresent(weights, preset.WeightFormat);
        gguf.Text = preset.GgufFilename;
        SetIfPresent(repository, preset.ModelRepositoryPath);
        SetIfPresent(log, preset.LogPath);
        overwrite.Checked = preset.OverwriteModels;
        debug.Checked = preset.LogLevel.Equals("DEBUG", StringComparison.OrdinalIgnoreCase);
        status.Text = $"Preset: {source.Text}";
    }

    // Fills the pull settings from the Downloads tab's recommendation; the user reviews them and presses Start Pull.
    public void Prefill(PullAdvice advice)
    {
        if (!advice.CanPull) throw new InvalidOperationException(advice.Message);
        // Set the source first: choosing a source can apply a saved preset, which the advice then overrides.
        source.Text = advice.SourceModel;
        sourceFormat.Text = advice.SourceFormat;
        task.Text = "text_generation";
        if (advice.Route == DownloadRoute.Gguf)
        {
            gguf.Text = advice.GgufFiles.FirstOrDefault() ?? string.Empty;
        }
        else
        {
            gguf.Text = string.Empty;
            pipeline.Text = advice.PipelineType;
        }
        status.Text = "Filled in from Downloads. Check the settings, then press Start Pull.";
        output($"OVMS Pull: {advice.SourceModel} ({advice.SourceFormat}" +
            (advice.Route == DownloadRoute.Gguf ? $", {gguf.Text}" : $", {advice.PipelineType}, weights {weights.Text}") +
            $"). Output folder: {BuildExpectedOvmsModelFolder(ExpandPathText(repository.Text), advice.SourceModel)}");
    }

    private static void SetIfPresent(Control control, string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) control.Text = value;
    }

    internal OvmsPullPlan BuildPlan()
    {
        bool isGguf = sourceFormat.Text == OvmsSourceFormatGguf;
        if (string.IsNullOrWhiteSpace(source.Text) || string.IsNullOrWhiteSpace(task.Text))
            throw new InvalidOperationException("Enter a source model and task.");
        if (isGguf && string.IsNullOrWhiteSpace(gguf.Text)) throw new InvalidOperationException("Enter the GGUF filename.");
        if (!isGguf && (string.IsNullOrWhiteSpace(pipeline.Text) || string.IsNullOrWhiteSpace(weights.Text)))
            throw new InvalidOperationException("Choose a pipeline and weight format.");
        string modelRoot = ExpandPathText(repository.Text);
        return new()
        {
            OvmsExecutable = ExpandPathText(executable.Text),
            PythonRuntimePath = ExpandPathText(python.Text),
            ExportToolsPath = ExpandPathText(exportTools.Text),
            ModelRepositoryPath = modelRoot,
            SourceModel = source.Text.Trim(),
            GgufFilename = gguf.Text.Trim(),
            IsGgufPull = isGguf,
            ExpectedModelFolder = BuildExpectedOvmsModelFolder(modelRoot, source.Text.Trim()),
            LogPath = ExpandPathText(log.Text),
            Arguments = BuildOvmsPullArguments(task.Text.Trim(), pipeline.Text.Trim(), source.Text.Trim(),
                gguf.Text.Trim(), modelRoot, weights.Text.Trim(), ExpandPathText(log.Text), overwrite.Checked, debug.Checked, isGguf),
        };
    }

    public void Preview()
    {
        var plan = BuildPlan();
        output($"{QuoteCommandArgument(plan.OvmsExecutable)} {FormatCommandArguments(plan.Arguments)}");
        status.Text = "Command shown in Output.";
    }

    private async Task<bool> CheckAsync(OvmsPullPlan plan)
    {
        ValidateOvmsPullPlan(plan);
        var result = await CheckOvmsPullVersionStackAsync(plan);
        foreach (string line in result.Lines) output(line);
        foreach (string command in result.RepairCommands) output(command);
        status.Text = result.CanRunPull ? "Stack check passed." : "Stack check failed. See Output.";
        return result.CanRunPull;
    }

    public async Task CheckAsync() => await CheckAsync(BuildPlan());

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var plan = BuildPlan();
        if (!await CheckAsync(plan)) return;
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(plan.ModelRepositoryPath);
        if (!string.IsNullOrWhiteSpace(plan.LogPath)) Directory.CreateDirectory(Path.GetDirectoryName(plan.LogPath)!);
        string script = WriteOvmsPullCommandScript(plan);
        Process.Start(new ProcessStartInfo("cmd.exe", $"/d /k {QuoteCommandPromptArgument(script)}")
        {
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(plan.OvmsExecutable),
        })?.Dispose();
        status.Text = "Pull started in Command Prompt.";
        output($"{status.Text} Output folder: {plan.ExpectedModelFolder}");
    }
}
