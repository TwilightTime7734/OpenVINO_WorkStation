using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WorkStationC_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            _localApiServer = new LocalModelApiServer(CreateLocalModelApiSnapshot, RunLocalApiTextGenerationAsync, LogLocalApiServerMessage);
            InitializeComponent();
            InitializeModelRootSetting();
            ApplyDarkTheme();
            InitializeControlBehavior();
        }

        #region Common
        private const ulong MaxChatResponseTokens = 64;

        private const int DefaultLocalApiContextWindowTokens = 32768;

        private const int MaxAdvertisedLocalApiContextWindowTokens = 1048576;

        private const int NativeDialogCancelled = unchecked((int)0x800704C7);

        private enum ModelFileKind
        {
            Unknown,
            OpenVinoGenAiDirectory,
            OpenVinoCompiledBlob,
            OpenVinoIr,
            OpenVinoWeights,
            OpenVinoCoreReadable,
            Gguf,
            PyTorchSource,
            SupportFile,
        }

        private enum ModelRunPath
        {
            Unknown,
            TextGeneration,
            VisionLanguage,
            TextToImage,
            ImageToImage,
            ImageClassification,
            ObjectDetection,
            SpeechRecognition,
            TextEmbedding,
            OpenVinoCore,
            CompiledBlob,
            Unsupported,
        }

        private enum FolderPickerResult
        {
            Selected,
            Cancelled,
            Failed,
        }

        private sealed record RuntimeModelSelection(string SelectedFolder, string RuntimePath, ModelFileKind Kind);

        private sealed record ModelCapabilityInfo(
            bool AcceptsText,
            bool AcceptsImage,
            bool ImageTestAvailable,
            string DisplayText,
            IReadOnlyList<string> OutputLines);

        private sealed record ModelConfigClassification(
            ModelRunPath RunPath,
            bool AcceptsText,
            bool AcceptsImage,
            bool ReturnsImage,
            bool ImageTestAvailable,
            IReadOnlyList<string> Evidence);

        private string _selectedModelFolder = string.Empty;

        private string _selectedRuntimeModelPath = string.Empty;

        private ModelFileKind _selectedRuntimeModelKind = ModelFileKind.Unknown;

        private ModelRunPath _selectedModelRunPath = ModelRunPath.Unknown;

        private string _loadedRuntimeModelPath = string.Empty;

        private ModelFileKind _loadedRuntimeModelKind = ModelFileKind.Unknown;

        private bool _loadedModelAcceptsText;

        private bool _loadedModelAcceptsImage;

        private bool _loadedModelImageTestAvailable;

        private OpenVinoSharp.GenAI.LLMPipeline? _cachedGenAiPipeline;

        private string _cachedGenAiModelPath = string.Empty;

        private string _cachedGenAiDevice = string.Empty;

        private OpenVinoSharp.GenAI.VLMPipeline? _cachedVlmPipeline;

        private string _cachedVlmModelPath = string.Empty;

        private string _cachedVlmDevice = string.Empty;

        private bool _updatingCompileTargetChoices;

        private readonly SemaphoreSlim _longOperationGate = new(1, 1);

        private CancellationTokenSource? _longOperationCancellation;

        private volatile bool _longOperationCancelRequested;

        private bool _pendingRuntimeModelRefresh;

        private readonly Dictionary<string, OvmsPullCommandBlock> _ovmsPullCommandDefaultsBySource = new(StringComparer.OrdinalIgnoreCase);

        private bool _updatingOvmsPullDefaults;

        private Task<T> RunOnUiThreadAsync<T>(Func<T> action)
        {
            if (IsDisposed)
            {
                return Task.FromException<T>(new ObjectDisposedException(Name));
            }

            if (!InvokeRequired)
            {
                try
                {
                    return Task.FromResult(action());
                }
                catch (Exception ex)
                {
                    return Task.FromException<T>(ex);
                }
            }

            TaskCompletionSource<T> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                BeginInvoke(new MethodInvoker(() =>
                {
                    try
                    {
                        completion.SetResult(action());
                    }
                    catch (Exception ex)
                    {
                        completion.SetException(ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }

            return completion.Task;
        }

        private void ApplyDarkTheme()
        {
            Color windowBack = Color.FromArgb(30, 30, 30);
            Color panelBack = Color.FromArgb(37, 37, 38);
            Color inputBack = Color.FromArgb(45, 45, 48);
            Color borderBack = Color.FromArgb(63, 63, 70);
            Color text = Color.FromArgb(241, 241, 241);
            Color mutedText = Color.FromArgb(170, 170, 170);
            Color accent = Color.FromArgb(0, 122, 204);

            BackColor = windowBack;
            ForeColor = text;

            tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;

            ApplyDarkTheme(this, windowBack, panelBack, inputBack, borderBack, text, mutedText);
            ApplyModelTableTheme(inputBack, text);

            lblTitle.ForeColor = text;
            lblSubtitle.ForeColor = mutedText;
            lblIrPlaceholder.ForeColor = mutedText;
            lblModelsStatus.ForeColor = mutedText;
            lblOvmsPullStatus.ForeColor = mutedText;
            lblDownloadStatus.ForeColor = mutedText;

            pnlIrVisualizer.BackColor = inputBack;
            txtOutput.BackColor = Color.FromArgb(18, 18, 18);
            txtOutput.ForeColor = Color.FromArgb(220, 220, 220);
        }

        private void ApplyModelTableTheme(Color inputBack, Color text)
        {
            ApplyModelListViewTheme(lstHfRepos, inputBack, text);
            ApplyModelListViewTheme(lstHfFiles, inputBack, text);
        }

        private static void ApplyModelListViewTheme(ListView listView, Color inputBack, Color text)
        {
            listView.BackColor = inputBack;
            listView.ForeColor = text;
            listView.GridLines = false;
            listView.OwnerDraw = true;
        }

        private void tabMain_DrawItem(object? sender, DrawItemEventArgs e) =>
            DrawDarkTab(
                e,
                Color.FromArgb(37, 37, 38),
                Color.FromArgb(45, 45, 48),
                Color.FromArgb(241, 241, 241),
                Color.FromArgb(170, 170, 170),
                Color.FromArgb(0, 122, 204));

        private void modelListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e) =>
            DrawModelTableHeader(e, Color.FromArgb(241, 241, 241));

        private void modelListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e) =>
            DrawModelTableCell(e, Color.FromArgb(45, 45, 48), Color.FromArgb(241, 241, 241));

        private static void DrawModelTableHeader(DrawListViewColumnHeaderEventArgs e, Color text)
        {
            using SolidBrush backgroundBrush = new(Color.FromArgb(55, 55, 58));
            using Pen gridPen = new(Color.FromArgb(150, 150, 150));

            e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            e.Graphics.DrawRectangle(gridPen, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));

            TextRenderer.DrawText(
                e.Graphics,
                e.Header?.Text ?? string.Empty,
                e.Font,
                Rectangle.Inflate(e.Bounds, -6, 0),
                text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static void DrawModelTableCell(DrawListViewSubItemEventArgs e, Color inputBack, Color text)
        {
            bool selected = e.Item?.Selected == true;
            Color background = selected ? Color.FromArgb(0, 90, 150) : inputBack;
            Color foreground = selected ? Color.White : text;

            using SolidBrush backgroundBrush = new(background);
            using Pen gridPen = new(Color.FromArgb(135, 135, 135));

            e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            e.Graphics.DrawRectangle(gridPen, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            flags |= e.Header?.TextAlign == HorizontalAlignment.Right ? TextFormatFlags.Right : TextFormatFlags.Left;

            TextRenderer.DrawText(
                e.Graphics,
                e.SubItem?.Text ?? string.Empty,
                e.Item?.ListView?.Font ?? SystemFonts.DefaultFont,
                Rectangle.Inflate(e.Bounds, -6, 0),
                foreground,
                flags);
        }

        private static void ApplyDarkTheme(Control parent, Color windowBack, Color panelBack, Color inputBack, Color borderBack, Color text, Color mutedText)
        {
            foreach (Control control in EnumerateControls(parent))
            {
                control.ForeColor = text;

                switch (control)
                {
                    case Button button:
                        button.BackColor = Color.FromArgb(55, 55, 58);
                        button.ForeColor = text;
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderColor = borderBack;
                        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 70, 74);
                        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(82, 82, 86);
                        break;

                    case TextBox textBox:
                        textBox.BackColor = inputBack;
                        textBox.ForeColor = text;
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        break;

                    case RichTextBox richTextBox:
                        richTextBox.BackColor = Color.FromArgb(18, 18, 18);
                        richTextBox.ForeColor = Color.FromArgb(220, 220, 220);
                        richTextBox.BorderStyle = BorderStyle.None;
                        break;

                    case ComboBox comboBox:
                        comboBox.BackColor = inputBack;
                        comboBox.ForeColor = text;
                        comboBox.FlatStyle = FlatStyle.Flat;
                        break;

                    case ListBox listBox:
                        listBox.BackColor = inputBack;
                        listBox.ForeColor = text;
                        listBox.BorderStyle = BorderStyle.FixedSingle;
                        break;

                    case NumericUpDown numeric:
                        numeric.BackColor = inputBack;
                        numeric.ForeColor = text;
                        break;

                    case CheckBox checkBox:
                        checkBox.BackColor = panelBack;
                        checkBox.ForeColor = text;
                        break;

                    case TabPage tabPage:
                        tabPage.BackColor = panelBack;
                        tabPage.ForeColor = text;
                        break;

                    case GroupBox groupBox:
                        groupBox.BackColor = panelBack;
                        groupBox.ForeColor = text;
                        break;

                    case SplitContainer split:
                        split.BackColor = borderBack;
                        split.Panel1.BackColor = panelBack;
                        split.Panel2.BackColor = panelBack;
                        break;

                    case TableLayoutPanel or FlowLayoutPanel:
                        control.BackColor = panelBack;
                        break;

                    case Panel:
                        control.BackColor = panelBack;
                        break;

                    case Label label when label.BorderStyle == BorderStyle.Fixed3D:
                        label.BackColor = borderBack;
                        label.ForeColor = borderBack;
                        break;

                    case Label label when label.ForeColor == SystemColors.GrayText:
                        label.ForeColor = mutedText;
                        break;

                    case Label:
                        control.BackColor = panelBack;
                        control.ForeColor = text;
                        break;

                    default:
                        control.BackColor = panelBack;
                        control.ForeColor = text;
                        break;
                }
            }
        }

        private void DrawDarkTab(DrawItemEventArgs e, Color selectedBack, Color tabBack, Color text, Color mutedText, Color accent)
        {
            TabPage tabPage = tabMain.TabPages[e.Index];
            bool enabled = tabPage.Enabled;
            bool selected = enabled && e.Index == tabMain.SelectedIndex;
            Rectangle bounds = e.Bounds;

            Color background = enabled
                ? (selected ? selectedBack : tabBack)
                : ControlPaint.Light(tabBack, 0.35f);
            using SolidBrush backgroundBrush = new(background);
            e.Graphics.FillRectangle(backgroundBrush, bounds);

            if (selected)
            {
                using SolidBrush accentBrush = new(accent);
                e.Graphics.FillRectangle(accentBrush, bounds.Left, bounds.Bottom - 3, bounds.Width, 3);
            }

            TextRenderer.DrawText(
                e.Graphics,
                tabPage.Text,
                tabMain.Font,
                bounds,
                !enabled ? SystemColors.GrayText : selected ? text : mutedText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void InitializeControlBehavior()
        {
            InitializeModelCatalogButtons();
            InitializeLocalApiServerControls();
            SetServerModelNameFromLoadedModelIfBlank();
            RefreshSelectedRuntimeModelState(out _);
            InitializeImageDropZone();
            UpdateMultiTargetControlState();
        }

        private void btnClearOutput_Click(object? sender, EventArgs e) => txtOutput.Clear();

        private void tabMain_Selecting(object? sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage is not null && !e.TabPage.Enabled)
            {
                e.Cancel = true;
                AppendOutput($"{e.TabPage.Text}: unavailable until the selected model loads successfully.");
            }
        }

        private void tabMain_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (tabMain.SelectedTab == tabServerTests)
            {
                SetServerModelNameFromLoadedModelIfBlank();
            }

            ApplySelectedModelCapabilitiesToActiveTab(reportStatus: true);
        }

        private async Task TestSelectedGgufModelAsync(string modelPath)
        {
            if (!TryGetPrimaryCompileTarget(out string device))
            {
                return;
            }
            CancellationTokenSource? operation = await TryBeginLongOperationAsync("Loading chat model...", allowCancel: true);
            if (operation is null)
            {
                return;
            }
            CancellationToken cancellationToken = operation.Token;

            btnConvertToIr.Enabled = false;
            string oldStatus = lblModelsStatus.Text;
            lblModelsStatus.Text = "Testing GGUF with OpenVINO GenAI...";

            try
            {
                AppendOutput("GGUF GenAI test");
                AppendOutput($"  Model: {modelPath}");
                AppendOutput($"  Device: {device}");
                AppendGenAiDeviceDetails(device);
                AppendOutput("  Note: GGUF uses OpenVINO GenAI LLMPipeline, not Core IR compile.");
                if (!EnsureOpenVinoGenAiRuntimeAvailable(out string reason))
                {
                    AppendOutput($"  GGUF GenAI unavailable: {reason}");
                    return;
                }

                SetActivityState("Loading chat model...", allowCancel: true);
                Progress<string> progress = new(message => SetActivityState(message, allowCancel: true));
                int streamedCharacters = 0;
                Progress<string> streamProgress = new(chunk =>
                {
                    streamedCharacters += chunk.Length;
                    SetActivityState($"Generating response... {streamedCharacters:N0} characters", allowCancel: true);
                });
                GgufTestResult result = await Task.Run(() => RunGgufGenAiSmokeTestSafely(
                    modelPath,
                    device,
                    progress: progress,
                    streamedText: streamProgress,
                    cancellationToken: cancellationToken), cancellationToken);

                AppendOutput($"  Load time: {result.LoadMilliseconds:N0} ms");
                AppendOutput($"  Generate time: {result.GenerateMilliseconds:N0} ms");
                AppendOutput($"  Output: {TrimForOutput(result.GeneratedText, 700)}");
                AppendOutput("  Status: GGUF loaded and generated text through OpenVINO GenAI.");
            }
            catch (OperationCanceledException)
            {
                AppendOutput("  Chat model test canceled.");
            }
            catch (Exception ex)
            {
                AppendOutput("  GGUF GenAI test failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"    {line}");
                }
                AppendOutput("  Check that openvino_genai_c.dll is bundled and that this GGUF architecture is supported by OpenVINO GenAI.");
            }
            finally
            {
                lblModelsStatus.Text = oldStatus;
                UpdateConvertActionButtons();
                FinishLongOperation();
            }
        }

        private async Task LoadSelectedGgufModelAsync(string modelPath)
        {
            string device = GetPrimaryCompileTarget();

            btnConvertToIr.Enabled = false;
            string oldStatus = lblModelsStatus.Text;
            lblModelsStatus.Text = "Loading GGUF selection into text inference fields...";

            try
            {
                AppendOutput("Load GGUF");
                AppendOutput($"  Model: {modelPath}");
                AppendOutput($"  Device: {device}");
                AppendOutput("  Runtime path: OpenVINO GenAI LLMPipeline.");

                txtInferModel.Text = Path.GetDirectoryName(modelPath) ?? modelPath;
                if (string.IsNullOrWhiteSpace(txtInferText.Text))
                {
                    txtInferText.Text = "The purpose of OpenVINO is";
                }

                await Task.CompletedTask;
                AppendOutput("  Status: GGUF model path loaded for OpenVINO GenAI runtime use.");
            }
            finally
            {
                lblModelsStatus.Text = oldStatus;
                UpdateConvertActionButtons();
            }
        }

        private sealed class GgufTestResult
        {
            public long LoadMilliseconds { get; init; }
            public long GenerateMilliseconds { get; init; }
            public string GeneratedText { get; init; } = string.Empty;
        }

        private sealed record GenAiTextRunAttempt(GgufTestResult? Result, Exception? Error);

        private sealed record GgufMetadataSummary(
            string Architecture,
            string Name,
            string TokenizerModel,
            string SizeLabel);

        private sealed record GenAiDeviceSettings(
            string DisplayDevice,
            string PipelineDevice,
            IReadOnlyDictionary<string, string> Properties)
        {
            public string CacheKey => Properties.Count == 0
                ? PipelineDevice
                : $"{PipelineDevice}|{string.Join(";", Properties.Select(property => $"{property.Key}={property.Value}"))}";
        }

        private GgufTestResult RunGgufGenAiSmokeTest(
            string modelPath,
            string device,
            string prompt = "The purpose of OpenVINO is",
            IProgress<string>? progress = null,
            IProgress<string>? streamedText = null,
            CancellationToken cancellationToken = default,
            ulong maxNewTokens = MaxChatResponseTokens,
            bool enableThinking = false,
            bool preserveThinking = false)
        {
            GenAiDeviceSettings deviceSettings = BuildGenAiDeviceSettings(device);
            cancellationToken.ThrowIfCancellationRequested();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool reusedLoadedModel = _cachedGenAiPipeline is not null
                && string.Equals(_cachedGenAiModelPath, modelPath, StringComparison.OrdinalIgnoreCase)
                && string.Equals(_cachedGenAiDevice, deviceSettings.CacheKey, StringComparison.OrdinalIgnoreCase);
            if (!reusedLoadedModel)
            {
                DisposeCachedGenAiPipeline();
                cancellationToken.ThrowIfCancellationRequested();
                _cachedGenAiPipeline = deviceSettings.Properties.Count == 0
                    ? new OpenVinoSharp.GenAI.LLMPipeline(modelPath, deviceSettings.PipelineDevice)
                    : new OpenVinoSharp.GenAI.LLMPipeline(modelPath, deviceSettings.PipelineDevice, new Dictionary<string, string>(deviceSettings.Properties));
                _cachedGenAiPipeline.StartChat();
                _cachedGenAiModelPath = modelPath;
                _cachedGenAiDevice = deviceSettings.CacheKey;
            }

            stopwatch.Stop();
            long loadMs = reusedLoadedModel ? 0 : stopwatch.ElapsedMilliseconds;
            progress?.Report(reusedLoadedModel
                ? "Model ready. Generating response..."
                : "Model loaded. Generating response...");

            using OpenVinoSharp.GenAI.GenerationConfig config = new();
            config.SetMaxNewTokens(maxNewTokens);
            config.SetDoSample(false);

            stopwatch.Restart();
            System.Text.StringBuilder generatedBuilder = new();
            _cachedGenAiPipeline!.FinishChat();
            _cachedGenAiPipeline.StartChat();
            using OpenVinoSharp.GenAI.ChatHistory chatHistory = BuildGenAiChatHistory(modelPath, prompt, enableThinking, preserveThinking, out OpenVinoSharp.GenAI.JsonContainer? extraContext);
            try
            {
                using OpenVinoSharp.GenAI.DecodedResults decodedResults = _cachedGenAiPipeline.GenerateWithHistory(
                    chatHistory,
                    config,
                    text =>
                    {
                        if (cancellationToken.IsCancellationRequested || _longOperationCancelRequested)
                        {
                            return OpenVinoSharp.GenAI.StreamingStatus.Cancel;
                        }

                        if (!string.IsNullOrEmpty(text))
                        {
                            generatedBuilder.Append(text);
                            streamedText?.Report(text);
                        }

                        return OpenVinoSharp.GenAI.StreamingStatus.Running;
                    });
                stopwatch.Stop();
                cancellationToken.ThrowIfCancellationRequested();
                string generated = NormalizeGeneratedAnswerText(generatedBuilder.Length > 0
                    ? generatedBuilder.ToString()
                    : decodedResults.GetText());

                return new GgufTestResult
                {
                    LoadMilliseconds = loadMs,
                    GenerateMilliseconds = stopwatch.ElapsedMilliseconds,
                    GeneratedText = generated,
                };
            }
            finally
            {
                extraContext?.Dispose();
            }
        }
        private GgufTestResult RunVlmGenAiTextSmokeTest(
            string modelPath,
            string device,
            string prompt = "Describe this image.",
            IProgress<string>? progress = null,
            IProgress<string>? streamedText = null,
            CancellationToken cancellationToken = default,
            ulong maxNewTokens = MaxChatResponseTokens,
            bool enableThinking = false,
            bool preserveThinking = false)
        {
            GenAiDeviceSettings deviceSettings = BuildGenAiDeviceSettings(device);
            cancellationToken.ThrowIfCancellationRequested();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool reusedLoadedModel = _cachedVlmPipeline is not null
                && string.Equals(_cachedVlmModelPath, modelPath, StringComparison.OrdinalIgnoreCase)
                && string.Equals(_cachedVlmDevice, deviceSettings.CacheKey, StringComparison.OrdinalIgnoreCase);
            if (!reusedLoadedModel)
            {
                DisposeCachedGenAiPipeline();
                cancellationToken.ThrowIfCancellationRequested();
                _cachedVlmPipeline = deviceSettings.Properties.Count == 0
                    ? new OpenVinoSharp.GenAI.VLMPipeline(modelPath, deviceSettings.PipelineDevice)
                    : new OpenVinoSharp.GenAI.VLMPipeline(modelPath, deviceSettings.PipelineDevice, new Dictionary<string, string>(deviceSettings.Properties));
                _cachedVlmModelPath = modelPath;
                _cachedVlmDevice = deviceSettings.CacheKey;
            }

            stopwatch.Stop();
            long loadMs = reusedLoadedModel ? 0 : stopwatch.ElapsedMilliseconds;
            progress?.Report(reusedLoadedModel
                ? "Model ready. Generating response..."
                : "Model loaded. Generating response...");

            using OpenVinoSharp.GenAI.GenerationConfig config = new();
            config.SetMaxNewTokens(maxNewTokens);
            config.SetDoSample(false);

            stopwatch.Restart();
            System.Text.StringBuilder generatedBuilder = new();
            // The installed OpenVINO GenAI C runtime does not export VLM generate_with_history.
            using OpenVinoSharp.GenAI.VLMDecodedResults decodedResults = _cachedVlmPipeline!.Generate(
                prompt,
                null,
                config,
                text =>
                {
                    if (cancellationToken.IsCancellationRequested || _longOperationCancelRequested)
                    {
                        return OpenVinoSharp.GenAI.StreamingStatus.Cancel;
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        generatedBuilder.Append(text);
                        streamedText?.Report(text);
                    }

                    return OpenVinoSharp.GenAI.StreamingStatus.Running;
                });
            stopwatch.Stop();
            cancellationToken.ThrowIfCancellationRequested();
            string generated = NormalizeGeneratedAnswerText(generatedBuilder.Length > 0
                ? generatedBuilder.ToString()
                : decodedResults.GetText());

            return new GgufTestResult
            {
                LoadMilliseconds = loadMs,
                GenerateMilliseconds = stopwatch.ElapsedMilliseconds,
                GeneratedText = generated,
            };
        }

        private static OpenVinoSharp.GenAI.ChatHistory BuildGenAiChatHistory(
            string modelPath,
            string prompt,
            bool enableThinking,
            bool preserveThinking,
            out OpenVinoSharp.GenAI.JsonContainer? extraContext)
        {
            OpenVinoSharp.GenAI.ChatHistory chatHistory = new();
            chatHistory.AddUserMessage(prompt);
            extraContext = null;

            if (modelPath.Contains("qwen", StringComparison.OrdinalIgnoreCase))
            {
                string thinkingOptionsJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    enable_thinking = enableThinking,
                    preserve_thinking = preserveThinking,
                });
                extraContext = OpenVinoSharp.GenAI.JsonContainer.FromJsonString(
                    thinkingOptionsJson);
                chatHistory.SetExtraContext(extraContext);
            }

            return chatHistory;
        }

        private static string NormalizeGeneratedAnswerText(string generated)
        {
            string text = (generated ?? string.Empty).Trim();
            const string thinkEnd = "</think>";
            int thinkEndIndex = text.LastIndexOf(thinkEnd, StringComparison.OrdinalIgnoreCase);
            if (thinkEndIndex >= 0)
            {
                text = text[(thinkEndIndex + thinkEnd.Length)..].Trim();
            }

            return text;
        }
        private GgufTestResult RunGgufGenAiSmokeTestSafely(
            string modelPath,
            string device,
            string prompt = "The purpose of OpenVINO is",
            IProgress<string>? progress = null,
            IProgress<string>? streamedText = null,
            CancellationToken cancellationToken = default,
            ulong maxNewTokens = MaxChatResponseTokens)
        {
            return RunGenAiTextSmokeTestSafely(
                modelPath,
                device,
                prompt,
                progress,
                streamedText,
                cancellationToken,
                useVisionPipeline: false,
                maxNewTokens);
        }

        private GgufTestResult RunGenAiTextSmokeTestSafely(
            string modelPath,
            string device,
            string prompt,
            IProgress<string>? progress,
            IProgress<string>? streamedText,
            CancellationToken cancellationToken,
            bool useVisionPipeline,
            ulong maxNewTokens = MaxChatResponseTokens,
            bool enableThinking = false,
            bool preserveThinking = false)
        {
            try
            {
                return useVisionPipeline
                    ? RunVlmGenAiTextSmokeTest(modelPath, device, prompt, progress, streamedText, cancellationToken, maxNewTokens, enableThinking, preserveThinking)
                    : RunGgufGenAiSmokeTest(modelPath, device, prompt, progress, streamedText, cancellationToken, maxNewTokens, enableThinking, preserveThinking);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("OpenVINO GenAI could not run this chat model in the current runtime.", ex);
            }
        }

        private void DisposeCachedGenAiPipeline()
        {
            _cachedGenAiPipeline?.Dispose();
            _cachedGenAiPipeline = null;
            _cachedGenAiModelPath = string.Empty;
            _cachedGenAiDevice = string.Empty;
            _cachedVlmPipeline?.Dispose();
            _cachedVlmPipeline = null;
            _cachedVlmModelPath = string.Empty;
            _cachedVlmDevice = string.Empty;
        }

        private bool HasCachedGenAiPipeline()
        {
            return _cachedGenAiPipeline is not null
                || _cachedVlmPipeline is not null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _longOperationCancelRequested = true;
            _longOperationCancellation?.Cancel();
            _modelCatalogCancellation?.Cancel();
            _modelCatalogCancellation?.Dispose();
            _modelFileListCancellation?.Cancel();
            _modelFileListCancellation?.Dispose();
            _localApiServer.Dispose();
            if (_longOperationCancellation is null)
            {
                DisposeCachedGenAiPipeline();
            }
            picImageResult.Image?.Dispose();
            picImageResult.Image = null;
            base.OnFormClosed(e);
        }

        private static string SafePortName(Func<string> readName, string fallback)
        {
            string value = SafePortText(readName, fallback);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string SafePortText(Func<string> readText, string fallback)
        {
            try
            {
                return readText();
            }
            catch
            {
                return fallback;
            }
        }

        private string ConfigureOpenVinoRuntimeSearchPath()
        {
            string projectDirectory = GetProjectDirectory();
            string[] candidates =
            {
                Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native"),
                Path.Combine(AppContext.BaseDirectory, "openvino", "runtime", "bin", "intel64", "Release"),
                Path.Combine(AppContext.BaseDirectory, "openvino"),
                Path.Combine(projectDirectory, "Runtime", "OpenVINO", "win-x64", "native"),
                Environment.GetEnvironmentVariable("OPENVINO_RUNTIME_DIR") ?? string.Empty,
            };

            foreach (string candidate in candidates)
            {
                string? dllDirectory = FindOpenVinoDllDirectory(candidate);
                if (dllDirectory is null)
                {
                    continue;
                }

                Environment.SetEnvironmentVariable("OPENVINO_RUNTIME_DIR", dllDirectory);
                AddNativeDllSearchPath(dllDirectory);
                AppendOutput($"Runtime DLLs: {dllDirectory}");
                AppendOpenVinoRuntimeDependencyStatus(dllDirectory);
                return dllDirectory;
            }

            AppendOutput("Runtime DLLs: not found");
            return string.Empty;
        }

        private bool EnsureOpenVinoGenAiRuntimeAvailable(out string reason)
        {
            string dllDirectory = ConfigureOpenVinoRuntimeSearchPath();
            if (string.IsNullOrWhiteSpace(dllDirectory))
            {
                reason = "OpenVINO runtime DLL folder was not found.";
                return false;
            }

            string[] requiredGenAiDlls =
            {
                "openvino_genai_c.dll",
                "openvino_genai.dll",
                "openvino_tokenizers.dll",
            };

            List<string> missing = requiredGenAiDlls
                .Where(fileName => !File.Exists(Path.Combine(dllDirectory, fileName)))
                .ToList();
            if (missing.Count > 0)
            {
                reason = $"{string.Join(", ", missing)} missing from {dllDirectory}. Core model files can still run, but chat/GGUF models need the GenAI native runtime.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private void AppendOpenVinoRuntimeDependencyStatus(string dllDirectory)
        {
            string[] requiredDlls =
            {
                "openvino_c.dll",
                "openvino.dll",
                "tbb12.dll",
            };

            List<string> missing = requiredDlls
                .Where(fileName => !File.Exists(Path.Combine(dllDirectory, fileName)))
                .ToList();

            if (missing.Count == 0)
            {
                AppendOutput("Runtime dependencies: OpenVINO core DLLs present");
                return;
            }

            AppendOutput($"Runtime dependencies missing: {string.Join(", ", missing)}");
            AppendOutput("  Build again so bundled OpenVINO/TBB DLLs are copied into the native runtime folder.");
        }

        private static string? FindOpenVinoDllDirectory(string? candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return null;
            }

            string[] possibleDirectories =
            {
                candidate,
                Path.Combine(candidate, "bin"),
                Path.Combine(candidate, "runtimes", "win-x64", "native"),
                Path.Combine(candidate, "runtime", "bin", "intel64", "Release"),
            };

            return possibleDirectories.FirstOrDefault(directory => File.Exists(Path.Combine(directory, "openvino_c.dll")));
        }

        private static void AddNativeDllSearchPath(string directory)
        {
            string? path = Environment.GetEnvironmentVariable("PATH");
            string[] entries = (path ?? string.Empty)
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (!entries.Contains(directory, StringComparer.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable("PATH", $"{directory}{Path.PathSeparator}{path}");
            }

            if (OperatingSystem.IsWindows())
            {
                SetDllDirectory(directory);
            }
        }

        private static IEnumerable<string> FormatException(Exception exception)
        {
            for (Exception? current = exception; current is not null; current = current.InnerException)
            {
                if (current is DllNotFoundException)
                {
                    yield return $"{current.GetType().Name}: OpenVINO native library or one of its dependency DLLs could not be loaded.";
                    continue;
                }

                yield return $"{current.GetType().Name}: {current.Message}";
                if (current is OpenVinoSharp.OVException ovException)
                {
                    yield return $"OpenVINO status: {FormatOpenVinoStatus(ovException.Status)}";
                }
            }
        }

        private static string FormatOpenVinoStatus(OpenVinoSharp.ExceptionStatus status)
        {
            string name = status.ToString();
            if (name.Equals("UNKNOW_EXCEPTION", StringComparison.OrdinalIgnoreCase))
            {
                name = "UNKNOWN_EXCEPTION";
            }

            return $"{name} ({(int)status})";
        }

        private void AppendOpenVinoProperty(
            OpenVinoSharp.Core core,
            string device,
            string key,
            string label,
            bool suppressUnavailable = false,
            IProgress<string>? output = null)
        {
            void Write(string message) => WriteOutput(output, message);

            try
            {
                string value = core.GetProperty(device, key);
                if (!string.IsNullOrWhiteSpace(value) || !suppressUnavailable)
                {
                    Write($"  {label}: {FormatPropertyValue(value)}");
                }
            }
            catch (Exception ex)
            {
                if (!suppressUnavailable)
                {
                    Write($"  {label}: unavailable ({ShortError(ex)})");
                }
            }
        }

        private static string FormatPropertyValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(empty)" : value.Replace(Environment.NewLine, " ");
        }

        private static string TrimOpenVinoVersion(string buildNumber)
        {
            if (string.IsNullOrWhiteSpace(buildNumber))
            {
                return "unknown build";
            }

            int releaseIndex = buildNumber.IndexOf("-releases/", StringComparison.OrdinalIgnoreCase);
            return releaseIndex > 0 ? buildNumber[..releaseIndex] : buildNumber;
        }

        private static string FormatBytes(ulong bytes)
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

        private static string FirstText(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        }

        private static string FormatResolution(VideoAdapterSnapshot adapter)
        {
            if (!string.IsNullOrWhiteSpace(adapter.VideoModeDescription))
            {
                return adapter.VideoModeDescription;
            }

            if (adapter.CurrentHorizontalResolution == 0 || adapter.CurrentVerticalResolution == 0)
            {
                return string.Empty;
            }

            string resolution = $"{adapter.CurrentHorizontalResolution} x {adapter.CurrentVerticalResolution}";
            return adapter.CurrentRefreshRate > 0 ? $"{resolution} @ {adapter.CurrentRefreshRate} Hz" : resolution;
        }

        private static string ShortError(Exception ex)
        {
            string message = ex.Message.Replace(Environment.NewLine, " ");
            return message.Length <= 120 ? message : $"{message[..117]}...";
        }

        private void AppendOutput(string message)
        {
            if (IsDisposed || txtOutput.IsDisposed)
            {
                return;
            }

            if (txtOutput.InvokeRequired)
            {
                txtOutput.BeginInvoke(() => AppendOutput(message));
                return;
            }

            txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtOutput.SelectionStart = txtOutput.TextLength;
            txtOutput.ScrollToCaret();
        }

        private void WriteOutput(IProgress<string>? output, string message)
        {
            if (output is null)
            {
                AppendOutput(message);
                return;
            }

            output.Report(message);
        }

        private async Task<CancellationTokenSource?> TryBeginLongOperationAsync(string status, bool allowCancel)
        {
            if (!await _longOperationGate.WaitAsync(0))
            {
                AppendOutput("Working: another task is already running. Cancel it or wait for it to finish.");
                return null;
            }

            _longOperationCancellation?.Dispose();
            _longOperationCancellation = new CancellationTokenSource();
            _longOperationCancelRequested = false;
            SetActivityState(status, allowCancel);
            return _longOperationCancellation;
        }

        private void FinishLongOperation()
        {
            ClearActivityState();

            CancellationTokenSource? cancellation = _longOperationCancellation;
            bool refreshRuntimeModel = _pendingRuntimeModelRefresh;
            _longOperationCancellation = null;
            _longOperationCancelRequested = false;
            _pendingRuntimeModelRefresh = false;
            cancellation?.Dispose();
            _longOperationGate.Release();

            if (refreshRuntimeModel && !IsDisposed)
            {
                RefreshSelectedRuntimeModelState(out _);
            }

            string primaryTarget = GetPrimaryCompileTargetForCurrentThread();
            bool cachedTextDeviceChanged = _cachedGenAiPipeline is not null
                && !string.Equals(_cachedGenAiDevice, BuildGenAiDeviceSettings(primaryTarget).CacheKey, StringComparison.OrdinalIgnoreCase);
            bool cachedVisionDeviceChanged = _cachedVlmPipeline is not null
                && !string.Equals(_cachedVlmDevice, BuildGenAiDeviceSettings(primaryTarget).CacheKey, StringComparison.OrdinalIgnoreCase);
            if (cachedTextDeviceChanged || cachedVisionDeviceChanged)
            {
                DisposeCachedGenAiPipeline();
            }
        }

        private void CancelLongOperation()
        {
            CancellationTokenSource? cancellation = _longOperationCancellation;
            if (cancellation is null)
            {
                AppendOutput("Cancel: nothing is running.");
                return;
            }

            _longOperationCancelRequested = true;
            cancellation.Cancel();
            SetActivityState("Cancel requested. Waiting for a safe stopping point...", allowCancel: false);
            AppendOutput("Cancel requested. The current task will stop at the next safe point.");
        }

        private void SetActivityState(string status)
        {
            SetActivityState(status, allowCancel: _longOperationCancellation is not null);
        }

        private void SetActivityState(string status, bool allowCancel)
        {
            if (IsDisposed || activityLayout.IsDisposed)
            {
                return;
            }

            if (activityLayout.InvokeRequired)
            {
                activityLayout.BeginInvoke(() => SetActivityState(status, allowCancel));
                return;
            }

            lblActivityStatus.Text = status;
            btnCancelActivity.Enabled = allowCancel && _longOperationCancellation is not null;
            btnCancelActivity.Visible = allowCancel;
            activityLayout.Visible = true;
            UseWaitCursor = true;
        }

        private void ClearActivityState()
        {
            if (IsDisposed || activityLayout.IsDisposed)
            {
                return;
            }

            if (activityLayout.InvokeRequired)
            {
                activityLayout.BeginInvoke(ClearActivityState);
                return;
            }

            UseWaitCursor = false;
            activityLayout.Visible = false;
            lblActivityStatus.Text = "Working...";
            btnCancelActivity.Enabled = false;
            btnCancelActivity.Visible = false;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]

        private static extern bool SetDllDirectory(string lpPathName);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]

        private static extern bool GlobalMemoryStatusEx([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] MemoryStatusEx lpBuffer);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]

        private sealed class MemoryStatusEx
        {
            public uint Length = (uint)System.Runtime.InteropServices.Marshal.SizeOf<MemoryStatusEx>();
            public uint MemoryLoad;
            public ulong TotalPhysical;
            public ulong AvailablePhysical;
            public ulong TotalPageFile;
            public ulong AvailablePageFile;
            public ulong TotalVirtual;
            public ulong AvailableVirtual;
            public ulong AvailableExtendedVirtual;
        }

        private void ShowActiveTabHelp()
        {
            TabPage? selectedTab = tabMain.SelectedTab;

            AppendOutput(string.Empty);
            AppendOutput($"Help - {selectedTab?.Text ?? "Current Tab"}");
            AppendOutput("------------------------------------------------------------");

            foreach (string line in GetHelpLinesForTab(selectedTab))
            {
                AppendOutput(line);
            }

            AppendOutput(string.Empty);
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            ShowActiveTabHelp();
        }

        private IEnumerable<string> GetHelpLinesForTab(TabPage? selectedTab)
        {
            if (selectedTab == tabWorkstation)
            {
                return new[]
                {
                    "Use this tab first. It checks whether this computer can run OpenVINO models.",
                    "Main model folder: choose where this app should store all model files.",
                    "The main model folder should contain model folders only. Each model folder keeps its files under main.",
                    "Check Workstation: scans Windows, CPU, memory, graphics adapters, and OpenVINO.",
                    "Runtime target: choose the OpenVINO target that all other tabs should use.",
                    "Model: choose the shared model folder used by model testing, Convert, and Tests.",
                    "Model can use: after a successful load, this summarizes whether text or image input was detected.",
                    "Image, Test Text, Labels, and Model Preset are on Tests and use the loaded Workstation model.",
                    "Compile to Target -> Run loads the selected model and runs the matching local model path.",
                    "Common target names are GPU.0, GPU.1, GPU.2, and MULTI. CPU appears only when no GPU target is available.",
                    "If GPU does not appear here, model tests and conversion GPU runs will not work yet.",
                };
            }

            if (selectedTab == tabDownloads)
            {
                return new[]
                {
                    "Downloads is where you paste a model web address and download its files.",
                    "Downloaded files go under the main model folder selected on the Workstation tab.",
                    "Web Address: paste a Hugging Face model page, tree page, file page, or a direct file address.",
                    "Find: checks whether the address can be read before downloading.",
                    "Download: downloads the files from the pasted address.",
                    "Button Label: enter the name to show on one of the saved website buttons.",
                    "Save To: choose which favorite button to update.",
                    "Save Address: saves the current Button Label and Web Address to the selected button.",
                    "Site 1 through Site 12: fill Web Address with a saved model address.",
                    "Saved address buttons are stored in workbench.cfg beside the project.",
                };
            }

            if (selectedTab == tabConvert)
            {
                return new[]
                {
                    "Convert lists OpenVINO .xml/.bin models under the main model folder.",
                    "Refresh Models scans the model folder for model XML files with matching BIN weights.",
                    "Choose .blob to compile the selected IR for the current Workstation target.",
                    "The .blob output is tied to the selected target and OpenVINO runtime version.",
                    "Blob output is written beside the model's main folder, such as ModelName\\GPU\\model.blob or ModelName\\MULTI\\model.blob.",
                    "",
                    "Use OVMS Pull for GGUF files with a Hugging Face repo and --gguf_filename.",
                    "Future ONNX, PyTorch, TensorFlow, and Paddle workflows belong in OVMS Pull or a source-converter path before this blob step.",
                };
            }

            if (selectedTab == tabOvmsPull)
            {
                return new[]
                {
                    "OVMS Pull runs ovms.exe --pull and writes the converted model into the selected model folder.",
                    "The defaults come from Context.md when that file is present.",
                    "Source model is the Hugging Face model ID, such as Qwen/Qwen3.5-4B.",
                    "Task, pipeline type, and weights are passed directly to OVMS.",
                    "Replace existing files adds --overwrite_models. Use it when rerunning the same pull; turn it off if you want to keep existing model files untouched.",
                    "Debug log uses DEBUG logging. Turn it off for normal INFO logging.",
                    "Python DLL folder is prepended to PATH so OVMS can load its bundled Python runtime before it starts the export subprocess.",
                    "Export tools folder is also prepended to PATH so OVMS finds the same optimum-cli used by the working Command Prompt.",
                    "Preview prints the command without running it. Start Pull opens a Command Prompt and runs the command there.",
                };
            }

            if (selectedTab == tabTests)
            {
                return new[]
                {
                    "Loaded Model Tests always use the successfully loaded Workstation model, detected type, and target.",
                    "Inspect Loaded Model reports Core metadata, imports a compiled blob, or reports GenAI pipeline details without generating again.",
                    "Send Message submits the Test Text to the loaded model. Controls that do not apply to that model are disabled.",
                    "Run Image Test sends the selected image to models with a supported image input. If the model returns an image, it appears in Returned Image and is saved in Results.",
                    "Vision-language models use both the selected image and Test Text, but they answer with text instead of returning a new picture.",
                    "Prompt-driven image editing models usually need a model-specific image-edit pipeline; if plain text cannot be sent safely, the app explains that in Output.",
                    "All results and errors appear in the shared Output panel.",
                };
            }

            if (selectedTab == tabServerTests)
            {
                return new[]
                {
                    "Server Tests only checks server input and output over HTTP.",
                    "Buttons on this tab send requests to the configured server URLs; they do not run the model directly.",
                    "Use Workstation to start or stop this app's local server.",
                    "API Endpoints shows the local server routes this app exposes.",
                    "Server tests without prompt or image checks routes such as Tags and Models.",
                    "Server tests with prompt or image sends sample Completions, Text, and Image POST requests.",
                    "Test Models fills the Server model dropdown from the server's /v1/models response.",
                    "Server model starts with the loaded Workstation model when available; when blank, the app uses the loaded or first known model name.",
                    "API Key is an optional bearer token for servers that require authorization.",
                    "Show URLs prints the current server URLs, model name, prompt, and URL checks without revealing the API key.",
                    "All results and errors appear in the shared Output panel.",
                };
            }

            return new[]
            {
                "No help text is available for this tab yet.",
            };
        }

        private static IEnumerable<Control> EnumerateControls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;

                foreach (Control nested in EnumerateControls(child))
                {
                    yield return nested;
                }
            }
        }

        private void BrowseForModelFolder(TextBox target)
        {
            string initialDirectory;
            try
            {
                initialDirectory = NormalizeModelRoot(txtModelRoot.Text);
            }
            catch (Exception ex)
            {
                AppendOutput($"Model folder is invalid: {ShortError(ex)}");
                return;
            }

            if (!Directory.Exists(initialDirectory))
            {
                AppendOutput($"Model folder does not exist: {initialDirectory}");
                return;
            }

            if (TryPickModelFolder(initialDirectory, out string selectedPath))
            {
                string selectedFolder = CollapseNestedMainFolderSelection(selectedPath, initialDirectory);
                target.Text = selectedFolder;
                AppendOutput($"Selected model folder: {target.Text}");
                if (!string.Equals(selectedPath, selectedFolder, StringComparison.OrdinalIgnoreCase))
                {
                    AppendOutput("  A nested main folder was selected, so the model folder above it was used.");
                }

                if (RefreshSelectedRuntimeModelState(out string reason))
                {
                    AppendOutput($"  Detected runtime path: {_selectedRuntimeModelPath}");
                    AppendOutput($"  Detected type: {DescribeRuntimeModelKind(_selectedRuntimeModelKind)}");
                }
                else
                {
                    AppendOutput($"  No runnable model detected: {reason}");
                }
            }
        }

        private bool TryPickModelFolder(string initialDirectory, out string selectedPath)
        {
            const string title = "Select the model's name folder. The app will inspect its contents and choose the runtime entry point.";

            selectedPath = string.Empty;
            FolderPickerResult result = TryShowNativeFolderPicker(title, initialDirectory, out selectedPath);
            if (result == FolderPickerResult.Selected)
            {
                return true;
            }

            return result == FolderPickerResult.Failed
                && TryShowFolderBrowserFallback(title, initialDirectory, out selectedPath);
        }

        private FolderPickerResult TryShowNativeFolderPicker(string title, string initialDirectory, out string selectedPath)
        {
            selectedPath = string.Empty;
            object? dialogObject = null;
            NativeShellItem? initialFolder = null;
            NativeShellItem? resultItem = null;

            try
            {
                dialogObject = new NativeFileOpenDialog();
                NativeFileOpenDialogInterface dialog = (NativeFileOpenDialogInterface)dialogObject;

                dialog.GetOptions(out NativeFileOpenOptions options);
                dialog.SetOptions(options
                    | NativeFileOpenOptions.PickFolders
                    | NativeFileOpenOptions.ForceFileSystem
                    | NativeFileOpenOptions.PathMustExist
                    | NativeFileOpenOptions.NoChangeDir);
                dialog.SetTitle(title);
                dialog.SetOkButtonLabel("Select Folder");

                if (TryCreateShellItem(initialDirectory, out NativeShellItem? shellFolder)
                    && shellFolder is not null)
                {
                    initialFolder = shellFolder;
                    dialog.SetFolder(shellFolder);
                    dialog.SetDefaultFolder(shellFolder);
                }

                int showResult = dialog.Show(Handle);
                if (showResult == NativeDialogCancelled)
                {
                    return FolderPickerResult.Cancelled;
                }

                if (showResult < 0)
                {
                    Marshal.ThrowExceptionForHR(showResult);
                }

                dialog.GetResult(out resultItem);
                selectedPath = GetShellItemFileSystemPath(resultItem);
                return string.IsNullOrWhiteSpace(selectedPath)
                    ? FolderPickerResult.Failed
                    : FolderPickerResult.Selected;
            }
            catch (Exception ex)
            {
                AppendOutput($"Model folder picker failed: {ShortError(ex)}");
                AppendOutput("  Trying the basic Windows folder picker instead.");
                return FolderPickerResult.Failed;
            }
            finally
            {
                ReleaseComObject(resultItem);
                ReleaseComObject(initialFolder);
                ReleaseComObject(dialogObject);
            }
        }

        private bool TryShowFolderBrowserFallback(string title, string initialDirectory, out string selectedPath)
        {
            selectedPath = string.Empty;
            using FolderBrowserDialog dialog = new()
            {
                Description = title,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false,
                SelectedPath = initialDirectory,
            };

            if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                return false;
            }

            selectedPath = dialog.SelectedPath;
            return true;
        }

        private static string CollapseNestedMainFolderSelection(string selectedPath, string modelRoot)
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

        private static bool AreSameDirectory(string firstPath, string secondPath)
        {
            return NormalizeDirectoryPathForComparison(firstPath)
                .Equals(NormalizeDirectoryPathForComparison(secondPath), StringComparison.OrdinalIgnoreCase);
        }

        private bool RefreshSelectedRuntimeModelState(out string reason)
        {
            if (_longOperationCancellation is not null)
            {
                _pendingRuntimeModelRefresh = true;
                reason = "A task is running. The model selection will update after it finishes.";
                return !string.IsNullOrWhiteSpace(_selectedRuntimeModelPath);
            }

            if (TryResolveRuntimeModel(txtInferModel.Text.Trim(), out RuntimeModelSelection? selection, out reason)
                && selection is not null)
            {
                bool selectionChanged = _selectedRuntimeModelKind != selection.Kind
                    || !string.Equals(_selectedRuntimeModelPath, selection.RuntimePath, StringComparison.OrdinalIgnoreCase);
                if (selectionChanged)
                {
                    DisposeCachedGenAiPipeline();
                    _loadedRuntimeModelPath = string.Empty;
                    _loadedRuntimeModelKind = ModelFileKind.Unknown;
                    ResetLoadedModelCapabilities("Model selected. Load it to see what it can use.");
                }

                _selectedModelFolder = selection.SelectedFolder;
                _selectedRuntimeModelPath = selection.RuntimePath;
                _selectedRuntimeModelKind = selection.Kind;
                _selectedModelRunPath = ClassifySelectedRuntimeModel().RunPath;
                bool selectedModelIsLoaded = _loadedRuntimeModelKind == selection.Kind
                    && string.Equals(
                        _loadedRuntimeModelPath,
                        selection.RuntimePath,
                        StringComparison.OrdinalIgnoreCase);
                UpdateRuntimeTabAvailability(selectedModelIsLoaded);
                if (!selectedModelIsLoaded)
                {
                    UpdateSelectedModelCapabilityPreview();
                }

                ApplySelectedModelCapabilitiesToActiveTab(reportStatus: false);
                return true;
            }

            _selectedModelFolder = string.Empty;
            DisposeCachedGenAiPipeline();
            _selectedRuntimeModelPath = string.Empty;
            _selectedRuntimeModelKind = ModelFileKind.Unknown;
            _selectedModelRunPath = ModelRunPath.Unknown;
            _loadedRuntimeModelPath = string.Empty;
            _loadedRuntimeModelKind = ModelFileKind.Unknown;
            ResetLoadedModelCapabilities("Load a model to see what it can use.");
            UpdateRuntimeTabAvailability(modelLoaded: false);
            ApplySelectedModelCapabilitiesToActiveTab(reportStatus: false);
            return false;
        }

        private void InvalidateLoadedRuntimeModel()
        {
            _loadedRuntimeModelPath = string.Empty;
            _loadedRuntimeModelKind = ModelFileKind.Unknown;
            UpdateSelectedModelCapabilityPreview();
            UpdateRuntimeTabAvailability(modelLoaded: false);
        }

        private void MarkSelectedRuntimeModelLoaded()
        {
            _loadedRuntimeModelPath = _selectedRuntimeModelPath;
            _loadedRuntimeModelKind = _selectedRuntimeModelKind;
            UpdateLoadedModelCapabilities(reportToOutput: true);
            UpdateRuntimeTabAvailability(modelLoaded: true);
            ApplySelectedModelCapabilitiesToActiveTab(reportStatus: false);
            AppendOutput("  Model load validated. Dependent tabs are now enabled.");
        }

        private void ResetLoadedModelCapabilities(string message)
        {
            _loadedModelAcceptsText = false;
            _loadedModelAcceptsImage = false;
            _loadedModelImageTestAvailable = false;
            txtModelCapabilities.Text = message;
        }

        private void UpdateSelectedModelCapabilityPreview()
        {
            _loadedModelAcceptsText = false;
            _loadedModelAcceptsImage = false;
            _loadedModelImageTestAvailable = false;
            ModelConfigClassification classification = ClassifySelectedRuntimeModel();
            _selectedModelRunPath = classification.RunPath;
            txtModelCapabilities.Text = BuildModelCapabilityDisplay(classification, loaded: false);
        }

        private void UpdateLoadedModelCapabilities(bool reportToOutput)
        {
            ModelCapabilityInfo capabilities = DetectSelectedModelCapabilities();
            _loadedModelAcceptsText = capabilities.AcceptsText;
            _loadedModelAcceptsImage = capabilities.AcceptsImage;
            _loadedModelImageTestAvailable = capabilities.ImageTestAvailable;
            txtModelCapabilities.Text = capabilities.DisplayText;

            if (!reportToOutput)
            {
                return;
            }

            AppendOutput("  Model can use:");
            foreach (string line in capabilities.OutputLines)
            {
                AppendOutput($"    {line}");
            }
        }

        private ModelConfigClassification ClassifySelectedRuntimeModel()
        {
            return ClassifyModelRunPath(_selectedModelFolder, _selectedRuntimeModelPath, _selectedRuntimeModelKind);
        }

        private static string BuildModelCapabilityDisplay(ModelConfigClassification classification, bool loaded)
        {
            string state = loaded ? "Loaded" : "Selected";
            string imageText = classification.AcceptsImage
                ? classification.ReturnsImage ? "yes, returns image" : "yes"
                : "not detected";
            return $"{state}. Run path: {DescribeModelRunPath(classification.RunPath)}. Text: {(classification.AcceptsText ? "yes" : "not detected")}. Image: {imageText}.";
        }

        private static ModelConfigClassification ClassifyModelRunPath(string selectedFolder, string runtimePath, ModelFileKind kind)
        {
            List<string> evidence = new();
            string modelDirectory = GetModelConfigDirectory(selectedFolder, runtimePath, kind);
            ModelConfigSnapshot snapshot = ReadModelConfigSnapshot(modelDirectory);
            evidence.AddRange(snapshot.Evidence);

            if (kind == ModelFileKind.Gguf)
            {
                evidence.Add("GGUF file");
                return new ModelConfigClassification(ModelRunPath.TextGeneration, true, false, false, false, evidence);
            }

            if (kind == ModelFileKind.OpenVinoCompiledBlob)
            {
                evidence.Add("compiled .blob file");
                return new ModelConfigClassification(ModelRunPath.CompiledBlob, false, false, false, false, evidence);
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
                return new ModelConfigClassification(diffusionPath, true, diffusionPath == ModelRunPath.ImageToImage, true, false, evidence);
            }

            if (kind == ModelFileKind.OpenVinoGenAiDirectory)
            {
                if (IsVisionLanguageConfig(snapshot, modelDirectory))
                {
                    return new ModelConfigClassification(ModelRunPath.VisionLanguage, true, true, false, true, evidence);
                }

                if (IsTextEmbeddingConfig(snapshot, modelDirectory))
                {
                    return new ModelConfigClassification(ModelRunPath.TextEmbedding, true, false, false, false, evidence);
                }

                return new ModelConfigClassification(ModelRunPath.TextGeneration, true, false, false, false, evidence);
            }

            if (IsVisionLanguageConfig(snapshot, modelDirectory))
            {
                return new ModelConfigClassification(ModelRunPath.VisionLanguage, true, true, false, true, evidence);
            }

            if (IsObjectDetectionConfig(snapshot))
            {
                return new ModelConfigClassification(ModelRunPath.ObjectDetection, false, true, false, true, evidence);
            }

            if (IsImageClassificationConfig(snapshot))
            {
                return new ModelConfigClassification(ModelRunPath.ImageClassification, false, true, false, true, evidence);
            }

            if (IsSpeechConfig(snapshot))
            {
                return new ModelConfigClassification(ModelRunPath.SpeechRecognition, false, false, false, false, evidence);
            }

            if (IsTextEmbeddingConfig(snapshot, modelDirectory))
            {
                return new ModelConfigClassification(ModelRunPath.TextEmbedding, true, false, false, false, evidence);
            }

            if (kind is ModelFileKind.OpenVinoIr or ModelFileKind.OpenVinoCoreReadable)
            {
                evidence.Add($"{Path.GetExtension(runtimePath).TrimStart('.')} model file");
                return new ModelConfigClassification(ModelRunPath.OpenVinoCore, false, false, false, false, evidence);
            }

            return new ModelConfigClassification(ModelRunPath.Unknown, false, false, false, false, evidence);

            static string _classNameLabel(string value) => $"class {value}";
        }

        private sealed record ModelConfigSnapshot(
            string ModelType,
            IReadOnlyList<string> Architectures,
            bool HasVisionConfig,
            bool HasTextConfig,
            bool LanguageModelOnly,
            bool HasDiffusionModelIndex,
            string ModelIndexClass,
            IReadOnlyList<string> ModelIndexKeys,
            IReadOnlyList<string> Evidence);

        private static ModelConfigSnapshot ReadModelConfigSnapshot(string modelDirectory)
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

        private static string GetModelConfigDirectory(string selectedFolder, string runtimePath, ModelFileKind kind)
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

        private static bool IsVisionLanguageConfig(ModelConfigSnapshot snapshot, string modelDirectory)
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

        private static bool IsImageClassificationConfig(ModelConfigSnapshot snapshot)
        {
            return snapshot.Architectures.Any(architecture => architecture.Contains("ForImageClassification", StringComparison.OrdinalIgnoreCase))
                || snapshot.ModelType.Contains("vit", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelType.Contains("resnet", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelType.Contains("swin", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelType.Contains("convnext", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelType.Contains("mobilenet", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelType.Contains("efficientnet", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsObjectDetectionConfig(ModelConfigSnapshot snapshot)
        {
            return snapshot.Architectures.Any(architecture =>
                    architecture.Contains("ForObjectDetection", StringComparison.OrdinalIgnoreCase)
                    || architecture.Contains("Detection", StringComparison.OrdinalIgnoreCase))
                || snapshot.ModelType.Contains("detr", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelType.Contains("yolo", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelType.Contains("ssd", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSpeechConfig(ModelConfigSnapshot snapshot)
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

        private static bool IsTextEmbeddingConfig(ModelConfigSnapshot snapshot, string modelDirectory)
        {
            return File.Exists(Path.Combine(modelDirectory, "openvino_text_embeddings_model.xml"))
                || snapshot.ModelType.Contains("embedding", StringComparison.OrdinalIgnoreCase)
                || snapshot.ModelType.Contains("sentence", StringComparison.OrdinalIgnoreCase)
                || snapshot.Architectures.Any(architecture => architecture.Contains("Embedding", StringComparison.OrdinalIgnoreCase));
        }

        private ModelCapabilityInfo DetectSelectedModelCapabilities()
        {
            try
            {
                ModelConfigClassification classification = ClassifySelectedRuntimeModel();
                _selectedModelRunPath = classification.RunPath;
                return _selectedRuntimeModelKind switch
                {
                    ModelFileKind.OpenVinoGenAiDirectory => DetectGenAiDirectoryCapabilities(_selectedRuntimeModelPath, classification),
                    ModelFileKind.Gguf => new ModelCapabilityInfo(
                        AcceptsText: true,
                        AcceptsImage: false,
                        ImageTestAvailable: false,
                        DisplayText: BuildModelCapabilityDisplay(classification, loaded: true),
                        OutputLines: new[]
                        {
                            $"Run path: {DescribeModelRunPath(classification.RunPath)}",
                            "Text: yes",
                            "Image: not detected for this GGUF chat route",
                        }),
                    ModelFileKind.OpenVinoIr or ModelFileKind.OpenVinoCoreReadable => DetectCoreReadableModelCapabilities(_selectedRuntimeModelPath),
                    ModelFileKind.OpenVinoCompiledBlob => new ModelCapabilityInfo(
                        AcceptsText: false,
                        AcceptsImage: false,
                        ImageTestAvailable: false,
                        DisplayText: "Compiled model loaded. Source input details are not available from the selected folder yet.",
                        OutputLines: new[]
                        {
                            "Compiled model loaded",
                            "Source input details are not available from the selected folder yet",
                        }),
                    _ => new ModelCapabilityInfo(
                        AcceptsText: false,
                        AcceptsImage: false,
                        ImageTestAvailable: false,
                        DisplayText: "Capabilities are unavailable for this model type.",
                        OutputLines: new[] { "Capabilities unavailable" }),
                };
            }
            catch (Exception ex)
            {
                return new ModelCapabilityInfo(
                    AcceptsText: false,
                    AcceptsImage: false,
                    ImageTestAvailable: false,
                    DisplayText: $"Capabilities could not be read: {ShortError(ex)}",
                    OutputLines: new[] { $"Could not read capabilities: {ShortError(ex)}" });
            }
        }

        private static ModelCapabilityInfo DetectGenAiDirectoryCapabilities(
            string modelDirectory,
            ModelConfigClassification classification)
        {
            bool looksVisionCapable = classification.RunPath == ModelRunPath.VisionLanguage;
            string evidence = string.Empty;
            if (looksVisionCapable)
            {
                LooksLikeVisionLanguageGenAiDirectory(modelDirectory, out evidence);
                if (string.IsNullOrWhiteSpace(evidence))
                {
                    evidence = classification.Evidence.FirstOrDefault() ?? "model config";
                }
            }
            bool returnsImage = classification.ReturnsImage;
            string imageText = looksVisionCapable
                ? "Image: yes. Run Image Test sends the picture and Test Text, then prints the text answer."
                : returnsImage
                    ? "Image: returns generated images. This app does not have that model route wired yet."
                : "Image: not detected.";
            string display = BuildModelCapabilityDisplay(classification, loaded: true);
            List<string> lines = new()
            {
                $"Run path: {DescribeModelRunPath(classification.RunPath)}",
                $"Text: {(classification.AcceptsText ? "yes" : "not detected")}",
                imageText,
            };

            if (looksVisionCapable)
            {
                lines.Add("Image test: available; response is text");
                if (!string.IsNullOrWhiteSpace(evidence))
                {
                    lines.Add($"Evidence: {evidence}");
                }
            }

            return new ModelCapabilityInfo(
                AcceptsText: classification.AcceptsText,
                AcceptsImage: classification.AcceptsImage,
                ImageTestAvailable: classification.ImageTestAvailable,
                DisplayText: display,
                OutputLines: lines);
        }

        private static bool LooksLikeVisionLanguageGenAiDirectory(string modelDirectory, out string evidence)
        {
            evidence = string.Empty;
            if (!Directory.Exists(modelDirectory))
            {
                return false;
            }

            string[] visionHints =
            {
                "vision",
                "visual",
                "image",
                "clip",
                "mm_projector",
                "multimodal",
                "multi_modal",
                "resampler",
                "patch",
                "qwen2_vl",
                "qwen2.5-vl",
                "qwen2_5_vl",
                "qwen-vl",
                "qwen_vl",
                "qwenvl",
                "qwen3-vl",
                "qwen3_vl",
                "llava",
                "internvl",
                "paligemma",
                "minicpmv",
            };

            try
            {
                string? matchingPath = Directory.EnumerateFiles(modelDirectory, "*", SearchOption.AllDirectories)
                    .Select(path => Path.GetRelativePath(modelDirectory, path))
                    .FirstOrDefault(relativePath => visionHints.Any(hint =>
                        relativePath.Contains(hint, StringComparison.OrdinalIgnoreCase)));
                if (!string.IsNullOrWhiteSpace(matchingPath))
                {
                    evidence = matchingPath;
                    return true;
                }

                foreach (string jsonPath in Directory.EnumerateFiles(modelDirectory, "*.json", SearchOption.AllDirectories).Take(20))
                {
                    FileInfo file = new(jsonPath);
                    if (file.Length > 512 * 1024)
                    {
                        continue;
                    }

                    string text = File.ReadAllText(jsonPath);
                    string? matchingHint = visionHints.FirstOrDefault(hint =>
                        text.Contains(hint, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrWhiteSpace(matchingHint))
                    {
                        evidence = $"{Path.GetRelativePath(modelDirectory, jsonPath)} contains `{matchingHint}`";
                        return true;
                    }
                }
            }
            catch
            {
                evidence = "folder scan failed";
            }

            return false;
        }

        private ModelCapabilityInfo DetectCoreReadableModelCapabilities(string modelPath)
        {
            ConfigureOpenVinoRuntimeSearchPath();
            string? binPath = FindMatchingBinPath(modelPath);
            using OpenVinoSharp.Core core = new();
            using OpenVinoSharp.Model model = string.IsNullOrWhiteSpace(binPath)
                ? core.ReadModelUnicode(modelPath)
                : core.ReadModelUnicode(modelPath, binPath);

            List<string> inputDescriptions = DescribeModelInputs(model);
            List<string> outputDescriptions = DescribeModelOutputs(model);
            bool acceptsText = false;
            bool acceptsImage = false;
            for (ulong index = 0; index < model.get_inputs_size(); index++)
            {
                using OpenVinoSharp.Input input = model.get_input(index);
                acceptsText |= LooksLikeTextInput(input);
                acceptsImage |= LooksLikeImageInput(input);
            }

            bool returnsImage = false;
            for (ulong index = 0; index < model.get_outputs_size(); index++)
            {
                using OpenVinoSharp.Output output = model.get_output(index);
                returnsImage |= LooksLikeImageOutput(output);
            }

            if (!acceptsText && !acceptsImage)
            {
                acceptsImage = inputDescriptions.Any(description =>
                    description.Contains("image", StringComparison.OrdinalIgnoreCase)
                    || description.Contains("pixel", StringComparison.OrdinalIgnoreCase));
                acceptsText = inputDescriptions.Any(description =>
                    description.Contains("input_ids", StringComparison.OrdinalIgnoreCase)
                    || description.Contains("attention_mask", StringComparison.OrdinalIgnoreCase));
            }

            string display = $"Text: {(acceptsText ? "yes" : "not detected")}. Image: {(acceptsImage ? "yes" : "not detected")}. Returns image: {(returnsImage ? "yes" : "not detected")}. Inputs: {string.Join(" | ", inputDescriptions.Take(3))}";
            List<string> lines = new()
            {
                $"Text: {(acceptsText ? "yes" : "not detected")}",
                $"Image: {(acceptsImage ? "yes" : "not detected")}",
                $"Returns image: {(returnsImage ? "yes" : "not detected")}",
            };
            lines.AddRange(inputDescriptions.Select(input => $"Input: {input}"));
            lines.AddRange(outputDescriptions.Select(output => $"Output: {output}"));

            return new ModelCapabilityInfo(
                AcceptsText: acceptsText,
                AcceptsImage: acceptsImage,
                ImageTestAvailable: acceptsImage,
                DisplayText: display,
                OutputLines: lines);
        }

        private static bool LooksLikeTextInput(OpenVinoSharp.Input input)
        {
            string name = SafePortText(() => input.get_any_name(), string.Empty).ToLowerInvariant();
            if (name.Contains("input_ids")
                || name.Contains("attention_mask")
                || name.Contains("position_ids")
                || name.Contains("token")
                || name.Contains("prompt"))
            {
                return true;
            }

            OpenVinoSharp.ElementType elementType = SafeElementType(input);
            bool integerType = elementType is OpenVinoSharp.ElementType.I8
                or OpenVinoSharp.ElementType.I16
                or OpenVinoSharp.ElementType.I32
                or OpenVinoSharp.ElementType.I64
                or OpenVinoSharp.ElementType.U8
                or OpenVinoSharp.ElementType.U16
                or OpenVinoSharp.ElementType.U32
                or OpenVinoSharp.ElementType.U64;
            int rank = SafePartialShapeRank(input);
            return integerType && rank is 1 or 2;
        }

        private static bool LooksLikeImageInput(OpenVinoSharp.Input input)
        {
            string name = SafePortText(() => input.get_any_name(), string.Empty).ToLowerInvariant();
            if (name.Contains("image")
                || name.Contains("images")
                || name.Contains("pixel")
                || name.Contains("vision")
                || name.Contains("patch"))
            {
                return true;
            }

            try
            {
                using OpenVinoSharp.PartialShape shape = input.get_partial_shape();
                if (shape.rank_is_dynamic() || shape.dims.Length != 4)
                {
                    return false;
                }

                OpenVinoSharp.Dimension channelFirst = shape.dims[1];
                OpenVinoSharp.Dimension channelLast = shape.dims[3];
                return IsImageChannelDimension(channelFirst) || IsImageChannelDimension(channelLast);
            }
            catch
            {
                return false;
            }
        }

        private static bool LooksLikeImageOutput(OpenVinoSharp.Output output)
        {
            string name = SafePortText(() => output.get_any_name(), string.Empty).ToLowerInvariant();
            if (name.Contains("image")
                || name.Contains("images")
                || name.Contains("pixel")
                || name.Contains("rgb")
                || name.Contains("decoded")
                || name.Contains("sample"))
            {
                return true;
            }

            try
            {
                using OpenVinoSharp.PartialShape shape = output.get_partial_shape();
                if (shape.rank_is_dynamic())
                {
                    return false;
                }

                return TryGetImageTensorInfo(
                    shape.dims.Select(dimension => dimension.is_static() ? dimension.get_length() : -1).ToArray(),
                    out _,
                    out _,
                    out _,
                    out _,
                    out _);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsImageChannelDimension(OpenVinoSharp.Dimension dimension)
        {
            if (!dimension.is_static())
            {
                return false;
            }

            long length = dimension.get_length();
            return length is 1 or 3 or 4;
        }

        private static int SafePartialShapeRank(OpenVinoSharp.Input input)
        {
            try
            {
                using OpenVinoSharp.PartialShape shape = input.get_partial_shape();
                return shape.rank_is_dynamic() ? -1 : (int)shape.rank.get_length();
            }
            catch
            {
                return -1;
            }
        }

        private static OpenVinoSharp.ElementType SafeElementType(OpenVinoSharp.Input input)
        {
            try
            {
                return input.get_element_type().get_type();
            }
            catch
            {
                return OpenVinoSharp.ElementType.UNDEFINED;
            }
        }

        private static OpenVinoSharp.ElementType SafeElementType(OpenVinoSharp.Output output)
        {
            try
            {
                return output.get_element_type().get_type();
            }
            catch
            {
                return OpenVinoSharp.ElementType.UNDEFINED;
            }
        }

        private void ApplySelectedModelCapabilitiesToActiveTab(bool reportStatus)
        {
            bool isLoaded = _loadedRuntimeModelKind != ModelFileKind.Unknown
                && !string.IsNullOrWhiteSpace(_loadedRuntimeModelPath);
            bool isTextGenerationModel = _selectedRuntimeModelKind is ModelFileKind.Gguf
                or ModelFileKind.OpenVinoGenAiDirectory;

            bool imageInputsApply = isLoaded && _loadedModelImageTestAvailable;
            lblInferImage.Enabled = imageInputsApply;
            txtInferImage.Enabled = imageInputsApply;
            btnBrowseInferImage.Enabled = imageInputsApply;
            btnRunImageTest.Enabled = imageInputsApply;
            bool coreImageSettingsApply = imageInputsApply && !isTextGenerationModel;
            lblInferLabels.Enabled = coreImageSettingsApply;
            txtInferLabels.Enabled = coreImageSettingsApply;
            btnBrowseInferLabels.Enabled = coreImageSettingsApply;
            lblInferPreset.Enabled = coreImageSettingsApply;
            cboInferPreset.Enabled = coreImageSettingsApply;

            bool textInputsApply = isLoaded && _loadedModelAcceptsText;
            lblInferText.Enabled = textInputsApply;
            txtInferText.Enabled = textInputsApply;

            if (tabMain.SelectedTab == tabTests)
            {
                txtTestModelPath.Text = isLoaded
                    ? $"{DescribeRuntimeModelKind(_loadedRuntimeModelKind)} | {_loadedRuntimeModelPath}"
                    : "No successfully loaded Workstation model";
                btnInspectOpenVinoModel.Enabled = isLoaded;
                btnRunOpenVinoModelTest.Enabled = isLoaded;
                lblImageResult.Enabled = imageInputsApply;
                picImageResult.Enabled = imageInputsApply;
            }

            if (!reportStatus || !isLoaded)
            {
                return;
            }

            if (tabMain.SelectedTab == tabTests && isTextGenerationModel && !_loadedModelAcceptsImage)
            {
                AppendOutput("Tests: text-generation model detected; image, labels, and image preset controls are disabled.");
            }
            else if (tabMain.SelectedTab == tabTests && isTextGenerationModel && _loadedModelAcceptsImage)
            {
                AppendOutput("Tests: this model can use text and images. Run Image Test returns a text answer for vision-language models.");
            }
        }

        private void UpdateRuntimeTabAvailability(bool modelLoaded)
        {
            // Workstation is where a folder is selected; Downloads must remain available to acquire one.
            tabWorkstation.Enabled = true;
            tabDownloads.Enabled = true;
            tabServerTests.Enabled = true;

            tabConvert.Enabled = true;
            tabTests.Enabled = modelLoaded;

            if (!modelLoaded && tabMain.SelectedTab is not null && !tabMain.SelectedTab.Enabled)
            {
                tabMain.SelectedTab = tabWorkstation;
            }

            tabMain.Invalidate();
        }

        private bool TryResolveRuntimeModel(
            string selectedPath,
            out RuntimeModelSelection? selection,
            out string reason)
        {
            selection = null;
            reason = string.Empty;

            try
            {
                string fullPath = Path.GetFullPath(selectedPath);
                if (File.Exists(fullPath))
                {
                    ModelFileKind fileKind = GetModelFileKind(fullPath);
                    if (fileKind is ModelFileKind.OpenVinoIr or ModelFileKind.OpenVinoCoreReadable
                        or ModelFileKind.OpenVinoCompiledBlob or ModelFileKind.Gguf)
                    {
                        selection = new RuntimeModelSelection(Path.GetDirectoryName(fullPath) ?? fullPath, fullPath, fileKind);
                        return true;
                    }

                    reason = "The selected path is not a supported runtime model.";
                    return false;
                }

                if (!Directory.Exists(fullPath))
                {
                    reason = "The selected model folder does not exist.";
                    return false;
                }

                string[] files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
                string[] mainFiles = GetPreferredMainRuntimeFiles(fullPath, files);
                string[] prioritizedFiles = mainFiles.Length > 0 ? mainFiles : files;

                foreach (string directory in prioritizedFiles
                    .Select(Path.GetDirectoryName)
                    .Where(directory => !string.IsNullOrWhiteSpace(directory))
                    .Cast<string>()
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(path => path.Length))
                {
                    if (IsOpenVinoGenAiDirectory(directory))
                    {
                        selection = new RuntimeModelSelection(fullPath, directory, ModelFileKind.OpenVinoGenAiDirectory);
                        return true;
                    }
                }

                string? preferredDeviceBlob = FindDeviceBlobForTarget(fullPath, files, GetPrimaryCompileTarget());
                string? runtimeFile = FindRuntimeFile(prioritizedFiles, ModelFileKind.OpenVinoIr)
                    ?? FindRuntimeFile(prioritizedFiles, ModelFileKind.OpenVinoCoreReadable)
                    ?? preferredDeviceBlob
                    ?? FindRuntimeFile(files, ModelFileKind.OpenVinoCompiledBlob)
                    ?? FindRuntimeFile(files, ModelFileKind.Gguf)
                    ?? FindRuntimeFile(files, ModelFileKind.OpenVinoIr)
                    ?? FindRuntimeFile(files, ModelFileKind.OpenVinoCoreReadable);

                if (runtimeFile is not null)
                {
                    selection = new RuntimeModelSelection(fullPath, runtimeFile, GetModelFileKind(runtimeFile));
                    return true;
                }

                reason = "No OpenVINO GenAI directory, blob file, GGUF, XML, ONNX, PB, Caffe, TFLite, or PDMODEL entry was found beneath it.";
                return false;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private static string[] GetPreferredMainRuntimeFiles(string selectedFolder, IReadOnlyList<string> files)
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

        private static string? FindRuntimeFile(IEnumerable<string> files, ModelFileKind kind)
        {
            return files
                .Where(path => GetModelFileKind(path) == kind)
                .Where(path => kind != ModelFileKind.OpenVinoIr || !IsOpenVinoSupportXml(path))
                .OrderBy(path => Path.GetFileName(path).Equals("openvino_model.xml", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(path => Path.GetFileName(path).Equals("openvino_language_model.xml", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(path => path.Length)
                .FirstOrDefault();
        }

        private static bool IsOpenVinoSupportXml(string path)
        {
            string fileName = Path.GetFileName(path);
            return fileName.Equals("openvino_tokenizer.xml", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals("openvino_detokenizer.xml", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals("openvino_text_embeddings_model.xml", StringComparison.OrdinalIgnoreCase)
                || fileName.StartsWith("openvino_vision_embeddings", StringComparison.OrdinalIgnoreCase);
        }

        private static string? FindDeviceBlobForTarget(string selectedFolder, IEnumerable<string> files, string device)
        {
            if (string.IsNullOrWhiteSpace(device))
            {
                return null;
            }

            string deviceFolder = SanitizePathSegment(device);
            return files
                .Where(path => GetModelFileKind(path) == ModelFileKind.OpenVinoCompiledBlob)
                .Where(path =>
                {
                    string relative = Path.GetRelativePath(selectedFolder, path);
                    string[] parts = relative.Split(
                        new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                        StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length > 1
                        && parts[0].Equals(deviceFolder, StringComparison.OrdinalIgnoreCase);
                })
                .OrderBy(path => path.Length)
                .FirstOrDefault();
        }

        private static bool IsOpenVinoGenAiDirectory(string directory)
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

        private static bool HasOpenVinoVlmLanguageModel(string directory)
        {
            return File.Exists(Path.Combine(directory, "openvino_language_model.xml"))
                && File.Exists(Path.Combine(directory, "openvino_language_model.bin"));
        }

        private static bool ShouldUseVisionGenAiPipeline(ModelFileKind kind, string modelPath)
        {
            if (kind != ModelFileKind.OpenVinoGenAiDirectory)
            {
                return false;
            }

            return ClassifyModelRunPath(modelPath, modelPath, kind).RunPath == ModelRunPath.VisionLanguage;
        }

        private static string DescribeRuntimeModelKind(ModelFileKind kind) => kind switch
        {
            ModelFileKind.OpenVinoGenAiDirectory => "OpenVINO GenAI model directory",
            ModelFileKind.OpenVinoCompiledBlob => "device-optimized OpenVINO compiled model",
            ModelFileKind.Gguf => "GGUF model",
            ModelFileKind.OpenVinoIr => "OpenVINO IR model",
            ModelFileKind.OpenVinoCoreReadable => "OpenVINO Core-readable model",
            _ => "unsupported model",
        };

        private static string DescribeModelRunPath(ModelRunPath path) => path switch
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
            ModelRunPath.CompiledBlob => "compiled OpenVINO model",
            ModelRunPath.Unsupported => "unsupported",
            _ => "unknown",
        };

        private void BrowseForInputImage(TextBox target)
        {
            using OpenFileDialog dialog = new()
            {
                Title = "Select input image",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                SetInferenceImage(dialog.FileName);
            }
        }

        private void BrowseForTextFile(TextBox target)
        {
            using OpenFileDialog dialog = new()
            {
                Title = "Select labels/text file",
                Filter = "Text files|*.txt;*.labels;*.csv|All files (*.*)|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                target.Text = dialog.FileName;
                AppendOutput($"Selected file: {dialog.FileName}");
            }
        }

        private void BrowseForFolder(TextBox target)
        {
            using FolderBrowserDialog dialog = new()
            {
                Description = "Select folder",
                UseDescriptionForTitle = true,
            };

            if (Directory.Exists(target.Text))
            {
                dialog.SelectedPath = target.Text;
            }

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                target.Text = dialog.SelectedPath;
                AppendOutput($"Selected folder: {dialog.SelectedPath}");
            }
        }

        private async Task RunLoadModelAsync()
        {
            string selectedFolder = txtInferModel.Text.Trim();
            string reason = "No model folder was selected.";
            if (string.IsNullOrWhiteSpace(selectedFolder)
                || !RefreshSelectedRuntimeModelState(out reason))
            {
                AppendOutput($"Inference: no runnable model found under: {selectedFolder}");
                AppendOutput($"  {reason}");
                return;
            }

            string modelPath = _selectedRuntimeModelPath;
            AppendOutput($"Inference: resolved {DescribeRuntimeModelKind(_selectedRuntimeModelKind)}");
            AppendOutput($"  Run path: {DescribeModelRunPath(_selectedModelRunPath)}");
            AppendOutput($"  Selected folder: {_selectedModelFolder}");
            AppendOutput($"  Runtime path: {modelPath}");
            if (!TryGetPrimaryCompileTarget(out _))
            {
                return;
            }

            CancellationTokenSource? operation = await TryBeginLongOperationAsync("Preparing model load, compile, and run...", allowCancel: true);
            if (operation is null)
            {
                return;
            }
            CancellationToken cancellationToken = operation.Token;
            btnLoadModel.Enabled = false;
            try
            {
                switch (_selectedRuntimeModelKind)
                {
                    case ModelFileKind.OpenVinoIr:
                    case ModelFileKind.OpenVinoCoreReadable:
                        SetActivityState("Loading and compiling OpenVINO Core model...", allowCancel: true);
                        await RunOpenVinoCoreInferenceAsync(modelPath, cancellationToken);
                        break;
                    case ModelFileKind.OpenVinoCompiledBlob:
                        SetActivityState("Importing and running compiled OpenVINO model...", allowCancel: true);
                        await RunOpenVinoCompiledBlobInferenceAsync(modelPath, cancellationToken: cancellationToken);
                        break;
                    case ModelFileKind.Gguf:
                        await RunGgufGenAiInferenceAsync(modelPath, true, cancellationToken);
                        break;
                    case ModelFileKind.OpenVinoGenAiDirectory:
                        if (_selectedModelRunPath is not (ModelRunPath.TextGeneration or ModelRunPath.VisionLanguage))
                        {
                            AppendOutput($"This selected model looks like {DescribeModelRunPath(_selectedModelRunPath)}.");
                            AppendOutput("That route is not wired to Load Model yet, so it was not sent through the chat model loader.");
                            break;
                        }

                        await RunGgufGenAiInferenceAsync(modelPath, false, cancellationToken);
                        break;
                    default:
                        ReportUnsupportedInferenceFile(modelPath);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                AppendOutput("Run canceled.");
            }
            catch (Exception ex)
            {
                AppendOutput("Inference failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"  {line}");
                }
            }
            finally
            {
                btnLoadModel.Enabled = true;
                FinishLongOperation();
            }
        }

        private async Task RunOpenVinoCoreInferenceAsync(string modelPath, CancellationToken cancellationToken = default)
        {
            string device = GetPrimaryCompileTarget();
            if (string.IsNullOrWhiteSpace(device))
            {
                throw new InvalidOperationException("No runtime target was selected.");
            }

            string sourceFolder = string.IsNullOrWhiteSpace(_selectedModelFolder)
                ? Path.GetDirectoryName(modelPath) ?? modelPath
                : _selectedModelFolder;
            string blobPath = GetOptimizedBlobPath(sourceFolder, modelPath, device);
            if (File.Exists(blobPath))
            {
                AppendOutput($"Inference: using blob file: {blobPath}");
                await RunOpenVinoCompiledBlobInferenceAsync(blobPath, markLoaded: false, cancellationToken: cancellationToken);
                MarkSelectedRuntimeModelLoaded();
                return;
            }

            await Task.Run(() => TestOpenVinoModelCore(
                modelPath,
                device,
                runInference: true,
                cancellationToken: cancellationToken), cancellationToken);
            MarkSelectedRuntimeModelLoaded();
        }

        private async Task RunOpenVinoCompiledBlobInferenceAsync(
            string blobPath,
            bool markLoaded = true,
            CancellationToken cancellationToken = default)
        {
            string device = GetPrimaryCompileTarget();
            if (string.IsNullOrWhiteSpace(device))
            {
                throw new InvalidOperationException("No runtime target was selected.");
            }

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                AppendOutput("Inference: importing device-optimized OpenVINO model");
                AppendOutput($"  Blob: {blobPath}");
                AppendOutput($"  Device: {device}");
                ConfigureOpenVinoRuntimeSearchPath();

                using OpenVinoSharp.Core core = new();
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using OpenVinoSharp.CompiledModel compiledModel = core.ImportModel(blobPath, device);
                stopwatch.Stop();
                cancellationToken.ThrowIfCancellationRequested();
                AppendOutput($"  Import time: {stopwatch.ElapsedMilliseconds:N0} ms");

                using OpenVinoSharp.InferRequest request = compiledModel.CreateInferRequest();
                List<OpenVinoSharp.Tensor> inputTensors = new();
                try
                {
                    if (!TryAttachDefaultInputTensors(compiledModel, request, inputTensors, out string reason))
                    {
                        AppendOutput($"  Inference skipped: {reason}");
                        return;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    stopwatch.Restart();
                    request.Infer();
                    stopwatch.Stop();
                    cancellationToken.ThrowIfCancellationRequested();
                    AppendOutput($"  Inference time: {stopwatch.ElapsedMilliseconds:N0} ms");
                    AppendProfilingInfo(request);
                }
                finally
                {
                    foreach (OpenVinoSharp.Tensor tensor in inputTensors)
                    {
                        tensor.Dispose();
                    }
                }
            }, cancellationToken);
            if (markLoaded)
            {
                MarkSelectedRuntimeModelLoaded();
            }
        }

        private static string BuildOptimizedBlobModelName(string sourceModelFolder, string sourcePath)
        {
            string folder = GetOptimizedBlobModelFolder(sourceModelFolder, sourcePath);
            string name = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            return SanitizePathSegment(string.IsNullOrWhiteSpace(name) ? "model" : name);
        }

        private static string GetOptimizedBlobModelFolder(string sourceModelFolder, string sourcePath)
        {
            string modelRoot = Path.GetFullPath(GetModelRoot());
            string selectedFolder = string.IsNullOrWhiteSpace(sourceModelFolder)
                ? Path.GetDirectoryName(sourcePath) ?? sourcePath
                : sourceModelFolder;
            string fullSelectedFolder = Path.GetFullPath(selectedFolder);
            string fullSourcePath = Path.GetFullPath(sourcePath);

            string? mainFolder = FindContainingMainFolder(fullSourcePath, modelRoot);
            if (mainFolder is not null && Directory.GetParent(mainFolder) is DirectoryInfo modelFolder)
            {
                return modelFolder.FullName;
            }

            if (Directory.Exists(fullSelectedFolder)
                && Path.GetFileName(fullSelectedFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                    .Equals("main", StringComparison.OrdinalIgnoreCase)
                && Directory.GetParent(fullSelectedFolder) is DirectoryInfo selectedParent)
            {
                return selectedParent.FullName;
            }

            return fullSelectedFolder;
        }

        private static string? FindContainingMainFolder(string path, string modelRoot)
        {
            DirectoryInfo? cursor = File.Exists(path)
                ? Directory.GetParent(path)
                : new DirectoryInfo(path);

            while (cursor is not null && IsPathUnder(cursor.FullName, modelRoot))
            {
                if (cursor.Name.Equals("main", StringComparison.OrdinalIgnoreCase))
                {
                    return cursor.FullName;
                }

                cursor = cursor.Parent;
            }

            return null;
        }

        private static string GetOptimizedBlobDeviceFolder(string sourceModelFolder, string sourcePath, string device)
        {
            return Path.Combine(GetOptimizedBlobModelFolder(sourceModelFolder, sourcePath), SanitizePathSegment(device));
        }

        private static string GetOptimizedBlobPath(string sourceModelFolder, string sourcePath, string device)
        {
            return Path.Combine(GetOptimizedBlobDeviceFolder(sourceModelFolder, sourcePath, device), "model.blob");
        }

        private async Task RunGgufGenAiInferenceAsync(
            string modelPath,
            bool isGguf = true,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetPrimaryCompileTarget(out string device))
            {
                return;
            }

            string prompt = string.IsNullOrWhiteSpace(txtInferText.Text)
                ? "The purpose of OpenVINO is"
                : txtInferText.Text.Trim();

            bool useVisionPipeline = !isGguf && ShouldUseVisionGenAiPipeline(_selectedRuntimeModelKind, modelPath);
            AppendOutput(isGguf
                ? "Chat model: loading GGUF and generating a short response"
                : useVisionPipeline
                    ? "Image and text model: loading and generating a short response"
                    : "Chat model: loading and generating a short response");
            AppendOutput($"  Model: {modelPath}");
            AppendOutput($"  Device: {device}");
            AppendGenAiDeviceDetails(device);
            AppendOutput($"  GenAI runtime: {(useVisionPipeline ? "VLMPipeline" : "LLMPipeline")}");
            if (isGguf)
            {
                AppendGgufModelDetails(modelPath);
            }

            AppendOutput($"  Response limit: {MaxChatResponseTokens} tokens. If the answer ends mid-sentence, the model reached this normal limit; the test did not fail.");

            if (!EnsureOpenVinoGenAiRuntimeAvailable(out string reason))
            {
                AppendOutput($"  OpenVINO GenAI unavailable: {reason}");
                return;
            }

            SetActivityState(isGguf ? "Loading GGUF model into memory..." : "Loading chat model...", allowCancel: true);
            Progress<string> progress = new(message => SetActivityState(message, allowCancel: true));
            int streamedCharacters = 0;
            Progress<string> streamProgress = new(chunk =>
            {
                streamedCharacters += chunk.Length;
                SetActivityState($"Generating response... {streamedCharacters:N0} characters", allowCancel: true);
            });
            GenAiTextRunAttempt attempt = await Task.Run(() => RunGenAiTextSmokeTestAttempt(
                modelPath,
                device,
                prompt,
                progress,
                streamProgress,
                cancellationToken,
                useVisionPipeline), cancellationToken);
            if (attempt.Error is not null)
            {
                AppendGenAiLoadFailure(attempt.Error, isGguf);
                return;
            }

            GgufTestResult result = attempt.Result
                ?? throw new InvalidOperationException("OpenVINO GenAI did not return a result.");
            AppendOutput($"  Load time: {result.LoadMilliseconds:N0} ms");
            AppendOutput($"  Generate time: {result.GenerateMilliseconds:N0} ms");
            AppendOutput($"  Output: {TrimForOutput(result.GeneratedText, 1000)}");
            MarkSelectedRuntimeModelLoaded();
        }

        private GenAiTextRunAttempt RunGenAiTextSmokeTestAttempt(
            string modelPath,
            string device,
            string prompt,
            IProgress<string>? progress,
            IProgress<string>? streamedText,
            CancellationToken cancellationToken,
            bool useVisionPipeline)
        {
            try
            {
                GgufTestResult result = useVisionPipeline
                    ? RunVlmGenAiTextSmokeTest(modelPath, device, prompt, progress, streamedText, cancellationToken)
                    : RunGgufGenAiSmokeTest(modelPath, device, prompt, progress, streamedText, cancellationToken);
                return new GenAiTextRunAttempt(result, null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new GenAiTextRunAttempt(null, ex);
            }
        }

        private void AppendGenAiLoadFailure(Exception exception, bool isGguf)
        {
            AppendOutput("Inference failed:");
            AppendOutput("  OpenVINO GenAI could not run this chat model in the current runtime.");
            foreach (string line in FormatException(exception))
            {
                AppendOutput($"  {line}");
            }

            if (isGguf)
            {
                AppendOutput("  Try a different GGUF file for this model, or use an OpenVINO model folder exported from the original Hugging Face model.");
            }
        }

        private void AppendGgufModelDetails(string modelPath)
        {
            if (!TryReadGgufMetadataSummary(modelPath, out GgufMetadataSummary metadata, out string error))
            {
                AppendOutput($"  GGUF details: unavailable ({error})");
                return;
            }

            if (!string.IsNullOrWhiteSpace(metadata.Name))
            {
                AppendOutput($"  GGUF name: {metadata.Name}");
            }

            if (!string.IsNullOrWhiteSpace(metadata.Architecture))
            {
                AppendOutput($"  GGUF model family: {metadata.Architecture}");
            }

            if (!string.IsNullOrWhiteSpace(metadata.TokenizerModel))
            {
                AppendOutput($"  GGUF tokenizer: {metadata.TokenizerModel}");
            }

            if (!string.IsNullOrWhiteSpace(metadata.SizeLabel))
            {
                AppendOutput($"  GGUF size label: {metadata.SizeLabel}");
            }

            if (metadata.Architecture.Equals("gemma4", StringComparison.OrdinalIgnoreCase)
                || metadata.Architecture.Equals("gemma4_unified", StringComparison.OrdinalIgnoreCase))
            {
                AppendOutput("  Note: Gemma 4 may need an OpenVINO exported model folder instead of direct GGUF loading in this runtime.");
            }
        }

        private static bool TryReadGgufMetadataSummary(string modelPath, out GgufMetadataSummary metadata, out string error)
        {
            metadata = new GgufMetadataSummary(string.Empty, string.Empty, string.Empty, string.Empty);
            error = string.Empty;

            try
            {
                Dictionary<string, string> values = ReadGgufMetadataValues(
                    modelPath,
                    new[]
                    {
                        "general.architecture",
                        "general.name",
                        "general.size_label",
                        "tokenizer.ggml.model",
                    });

                metadata = new GgufMetadataSummary(
                    values.GetValueOrDefault("general.architecture", string.Empty),
                    values.GetValueOrDefault("general.name", string.Empty),
                    values.GetValueOrDefault("tokenizer.ggml.model", string.Empty),
                    values.GetValueOrDefault("general.size_label", string.Empty));
                return true;
            }
            catch (Exception ex)
            {
                error = ShortError(ex);
                return false;
            }
        }

        private static Dictionary<string, string> ReadGgufMetadataValues(string modelPath, IReadOnlyCollection<string> wantedKeys)
        {
            Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> wanted = wantedKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            using FileStream stream = File.Open(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: false);

            string magic = new(reader.ReadChars(4));
            if (!magic.Equals("GGUF", StringComparison.Ordinal))
            {
                throw new InvalidDataException("not a GGUF file.");
            }

            _ = reader.ReadUInt32();
            _ = reader.ReadUInt64();
            ulong metadataCount = reader.ReadUInt64();
            for (ulong index = 0; index < metadataCount; index++)
            {
                string key = ReadGgufString(reader);
                GgufValueType valueType = (GgufValueType)reader.ReadUInt32();
                if (wanted.Contains(key))
                {
                    values[key] = ReadGgufValueAsText(reader, valueType);
                    if (wanted.All(values.ContainsKey))
                    {
                        break;
                    }
                }
                else
                {
                    SkipGgufValue(reader, valueType);
                }
            }

            return values;
        }

        private enum GgufValueType : uint
        {
            UInt8 = 0,
            Int8 = 1,
            UInt16 = 2,
            Int16 = 3,
            UInt32 = 4,
            Int32 = 5,
            Float32 = 6,
            Bool = 7,
            String = 8,
            Array = 9,
            UInt64 = 10,
            Int64 = 11,
            Float64 = 12,
        }

        private static string ReadGgufValueAsText(BinaryReader reader, GgufValueType valueType)
        {
            return valueType switch
            {
                GgufValueType.UInt8 => reader.ReadByte().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.Int8 => reader.ReadSByte().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.UInt16 => reader.ReadUInt16().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.Int16 => reader.ReadInt16().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.UInt32 => reader.ReadUInt32().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.Int32 => reader.ReadInt32().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.Float32 => reader.ReadSingle().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.Bool => reader.ReadBoolean() ? "true" : "false",
                GgufValueType.String => ReadGgufString(reader),
                GgufValueType.UInt64 => reader.ReadUInt64().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.Int64 => reader.ReadInt64().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.Float64 => reader.ReadDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                GgufValueType.Array => ReadGgufArrayPreview(reader),
                _ => throw new InvalidDataException($"unsupported GGUF metadata value type {(uint)valueType}."),
            };
        }

        private static string ReadGgufArrayPreview(BinaryReader reader)
        {
            GgufValueType elementType = (GgufValueType)reader.ReadUInt32();
            ulong count = reader.ReadUInt64();
            const int PreviewCount = 4;
            List<string> preview = new();
            for (ulong index = 0; index < count; index++)
            {
                if (index < PreviewCount)
                {
                    preview.Add(ReadGgufValueAsText(reader, elementType));
                }
                else
                {
                    SkipGgufValue(reader, elementType);
                }
            }

            string suffix = count > PreviewCount ? ", ..." : string.Empty;
            return $"[{string.Join(", ", preview)}{suffix}]";
        }

        private static void SkipGgufValue(BinaryReader reader, GgufValueType valueType)
        {
            switch (valueType)
            {
                case GgufValueType.UInt8:
                case GgufValueType.Int8:
                case GgufValueType.Bool:
                    SkipGgufBytes(reader, 1);
                    break;
                case GgufValueType.UInt16:
                case GgufValueType.Int16:
                    SkipGgufBytes(reader, 2);
                    break;
                case GgufValueType.UInt32:
                case GgufValueType.Int32:
                case GgufValueType.Float32:
                    SkipGgufBytes(reader, 4);
                    break;
                case GgufValueType.UInt64:
                case GgufValueType.Int64:
                case GgufValueType.Float64:
                    SkipGgufBytes(reader, 8);
                    break;
                case GgufValueType.String:
                    SkipGgufBytes(reader, reader.ReadUInt64());
                    break;
                case GgufValueType.Array:
                    GgufValueType elementType = (GgufValueType)reader.ReadUInt32();
                    ulong count = reader.ReadUInt64();
                    for (ulong index = 0; index < count; index++)
                    {
                        SkipGgufValue(reader, elementType);
                    }

                    break;
                default:
                    throw new InvalidDataException($"unsupported GGUF metadata value type {(uint)valueType}.");
            }
        }

        private static string ReadGgufString(BinaryReader reader)
        {
            ulong length = reader.ReadUInt64();
            if (length > 1024 * 1024)
            {
                throw new InvalidDataException($"GGUF metadata string is too large: {length:N0} bytes.");
            }

            byte[] bytes = reader.ReadBytes((int)length);
            if (bytes.Length != (int)length)
            {
                throw new EndOfStreamException("GGUF metadata ended in the middle of a string.");
            }

            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private static void SkipGgufBytes(BinaryReader reader, ulong byteCount)
        {
            if (byteCount > long.MaxValue)
            {
                throw new InvalidDataException($"GGUF metadata item is too large: {byteCount:N0} bytes.");
            }

            reader.BaseStream.Seek((long)byteCount, SeekOrigin.Current);
        }

        private void ReportUnsupportedInferenceFile(string modelPath)
        {
            AppendOutput($"Inference: unsupported standalone model file: {modelPath}");
            AppendOutput("  Convert lists OpenVINO .xml/.bin files and creates target-specific blobs from them.");
            AppendOutput("  Use OVMS Pull for GGUF files with --gguf_filename.");
            AppendOutput("  GenAI can run supported .gguf files directly, and it can run exported OpenVINO GenAI model directories.");
            AppendOutput("  Files like .bin, .json, .txt, .model, .safetensors, .pt, and .pth are support/source files, not standalone runtime entries.");
        }

        private void btnCancelActivity_Click(object sender, EventArgs e)
        {
            CancelLongOperation();
        }
        #endregion

        #region Workstation
        private const string PreferredLocalModelApiServerHost = "192.168.0.11";
        private const string LocalModelApiServerBindAddress = "0.0.0.0";
        private const int LocalModelApiServerPort = 11434;
        private const int DownloadShortcutCount = 12;
        private const string WorkbenchConfigFileName = "workbench.cfg";
        private static string LocalModelApiServerBaseUrl => $"http://{GetLocalModelApiServerAdvertisedHost()}:{LocalModelApiServerPort}";

        private sealed class WorkbenchConfig
        {
            public string ModelRoot { get; set; } = string.Empty;
            public List<DownloadShortcutConfig> DownloadShortcuts { get; } = new();
        }

        private sealed class DownloadShortcutConfig
        {
            public string Label { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
        }

        private sealed record LocalApiLoadedTextModelContext(
            bool ModelLoaded,
            string ModelPath,
            ModelFileKind Kind,
            string Device,
            bool AcceptsText,
            string ModelName,
            IReadOnlyList<LocalApiKnownModelContext> KnownModels);
        private sealed record LocalApiKnownModelContext(
            string Name,
            string SelectedFolder,
            string RuntimePath,
            ModelFileKind Kind,
            bool Loaded);
        private sealed record LocalApiRootModelFolder(
            string Name,
            string Folder,
            string RuntimePath,
            ModelFileKind Kind,
            bool Loaded);

        private readonly LocalModelApiServer _localApiServer;

        private async void Form1_Shown(object? sender, EventArgs e)
        {
            // Let the form and startup message paint before hardware and OpenVINO discovery begins.
            await Task.Delay(150);
            await RunWorkstationCheckAsync("Startup: automatically checking workstation hardware and OpenVINO devices...");
        }

        private void InitializeLocalApiServerControls()
        {
            string baseUrl = LocalModelApiServerBaseUrl;
            txtLocalServerUrl.Text = baseUrl;
            SetLocalApiEndpointDefaults(baseUrl);
            UpdateLocalApiServerControls();
        }

        private void StartLocalApiServer()
        {
            if (_localApiServer.IsRunning)
            {
                AppendOutput($"Local model server is already running at {_localApiServer.BaseUrl}.");
                UpdateLocalApiServerControls();
                return;
            }

            try
            {
                _localApiServer.Start(
                    System.Net.IPAddress.Parse(LocalModelApiServerBindAddress),
                    LocalModelApiServerPort,
                    GetLocalModelApiServerAdvertisedHost());
                UpdateLocalApiServerControls();
                string baseUrl = _localApiServer.BaseUrl;
                AppendOutput("Local model server started.");
                AppendOutput($"  Listening: all network adapters on port {LocalModelApiServerPort}");
                AppendOutput($"  Health: {baseUrl}/health");
                AppendOutput($"  Tags: {baseUrl}/api/tags");
                AppendOutput($"  Search: {baseUrl}/api/search");
                AppendOutput($"  Models: {baseUrl}/v1/models");
                AppendOutput($"  Chat: {baseUrl}/v1/chat/completions");
                AppendOutput($"  Text: {baseUrl}/v1/completions");
                if (_loadedRuntimeModelKind == ModelFileKind.Unknown || string.IsNullOrWhiteSpace(_loadedRuntimeModelPath))
                {
                    AppendOutput("  No Workstation model is loaded yet. Tags can still list known model folders.");
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                AppendOutput($"Local model server could not start on port {LocalModelApiServerPort}: {ShortError(ex)}");
                if (ex.SocketErrorCode == System.Net.Sockets.SocketError.AddressAlreadyInUse)
                {
                    AppendOutput("  Another program is already using that port.");
                }
                else if (ex.SocketErrorCode == System.Net.Sockets.SocketError.AddressNotAvailable)
                {
                    AppendOutput("  That address is not assigned to this computer. Use this machine's current IPv4 address or listen on all adapters.");
                }

                UpdateLocalApiServerControls();
            }
            catch (Exception ex)
            {
                AppendOutput($"Local model server could not start: {ShortError(ex)}");
                UpdateLocalApiServerControls();
            }
        }

        private async Task StopLocalApiServerAsync(bool reportStatus)
        {
            if (!_localApiServer.IsRunning)
            {
                if (reportStatus)
                {
                    AppendOutput("Local model server is already stopped.");
                }

                UpdateLocalApiServerControls();
                return;
            }

            await _localApiServer.StopAsync();
            UpdateLocalApiServerControls();
            if (reportStatus)
            {
                AppendOutput("Local model server stopped.");
            }
        }

        private void UpdateLocalApiServerControls()
        {
            bool running = _localApiServer.IsRunning;
            btnStartLocalServer.Enabled = !running;
            btnStopLocalServer.Enabled = running;
            txtLocalServerStatus.Text = running
                ? $"Running at {_localApiServer.BaseUrl}"
                : "Stopped";
            txtLocalServerUrl.Text = running
                ? _localApiServer.BaseUrl
                : LocalModelApiServerBaseUrl;
            SetLocalApiEndpointDefaults(txtLocalServerUrl.Text);
        }

        private void SetLocalApiEndpointDefaults(string baseUrl)
        {
            string trimmed = baseUrl.TrimEnd('/');
            txtTestEndpoint.Text = $"{trimmed}/api/tags";
            txtModelsEndpoint.Text = $"{trimmed}/v1/models";
            txtChatEndpoint.Text = $"{trimmed}/v1/chat/completions";
            txtTextEndpoint.Text = $"{trimmed}/v1/completions";
            txtImageEndpoint.Text = $"{trimmed}/v1/images/generations";
            UpdateServerEndpointList(trimmed);
        }

        private void UpdateServerEndpointList(string baseUrl)
        {
            string trimmed = baseUrl.TrimEnd('/');
            txtServerEndpointList.Text = string.Join(
                Environment.NewLine,
                new[]
                {
                    $"Health: {trimmed}/health",
                    $"Tags: {trimmed}/api/tags",
                    $"Search: {trimmed}/api/search",
                    $"Models: {trimmed}/v1/models",
                    $"Completions: {trimmed}/v1/chat/completions",
                    $"Text: {trimmed}/v1/completions",
                    $"Image: {trimmed}/v1/images/generations",
                });
        }

        private static string GetLocalModelApiServerAdvertisedHost()
        {
            if (IsLocalIpv4Address(PreferredLocalModelApiServerHost))
            {
                return PreferredLocalModelApiServerHost;
            }

            System.Net.IPAddress[] addresses;
            try
            {
                addresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            }
            catch
            {
                return "127.0.0.1";
            }

            System.Net.IPAddress? privateAddress = addresses.FirstOrDefault(IsPrivateIpv4Address);
            if (privateAddress is not null)
            {
                return privateAddress.ToString();
            }

            System.Net.IPAddress? ipv4Address = addresses.FirstOrDefault(address =>
                address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                && !System.Net.IPAddress.IsLoopback(address));
            return ipv4Address?.ToString() ?? "127.0.0.1";
        }

        private static bool IsLocalIpv4Address(string addressText)
        {
            if (!System.Net.IPAddress.TryParse(addressText, out System.Net.IPAddress? expected)
                || expected.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return false;
            }

            try
            {
                return System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())
                    .Any(address => address.Equals(expected));
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPrivateIpv4Address(System.Net.IPAddress address)
        {
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork
                || System.Net.IPAddress.IsLoopback(address))
            {
                return false;
            }

            byte[] bytes = address.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168);
        }

        private void btnStartLocalServer_Click(object? sender, EventArgs e)
        {
            StartLocalApiServer();
        }

        private async void btnStopLocalServer_Click(object? sender, EventArgs e)
        {
            await StopLocalApiServerAsync(reportStatus: true);
        }

        private LocalModelApiSnapshot CreateLocalModelApiSnapshot()
        {
            List<LocalModelApiModel> models = new();
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            foreach (LocalApiRootModelFolder entry in EnumerateLocalApiRootModelFolders())
            {
                AddLocalApiModel(
                    models,
                    names,
                    entry.Name,
                    "Main model folder",
                    entry.Folder,
                    entry.RuntimePath,
                    entry.Kind,
                    entry.Loaded);
            }

            bool workstationModelLoaded = _loadedRuntimeModelKind != ModelFileKind.Unknown
                && !string.IsNullOrWhiteSpace(_loadedRuntimeModelPath);
            string loadedModelName = models.FirstOrDefault(model => model.Loaded)?.Name
                ?? (workstationModelLoaded ? BuildCurrentLoadedLocalApiModelName(_selectedModelFolder, _loadedRuntimeModelPath) : string.Empty);
            if (workstationModelLoaded && !string.IsNullOrWhiteSpace(loadedModelName) && !models.Any(model => model.Loaded))
            {
                AddLocalApiModel(
                    models,
                    names,
                    loadedModelName,
                    "Loaded Workstation model",
                    _selectedModelFolder,
                    _loadedRuntimeModelPath,
                    _loadedRuntimeModelKind,
                    loaded: true);
            }

            return new LocalModelApiSnapshot
            {
                ModelLoaded = workstationModelLoaded,
                LoadedModelName = loadedModelName,
                Models = models.OrderByDescending(model => model.Loaded)
                    .ThenBy(model => model.Name, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
            };
        }

        private IReadOnlyList<LocalApiKnownModelContext> CreateLocalApiKnownModelContexts()
        {
            List<LocalApiKnownModelContext> models = new();
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            foreach (LocalApiRootModelFolder entry in EnumerateLocalApiRootModelFolders())
            {
                AddLocalApiKnownModel(
                    models,
                    names,
                    entry.Name,
                    entry.Folder,
                    entry.RuntimePath,
                    entry.Kind,
                    entry.Loaded);
            }

            if (!models.Any(model => model.Loaded)
                && _loadedRuntimeModelKind != ModelFileKind.Unknown
                && !string.IsNullOrWhiteSpace(_loadedRuntimeModelPath))
            {
                AddLocalApiKnownModel(
                    models,
                    names,
                    BuildCurrentLoadedLocalApiModelName(_selectedModelFolder, _loadedRuntimeModelPath),
                    string.IsNullOrWhiteSpace(_selectedModelFolder) ? Path.GetDirectoryName(_loadedRuntimeModelPath) ?? _loadedRuntimeModelPath : _selectedModelFolder,
                    _loadedRuntimeModelPath,
                    _loadedRuntimeModelKind,
                    loaded: true);
            }

            return models.OrderByDescending(model => model.Loaded)
                .ThenBy(model => model.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private IReadOnlyList<LocalApiRootModelFolder> EnumerateLocalApiRootModelFolders()
        {
            string modelRoot = GetModelRoot();
            if (!Directory.Exists(modelRoot))
            {
                return Array.Empty<LocalApiRootModelFolder>();
            }

            List<LocalApiRootModelFolder> models = new();
            foreach (string directory in Directory.EnumerateDirectories(modelRoot, "*", SearchOption.TopDirectoryOnly)
                .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
            {
                DirectoryInfo info = new(directory);
                if (IsReservedModelRootFolder(info.Name)
                    || info.Attributes.HasFlag(FileAttributes.Hidden)
                    || info.Attributes.HasFlag(FileAttributes.System))
                {
                    continue;
                }

                if (!TryResolveRuntimeModel(directory, out RuntimeModelSelection? selection, out _)
                    || selection is null)
                {
                    continue;
                }

                models.Add(new LocalApiRootModelFolder(
                    info.Name,
                    directory,
                    selection.RuntimePath,
                    selection.Kind,
                    IsLocalApiRuntimeModelLoaded(selection.Kind, selection.RuntimePath)));
            }

            return models;
        }

        private static bool IsReservedModelRootFolder(string folderName)
        {
            return folderName.Equals("downloads", StringComparison.OrdinalIgnoreCase)
                || folderName.Equals("converted", StringComparison.OrdinalIgnoreCase)
                || folderName.Equals("local", StringComparison.OrdinalIgnoreCase)
                || folderName.Equals("compiled-cache", StringComparison.OrdinalIgnoreCase)
                || folderName.Equals("results", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsLocalApiRuntimeModelLoaded(ModelFileKind kind, string runtimePath)
        {
            return _loadedRuntimeModelKind == kind
                && string.Equals(_loadedRuntimeModelPath, runtimePath, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildCurrentLoadedLocalApiModelName(string selectedFolder, string runtimePath)
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

        private static void AddLocalApiKnownModel(
            List<LocalApiKnownModelContext> models,
            HashSet<string> names,
            string requestedName,
            string selectedFolder,
            string runtimePath,
            ModelFileKind kind,
            bool loaded)
        {
            string name = EnsureUniqueLocalApiModelName(NormalizeLocalApiModelName(requestedName), names);
            models.Add(new LocalApiKnownModelContext(name, selectedFolder, runtimePath, kind, loaded));
        }

        private static void AddLocalApiModel(
            List<LocalModelApiModel> models,
            HashSet<string> names,
            string requestedName,
            string source,
            string selectedFolder,
            string runtimePath,
            ModelFileKind kind,
            bool loaded)
        {
            string name = EnsureUniqueLocalApiModelName(NormalizeLocalApiModelName(requestedName), names);
            DateTimeOffset modifiedAtUtc = GetLocalApiModifiedTime(runtimePath);
            long sizeBytes = EstimateLocalApiModelSize(runtimePath);
            ModelConfigClassification classification = ClassifyModelRunPath(selectedFolder, runtimePath, kind);
            bool supportsChat = kind is ModelFileKind.OpenVinoGenAiDirectory or ModelFileKind.Gguf
                && classification.AcceptsText;
            bool supportsImageInput = kind == ModelFileKind.OpenVinoGenAiDirectory
                && classification.AcceptsImage;

            models.Add(new LocalModelApiModel
            {
                Name = name,
                Source = string.IsNullOrWhiteSpace(source) ? "model folder" : source,
                RuntimePath = runtimePath,
                Kind = DescribeRuntimeModelKind(kind),
                Format = DetectLocalApiFormat(kind, runtimePath),
                Family = DetectLocalApiFamily(runtimePath),
                QuantizationLevel = DetectLocalApiQuantization(runtimePath),
                ContextWindowTokens = DetectLocalApiContextWindow(selectedFolder, runtimePath, kind),
                SupportsChat = supportsChat,
                SupportsImageInput = supportsImageInput,
                Loaded = loaded,
                ModifiedAtUtc = modifiedAtUtc,
                SizeBytes = sizeBytes,
                Digest = LocalModelApiServer.CreateDigest(runtimePath, sizeBytes, modifiedAtUtc),
            });
        }

        private static string NormalizeLocalApiModelName(string value)
        {
            string normalized = value.Trim()
                .Replace('\\', '/')
                .Replace(" ", "-", StringComparison.Ordinal);
            return string.IsNullOrWhiteSpace(normalized) ? "model" : normalized;
        }

        private static string EnsureUniqueLocalApiModelName(string name, HashSet<string> existing)
        {
            if (existing.Add(name))
            {
                return name;
            }

            for (int suffix = 2; suffix < 1000; suffix++)
            {
                string candidate = $"{name}-{suffix}";
                if (existing.Add(candidate))
                {
                    return candidate;
                }
            }

            string fallback = $"{name}-{Guid.NewGuid():N}";
            existing.Add(fallback);
            return fallback;
        }

        private static DateTimeOffset GetLocalApiModifiedTime(string path)
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

        private static long EstimateLocalApiModelSize(string path)
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

        private static string DetectLocalApiFormat(ModelFileKind kind, string path)
        {
            return kind switch
            {
                ModelFileKind.OpenVinoGenAiDirectory => "openvino_genai",
                ModelFileKind.OpenVinoCompiledBlob => "openvino_blob",
                ModelFileKind.OpenVinoIr => "openvino_ir",
                ModelFileKind.OpenVinoCoreReadable => Path.GetExtension(path).TrimStart('.').ToLowerInvariant(),
                ModelFileKind.Gguf => "gguf",
                _ => "unknown",
            };
        }

        private static string DetectLocalApiFamily(string path)
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

        private static string DetectLocalApiQuantization(string path)
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

        private static int DetectLocalApiContextWindow(string selectedFolder, string runtimePath, ModelFileKind kind)
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

        private static IEnumerable<string> EnumerateLocalApiContextConfigFiles(string selectedFolder, string runtimePath, ModelFileKind kind)
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

        private static IEnumerable<string> EnumerateDistinctExistingDirectories(params string[] paths)
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

        private static int? TryReadContextWindowFromJson(string configPath)
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

        private static int? TryReadContextWindowFromJsonElement(System.Text.Json.JsonElement element)
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

        private static int? TryReadContextTokenCount(System.Text.Json.JsonElement element, string propertyName)
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

        private void LogLocalApiServerMessage(string message)
        {
            AppendOutput(message);
        }

        private async Task<LocalModelApiTextResult> RunLocalApiTextGenerationAsync(
            LocalModelApiTextRequest request,
            CancellationToken serverCancellation)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return LocalModelApiTextResult.Error(400, "Enter text before sending a chat request.", "bad_request");
            }

            LocalApiLoadedTextModelContext context = await ReadLocalApiLoadedTextModelContextAsync();
            LocalModelApiTextResult? modelSelectionError = ResolveLocalApiRequestedModel(request, context, out LocalApiKnownModelContext? targetModel);
            if (modelSelectionError is not null)
            {
                return modelSelectionError;
            }

            if (targetModel is null)
            {
                return LocalModelApiTextResult.Error(409, "Load a Workstation text model before using chat over HTTP.", "model_not_loaded");
            }

            bool targetModelIsLoaded = targetModel.Loaded
                || (context.ModelLoaded
                    && context.Kind == targetModel.Kind
                    && string.Equals(context.ModelPath, targetModel.RuntimePath, StringComparison.OrdinalIgnoreCase));

            if (targetModelIsLoaded && !context.AcceptsText)
            {
                return LocalModelApiTextResult.Error(400, "The loaded Workstation model does not support text chat in this app.", "unsupported_model");
            }

            if (targetModel.Kind is not (ModelFileKind.OpenVinoGenAiDirectory or ModelFileKind.Gguf))
            {
                return LocalModelApiTextResult.Error(400, "HTTP chat currently supports loaded OpenVINO GenAI and GGUF text models only.", "unsupported_model");
            }

            if (string.IsNullOrWhiteSpace(context.Device))
            {
                return LocalModelApiTextResult.Error(409, "Choose a runtime target on Workstation before using chat over HTTP.", "target_missing");
            }

            if (!EnsureOpenVinoGenAiRuntimeAvailable(out string runtimeReason))
            {
                return LocalModelApiTextResult.Error(503, $"OpenVINO GenAI is unavailable: {runtimeReason}", "runtime_unavailable");
            }

            CancellationTokenSource? operation = await TryBeginLongOperationAsync("Server chat request...", allowCancel: true);
            if (operation is null)
            {
                return LocalModelApiTextResult.Error(409, "Another task is already running in this app. Wait for it to finish or cancel it.", "busy");
            }

            using CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(serverCancellation, operation.Token);
            CancellationToken cancellationToken = linkedCancellation.Token;

            try
            {
                if (!targetModelIsLoaded)
                {
                    await SelectLocalApiModelForServerRequestAsync(targetModel);
                }

                AppendOutput("Server chat request");
                AppendOutput($"  Model: {targetModel.Name}");
                if (!targetModelIsLoaded)
                {
                    AppendOutput("  Model was selected by the server request and loaded for this run.");
                }
                AppendOutput($"  Target: {context.Device}");
                AppendGenAiDeviceDetails(context.Device);
                AppendOutput($"  Prompt: {TrimForOutput(request.Prompt, 500)}");
                AppendOutput($"  Response limit: {request.MaxTokens:N0} tokens");

                SetActivityState("Server chat: generating response...", allowCancel: true);
                int streamedCharacters = 0;
                Progress<string> progress = new(message => SetActivityState(message, allowCancel: true));
                Progress<string> streamProgress = new(chunk =>
                {
                    streamedCharacters += chunk.Length;
                    SetActivityState($"Server chat: {streamedCharacters:N0} characters", allowCancel: true);
                });

                GgufTestResult result = await Task.Run(() => RunGenAiTextSmokeTestSafely(
                    targetModel.RuntimePath,
                    context.Device,
                    request.Prompt,
                    progress,
                    streamProgress,
                    cancellationToken,
                    ShouldUseVisionGenAiPipeline(targetModel.Kind, targetModel.RuntimePath),
                    maxNewTokens: (ulong)request.MaxTokens,
                    enableThinking: request.EnableThinking,
                    preserveThinking: request.PreserveThinking), cancellationToken);

                AppendOutput($"  Generate time: {result.GenerateMilliseconds:N0} ms");
                AppendOutput($"  Output: {TrimForOutput(result.GeneratedText, 1000)}");
                if (!targetModelIsLoaded)
                {
                    await MarkLocalApiModelLoadedForServerRequestAsync(targetModel);
                }

                return LocalModelApiTextResult.Ok(
                    result.GeneratedText,
                    targetModel.Name,
                    result.LoadMilliseconds,
                    result.GenerateMilliseconds);
            }
            catch (OperationCanceledException) when (!serverCancellation.IsCancellationRequested)
            {
                AppendOutput("Server chat request canceled.");
                return LocalModelApiTextResult.Error(503, "Server chat request was canceled.", "canceled");
            }
            catch (Exception ex)
            {
                AppendOutput("Server chat request failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"  {line}");
                }

                return LocalModelApiTextResult.Error(500, $"Local model generation failed: {ShortError(ex)}", "server_error");
            }
            finally
            {
                FinishLongOperation();
            }
        }

        private Task<LocalApiLoadedTextModelContext> ReadLocalApiLoadedTextModelContextAsync()
        {
            return RunOnUiThreadAsync(() =>
            {
                LocalModelApiSnapshot snapshot = CreateLocalModelApiSnapshot();
                IReadOnlyList<LocalApiKnownModelContext> knownModels = CreateLocalApiKnownModelContexts();
                string loadedModelName = snapshot.LoadedModelName;
                return new LocalApiLoadedTextModelContext(
                    _loadedRuntimeModelKind != ModelFileKind.Unknown && !string.IsNullOrWhiteSpace(_loadedRuntimeModelPath),
                    _loadedRuntimeModelPath,
                    _loadedRuntimeModelKind,
                    GetPrimaryCompileTarget(),
                    _loadedModelAcceptsText,
                    loadedModelName,
                    knownModels);
            });
        }

        private static LocalModelApiTextResult? ResolveLocalApiRequestedModel(
            LocalModelApiTextRequest request,
            LocalApiLoadedTextModelContext context,
            out LocalApiKnownModelContext? targetModel)
        {
            targetModel = null;
            string requestedModel = request.Model.Trim();
            if (string.IsNullOrWhiteSpace(requestedModel))
            {
                targetModel = context.KnownModels.FirstOrDefault(model => model.Loaded);
                return null;
            }

            targetModel = context.KnownModels.FirstOrDefault(model => LocalApiModelNamesMatch(requestedModel, model.Name));
            if (targetModel is not null)
            {
                return null;
            }

            return LocalModelApiTextResult.Error(
                404,
                $"Model `{requestedModel}` was not found. Use GET /v1/models to list available model names.",
                "model_not_found");
        }

        private Task SelectLocalApiModelForServerRequestAsync(LocalApiKnownModelContext model)
        {
            return RunOnUiThreadAsync(() =>
            {
                txtInferModel.Text = model.SelectedFolder;
                _selectedModelFolder = model.SelectedFolder;
                _selectedRuntimeModelPath = model.RuntimePath;
                _selectedRuntimeModelKind = model.Kind;
                _loadedRuntimeModelPath = string.Empty;
                _loadedRuntimeModelKind = ModelFileKind.Unknown;
                ResetLoadedModelCapabilities("Server request selected a model. Loading...");
                UpdateRuntimeTabAvailability(modelLoaded: false);
                ApplySelectedModelCapabilitiesToActiveTab(reportStatus: false);
                return true;
            });
        }

        private Task MarkLocalApiModelLoadedForServerRequestAsync(LocalApiKnownModelContext model)
        {
            return RunOnUiThreadAsync(() =>
            {
                _selectedModelFolder = model.SelectedFolder;
                _selectedRuntimeModelPath = model.RuntimePath;
                _selectedRuntimeModelKind = model.Kind;
                txtInferModel.Text = model.SelectedFolder;
                MarkSelectedRuntimeModelLoaded();
                return true;
            });
        }

        private static bool LocalApiModelNamesMatch(string requestedModel, string knownModel)
        {
            if (string.IsNullOrWhiteSpace(requestedModel) || string.IsNullOrWhiteSpace(knownModel))
            {
                return false;
            }

            return string.Equals(requestedModel.Trim(), knownModel.Trim(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    NormalizeLocalApiModelName(requestedModel),
                    NormalizeLocalApiModelName(knownModel),
                    StringComparison.OrdinalIgnoreCase);
        }

        private void cboWorkstationTarget_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateMultiTargetControlState();
            HandleWorkstationTargetChanged();
        }

        private void cboWorkstationMultiTarget_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (IsMultiTargetSelection(cboWorkstationTarget.Text))
            {
                HandleWorkstationTargetChanged();
            }
        }

        private void HandleWorkstationTargetChanged()
        {
            if (_updatingCompileTargetChoices)
            {
                return;
            }

            string device = GetPrimaryCompileTarget();
            if (string.IsNullOrWhiteSpace(device))
            {
                return;
            }

            if (_longOperationCancellation is not null
                && HasCachedGenAiPipeline())
            {
                _pendingRuntimeModelRefresh = true;
                AppendOutput("Runtime target change will apply after the current task finishes.");
                return;
            }

            bool cachedTextDeviceChanged = _cachedGenAiPipeline is not null
                && !string.Equals(_cachedGenAiDevice, BuildGenAiDeviceSettings(device).CacheKey, StringComparison.OrdinalIgnoreCase);
            bool cachedVisionDeviceChanged = _cachedVlmPipeline is not null
                && !string.Equals(_cachedVlmDevice, BuildGenAiDeviceSettings(device).CacheKey, StringComparison.OrdinalIgnoreCase);
            if (cachedTextDeviceChanged || cachedVisionDeviceChanged)
            {
                DisposeCachedGenAiPipeline();
            }

            AppendOutput($"Runtime target: {device}");
        }

        private void txtInferModel_TextChanged(object? sender, EventArgs e) => RefreshSelectedRuntimeModelState(out _);

        private void txtModelRoot_Leave(object? sender, EventArgs e) => SaveModelRootFromTextBox();

        private void txtModelRoot_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveModelRootFromTextBox();
                e.SuppressKeyPress = true;
            }
        }

        private void InitializeModelRootSetting()
        {
            string modelRoot = GetConfiguredModelRoot();
            txtModelRoot.Text = modelRoot;
            EnsureModelRootFolders(modelRoot);
            SaveModelRoot(modelRoot);
        }

        private void btnBrowseModelRoot_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new()
            {
                Description = "Select the main model folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = Directory.Exists(txtModelRoot.Text) ? txtModelRoot.Text : GetDefaultModelRoot(),
            };

            if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                return;
            }

            txtModelRoot.Text = Path.GetFullPath(dialog.SelectedPath);
            SaveModelRootFromTextBox();
        }

        private void SaveModelRootFromTextBox()
        {
            try
            {
                string modelRoot = NormalizeModelRoot(txtModelRoot.Text);
                EnsureModelRootFolders(modelRoot);
                SaveModelRoot(modelRoot);
                txtModelRoot.Text = modelRoot;
                AppendOutput($"Main model folder: {modelRoot}");
                AppendOutput("  Created/verified main folder.");
                RefreshConvertModelSelection();
            }
            catch (Exception ex)
            {
                AppendOutput($"Model folder setting failed: {ShortError(ex)}");
            }
        }

        private static string GetModelRoot()
        {
            string modelRoot = GetConfiguredModelRoot();
            EnsureModelRootFolders(modelRoot);
            return modelRoot;
        }

        private static string GetConfiguredModelRoot()
        {
            WorkbenchConfig config = LoadWorkbenchConfig();
            return string.IsNullOrWhiteSpace(config.ModelRoot)
                ? GetDefaultModelRoot()
                : NormalizeModelRoot(config.ModelRoot);
        }

        private static string GetDefaultModelRoot()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string baseFolder = string.IsNullOrWhiteSpace(documents)
                ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                : documents;

            return Path.Combine(baseFolder, "OpenVINO Workbench Models");
        }

        private static string GetWorkbenchConfigPath()
        {
            return Path.Combine(GetProjectDirectory(), WorkbenchConfigFileName);
        }

        private static string NormalizeModelRoot(string modelRoot)
        {
            if (string.IsNullOrWhiteSpace(modelRoot))
            {
                return GetDefaultModelRoot();
            }

            string expanded = Environment.ExpandEnvironmentVariables(modelRoot.Trim());
            return Path.GetFullPath(expanded);
        }

        private static void SaveModelRoot(string modelRoot)
        {
            WorkbenchConfig config = LoadWorkbenchConfig();
            config.ModelRoot = NormalizeModelRoot(modelRoot);
            SaveWorkbenchConfig(config);
        }

        private static IReadOnlyList<DownloadShortcutConfig> GetDownloadShortcutConfigs()
        {
            return LoadWorkbenchConfig().DownloadShortcuts
                .Select(shortcut => new DownloadShortcutConfig
                {
                    Label = shortcut.Label,
                    Url = shortcut.Url,
                })
                .ToArray();
        }

        private static void SaveDownloadShortcutConfig(int shortcutIndex, string label, string url)
        {
            if (shortcutIndex < 0 || shortcutIndex >= DownloadShortcutCount)
            {
                throw new ArgumentOutOfRangeException(nameof(shortcutIndex), "Shortcut index is outside the configured button range.");
            }

            WorkbenchConfig config = LoadWorkbenchConfig();
            EnsureDownloadShortcutSlots(config);
            config.DownloadShortcuts[shortcutIndex].Label = CleanConfigValue(label);
            config.DownloadShortcuts[shortcutIndex].Url = CleanConfigValue(url);
            SaveWorkbenchConfig(config);
        }

        private static WorkbenchConfig LoadWorkbenchConfig()
        {
            WorkbenchConfig config = new()
            {
                ModelRoot = GetDefaultModelRoot(),
            };

            string configPath = GetWorkbenchConfigPath();
            if (File.Exists(configPath))
            {
                foreach (string rawLine in File.ReadAllLines(configPath))
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int separatorIndex = line.IndexOf('=');
                    if (separatorIndex <= 0)
                    {
                        continue;
                    }

                    string key = line[..separatorIndex].Trim();
                    string value = line[(separatorIndex + 1)..].Trim();
                    ApplyWorkbenchConfigValue(config, key, value);
                }
            }

            EnsureDownloadShortcutSlots(config);
            return config;
        }

        private static void ApplyWorkbenchConfigValue(WorkbenchConfig config, string key, string value)
        {
            if (key.Equals("modelRoot", StringComparison.OrdinalIgnoreCase))
            {
                config.ModelRoot = value;
                return;
            }

            for (int index = 0; index < DownloadShortcutCount; index++)
            {
                int buttonNumber = index + 1;
                if (key.Equals($"downloadShortcut{buttonNumber}Label", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureDownloadShortcutSlots(config);
                    config.DownloadShortcuts[index].Label = value;
                    return;
                }

                if (key.Equals($"downloadShortcut{buttonNumber}Url", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureDownloadShortcutSlots(config);
                    config.DownloadShortcuts[index].Url = value;
                    return;
                }
            }
        }

        private static void SaveWorkbenchConfig(WorkbenchConfig config)
        {
            EnsureDownloadShortcutSlots(config);
            string modelRoot = string.IsNullOrWhiteSpace(config.ModelRoot)
                ? GetDefaultModelRoot()
                : NormalizeModelRoot(config.ModelRoot);

            System.Text.StringBuilder builder = new();
            builder.AppendLine("# OpenVINO Workbench configuration");
            builder.AppendLine($"modelRoot={CleanConfigValue(modelRoot)}");
            for (int index = 0; index < DownloadShortcutCount; index++)
            {
                int buttonNumber = index + 1;
                DownloadShortcutConfig shortcut = config.DownloadShortcuts[index];
                builder.AppendLine($"downloadShortcut{buttonNumber}Label={CleanConfigValue(shortcut.Label)}");
                builder.AppendLine($"downloadShortcut{buttonNumber}Url={CleanConfigValue(shortcut.Url)}");
            }

            WriteTextAtomically(GetWorkbenchConfigPath(), NormalizeTextFileLineEndings(builder.ToString()));
        }

        private static void EnsureDownloadShortcutSlots(WorkbenchConfig config)
        {
            while (config.DownloadShortcuts.Count < DownloadShortcutCount)
            {
                config.DownloadShortcuts.Add(new DownloadShortcutConfig());
            }

            while (config.DownloadShortcuts.Count > DownloadShortcutCount)
            {
                config.DownloadShortcuts.RemoveAt(config.DownloadShortcuts.Count - 1);
            }
        }

        private static string CleanConfigValue(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r", " ", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal)
                .Trim();
        }

        private static void EnsureModelRootFolders(string modelRoot)
        {
            string root = NormalizeModelRoot(modelRoot);
            Directory.CreateDirectory(root);
        }

        private static IReadOnlyList<string> GetAvailableOpenVinoDevices(OpenVinoSharp.Core core)
        {
            return core.GetAvailableDevices();
        }

        private IReadOnlyList<string> GetCompileTargetChoices()
        {
            List<string> choices = cboWorkstationTarget.Items
                .Cast<object>()
                .Select(item => item.ToString() ?? string.Empty)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Where(item => !IsMultiTargetSelection(item))
                .ToList();

            choices.AddRange(cboWorkstationMultiTarget.Items
                .Cast<object>()
                .Select(item => item.ToString() ?? string.Empty)
                .Where(item => !string.IsNullOrWhiteSpace(item)));

            return choices
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private IReadOnlyList<string> GetSelectedCompileTargets()
        {
            return TryGetPrimaryCompileTarget(out string target)
                ? new[] { target }
                : Array.Empty<string>();
        }

        private string GetPrimaryCompileTarget()
        {
            string selected = cboWorkstationTarget.Text.Trim();
            if (!IsMultiTargetSelection(selected))
            {
                return selected;
            }

            return cboWorkstationMultiTarget.Text.Trim();
        }

        private bool TryGetPrimaryCompileTarget(out string target)
        {
            target = GetPrimaryCompileTarget();
            if (!string.IsNullOrWhiteSpace(target))
            {
                return true;
            }

            if (IsMultiTargetSelection(cboWorkstationTarget.Text))
            {
                AppendOutput("Choose the devices for Multi first. If the list is empty, click Check Workstation.");
                return false;
            }

            AppendOutput("Choose a runtime target on Workstation first. If the list is empty, click Check Workstation.");
            return false;
        }

        private static GenAiDeviceSettings BuildGenAiDeviceSettings(string target)
        {
            string trimmed = target.Trim();
            if (!IsOpenVinoMultiTarget(trimmed))
            {
                return new GenAiDeviceSettings(trimmed, trimmed, new Dictionary<string, string>());
            }

            string devicePriorities = string.Join(
                ",",
                trimmed["MULTI:".Length..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(device => !string.IsNullOrWhiteSpace(device))
                .Distinct(StringComparer.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(devicePriorities))
            {
                return new GenAiDeviceSettings(trimmed, trimmed, new Dictionary<string, string>());
            }

            return new GenAiDeviceSettings(
                DisplayDevice: trimmed,
                PipelineDevice: $"HETERO:{devicePriorities}",
                Properties: new Dictionary<string, string>
                {
                    ["MODEL_DISTRIBUTION_POLICY"] = "PIPELINE_PARALLEL",
                });
        }

        private void AppendGenAiDeviceDetails(string target)
        {
            GenAiDeviceSettings settings = BuildGenAiDeviceSettings(target);
            if (!string.Equals(settings.DisplayDevice, settings.PipelineDevice, StringComparison.OrdinalIgnoreCase))
            {
                AppendOutput($"  OpenVINO GenAI target: split this model across {settings.PipelineDevice["HETERO:".Length..]}");
            }
        }

        private void UpdateCompileTargetChoices(IEnumerable<string> targets)
        {
            string previousPrimary = cboWorkstationTarget.Text.Trim();
            string previousTarget = GetPrimaryCompileTarget();
            string[] validTargets = targets
                .Where(target => !string.IsNullOrWhiteSpace(target))
                .Select(target => target.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            string[] multiChoices = validTargets
                .Where(IsOpenVinoMultiTarget)
                .ToArray();
            string[] primaryChoices = validTargets
                .Where(target => !IsOpenVinoMultiTarget(target))
                .ToArray();

            if (multiChoices.Length > 0)
            {
                primaryChoices = primaryChoices
                    .Concat(new[] { "MULTI" })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            if (primaryChoices.Length == 0)
            {
                primaryChoices = new[] { "CPU" };
            }

            _updatingCompileTargetChoices = true;
            try
            {
                cboWorkstationTarget.Items.Clear();
                cboWorkstationTarget.Items.AddRange(primaryChoices.Cast<object>().ToArray());
                cboWorkstationMultiTarget.Items.Clear();
                cboWorkstationMultiTarget.Items.AddRange(multiChoices.Cast<object>().ToArray());

                string primaryPrevious = IsOpenVinoMultiTarget(previousTarget) || IsMultiTargetSelection(previousPrimary)
                    ? "MULTI"
                    : previousPrimary;
                string defaultMultiTarget = GetPreferredDefaultMultiGpuTarget(multiChoices);
                string defaultPrimaryTarget = string.IsNullOrWhiteSpace(defaultMultiTarget) ? string.Empty : "MULTI";
                SelectTargetChoice(cboWorkstationTarget, primaryChoices, primaryPrevious, defaultPrimaryTarget);
                SelectTargetChoice(cboWorkstationMultiTarget, multiChoices, previousTarget, defaultMultiTarget);
            }
            finally
            {
                _updatingCompileTargetChoices = false;
            }

            UpdateMultiTargetControlState();
        }

        private static void SelectTargetChoice(ComboBox comboBox, string[] choices, string previous, string fallback = "")
        {
            if (choices.Length == 0)
            {
                comboBox.SelectedIndex = -1;
                return;
            }

            int selectedIndex = Array.FindIndex(choices, choice => string.Equals(choice, previous, StringComparison.OrdinalIgnoreCase));
            if (selectedIndex < 0)
            {
                selectedIndex = Array.FindIndex(choices, choice => string.Equals(choice, fallback, StringComparison.OrdinalIgnoreCase));
            }

            if (selectedIndex < 0)
            {
                selectedIndex = Array.FindIndex(choices, choice => string.Equals(choice, "CPU", StringComparison.OrdinalIgnoreCase));
            }

            comboBox.SelectedIndex = Math.Max(0, selectedIndex);
        }

        private static string GetPreferredDefaultMultiGpuTarget(string[] multiChoices)
        {
            return multiChoices.FirstOrDefault(
                target => target.Equals("MULTI:GPU.1,GPU.2", StringComparison.OrdinalIgnoreCase),
                string.Empty);
        }

        private void UpdateMultiTargetControlState()
        {
            bool multiSelected = IsMultiTargetSelection(cboWorkstationTarget.Text);
            bool hasChoices = cboWorkstationMultiTarget.Items.Count > 0;
            bool enabled = multiSelected && hasChoices;

            lblWorkstationMultiTarget.Enabled = enabled;
            cboWorkstationMultiTarget.Enabled = enabled;

            if (multiSelected && hasChoices && cboWorkstationMultiTarget.SelectedIndex < 0)
            {
                cboWorkstationMultiTarget.SelectedIndex = 0;
            }
        }

        private static bool IsMultiTargetSelection(string target)
        {
            return target.Trim().Equals("MULTI", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsOpenVinoMultiTarget(string target)
        {
            return target.Trim().StartsWith("MULTI:", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsGpuCompileTarget(string target)
        {
            string trimmed = target.Trim();
            if (!trimmed.StartsWith("GPU", StringComparison.OrdinalIgnoreCase)
                || trimmed.Contains(':', StringComparison.Ordinal))
            {
                return false;
            }

            int dotCount = 0;
            foreach (char character in trimmed)
            {
                if (character == '.')
                {
                    dotCount++;
                }
            }

            return dotCount <= 1;
        }

        private static IReadOnlyList<string> BuildMultiCompileTargetChoices(IReadOnlyList<string> gpuTargets)
        {
            if (gpuTargets.Count < 2)
            {
                return Array.Empty<string>();
            }

            List<string> choices = new()
            {
                $"MULTI:{string.Join(",", gpuTargets)}",
            };

            for (int first = 0; first < gpuTargets.Count - 1; first++)
            {
                for (int second = first + 1; second < gpuTargets.Count; second++)
                {
                    choices.Add($"MULTI:{gpuTargets[first]},{gpuTargets[second]}");
                }
            }

            return choices
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private IReadOnlyList<string> BuildCompileTargetChoices(OpenVinoSharp.Core core, IReadOnlyList<string> devices)
        {
            List<string> gpuTargets = new();
            bool cpuAvailable = false;

            foreach (string rawDevice in devices)
            {
                string device = rawDevice.Trim();
                if (device.Equals("CPU", StringComparison.OrdinalIgnoreCase))
                {
                    cpuAvailable = true;
                    continue;
                }

                if (!IsGpuCompileTarget(device))
                {
                    continue;
                }

                if (device.Contains('.', StringComparison.Ordinal))
                {
                    gpuTargets.Add(device);
                    continue;
                }

                IReadOnlyList<string> subDevices = GetOpenVinoSubDeviceTargets(core, device);
                if (subDevices.Count == 0)
                {
                    gpuTargets.Add(device);
                    continue;
                }

                gpuTargets.AddRange(subDevices.Where(IsGpuCompileTarget));
            }

            string[] distinctGpuTargets = gpuTargets
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<string> targets = new();
            if (distinctGpuTargets.Length > 0)
            {
                targets.AddRange(distinctGpuTargets);
                targets.AddRange(BuildMultiCompileTargetChoices(distinctGpuTargets));
            }
            else if (cpuAvailable)
            {
                targets.Add("CPU");
            }

            return targets
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static IReadOnlyList<string> GetOpenVinoSubDeviceTargets(OpenVinoSharp.Core core, string device)
        {
            string raw;
            try
            {
                raw = core.GetProperty(device, OpenVinoSharp.PropertyKeys.AvailableDevices);
            }
            catch
            {
                return Array.Empty<string>();
            }

            return raw
                .Split(new[] { ',', ';', '[', ']', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(subDevice => NormalizeOpenVinoSubDeviceName(device, subDevice))
                .Where(subDevice => !string.IsNullOrWhiteSpace(subDevice))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string NormalizeOpenVinoSubDeviceName(string parentDevice, string subDevice)
        {
            if (subDevice.Contains('.', StringComparison.Ordinal) || subDevice.Contains(':', StringComparison.Ordinal))
            {
                return subDevice;
            }

            return $"{parentDevice}.{subDevice}";
        }

        private void AppendHardwareDiscovery(IProgress<string>? output = null)
        {
            void Write(string message) => WriteOutput(output, message);
            bool reportedAnySection = false;

            try
            {
                List<CpuSnapshot> cpus = QueryCpuSnapshots();
                if (cpus.Count == 0)
                {
                    Write("CPU: not reported");
                }
                else
                {
                    foreach (CpuSnapshot cpu in cpus)
                    {
                        string cpuName = FirstText(cpu.Name, cpu.Caption, "Unknown CPU");
                        Write($"CPU: {cpuName}");
                        Write($"  Cores/threads: {cpu.Cores}/{cpu.LogicalProcessors}");

                        if (cpu.MaxClockSpeed > 0)
                        {
                            Write($"  Max clock: {cpu.MaxClockSpeed:N0} MHz");
                        }
                    }
                }

                reportedAnySection = true;
            }
            catch (Exception ex)
            {
                Write($"CPU query failed: {ShortError(ex)}");
            }

            try
            {
                MemorySnapshot memory = GetMemorySnapshot();
                Write($"Memory: {FormatBytes(memory.TotalPhysical)} total, {FormatBytes(memory.AvailablePhysical)} available");
                reportedAnySection = true;
            }
            catch (Exception ex)
            {
                Write($"Memory query failed: {ShortError(ex)}");
            }

            try
            {
                List<VideoAdapterSnapshot> adapters = QueryVideoAdapterSnapshots();
                if (adapters.Count == 0)
                {
                    Write("Graphics adapters: not reported");
                }
                else
                {
                    Write($"Graphics adapters: {adapters.Count}");
                    foreach (VideoAdapterSnapshot adapter in adapters)
                    {
                        string adapterName = FirstText(adapter.Name, adapter.Caption, "Unknown adapter");
                        Write($"- {adapterName}");

                        if (!string.IsNullOrWhiteSpace(adapter.AdapterCompatibility))
                        {
                            Write($"  Vendor: {adapter.AdapterCompatibility}");
                        }

                        if (!string.IsNullOrWhiteSpace(adapter.VideoProcessor))
                        {
                            Write($"  Processor: {adapter.VideoProcessor}");
                        }

                        if (adapter.AdapterRAM > 0)
                        {
                            string memoryNote = IsIntegratedVideoAdapter(adapter)
                                ? "WMI reported; integrated GPUs may share system memory"
                                : "WMI reported";
                            Write($"  Adapter RAM: {FormatBytes(adapter.AdapterRAM)} ({memoryNote})");
                        }

                        if (!string.IsNullOrWhiteSpace(adapter.DriverVersion))
                        {
                            Write($"  Driver: {adapter.DriverVersion}");
                        }

                        if (!string.IsNullOrWhiteSpace(adapter.Status))
                        {
                            Write($"  Status: {adapter.Status}");
                        }

                        if (!string.IsNullOrWhiteSpace(adapter.PnpDeviceId))
                        {
                            Write($"  PnP ID: {adapter.PnpDeviceId}");
                        }

                        string resolution = FormatResolution(adapter);
                        if (!string.IsNullOrWhiteSpace(resolution))
                        {
                            Write($"  Display: {resolution}");
                        }
                    }
                }

                reportedAnySection = true;
            }
            catch (Exception ex)
            {
                Write($"Graphics adapter query failed: {ShortError(ex)}");
            }

            if (!reportedAnySection)
            {
                Write("Hardware query failed: no hardware sections could be read.");
            }
        }

        private readonly record struct CpuSnapshot(string Name, string Caption, uint Cores, uint LogicalProcessors, uint MaxClockSpeed);

        private readonly record struct MemorySnapshot(ulong TotalPhysical, ulong AvailablePhysical);

        private readonly record struct VideoAdapterSnapshot(
            string Name,
            string Caption,
            string AdapterCompatibility,
            ulong AdapterRAM,
            string DriverVersion,
            string PnpDeviceId,
            string VideoProcessor,
            string Status,
            string VideoModeDescription,
            uint CurrentHorizontalResolution,
            uint CurrentVerticalResolution,
            uint CurrentRefreshRate);

        private static List<CpuSnapshot> QueryCpuSnapshots()
        {
            List<CpuSnapshot> cpus = new();

            foreach (object cpu in QueryWmi("SELECT Name, Caption, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed FROM Win32_Processor"))
            {
                cpus.Add(new CpuSnapshot(
                    GetWmiString(cpu, "Name"),
                    GetWmiString(cpu, "Caption"),
                    GetWmiUInt32(cpu, "NumberOfCores"),
                    GetWmiUInt32(cpu, "NumberOfLogicalProcessors"),
                    GetWmiUInt32(cpu, "MaxClockSpeed")));
            }

            return cpus;
        }

        private static List<VideoAdapterSnapshot> QueryVideoAdapterSnapshots()
        {
            List<VideoAdapterSnapshot> adapters = new();

            foreach (object adapter in QueryWmi("SELECT Name, Caption, AdapterCompatibility, AdapterRAM, DriverVersion, PNPDeviceID, VideoProcessor, Status, VideoModeDescription, CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate FROM Win32_VideoController"))
            {
                adapters.Add(new VideoAdapterSnapshot(
                    GetWmiString(adapter, "Name"),
                    GetWmiString(adapter, "Caption"),
                    GetWmiString(adapter, "AdapterCompatibility"),
                    GetWmiUInt64(adapter, "AdapterRAM"),
                    GetWmiString(adapter, "DriverVersion"),
                    GetWmiString(adapter, "PNPDeviceID"),
                    GetWmiString(adapter, "VideoProcessor"),
                    GetWmiString(adapter, "Status"),
                    GetWmiString(adapter, "VideoModeDescription"),
                    GetWmiUInt32(adapter, "CurrentHorizontalResolution"),
                    GetWmiUInt32(adapter, "CurrentVerticalResolution"),
                    GetWmiUInt32(adapter, "CurrentRefreshRate")));
            }

            return adapters;
        }

        private static bool IsIntegratedVideoAdapter(VideoAdapterSnapshot adapter)
        {
            string combined = $"{adapter.Name} {adapter.Caption} {adapter.AdapterCompatibility} {adapter.VideoProcessor}";
            return combined.Contains("Intel", StringComparison.OrdinalIgnoreCase)
                || combined.Contains("Integrated", StringComparison.OrdinalIgnoreCase)
                || combined.Contains("Iris", StringComparison.OrdinalIgnoreCase)
                || combined.Contains("UHD Graphics", StringComparison.OrdinalIgnoreCase);
        }

        private static List<object> QueryWmi(string query)
        {
            Type locatorType = Type.GetTypeFromProgID("WbemScripting.SWbemLocator")
                ?? throw new InvalidOperationException("WMI COM provider is not available.");

            object? locator = null;
            object? services = null;
            object? results = null;

            try
            {
                locator = Activator.CreateInstance(locatorType)
                    ?? throw new InvalidOperationException("Unable to create WMI locator.");
                services = locatorType.InvokeMember("ConnectServer", System.Reflection.BindingFlags.InvokeMethod, null, locator, new object?[] { ".", "root\\cimv2" })
                    ?? throw new InvalidOperationException("Unable to connect to WMI.");
                results = services.GetType().InvokeMember("ExecQuery", System.Reflection.BindingFlags.InvokeMethod, null, services, new object?[] { query });

                List<object> items = new();
                if (results is System.Collections.IEnumerable enumerable)
                {
                    foreach (object item in enumerable)
                    {
                        items.Add(item);
                    }
                }

                return items;
            }
            finally
            {
                ReleaseComObject(results);
                ReleaseComObject(services);
                ReleaseComObject(locator);
            }
        }

        private static string GetWmiString(object source, string propertyName)
        {
            object? value = GetWmiValue(source, propertyName);
            return value?.ToString() ?? string.Empty;
        }

private static uint GetWmiUInt32(object source, string propertyName) 
{ 
    object? value = GetWmiValue(source, propertyName); 
    return value is null ? 0 : System.Convert.ToUInt32(value!, System.Globalization.CultureInfo.InvariantCulture); 
} 

private static ulong GetWmiUInt64(object source, string propertyName) 
{ 
    object? value = GetWmiValue(source, propertyName); 
    return value is null ? 0 : System.Convert.ToUInt64(value!, System.Globalization.CultureInfo.InvariantCulture); 
}
        private static object? GetWmiValue(object source, string propertyName)
        {
            try
            {
                return source.GetType().InvokeMember(propertyName, System.Reflection.BindingFlags.GetProperty, null, source, null);
            }
            catch
            {
                return null;
            }
        }

        private static void ReleaseComObject(object? value)
        {
            if (value is not null && System.Runtime.InteropServices.Marshal.IsComObject(value))
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(value);
            }
        }

        private static bool TryCreateShellItem(string path, out NativeShellItem? shellItem)
        {
            shellItem = null;
            Guid shellItemId = typeof(NativeShellItem).GUID;
            int result = SHCreateItemFromParsingName(path, IntPtr.Zero, ref shellItemId, out NativeShellItem item);
            if (result < 0)
            {
                return false;
            }

            shellItem = item;
            return true;
        }

        private static string GetShellItemFileSystemPath(NativeShellItem shellItem)
        {
            shellItem.GetDisplayName(NativeShellItemDisplayName.FileSystemPath, out IntPtr pathPointer);
            try
            {
                return Marshal.PtrToStringUni(pathPointer) ?? string.Empty;
            }
            finally
            {
                if (pathPointer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pathPointer);
                }
            }
        }

        [Flags]
        private enum NativeFileOpenOptions : uint
        {
            NoChangeDir = 0x00000008,
            PickFolders = 0x00000020,
            ForceFileSystem = 0x00000040,
            PathMustExist = 0x00000800,
        }

        private enum NativeShellItemDisplayName : uint
        {
            FileSystemPath = 0x80058000,
        }

        [ComImport]
        [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        private sealed class NativeFileOpenDialog
        {
        }

        [ComImport]
        [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface NativeFileOpenDialogInterface
        {
            [PreserveSig]
            int Show(IntPtr parent);
            void SetFileTypes(uint fileTypesCount, IntPtr filterSpec);
            void SetFileTypeIndex(uint fileTypeIndex);
            void GetFileTypeIndex(out uint fileTypeIndex);
            void Advise(IntPtr events, out uint cookie);
            void Unadvise(uint cookie);
            void SetOptions(NativeFileOpenOptions options);
            void GetOptions(out NativeFileOpenOptions options);
            void SetDefaultFolder(NativeShellItem shellItem);
            void SetFolder(NativeShellItem shellItem);
            void GetFolder(out NativeShellItem shellItem);
            void GetCurrentSelection(out NativeShellItem shellItem);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);
            void GetResult(out NativeShellItem shellItem);
            void AddPlace(NativeShellItem shellItem, int fileDialogAddPlace);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string defaultExtension);
            void Close(int result);
            void SetClientGuid(ref Guid guid);
            void ClearClientData();
            void SetFilter(IntPtr filter);
            void GetResults(out IntPtr shellItemArray);
            void GetSelectedItems(out IntPtr shellItemArray);
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface NativeShellItem
        {
            void BindToHandler(IntPtr bindContext, ref Guid handlerId, ref Guid interfaceId, out IntPtr value);
            void GetParent(out NativeShellItem shellItem);
            void GetDisplayName(NativeShellItemDisplayName displayName, out IntPtr name);
            void GetAttributes(uint attributeMask, out uint attributes);
            void Compare(NativeShellItem shellItem, uint hint, out int order);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            IntPtr bindContext,
            ref Guid interfaceId,
            [MarshalAs(UnmanagedType.Interface)] out NativeShellItem shellItem);

        private static MemorySnapshot GetMemorySnapshot()
        {
            MemoryStatusEx status = new();
            if (!GlobalMemoryStatusEx(status))
            {
                return new MemorySnapshot(0, 0);
            }

            return new MemorySnapshot(status.TotalPhysical, status.AvailablePhysical);
        }


        private bool AppendOpenVinoDeviceDiagnostics(
            IProgress<string>? output = null,
            IProgress<IReadOnlyList<string>>? compileTargetsProgress = null)
        {
            void Write(string message) => WriteOutput(output, message);

            try
            {
                ConfigureOpenVinoRuntimeSearchPath();

                using OpenVinoSharp.Core core = new();
                IReadOnlyList<string> devices = core.GetAvailableDevices();

                if (devices.Count == 0)
                {
                    Write("  OpenVINO initialized, but reported no available devices.");
                    return false;
                }

                Write($"Devices: {string.Join(", ", devices)}");
                IReadOnlyList<string> compileTargets = BuildCompileTargetChoices(core, devices);
                if (compileTargetsProgress is null)
                {
                    UpdateCompileTargetChoices(compileTargets);
                }
                else
                {
                    compileTargetsProgress.Report(compileTargets);
                }
                Write($"Compile targets: {string.Join(", ", compileTargets)}");

                foreach (string device in devices)
                {
                    Write($"- {device}");
                    AppendOpenVinoProperty(core, device, "FULL_DEVICE_NAME", "Name", output: output);
                    AppendOpenVinoProperty(core, device, "DEVICE_ARCHITECTURE", "Architecture", output: output);
                    AppendOpenVinoProperty(core, device, OpenVinoSharp.PropertyKeys.AvailableDevices, "Sub-devices", suppressUnavailable: true, output: output);

                    try
                    {
                        KeyValuePair<string, OpenVinoSharp.Version> version = core.get_versions(device);
                        if (!string.IsNullOrWhiteSpace(version.Key))
                        {
                            Write($"  Runtime: {TrimOpenVinoVersion(version.Value.buildNumber)} ({version.Value.description})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Write($"  Runtime: unavailable ({ShortError(ex)})");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Write("  OpenVINO device query failed:");
                foreach (string line in FormatException(ex))
                {
                    Write($"    {line}");
                }
                return false;
            }
        }

        private void AppendEnvironmentVariable(string name)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            AppendOutput($"  {name}: {(string.IsNullOrWhiteSpace(value) ? "(not set)" : value)}");
        }

        private void AppendPathMatches(string term)
        {
            string? path = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(path))
            {
                AppendOutput("  PATH matches: (PATH is empty)");
                return;
            }

            string[] matches = path
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(entry => entry.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (matches.Length == 0)
            {
                AppendOutput($"  PATH matches containing '{term}': none");
                return;
            }

            AppendOutput($"  PATH matches containing '{term}':");
            foreach (string match in matches)
            {
                AppendOutput($"    {match}");
            }
        }

        private static string FormatFound(bool found, string path)
        {
            return found ? $"found ({path})" : $"not found ({path})";
        }

        private static string GetProjectDirectory()
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WorkStationC#.csproj")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? AppContext.BaseDirectory;
        }

        private string GetPrimaryCompileTargetForCurrentThread()
        {
            if (IsDisposed || cboWorkstationTarget.IsDisposed)
            {
                return string.Empty;
            }

            if (!cboWorkstationTarget.InvokeRequired)
            {
                return GetPrimaryCompileTarget();
            }

            try
            {
                return (string)cboWorkstationTarget.Invoke(new Func<string>(GetPrimaryCompileTarget));
            }
            catch (ObjectDisposedException)
            {
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        private async Task RunWorkstationCheckAsync(string heading)
        {
            CancellationTokenSource? operation = await TryBeginLongOperationAsync("Checking workstation...", allowCancel: false);
            if (operation is null)
            {
                return;
            }

            CancellationToken cancellationToken = operation.Token;
            btnCheckWorkstation.Enabled = false;

            try
            {
                IProgress<string> output = new Progress<string>(AppendOutput);
                IProgress<IReadOnlyList<string>> compileTargetsProgress = new Progress<IReadOnlyList<string>>(UpdateCompileTargetChoices);
                bool openVinoDevicesFound = false;

                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    output.Report(heading);
                    output.Report(string.Empty);

                    output.Report("Runtime");
                    output.Report($"  OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
                    output.Report($"  Architecture: {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture} OS, {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture} process");
                    output.Report($"  .NET: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
                    output.Report($"  CPU threads: {Environment.ProcessorCount}");
                    output.Report(string.Empty);

                    cancellationToken.ThrowIfCancellationRequested();
                    output.Report("Hardware");
                    AppendHardwareDiscovery(output);
                    output.Report(string.Empty);

                    cancellationToken.ThrowIfCancellationRequested();
                    output.Report("OpenVINO");
                    openVinoDevicesFound = AppendOpenVinoDeviceDiagnostics(output, compileTargetsProgress);
                    output.Report(string.Empty);

                    cancellationToken.ThrowIfCancellationRequested();
                    if (!openVinoDevicesFound)
                    {
                        output.Report("OpenVINO did not report devices. No external GPU inventory command was run.");
                        output.Report(string.Empty);
                    }

                    output.Report("Status");
                    output.Report("  Done. GPU visibility is reported through OpenVINO device enumeration.");
                    output.Report(string.Empty);
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                AppendOutput("Workstation check canceled.");
            }
            finally
            {
                btnCheckWorkstation.Enabled = true;
                FinishLongOperation();
            }
        }

        private async void btnCheckWorkstation_Click(object sender, EventArgs e)
        {
            await RunWorkstationCheckAsync($"Workstation check - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        private static string GetBundledSampleModelPath()
        {
            return Path.Combine(AppContext.BaseDirectory, "Samples", "mobilenet_v3_small", "mobilenet_v3_small.xml");
        }

        private void UseBundledSampleModel()
        {
            string sampleModelPath = GetBundledSampleModelPath();
            if (!File.Exists(sampleModelPath))
            {
                AppendOutput($"Sample model not found: {sampleModelPath}");
                AppendOutput("Build the app again so bundled sample content is copied to the output folder.");
                return;
            }

            string sampleModelFolder = Path.GetDirectoryName(sampleModelPath) ?? sampleModelPath;
            txtInferModel.Text = sampleModelFolder;
            AppendOutput($"Sample model folder selected: {sampleModelFolder}");
            if (RefreshSelectedRuntimeModelState(out string reason))
            {
                AppendOutput($"  Detected runtime path: {_selectedRuntimeModelPath}");
                AppendOutput($"  Detected type: {DescribeRuntimeModelKind(_selectedRuntimeModelKind)}");
            }
            else
            {
                AppendOutput($"  Sample model could not be resolved: {reason}");
            }
        }

        private void btnBrowseInferModel_Click(object? sender, EventArgs e)
        {
            BrowseForModelFolder(txtInferModel);
        }

        private void btnUseSampleModel_Click(object? sender, EventArgs e) => UseBundledSampleModel();

        private async void btnLoadModel_Click(object? sender, EventArgs e)
        {
            await RunLoadModelAsync();
        }
        #endregion

        #region Downloads
        private enum ModelCatalogSource
        {
            HuggingFace,
            OpenVino,
            Ollama,
            Intel,
            Ultralytics,
            OfficialOpenVino,
        }

        private sealed class ModelCatalogItem
        {
            public required string Id { get; init; }
            public ModelCatalogSource SourceKind { get; init; } = ModelCatalogSource.HuggingFace;
            public string Source { get; init; } = string.Empty;
            public string Task { get; init; } = string.Empty;
            public string Library { get; init; } = string.Empty;
            public string Quantization { get; init; } = string.Empty;
            public int? Downloads { get; init; }
            public int? Likes { get; init; }

            public override string ToString()
            {
                List<string> parts = new() { Id };

                if (Downloads is not null)
                {
                    parts.Add($"{Downloads:N0} downloads");
                }

                if (Likes is not null)
                {
                    parts.Add($"{Likes:N0} likes");
                }

                if (!string.IsNullOrWhiteSpace(Task))
                {
                    parts.Add(Task);
                }

                if (!string.IsNullOrWhiteSpace(Library))
                {
                    parts.Add(Library);
                }

                if (!string.IsNullOrWhiteSpace(Quantization))
                {
                    parts.Add(Quantization);
                }

                return string.Join("  |  ", parts);
            }
        }

        private sealed class ModelFileListItem
        {
            public required string Path { get; init; }
            public bool IsFolder { get; init; }
            public long? SizeBytes { get; init; }
            public string Notes { get; init; } = string.Empty;

            public override string ToString()
            {
                return IsFolder ? $"[Folder] {Path}" : Path;
            }
        }

        private sealed record DownloadFileResult(string TargetPath, bool Skipped);

        private sealed class ModelWebsiteEntry
        {
            public string Name { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;

            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(Name) ? Url : $"{Name} - {Url}";
            }
        }

        private static readonly ModelWebsiteEntry[] DefaultModelWebsites =
        {
            new ModelWebsiteEntry { Name = "Hugging Face Models", Url = "https://huggingface.co/models" },
            new ModelWebsiteEntry { Name = "Ollama Library", Url = "https://ollama.com/library" },
            new ModelWebsiteEntry { Name = "Kaggle Models", Url = "https://www.kaggle.com/models" },
            new ModelWebsiteEntry { Name = "Models.dev", Url = "https://models.dev/" },
            new ModelWebsiteEntry { Name = "OpenVINO on Hugging Face", Url = "https://huggingface.co/OpenVINO" },
            new ModelWebsiteEntry { Name = "Intel on Hugging Face", Url = "https://huggingface.co/Intel" },
            new ModelWebsiteEntry { Name = "Ultralytics Models", Url = "https://docs.ultralytics.com/models/" },
            new ModelWebsiteEntry { Name = "OpenVINO Model Zoo", Url = "https://github.com/openvinotoolkit/open_model_zoo/tree/master/models/public" },
        };

        private static ListViewItem CreateModelListViewItem(ModelCatalogItem item)
        {
            ListViewItem row = new(item.Id)
            {
                Tag = item,
            };

            row.SubItems.Add(item.Downloads?.ToString("N0") ?? string.Empty);
            row.SubItems.Add(item.Likes?.ToString("N0") ?? string.Empty);
            row.SubItems.Add(item.Source);
            row.SubItems.Add(item.Task);
            row.SubItems.Add(item.Library);
            row.SubItems.Add(item.Quantization.Replace("quant: ", string.Empty, StringComparison.OrdinalIgnoreCase));
            return row;
        }

        private static ListViewItem CreateModelFileListViewItem(ModelFileListItem item)
        {
            string extension = item.IsFolder ? string.Empty : Path.GetExtension(item.Path).TrimStart('.');
            string size = item.IsFolder
                ? string.Empty
                : item.SizeBytes is > 0
                    ? FormatBytes((ulong)item.SizeBytes.Value)
                    : "unknown";
            ListViewItem row = new(item.Path)
            {
                Tag = item,
            };

            row.SubItems.Add(item.IsFolder ? "Folder" : "File");
            row.SubItems.Add(extension);
            row.SubItems.Add(size);
            row.SubItems.Add(item.Notes);
            return row;
        }

        private static readonly System.Net.Http.HttpClient ModelCatalogClient = CreateModelCatalogClient();
        private ModelCatalogSource _modelCatalogSource = ModelCatalogSource.HuggingFace;
        private CancellationTokenSource? _modelCatalogCancellation;
        private CancellationTokenSource? _modelFileListCancellation;
        private int _modelCatalogRequestId;
        private int _modelFileListRequestId;
        private CancellationTokenSource? _downloadCancellation;
        private static System.Net.Http.HttpClient CreateModelCatalogClient()
        {
            System.Net.Http.HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WorkStationCSharp/0.1");
            return client;
        }

        private void InitializeModelCatalogButtons()
        {
            InitializeDownloadShortcutSlotPicker();
            LoadDownloadShortcutButtons();
            InitializeOvmsPullTab();
            RefreshConvertModelSelection();
        }

        private void btnPresetHuggingFace_Click(object? sender, EventArgs e) => UseDownloadShortcut(0);
        private void btnPresetOpenVino_Click(object? sender, EventArgs e) => UseDownloadShortcut(1);
        private void btnPresetIntel_Click(object? sender, EventArgs e) => UseDownloadShortcut(2);
        private void btnPresetUltralytics_Click(object? sender, EventArgs e) => UseDownloadShortcut(3);
        private void btnPresetOllama_Click(object? sender, EventArgs e) => UseDownloadShortcut(4);
        private void btnOfficialFiles_Click(object? sender, EventArgs e) => UseDownloadShortcut(5);
        private void btnRefreshOfficialIndex_Click(object? sender, EventArgs e) => UseDownloadShortcut(6);
        private void btnSearchRepos_Click(object? sender, EventArgs e) => UseDownloadShortcut(10);
        private void btnSearchAllModels_Click(object? sender, EventArgs e) => UseDownloadShortcut(11);
        private async void btnFindDownloadAddress_Click(object? sender, EventArgs e) => await FindDownloadAddressAsync();
        private async void btnLoadFiles_Click(object? sender, EventArgs e) => await DownloadFromWebAddressAsync();
        private void btnSaveFavoriteSite_Click(object? sender, EventArgs e) => SaveDownloadShortcutFromFields(GetSelectedDownloadShortcutIndex());
        private async void lstHfRepos_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SetSelectedModelId();
            await LoadSelectedModelFilesAsync();
            UpdateDownloadUrlPreview();
        }

        private void lstHfFiles_SelectedIndexChanged(object? sender, EventArgs e) => HandleDownloadFileSelectionChanged();
        private void txtRepoId_TextChanged(object? sender, EventArgs e) => UpdateDownloadUrlPreview();
        private void txtRevision_TextChanged(object? sender, EventArgs e) => UpdateDownloadUrlPreview();

        private Button[] GetDownloadShortcutButtons()
        {
            return new[]
            {
                btnPresetHuggingFace,
                btnPresetOpenVino,
                btnPresetIntel,
                btnPresetUltralytics,
                btnPresetOllama,
                btnOfficialFiles,
                btnRefreshOfficialIndex,
                btnOpenModelWebsite,
                btnAddModelWebsite,
                btnDeleteModelWebsite,
                btnSearchRepos,
                btnSearchAllModels,
            };
        }

        private void InitializeDownloadShortcutSlotPicker()
        {
            cboModelWebsites.Items.Clear();
            for (int index = 0; index < DownloadShortcutCount; index++)
            {
                cboModelWebsites.Items.Add($"Site {index + 1}");
            }

            if (cboModelWebsites.Items.Count > 0)
            {
                cboModelWebsites.SelectedIndex = 0;
            }
        }

        private void LoadDownloadShortcutButtons()
        {
            IReadOnlyList<DownloadShortcutConfig> shortcuts = GetDownloadShortcutConfigs();
            Button[] buttons = GetDownloadShortcutButtons();
            for (int index = 0; index < buttons.Length; index++)
            {
                DownloadShortcutConfig shortcut = shortcuts[index];
                ApplyDownloadShortcutButton(buttons[index], index, shortcut.Label, shortcut.Url);
            }
        }

        private void ApplyDownloadShortcutButton(Button button, int shortcutIndex, string label, string url)
        {
            string buttonText = string.IsNullOrWhiteSpace(label)
                ? $"Site {shortcutIndex + 1}"
                : label.Trim();
            button.Text = buttonText;
            button.Enabled = true;
            button.Tag = new DownloadShortcutConfig
            {
                Label = buttonText,
                Url = url.Trim(),
            };
        }

        private void UseDownloadShortcut(int shortcutIndex)
        {
            IReadOnlyList<DownloadShortcutConfig> shortcuts = GetDownloadShortcutConfigs();
            if (shortcutIndex < 0 || shortcutIndex >= shortcuts.Count)
            {
                AppendOutput("Downloads: that saved address button is not available.");
                return;
            }

            DownloadShortcutConfig shortcut = shortcuts[shortcutIndex];
            if (string.IsNullOrWhiteSpace(shortcut.Url))
            {
                AppendOutput($"Downloads: save an address to Site {shortcutIndex + 1} before using it.");
                return;
            }

            txtDownloadUrl.Text = shortcut.Url;
            txtHfQuery.Text = shortcut.Label;
            txtDownloadUrl.Focus();
            txtDownloadUrl.SelectAll();
            AppendOutput($"Downloads: filled Web Address from Site {shortcutIndex + 1}.");
        }

        private void SaveDownloadShortcutFromFields(int shortcutIndex)
        {
            string urlText = txtDownloadUrl.Text.Trim();
            if (!TryNormalizeDownloadWebAddress(urlText, out Uri? uri, out string error) || uri is null)
            {
                AppendOutput($"Downloads: {error}");
                return;
            }

            string label = txtHfQuery.Text.Trim();
            if (string.IsNullOrWhiteSpace(label))
            {
                label = BuildDownloadShortcutLabel(uri);
                txtHfQuery.Text = label;
            }

            SaveDownloadShortcutConfig(shortcutIndex, label, uri.AbsoluteUri);
            LoadDownloadShortcutButtons();
            AppendOutput($"Downloads: saved address to Site {shortcutIndex + 1}: {label}: {uri.AbsoluteUri}");
        }

        private int GetSelectedDownloadShortcutIndex()
        {
            if (cboModelWebsites.SelectedIndex >= 0 && cboModelWebsites.SelectedIndex < DownloadShortcutCount)
            {
                return cboModelWebsites.SelectedIndex;
            }

            return 0;
        }

        private static bool TryNormalizeDownloadWebAddress(string text, out Uri? uri, out string error)
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

        private static string BuildDownloadShortcutLabel(Uri uri)
        {
            if (uri.Host.Equals("huggingface.co", StringComparison.OrdinalIgnoreCase))
            {
                string[] segments = uri.AbsolutePath
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Uri.UnescapeDataString)
                    .ToArray();
                if (segments.Length >= 2)
                {
                    return segments[1];
                }
            }

            return uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? uri.Host[4..]
                : uri.Host;
        }

        private void btnOpenModelWebsite_Click(object? sender, EventArgs e) => UseDownloadShortcut(7);

        private void btnAddModelWebsite_Click(object? sender, EventArgs e) => UseDownloadShortcut(8);

        private void btnDeleteModelWebsite_Click(object? sender, EventArgs e) => UseDownloadShortcut(9);

        private void LoadModelWebsites()
        {
            List<ModelWebsiteEntry> entries;
            try
            {
                entries = LoadModelWebsiteEntries();
            }
            catch (Exception ex)
            {
                AppendOutput($"Websites: could not read saved websites: {ShortError(ex)}");
                entries = CloneDefaultModelWebsites();
            }

            cboModelWebsites.Items.Clear();
            foreach (ModelWebsiteEntry entry in entries)
            {
                cboModelWebsites.Items.Add(entry);
            }

            if (cboModelWebsites.Items.Count > 0)
            {
                cboModelWebsites.SelectedIndex = 0;
            }
            else
            {
                cboModelWebsites.Text = string.Empty;
            }
        }

        private static List<ModelWebsiteEntry> LoadModelWebsiteEntries()
        {
            string path = GetModelWebsitesPath();
            if (!File.Exists(path))
            {
                return CloneDefaultModelWebsites();
            }

            List<ModelWebsiteEntry>? entries = System.Text.Json.JsonSerializer.Deserialize<List<ModelWebsiteEntry>>(File.ReadAllText(path));
            if (entries is null)
            {
                throw new InvalidDataException($"The website list contains JSON null: {path}");
            }

            return NormalizeModelWebsiteEntries(entries);
        }

        private static void SaveModelWebsiteEntries(List<ModelWebsiteEntry> entries)
        {
            List<ModelWebsiteEntry> normalizedEntries = NormalizeModelWebsiteEntries(entries);
            System.Text.Json.JsonSerializerOptions options = new() { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(normalizedEntries, options);
            WriteTextAtomically(GetModelWebsitesPath(), NormalizeTextFileLineEndings(json));
        }

        private static string GetModelWebsitesPath()
        {
            return Path.Combine(GetProjectDirectory(), "model-websites.json");
        }

        private static List<ModelWebsiteEntry> CloneDefaultModelWebsites()
        {
            return DefaultModelWebsites
                .Select(entry => new ModelWebsiteEntry { Name = entry.Name, Url = entry.Url })
                .ToList();
        }

        private static List<ModelWebsiteEntry> NormalizeModelWebsiteEntries(IEnumerable<ModelWebsiteEntry> entries)
        {
            List<ModelWebsiteEntry> normalizedEntries = new();
            HashSet<string> seenUrls = new(StringComparer.OrdinalIgnoreCase);
            foreach (ModelWebsiteEntry entry in entries)
            {
                string input = string.IsNullOrWhiteSpace(entry.Name)
                    ? entry.Url
                    : $"{entry.Name} | {entry.Url}";
                if (!TryCreateModelWebsiteEntry(input, out ModelWebsiteEntry normalizedEntry, out string error))
                {
                    throw new InvalidDataException($"Bad website entry `{entry}`: {error}");
                }

                if (seenUrls.Add(normalizedEntry.Url))
                {
                    normalizedEntries.Add(normalizedEntry);
                }
            }

            return normalizedEntries;
        }

        private List<ModelWebsiteEntry> GetModelWebsiteEntriesFromUi()
        {
            return cboModelWebsites.Items
                .OfType<ModelWebsiteEntry>()
                .Select(entry => new ModelWebsiteEntry { Name = entry.Name, Url = entry.Url })
                .ToList();
        }

        private bool TryGetSelectedOrTypedModelWebsite(out ModelWebsiteEntry entry, out string error)
        {
            string typed = cboModelWebsites.Text.Trim();
            if (cboModelWebsites.SelectedItem is ModelWebsiteEntry selected
                && (string.IsNullOrWhiteSpace(typed) || typed.Equals(selected.ToString(), StringComparison.Ordinal)))
            {
                entry = new ModelWebsiteEntry { Name = selected.Name, Url = selected.Url };
                error = string.Empty;
                return true;
            }

            return TryCreateModelWebsiteEntry(typed, out entry, out error);
        }

        private void SelectModelWebsite(string url)
        {
            for (int index = 0; index < cboModelWebsites.Items.Count; index++)
            {
                if (cboModelWebsites.Items[index] is ModelWebsiteEntry entry
                    && entry.Url.Equals(url, StringComparison.OrdinalIgnoreCase))
                {
                    cboModelWebsites.SelectedIndex = index;
                    return;
                }
            }
        }

        private static bool TryCreateModelWebsiteEntry(string text, out ModelWebsiteEntry entry, out string error)
        {
            entry = new ModelWebsiteEntry();
            string input = text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                error = "enter a website address first.";
                return false;
            }

            string name = string.Empty;
            string urlText = input;
            int separatorIndex = input.LastIndexOf('|');
            if (separatorIndex >= 0)
            {
                name = input[..separatorIndex].Trim();
                urlText = input[(separatorIndex + 1)..].Trim();
            }
            else
            {
                int schemeIndex = FindUrlSchemeIndex(input);
                if (schemeIndex > 0)
                {
                    name = input[..schemeIndex].Trim().TrimEnd('-', '\u2013', '\u2014').Trim();
                    urlText = input[schemeIndex..].Trim();
                }
            }

            if (!urlText.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !urlText.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                urlText = $"https://{urlText.TrimStart('/')}";
            }

            if (!Uri.TryCreate(urlText, UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || string.IsNullOrWhiteSpace(uri.Host))
            {
                error = "enter a normal web address, like https://huggingface.co/models.";
                return false;
            }

            entry = new ModelWebsiteEntry
            {
                Name = string.IsNullOrWhiteSpace(name) ? uri.Host : name,
                Url = uri.AbsoluteUri,
            };
            error = string.Empty;
            return true;
        }

        private static int FindUrlSchemeIndex(string text)
        {
            int httpIndex = text.IndexOf("http://", StringComparison.OrdinalIgnoreCase);
            int httpsIndex = text.IndexOf("https://", StringComparison.OrdinalIgnoreCase);
            if (httpIndex < 0)
            {
                return httpsIndex;
            }

            if (httpsIndex < 0)
            {
                return httpIndex;
            }

            return Math.Min(httpIndex, httpsIndex);
        }

        private static string NormalizeTextFileLineEndings(string text)
        {
            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
            return normalized.EndsWith(Environment.NewLine, StringComparison.Ordinal)
                ? normalized
                : normalized + Environment.NewLine;
        }

        private void btnDeleteDownload_Click(object? sender, EventArgs e)
        {
            string repoId = txtRepoId.Text.Trim();
            if (string.IsNullOrWhiteSpace(repoId))
            {
                AppendOutput("Delete download: select or enter a repo/model ID first.");
                return;
            }

            string revision = string.IsNullOrWhiteSpace(txtRevision.Text) ? "main" : txtRevision.Text.Trim();
            string downloadFolder;
            try
            {
                downloadFolder = BuildModelDownloadDirectory(repoId, revision);
            }
            catch (Exception ex)
            {
                AppendOutput($"Delete download failed: {ShortError(ex)}");
                return;
            }

            string downloadRoot = Path.GetFullPath(GetDownloadSafetyRoot(repoId));
            if (!IsPathUnder(downloadFolder, downloadRoot))
            {
                AppendOutput("Delete download refused: selected folder is outside the model library.");
                return;
            }

            if (!Directory.Exists(downloadFolder))
            {
                AppendOutput($"Delete download: folder not found: {downloadFolder}");
                return;
            }

            DialogResult result = MessageBox.Show(
                this,
                $"Delete downloaded model folder?{Environment.NewLine}{downloadFolder}",
                "Delete Download",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Directory.Delete(downloadFolder, recursive: true);
                lstHfFiles.Items.Clear();
                RefreshConvertModelSelection();
                AppendOutput($"Delete download: removed {downloadFolder}");
            }
            catch (Exception ex)
            {
                AppendOutput($"Delete download failed: {ShortError(ex)}");
            }
        }


        private async Task LoadHuggingFaceModelsAsync(ModelCatalogSource source, string sourceName, string query)
        {
            int requestId = BeginModelCatalogRequest(out CancellationToken cancellationToken);
            _modelCatalogSource = source;
            txtHfQuery.Text = query;
            lstHfRepos.Items.Clear();
            lstHfFiles.Items.Clear();
            txtRepoId.Clear();

            try
            {
                AppendOutput($"Models: loading {sourceName} results from Hugging Face...");
                string escapedQuery = Uri.EscapeDataString(query);
                string url = $"https://huggingface.co/api/models?search={escapedQuery}&sort=downloads&direction=-1&limit=50";
                using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using System.Text.Json.JsonDocument document = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                List<ListViewItem> rows = new();
                foreach (System.Text.Json.JsonElement model in document.RootElement.EnumerateArray())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!model.TryGetProperty("id", out System.Text.Json.JsonElement idElement))
                    {
                        continue;
                    }

                    string? modelId = idElement.GetString();
                    if (string.IsNullOrWhiteSpace(modelId))
                    {
                        continue;
                    }

                    rows.Add(CreateModelListViewItem(CreateHuggingFaceCatalogItem(model, modelId, source, sourceName)));
                }

                if (!IsCurrentModelCatalogRequest(requestId, cancellationToken))
                {
                    return;
                }

                foreach (ListViewItem row in rows)
                {
                    lstHfRepos.Items.Add(row);
                }
                SelectFirstModelCatalogItem();
                AppendOutput($"Models: loaded {rows.Count} {sourceName} entries.");
            }
            catch (OperationCanceledException) when (!IsCurrentModelCatalogRequest(requestId, cancellationToken))
            {
            }
            catch (Exception ex)
            {
                AppendOutput($"Models: {sourceName} lookup failed ({ShortError(ex)})");
            }
        }

        private async Task LoadOllamaLibraryAsync(string query)
        {
            int requestId = BeginModelCatalogRequest(out CancellationToken cancellationToken);
            _modelCatalogSource = ModelCatalogSource.Ollama;
            txtHfQuery.Text = query;
            lstHfRepos.Items.Clear();
            lstHfFiles.Items.Clear();
            txtRepoId.Clear();
            txtRevision.Text = "latest";

            try
            {
                AppendOutput("Models: loading Ollama library results...");
                string trimmedQuery = query.Trim();
                string url = string.IsNullOrWhiteSpace(trimmedQuery)
                    ? "https://ollama.com/search"
                    : $"https://ollama.com/search?q={Uri.EscapeDataString(trimmedQuery)}";
                string html = await ModelCatalogClient.GetStringAsync(url, cancellationToken);

                List<ListViewItem> rows = ParseOllamaLibraryRows(html)
                    .Select(CreateModelListViewItem)
                    .ToList();

                if (!IsCurrentModelCatalogRequest(requestId, cancellationToken))
                {
                    return;
                }

                foreach (ListViewItem row in rows)
                {
                    lstHfRepos.Items.Add(row);
                }
                SelectFirstModelCatalogItem();
                AppendOutput($"Models: loaded {rows.Count} Ollama entries.");
            }
            catch (OperationCanceledException) when (!IsCurrentModelCatalogRequest(requestId, cancellationToken))
            {
            }
            catch (Exception ex)
            {
                AppendOutput($"Models: Ollama library lookup failed ({ShortError(ex)})");
            }
        }

        private async Task LoadAllModelCatalogsAsync()
        {
            int requestId = BeginModelCatalogRequest(out CancellationToken cancellationToken);
            string query = txtHfQuery.Text.Trim();
            lstHfRepos.Items.Clear();
            lstHfFiles.Items.Clear();
            txtRepoId.Clear();

            if (string.IsNullOrWhiteSpace(query))
            {
                AppendOutput("Models: enter a model name to search, such as Qwen3.6.");
                return;
            }

            try
            {
                AppendOutput($"Models: searching model sources for `{query}`...");
                List<ModelCatalogItem> items = new();
                await AddCatalogResultsAsync(items, "Hugging Face", () => FetchHuggingFaceCatalogItemsAsync(ModelCatalogSource.HuggingFace, "Hugging Face", query, cancellationToken));
                await AddCatalogResultsAsync(items, "OpenVINO on Hugging Face", () => FetchHuggingFaceCatalogItemsAsync(ModelCatalogSource.OpenVino, "OpenVINO", $"OpenVINO {query}", cancellationToken));
                await AddCatalogResultsAsync(items, "Intel on Hugging Face", () => FetchHuggingFaceCatalogItemsAsync(ModelCatalogSource.Intel, "Intel", $"Intel {query}", cancellationToken));
                await AddCatalogResultsAsync(items, "Ollama Library", async () =>
                {
                    string html = await ModelCatalogClient.GetStringAsync($"https://ollama.com/search?q={Uri.EscapeDataString(query)}", cancellationToken);
                    return ParseOllamaLibraryRows(html);
                });
                await AddCatalogResultsAsync(items, "OpenVINO Model Zoo", () => FetchOpenVinoModelZooSearchItemsAsync(query, cancellationToken));

                if (!IsCurrentModelCatalogRequest(requestId, cancellationToken))
                {
                    return;
                }

                List<ModelCatalogItem> distinctItems = items
                    .GroupBy(item => $"{item.SourceKind}|{item.Id}", StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderBy(item => item.Source, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (ModelCatalogItem item in distinctItems)
                {
                    lstHfRepos.Items.Add(CreateModelListViewItem(item));
                }

                SelectFirstModelCatalogItem();
                AppendOutput($"Models: found {distinctItems.Count} result(s) across searchable sources.");
                AppendOutput("Models: Kaggle, Models.dev, and general websites are not searched here because they do not share a simple downloadable file-list API in this app.");
            }
            catch (OperationCanceledException) when (!IsCurrentModelCatalogRequest(requestId, cancellationToken))
            {
            }
            catch (OperationCanceledException)
            {
                AppendOutput("Models: search canceled.");
            }
        }

        private async Task AddCatalogResultsAsync(
            List<ModelCatalogItem> items,
            string sourceName,
            Func<Task<IReadOnlyList<ModelCatalogItem>>> load)
        {
            try
            {
                IReadOnlyList<ModelCatalogItem> results = await load();
                items.AddRange(results);
                AppendOutput($"  {sourceName}: {results.Count} result(s)");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                AppendOutput($"  {sourceName}: search failed ({ShortError(ex)})");
            }
        }

        private async Task<IReadOnlyList<ModelCatalogItem>> FetchHuggingFaceCatalogItemsAsync(
            ModelCatalogSource source,
            string sourceName,
            string query,
            CancellationToken cancellationToken)
        {
            string escapedQuery = Uri.EscapeDataString(query);
            string url = $"https://huggingface.co/api/models?search={escapedQuery}&sort=downloads&direction=-1&limit=25";
            using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using System.Text.Json.JsonDocument document = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            List<ModelCatalogItem> results = new();
            foreach (System.Text.Json.JsonElement model in document.RootElement.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!model.TryGetProperty("id", out System.Text.Json.JsonElement idElement))
                {
                    continue;
                }

                string? modelId = idElement.GetString();
                if (!string.IsNullOrWhiteSpace(modelId))
                {
                    results.Add(CreateHuggingFaceCatalogItem(model, modelId, source, sourceName));
                }
            }

            return results;
        }

        private async Task<IReadOnlyList<ModelCatalogItem>> FetchOpenVinoModelZooSearchItemsAsync(
            string query,
            CancellationToken cancellationToken)
        {
            List<ModelCatalogItem> results = new();
            foreach (string section in new[] { "intel", "public" })
            {
                cancellationToken.ThrowIfCancellationRequested();
                string url = $"https://api.github.com/repos/openvinotoolkit/open_model_zoo/contents/models/{section}";
                using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using System.Text.Json.JsonDocument document = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                foreach (System.Text.Json.JsonElement entry in document.RootElement.EnumerateArray())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!entry.TryGetProperty("type", out System.Text.Json.JsonElement typeElement)
                        || !string.Equals(typeElement.GetString(), "dir", StringComparison.OrdinalIgnoreCase)
                        || !entry.TryGetProperty("name", out System.Text.Json.JsonElement nameElement))
                    {
                        continue;
                    }

                    string? modelName = nameElement.GetString();
                    if (string.IsNullOrWhiteSpace(modelName)
                        || !modelName.Contains(query, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    results.Add(new ModelCatalogItem
                    {
                        Id = $"open_model_zoo/{section}/{modelName}",
                        SourceKind = ModelCatalogSource.OfficialOpenVino,
                        Source = "OpenVINO Model Zoo",
                        Task = section,
                    });
                }
            }

            return results;
        }

        private static IReadOnlyList<ModelCatalogItem> ParseOllamaLibraryRows(string html)
        {
            List<ModelCatalogItem> items = new();
            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            System.Text.RegularExpressions.Regex linkRegex = new(
                "href=\"/library/(?<name>[A-Za-z0-9][A-Za-z0-9._-]*(?:/[A-Za-z0-9][A-Za-z0-9._-]*)?)\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(200));

            foreach (System.Text.RegularExpressions.Match match in linkRegex.Matches(html))
            {
                string name = System.Net.WebUtility.HtmlDecode(match.Groups["name"].Value).Trim('/');
                if (string.IsNullOrWhiteSpace(name) || !seen.Add(name))
                {
                    continue;
                }

                items.Add(new ModelCatalogItem
                {
                    Id = name,
                    SourceKind = ModelCatalogSource.Ollama,
                    Source = "Ollama",
                    Task = "Ollama library",
                    Library = "GGUF",
                });

                if (items.Count >= 100)
                {
                    break;
                }
            }

            return items;
        }

        private async Task LoadOpenVinoModelZooIndexAsync()
        {
            int requestId = BeginModelCatalogRequest(out CancellationToken cancellationToken);
            _modelCatalogSource = ModelCatalogSource.OfficialOpenVino;
            txtHfQuery.Text = "openvino model zoo";
            lstHfRepos.Items.Clear();
            lstHfFiles.Items.Clear();
            txtRepoId.Clear();

            try
            {
                AppendOutput("Models: loading official OpenVINO Model Zoo index...");
                List<ListViewItem> rows = new();
                foreach (string section in new[] { "intel", "public" })
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string url = $"https://api.github.com/repos/openvinotoolkit/open_model_zoo/contents/models/{section}";
                    using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(url, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using System.Text.Json.JsonDocument document = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                    foreach (System.Text.Json.JsonElement entry in document.RootElement.EnumerateArray())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!entry.TryGetProperty("type", out System.Text.Json.JsonElement typeElement)
                            || !string.Equals(typeElement.GetString(), "dir", StringComparison.OrdinalIgnoreCase)
                            || !entry.TryGetProperty("name", out System.Text.Json.JsonElement nameElement))
                        {
                            continue;
                        }

                        string? modelName = nameElement.GetString();
                        if (string.IsNullOrWhiteSpace(modelName))
                        {
                            continue;
                        }

                        rows.Add(CreateModelListViewItem(new ModelCatalogItem
                        {
                            Id = $"open_model_zoo/{section}/{modelName}",
                            SourceKind = ModelCatalogSource.OfficialOpenVino,
                            Source = "OpenVINO Model Zoo",
                            Task = section,
                        }));
                    }
                }

                if (!IsCurrentModelCatalogRequest(requestId, cancellationToken))
                {
                    return;
                }

                foreach (ListViewItem row in rows)
                {
                    lstHfRepos.Items.Add(row);
                }
                SelectFirstModelCatalogItem();
                AppendOutput($"Models: loaded {rows.Count} official OpenVINO entries.");
            }
            catch (OperationCanceledException) when (!IsCurrentModelCatalogRequest(requestId, cancellationToken))
            {
            }
            catch (Exception ex)
            {
                AppendOutput($"Models: official OpenVINO index lookup failed ({ShortError(ex)})");
            }
        }

        private void SelectFirstModelCatalogItem()
        {
            if (lstHfRepos.Items.Count > 0)
            {
                lstHfRepos.Items[0].Selected = true;
                lstHfRepos.Items[0].Focused = true;
                SetSelectedModelId();
            }
        }

        private void SetSelectedModelId()
        {
            if (lstHfRepos.SelectedItems.Count > 0)
            {
                if (lstHfRepos.SelectedItems[0].Tag is ModelCatalogItem item)
                {
                    _modelCatalogSource = item.SourceKind;
                    txtRepoId.Text = item.Id;
                }
                else
                {
                    txtRepoId.Text = lstHfRepos.SelectedItems[0].Text;
                }

                txtRevision.Text = _modelCatalogSource == ModelCatalogSource.Ollama ? "latest" : "main";
            }
        }

        private int BeginModelCatalogRequest(out CancellationToken cancellationToken)
        {
            _modelCatalogCancellation?.Cancel();
            _modelCatalogCancellation?.Dispose();
            _modelFileListCancellation?.Cancel();
            _modelFileListCancellation?.Dispose();
            _modelFileListCancellation = null;
            _modelFileListRequestId++;
            _modelCatalogCancellation = new CancellationTokenSource();
            cancellationToken = _modelCatalogCancellation.Token;
            return ++_modelCatalogRequestId;
        }

        private bool IsCurrentModelCatalogRequest(int requestId, CancellationToken cancellationToken)
        {
            return requestId == _modelCatalogRequestId && !cancellationToken.IsCancellationRequested;
        }

        private int BeginModelFileListRequest(out CancellationToken cancellationToken)
        {
            _modelFileListCancellation?.Cancel();
            _modelFileListCancellation?.Dispose();
            _modelFileListCancellation = new CancellationTokenSource();
            cancellationToken = _modelFileListCancellation.Token;
            return ++_modelFileListRequestId;
        }

        private bool IsCurrentModelFileListRequest(int requestId, CancellationToken cancellationToken)
        {
            return requestId == _modelFileListRequestId && !cancellationToken.IsCancellationRequested;
        }

        private async Task LoadSelectedModelFilesAsync()
        {
            string repoId = txtRepoId.Text.Trim();
            if (string.IsNullOrWhiteSpace(repoId))
            {
                return;
            }

            int requestId = BeginModelFileListRequest(out CancellationToken cancellationToken);
            lstHfFiles.Items.Clear();

            try
            {
                if (_modelCatalogSource == ModelCatalogSource.Ollama)
                {
                    await LoadOllamaModelTagsAsync(repoId, requestId, cancellationToken);
                }
                else if (repoId.StartsWith("open_model_zoo/", StringComparison.OrdinalIgnoreCase))
                {
                    await LoadOpenVinoModelZooFilesAsync(repoId, requestId, cancellationToken);
                }
                else
                {
                    await LoadHuggingFaceModelFilesAsync(repoId, GetRemoteRevision(), requestId, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (!IsCurrentModelFileListRequest(requestId, cancellationToken))
            {
            }
            catch (Exception ex)
            {
                if (IsCurrentModelFileListRequest(requestId, cancellationToken))
                {
                    AppendOutput($"Models: file list lookup failed for {repoId} ({ShortError(ex)})");
                }
            }
        }

        private async Task LoadOllamaModelTagsAsync(string modelName, int requestId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return;
            }

            AppendOutput($"Models: loading Ollama tags for {modelName}...");
            string url = $"https://ollama.com/library/{EscapeUrlPath(modelName)}/tags";
            string html = await ModelCatalogClient.GetStringAsync(url, cancellationToken);
            IReadOnlyList<string> tags = ParseOllamaModelTags(html, modelName);
            if (!IsCurrentModelFileListRequest(requestId, cancellationToken))
            {
                return;
            }

            PopulateModelFileList(tags.Select(tag => new ModelFileListItem
            {
                Path = tag,
                IsFolder = false,
                Notes = "Ollama registry tag",
            }));

            if (lstHfFiles.Items.Count > 0)
            {
                lstHfFiles.Items[0].Selected = true;
                lstHfFiles.Items[0].Focused = true;
            }

            txtRevision.Text = tags.FirstOrDefault() ?? "latest";
            AppendOutput($"Models: loaded {tags.Count} Ollama tag(s) for {modelName}.");
        }

        private static IReadOnlyList<string> ParseOllamaModelTags(string html, string modelName)
        {
            List<string> tags = new() { "latest" };
            string escapedModelName = System.Text.RegularExpressions.Regex.Escape(modelName.Trim());
            System.Text.RegularExpressions.Regex tagRegex = new(
                $"{escapedModelName}:(?<tag>[A-Za-z0-9][A-Za-z0-9._-]*)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(200));

            foreach (System.Text.RegularExpressions.Match match in tagRegex.Matches(html))
            {
                string tag = System.Net.WebUtility.HtmlDecode(match.Groups["tag"].Value);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    tags.Add(tag);
                }
            }

            return tags
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag.Equals("latest", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .Take(200)
                .ToArray();
        }

        private async Task LoadHuggingFaceModelFilesAsync(
            string repoId,
            string revision,
            int requestId,
            CancellationToken cancellationToken)
        {
            AppendOutput($"Models: loading files for {repoId}...");
            List<ModelFileListItem> fileItems = await GetHuggingFaceModelFileItemsAsync(repoId, revision, cancellationToken);

            if (!IsCurrentModelFileListRequest(requestId, cancellationToken))
            {
                return;
            }

            PopulateModelFileList(fileItems);
            AppendOutput($"Models: loaded {fileItems.Count} files for {repoId}.");
        }

        private async Task<List<ModelFileListItem>> GetHuggingFaceModelFileItemsAsync(
            string repoId,
            string revision,
            CancellationToken cancellationToken)
        {
            string escapedRepoId = string.Join("/", repoId.Split('/').Select(Uri.EscapeDataString));
            string revisionQuery = string.IsNullOrWhiteSpace(revision)
                ? string.Empty
                : $"?revision={Uri.EscapeDataString(revision)}";
            string url = $"https://huggingface.co/api/models/{escapedRepoId}{revisionQuery}";

            using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(url, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound && !revision.Equals("main", StringComparison.OrdinalIgnoreCase))
            {
                AppendOutput($"Downloads: Hugging Face did not find revision `{revision}` for {repoId}. Check the address, or use `/tree/main` if the branch is main.");
            }

            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using System.Text.Json.JsonDocument document = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("siblings", out System.Text.Json.JsonElement siblings)
                || siblings.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                return new List<ModelFileListItem>();
            }

            List<ModelFileListItem> fileItems = new();
            foreach (System.Text.Json.JsonElement sibling in siblings.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                string fileName = TryReadString(sibling, "rfilename");
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                long? sizeBytes = TryReadLong(sibling, "size") ?? TryReadNestedLong(sibling, "lfs", "size");
                fileItems.Add(new ModelFileListItem
                {
                    Path = fileName,
                    IsFolder = false,
                    SizeBytes = sizeBytes,
                    Notes = BuildFileNotes(fileName, sizeBytes),
                });
            }

            return fileItems;
        }

        private async Task LoadOpenVinoModelZooFilesAsync(
            string repoId,
            int requestId,
            CancellationToken cancellationToken)
        {
            string[] parts = repoId.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                AppendOutput($"Models: cannot parse Open Model Zoo path {repoId}.");
                return;
            }

            string section = Uri.EscapeDataString(parts[1]);
            string modelName = Uri.EscapeDataString(parts[2]);
            string url = $"https://api.github.com/repos/openvinotoolkit/open_model_zoo/contents/models/{section}/{modelName}";

            AppendOutput($"Models: loading Open Model Zoo files for {repoId}...");
            using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using System.Text.Json.JsonDocument document = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            List<ModelFileListItem> fileItems = new();
            List<string> folderNames = new();
            foreach (System.Text.Json.JsonElement entry in document.RootElement.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                string name = TryReadString(entry, "name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                string type = TryReadString(entry, "type");
                if (string.Equals(type, "dir", StringComparison.OrdinalIgnoreCase))
                {
                    folderNames.Add(name);
                }
                else
                {
                    long? sizeBytes = TryReadLong(entry, "size");
                    fileItems.Add(new ModelFileListItem
                    {
                        Path = name,
                        IsFolder = false,
                        SizeBytes = sizeBytes,
                        Notes = BuildFileNotes(name, sizeBytes),
                    });
                }
            }

            if (!IsCurrentModelFileListRequest(requestId, cancellationToken))
            {
                return;
            }

            PopulateModelFileList(fileItems, folderNames);
            AppendOutput($"Models: loaded {fileItems.Count} files and {folderNames.Count} folders for {repoId}.");
        }

        private void PopulateModelFileList(IEnumerable<string> fileNames, IEnumerable<string>? explicitFolderNames = null)
        {
            PopulateModelFileList(
                fileNames
                    .Where(file => !string.IsNullOrWhiteSpace(file))
                    .Select(file => new ModelFileListItem
                    {
                        Path = file,
                        IsFolder = false,
                        Notes = BuildFileNotes(file, null),
                    }),
                explicitFolderNames);
        }

        private void PopulateModelFileList(IEnumerable<ModelFileListItem> fileItems, IEnumerable<string>? explicitFolderNames = null)
        {
            lstHfFiles.Items.Clear();

            List<ModelFileListItem> files = fileItems
                .Where(item => !string.IsNullOrWhiteSpace(item.Path))
                .Select(item => new ModelFileListItem
                {
                    Path = item.Path.Replace('\\', '/'),
                    IsFolder = item.IsFolder,
                    SizeBytes = item.SizeBytes,
                    Notes = item.Notes,
                })
                .GroupBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Where(item => !item.IsFolder)
                .OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            SortedSet<string> folders = new(StringComparer.OrdinalIgnoreCase);
            foreach (string folder in explicitFolderNames ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    folders.Add(folder.Trim().TrimEnd('/', '\\').Replace('\\', '/'));
                }
            }

            foreach (ModelFileListItem file in files)
            {
                foreach (string folder in GetModelFileFolderChain(file.Path))
                {
                    folders.Add(folder);
                }
            }

            foreach (string folder in folders)
            {
                lstHfFiles.Items.Add(CreateModelFileListViewItem(new ModelFileListItem
                {
                    Path = folder,
                    IsFolder = true,
                }));
            }

            foreach (ModelFileListItem file in files)
            {
                lstHfFiles.Items.Add(CreateModelFileListViewItem(file));
            }
        }

        private sealed class HuggingFaceDownloadAddress
        {
            public required string RepoId { get; init; }
            public required string Revision { get; init; }
            public string FolderPath { get; init; } = string.Empty;
            public string FilePath { get; init; } = string.Empty;
        }

        private async Task FindDownloadAddressAsync()
        {
            if (_downloadCancellation is not null)
            {
                AppendOutput("Find: wait for the current download to finish first.");
                return;
            }

            if (!TryNormalizeDownloadWebAddress(txtDownloadUrl.Text, out Uri? uri, out string error) || uri is null)
            {
                AppendOutput($"Find: {error}");
                return;
            }

            txtDownloadUrl.Text = uri.AbsoluteUri;
            btnClearFileSelection.Enabled = false;
            lblDownloadStatus.Text = "Checking address...";

            try
            {
                using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(30));
                if (uri.Host.Equals("huggingface.co", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseHuggingFaceDownloadAddress(uri, out HuggingFaceDownloadAddress? address, out error) || address is null)
                    {
                        AppendOutput($"Find: {error}");
                        lblDownloadStatus.Text = "Address not found";
                        return;
                    }

                    await FindHuggingFaceAddressAsync(address, timeout.Token);
                    return;
                }

                await FindDirectWebFileAsync(uri, timeout.Token);
            }
            catch (OperationCanceledException)
            {
                lblDownloadStatus.Text = "Find timed out";
                AppendOutput("Find: address check timed out.");
            }
            catch (Exception ex)
            {
                lblDownloadStatus.Text = "Address not found";
                AppendOutput($"Find: address check failed ({ShortError(ex)})");
            }
            finally
            {
                btnClearFileSelection.Enabled = true;
            }
        }

        private async Task FindHuggingFaceAddressAsync(HuggingFaceDownloadAddress address, CancellationToken cancellationToken)
        {
            _modelCatalogSource = ModelCatalogSource.HuggingFace;
            txtRepoId.Text = address.RepoId;
            txtRevision.Text = address.Revision;
            lstHfRepos.Items.Clear();
            lstHfFiles.Items.Clear();

            AppendOutput($"Find: checking Hugging Face address for {address.RepoId}...");
            List<ModelFileListItem> fileItems = await GetHuggingFaceModelFileItemsAsync(
                address.RepoId,
                address.Revision,
                cancellationToken);
            PopulateModelFileList(fileItems);

            List<string> files = fileItems
                .Where(item => !item.IsFolder)
                .Select(item => item.Path)
                .Where(file => string.IsNullOrWhiteSpace(address.FilePath)
                    ? string.IsNullOrWhiteSpace(address.FolderPath) || IsInModelFolder(file, address.FolderPath)
                    : file.Equals(address.FilePath, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (files.Count == 0)
            {
                lblDownloadStatus.Text = "No files found";
                AppendOutput("Find: address opened, but no downloadable files matched that repo path.");
                return;
            }

            string destinationFolder = BuildModelDownloadDirectory(address.RepoId, address.Revision);
            lblDownloadStatus.Text = $"Found {files.Count:N0} file(s)";
            AppendOutput("Find: address looks good.");
            AppendOutput($"  Repository: {address.RepoId}");
            AppendOutput($"  Revision: {address.Revision}");
            if (!string.IsNullOrWhiteSpace(address.FolderPath))
            {
                AppendOutput($"  Folder: {address.FolderPath}");
            }

            if (!string.IsNullOrWhiteSpace(address.FilePath))
            {
                AppendOutput($"  File: {address.FilePath}");
            }

            AppendOutput($"  Files found: {files.Count:N0}");
            AppendOutput($"  Download folder: {destinationFolder}");
        }

        private async Task FindDirectWebFileAsync(Uri uri, CancellationToken cancellationToken)
        {
            string fileName = Uri.UnescapeDataString(Path.GetFileName(uri.LocalPath));
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.', StringComparison.Ordinal))
            {
                lblDownloadStatus.Text = "Address is a web page";
                AppendOutput("Find: this looks like a web page. Use a Hugging Face model address or a direct file address.");
                return;
            }

            AppendOutput($"Find: checking direct file address {uri.AbsoluteUri}...");
            using System.Net.Http.HttpRequestMessage headRequest = new(System.Net.Http.HttpMethod.Head, uri);
            using System.Net.Http.HttpResponseMessage headResponse = await ModelCatalogClient.SendAsync(
                headRequest,
                System.Net.Http.HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (headResponse.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
            {
                using System.Net.Http.HttpResponseMessage getResponse = await ModelCatalogClient.GetAsync(
                    uri,
                    System.Net.Http.HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                AppendDirectWebFileFindResult(uri, fileName, getResponse);
                return;
            }

            AppendDirectWebFileFindResult(uri, fileName, headResponse);
        }

        private void AppendDirectWebFileFindResult(Uri uri, string fileName, System.Net.Http.HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            long? sizeBytes = response.Content.Headers.ContentLength;
            string sizeText = sizeBytes is > 0 ? FormatBytes((ulong)sizeBytes.Value) : "unknown size";
            string targetPath = BuildDirectWebFileDownloadPath(uri, fileName);

            lblDownloadStatus.Text = $"Found {sizeText}";
            AppendOutput("Find: direct file address looks good.");
            AppendOutput($"  File: {fileName}");
            AppendOutput($"  Size: {sizeText}");
            AppendOutput($"  Download path: {targetPath}");
        }

        private async Task DownloadFromWebAddressAsync()
        {
            if (_downloadCancellation is not null)
            {
                AppendOutput("Downloads: a download is already running.");
                return;
            }

            if (!TryNormalizeDownloadWebAddress(txtDownloadUrl.Text, out Uri? uri, out string error) || uri is null)
            {
                AppendOutput($"Downloads: {error}");
                return;
            }

            txtDownloadUrl.Text = uri.AbsoluteUri;

            if (uri.Host.Equals("huggingface.co", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryParseHuggingFaceDownloadAddress(uri, out HuggingFaceDownloadAddress? address, out error) || address is null)
                {
                    AppendOutput($"Downloads: {error}");
                    return;
                }

                await DownloadHuggingFaceAddressAsync(address);
                return;
            }

            await DownloadDirectWebFileAsync(uri);
        }

        private static bool TryParseHuggingFaceDownloadAddress(
            Uri uri,
            out HuggingFaceDownloadAddress? address,
            out string error)
        {
            address = null;
            string[] segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.UnescapeDataString)
                .ToArray();

            if (segments.Length < 2)
            {
                error = "paste a Hugging Face model address, like https://huggingface.co/Qwen/Qwen3-Coder-Next-FP8/tree/main.";
                return false;
            }

            string repoId = $"{segments[0]}/{segments[1]}";
            string revision = "main";
            string folderPath = string.Empty;
            string filePath = string.Empty;

            if (segments.Length > 2)
            {
                string mode = segments[2];
                if (mode.Equals("tree", StringComparison.OrdinalIgnoreCase))
                {
                    if (segments.Length >= 4)
                    {
                        revision = segments[3];
                    }

                    if (segments.Length >= 5)
                    {
                        folderPath = string.Join("/", segments.Skip(4));
                    }
                }
                else if (mode.Equals("blob", StringComparison.OrdinalIgnoreCase)
                    || mode.Equals("resolve", StringComparison.OrdinalIgnoreCase))
                {
                    if (segments.Length < 5)
                    {
                        error = "the Hugging Face file address is missing the file path.";
                        return false;
                    }

                    revision = segments[3];
                    filePath = string.Join("/", segments.Skip(4));
                }
                else
                {
                    error = "paste a Hugging Face model page, tree page, or file page.";
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(revision))
            {
                revision = "main";
            }

            address = new HuggingFaceDownloadAddress
            {
                RepoId = repoId,
                Revision = revision,
                FolderPath = folderPath,
                FilePath = filePath,
            };
            error = string.Empty;
            return true;
        }

        private async Task DownloadHuggingFaceAddressAsync(HuggingFaceDownloadAddress address)
        {
            _modelCatalogSource = ModelCatalogSource.HuggingFace;
            txtRepoId.Text = address.RepoId;
            txtRevision.Text = address.Revision;
            lstHfRepos.Items.Clear();
            lstHfFiles.Items.Clear();

            _downloadCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _downloadCancellation.Token;
            SetDownloadControlsEnabled(isDownloading: true);
            ClearDownloadProgress();

            try
            {
                List<string> files;
                if (!string.IsNullOrWhiteSpace(address.FilePath))
                {
                    files = new List<string> { address.FilePath };
                }
                else
                {
                    AppendOutput($"Downloads: reading Hugging Face file list for {address.RepoId}...");
                    List<ModelFileListItem> fileItems = await GetHuggingFaceModelFileItemsAsync(
                        address.RepoId,
                        address.Revision,
                        cancellationToken);
                    PopulateModelFileList(fileItems);
                    files = fileItems
                        .Where(item => !item.IsFolder)
                        .Select(item => item.Path)
                        .Where(file => string.IsNullOrWhiteSpace(address.FolderPath) || IsInModelFolder(file, address.FolderPath))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                if (files.Count == 0)
                {
                    AppendOutput("Downloads: no downloadable files were found at that Hugging Face address.");
                    return;
                }

                string destinationFolder = BuildModelDownloadDirectory(address.RepoId, address.Revision);
                AppendOutput($"Downloads: downloading {files.Count} file(s) from {address.RepoId}...");
                AppendOutput($"Downloads: destination {destinationFolder}");

                for (int index = 0; index < files.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string progressLabel = $"{index + 1}/{files.Count} {files[index]}";
                    DownloadFileResult result = await DownloadModelFileAsync(
                        address.RepoId,
                        files[index],
                        address.Revision,
                        progressLabel,
                        cancellationToken);
                    AppendOutput(result.Skipped
                        ? $"Downloads: skipped {files[index]}; existing file is already complete."
                        : $"Downloads: downloaded {files[index]} to {result.TargetPath}");
                }

                txtInferModel.Text = Path.Combine(GetModelRoot(), GetHuggingFaceModelFolderName(address.RepoId));
                RefreshConvertModelSelection();
                ClearDownloadProgress();
                AppendOutput(files.Count == 1 ? "Downloads: file download complete." : "Downloads: model download complete.");
            }
            catch (OperationCanceledException)
            {
                ClearDownloadProgress();
                AppendOutput("Downloads: download canceled.");
            }
            catch (Exception ex)
            {
                ClearDownloadProgress();
                AppendOutput($"Downloads: download failed ({ShortError(ex)})");
            }
            finally
            {
                _downloadCancellation.Dispose();
                _downloadCancellation = null;
                SetDownloadControlsEnabled(isDownloading: false);
            }
        }

        private async Task DownloadDirectWebFileAsync(Uri uri)
        {
            string fileName = SanitizePathSegment(Uri.UnescapeDataString(Path.GetFileName(uri.LocalPath)));
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.', StringComparison.Ordinal))
            {
                AppendOutput("Downloads: this looks like a web page. Use a Hugging Face model address or a direct file address.");
                return;
            }

            _downloadCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _downloadCancellation.Token;
            SetDownloadControlsEnabled(isDownloading: true);
            ClearDownloadProgress();

            try
            {
                string targetPath = BuildDirectWebFileDownloadPath(uri, fileName);
                ValidateDownloadDestination(targetPath);
                string temporaryPath = Path.Combine(
                    Path.GetDirectoryName(targetPath)!,
                    $".{Path.GetFileNameWithoutExtension(targetPath)}.{Guid.NewGuid():N}.partial{Path.GetExtension(targetPath)}");

                try
                {
                    AppendOutput($"Downloads: downloading direct file {uri.AbsoluteUri}");
                    AppendOutput($"Downloads: destination {targetPath}");
                    using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(
                        uri,
                        System.Net.Http.HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);
                    response.EnsureSuccessStatusCode();
                    long? totalBytes = response.Content.Headers.ContentLength;

                    if (IsExistingFileSameSize(targetPath, totalBytes))
                    {
                        UpdateDownloadProgress(fileName, totalBytes!.Value, totalBytes, TimeSpan.Zero);
                        txtInferModel.Text = Path.GetDirectoryName(Path.GetDirectoryName(targetPath) ?? targetPath) ?? targetPath;
                        RefreshConvertModelSelection();
                        ClearDownloadProgress();
                        AppendOutput($"Downloads: skipped {fileName}; existing file is already complete.");
                        return;
                    }

                    string? targetDirectory = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrWhiteSpace(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                        ValidateDownloadDestination(targetPath);
                    }

                    long downloadedBytes = 0;
                    System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    await using (Stream input = await response.Content.ReadAsStreamAsync(cancellationToken))
                    await using (FileStream output = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                    {
                        byte[] buffer = new byte[81920];
                        int bytesRead;
                        DateTime lastProgressUpdate = DateTime.MinValue;
                        while ((bytesRead = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                        {
                            await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                            downloadedBytes += bytesRead;

                            if (DateTime.UtcNow - lastProgressUpdate >= TimeSpan.FromMilliseconds(250))
                            {
                                UpdateDownloadProgress(fileName, downloadedBytes, totalBytes, stopwatch.Elapsed);
                                lastProgressUpdate = DateTime.UtcNow;
                            }
                        }

                        await output.FlushAsync(cancellationToken);
                    }

                    if (totalBytes.HasValue && downloadedBytes != totalBytes.Value)
                    {
                        throw new InvalidDataException($"Download ended at {downloadedBytes:N0} bytes; the server announced {totalBytes.Value:N0} bytes.");
                    }

                    AppendDownloadedFileValidation(temporaryPath);
                    File.Move(temporaryPath, targetPath, overwrite: true);
                    UpdateDownloadProgress(fileName, downloadedBytes, totalBytes, stopwatch.Elapsed);
                    txtInferModel.Text = Path.GetDirectoryName(Path.GetDirectoryName(targetPath) ?? targetPath) ?? targetPath;
                    RefreshConvertModelSelection();
                    ClearDownloadProgress();
                    AppendOutput($"Downloads: file download complete: {targetPath}");
                }
                catch
                {
                    if (File.Exists(temporaryPath))
                    {
                        File.Delete(temporaryPath);
                    }

                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                ClearDownloadProgress();
                AppendOutput("Downloads: download canceled.");
            }
            catch (Exception ex)
            {
                ClearDownloadProgress();
                AppendOutput($"Downloads: direct download failed ({ShortError(ex)})");
            }
            finally
            {
                _downloadCancellation.Dispose();
                _downloadCancellation = null;
                SetDownloadControlsEnabled(isDownloading: false);
            }
        }

        private async Task DownloadFromListedModelFilesAsync()
        {
            if (_downloadCancellation is not null)
            {
                AppendOutput("Models: a download is already running.");
                return;
            }

            string repoId = txtRepoId.Text.Trim();
            string revision = GetRemoteRevision();

            if (string.IsNullOrWhiteSpace(repoId))
            {
                AppendOutput("Models: select a model in the top list before downloading.");
                return;
            }

            if (_modelCatalogSource == ModelCatalogSource.Ollama)
            {
                await DownloadSelectedOllamaModelAsync(repoId, revision);
                return;
            }

            if (lstHfFiles.Items.Count == 0)
            {
                await LoadSelectedModelFilesAsync();
            }

            ModelFileListItem? selectedItem = GetSelectedModelFileItem();
            List<string> files = selectedItem is null
                ? GetListedModelFilePaths()
                : GetDownloadPathsForSelectedLowerItem(selectedItem);

            if (files.Count == 0)
            {
                AppendOutput("Models: no downloadable files are listed for the selected model.");
                return;
            }

            string destinationFolder = BuildModelDownloadDirectory(repoId, revision);
            _downloadCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _downloadCancellation.Token;
            SetDownloadControlsEnabled(isDownloading: true);
            ClearDownloadProgress();

            try
            {
                if (selectedItem is null)
                {
                    AppendOutput($"Models: no lower file selected; downloading all {files.Count} listed files for {repoId}...");
                }
                else if (files.Count == 1)
                {
                    AppendOutput($"Models: downloading selected file {files[0]}...");
                }
                else
                {
                    AppendOutput($"Models: downloading {files.Count} files from selected folder {selectedItem.Path}...");
                }

                AppendOutput($"Models: destination {destinationFolder}");
                for (int index = 0; index < files.Count; index++)
                {
                    string progressLabel = $"{index + 1}/{files.Count} {files[index]}";
                    DownloadFileResult result = await DownloadModelFileAsync(repoId, files[index], revision, progressLabel, cancellationToken);
                    AppendOutput(result.Skipped
                        ? $"Models: skipped {files[index]}; existing file is already complete."
                        : $"Models: downloaded {files[index]} to {result.TargetPath}");
                }

                AppendOutput(files.Count == 1 ? "Models: file download complete." : "Models: model download complete.");
                RefreshConvertModelSelection();
                ClearDownloadProgress();
            }
            catch (OperationCanceledException)
            {
                ClearDownloadProgress();
                AppendOutput("Models: download canceled.");
            }
            catch (Exception ex)
            {
                ClearDownloadProgress();
                AppendOutput($"Models: download failed ({ShortError(ex)})");
            }
            finally
            {
                _downloadCancellation.Dispose();
                _downloadCancellation = null;
                SetDownloadControlsEnabled(isDownloading: false);
            }
        }

        private async Task DownloadSelectedOllamaModelAsync(string requestedModel, string requestedTag)
        {
            string selectedTag = GetSelectedOllamaTag(requestedTag);
            SplitOllamaModelNameAndTag(requestedModel, selectedTag, out string modelName, out string tag);
            if (string.IsNullOrWhiteSpace(modelName))
            {
                AppendOutput("Models: select or enter an Ollama model first.");
                return;
            }

            string destinationFolder = BuildModelDownloadDirectory(modelName, tag);
            _downloadCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _downloadCancellation.Token;
            SetDownloadControlsEnabled(isDownloading: true);
            ClearDownloadProgress();

            try
            {
                Directory.CreateDirectory(destinationFolder);
                AppendOutput($"Models: downloading Ollama model {modelName}:{tag}...");
                AppendOutput($"Models: destination {destinationFolder}");

                string manifestUrl = BuildOllamaRegistryManifestUrl(modelName, tag);
                string manifestJson = await DownloadTextAsync(manifestUrl, cancellationToken);
                string manifestPath = Path.Combine(destinationFolder, "ollama.manifest.json");
                WriteTextAtomically(manifestPath, NormalizeTextFileLineEndings(manifestJson));
                AppendOutput($"Models: downloaded manifest to {manifestPath}");

                using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(manifestJson);
                List<OllamaRegistryLayer> layers = ReadOllamaRegistryLayers(document.RootElement);
                OllamaRegistryLayer? config = ReadOllamaRegistryConfig(document.RootElement);

                int itemCount = layers.Count + (config is null ? 0 : 1);
                int itemIndex = 0;
                if (config is not null)
                {
                    itemIndex++;
                    string targetPath = Path.Combine(destinationFolder, "ollama.config.json");
                    await DownloadOllamaRegistryBlobAsync(modelName, config.Digest, targetPath, $"{itemIndex}/{itemCount} config", cancellationToken);
                }

                for (int index = 0; index < layers.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    itemIndex++;
                    OllamaRegistryLayer layer = layers[index];
                    string targetPath = BuildOllamaLayerTargetPath(destinationFolder, index + 1, layer.MediaType, layer.Digest);
                    await DownloadOllamaRegistryBlobAsync(modelName, layer.Digest, targetPath, $"{itemIndex}/{itemCount} {Path.GetFileName(targetPath)}", cancellationToken);
                }

                AppendOutput($"Models: Ollama model download complete: {modelName}:{tag}");
                txtInferModel.Text = Path.Combine(GetModelRoot(), GetHuggingFaceModelFolderName(modelName));
                RefreshConvertModelSelection($"model:{GetHuggingFaceModelFolderName(modelName)}");
                ClearDownloadProgress();
            }
            catch (OperationCanceledException)
            {
                ClearDownloadProgress();
                AppendOutput("Models: Ollama download canceled.");
            }
            catch (Exception ex)
            {
                ClearDownloadProgress();
                AppendOutput($"Models: Ollama download failed ({ShortError(ex)})");
            }
            finally
            {
                _downloadCancellation.Dispose();
                _downloadCancellation = null;
                SetDownloadControlsEnabled(isDownloading: false);
            }
        }

        private sealed record OllamaRegistryLayer(string Digest, string MediaType, long Size);

        private static OllamaRegistryLayer? ReadOllamaRegistryConfig(System.Text.Json.JsonElement root)
        {
            if (!root.TryGetProperty("config", out System.Text.Json.JsonElement config)
                || config.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return null;
            }

            string digest = TryReadString(config, "digest");
            if (string.IsNullOrWhiteSpace(digest))
            {
                return null;
            }

            return new OllamaRegistryLayer(
                digest,
                TryReadString(config, "mediaType"),
                TryReadLong(config, "size") ?? 0);
        }

        private static List<OllamaRegistryLayer> ReadOllamaRegistryLayers(System.Text.Json.JsonElement root)
        {
            List<OllamaRegistryLayer> layers = new();
            if (!root.TryGetProperty("layers", out System.Text.Json.JsonElement layerArray)
                || layerArray.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                return layers;
            }

            foreach (System.Text.Json.JsonElement layer in layerArray.EnumerateArray())
            {
                if (layer.ValueKind != System.Text.Json.JsonValueKind.Object)
                {
                    continue;
                }

                string digest = TryReadString(layer, "digest");
                if (string.IsNullOrWhiteSpace(digest))
                {
                    continue;
                }

                layers.Add(new OllamaRegistryLayer(
                    digest,
                    TryReadString(layer, "mediaType"),
                    TryReadLong(layer, "size") ?? 0));
            }

            return layers;
        }

        private async Task<string> DownloadTextAsync(string url, CancellationToken cancellationToken)
        {
            using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private async Task DownloadOllamaRegistryBlobAsync(
            string modelName,
            string digest,
            string targetPath,
            string progressLabel,
            CancellationToken cancellationToken)
        {
            ValidateOllamaDigest(digest);
            ValidateDownloadDestination(targetPath);
            string temporaryPath = Path.Combine(
                Path.GetDirectoryName(targetPath)!,
                $".{Path.GetFileNameWithoutExtension(targetPath)}.{Guid.NewGuid():N}.partial{Path.GetExtension(targetPath)}");

            try
            {
                string blobUrl = BuildOllamaRegistryBlobUrl(modelName, digest);
                using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(
                    blobUrl,
                    System.Net.Http.HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                response.EnsureSuccessStatusCode();
                long? totalBytes = response.Content.Headers.ContentLength;

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                ValidateDownloadDestination(targetPath);

                if (File.Exists(targetPath) && FileMatchesOllamaDigest(targetPath, digest))
                {
                    UpdateDownloadProgress(progressLabel, totalBytes ?? new FileInfo(targetPath).Length, totalBytes, TimeSpan.Zero);
                    AppendOutput($"Models: skipped {Path.GetFileName(targetPath)}; existing blob is already complete.");
                    return;
                }

                long downloadedBytes = 0;
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await using (Stream input = await response.Content.ReadAsStreamAsync(cancellationToken))
                await using (FileStream output = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    byte[] buffer = new byte[81920];
                    int bytesRead;
                    DateTime lastProgressUpdate = DateTime.MinValue;
                    while ((bytesRead = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                    {
                        await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        downloadedBytes += bytesRead;

                        if (DateTime.UtcNow - lastProgressUpdate >= TimeSpan.FromMilliseconds(250))
                        {
                            UpdateDownloadProgress(progressLabel, downloadedBytes, totalBytes, stopwatch.Elapsed);
                            lastProgressUpdate = DateTime.UtcNow;
                        }
                    }

                    await output.FlushAsync(cancellationToken);
                }

                if (totalBytes.HasValue && downloadedBytes != totalBytes.Value)
                {
                    throw new InvalidDataException($"Download ended at {downloadedBytes:N0} bytes; the registry announced {totalBytes.Value:N0} bytes.");
                }

                if (!FileMatchesOllamaDigest(temporaryPath, digest))
                {
                    throw new InvalidDataException($"Downloaded blob did not match {digest}.");
                }

                File.Move(temporaryPath, targetPath, overwrite: true);
                UpdateDownloadProgress(progressLabel, downloadedBytes, totalBytes, stopwatch.Elapsed);
                AppendOutput($"Models: downloaded {Path.GetFileName(targetPath)}");
            }
            catch
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }

                throw;
            }
        }

        private string GetSelectedOllamaTag(string fallbackTag)
        {
            ModelFileListItem? selectedItem = GetSelectedModelFileItem();
            string tag = selectedItem is { IsFolder: false }
                ? selectedItem.Path.Trim()
                : fallbackTag.Trim();
            return string.IsNullOrWhiteSpace(tag) || tag.Equals("main", StringComparison.OrdinalIgnoreCase)
                ? "latest"
                : tag.TrimStart(':');
        }

        private static void SplitOllamaModelNameAndTag(string requestedModel, string fallbackTag, out string modelName, out string tag)
        {
            string trimmed = requestedModel.Trim();
            int tagSeparator = trimmed.LastIndexOf(':');
            int lastSlash = trimmed.LastIndexOfAny(new[] { '/', '\\' });
            if (tagSeparator > lastSlash)
            {
                modelName = trimmed[..tagSeparator];
                tag = trimmed[(tagSeparator + 1)..];
            }
            else
            {
                modelName = trimmed;
                tag = fallbackTag;
            }

            tag = string.IsNullOrWhiteSpace(tag) ? "latest" : tag.Trim();
        }

        private static string BuildOllamaRegistryManifestUrl(string modelName, string tag)
        {
            return $"https://registry.ollama.ai/v2/{BuildOllamaRegistryModelPath(modelName)}/manifests/{Uri.EscapeDataString(tag)}";
        }

        private static string BuildOllamaRegistryBlobUrl(string modelName, string digest)
        {
            return $"https://registry.ollama.ai/v2/{BuildOllamaRegistryModelPath(modelName)}/blobs/{digest}";
        }

        private static string BuildOllamaRegistryModelPath(string modelName)
        {
            string normalized = modelName.Trim().Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException("The Ollama model name is empty.");
            }

            string registryPath = normalized.Contains('/', StringComparison.Ordinal)
                ? normalized
                : $"library/{normalized}";
            return EscapeUrlPath(registryPath);
        }

        private static string BuildOllamaLayerTargetPath(string destinationFolder, int index, string mediaType, string digest)
        {
            string kind = mediaType
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault() ?? "layer";
            string digestName = digest.Replace(':', '-');
            string fileName = kind switch
            {
                "model" => "model.gguf",
                "adapter" => $"adapter-{index}.gguf",
                "projector" => $"projector-{index}.gguf",
                "template" => $"ollama-template-{index}.txt",
                "license" => $"ollama-license-{index}.txt",
                "params" => $"ollama-params-{index}.txt",
                "system" => $"ollama-system-{index}.txt",
                _ => $"ollama-layer-{index}-{digestName}.blob",
            };

            string targetPath = Path.Combine(destinationFolder, fileName);
            if (kind == "model" && File.Exists(targetPath))
            {
                targetPath = Path.Combine(destinationFolder, $"model-{index}.gguf");
            }

            return targetPath;
        }

        private static void ValidateOllamaDigest(string digest)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                digest,
                "^sha256:[0-9a-f]{64}$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(100)))
            {
                throw new InvalidDataException($"Ollama registry returned an unsupported digest: {digest}");
            }
        }

        private static bool FileMatchesOllamaDigest(string path, string digest)
        {
            string expected = digest["sha256:".Length..].ToLowerInvariant();
            using FileStream stream = File.OpenRead(path);
            byte[] hash = System.Security.Cryptography.SHA256.HashData(stream);
            string actual = System.Convert.ToHexString(hash).ToLowerInvariant();
            return actual.Equals(expected, StringComparison.OrdinalIgnoreCase);
        }

        private string GetRemoteRevision()
        {
            string revision = txtRevision.Text.Trim();
            return string.IsNullOrWhiteSpace(revision)
                || revision.Equals("downloaded", StringComparison.OrdinalIgnoreCase)
                || revision.Equals("local", StringComparison.OrdinalIgnoreCase)
                    ? "main"
                    : revision;
        }

        private async Task PrepareSelectedModelAsync()
        {
            string repoId = txtRepoId.Text.Trim();
            string revision = string.IsNullOrWhiteSpace(txtRevision.Text) ? "main" : txtRevision.Text.Trim();
            ConvertModelSelectionEntry? convertEntry = GetSelectedConvertModelEntry();

            if (string.IsNullOrWhiteSpace(repoId) && convertEntry is null)
            {
                AppendOutput("Prepare: select a model in the top list first.");
                return;
            }

            if (lstHfFiles.Items.Count == 0 && convertEntry is null)
            {
                await LoadSelectedModelFilesAsync();
            }

            string modelDirectory = convertEntry?.ModelDirectory ?? BuildModelDownloadDirectory(repoId, revision);
            if (!Directory.Exists(modelDirectory))
            {
                AppendOutput($"Prepare: download folder does not exist yet: {modelDirectory}");
                return;
            }

            string oldStatus = lblModelsStatus.Text;
            btnConvertToIr.Enabled = false;
            lblModelsStatus.Text = "Preparing downloaded model files...";

            try
            {
                AppendOutput("Prepare: checking downloaded model files.");
                AppendOutput("Prepare: using bundled OpenVINO runtime only.");

                string manifestPath = PrepareDownloadedModel(convertEntry?.Id ?? repoId, convertEntry?.Revision ?? revision, modelDirectory);
                string? readableModelPath = ResolveSelectedOpenVinoModelPath(logMissing: false);
                if (readableModelPath is not null)
                {
                    AppendOutput($"Prepare: validating OpenVINO can read {readableModelPath}...");
                    ModelCompileResult metadata = await Task.Run(() => LoadOpenVinoModelMetadata(readableModelPath));
                    AppendOutput($"Prepare: readable model `{metadata.FriendlyName}`.");
                    AppendOutput($"Prepare: inputs {metadata.Inputs.Count}, outputs {metadata.Outputs.Count}.");
                }
                else
                {
                    AppendOutput("Prepare: no OpenVINO-readable model file found. GGUF files belong in OVMS Pull or the OpenVINO GenAI runtime path.");
                }

                RefreshConvertModelSelection(convertEntry?.Id);
                lblIrPlaceholder.Text = $"Prepared: {Path.GetFileName(manifestPath)}";
                AppendOutput($"Prepare: manifest written to {manifestPath}");
            }
            catch (Exception ex)
            {
                AppendOutput("Prepare failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"  {line}");
                }
            }
            finally
            {
                lblModelsStatus.Text = oldStatus;
                UpdateConvertActionButtons();
            }
        }

        private string PrepareDownloadedModel(string repoId, string revision, string modelDirectory)
        {
            string[] files = Directory
                .EnumerateFiles(modelDirectory, "*", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (files.Length == 0)
            {
                throw new InvalidOperationException("The model download folder is empty.");
            }

            List<PreparedIrModel> irModels = files
                .Where(path => path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .Select(xml => new PreparedIrModel
                {
                    Xml = Path.GetRelativePath(modelDirectory, xml),
                    Bin = File.Exists(Path.ChangeExtension(xml, ".bin"))
                        ? Path.GetRelativePath(modelDirectory, Path.ChangeExtension(xml, ".bin")!)
                        : string.Empty,
                })
                .ToList();

            string[] missingWeights = irModels
                .Where(model => !model.HasWeights)
                .Select(model => model.Xml)
                .ToArray();

            string[] nonIrModelFiles = files
                .Where(path => IsLikelyModelSourceFile(path) && !path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
                .Select(path => Path.GetRelativePath(modelDirectory, path))
                .ToArray();

            string[] readableModelFiles = files
                .Where(IsOpenVinoReadableModelFile)
                .Select(path => Path.GetRelativePath(modelDirectory, path))
                .ToArray();

            string status = irModels.Count > 0 && missingWeights.Length == 0
                ? "ready_for_server_compile"
                : irModels.Count > 0
                    ? "ir_missing_some_weights"
                    : readableModelFiles.Length > 0
                        ? "openvino_readable_model"
                        : "downloaded_conversion_needed";

            object manifest = new
            {
                preparedAtUtc = DateTime.UtcNow,
                source = repoId,
                revision,
                modelDirectory,
                status,
                intendedTargets = GetCompileTargetChoices().ToArray(),
                fileCount = files.Length,
                files = files.Select(path => Path.GetRelativePath(modelDirectory, path)).ToArray(),
                openVinoIrModels = irModels,
                openVinoReadableModels = readableModelFiles,
                missingWeights,
                nonIrModelFiles,
            };

            string manifestPath = Path.Combine(modelDirectory, "model.prepare.json");
            System.Text.Json.JsonSerializerOptions options = new() { WriteIndented = true };
            WriteTextAtomically(manifestPath, System.Text.Json.JsonSerializer.Serialize(manifest, options));

            AppendOutput($"Prepare: {files.Length} file(s), {irModels.Count} IR model(s), {readableModelFiles.Length} OpenVINO-readable model file(s), {nonIrModelFiles.Length} non-IR model file(s).");
            AppendOutput($"Prepare: targets {string.Join(", ", GetCompileTargetChoices())}.");
            if (missingWeights.Length > 0)
            {
                AppendOutput($"Prepare: warning - {missingWeights.Length} XML file(s) do not have matching BIN weights.");
            }

            if (readableModelFiles.Length == 0 && nonIrModelFiles.Length > 0)
            {
                AppendOutput("Prepare: conversion to OpenVINO IR is still needed before compile.");
            }

            return manifestPath;
        }

        private static void CopyDirectory(
            string sourceFolder,
            string destinationFolder,
            CancellationToken cancellationToken = default,
            IProgress<string>? progress = null)
        {
            int copiedFiles = 0;
            foreach (string directory in Directory.EnumerateDirectories(sourceFolder, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string relative = Path.GetRelativePath(sourceFolder, directory);
                Directory.CreateDirectory(Path.Combine(destinationFolder, relative));
            }

            foreach (string file in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string relative = Path.GetRelativePath(sourceFolder, file);
                string destination = Path.Combine(destinationFolder, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(file, destination, overwrite: true);
                copiedFiles++;
                if (copiedFiles == 1 || copiedFiles % 25 == 0)
                {
                    progress?.Report($"Copying files... {copiedFiles:N0} files copied");
                }
            }
        }

        private static bool IsLikelyModelSourceFile(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            return extension is ".xml" or ".bin" or ".onnx" or ".pt" or ".pth" or ".safetensors" or ".pb" or ".caffemodel" or ".prototxt" or ".tflite" or ".pdmodel" or ".gguf" or ".blob";
        }

        private static bool IsOpenVinoReadableModelFile(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            return extension is ".xml" or ".onnx" or ".pb" or ".caffemodel" or ".tflite" or ".pdmodel";
        }

        private static bool IsGgufModelFile(string path)
        {
            return Path.GetExtension(path).Equals(".gguf", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<DownloadFileResult> DownloadModelFileAsync(string repoId, string fileName, string revision, string progressLabel, CancellationToken cancellationToken)
        {
            string downloadUrl = BuildModelFileDownloadUrl(repoId, fileName, revision);
            string targetPath = BuildModelFileDownloadPath(repoId, fileName, revision);
            ValidateDownloadDestination(targetPath);
            string temporaryPath = Path.Combine(
                Path.GetDirectoryName(targetPath)!,
                $".{Path.GetFileNameWithoutExtension(targetPath)}.{Guid.NewGuid():N}.partial{Path.GetExtension(targetPath)}");

            try
            {
                using System.Net.Http.HttpResponseMessage response = await ModelCatalogClient.GetAsync(
                    downloadUrl,
                    System.Net.Http.HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                response.EnsureSuccessStatusCode();
                long? totalBytes = response.Content.Headers.ContentLength;

                string? targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrWhiteSpace(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                    ValidateDownloadDestination(targetPath);
                }

                if (IsExistingFileSameSize(targetPath, totalBytes))
                {
                    UpdateDownloadProgress(progressLabel, totalBytes!.Value, totalBytes, TimeSpan.Zero);
                    return new DownloadFileResult(targetPath, Skipped: true);
                }

                long downloadedBytes = 0;
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await using (Stream input = await response.Content.ReadAsStreamAsync(cancellationToken))
                await using (FileStream output = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    byte[] buffer = new byte[81920];
                    int bytesRead;
                    DateTime lastProgressUpdate = DateTime.MinValue;
                    while ((bytesRead = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                    {
                        await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        downloadedBytes += bytesRead;

                        if (DateTime.UtcNow - lastProgressUpdate >= TimeSpan.FromMilliseconds(250))
                        {
                            UpdateDownloadProgress(progressLabel, downloadedBytes, totalBytes, stopwatch.Elapsed);
                            lastProgressUpdate = DateTime.UtcNow;
                        }
                    }

                    await output.FlushAsync(cancellationToken);
                }

                if (totalBytes.HasValue && downloadedBytes != totalBytes.Value)
                {
                    throw new InvalidDataException($"Download ended at {downloadedBytes:N0} bytes; the server announced {totalBytes.Value:N0} bytes.");
                }

                AppendDownloadedFileValidation(temporaryPath);
                File.Move(temporaryPath, targetPath, overwrite: true);
                UpdateDownloadProgress(progressLabel, downloadedBytes, totalBytes, stopwatch.Elapsed);
                return new DownloadFileResult(targetPath, Skipped: false);
            }
            catch (Exception downloadException)
            {
                try
                {
                    if (File.Exists(temporaryPath))
                    {
                        File.Delete(temporaryPath);
                    }
                }
                catch (Exception cleanupException)
                {
                    throw new AggregateException(
                        $"The download failed and its temporary file could not be removed: {temporaryPath}",
                        downloadException,
                        cleanupException);
                }

                throw;
            }
        }

        private static bool IsExistingFileSameSize(string targetPath, long? expectedBytes)
        {
            return expectedBytes.HasValue
                && expectedBytes.Value >= 0
                && File.Exists(targetPath)
                && new FileInfo(targetPath).Length == expectedBytes.Value;
        }

        private void AppendDownloadedFileValidation(string targetPath)
        {
            FileInfo file = new(targetPath);
            string extension = file.Extension.ToLowerInvariant();

            AppendOutput($"Models: downloaded file size {FormatBytes((ulong)Math.Max(0, file.Length))}");

            if (extension == ".gguf")
            {
                bool hasGgufHeader = FileStartsWithAscii(targetPath, "GGUF");
                if (!hasGgufHeader)
                {
                    throw new InvalidDataException("The downloaded .gguf file does not start with a GGUF header. The server may have returned a pointer, error page, or incomplete file.");
                }

                if (file.Length < 100L * 1024L * 1024L)
                {
                    AppendOutput("Models: warning - .gguf file is under 100 MB. Real LLM GGUF model weights are usually much larger.");
                }

                AppendOutput("Models: note - GGUF files use OpenVINO GenAI LLMPipeline, not the standard OpenVINO Core IR path.");
                AppendOutput("Models: after download, use Workstation to choose the runtime target and Inference to test supported GGUF models.");
            }
        }

        private static bool FileStartsWithAscii(string path, string expectedText)
        {
            byte[] expected = System.Text.Encoding.ASCII.GetBytes(expectedText);
            Span<byte> buffer = stackalloc byte[expected.Length];

            using FileStream stream = File.OpenRead(path);
            return stream.Read(buffer) == expected.Length && buffer.SequenceEqual(expected);
        }

        private List<string> GetDownloadPathsForSelectedLowerItem(ModelFileListItem selectedItem)
        {
            if (!selectedItem.IsFolder)
            {
                return new List<string> { selectedItem.Path };
            }

            return GetListedModelFilePaths()
                .Where(file => IsInModelFolder(file, selectedItem.Path))
                .ToList();
        }

        private List<string> GetListedModelFilePaths()
        {
            return lstHfFiles.Items
                .Cast<object>()
                .Select(GetModelFileListItem)
                .OfType<ModelFileListItem>()
                .Where(item => !item.IsFolder)
                .Select(item => item.Path)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private ModelFileListItem? GetSelectedModelFileItem()
        {
            return lstHfFiles.SelectedItems.Count == 0
                ? null
                : GetModelFileListItem(lstHfFiles.SelectedItems[0]);
        }

        private static ModelFileListItem? GetModelFileListItem(object? item)
        {
            return item switch
            {
                ListViewItem row when row.Tag is ModelFileListItem fileItem => fileItem,
                ModelFileListItem fileItem => fileItem,
                string text when !string.IsNullOrWhiteSpace(text) => new ModelFileListItem { Path = text, IsFolder = false },
                _ => null,
            };
        }

        private void SetDownloadControlsEnabled(bool isDownloading)
        {
            btnLoadFiles.Enabled = !isDownloading;
            btnCancelDownload.Enabled = isDownloading;
            btnClearFileSelection.Enabled = !isDownloading;
            btnCopyDownloadUrl.Enabled = !isDownloading;
        }

        private void ClearDownloadProgress()
        {
            progressDownload.Value = 0;
            lblDownloadStatus.Text = string.Empty;
        }

        private void UpdateDownloadProgress(string label, long downloadedBytes, long? totalBytes, TimeSpan elapsed)
        {
            if (totalBytes is > 0)
            {
                int percent = (int)Math.Clamp(downloadedBytes * 100 / totalBytes.Value, 0, 100);
                progressDownload.Value = percent;

                double bytesPerSecond = elapsed.TotalSeconds <= 0 ? 0 : downloadedBytes / elapsed.TotalSeconds;
                string eta = bytesPerSecond <= 0
                    ? "estimating"
                    : FormatDuration(TimeSpan.FromSeconds(Math.Max(0, (totalBytes.Value - downloadedBytes) / bytesPerSecond)));

                lblDownloadStatus.Text = $"{percent}%  {FormatBytes((ulong)downloadedBytes)} / {FormatBytes((ulong)totalBytes.Value)}  ETA {eta}";
                return;
            }

            lblDownloadStatus.Text = $"{FormatBytes((ulong)Math.Max(0, downloadedBytes))} downloaded";
            if (!string.IsNullOrWhiteSpace(label))
            {
                lblDownloadStatus.Text = $"{label}: {lblDownloadStatus.Text}";
            }
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }

            if (duration.TotalMinutes >= 1)
            {
                return $"{duration.Minutes}m {duration.Seconds}s";
            }

            return $"{Math.Max(0, duration.Seconds)}s";
        }

        private static string GetModelFileFolder(string fileName)
        {
            int lastSlash = fileName.LastIndexOfAny(new[] { '/', '\\' });
            return lastSlash < 0 ? string.Empty : fileName[..lastSlash].Replace('\\', '/');
        }

        private static IEnumerable<string> GetModelFileFolderChain(string fileName)
        {
            string folder = GetModelFileFolder(fileName);
            while (!string.IsNullOrWhiteSpace(folder))
            {
                yield return folder;
                folder = GetModelFileFolder(folder);
            }
        }

        private static bool IsInModelFolder(string fileName, string folder)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            string normalizedFile = fileName.Replace('\\', '/');
            return string.IsNullOrEmpty(folder)
                ? !normalizedFile.Contains('/')
                : normalizedFile.StartsWith($"{folder}/", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildModelFileDownloadUrl(string repoId, string fileName, string revision)
        {
            if (repoId.StartsWith("open_model_zoo/", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = repoId.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException($"Cannot parse Open Model Zoo path {repoId}.");
                }

                return $"https://raw.githubusercontent.com/openvinotoolkit/open_model_zoo/master/models/{Uri.EscapeDataString(parts[1])}/{Uri.EscapeDataString(parts[2])}/{EscapeUrlPath(fileName)}";
            }

            return $"https://huggingface.co/{EscapeUrlPath(repoId)}/resolve/{Uri.EscapeDataString(revision)}/{EscapeUrlPath(fileName)}";
        }

        private static string BuildModelFileDownloadPath(string repoId, string fileName, string revision)
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

        private static string BuildModelDownloadDirectory(string repoId, string revision)
        {
            string modelFolderName = GetDownloadModelFolderName(repoId);
            return Path.GetFullPath(Path.Combine(GetModelRoot(), modelFolderName, "main"));
        }

        private static string GetDownloadSafetyRoot(string repoId)
        {
            return GetModelRoot();
        }

        private static string GetHuggingFaceModelFolderName(string repoId)
        {
            return GetDownloadModelFolderName(repoId);
        }

        private static string GetDownloadModelFolderName(string modelId)
        {
            string modelName = modelId
                .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault() ?? modelId;
            return SanitizePathSegment(modelName);
        }

        private static string BuildDirectWebFileDownloadPath(Uri uri, string fileName)
        {
            string modelName = BuildDirectWebFileModelFolderName(uri, fileName);
            return Path.GetFullPath(Path.Combine(GetModelRoot(), modelName, "main", SanitizePathSegment(fileName)));
        }

        private static string BuildDirectWebFileModelFolderName(Uri uri, string fileName)
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

        private static bool IsGenericDirectModelFileStem(string fileStem)
        {
            return fileStem.Equals("model", StringComparison.OrdinalIgnoreCase)
                || fileStem.Equals("openvino_model", StringComparison.OrdinalIgnoreCase)
                || fileStem.Equals("pytorch_model", StringComparison.OrdinalIgnoreCase)
                || fileStem.Equals("tf_model", StringComparison.OrdinalIgnoreCase)
                || fileStem.Equals("weights", StringComparison.OrdinalIgnoreCase)
                || fileStem.Equals("config", StringComparison.OrdinalIgnoreCase)
                || fileStem.Equals("tokenizer", StringComparison.OrdinalIgnoreCase);
        }

        private static string EscapeUrlPath(string path)
        {
            return string.Join("/", path
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));
        }

        private static string SanitizePathSegment(string segment)
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

        private static void ValidateDownloadDestination(string targetPath)
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

        private static ModelCatalogItem CreateHuggingFaceCatalogItem(
            System.Text.Json.JsonElement model,
            string modelId,
            ModelCatalogSource source,
            string sourceName)
        {
            List<string> metadata = new();
            ReadStringArray(model, "tags", metadata);
            ReadSiblingFileNames(model, metadata);

            string quantization = DetectQuantization(metadata);

            return new ModelCatalogItem
            {
                Id = modelId,
                SourceKind = source,
                Source = sourceName,
                Task = TryReadString(model, "pipeline_tag"),
                Library = TryReadString(model, "library_name"),
                Downloads = TryReadInt(model, "downloads"),
                Likes = TryReadInt(model, "likes"),
                Quantization = string.IsNullOrWhiteSpace(quantization) ? string.Empty : $"quant: {quantization}",
            };
        }

        private static string TryReadString(System.Text.Json.JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
                && property.ValueKind == System.Text.Json.JsonValueKind.String
                    ? property.GetString() ?? string.Empty
                    : string.Empty;
        }

        private static int? TryReadInt(System.Text.Json.JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                System.Text.Json.JsonValueKind.Number when property.TryGetInt32(out int value) => value,
                _ => null,
            };
        }

        private static long? TryReadLong(System.Text.Json.JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                System.Text.Json.JsonValueKind.Number when property.TryGetInt64(out long value) => value,
                _ => null,
            };
        }

        private static long? TryReadNestedLong(System.Text.Json.JsonElement element, string parentName, string propertyName)
        {
            return element.TryGetProperty(parentName, out System.Text.Json.JsonElement parent)
                && parent.ValueKind == System.Text.Json.JsonValueKind.Object
                    ? TryReadLong(parent, propertyName)
                    : null;
        }

        private static string BuildFileNotes(string path, long? sizeBytes)
        {
            ModelFileKind kind = GetModelFileKind(path);
            if (kind == ModelFileKind.Gguf)
            {
                if (sizeBytes is null)
                {
                    return "GGUF size unknown";
                }

                if (sizeBytes < 100L * 1024L * 1024L)
                {
                    return "GGUF suspiciously small";
                }

                return "GGUF GenAI model";
            }

            if (kind == ModelFileKind.OpenVinoIr)
            {
                return "OpenVINO IR model";
            }

            if (kind == ModelFileKind.OpenVinoWeights)
            {
                return "OpenVINO weights";
            }

            if (kind == ModelFileKind.OpenVinoCoreReadable)
            {
                return Path.GetExtension(path).ToLowerInvariant() switch
                {
                    ".onnx" => "OpenVINO-readable ONNX",
                    ".caffemodel" => "OpenVINO-readable Caffe",
                    ".pb" or ".tflite" => "OpenVINO-readable TensorFlow",
                    ".pdmodel" => "OpenVINO-readable PaddlePaddle",
                    _ => "OpenVINO-readable model",
                };
            }

            return sizeBytes is null ? "size unknown" : string.Empty;
        }

        private static ModelFileKind GetModelFileKind(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".xml" => ModelFileKind.OpenVinoIr,
                ".blob" => ModelFileKind.OpenVinoCompiledBlob,
                ".bin" => ModelFileKind.OpenVinoWeights,
                ".onnx" or ".pb" or ".caffemodel" or ".tflite" or ".pdmodel" => ModelFileKind.OpenVinoCoreReadable,
                ".gguf" => ModelFileKind.Gguf,
                ".pt" or ".pth" => ModelFileKind.PyTorchSource,
                ".safetensors" or ".json" or ".model" or ".txt" or ".prototxt" => ModelFileKind.SupportFile,
                _ => ModelFileKind.Unknown,
            };
        }

        private static void ReadStringArray(System.Text.Json.JsonElement element, string propertyName, List<string> values)
        {
            if (!element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
                || property.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                return;
            }

            foreach (System.Text.Json.JsonElement item in property.EnumerateArray())
            {
                string? value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }
        }

        private static void ReadSiblingFileNames(System.Text.Json.JsonElement element, List<string> values)
        {
            if (!element.TryGetProperty("siblings", out System.Text.Json.JsonElement siblings)
                || siblings.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                return;
            }

            foreach (System.Text.Json.JsonElement sibling in siblings.EnumerateArray())
            {
                string fileName = TryReadString(sibling, "rfilename");
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    values.Add(fileName);
                }
            }
        }

        private static string DetectQuantization(IEnumerable<string> metadata)
        {
            string joined = string.Join(" ", metadata).ToLowerInvariant();
            string[] markers = { "int4", "int8", "uint8", "fp8", "fp16", "float16", "bf16", "bfloat16", "fp32", "float32", "q4", "q5", "q6", "q8", "gguf" };
            List<string> matches = new();

            foreach (string marker in markers)
            {
                if (joined.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(marker.ToUpperInvariant());
                }
            }

            return matches.Count == 0 ? string.Empty : string.Join(", ", matches.Distinct());
        }

        private void HandleDownloadFileSelectionChanged()
        {
            UpdateDownloadUrlPreview();

            ModelFileListItem? selectedItem = GetSelectedModelFileItem();
            if (selectedItem is null)
            {
                return;
            }

            AppendDownloadSelectionGuidance(selectedItem);
        }

        private void txtDownloadUrl_Enter(object sender, EventArgs e)
        {
            txtDownloadUrl.SelectAll();
        }

        private void btnDownloadHelp_Click(object? sender, EventArgs e)
        {
            AppendOutput(string.Empty);
            AppendOutput("Download Help - choosing a model format");
            AppendOutput("------------------------------------------------------------");
            AppendOutput(".xml + .bin: OpenVINO IR. The .xml describes the network; the matching .bin holds the weights. Choose this when you want the best OpenVINO path for Intel GPU targets, MULTI, model tests, and Convert.");
            AppendOutput(".blob: device-optimized OpenVINO compiled model. Choose this after Convert creates it for a specific target such as GPU or MULTI. It loads quickly, but it is tied to the selected device and OpenVINO runtime version.");
            AppendOutput(".onnx, .pb, .tflite, .caffemodel, .pdmodel: source formats OpenVINO Core can read. They are convenient downloads, but converting or exporting to IR/blob usually gives this app more predictable behavior.");
            AppendOutput(".gguf: single-file chat model format used by llama.cpp and Ollama-style LLMs. Choose it when you want a quick local text model and the architecture is supported by OpenVINO GenAI.");
            AppendOutput(".safetensors: common Hugging Face weight files. They are safer than old pickle-based PyTorch checkpoints, but this app does not run them directly. Download them with the full model folder when you plan to convert with Optimum/OpenVINO tools.");
            AppendOutput(".pt, .pth: PyTorch checkpoint files. They are common for research models, but usually need Python export/conversion before this self-contained app can run them.");
            AppendOutput(".bin by itself: usually weights only. For OpenVINO IR it must sit beside the matching .xml. For Hugging Face models it may be PyTorch weights and not directly runnable here.");
            AppendOutput(".json: configuration, tokenizer, generation settings, label maps, or metadata. Useful support files, but not a runnable model by themselves.");
            AppendOutput(".model: usually a tokenizer model, such as SentencePiece. Keep it with text models; it is not a runtime model by itself.");
            AppendOutput(".txt, .labels, .csv: labels, license text, prompts, or notes. Useful for display and classification labels, but not model weights.");
            AppendOutput(".prototxt: Caffe network description. It usually pairs with a .caffemodel weights file.");
            AppendOutput(".onnx_data or extra shard files: large ONNX models may store weights beside the .onnx. Keep the whole folder together when downloading.");
            AppendOutput(".h5, .keras, .ckpt: Keras/TensorFlow training formats. Useful if you plan to convert them elsewhere; this app is built around OpenVINO-ready runtime files.");
            AppendOutput(".npz, .npy: NumPy weights or sample tensors. They can be useful for converters or tests, but they are not standalone models here.");
            AppendOutput(".engine, .plan: TensorRT engine files. They are NVIDIA-specific compiled models and are not useful for OpenVINO on Intel GPUs.");
            AppendOutput(".mlmodel: Apple Core ML model. Useful on Apple platforms, but not a direct runtime model for this Windows/OpenVINO app.");
            AppendOutput("Why choose .xml/.bin instead of .gguf?");
            AppendOutput("  Choose .xml/.bin or a converted blob when you want the normal OpenVINO runtime path, model inspection, image/classification models, or the most control over GPU compilation.");
            AppendOutput("  Choose .gguf when you mainly want a text chat model and accept the GenAI GGUF route. Use OVMS Pull with --gguf_filename for direct Hugging Face GGUF pulls.");
            AppendOutput("Intel Arc Pro B70 GPU notes");
            AppendOutput("  The file extension does not directly choose special GPU registers. OpenVINO chooses GPU kernels and hardware features after you compile or load the model for a GPU target.");
            AppendOutput("  Intel Arc Pro B70 is an Xe2 GPU with Intel XMX engines and OpenVINO support. To give OpenVINO the best chance to use those GPU features, use an OpenVINO IR model or a GPU-targeted blob, select GPU.0, GPU.1, GPU.2, or MULTI on Workstation, then load it.");
            AppendOutput("  GGUF can still run on Intel GPU through OpenVINO GenAI for supported LLMs, but it is the less general path and may not expose the same conversion or inspection controls.");
            AppendOutput("Rule of thumb: for Arc Pro B70 testing, use supported GGUF models through GenAI when you want a text chat model. Use Convert only when you already have OpenVINO .xml/.bin and want a GPU-targeted blob.");
            AppendOutput(string.Empty);
        }

        private void UpdateDownloadUrlPreview()
        {
            if (!txtDownloadUrl.ReadOnly)
            {
                return;
            }

            try
            {
                string repoId = txtRepoId.Text.Trim();
                ModelFileListItem? selectedItem = GetSelectedModelFileItem();

                if (string.IsNullOrWhiteSpace(repoId) || selectedItem is null)
                {
                    txtDownloadUrl.Clear();
                    return;
                }

                if (selectedItem.IsFolder)
                {
                    txtDownloadUrl.Text = "(folder selection downloads multiple file URLs)";
                    return;
                }

                if (_modelCatalogSource == ModelCatalogSource.Ollama)
                {
                    SplitOllamaModelNameAndTag(repoId, selectedItem.Path, out string modelName, out string tag);
                    txtDownloadUrl.Text = BuildOllamaRegistryManifestUrl(modelName, tag);
                    return;
                }

                txtDownloadUrl.Text = BuildModelFileDownloadUrl(repoId, selectedItem.Path, GetRemoteRevision());
            }
            catch (Exception ex)
            {
                txtDownloadUrl.Text = $"Unable to build URL: {ShortError(ex)}";
            }
        }

        private void AppendDownloadSelectionGuidance(ModelFileListItem item)
        {
            if (_modelCatalogSource == ModelCatalogSource.Ollama)
            {
                AppendOutput($"Models: selected Ollama tag `{item.Path}`. Download will fetch its manifest and registry blobs.");
                return;
            }

            if (item.IsFolder)
            {
                AppendOutput($"Models: selected folder `{item.Path}`. Download will include all listed files under that folder.");
                return;
            }

            switch (GetModelFileKind(item.Path))
            {
                case ModelFileKind.OpenVinoIr:
                    AppendOutput($"Models: `{item.Path}` is OpenVINO IR. Download the matching .bin too if present.");
                    break;
                case ModelFileKind.OpenVinoWeights:
                    AppendOutput($"Models: `{item.Path}` is usually OpenVINO IR weights. Download it with the matching .xml; it is not useful by itself.");
                    break;
                case ModelFileKind.OpenVinoCoreReadable:
                    AppendOutput($"Models: `{item.Path}` can be read by OpenVINO Core. After download, use Workstation to choose the target and Inference to test it.");
                    break;
                case ModelFileKind.Gguf:
                    AppendOutput($"Models: `{item.Path}` is GGUF. Use it with OpenVINO GenAI for supported LLM architectures.");
                    break;
                case ModelFileKind.PyTorchSource:
                    AppendOutput($"Models: `{item.Path}` is a PyTorch source/checkpoint file. It usually needs Python/Optimum/OpenVINO conversion before this self-contained app can compile it.");
                    break;
                case ModelFileKind.SupportFile:
                    AppendOutput($"Models: `{item.Path}` is likely a tokenizer/config/weight support file. Download it as part of a full model folder rather than as a standalone OpenVINO model.");
                    break;
                default:
                    string extension = Path.GetExtension(item.Path).ToLowerInvariant();
                    AppendOutput($"Models: `{item.Path}` has extension `{extension}`. For text chat models, GGUF runs through GenAI. For other model types, prefer OpenVINO IR .xml/.bin when available.");
                    break;
            }
        }

        private void btnCancelDownload_Click(object sender, EventArgs e)
        {
            if (_downloadCancellation is null)
            {
                AppendOutput("Models: no download is running.");
                return;
            }

            AppendOutput("Models: canceling download...");
            _downloadCancellation.Cancel();
        }

        private async Task LoadCurrentModelCatalogAsync()
        {
            if (_modelCatalogSource == ModelCatalogSource.OfficialOpenVino)
            {
                await LoadOpenVinoModelZooIndexAsync();
                return;
            }

            if (_modelCatalogSource == ModelCatalogSource.Ollama)
            {
                await LoadOllamaLibraryAsync(txtHfQuery.Text);
                return;
            }

            string sourceName = _modelCatalogSource switch
            {
                ModelCatalogSource.OpenVino => "OpenVINO",
                ModelCatalogSource.Intel => "Intel",
                ModelCatalogSource.Ultralytics => "Ultralytics",
                _ => "Hugging Face",
            };

            await LoadHuggingFaceModelsAsync(_modelCatalogSource, sourceName, txtHfQuery.Text);
        }
        #endregion

        #region Shared conversion
        private static string? TryReadJsonString(System.Text.Json.JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
                && property.ValueKind == System.Text.Json.JsonValueKind.String
                ? property.GetString()
                : null;
        }

        private static bool? TryReadJsonBool(System.Text.Json.JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
                && (property.ValueKind == System.Text.Json.JsonValueKind.True || property.ValueKind == System.Text.Json.JsonValueKind.False)
                ? property.GetBoolean()
                : null;
        }

        private static IReadOnlyList<string> TryReadJsonStringArray(System.Text.Json.JsonElement element, string propertyName)
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

        private static bool IsVisionLanguageModelType(string modelType)
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

        private static string FormatCommandArguments(IEnumerable<string> arguments)
        {
            return string.Join(" ", arguments.Select(QuoteCommandArgument));
        }

        private static string QuoteCommandArgument(string argument)
        {
            return argument.Any(char.IsWhiteSpace) || argument.Contains('"', StringComparison.Ordinal)
                ? $"\"{argument.Replace("\"", "\\\"", StringComparison.Ordinal)}\""
                : argument;
        }

        private static void WriteTextAtomically(string path, string contents)
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

        private static bool IsPathUnder(string candidatePath, string rootPath)
        {
            string candidate = Path.GetFullPath(candidatePath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string root = Path.GetFullPath(rootPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDirectoryPathForComparison(string path)
        {
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
        }

        #endregion

        #region OVMS Pull
        private const string DefaultOvmsExecutablePath = "C:\\Example\\OpenVINO\\ovms.exe";
        private const string DefaultOvmsPythonRuntimePath = "C:\\Example\\OpenVINO\\python";
        private const string DefaultOvmsExportToolsPath = "C:\\Users\\ExampleUser\\AppData\\Local\\Python\\pythoncore-3.14-64\\Scripts";
        private const string DefaultOvmsModelRepositoryPath = "C:\\Example\\Models";
        private const string PreferredOvmsPullSourceModel = "Qwen/Qwen3.5-4B";
        private const string OvmsSourceFormatExport = "HF export";
        private const string OvmsSourceFormatGguf = "GGUF";
        private const string DefaultOvmsGgufSourceModel = "unsloth/Qwen3.6-27B-MTP-GGUF";
        private const string DefaultOvmsGgufFilename = "Qwen3.6-27B-UD-Q8_K_XL.gguf";

        private sealed class OvmsPullDefaults
        {
            public string OvmsExecutable { get; set; } = DefaultOvmsExecutablePath;
            public string PythonRuntimePath { get; set; } = DefaultOvmsPythonRuntimePath;
            public string ExportToolsPath { get; set; } = DefaultOvmsExportToolsPath;
            public string SourceFormat { get; set; } = OvmsSourceFormatExport;
            public string Task { get; set; } = "text_generation";
            public string PipelineType { get; set; } = "VLM";
            public string SourceModel { get; set; } = PreferredOvmsPullSourceModel;
            public string GgufFilename { get; set; } = string.Empty;
            public string ModelRepositoryPath { get; set; } = DefaultOvmsModelRepositoryPath;
            public string WeightFormat { get; set; } = "int8";
            public string LogPath { get; set; } = "C:\\Example\\OpenVINO\\qwen35-4b-pull-global.log";
            public bool OverwriteModels { get; set; } = true;
            public bool DebugLogging { get; set; } = true;
            public List<string> SourceFormats { get; } = new() { OvmsSourceFormatExport, OvmsSourceFormatGguf };
            public List<string> Tasks { get; } = new() { "text_generation" };
            public List<string> PipelineTypes { get; } = new() { "VLM", "LM" };
            public List<string> SourceModels { get; } = new() { PreferredOvmsPullSourceModel, DefaultOvmsGgufSourceModel };
            public List<string> WeightFormats { get; } = new() { "int8", "int4", "fp16", "fp32" };
            public List<OvmsPullCommandBlock> Commands { get; } = new();
        }

        private sealed class OvmsPullCommandBlock
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

        private sealed class OvmsPullPlan
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

        private sealed record OvmsVersionExpectation(
            string Name,
            string ExpectedVersion,
            bool Required,
            bool Strict);

        private sealed class OvmsStackCheckResult
        {
            public bool CanRunPull { get; set; } = true;
            public bool HasWarnings { get; set; }
            public string ExportPythonPath { get; set; } = string.Empty;
            public List<string> Lines { get; } = new();
            public List<string> RepairCommands { get; } = new();
        }

        private sealed class OvmsCommandResult
        {
            public int ExitCode { get; init; }
            public bool TimedOut { get; init; }
            public string StandardOutput { get; init; } = string.Empty;
            public string StandardError { get; init; } = string.Empty;
            public string StartError { get; init; } = string.Empty;
            public bool Started => string.IsNullOrWhiteSpace(StartError);
            public bool Succeeded => Started && !TimedOut && ExitCode == 0;
        }

        private void InitializeOvmsPullTab()
        {
            OvmsPullDefaults defaults = LoadOvmsPullDefaultsFromContext();
            _ovmsPullCommandDefaultsBySource.Clear();
            foreach (OvmsPullCommandBlock command in defaults.Commands)
            {
                if (!string.IsNullOrWhiteSpace(command.SourceModel))
                {
                    _ovmsPullCommandDefaultsBySource[command.SourceModel] = command;
                }
            }

            _updatingOvmsPullDefaults = true;
            try
            {
                txtOvmsExecutable.Text = defaults.OvmsExecutable;
                txtOvmsPythonPath.Text = defaults.PythonRuntimePath;
                txtOvmsExportToolsPath.Text = defaults.ExportToolsPath;
                SetComboValues(cboOvmsSourceFormat, defaults.SourceFormats, defaults.SourceFormat);
                SetComboValues(cboOvmsTask, defaults.Tasks, defaults.Task);
                SetComboValues(cboOvmsPipelineType, defaults.PipelineTypes, defaults.PipelineType);
                SetComboValues(cboOvmsSourceModel, defaults.SourceModels, defaults.SourceModel);
                SetComboValues(cboOvmsWeightFormat, defaults.WeightFormats, defaults.WeightFormat);
                txtOvmsGgufFilename.Text = defaults.GgufFilename;
                txtOvmsModelRepository.Text = defaults.ModelRepositoryPath;
                txtOvmsLogPath.Text = defaults.LogPath;
                chkOvmsOverwrite.Checked = defaults.OverwriteModels;
                chkOvmsDebug.Checked = defaults.DebugLogging;
            }
            finally
            {
                _updatingOvmsPullDefaults = false;
            }

            UpdateOvmsPullStatus();
        }

        private static void SetComboValues(ComboBox comboBox, IEnumerable<string> values, string selectedValue)
        {
            string[] distinctValues = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            comboBox.Items.Clear();
            foreach (string value in distinctValues)
            {
                comboBox.Items.Add(value);
            }

            int selectedIndex = Array.FindIndex(
                distinctValues,
                value => value.Equals(selectedValue, StringComparison.OrdinalIgnoreCase));
            if (selectedIndex >= 0)
            {
                comboBox.SelectedIndex = selectedIndex;
            }
            else
            {
                comboBox.Text = selectedValue;
            }
        }

        private static OvmsPullDefaults LoadOvmsPullDefaultsFromContext()
        {
            OvmsPullDefaults defaults = new();
            string contextPath = GetProjectContextPath();
            if (File.Exists(contextPath))
            {
                string text = File.ReadAllText(contextPath);
                defaults.OvmsExecutable = ReadContextValue(text, "OVMS executable:") ?? defaults.OvmsExecutable;
                string? globalOptimumCli = ReadContextValue(text, "Global optimum-cli:");
                string? globalOptimumCliFolder = string.IsNullOrWhiteSpace(globalOptimumCli)
                    ? null
                    : Path.GetDirectoryName(globalOptimumCli);
                defaults.ExportToolsPath = string.IsNullOrWhiteSpace(globalOptimumCliFolder)
                    ? defaults.ExportToolsPath
                    : globalOptimumCliFolder;
                string? bundledPython = ReadContextValue(text, "Bundled Python runtime:");
                string? bundledPythonFolder = string.IsNullOrWhiteSpace(bundledPython)
                    ? null
                    : Path.GetDirectoryName(bundledPython);
                defaults.PythonRuntimePath = string.IsNullOrWhiteSpace(bundledPythonFolder)
                    ? defaults.PythonRuntimePath
                    : bundledPythonFolder;
                defaults.ModelRepositoryPath = ReadContextValue(text, "Model repository:") ?? defaults.ModelRepositoryPath;

                foreach (OvmsPullCommandBlock command in ExtractOvmsPullCommands(text))
                {
                    defaults.Commands.Add(command);
                    AddUniqueOption(defaults.SourceFormats, command.SourceFormat);
                    AddUniqueOption(defaults.Tasks, command.Task);
                    AddUniqueOption(defaults.PipelineTypes, command.PipelineType);
                    AddUniqueOption(defaults.SourceModels, command.SourceModel);
                    AddUniqueOption(defaults.WeightFormats, command.WeightFormat);
                }
            }

            OvmsPullCommandBlock builtInGgufCommand = new()
            {
                SourceFormat = OvmsSourceFormatGguf,
                Task = "text_generation",
                SourceModel = DefaultOvmsGgufSourceModel,
                GgufFilename = DefaultOvmsGgufFilename,
                ModelRepositoryPath = defaults.ModelRepositoryPath,
                LogLevel = "DEBUG",
                LogPath = "C:\\Example\\OpenVINO\\qwen36-27b-mtp-gguf-q8.log",
                OverwriteModels = true,
            };
            defaults.Commands.Add(builtInGgufCommand);
            AddUniqueOption(defaults.SourceFormats, builtInGgufCommand.SourceFormat);
            AddUniqueOption(defaults.Tasks, builtInGgufCommand.Task);
            AddUniqueOption(defaults.SourceModels, builtInGgufCommand.SourceModel);

            OvmsPullCommandBlock? selectedCommand = defaults.Commands.FirstOrDefault(
                    command => command.SourceModel.Equals(PreferredOvmsPullSourceModel, StringComparison.OrdinalIgnoreCase))
                ?? defaults.Commands.FirstOrDefault(command => command.PipelineType.Equals("VLM", StringComparison.OrdinalIgnoreCase))
                ?? defaults.Commands.FirstOrDefault();

            if (selectedCommand is not null)
            {
                ApplyCommandBlockToDefaults(defaults, selectedCommand);
            }

            AddUniqueOption(defaults.Tasks, defaults.Task);
            AddUniqueOption(defaults.PipelineTypes, defaults.PipelineType);
            AddUniqueOption(defaults.SourceModels, defaults.SourceModel);
            AddUniqueOption(defaults.WeightFormats, defaults.WeightFormat);
            return defaults;
        }

        private static void ApplyCommandBlockToDefaults(OvmsPullDefaults defaults, OvmsPullCommandBlock command)
        {
            if (!string.IsNullOrWhiteSpace(command.OvmsExecutable))
            {
                defaults.OvmsExecutable = command.OvmsExecutable;
            }

            if (!string.IsNullOrWhiteSpace(command.SourceFormat))
            {
                defaults.SourceFormat = command.SourceFormat;
            }
            else if (!string.IsNullOrWhiteSpace(command.GgufFilename))
            {
                defaults.SourceFormat = OvmsSourceFormatGguf;
            }

            if (!string.IsNullOrWhiteSpace(command.Task))
            {
                defaults.Task = command.Task;
            }

            if (!string.IsNullOrWhiteSpace(command.PipelineType))
            {
                defaults.PipelineType = command.PipelineType;
            }

            if (!string.IsNullOrWhiteSpace(command.SourceModel))
            {
                defaults.SourceModel = command.SourceModel;
            }

            if (!string.IsNullOrWhiteSpace(command.GgufFilename))
            {
                defaults.GgufFilename = command.GgufFilename;
            }

            if (!string.IsNullOrWhiteSpace(command.ModelRepositoryPath))
            {
                defaults.ModelRepositoryPath = command.ModelRepositoryPath;
            }

            if (!string.IsNullOrWhiteSpace(command.WeightFormat))
            {
                defaults.WeightFormat = command.WeightFormat;
            }

            if (!string.IsNullOrWhiteSpace(command.LogPath))
            {
                defaults.LogPath = command.LogPath;
            }

            defaults.OverwriteModels = command.OverwriteModels;
            defaults.DebugLogging = command.LogLevel.Equals("DEBUG", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetProjectContextPath()
        {
            return Path.Combine(GetProjectDirectory(), "Context.md");
        }

        private static string? ReadContextValue(string text, string label)
        {
            foreach (string rawLine in SplitTextLines(text))
            {
                string line = rawLine.Trim();
                if (!line.StartsWith(label, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string value = line[label.Length..].Trim();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            return null;
        }

        private static IReadOnlyList<OvmsPullCommandBlock> ExtractOvmsPullCommands(string text)
        {
            List<OvmsPullCommandBlock> commands = new();
            OvmsPullCommandBlock? current = null;

            foreach (string rawLine in SplitTextLines(text))
            {
                string line = rawLine.Trim();
                if (line.StartsWith("```", StringComparison.Ordinal))
                {
                    AddCurrentCommand();
                    continue;
                }

                if (line.Contains("ovms.exe", StringComparison.OrdinalIgnoreCase)
                    && line.Contains("--pull", StringComparison.OrdinalIgnoreCase))
                {
                    AddCurrentCommand();
                    current = new OvmsPullCommandBlock();
                    int pullIndex = line.IndexOf("--pull", StringComparison.OrdinalIgnoreCase);
                    string executable = pullIndex > 0 ? line[..pullIndex].Trim() : line;
                    current.OvmsExecutable = executable.TrimEnd('^').Trim().Trim('"');
                    continue;
                }

                if (current is null || !line.StartsWith("--", StringComparison.Ordinal))
                {
                    continue;
                }

                ParseOvmsPullArgumentLine(current, line);
            }

            AddCurrentCommand();
            return commands;

            void AddCurrentCommand()
            {
                if (current is not null && current.HasArguments)
                {
                    commands.Add(current);
                }

                current = null;
            }
        }

        private static void ParseOvmsPullArgumentLine(OvmsPullCommandBlock command, string rawLine)
        {
            string line = rawLine.TrimEnd('^').Trim();
            if (!line.StartsWith("--", StringComparison.Ordinal))
            {
                return;
            }

            string option = line[2..];
            int separator = option.IndexOfAny(new[] { ' ', '\t' });
            string name = separator < 0 ? option : option[..separator];
            string value = separator < 0 ? string.Empty : option[(separator + 1)..].Trim().Trim('"');
            switch (name)
            {
                case "task":
                    command.Task = value;
                    break;
                case "pipeline_type":
                    command.PipelineType = value;
                    break;
                case "source_model":
                    command.SourceModel = value;
                    break;
                case "gguf_filename":
                    command.GgufFilename = value;
                    command.SourceFormat = OvmsSourceFormatGguf;
                    break;
                case "model_repository_path":
                    command.ModelRepositoryPath = value;
                    break;
                case "weight-format":
                    command.WeightFormat = value;
                    break;
                case "overwrite_models":
                    command.OverwriteModels = true;
                    break;
                case "log_level":
                    command.LogLevel = value;
                    break;
                case "log_path":
                    command.LogPath = value;
                    break;
            }
        }

        private static IEnumerable<string> SplitTextLines(string text)
        {
            return text.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n')
                .Split('\n');
        }

        private static void AddUniqueOption(List<string> values, string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || values.Any(existing => existing.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            values.Add(value.Trim());
        }

        private void cboOvmsSourceFormat_SelectedIndexChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void cboOvmsSourceFormat_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void cboOvmsTask_SelectedIndexChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void cboOvmsTask_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void cboOvmsPipelineType_SelectedIndexChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void cboOvmsPipelineType_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void cboOvmsWeightFormat_SelectedIndexChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void cboOvmsWeightFormat_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void txtOvmsExecutable_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void txtOvmsPythonPath_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void txtOvmsExportToolsPath_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void txtOvmsGgufFilename_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void txtOvmsModelRepository_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void txtOvmsLogPath_TextChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void chkOvmsOverwrite_CheckedChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private void chkOvmsDebug_CheckedChanged(object? sender, EventArgs e) => UpdateOvmsPullStatus();
        private async void btnCheckOvmsVersions_Click(object? sender, EventArgs e) => await CheckOvmsVersionsForSelectedPullAsync();
        private void btnPreviewOvmsPull_Click(object? sender, EventArgs e) => PreviewOvmsPullCommand();
        private async void btnRunOvmsPull_Click(object? sender, EventArgs e) => await StartOvmsPullCommandPromptAsync();

        private void cboOvmsSourceModel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyOvmsSourceDefaultsFromSelection();
            UpdateOvmsPullStatus();
        }

        private void cboOvmsSourceModel_TextChanged(object? sender, EventArgs e)
        {
            if (!_updatingOvmsPullDefaults)
            {
                UpdateOvmsPullStatus();
            }
        }

        private void ApplyOvmsSourceDefaultsFromSelection()
        {
            if (_updatingOvmsPullDefaults)
            {
                return;
            }

            string sourceModel = cboOvmsSourceModel.Text.Trim();
            if (string.IsNullOrWhiteSpace(sourceModel)
                || !_ovmsPullCommandDefaultsBySource.TryGetValue(sourceModel, out OvmsPullCommandBlock? command))
            {
                return;
            }

            _updatingOvmsPullDefaults = true;
            try
            {
                SetControlTextIfNotBlank(cboOvmsSourceFormat, string.IsNullOrWhiteSpace(command.SourceFormat)
                    ? string.IsNullOrWhiteSpace(command.GgufFilename) ? OvmsSourceFormatExport : OvmsSourceFormatGguf
                    : command.SourceFormat);
                SetControlTextIfNotBlank(cboOvmsTask, command.Task);
                SetControlTextIfNotBlank(cboOvmsPipelineType, command.PipelineType);
                SetControlTextIfNotBlank(cboOvmsWeightFormat, command.WeightFormat);
                SetControlTextIfNotBlank(txtOvmsGgufFilename, command.GgufFilename);
                SetControlTextIfNotBlank(txtOvmsModelRepository, command.ModelRepositoryPath);
                SetControlTextIfNotBlank(txtOvmsLogPath, command.LogPath);
                if (!string.IsNullOrWhiteSpace(command.OvmsExecutable))
                {
                    txtOvmsExecutable.Text = command.OvmsExecutable;
                }

                chkOvmsOverwrite.Checked = command.OverwriteModels;
                chkOvmsDebug.Checked = command.LogLevel.Equals("DEBUG", StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                _updatingOvmsPullDefaults = false;
            }
        }

        private static void SetControlTextIfNotBlank(Control control, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                control.Text = value.Trim();
            }
        }

        private void UpdateOvmsPullStatus()
        {
            if (_updatingOvmsPullDefaults)
            {
                return;
            }

            bool isGgufPull = IsOvmsGgufPullSelected();
            cboOvmsPipelineType.Enabled = !isGgufPull;
            lblOvmsPipelineType.Enabled = !isGgufPull;
            cboOvmsWeightFormat.Enabled = !isGgufPull;
            lblOvmsWeightFormat.Enabled = !isGgufPull;
            txtOvmsExportToolsPath.Enabled = !isGgufPull;
            lblOvmsExportToolsPath.Enabled = !isGgufPull;
            txtOvmsGgufFilename.Enabled = isGgufPull;
            lblOvmsGgufFilename.Enabled = isGgufPull;

            bool hasRequiredFields =
                !string.IsNullOrWhiteSpace(txtOvmsExecutable.Text)
                && !string.IsNullOrWhiteSpace(txtOvmsPythonPath.Text)
                && !string.IsNullOrWhiteSpace(cboOvmsTask.Text)
                && !string.IsNullOrWhiteSpace(cboOvmsSourceFormat.Text)
                && !string.IsNullOrWhiteSpace(cboOvmsSourceModel.Text)
                && !string.IsNullOrWhiteSpace(txtOvmsModelRepository.Text)
                && (isGgufPull
                    ? !string.IsNullOrWhiteSpace(txtOvmsGgufFilename.Text)
                    : !string.IsNullOrWhiteSpace(txtOvmsExportToolsPath.Text)
                        && !string.IsNullOrWhiteSpace(cboOvmsPipelineType.Text)
                        && !string.IsNullOrWhiteSpace(cboOvmsWeightFormat.Text));

            btnCheckOvmsVersions.Enabled = hasRequiredFields;
            btnPreviewOvmsPull.Enabled = hasRequiredFields;
            btnRunOvmsPull.Enabled = hasRequiredFields;

            if (!hasRequiredFields)
            {
                lblOvmsPullStatus.Text = "Fill in the OVMS pull settings.";
                return;
            }

            string executable = ExpandPathText(txtOvmsExecutable.Text);
            string pythonPath = ExpandPathText(txtOvmsPythonPath.Text);
            string exportToolsPath = ExpandPathText(txtOvmsExportToolsPath.Text);
            if (!File.Exists(executable))
            {
                lblOvmsPullStatus.Text = $"OVMS program not found: {executable}";
                return;
            }

            if (!Directory.Exists(pythonPath))
            {
                lblOvmsPullStatus.Text = $"Python DLL folder not found: {pythonPath}";
                return;
            }

            if (!isGgufPull)
            {
                string missingExportToolsPath = SplitPathList(exportToolsPath)
                    .FirstOrDefault(path => !Directory.Exists(path))
                    ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(missingExportToolsPath))
                {
                    lblOvmsPullStatus.Text = $"Export tools folder not found: {missingExportToolsPath}";
                    return;
                }
            }

            lblOvmsPullStatus.Text = isGgufPull
                ? $"Ready: {cboOvmsSourceModel.Text.Trim()} / {txtOvmsGgufFilename.Text.Trim()} -> {txtOvmsModelRepository.Text.Trim()}"
                : $"Ready: {cboOvmsSourceModel.Text.Trim()} -> {txtOvmsModelRepository.Text.Trim()}";
        }

        private bool IsOvmsGgufPullSelected()
        {
            return cboOvmsSourceFormat.Text.Trim().Equals(OvmsSourceFormatGguf, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(txtOvmsGgufFilename.Text)
                    && cboOvmsSourceModel.Text.Contains("GGUF", StringComparison.OrdinalIgnoreCase));
        }

        private void PreviewOvmsPullCommand()
        {
            try
            {
                OvmsPullPlan plan = BuildOvmsPullPlan();
                AppendOutput("OVMS pull command");
                AppendOutput($"  {plan.OvmsExecutable} {FormatCommandArguments(plan.Arguments)}");
                AppendOutput(plan.IsGgufPull
                    ? $"  Source type: GGUF direct ({plan.GgufFilename})"
                    : "  Source type: Hugging Face export through OVMS/Optimum");
                AppendOutput($"  Python DLL folder added to PATH: {plan.PythonRuntimePath}");
                if (!plan.IsGgufPull)
                {
                    AppendOutput($"  Export tools folder added to PATH: {plan.ExportToolsPath}");
                }

                AppendOutput($"  Command Prompt script: {BuildOvmsPullCommandScriptPath(plan)}");
                AppendOutput($"  Expected model folder: {plan.ExpectedModelFolder}");
                AppendOutput(chkOvmsOverwrite.Checked
                    ? "  Replace existing files: on. OVMS may replace the existing files for this model."
                    : "  Replace existing files: off. Existing files for this model are kept unless OVMS requires a clean target.");
                AppendOutput(chkOvmsDebug.Checked
                    ? "  Log level: DEBUG"
                    : "  Log level: INFO");
            }
            catch (Exception ex)
            {
                AppendOutput($"OVMS pull preview failed: {ShortError(ex)}");
            }
        }

        private async Task<bool> CheckOvmsVersionsForSelectedPullAsync()
        {
            OvmsPullPlan plan;
            try
            {
                plan = BuildOvmsPullPlan();
                ValidateOvmsPullPlan(plan);
            }
            catch (Exception ex)
            {
                AppendOutput($"OVMS stack check could not start: {ShortError(ex)}");
                UpdateOvmsPullStatus();
                return false;
            }

            OvmsStackCheckResult result = await CheckOvmsPullVersionStackAsync(plan, reportToOutput: true);
            lblOvmsPullStatus.Text = result.CanRunPull
                ? result.HasWarnings ? "Version check passed with warnings." : "Version check passed."
                : "Version check failed. See Output.";
            return result.CanRunPull;
        }

        private async Task<OvmsStackCheckResult> CheckOvmsPullVersionStackAsync(OvmsPullPlan plan, bool reportToOutput)
        {
            OvmsStackCheckResult result = new();
            IReadOnlyList<string> pathEntries = BuildOvmsPathPrefixEntries(plan);
            IReadOnlyList<OvmsVersionExpectation> expectations = LoadOvmsVersionExpectationsFromContext();
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
                ReportOvmsStackCheckResult(result, reportToOutput);
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
                ReportOvmsStackCheckResult(result, reportToOutput);
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
            ReportOvmsStackCheckResult(result, reportToOutput);
            return result;
        }

        private static void AddStackFailure(OvmsStackCheckResult result, string message)
        {
            result.CanRunPull = false;
            result.Lines.Add($"FAIL: {message}");
        }

        private static void AddStackWarning(OvmsStackCheckResult result, string message)
        {
            result.HasWarnings = true;
            result.Lines.Add($"WARN: {message}");
        }

        private static void CompareVersion(OvmsStackCheckResult result, OvmsVersionExpectation expectation, string actualVersion)
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

        private static void AppendCommandCheckOutput(OvmsStackCheckResult result, string label, OvmsCommandResult commandResult)
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

        private static void FinishOvmsStackCheck(
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

        private void ReportOvmsStackCheckResult(OvmsStackCheckResult result, bool reportToOutput)
        {
            if (!reportToOutput)
            {
                return;
            }

            foreach (string line in result.Lines)
            {
                AppendOutput(line);
            }

            if (result.RepairCommands.Count == 0)
            {
                return;
            }

            AppendOutput("Recommended repair commands for the export Python:");
            foreach (string command in result.RepairCommands)
            {
                AppendOutput($"  {command}");
            }

            AppendOutput("Run the repair commands in a normal Command Prompt, then click Check Stack again.");
        }

        private async Task StartOvmsPullCommandPromptAsync()
        {
            OvmsPullPlan plan;
            try
            {
                plan = BuildOvmsPullPlan();
                ValidateOvmsPullPlan(plan);
            }
            catch (Exception ex)
            {
                AppendOutput($"OVMS pull could not start: {ShortError(ex)}");
                UpdateOvmsPullStatus();
                return;
            }

            OvmsStackCheckResult versionCheck = await CheckOvmsPullVersionStackAsync(plan, reportToOutput: true);
            if (!versionCheck.CanRunPull)
            {
                lblOvmsPullStatus.Text = "Version check failed. See Output.";
                return;
            }

            StartOvmsPullCommandPrompt(plan);
        }

        private void StartOvmsPullCommandPrompt(OvmsPullPlan plan)
        {
            try
            {
                Directory.CreateDirectory(plan.ModelRepositoryPath);
                string? logDirectory = string.IsNullOrWhiteSpace(plan.LogPath) ? null : Path.GetDirectoryName(plan.LogPath);
                if (!string.IsNullOrWhiteSpace(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string scriptPath = WriteOvmsPullCommandScript(plan);
                System.Diagnostics.ProcessStartInfo startInfo = new()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/d /k {QuoteCommandPromptArgument(scriptPath)}",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(plan.OvmsExecutable) ?? GetProjectDirectory(),
                };

                System.Diagnostics.Process.Start(startInfo);

                AppendOutput("OVMS pull");
                AppendOutput("  Started in a Command Prompt window.");
                AppendOutput($"  Script: {scriptPath}");
                AppendOutput($"  Command: {plan.OvmsExecutable} {FormatCommandArguments(plan.Arguments)}");
                AppendOutput(plan.IsGgufPull
                    ? $"  Source type: GGUF direct ({plan.GgufFilename})"
                    : "  Source type: Hugging Face export through OVMS/Optimum");
                AppendOutput($"  Python DLL folder added to PATH: {plan.PythonRuntimePath}");
                if (!plan.IsGgufPull)
                {
                    AppendOutput($"  Export tools folder added to PATH: {plan.ExportToolsPath}");
                }

                AppendOutput($"  Model folder: {plan.ModelRepositoryPath}");
                AppendOutput($"  Expected output: {plan.ExpectedModelFolder}");
                if (!string.IsNullOrWhiteSpace(plan.LogPath))
                {
                    AppendOutput($"  Log file: {plan.LogPath}");
                }

                AppendOutput("  Close that Command Prompt when the pull is finished.");
                lblOvmsPullStatus.Text = $"Command Prompt started for {plan.SourceModel}.";
            }
            catch (Exception ex)
            {
                AppendOutput("  OVMS pull could not open Command Prompt:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"    {line}");
                }

                lblOvmsPullStatus.Text = "OVMS pull could not open Command Prompt. See Output for details.";
                UpdateOvmsPullStatus();
            }
        }

        private OvmsPullPlan BuildOvmsPullPlan()
        {
            string ovmsExecutable = ExpandPathText(txtOvmsExecutable.Text);
            string pythonPath = ExpandPathText(txtOvmsPythonPath.Text);
            string exportToolsPath = ExpandPathText(txtOvmsExportToolsPath.Text);
            string sourceFormat = cboOvmsSourceFormat.Text.Trim();
            bool isGgufPull = sourceFormat.Equals(OvmsSourceFormatGguf, StringComparison.OrdinalIgnoreCase);
            string task = cboOvmsTask.Text.Trim();
            string pipelineType = cboOvmsPipelineType.Text.Trim();
            string sourceModel = cboOvmsSourceModel.Text.Trim();
            string ggufFilename = txtOvmsGgufFilename.Text.Trim();
            string modelRepositoryPath = ExpandPathText(txtOvmsModelRepository.Text);
            string weightFormat = cboOvmsWeightFormat.Text.Trim();
            string logPath = ExpandPathText(txtOvmsLogPath.Text);

            if (string.IsNullOrWhiteSpace(ovmsExecutable))
            {
                throw new InvalidOperationException("Enter the OVMS program path.");
            }

            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                throw new InvalidOperationException("Enter the Python DLL folder.");
            }

            if (!isGgufPull && string.IsNullOrWhiteSpace(exportToolsPath))
            {
                throw new InvalidOperationException("Enter the export tools folder.");
            }

            if (string.IsNullOrWhiteSpace(task))
            {
                throw new InvalidOperationException("Choose a task.");
            }

            if (!isGgufPull && string.IsNullOrWhiteSpace(pipelineType))
            {
                throw new InvalidOperationException("Choose a pipeline type.");
            }

            if (string.IsNullOrWhiteSpace(sourceModel))
            {
                throw new InvalidOperationException("Choose a source model.");
            }

            if (string.IsNullOrWhiteSpace(modelRepositoryPath))
            {
                throw new InvalidOperationException("Enter the model folder.");
            }

            if (isGgufPull && string.IsNullOrWhiteSpace(ggufFilename))
            {
                throw new InvalidOperationException("Enter the GGUF filename from the Hugging Face repository.");
            }

            if (!isGgufPull && string.IsNullOrWhiteSpace(weightFormat))
            {
                throw new InvalidOperationException("Choose a weight format.");
            }

            IReadOnlyList<string> arguments = BuildOvmsPullArguments(
                task,
                pipelineType,
                sourceModel,
                ggufFilename,
                modelRepositoryPath,
                weightFormat,
                logPath,
                chkOvmsOverwrite.Checked,
                chkOvmsDebug.Checked,
                isGgufPull);

            return new OvmsPullPlan
            {
                OvmsExecutable = ovmsExecutable,
                PythonRuntimePath = pythonPath,
                ExportToolsPath = exportToolsPath,
                ModelRepositoryPath = modelRepositoryPath,
                SourceModel = sourceModel,
                GgufFilename = ggufFilename,
                IsGgufPull = isGgufPull,
                LogPath = logPath,
                ExpectedModelFolder = BuildExpectedOvmsModelFolder(modelRepositoryPath, sourceModel),
                Arguments = arguments,
            };
        }

        private static IReadOnlyList<string> BuildOvmsPullArguments(
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

        private static void ValidateOvmsPullPlan(OvmsPullPlan plan)
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

        private static string WriteOvmsPullCommandScript(OvmsPullPlan plan)
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

        private static string BuildOvmsPullCommandScriptPath(OvmsPullPlan plan)
        {
            string folder = Path.Combine(Path.GetTempPath(), "OpenVINO-Workbench");
            string sourceName = SanitizePathSegment(plan.SourceModel.Replace('/', '-').Replace('\\', '-'));
            return Path.Combine(folder, $"ovms-pull-{sourceName}.cmd");
        }

        private static IReadOnlyList<string> BuildOvmsPathPrefixEntries(OvmsPullPlan plan)
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

        private static string? FindOvmsExportPythonPath(IEnumerable<string> pathEntries)
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

        private static string? FindExecutableOnPath(string executableName, IEnumerable<string> pathEntries)
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

        private static IReadOnlyList<OvmsVersionExpectation> LoadOvmsVersionExpectationsFromContext()
        {
            Dictionary<string, string> versions = new(StringComparer.OrdinalIgnoreCase)
            {
                ["python"] = "3.14.6",
                ["transformers"] = "5.2.0",
                ["optimum-intel"] = "2.1.0.dev0",
                ["optimum"] = "2.2.0",
                ["openvino"] = "2026.2.1",
                ["huggingface-hub"] = "1.21.0",
                ["accelerate"] = "1.14.0",
            };

            string contextPath = GetProjectContextPath();
            if (File.Exists(contextPath))
            {
                ApplyKnownGlobalExportStackFromContext(File.ReadAllText(contextPath), versions);
            }

            return new[]
            {
                new OvmsVersionExpectation("Python", versions["python"], Required: true, Strict: true),
                new OvmsVersionExpectation("transformers", versions["transformers"], Required: true, Strict: true),
                new OvmsVersionExpectation("optimum-intel", versions["optimum-intel"], Required: true, Strict: true),
                new OvmsVersionExpectation("optimum", versions["optimum"], Required: true, Strict: true),
                new OvmsVersionExpectation("openvino", versions["openvino"], Required: true, Strict: true),
                new OvmsVersionExpectation("huggingface-hub", versions["huggingface-hub"], Required: false, Strict: false),
                new OvmsVersionExpectation("accelerate", versions["accelerate"], Required: false, Strict: false),
            };
        }

        private static void ApplyKnownGlobalExportStackFromContext(string text, IDictionary<string, string> versions)
        {
            bool inKnownStackSection = false;
            bool inCodeBlock = false;
            bool readVersionTable = false;
            foreach (string rawLine in SplitTextLines(text))
            {
                string line = rawLine.Trim();
                if (line.StartsWith("##", StringComparison.Ordinal))
                {
                    if (inKnownStackSection)
                    {
                        break;
                    }

                    inKnownStackSection = line.TrimStart('#', ' ').StartsWith("Known global export stack", StringComparison.OrdinalIgnoreCase)
                        && line.Contains("export stack", StringComparison.OrdinalIgnoreCase);
                    inCodeBlock = false;
                    continue;
                }

                if (line.StartsWith("Known-good global export stack:", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("Known global export stack:", StringComparison.OrdinalIgnoreCase))
                {
                    inKnownStackSection = true;
                    inCodeBlock = false;
                    continue;
                }

                if (!inKnownStackSection)
                {
                    continue;
                }

                if (line.StartsWith("```", StringComparison.Ordinal))
                {
                    if (inCodeBlock && readVersionTable)
                    {
                        break;
                    }

                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                if (inCodeBlock)
                {
                    readVersionTable |= ApplyExpectedVersionLine(line, versions);
                }
            }
        }

        private static bool ApplyExpectedVersionLine(string line, IDictionary<string, string> versions)
        {
            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return false;
            }

            string name = NormalizePythonPackageName(parts[0]);
            if (!versions.ContainsKey(name))
            {
                return false;
            }

            versions[name] = parts[1].Trim().TrimEnd(',');
            return true;
        }

        private static async Task<OvmsCommandResult> RunOvmsCommandAsync(
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

        private static string CombineCommandOutput(OvmsCommandResult commandResult)
        {
            return string.Join(
                Environment.NewLine,
                new[] { commandResult.StandardOutput, commandResult.StandardError }.Where(text => !string.IsNullOrWhiteSpace(text)));
        }

        private static IEnumerable<string> GetFirstNonEmptyCommandLines(OvmsCommandResult commandResult, int maxLines)
        {
            return SplitTextLines(CombineCommandOutput(commandResult))
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Take(maxLines);
        }

        private static string ParsePythonVersion(string commandOutput)
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

        private static Dictionary<string, string> ParsePipShowVersions(string commandOutput)
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

        private static string NormalizePythonPackageName(string name)
        {
            return name.Trim().Replace('_', '-').ToLowerInvariant();
        }

        private static IReadOnlyList<string> BuildOvmsStackRepairCommands(
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

        private static IEnumerable<string> SplitPathList(string pathList)
        {
            return pathList
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(path => Environment.ExpandEnvironmentVariables(path.Trim().Trim('"')))
                .Where(path => !string.IsNullOrWhiteSpace(path));
        }

        private static string BuildBatchCommandLine(string executable, IEnumerable<string> arguments)
        {
            return QuoteBatchArgument(executable) + " " + string.Join(" ", arguments.Select(QuoteBatchArgument));
        }

        private static string QuoteBatchArgument(string value)
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        private static string QuoteCommandPromptArgument(string value)
        {
            return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
        }

        private static string BuildExpectedOvmsModelFolder(string modelRepositoryPath, string sourceModel)
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

        private static string ExpandPathText(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : Environment.ExpandEnvironmentVariables(value.Trim().Trim('"'));
        }

        #endregion

        #region Convert
        private sealed class ModelCompileResult
        {
            public required string ModelPath { get; init; }
            public string BinPath { get; init; } = string.Empty;
            public required string FriendlyName { get; init; }
            public required IReadOnlyList<string> AvailableDevices { get; init; }
            public required IReadOnlyList<DeviceCompileResult> CompiledDevices { get; init; }
            public required IReadOnlyList<string> Inputs { get; init; }
            public required IReadOnlyList<string> Outputs { get; init; }
            public long ReadMilliseconds { get; init; }
        }

        private sealed class DeviceCompileResult
        {
            public required string DeviceName { get; init; }
            public string FullDeviceName { get; init; } = string.Empty;
            public long CompileMilliseconds { get; init; }
        }

        private sealed class PreparedIrModel
        {
            public required string Xml { get; init; }
            public string Bin { get; init; } = string.Empty;
            public bool HasWeights => !string.IsNullOrWhiteSpace(Bin);
        }

        private sealed class IrConversionResult
        {
            public required string SourcePath { get; init; }
            public required string OutputFolder { get; init; }
            public required string PrimaryXml { get; init; }
            public string PrimaryBin { get; init; } = string.Empty;
            public IReadOnlyList<string> OutputFiles { get; init; } = Array.Empty<string>();
            public long ConvertMilliseconds { get; init; }
            public string Note { get; init; } = string.Empty;
        }

        private enum ConvertModelSourceKind
        {
            OpenVinoIr,
        }

        private sealed class ConvertModelSelectionEntry
        {
            public required string Id { get; init; }
            public required string DisplayName { get; init; }
            public required string ModelDirectory { get; init; }
            public required string SourcePath { get; init; }
            public string Revision { get; init; } = "downloaded";
            public bool IsLocal { get; init; }
            public ConvertModelSourceKind Kind { get; init; } = ConvertModelSourceKind.OpenVinoIr;
            public bool IsOpenVinoIr => Kind == ConvertModelSourceKind.OpenVinoIr;

            public override string ToString()
            {
                return DisplayName;
            }
        }

        private void btnRefreshConvertModels_Click(object? sender, EventArgs e) => RefreshConvertModelSelection(reportStatus: true);
        private async void btnConvertToIr_Click(object? sender, EventArgs e) => await CreateBlobFromSelectedIrAsync();
        private void cboConvertSourceModel_SelectedIndexChanged(object? sender, EventArgs e) => SelectConvertModelFromCombo();
        private void chkConvertBlob_CheckedChanged(object? sender, EventArgs e) => UpdateConvertActionButtons();

        private void ShowConvertHelp()
        {
            AppendOutput(string.Empty);
            AppendOutput("Convert Help - what a blob is");
            AppendOutput("------------------------------------------------------------");
            AppendOutput("A blob is an OpenVINO compiled model saved to disk for one runtime target, such as GPU.0, GPU.1, GPU.2, or MULTI.");
            AppendOutput("This app writes blobs beside the model's main folder: ModelName\\main\\... and ModelName\\GPU\\model.blob.");
            AppendOutput("Benefit: the model is already compiled for that target, so later loads can start faster and use the OpenVINO settings chosen during export.");
            AppendOutput("Tradeoff: a blob is not a portable source model. It is tied to the selected target and the OpenVINO runtime version used to create it.");
            AppendOutput("Convert lists OpenVINO .xml/.bin models from the model root.");
            AppendOutput("Choose an existing .xml/.bin entry and .blob to compile it directly for the current Workstation target.");
            AppendOutput("GGUF pulls now belong on OVMS Pull with --gguf_filename instead of Convert.");
            AppendOutput("If you use a native OpenVINO IR download, keep tokenizer files such as openvino_tokenizer.xml and openvino_detokenizer.xml beside the model.");
            AppendOutput("Choose .blob to compile the saved IR for the current Workstation target.");
            AppendOutput("Blob output is written beside the model's main folder, such as ModelName\\GPU\\model.blob or ModelName\\MULTI\\model.blob.");
            AppendOutput("Formats that do not convert here");
            AppendOutput("  .gguf: pull directly with OVMS Pull and a GGUF filename.");
            AppendOutput("  .safetensors, .pt, .pth, .onnx, .pb, .pdmodel: planned for OVMS/OpenVINO pull or converter workflows, not this blob-only tab.");
            AppendOutput("  .json, .model, .txt, .labels, .csv: support files such as config, tokenizer, labels, or notes. Keep them with the model, but they are not compiled into a blob by themselves.");
            AppendOutput("  .bin by itself: usually weights only. It helps when paired with the matching .xml, but it is not a standalone Convert input.");
            AppendOutput("  .engine and .plan: TensorRT/NVIDIA compiled formats. They do not help OpenVINO on Intel GPUs.");
            AppendOutput("Rule of thumb: choose the Workstation target you actually plan to run before creating a blob. Blobs may need to be regenerated after changing devices or OpenVINO versions.");
            AppendOutput(string.Empty);
        }

        private void RefreshConvertModelSelection(string? selectedId = null, bool reportStatus = false)
        {
            List<ConvertModelSelectionEntry> entries;
            try
            {
                entries = EnumerateConvertModelSelections()
                    .OrderBy(entry => entry.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex)
            {
                AppendOutput($"Convert model list could not be read: {ShortError(ex)}");
                return;
            }

            cboConvertSourceModel.Items.Clear();
            foreach (ConvertModelSelectionEntry entry in entries)
            {
                cboConvertSourceModel.Items.Add(entry);
            }

            if (entries.Count == 0)
            {
                cboConvertSourceModel.Text = string.Empty;
                lblIrPlaceholder.Text = "No OpenVINO .xml/.bin models found under the model root.";
                lblModelsStatus.Text = "Click Refresh Models after downloading model files.";
                if (reportStatus)
                {
                    AppendOutput("Convert: no OpenVINO .xml/.bin models found under the model root.");
                }

                UpdateConvertActionButtons();
                return;
            }

            int selectedIndex = selectedId is null
                ? 0
                : entries.FindIndex(entry => entry.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
            cboConvertSourceModel.SelectedIndex = Math.Max(0, selectedIndex);

            if (reportStatus)
            {
                AppendOutput($"Convert: found {entries.Count} OpenVINO .xml/.bin model(s).");
            }

            UpdateConvertActionButtons();
        }

        private static IEnumerable<ConvertModelSelectionEntry> EnumerateConvertModelSelections()
        {
            string modelRoot = Path.GetFullPath(GetModelRoot());
            if (!Directory.Exists(modelRoot))
            {
                yield break;
            }

            foreach (string modelFolder in Directory.EnumerateDirectories(modelRoot))
            {
                string fullModelFolder = Path.GetFullPath(modelFolder);
                string modelFolderName = Path.GetFileName(fullModelFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (string.IsNullOrWhiteSpace(modelFolderName) || IsReservedModelRootFolder(modelFolderName))
                {
                    continue;
                }

                foreach (string irXmlPath in EnumerateConvertOpenVinoIrFilesForModel(modelRoot, fullModelFolder))
                {
                    string fullIrXmlPath = Path.GetFullPath(irXmlPath);
                    string relativePath = Path.GetRelativePath(modelRoot, fullIrXmlPath);
                    yield return new ConvertModelSelectionEntry
                    {
                        Id = $"ir:{relativePath}",
                        DisplayName = BuildIrConvertDisplayName(modelRoot, fullIrXmlPath),
                        ModelDirectory = fullModelFolder,
                        SourcePath = fullIrXmlPath,
                        Revision = "model",
                        Kind = ConvertModelSourceKind.OpenVinoIr,
                    };
                }
            }
        }

        private static IEnumerable<string> EnumerateConvertOpenVinoIrFilesForModel(string modelRoot, string modelFolder)
        {
            string fullModelFolder = Path.GetFullPath(modelFolder);
            if (!IsPathUnder(fullModelFolder, modelRoot))
            {
                yield break;
            }

            foreach (string xmlPath in Directory.EnumerateFiles(fullModelFolder, "*.xml", SearchOption.AllDirectories))
            {
                string fullXmlPath = Path.GetFullPath(xmlPath);
                if (!IsConvertBlobSourceXml(fullXmlPath))
                {
                    continue;
                }

                yield return fullXmlPath;
            }
        }

        private static bool IsConvertBlobSourceXml(string xmlPath)
        {
            return File.Exists(xmlPath)
                && Path.GetExtension(xmlPath).Equals(".xml", StringComparison.OrdinalIgnoreCase)
                && !IsOpenVinoSupportXml(xmlPath)
                && File.Exists(Path.ChangeExtension(xmlPath, ".bin"));
        }

        private static bool HasMatchingIrBin(string xmlPath)
        {
            return File.Exists(xmlPath) && File.Exists(Path.ChangeExtension(xmlPath, ".bin"));
        }

        private static string BuildIrConvertDisplayName(string modelRoot, string xmlPath)
        {
            string modelFolder = GetConvertModelFolderForOpenVinoIr(modelRoot, xmlPath);
            string modelName = Path.GetFileName(modelFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string relativeFile = Path.GetRelativePath(modelFolder, xmlPath);
            return string.IsNullOrWhiteSpace(modelName)
                ? $"{relativeFile} (.xml/.bin)"
                : $"{modelName} - {relativeFile} (.xml/.bin)";
        }

        private static string GetConvertModelFolderForOpenVinoIr(string modelRoot, string xmlPath)
        {
            string fullXmlPath = Path.GetFullPath(xmlPath);
            string? mainFolder = FindContainingMainFolder(fullXmlPath, modelRoot);
            if (mainFolder is not null && Directory.GetParent(mainFolder) is DirectoryInfo modelFolder)
            {
                return modelFolder.FullName;
            }

            string relativePath = Path.GetRelativePath(modelRoot, fullXmlPath);
            string[] segments = relativePath
                .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 1 && !IsReservedModelRootFolder(segments[0]))
            {
                return Path.Combine(modelRoot, segments[0]);
            }

            return Path.GetDirectoryName(fullXmlPath) ?? modelRoot;
        }

        private void SelectConvertModelFromCombo()
        {
            ConvertModelSelectionEntry? entry = GetSelectedConvertModelEntry();
            if (entry is null)
            {
                lblIrPlaceholder.Text = "Select an OpenVINO .xml/.bin model, then create .blob.";
                UpdateConvertActionButtons();
                return;
            }

            chkConvertBlob.Checked = true;
            lblIrPlaceholder.Text = $"Selected OpenVINO IR model: {entry.SourcePath}";
            lblModelsStatus.Text = $"Ready to create .blob from {Path.GetFileName(entry.SourcePath)}.";
            UpdateConvertActionButtons();
        }

        private void UpdateConvertActionButtons()
        {
            ConvertModelSelectionEntry? entry = GetSelectedConvertModelEntry();
            btnConvertToIr.Text = "Create Blob";

            bool hasModel = entry is not null && File.Exists(entry.SourcePath);
            btnConvertToIr.Enabled = hasModel && chkConvertBlob.Checked;
        }

        private async Task CreateBlobFromSelectedIrAsync()
        {
            ConvertModelSelectionEntry? entry = GetSelectedConvertModelEntry();
            if (entry is null)
            {
                AppendOutput("Convert: select an OpenVINO .xml/.bin model first.");
                return;
            }

            if (!chkConvertBlob.Checked)
            {
                AppendOutput("Convert: choose .blob.");
                return;
            }

            if (!TryGetPrimaryCompileTarget(out string device))
            {
                return;
            }

            await ExportIrBlobForTargetAsync(entry, entry.SourcePath, device);
        }

        private async Task<string?> ExportIrBlobForTargetAsync(ConvertModelSelectionEntry entry, string sourceXml, string device)
        {
            string modelFolder = GetOptimizedBlobModelFolder(entry.ModelDirectory, sourceXml);
            string deviceFolder = GetOptimizedBlobDeviceFolder(entry.ModelDirectory, sourceXml, device);
            string blobPath = Path.Combine(deviceFolder, "model.blob");
            string temporaryBlobPath = $"{blobPath}.partial";

            CancellationTokenSource? operation = await TryBeginLongOperationAsync($"Creating .blob for {device}...", allowCancel: true);
            if (operation is null)
            {
                return null;
            }

            CancellationToken cancellationToken = operation.Token;
            btnConvertToIr.Enabled = false;
            string oldStatus = lblModelsStatus.Text;
            lblModelsStatus.Text = $"Creating .blob for {device}...";
            SetActivityState($"Compiling model for {device}...", allowCancel: true);

            try
            {
                Directory.CreateDirectory(deviceFolder);
                AppendOutput("Create .blob");
                AppendOutput($"  Source: {sourceXml}");
                AppendOutput($"  Target: {device}");
                AppendOutput($"  Destination: {deviceFolder}");
                ConfigureOpenVinoRuntimeSearchPath();

                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string? sourceBinPath = FindMatchingBinPath(sourceXml);
                    using OpenVinoSharp.Core core = new();
                    using OpenVinoSharp.Model model = string.IsNullOrWhiteSpace(sourceBinPath)
                        ? core.ReadModelUnicode(sourceXml)
                        : core.ReadModelUnicode(sourceXml, sourceBinPath);
                    cancellationToken.ThrowIfCancellationRequested();
                    using OpenVinoSharp.CompiledModel compiledModel = core.CompileModel(model, device);
                    cancellationToken.ThrowIfCancellationRequested();
                    compiledModel.ExportModel(temporaryBlobPath);
                    cancellationToken.ThrowIfCancellationRequested();
                    using OpenVinoSharp.CompiledModel validationModel = core.ImportModel(temporaryBlobPath, device);
                    _ = validationModel.InputCount;
                    cancellationToken.ThrowIfCancellationRequested();
                }, cancellationToken);

                File.Move(temporaryBlobPath, blobPath, overwrite: true);

                object manifest = new
                {
                    exportedAtUtc = DateTime.UtcNow,
                    sourcePath = sourceXml,
                    originalSource = entry.SourcePath,
                    sourceKind = entry.Kind.ToString(),
                    format = "OpenVINO compiled model blob",
                    device,
                    deviceFolder = Path.GetFileName(deviceFolder),
                    blob = Path.GetFileName(blobPath),
                };
                string manifestPath = Path.Combine(deviceFolder, "conversion.json");
                WriteTextAtomically(manifestPath, System.Text.Json.JsonSerializer.Serialize(
                    manifest,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

                AppendOutput($"  Compiled blob: {blobPath}");
                AppendOutput("  Status: .blob export and validation succeeded.");
                txtInferModel.Text = modelFolder;
                lblIrPlaceholder.Text = $"Created blob for {device}: {blobPath}";
                return blobPath;
            }
            catch (OperationCanceledException)
            {
                AppendOutput("  .blob export canceled.");
                return null;
            }
            catch (Exception ex)
            {
                AppendOutput("  .blob export failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"    {line}");
                }

                return null;
            }
            finally
            {
                if (File.Exists(temporaryBlobPath))
                {
                    File.Delete(temporaryBlobPath);
                }

                lblModelsStatus.Text = oldStatus;
                UpdateConvertActionButtons();
                FinishLongOperation();
            }
        }

        private async Task PrepareCoreReadableModelAsIrAsync(ConvertModelSelectionEntry entry, string sourcePath)
        {
            string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                CancellationTokenSource? operation = await TryBeginLongOperationAsync("Preparing .xml/.bin files...", allowCancel: true);
                if (operation is null)
                {
                    return;
                }

                btnConvertToIr.Enabled = false;
                string oldStatus = lblModelsStatus.Text;
                lblModelsStatus.Text = "Preparing .xml/.bin files...";

                try
                {
                    IrConversionResult result = await Task.Run(() =>
                    {
                        operation.Token.ThrowIfCancellationRequested();
                        string outputFolder = GetIrOutputMainFolder(entry.ModelDirectory, sourcePath);
                        Directory.CreateDirectory(outputFolder);
                        List<string> sourceFiles = new() { sourcePath };
                        string? binPath = FindMatchingBinPath(sourcePath);
                        if (!string.IsNullOrWhiteSpace(binPath))
                        {
                            sourceFiles.Add(binPath);
                        }

                        IReadOnlyList<string> outputFiles = CopyIrFilesToOutputFolder(sourceFiles, outputFolder);
                        string outputXml = Path.Combine(outputFolder, Path.GetFileName(sourcePath));
                        string outputBin = FindMatchingBinPath(outputXml) ?? string.Empty;
                        WriteIrConversionManifest(outputFolder, sourcePath, "OpenVINO IR copy", outputFiles);
                        return new IrConversionResult
                        {
                            SourcePath = sourcePath,
                            OutputFolder = outputFolder,
                            PrimaryXml = outputXml,
                            PrimaryBin = outputBin,
                            OutputFiles = outputFiles,
                            ConvertMilliseconds = 0,
                            Note = outputFolder.Equals(Path.GetDirectoryName(sourcePath), StringComparison.OrdinalIgnoreCase)
                                ? "The selected file is already OpenVINO IR."
                                : "The selected IR files were copied into the model's main folder.",
                        };
                    }, operation.Token);

                    AppendOutput("Convert to .xml/.bin");
                    AppendOutput($"  Source: {sourcePath}");
                    AppendOutput($"  Destination: {result.OutputFolder}");
                    AppendOutput($"  Primary XML: {result.PrimaryXml}");
                    if (!string.IsNullOrWhiteSpace(result.PrimaryBin))
                    {
                        AppendOutput($"  Primary BIN: {result.PrimaryBin}");
                    }

                    AppendOutput($"  Note: {result.Note}");
                    AppendOutput("  Status: .xml/.bin files are ready.");
                    txtInferModel.Text = result.OutputFolder;
                    RefreshConvertModelSelection(entry.Id);
                    lblIrPlaceholder.Text = $"Ready IR model: {result.OutputFolder}";
                }
                catch (OperationCanceledException)
                {
                    AppendOutput("  Convert to .xml/.bin canceled.");
                }
                catch (Exception ex)
                {
                    AppendOutput("  Convert to .xml/.bin failed:");
                    foreach (string line in FormatException(ex))
                    {
                        AppendOutput($"    {line}");
                    }
                }
                finally
                {
                    lblModelsStatus.Text = oldStatus;
                    UpdateConvertActionButtons();
                    FinishLongOperation();
                }

                return;
            }

            AppendOutput("Convert to .xml/.bin");
            AppendOutput($"  Source: {sourcePath}");
            AppendOutput("  Status: this file can be read and optimized by OpenVINO here, but this bundled C# runtime does not expose OpenVINO's source IR save API for this format.");
            AppendOutput("  Export this source model to .xml/.bin with OpenVINO Python/OVC before using it here.");
        }

        private async Task ExportSelectedModelForTargetAsync()
        {
            ConvertModelSelectionEntry? entry = GetSelectedConvertModelEntry();
            string? sourcePath = ResolveSelectedOpenVinoModelPath(logMissing: false);
            if (entry is null || sourcePath is null)
            {
                AppendOutput("Convert: select an OpenVINO .xml/.bin model.");
                return;
            }

            if (!TryGetPrimaryCompileTarget(out string device))
            {
                return;
            }
            string modelName = BuildOptimizedBlobModelName(entry.ModelDirectory, sourcePath);
            string outputFolder = GetOptimizedBlobModelFolder(entry.ModelDirectory, sourcePath);
            string deviceFolder = GetOptimizedBlobDeviceFolder(entry.ModelDirectory, sourcePath, device);
            string blobPath = Path.Combine(deviceFolder, "model.blob");
            string temporaryBlobPath = $"{blobPath}.partial";
            CancellationTokenSource? operation = await TryBeginLongOperationAsync($"Optimizing model for {device}...", allowCancel: true);
            if (operation is null)
            {
                return;
            }
            CancellationToken cancellationToken = operation.Token;

            btnConvertToIr.Enabled = false;
            string oldStatus = lblModelsStatus.Text;
            lblModelsStatus.Text = $"Optimizing model permanently for {device}...";
            SetActivityState($"Compiling and exporting model for {device}...", allowCancel: true);

            try
            {
                Directory.CreateDirectory(deviceFolder);
                AppendOutput("Export device-optimized model");
                AppendOutput($"  Source: {sourcePath}");
                AppendOutput($"  Target: {device}");
                AppendOutput($"  Destination: {deviceFolder}");
                ConfigureOpenVinoRuntimeSearchPath();

                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string? sourceBinPath = FindMatchingBinPath(sourcePath);
                    using OpenVinoSharp.Core core = new();
                    using OpenVinoSharp.Model model = string.IsNullOrWhiteSpace(sourceBinPath)
                        ? core.ReadModelUnicode(sourcePath)
                        : core.ReadModelUnicode(sourcePath, sourceBinPath);
                    cancellationToken.ThrowIfCancellationRequested();
                    using OpenVinoSharp.CompiledModel compiledModel = core.CompileModel(model, device);
                    cancellationToken.ThrowIfCancellationRequested();
                    compiledModel.ExportModel(temporaryBlobPath);
                    cancellationToken.ThrowIfCancellationRequested();
                    using OpenVinoSharp.CompiledModel validationModel = core.ImportModel(temporaryBlobPath, device);
                    _ = validationModel.InputCount;
                    cancellationToken.ThrowIfCancellationRequested();
                }, cancellationToken);

                File.Move(temporaryBlobPath, blobPath, overwrite: true);

                object manifest = new
                {
                    exportedAtUtc = DateTime.UtcNow,
                    sourcePath,
                    format = "OpenVINO compiled model blob",
                    device,
                    deviceFolder = Path.GetFileName(deviceFolder),
                    blob = Path.GetFileName(blobPath),
                };
                string manifestPath = Path.Combine(deviceFolder, "conversion.json");
                WriteTextAtomically(manifestPath, System.Text.Json.JsonSerializer.Serialize(
                    manifest,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

                AppendOutput($"  Compiled blob: {blobPath}");
                AppendOutput("  Status: export and import validation succeeded.");
                txtInferModel.Text = outputFolder;
                RefreshConvertModelSelection($"model:{modelName}");
                lblIrPlaceholder.Text = $"Optimized for {device} and validated: {deviceFolder}";
            }
            catch (OperationCanceledException)
            {
                AppendOutput("  Target optimization canceled.");
            }
            catch (Exception ex)
            {
                AppendOutput("  Target optimization failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"    {line}");
                }
            }
            finally
            {
                if (File.Exists(temporaryBlobPath)) File.Delete(temporaryBlobPath);
                lblModelsStatus.Text = oldStatus;
                UpdateConvertActionButtons();
                FinishLongOperation();
            }
        }

        private async Task CompileSelectedOpenVinoModelAsync()
        {
            string? modelPath = ResolveSelectedOpenVinoModelPath();
            IReadOnlyList<string> targets = GetSelectedCompileTargets();

            if (modelPath is null)
            {
                AppendOutput("Convert: select an OpenVINO .xml/.bin model.");
                return;
            }

            if (targets.Count == 0)
            {
                return;
            }

            CancellationTokenSource? operation = await TryBeginLongOperationAsync("Compiling model...", allowCancel: true);
            if (operation is null)
            {
                return;
            }
            CancellationToken cancellationToken = operation.Token;
            btnConvertToIr.Enabled = false;
            string oldStatus = lblModelsStatus.Text;
            lblModelsStatus.Text = "Compiling model for the selected target...";

            try
            {
                AppendOutput("Compile");
                AppendOutput($"  Model: {modelPath}");
                AppendOutput($"  Target: {string.Join(", ", targets)}");
                ConfigureOpenVinoRuntimeSearchPath();

                Progress<string> progress = new(message => AppendOutput(message));
                ModelCompileResult result = await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ModelCompileResult compileResult = CompileOpenVinoModelForTargets(modelPath, targets, progress, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    return compileResult;
                }, cancellationToken);

                AppendOutput($"  Devices: {string.Join(", ", result.AvailableDevices)}");
                AppendOutput($"  Friendly name: {result.FriendlyName}");
                if (!string.IsNullOrWhiteSpace(result.BinPath))
                {
                    AppendOutput($"  Weights: {result.BinPath}");
                }

                AppendOutput($"  Read time: {result.ReadMilliseconds:N0} ms");
                AppendOutput("  Compile targets:");
                foreach (DeviceCompileResult device in result.CompiledDevices)
                {
                    string displayName = string.IsNullOrWhiteSpace(device.FullDeviceName)
                        ? device.DeviceName
                        : $"{device.DeviceName} - {device.FullDeviceName}";
                    AppendOutput($"    {displayName}: {device.CompileMilliseconds:N0} ms");
                }

                AppendOutput("  Inputs:");
                foreach (string input in result.Inputs)
                {
                    AppendOutput($"    {input}");
                }

                AppendOutput("  Outputs:");
                foreach (string output in result.Outputs)
                {
                    AppendOutput($"    {output}");
                }

                AppendOutput($"  Status: model compiled successfully for {result.CompiledDevices.Count} target(s). Use Load Model to send it to Tests.");
            }
            catch (OperationCanceledException)
            {
                AppendOutput("  Compile canceled.");
            }
            catch (Exception ex)
            {
                AppendOutput("  Compile failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"    {line}");
                }
            }
            finally
            {
                lblModelsStatus.Text = oldStatus;
                UpdateConvertActionButtons();
                FinishLongOperation();
            }
        }

        private async Task LoadSelectedOpenVinoModelAsync()
        {
            string? modelPath = ResolveSelectedOpenVinoModelPath();

            if (modelPath is null)
            {
                AppendOutput("Convert: select an OpenVINO .xml/.bin model.");
                return;
            }

            if (!TryGetPrimaryCompileTarget(out string target))
            {
                return;
            }

            CancellationTokenSource? operation = await TryBeginLongOperationAsync("Loading model information...", allowCancel: true);
            if (operation is null)
            {
                return;
            }
            CancellationToken cancellationToken = operation.Token;
            btnConvertToIr.Enabled = false;
            string oldStatus = lblModelsStatus.Text;
            lblModelsStatus.Text = "Loading model metadata into Tests...";

            try
            {
                AppendOutput("Load");
                AppendOutput($"  Model: {modelPath}");
                AppendOutput($"  Target: {target}");
                ConfigureOpenVinoRuntimeSearchPath();

                ModelCompileResult result = await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ModelCompileResult metadata = LoadOpenVinoModelMetadata(modelPath, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    return metadata;
                }, cancellationToken);

                AppendOutput($"  Friendly name: {result.FriendlyName}");
                if (!string.IsNullOrWhiteSpace(result.BinPath))
                {
                    AppendOutput($"  Weights: {result.BinPath}");
                }

                AppendOutput($"  Read time: {result.ReadMilliseconds:N0} ms");
                AppendOutput("  Inputs:");
                foreach (string input in result.Inputs)
                {
                    AppendOutput($"    {input}");
                }

                AppendOutput("  Outputs:");
                foreach (string output in result.Outputs)
                {
                    AppendOutput($"    {output}");
                }

                txtInferModel.Text = Path.GetDirectoryName(result.ModelPath) ?? result.ModelPath;
                AppendOutput("  Status: loaded into Tests.");
            }
            catch (OperationCanceledException)
            {
                AppendOutput("  Load canceled.");
            }
            catch (Exception ex)
            {
                AppendOutput("  Load failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"    {line}");
                }
            }
            finally
            {
                lblModelsStatus.Text = oldStatus;
                UpdateConvertActionButtons();
                FinishLongOperation();
            }
        }

        private ModelCompileResult LoadOpenVinoModelMetadata(string modelPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? binPath = FindMatchingBinPath(modelPath);

            using OpenVinoSharp.Core core = new();

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using OpenVinoSharp.Model model = string.IsNullOrWhiteSpace(binPath)
                ? core.ReadModelUnicode(modelPath)
                : core.ReadModelUnicode(modelPath, binPath);
            stopwatch.Stop();
            cancellationToken.ThrowIfCancellationRequested();

            return new ModelCompileResult
            {
                ModelPath = modelPath,
                BinPath = binPath ?? string.Empty,
                FriendlyName = model.get_friendly_name(),
                AvailableDevices = GetAvailableOpenVinoDevices(core),
                CompiledDevices = Array.Empty<DeviceCompileResult>(),
                Inputs = DescribeModelInputs(model),
                Outputs = DescribeModelOutputs(model),
                ReadMilliseconds = stopwatch.ElapsedMilliseconds,
            };
        }

        private ModelCompileResult CompileOpenVinoModelForTargets(
            string modelPath,
            IReadOnlyList<string> targets,
            IProgress<string>? progress,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? binPath = FindMatchingBinPath(modelPath);

            using OpenVinoSharp.Core core = new();
            IReadOnlyList<string> availableDevices = GetAvailableOpenVinoDevices(core);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using OpenVinoSharp.Model model = string.IsNullOrWhiteSpace(binPath)
                ? core.ReadModelUnicode(modelPath)
                : core.ReadModelUnicode(modelPath, binPath);
            stopwatch.Stop();
            cancellationToken.ThrowIfCancellationRequested();
            long readMs = stopwatch.ElapsedMilliseconds;

            List<string> inputs = DescribeModelInputs(model);
            List<string> outputs = DescribeModelOutputs(model);
            string friendlyName = model.get_friendly_name();

            List<DeviceCompileResult> compiledDevices = new();
            foreach (string target in targets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report($"  Compiling {target}...");
                stopwatch.Restart();
                using OpenVinoSharp.CompiledModel compiledModel = core.CompileModel(model, target);
                stopwatch.Stop();
                cancellationToken.ThrowIfCancellationRequested();

                compiledDevices.Add(new DeviceCompileResult
                {
                    DeviceName = target,
                    FullDeviceName = TryGetOpenVinoProperty(core, target, "FULL_DEVICE_NAME"),
                    CompileMilliseconds = stopwatch.ElapsedMilliseconds,
                });
                progress?.Report($"  Compiled {target} in {stopwatch.ElapsedMilliseconds:N0} ms.");
            }

            return new ModelCompileResult
            {
                ModelPath = modelPath,
                BinPath = binPath ?? string.Empty,
                FriendlyName = friendlyName,
                AvailableDevices = availableDevices,
                CompiledDevices = compiledDevices,
                Inputs = inputs,
                Outputs = outputs,
                ReadMilliseconds = readMs,
            };
        }

        private static string TryGetOpenVinoProperty(OpenVinoSharp.Core core, string device, string key)
        {
            try
            {
                return core.GetProperty(device, key);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string? ResolveSelectedOpenVinoModelPath(bool logMissing = true)
        {
            string repoId = txtRepoId.Text.Trim();
            string revision = string.IsNullOrWhiteSpace(txtRevision.Text) ? "main" : txtRevision.Text.Trim();
            ModelFileListItem? selectedItem = GetSelectedModelFileItem();
            ConvertModelSelectionEntry? convertEntry = GetSelectedConvertModelEntry();

            if (convertEntry is { IsOpenVinoIr: true } && File.Exists(convertEntry.SourcePath))
            {
                return Path.GetFullPath(convertEntry.SourcePath);
            }

            if (selectedItem is { IsFolder: false } && IsOpenVinoReadableModelFile(selectedItem.Path))
            {
                string? selectedPath = ResolveListedModelFilePath(selectedItem.Path, repoId, revision, convertEntry);
                if (selectedPath is not null)
                {
                    return selectedPath;
                }
            }

            List<string> searchRoots = new();
            if (convertEntry is not null && Directory.Exists(convertEntry.ModelDirectory))
            {
                searchRoots.Add(convertEntry.ModelDirectory);
            }

            if (!string.IsNullOrWhiteSpace(repoId))
            {
                string modelDirectory = BuildModelDownloadDirectory(repoId, revision);
                if (Directory.Exists(modelDirectory))
                {
                    searchRoots.Add(modelDirectory);
                }
            }

            string[] candidates = searchRoots
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                .Where(IsOpenVinoReadableModelFile)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (candidates.Length == 0)
            {
                if (logMissing)
                {
                    AppendOutput("Prepare: no .onnx, .pb, .caffemodel, .tflite, .pdmodel, or .xml/.bin model file found in the selected folder.");
                }

                return null;
            }

            return candidates[0];
        }

        private string? ResolveListedModelFilePath(
            string filePath,
            string repoId,
            string revision,
            ConvertModelSelectionEntry? convertEntry)
        {
            if (convertEntry is not null)
            {
                string selectedConvertPath = Path.Combine(convertEntry.ModelDirectory, filePath);
                if (File.Exists(selectedConvertPath))
                {
                    return Path.GetFullPath(selectedConvertPath);
                }
            }

            if (!string.IsNullOrWhiteSpace(repoId))
            {
                string selectedDownloadPath = BuildModelFileDownloadPath(repoId, filePath, revision);
                if (File.Exists(selectedDownloadPath))
                {
                    return selectedDownloadPath;
                }
            }

            return null;
        }

        private string? ResolveSelectedAnyConvertFilePath()
        {
            ConvertModelSelectionEntry? convertEntry = GetSelectedConvertModelEntry();
            return convertEntry is not null && File.Exists(convertEntry.SourcePath)
                ? Path.GetFullPath(convertEntry.SourcePath)
                : null;
        }

        private static string GetIrOutputMainFolder(string sourceModelFolder, string sourcePath)
        {
            string modelRoot = Path.GetFullPath(GetModelRoot());
            string fullSourcePath = Path.GetFullPath(sourcePath);
            string? containingMain = FindContainingMainFolder(fullSourcePath, modelRoot);
            if (containingMain is not null)
            {
                return containingMain;
            }

            string selectedFolder = string.IsNullOrWhiteSpace(sourceModelFolder)
                ? Path.GetDirectoryName(fullSourcePath) ?? fullSourcePath
                : sourceModelFolder;
            string fullSelectedFolder = Path.GetFullPath(selectedFolder);
            if (Directory.Exists(fullSelectedFolder)
                && Path.GetFileName(fullSelectedFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                    .Equals("main", StringComparison.OrdinalIgnoreCase))
            {
                return fullSelectedFolder;
            }

            string modelFolder = GetOptimizedBlobModelFolder(sourceModelFolder, sourcePath);
            if (Path.GetFileName(modelFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                .Equals("main", StringComparison.OrdinalIgnoreCase))
            {
                return modelFolder;
            }

            return Path.Combine(modelFolder, "main");
        }

        private static IReadOnlyList<string> CopyIrFilesToOutputFolder(IEnumerable<string> sourceFiles, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            List<string> outputFiles = new();
            foreach (string sourceFile in sourceFiles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!File.Exists(sourceFile))
                {
                    continue;
                }

                string destination = Path.GetFullPath(Path.Combine(outputFolder, Path.GetFileName(sourceFile)));
                string sourceFullPath = Path.GetFullPath(sourceFile);
                if (!sourceFullPath.Equals(destination, StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(sourceFullPath, destination, overwrite: true);
                }

                outputFiles.Add(destination);
            }

            return outputFiles
                .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static void WriteIrConversionManifest(
            string outputFolder,
            string sourcePath,
            string method,
            IReadOnlyList<string> outputFiles)
        {
            object manifest = new
            {
                convertedAtUtc = DateTime.UtcNow,
                sourcePath,
                format = "OpenVINO IR model",
                method,
                files = outputFiles.Select(Path.GetFileName).ToArray(),
            };
            string manifestPath = Path.Combine(outputFolder, "ir-conversion.json");
            string json = System.Text.Json.JsonSerializer.Serialize(
                manifest,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            WriteTextAtomically(manifestPath, NormalizeTextFileLineEndings(json));
        }

        private static string? FindMatchingBinPath(string modelPath)
        {
            string binPath = Path.ChangeExtension(modelPath, ".bin");
            return File.Exists(binPath) ? binPath : null;
        }

        private static List<string> DescribeModelInputs(OpenVinoSharp.Model model)
        {
            List<string> inputs = new();
            for (ulong index = 0; index < model.get_inputs_size(); index++)
            {
                using OpenVinoSharp.Input input = model.get_input(index);
                inputs.Add(DescribeInput(index, input));
            }

            return inputs;
        }

        private static List<string> DescribeModelOutputs(OpenVinoSharp.Model model)
        {
            List<string> outputs = new();
            for (ulong index = 0; index < model.get_outputs_size(); index++)
            {
                using OpenVinoSharp.Output output = model.get_output(index);
                outputs.Add(DescribeOutput(index, output));
            }

            return outputs;
        }

        private static string DescribeInput(ulong index, OpenVinoSharp.Input input)
        {
            string name = SafePortName(() => input.get_any_name(), $"input_{index}");
            string type = SafePortText(() => input.get_element_type().ToString(), "type unavailable");
            string shape = SafePortText(() =>
            {
                using OpenVinoSharp.PartialShape inputShape = input.get_partial_shape();
                return inputShape.ToString();
            }, "shape unavailable");

            return $"{name}: {type}, {shape}";
        }

        private static string DescribeOutput(ulong index, OpenVinoSharp.Output output)
        {
            string name = SafePortName(() => output.get_any_name(), $"output_{index}");
            string type = SafePortText(() => output.get_element_type().ToString(), "type unavailable");
            string shape = SafePortText(() =>
            {
                using OpenVinoSharp.PartialShape outputShape = output.get_partial_shape();
                return outputShape.ToString();
            }, "shape unavailable");

            return $"{name}: {type}, {shape}";
        }

        private ConvertModelSelectionEntry? GetSelectedConvertModelEntry()
        {
            if (cboConvertSourceModel.SelectedItem is not ConvertModelSelectionEntry entry)
            {
                return null;
            }

            return File.Exists(entry.SourcePath) ? entry : null;
        }
        #endregion

        #region Tests
        private enum ImageTensorLayout
        {
            Hwc,
            Chw,
            Nhwc,
            Nchw,
        }

        private sealed class ImageModelRunResult
        {
            public Bitmap? ReturnedImage { get; set; }
            public string GeneratedText { get; set; } = string.Empty;
            public List<string> Messages { get; } = new();
        }

        private Color _imageDropNormalBackColor;

        private async Task TestOpenVinoModelAsync(bool runInference)
        {
            if (!RefreshSelectedRuntimeModelState(out string reason))
            {
                AppendOutput($"Loaded model test unavailable: {reason}");
                return;
            }

            string modelPath = _selectedRuntimeModelPath;
            txtTestModelPath.Text = $"{DescribeRuntimeModelKind(_selectedRuntimeModelKind)} | {modelPath}";
            btnInspectOpenVinoModel.Enabled = false;
            btnRunOpenVinoModelTest.Enabled = false;
            CancellationTokenSource? operation = null;

            try
            {
                if (runInference)
                {
                    AppendOutput("Tests: running the loaded model through its Workstation runtime path.");
                    await RunLoadModelAsync();
                    return;
                }

                if (!TryGetPrimaryCompileTarget(out string device))
                {
                    return;
                }

                operation = await TryBeginLongOperationAsync("Inspecting loaded model...", allowCancel: true);
                if (operation is null)
                {
                    return;
                }
                CancellationToken cancellationToken = operation.Token;

                AppendOutput("Loaded model inspection");
                AppendOutput($"  Folder: {_selectedModelFolder}");
                AppendOutput($"  Type: {DescribeRuntimeModelKind(_selectedRuntimeModelKind)}");
                AppendOutput($"  Runtime path: {modelPath}");
                AppendOutput($"  Target: {device}");

                switch (_selectedRuntimeModelKind)
                {
                    case ModelFileKind.OpenVinoIr:
                    case ModelFileKind.OpenVinoCoreReadable:
                        await Task.Run(() => TestOpenVinoModelCore(modelPath, device, runInference: false, cancellationToken: cancellationToken), cancellationToken);
                        break;
                    case ModelFileKind.OpenVinoCompiledBlob:
                        await Task.Run(() =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            InspectCompiledBlobCore(modelPath, device);
                            cancellationToken.ThrowIfCancellationRequested();
                        }, cancellationToken);
                        break;
                    case ModelFileKind.Gguf:
                    case ModelFileKind.OpenVinoGenAiDirectory:
                        AppendOutput($"  GenAI runtime: {(ShouldUseVisionGenAiPipeline(_selectedRuntimeModelKind, modelPath) ? "VLMPipeline" : "LLMPipeline")}");
                        AppendOutput("  Status: model was already load-and-generation validated on Workstation.");
                        AppendOutput("  Click Send Message to load it again and generate a short response.");
                        break;
                    default:
                        AppendOutput("  Status: unsupported model type.");
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                AppendOutput("Loaded model inspection canceled.");
            }
            catch (Exception ex)
            {
                AppendOutput("OpenVINO model test failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"  {line}");
                }
            }
            finally
            {
                btnInspectOpenVinoModel.Enabled = true;
                btnRunOpenVinoModelTest.Enabled = true;
                if (operation is not null)
                {
                    FinishLongOperation();
                }
            }
        }

        private void InspectCompiledBlobCore(string blobPath, string device)
        {
            ConfigureOpenVinoRuntimeSearchPath();
            using OpenVinoSharp.Core core = new();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using OpenVinoSharp.CompiledModel compiledModel = core.ImportModel(blobPath, device);
            stopwatch.Stop();
            AppendOutput($"  Import time: {stopwatch.ElapsedMilliseconds:N0} ms");
            AppendOutput($"  Inputs: {compiledModel.InputCount}");
            AppendOutput($"  Outputs: {compiledModel.OutputCount}");
            AppendCompiledModelProperty(compiledModel, OpenVinoSharp.PropertyKeys.ModelName, "Compiled model");
            AppendCompiledModelProperty(compiledModel, OpenVinoSharp.PropertyKeys.OptimalNumberOfInferRequests, "Optimal infer requests");
            AppendOutput("  Status: compiled model imported successfully.");
        }

        private async void btnInspectOpenVinoModel_Click(object? sender, EventArgs e)
        {
            await TestOpenVinoModelAsync(runInference: false);
        }

        private async void btnRunOpenVinoModelTest_Click(object? sender, EventArgs e)
        {
            await TestOpenVinoModelAsync(runInference: true);
        }

        private async Task RunImageTestAsync()
        {
            bool modelLoaded = _loadedRuntimeModelKind != ModelFileKind.Unknown
                && !string.IsNullOrWhiteSpace(_loadedRuntimeModelPath);
            if (!modelLoaded)
            {
                AppendOutput("Image test unavailable: load a Workstation model first.");
                return;
            }

            if (!_loadedModelImageTestAvailable)
            {
                AppendOutput("Image test unavailable: the loaded model does not expose a usable image input for this app.");
                return;
            }

            if (!TryGetSelectedImagePath(out string imagePath, out string imageReason))
            {
                AppendOutput($"Image test unavailable: {imageReason}");
                return;
            }

            if (!TryGetPrimaryCompileTarget(out string device))
            {
                return;
            }

            string modelPath = _loadedRuntimeModelPath;
            ModelFileKind modelKind = _loadedRuntimeModelKind;
            string prompt = string.IsNullOrWhiteSpace(txtInferText.Text)
                ? "Describe this image."
                : txtInferText.Text.Trim();

            CancellationTokenSource? operation = await TryBeginLongOperationAsync("Running image test...", allowCancel: true);
            if (operation is null)
            {
                return;
            }

            CancellationToken cancellationToken = operation.Token;
            btnRunImageTest.Enabled = false;
            btnBrowseInferImage.Enabled = false;
            btnRunOpenVinoModelTest.Enabled = false;
            SetImageResultPreview(null);

            try
            {
                AppendOutput("Image test");
                AppendOutput($"  Model: {modelPath}");
                AppendOutput($"  Image: {imagePath}");
                AppendOutput($"  Target: {device}");

                ImageModelRunResult? result = null;
                switch (modelKind)
                {
                    case ModelFileKind.OpenVinoGenAiDirectory:
                        SetActivityState("Loading vision model...", allowCancel: true);
                        result = await Task.Run(() => RunGenAiVisionImageTestSafely(
                            modelPath,
                            device,
                            imagePath,
                            prompt,
                            new Progress<string>(message => SetActivityState(message, allowCancel: true)),
                            cancellationToken), cancellationToken);
                        break;

                    case ModelFileKind.OpenVinoIr:
                    case ModelFileKind.OpenVinoCoreReadable:
                        SetActivityState("Running image through OpenVINO model...", allowCancel: true);
                        result = await Task.Run(() => RunOpenVinoCoreImageTest(
                            modelPath,
                            device,
                            imagePath,
                            prompt,
                            cancellationToken), cancellationToken);
                        break;

                    case ModelFileKind.OpenVinoCompiledBlob:
                        SetActivityState("Running image through device-optimized model...", allowCancel: true);
                        result = await Task.Run(() => RunOpenVinoCompiledBlobImageTest(
                            modelPath,
                            device,
                            imagePath,
                            prompt,
                            cancellationToken), cancellationToken);
                        break;

                    default:
                        AppendOutput($"  This model type does not support image testing here: {DescribeRuntimeModelKind(modelKind)}");
                        break;
                }

                if (result is not null)
                {
                    foreach (string message in result.Messages)
                    {
                        AppendOutput($"  {message}");
                    }

                    if (!string.IsNullOrWhiteSpace(result.GeneratedText))
                    {
                        AppendOutput($"  Output: {TrimForOutput(result.GeneratedText, 1200)}");
                    }

                    if (result.ReturnedImage is not null)
                    {
                        SetImageResultPreview(result.ReturnedImage);
                        result.ReturnedImage = null;
                    }
                    else if (string.IsNullOrWhiteSpace(result.GeneratedText))
                    {
                        AppendOutput("  No returned image was found to preview.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                AppendOutput("Image test canceled.");
            }
            catch (Exception ex)
            {
                AppendOutput("Image test failed:");
                foreach (string line in FormatException(ex))
                {
                    AppendOutput($"  {line}");
                }
            }
            finally
            {
                FinishLongOperation();
                ApplySelectedModelCapabilitiesToActiveTab(reportStatus: false);
            }
        }

        private bool TryGetSelectedImagePath(out string imagePath, out string reason)
        {
            imagePath = string.Empty;
            string candidate = txtInferImage.Text.Trim();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                reason = "choose or drop an image first.";
                return false;
            }

            try
            {
                string fullPath = Path.GetFullPath(candidate);
                string extension = Path.GetExtension(fullPath).ToLowerInvariant();
                if (!File.Exists(fullPath))
                {
                    reason = $"image file was not found: {candidate}";
                    return false;
                }

                if (extension is not (".jpg" or ".jpeg" or ".png" or ".bmp"))
                {
                    reason = "choose a JPG, JPEG, PNG, or BMP image.";
                    return false;
                }

                imagePath = fullPath;
                reason = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                reason = ShortError(ex);
                return false;
            }
        }

        private ImageModelRunResult RunGenAiVisionImageTestSafely(
            string modelPath,
            string device,
            string imagePath,
            string prompt,
            IProgress<string>? progress,
            CancellationToken cancellationToken)
        {
            try
            {
                return RunGenAiVisionImageTest(modelPath, device, imagePath, prompt, progress, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("OpenVINO GenAI could not run this image-and-text model in the current runtime.", ex);
            }
        }

        private ImageModelRunResult RunGenAiVisionImageTest(
            string modelPath,
            string device,
            string imagePath,
            string prompt,
            IProgress<string>? progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ImageModelRunResult result = new();
            if (!EnsureOpenVinoGenAiRuntimeAvailable(out string reason))
            {
                result.Messages.Add($"OpenVINO GenAI unavailable: {reason}");
                return result;
            }

            using OpenVinoSharp.Tensor imageTensor = CreateRgbImageTensorForVisionPipeline(imagePath, out string imageDescription);
            result.Messages.Add(imageDescription);

            bool reusedLoadedModel = _cachedVlmPipeline is not null
                && string.Equals(_cachedVlmModelPath, modelPath, StringComparison.OrdinalIgnoreCase)
                && string.Equals(_cachedVlmDevice, device, StringComparison.OrdinalIgnoreCase);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            if (!reusedLoadedModel)
            {
                _cachedVlmPipeline?.Dispose();
                _cachedVlmPipeline = null;
                _cachedVlmModelPath = string.Empty;
                _cachedVlmDevice = string.Empty;

                cancellationToken.ThrowIfCancellationRequested();
                _cachedVlmPipeline = new OpenVinoSharp.GenAI.VLMPipeline(modelPath, device);
                _cachedVlmModelPath = modelPath;
                _cachedVlmDevice = device;
            }

            stopwatch.Stop();
            result.Messages.Add(reusedLoadedModel
                ? "Vision model was already loaded."
                : $"Vision model load time: {stopwatch.ElapsedMilliseconds:N0} ms");

            using OpenVinoSharp.GenAI.GenerationConfig config = new();
            config.SetMaxNewTokens(MaxChatResponseTokens);
            config.SetDoSample(false);

            progress?.Report("Generating image answer...");
            int streamedCharacters = 0;
            System.Text.StringBuilder generatedBuilder = new();
            stopwatch.Restart();
            using OpenVinoSharp.GenAI.VLMDecodedResults decodedResults = _cachedVlmPipeline!.Generate(
                prompt,
                new[] { imageTensor },
                config,
                text =>
                {
                    if (cancellationToken.IsCancellationRequested || _longOperationCancelRequested)
                    {
                        return OpenVinoSharp.GenAI.StreamingStatus.Cancel;
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        generatedBuilder.Append(text);
                        streamedCharacters += text.Length;
                        progress?.Report($"Generating image answer... {streamedCharacters:N0} characters");
                    }

                    return OpenVinoSharp.GenAI.StreamingStatus.Running;
                });
            stopwatch.Stop();
            cancellationToken.ThrowIfCancellationRequested();

            result.Messages.Add($"Generate time: {stopwatch.ElapsedMilliseconds:N0} ms");
            result.Messages.Add($"Response limit: {MaxChatResponseTokens} tokens. If the answer ends mid-sentence, the model reached this normal limit; the test did not fail.");
            result.Messages.Add("Vision-language models answer with text, so there is no returned picture to preview.");
            result.GeneratedText = generatedBuilder.Length > 0
                ? generatedBuilder.ToString()
                : decodedResults.GetText();
            return result;
        }

        private ImageModelRunResult RunOpenVinoCoreImageTest(
            string modelPath,
            string device,
            string imagePath,
            string prompt,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ImageModelRunResult result = new();
            ConfigureOpenVinoRuntimeSearchPath();

            string sourceFolder = string.IsNullOrWhiteSpace(_selectedModelFolder)
                ? Path.GetDirectoryName(modelPath) ?? modelPath
                : _selectedModelFolder;
            string blobPath = GetOptimizedBlobPath(sourceFolder, modelPath, device);
            if (!File.Exists(blobPath))
            {
                throw new InvalidOperationException(
                    $"Missing blob file for this source model and {device}: {blobPath}. Use the Convert tab to create the blob file first, then select the optimized model folder and run the image test again.");
            }

            using OpenVinoSharp.Core core = new();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using OpenVinoSharp.CompiledModel cachedCompiledModel = core.ImportModel(blobPath, device);
            stopwatch.Stop();
            cancellationToken.ThrowIfCancellationRequested();
            result.Messages.Add($"Used blob file: {blobPath}");
            result.Messages.Add($"Load time: {stopwatch.ElapsedMilliseconds:N0} ms");
            return RunImageThroughCompiledModel(cachedCompiledModel, imagePath, prompt, result, cancellationToken);
        }

        private ImageModelRunResult RunOpenVinoCompiledBlobImageTest(
            string blobPath,
            string device,
            string imagePath,
            string prompt,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ImageModelRunResult result = new();
            ConfigureOpenVinoRuntimeSearchPath();

            using OpenVinoSharp.Core core = new();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using OpenVinoSharp.CompiledModel compiledModel = core.ImportModel(blobPath, device);
            stopwatch.Stop();
            cancellationToken.ThrowIfCancellationRequested();
            result.Messages.Add($"Load time: {stopwatch.ElapsedMilliseconds:N0} ms");
            return RunImageThroughCompiledModel(compiledModel, imagePath, prompt, result, cancellationToken);
        }

        private ImageModelRunResult RunImageThroughCompiledModel(
            OpenVinoSharp.CompiledModel compiledModel,
            string imagePath,
            string prompt,
            ImageModelRunResult result,
            CancellationToken cancellationToken)
        {
            using OpenVinoSharp.InferRequest request = compiledModel.CreateInferRequest();
            List<OpenVinoSharp.Tensor> inputTensors = new();
            try
            {
                if (!TryAttachImageInputTensors(compiledModel, request, imagePath, prompt, inputTensors, result.Messages, out string reason))
                {
                    result.Messages.Add($"Image test skipped: {reason}");
                    return result;
                }

                cancellationToken.ThrowIfCancellationRequested();
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                request.Infer();
                stopwatch.Stop();
                cancellationToken.ThrowIfCancellationRequested();
                result.Messages.Add($"Run time: {stopwatch.ElapsedMilliseconds:N0} ms");

                if (TryDecodeFirstReturnedImage(compiledModel, request, out Bitmap? returnedImage, out string outputDescription, out string decodeReason)
                    && returnedImage is not null)
                {
                    result.ReturnedImage = returnedImage;
                    result.Messages.Add($"Returned image decoded from {outputDescription}.");
                }
                else
                {
                    result.Messages.Add($"No returned image preview: {decodeReason}");
                }
            }
            finally
            {
                foreach (OpenVinoSharp.Tensor tensor in inputTensors)
                {
                    tensor.Dispose();
                }
            }

            return result;
        }

        private void TestOpenVinoModelCore(
            string modelPath,
            string device,
            bool runInference,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AppendOutput($"OpenVINO model test - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            AppendOutput($"  Model: {modelPath}");
            AppendOutput($"  Device: {device}");

            ConfigureOpenVinoRuntimeSearchPath();
            cancellationToken.ThrowIfCancellationRequested();

            string? binPath = FindMatchingBinPath(modelPath);
            using OpenVinoSharp.Core core = new();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using OpenVinoSharp.Model model = string.IsNullOrWhiteSpace(binPath)
                ? core.ReadModelUnicode(modelPath)
                : core.ReadModelUnicode(modelPath, binPath);
            stopwatch.Stop();
            cancellationToken.ThrowIfCancellationRequested();

            AppendOutput($"  Friendly name: {model.get_friendly_name()}");
            AppendOutput($"  Read time: {stopwatch.ElapsedMilliseconds:N0} ms");
            if (!string.IsNullOrWhiteSpace(binPath))
            {
                AppendOutput($"  Weights: {binPath}");
            }

            AppendOutput("  Inputs:");
            foreach (string input in DescribeModelInputs(model))
            {
                AppendOutput($"    {input}");
            }

            AppendOutput("  Outputs:");
            foreach (string output in DescribeModelOutputs(model))
            {
                AppendOutput($"    {output}");
            }

            Dictionary<string, string> compileProperties = new()
            {
                [OpenVinoSharp.PropertyKeys.EnableProfiling] = "YES",
            };

            stopwatch.Restart();
            using OpenVinoSharp.CompiledModel compiledModel = core.CompileModel(model, device, compileProperties);
            stopwatch.Stop();
            cancellationToken.ThrowIfCancellationRequested();
            AppendOutput($"  Compile time: {stopwatch.ElapsedMilliseconds:N0} ms");
            AppendCompiledModelProperty(compiledModel, OpenVinoSharp.PropertyKeys.ModelName, "Compiled model");
            AppendCompiledModelProperty(compiledModel, OpenVinoSharp.PropertyKeys.OptimalNumberOfInferRequests, "Optimal infer requests");
            AppendCompiledModelProperty(compiledModel, OpenVinoSharp.PropertyKeys.SupportedProperties, "Supported properties");

            if (!runInference)
            {
                AppendOutput("  Status: inspected and compiled. Use Run + Profile to execute one zero-input inference where possible.");
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            using OpenVinoSharp.InferRequest request = compiledModel.CreateInferRequest();
            List<OpenVinoSharp.Tensor> inputTensors = new();
            try
            {
                if (!TryAttachDefaultInputTensors(compiledModel, request, inputTensors, out string reason))
                {
                    AppendOutput($"  Inference skipped: {reason}");
                    return;
                }

                stopwatch.Restart();
                request.Infer();
                stopwatch.Stop();
                cancellationToken.ThrowIfCancellationRequested();
                AppendOutput($"  Inference time: {stopwatch.ElapsedMilliseconds:N0} ms");
                AppendProfilingInfo(request);
            }
            finally
            {
                foreach (OpenVinoSharp.Tensor tensor in inputTensors)
                {
                    tensor.Dispose();
                }
            }
        }

        private static bool TryAttachDefaultInputTensors(
            OpenVinoSharp.CompiledModel compiledModel,
            OpenVinoSharp.InferRequest request,
            List<OpenVinoSharp.Tensor> inputTensors,
            out string reason)
        {
            ulong inputCount = compiledModel.InputCount;
            for (ulong i = 0; i < inputCount; i++)
            {
                using OpenVinoSharp.Input input = compiledModel.GetInput(i);
                using OpenVinoSharp.PartialShape partialShape = input.get_partial_shape();
                if (!partialShape.is_static())
                {
                    reason = $"input {i} has dynamic shape {partialShape}. The generic one-shot runner cannot safely choose token, batch, or sequence dimensions; use a model-specific pipeline or reshape the model first.";
                    return false;
                }

                using OpenVinoSharp.Shape shape = partialShape.to_shape();
                long elements = shape.get_total_elements();
                if (elements <= 0)
                {
                    reason = $"input {i} has a zero-sized shape: {shape}";
                    return false;
                }

                if (elements > 50_000_000)
                {
                    reason = $"input {i} is too large for a safe default allocation: {shape}";
                    return false;
                }

                OpenVinoSharp.ElementType elementType = input.get_element_type().get_type();
                if (!TryCreateDefaultInputTensor(shape, elementType, string.Empty, out OpenVinoSharp.Tensor? tensor, out reason)
                    || tensor is null)
                {
                    return false;
                }

                inputTensors.Add(tensor);
                request.SetInputTensor(i, tensor);
            }

            reason = string.Empty;
            return true;
        }

        private static bool TryAttachImageInputTensors(
            OpenVinoSharp.CompiledModel compiledModel,
            OpenVinoSharp.InferRequest request,
            string imagePath,
            string prompt,
            List<OpenVinoSharp.Tensor> inputTensors,
            List<string> messages,
            out string reason)
        {
            ulong inputCount = compiledModel.InputCount;
            bool imageAttached = false;
            bool promptAttached = false;
            bool promptNeedsTokenizer = false;

            for (ulong i = 0; i < inputCount; i++)
            {
                using OpenVinoSharp.Input input = compiledModel.GetInput(i);
                using OpenVinoSharp.PartialShape partialShape = input.get_partial_shape();
                if (!partialShape.is_static())
                {
                    reason = $"input {i} has dynamic shape {partialShape}. This image test needs a static image size or a model-specific pipeline.";
                    return false;
                }

                using OpenVinoSharp.Shape shape = partialShape.to_shape();
                OpenVinoSharp.ElementType elementType = input.get_element_type().get_type();
                OpenVinoSharp.Tensor? tensor = null;

                if (!imageAttached && LooksLikeImageInput(input))
                {
                    if (!TryCreateImageTensorForModelInput(
                        shape,
                        elementType,
                        imagePath,
                        out tensor,
                        out string imageDescription,
                        out reason)
                        || tensor is null)
                    {
                        return false;
                    }

                    imageAttached = true;
                    messages.Add($"Image input: {SafePortText(() => input.get_any_name(), $"input {i}")}; {imageDescription}");
                }
                else if (elementType == OpenVinoSharp.ElementType.STRING)
                {
                    if (!TryCreateDefaultInputTensor(shape, elementType, prompt, out tensor, out reason)
                        || tensor is null)
                    {
                        return false;
                    }

                    promptAttached = true;
                    messages.Add($"Test Text sent to: {SafePortText(() => input.get_any_name(), $"input {i}")}");
                }
                else
                {
                    if (LooksLikeTextInput(input))
                    {
                        promptNeedsTokenizer = true;
                    }

                    if (!TryCreateDefaultInputTensor(shape, elementType, string.Empty, out tensor, out reason)
                        || tensor is null)
                    {
                        return false;
                    }
                }

                inputTensors.Add(tensor);
                request.SetInputTensor(i, tensor);
            }

            if (!imageAttached)
            {
                reason = "no image input was found on the compiled model.";
                return false;
            }

            if (promptNeedsTokenizer && !promptAttached && !string.IsNullOrWhiteSpace(prompt))
            {
                messages.Add("Test Text was not used because this Core model expects token IDs, not plain text. Use a model-specific text/image pipeline for prompt-driven image editing.");
            }
            else if (!promptAttached && !string.IsNullOrWhiteSpace(prompt))
            {
                messages.Add("Test Text was not used because the model did not expose a plain-text input.");
            }

            reason = string.Empty;
            return true;
        }

        private static bool TryCreateDefaultInputTensor(
            OpenVinoSharp.Shape shape,
            OpenVinoSharp.ElementType elementType,
            string textValue,
            out OpenVinoSharp.Tensor? tensor,
            out string reason)
        {
            tensor = null;
            long elements = shape.get_total_elements();
            if (elements <= 0)
            {
                reason = $"input has a zero-sized shape: {shape}";
                return false;
            }

            if (elements > 50_000_000)
            {
                reason = $"input is too large for a safe default allocation: {shape}";
                return false;
            }

            if (elementType == OpenVinoSharp.ElementType.UNDEFINED || elementType == OpenVinoSharp.ElementType.DYNAMIC)
            {
                reason = $"input has unsupported element type: {elementType}";
                return false;
            }

            if (elementType == OpenVinoSharp.ElementType.STRING)
            {
                if (elements > 1_000_000)
                {
                    reason = $"string input is too large for a safe default allocation: {shape}";
                    return false;
                }

                string[] values = Enumerable.Repeat(textValue, checked((int)elements)).ToArray();
                tensor = new OpenVinoSharp.Tensor(shape, values);
                reason = string.Empty;
                return true;
            }

            tensor = new OpenVinoSharp.Tensor(shape, elementType);
            reason = string.Empty;
            return true;
        }

        private static OpenVinoSharp.Tensor CreateRgbImageTensorForVisionPipeline(string imagePath, out string description)
        {
            using Bitmap bitmap = LoadRgbBitmapForVisionPipeline(imagePath, out bool resized);
            // OpenVINO GenAI VLM image tensors are batched NHWC: 1 x height x width x 3.
            using OpenVinoSharp.Shape shape = new(new long[] { 1, bitmap.Height, bitmap.Width, 3 });
            OpenVinoSharp.Tensor tensor = new(shape, OpenVinoSharp.ElementType.U8);
            Span<byte> data = tensor.AsSpan<byte>();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    int offset = ((y * bitmap.Width) + x) * 3;
                    data[offset] = color.R;
                    data[offset + 1] = color.G;
                    data[offset + 2] = color.B;
                }
            }

            description = resized
                ? $"Image prepared as RGB {bitmap.Width} x {bitmap.Height} for the vision model."
                : $"Image prepared as RGB {bitmap.Width} x {bitmap.Height}.";
            return tensor;
        }

        private static Bitmap LoadRgbBitmapForVisionPipeline(string imagePath, out bool resized)
        {
            const int maxEdge = 2048;
            using Bitmap source = new(imagePath);
            int targetWidth = source.Width;
            int targetHeight = source.Height;
            resized = false;

            int longestEdge = Math.Max(source.Width, source.Height);
            if (longestEdge > maxEdge)
            {
                double scale = maxEdge / (double)longestEdge;
                targetWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
                targetHeight = Math.Max(1, (int)Math.Round(source.Height * scale));
                resized = true;
            }

            return CreateResizedRgbBitmap(source, targetWidth, targetHeight);
        }

        private static bool TryCreateImageTensorForModelInput(
            OpenVinoSharp.Shape shape,
            OpenVinoSharp.ElementType elementType,
            string imagePath,
            out OpenVinoSharp.Tensor? tensor,
            out string description,
            out string reason)
        {
            tensor = null;
            description = string.Empty;
            long[] dims = shape.get_dims();
            if (!TryGetImageTensorInfo(dims, out ImageTensorLayout layout, out int width, out int height, out int channels, out int batches))
            {
                reason = $"image input shape is not recognized: {shape}";
                return false;
            }

            long elements = shape.get_total_elements();
            if (elements <= 0 || elements > 200_000_000)
            {
                reason = $"image input is too large for a safe allocation: {shape}";
                return false;
            }

            using Bitmap bitmap = LoadExactRgbBitmap(imagePath, width, height);
            switch (elementType)
            {
                case OpenVinoSharp.ElementType.F32:
                    float[] floatData = new float[checked((int)elements)];
                    FillImageData(floatData, bitmap, layout, width, height, channels, batches);
                    tensor = new OpenVinoSharp.Tensor(shape, floatData);
                    break;

                case OpenVinoSharp.ElementType.F16:
                    tensor = new OpenVinoSharp.Tensor(shape, elementType);
                    FillImageData(tensor.AsSpan<Half>(), bitmap, layout, width, height, channels, batches);
                    break;

                case OpenVinoSharp.ElementType.U8:
                    tensor = new OpenVinoSharp.Tensor(shape, elementType);
                    FillImageData(tensor.AsSpan<byte>(), bitmap, layout, width, height, channels, batches);
                    break;

                default:
                    reason = $"image input type {elementType} is not supported by the generic image test.";
                    return false;
            }

            description = $"resized to {width} x {height}, {channels} channel(s), {layout}, {elementType}.";
            if (batches > 1)
            {
                description += $" The same image was copied into {batches} batches.";
            }

            reason = string.Empty;
            return true;
        }

        private static Bitmap LoadExactRgbBitmap(string imagePath, int width, int height)
        {
            using Bitmap source = new(imagePath);
            return CreateResizedRgbBitmap(source, width, height);
        }

        private static Bitmap CreateResizedRgbBitmap(Bitmap source, int width, int height)
        {
            Bitmap target = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using Graphics graphics = Graphics.FromImage(target);
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.DrawImage(source, new Rectangle(0, 0, width, height));
            return target;
        }

        private static void FillImageData(float[] data, Bitmap bitmap, ImageTensorLayout layout, int width, int height, int channels, int batches)
        {
            for (int batch = 0; batch < batches; batch++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        for (int channel = 0; channel < channels; channel++)
                        {
                            data[checked((int)GetImageTensorIndex(layout, batch, channel, y, x, channels, height, width))] =
                                GetImageChannelByte(color, channel, channels);
                        }
                    }
                }
            }
        }

        private static void FillImageData(Span<byte> data, Bitmap bitmap, ImageTensorLayout layout, int width, int height, int channels, int batches)
        {
            for (int batch = 0; batch < batches; batch++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        for (int channel = 0; channel < channels; channel++)
                        {
                            data[checked((int)GetImageTensorIndex(layout, batch, channel, y, x, channels, height, width))] =
                                GetImageChannelByte(color, channel, channels);
                        }
                    }
                }
            }
        }

        private static void FillImageData(Span<Half> data, Bitmap bitmap, ImageTensorLayout layout, int width, int height, int channels, int batches)
        {
            for (int batch = 0; batch < batches; batch++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        for (int channel = 0; channel < channels; channel++)
                        {
                            data[checked((int)GetImageTensorIndex(layout, batch, channel, y, x, channels, height, width))] =
                                (Half)GetImageChannelByte(color, channel, channels);
                        }
                    }
                }
            }
        }

        private static byte GetImageChannelByte(Color color, int channel, int channels)
        {
            if (channels == 1)
            {
                return (byte)Math.Clamp((int)Math.Round((color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114)), 0, 255);
            }

            return channel switch
            {
                0 => color.R,
                1 => color.G,
                2 => color.B,
                3 => color.A,
                _ => (byte)0,
            };
        }

        private static bool TryDecodeFirstReturnedImage(
            OpenVinoSharp.CompiledModel compiledModel,
            OpenVinoSharp.InferRequest request,
            out Bitmap? returnedImage,
            out string outputDescription,
            out string reason)
        {
            returnedImage = null;
            outputDescription = string.Empty;
            List<string> skippedOutputs = new();

            for (ulong i = 0; i < compiledModel.OutputCount; i++)
            {
                using OpenVinoSharp.Output output = compiledModel.GetOutput(i);
                using OpenVinoSharp.Tensor outputTensor = request.GetOutputTensor(i);
                string label = SafePortText(() => output.get_any_name(), $"output {i}");
                if (TryDecodeImageTensor(outputTensor, out returnedImage, out reason)
                    && returnedImage is not null)
                {
                    outputDescription = $"{label} ({reason})";
                    return true;
                }

                skippedOutputs.Add($"{label}: {reason}");
            }

            reason = skippedOutputs.Count == 0
                ? "the model did not report any outputs."
                : string.Join("; ", skippedOutputs.Take(4));
            return false;
        }

        private static bool TryDecodeImageTensor(OpenVinoSharp.Tensor tensor, out Bitmap? image, out string reason)
        {
            image = null;
            using OpenVinoSharp.Shape shape = tensor.shape;
            long[] dims = shape.get_dims();
            if (!TryGetImageTensorInfo(dims, out ImageTensorLayout layout, out int width, out int height, out int channels, out int batches))
            {
                reason = $"shape {shape} is not image-shaped.";
                return false;
            }

            if (width > 8192 || height > 8192)
            {
                reason = $"image output is too large to preview safely: {width} x {height}.";
                return false;
            }

            float[] values;
            OpenVinoSharp.ElementType elementType = tensor.element_type;
            switch (elementType)
            {
                case OpenVinoSharp.ElementType.F32:
                    values = tensor.get_float_data();
                    break;

                case OpenVinoSharp.ElementType.F16:
                    ReadOnlySpan<Half> halfValues = tensor.AsReadOnlySpan<Half>();
                    values = new float[halfValues.Length];
                    for (int i = 0; i < halfValues.Length; i++)
                    {
                        values[i] = (float)halfValues[i];
                    }
                    break;

                case OpenVinoSharp.ElementType.U8:
                    byte[] byteValues = tensor.get_byte_data();
                    values = new float[byteValues.Length];
                    for (int i = 0; i < byteValues.Length; i++)
                    {
                        values[i] = byteValues[i];
                    }
                    break;

                default:
                    reason = $"output type {elementType} is not supported for image preview.";
                    return false;
            }

            if (values.Length == 0)
            {
                reason = "output tensor is empty.";
                return false;
            }

            float min = values.Min();
            float max = values.Max();
            image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r = DecodeOutputChannel(values, layout, 0, y, x, channels, height, width, min, max, elementType);
                    byte g = channels == 1
                        ? r
                        : DecodeOutputChannel(values, layout, 1, y, x, channels, height, width, min, max, elementType);
                    byte b = channels == 1
                        ? r
                        : DecodeOutputChannel(values, layout, 2, y, x, channels, height, width, min, max, elementType);
                    image.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            reason = $"{width} x {height}, {channels} channel(s), {layout}, {elementType}, batch 1 of {batches}";
            return true;
        }

        private static byte DecodeOutputChannel(
            float[] values,
            ImageTensorLayout layout,
            int channel,
            int y,
            int x,
            int channels,
            int height,
            int width,
            float min,
            float max,
            OpenVinoSharp.ElementType elementType)
        {
            int index = checked((int)GetImageTensorIndex(layout, 0, Math.Min(channel, channels - 1), y, x, channels, height, width));
            float value = values[index];
            if (elementType != OpenVinoSharp.ElementType.U8)
            {
                if (min >= 0 && max <= 1.1f)
                {
                    value *= 255f;
                }
                else if (min >= -1.1f && max <= 1.1f)
                {
                    value = (value + 1f) * 127.5f;
                }
            }

            return (byte)Math.Clamp((int)Math.Round(value), 0, 255);
        }

        private static bool TryGetImageTensorInfo(
            long[] dims,
            out ImageTensorLayout layout,
            out int width,
            out int height,
            out int channels,
            out int batches)
        {
            layout = ImageTensorLayout.Hwc;
            width = 0;
            height = 0;
            channels = 0;
            batches = 1;

            if (dims.Any(dim => dim <= 0 || dim > int.MaxValue))
            {
                return false;
            }

            if (dims.Length == 4)
            {
                batches = checked((int)dims[0]);
                if (IsImageChannelLength(dims[1]))
                {
                    layout = ImageTensorLayout.Nchw;
                    channels = checked((int)dims[1]);
                    height = checked((int)dims[2]);
                    width = checked((int)dims[3]);
                    return width > 0 && height > 0 && batches > 0;
                }

                if (IsImageChannelLength(dims[3]))
                {
                    layout = ImageTensorLayout.Nhwc;
                    height = checked((int)dims[1]);
                    width = checked((int)dims[2]);
                    channels = checked((int)dims[3]);
                    return width > 0 && height > 0 && batches > 0;
                }
            }
            else if (dims.Length == 3)
            {
                if (IsImageChannelLength(dims[0]))
                {
                    layout = ImageTensorLayout.Chw;
                    channels = checked((int)dims[0]);
                    height = checked((int)dims[1]);
                    width = checked((int)dims[2]);
                    return width > 0 && height > 0;
                }

                if (IsImageChannelLength(dims[2]))
                {
                    layout = ImageTensorLayout.Hwc;
                    height = checked((int)dims[0]);
                    width = checked((int)dims[1]);
                    channels = checked((int)dims[2]);
                    return width > 0 && height > 0;
                }
            }

            return false;
        }

        private static bool IsImageChannelLength(long length) => length is 1 or 3 or 4;

        private static long GetImageTensorIndex(
            ImageTensorLayout layout,
            int batch,
            int channel,
            int y,
            int x,
            int channels,
            int height,
            int width)
        {
            return layout switch
            {
                ImageTensorLayout.Nchw => (((long)batch * channels + channel) * height + y) * width + x,
                ImageTensorLayout.Nhwc => (((long)batch * height + y) * width + x) * channels + channel,
                ImageTensorLayout.Chw => ((long)channel * height + y) * width + x,
                _ => ((long)y * width + x) * channels + channel,
            };
        }

        private void SetImageResultPreview(Bitmap? image)
        {
            if (picImageResult.InvokeRequired)
            {
                picImageResult.BeginInvoke(() => SetImageResultPreview(image));
                return;
            }

            Image? oldImage = picImageResult.Image;
            picImageResult.Image = image;
            oldImage?.Dispose();
        }

        private void AppendCompiledModelProperty(OpenVinoSharp.CompiledModel compiledModel, string key, string label)
        {
            try
            {
                string value = compiledModel.GetProperty(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    AppendOutput($"  {label}: {FormatPropertyValue(value)}");
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"  {label}: unavailable ({ShortError(ex)})");
            }
        }

        private void AppendProfilingInfo(OpenVinoSharp.InferRequest request)
        {
            OpenVinoSharp.ProfilingInfo[] profiling = request.GetProfilingInfo();
            if (profiling.Length == 0)
            {
                AppendOutput("  Profiling: no rows reported");
                return;
            }

            AppendOutput($"  Profiling rows: {profiling.Length}");
            foreach (OpenVinoSharp.ProfilingInfo row in profiling
                .Where(item => item.status == OpenVinoSharp.ProfilingInfo.Status.EXECUTED)
                .OrderByDescending(item => item.real_time)
                .Take(12))
            {
                AppendOutput($"    {row.real_time,8:N0} us  {row.node_type}  {row.exec_type}  {row.node_name}");
            }
        }

        private void btnBrowseInferImage_Click(object? sender, EventArgs e) => BrowseForInputImage(txtInferImage);
        private async void btnRunImageTest_Click(object? sender, EventArgs e) => await RunImageTestAsync();
        private void btnBrowseInferLabels_Click(object? sender, EventArgs e) => BrowseForTextFile(txtInferLabels);
        private void cboInferPreset_SelectedIndexChanged(object? sender, EventArgs e) => AppendOutput($"Inference preset: {cboInferPreset.Text}");
        private void InitializeImageDropZone()
        {
            _imageDropNormalBackColor = txtInferImage.BackColor;
        }

        private void txtInferImage_DragEnter(object? sender, DragEventArgs e)
        {
            bool acceptsImage = txtInferImage.Enabled && TryGetSingleDroppedImage(e.Data, out string? _);
            e.Effect = acceptsImage ? DragDropEffects.Copy : DragDropEffects.None;
            txtInferImage.BackColor = acceptsImage
                ? ControlPaint.Light(_imageDropNormalBackColor, 0.25f)
                : _imageDropNormalBackColor;
        }

        private void txtInferImage_DragLeave(object? sender, EventArgs e) =>
            txtInferImage.BackColor = _imageDropNormalBackColor;

        private void txtInferImage_DragDrop(object? sender, DragEventArgs e)
        {
            txtInferImage.BackColor = _imageDropNormalBackColor;
            if (TryGetSingleDroppedImage(e.Data, out string? imagePath) && imagePath is not null)
            {
                SetInferenceImage(imagePath);
            }
            else
            {
                AppendOutput("Image drop rejected: drop one JPG, JPEG, PNG, or BMP file.");
            }
        }

        private static bool TryGetSingleDroppedImage(IDataObject? data, out string? imagePath)
        {
            imagePath = null;
            if (data?.GetData(DataFormats.FileDrop) is not string[] { Length: 1 } files)
            {
                return false;
            }

            string candidate = files[0];
            string extension = Path.GetExtension(candidate).ToLowerInvariant();
            if (!File.Exists(candidate) || extension is not (".jpg" or ".jpeg" or ".png" or ".bmp"))
            {
                return false;
            }

            imagePath = Path.GetFullPath(candidate);
            return true;
        }

        private void SetInferenceImage(string imagePath)
        {
            string fullPath = Path.GetFullPath(imagePath);
            string extension = Path.GetExtension(fullPath).ToLowerInvariant();
            if (!File.Exists(fullPath) || extension is not (".jpg" or ".jpeg" or ".png" or ".bmp"))
            {
                AppendOutput($"Image selection rejected: unsupported or missing file: {imagePath}");
                return;
            }

            txtInferImage.Text = fullPath;
            AppendOutput($"Image selected for the next model run: {fullPath}");
        }

        private void lblInferText_Click(object sender, EventArgs e)
        {

        }

        private void lblInferPreset_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region Server tests
        private static readonly System.Net.Http.HttpClient EndpointTestClient = CreateEndpointTestClient();
        private static System.Net.Http.HttpClient CreateEndpointTestClient()
        {
            System.Net.Http.HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(30),
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OpenVINO-Workbench-Endpoint-Test/0.1");
            return client;
        }

        private async Task TestEndpointAsync()
        {
            string endpoint = txtTestEndpoint.Text.Trim();
            if (!TryCreateAbsoluteUri(endpoint, out Uri? uri) || uri is null)
            {
                AppendOutput($"Server tags test: invalid URL `{endpoint}`.");
                return;
            }

            if (!CanSendServerTest(uri, "Server tags test"))
            {
                return;
            }

            btnTestEndpoint.Enabled = false;
            try
            {
                AppendOutput($"Server tags test: GET {uri}");
                using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, uri);
                AddOptionalBearerToken(request);

                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using System.Net.Http.HttpResponseMessage response = await EndpointTestClient.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();
                stopwatch.Stop();

                AppendOutput($"  Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                AppendOutput($"  Time: {stopwatch.ElapsedMilliseconds:N0} ms");
                AppendOutput($"  Content-Type: {response.Content.Headers.ContentType?.ToString() ?? "(none)"}");
                AppendOutput($"  Body: {TrimForOutput(body, 700)}");
            }
            catch (Exception ex)
            {
                AppendOutput($"Server tags test failed: {ShortError(ex)}");
            }
            finally
            {
                btnTestEndpoint.Enabled = true;
            }
        }

        private async void btnTestEndpoint_Click(object? sender, EventArgs e)
        {
            await TestEndpointAsync();
        }

        private async Task TestModelsEndpointAsync()
        {
            string endpoint = txtModelsEndpoint.Text.Trim();
            if (!TryCreateAbsoluteUri(endpoint, out Uri? uri) || uri is null)
            {
                AppendOutput($"Server models test: invalid URL `{endpoint}`.");
                return;
            }

            if (!CanSendServerTest(uri, "Server models test"))
            {
                return;
            }

            btnTestModelsEndpoint.Enabled = false;
            try
            {
                AppendOutput($"Server models test: GET {uri}");
                using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, uri);
                AddOptionalBearerToken(request);

                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using System.Net.Http.HttpResponseMessage response = await EndpointTestClient.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();
                stopwatch.Stop();

                AppendOutput($"  Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                AppendOutput($"  Time: {stopwatch.ElapsedMilliseconds:N0} ms");
                AppendOutput($"  Content-Type: {response.Content.Headers.ContentType?.ToString() ?? "(none)"}");
                AppendOutput($"  Body: {TrimForOutput(body, 700)}");
                UpdateServerModelDropdownFromModelsResponse(body);
            }
            catch (Exception ex)
            {
                AppendOutput($"Server models test failed: {ShortError(ex)}");
            }
            finally
            {
                btnTestModelsEndpoint.Enabled = true;
            }
        }

        private async void btnTestModelsEndpoint_Click(object? sender, EventArgs e)
        {
            await TestModelsEndpointAsync();
        }

        private async Task TestChatAsync()
        {
            string endpoint = txtChatEndpoint.Text.Trim();
            string model = GetServerTestModelName();
            string prompt = txtChatPrompt.Text.Trim();

            if (!TryCreateAbsoluteUri(endpoint, out Uri? uri) || uri is null)
            {
                AppendOutput($"Server chat test: invalid URL `{endpoint}`.");
                return;
            }

            if (!CanSendServerTest(uri, "Server chat test"))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                AppendOutput("Server chat test: enter a server prompt.");
                return;
            }

            btnTestChat.Enabled = false;
            try
            {
                AppendOutput($"Server chat test: POST {uri}");
                AppendOutput($"  Model: {model}");

                var payload = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt },
                    },
                    temperature = 0.2,
                    stream = false,
                };

                string json = System.Text.Json.JsonSerializer.Serialize(payload);
                using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Post, uri)
                {
                    Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json"),
                };
                AddOptionalBearerToken(request);

                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using System.Net.Http.HttpResponseMessage response = await EndpointTestClient.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();
                stopwatch.Stop();

                AppendOutput($"  Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                AppendOutput($"  Time: {stopwatch.ElapsedMilliseconds:N0} ms");
                AppendChatResponse(body);
            }
            catch (Exception ex)
            {
                AppendOutput($"Server chat test failed: {ShortError(ex)}");
            }
            finally
            {
                btnTestChat.Enabled = true;
            }
        }

        private async void btnTestChat_Click(object? sender, EventArgs e)
        {
            await TestChatAsync();
        }

        private async Task TestTextCompletionAsync()
        {
            string endpoint = txtTextEndpoint.Text.Trim();
            string model = GetServerTestModelName();
            string prompt = txtChatPrompt.Text.Trim();

            if (!TryCreateAbsoluteUri(endpoint, out Uri? uri) || uri is null)
            {
                AppendOutput($"Server text test: invalid URL `{endpoint}`.");
                return;
            }

            if (!CanSendServerTest(uri, "Server text test"))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                AppendOutput("Server text test: enter a server prompt.");
                return;
            }

            btnTestTextEndpoint.Enabled = false;
            try
            {
                AppendOutput($"Server text test: POST {uri}");
                AppendOutput($"  Model: {model}");

                var payload = new
                {
                    model,
                    prompt,
                    max_tokens = (int)MaxChatResponseTokens,
                    temperature = 0.2,
                    stream = false,
                };

                string body = await PostServerJsonAsync(uri, payload);
                AppendServerJsonResponse(body);
            }
            catch (Exception ex)
            {
                AppendOutput($"Server text test failed: {ShortError(ex)}");
            }
            finally
            {
                btnTestTextEndpoint.Enabled = true;
            }
        }

        private async void btnTestTextEndpoint_Click(object? sender, EventArgs e)
        {
            await TestTextCompletionAsync();
        }

        private async Task TestImageEndpointAsync()
        {
            string endpoint = txtImageEndpoint.Text.Trim();
            string model = GetServerTestModelName();
            string prompt = txtChatPrompt.Text.Trim();

            if (!TryCreateAbsoluteUri(endpoint, out Uri? uri) || uri is null)
            {
                AppendOutput($"Server image test: invalid URL `{endpoint}`.");
                return;
            }

            if (!CanSendServerTest(uri, "Server image test"))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                AppendOutput("Server image test: enter a server prompt.");
                return;
            }

            btnTestImageEndpoint.Enabled = false;
            try
            {
                AppendOutput($"Server image test: POST {uri}");
                AppendOutput($"  Model: {model}");

                var payload = new
                {
                    model,
                    prompt,
                    size = "512x512",
                    n = 1,
                };

                string body = await PostServerJsonAsync(uri, payload);
                AppendServerJsonResponse(body);
            }
            catch (Exception ex)
            {
                AppendOutput($"Server image test failed: {ShortError(ex)}");
            }
            finally
            {
                btnTestImageEndpoint.Enabled = true;
            }
        }

        private async void btnTestImageEndpoint_Click(object? sender, EventArgs e)
        {
            await TestImageEndpointAsync();
        }

        private bool CanSendServerTest(Uri uri, string testName)
        {
            if (IsThisAppsLocalServerUri(uri) && !_localApiServer.IsRunning)
            {
                AppendOutput($"{testName}: local server is stopped. Open Workstation and click Start Server first.");
                return false;
            }

            return true;
        }

        private static bool IsThisAppsLocalServerUri(Uri uri)
        {
            bool isConfiguredHost =
                string.Equals(uri.Host, GetLocalModelApiServerAdvertisedHost(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Host, PreferredLocalModelApiServerHost, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase);

            return isConfiguredHost && uri.Port == LocalModelApiServerPort;
        }

        private async Task<string> PostServerJsonAsync(Uri uri, object payload)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(payload);
            using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Post, uri)
            {
                Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            };
            AddOptionalBearerToken(request);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using System.Net.Http.HttpResponseMessage response = await EndpointTestClient.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();

            AppendOutput($"  Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            AppendOutput($"  Time: {stopwatch.ElapsedMilliseconds:N0} ms");
            return body;
        }

        private string GetServerTestModelName()
        {
            SetServerModelNameFromLoadedModelIfBlank();
            string typed = cboServerModel.Text.Trim();
            if (!string.IsNullOrWhiteSpace(typed))
            {
                return typed;
            }

            LocalModelApiSnapshot snapshot = CreateLocalModelApiSnapshot();
            if (!string.IsNullOrWhiteSpace(snapshot.LoadedModelName))
            {
                return snapshot.LoadedModelName;
            }

            string? firstKnownModel = snapshot.Models.FirstOrDefault()?.Name;
            return string.IsNullOrWhiteSpace(firstKnownModel) ? "loaded-model" : firstKnownModel;
        }

        private void SetServerModelNameFromLoadedModelIfBlank()
        {
            if (!string.IsNullOrWhiteSpace(cboServerModel.Text))
            {
                return;
            }

            string loadedModel = CreateLocalModelApiSnapshot().LoadedModelName;
            if (!string.IsNullOrWhiteSpace(loadedModel))
            {
                cboServerModel.Text = loadedModel;
            }
        }

        private void ShowServerSettings()
        {
            string tagsUrl = txtTestEndpoint.Text.Trim();
            string modelsUrl = txtModelsEndpoint.Text.Trim();
            string chatUrl = txtChatEndpoint.Text.Trim();
            string textUrl = txtTextEndpoint.Text.Trim();
            string imageUrl = txtImageEndpoint.Text.Trim();
            string serverModel = cboServerModel.Text.Trim();
            string prompt = txtChatPrompt.Text.Trim();
            string apiKey = txtTestApiKey.Text;

            AppendOutput("Server settings");
            AppendOutput($"  Local server address: {FormatSettingValue(txtLocalServerUrl.Text)}");
            AppendOutput($"  Local server status: {FormatSettingValue(txtLocalServerStatus.Text)}");
            AppendOutput($"  Server tags URL: {FormatSettingValue(tagsUrl)}");
            AppendOutput($"    Check: {DescribeUrlCheck(tagsUrl, expectedPathHint: "/api/tags")}");
            AppendOutput($"  Server models URL: {FormatSettingValue(modelsUrl)}");
            AppendOutput($"    Check: {DescribeUrlCheck(modelsUrl, expectedPathHint: "/v1/models")}");
            AppendOutput($"  Server completions URL: {FormatSettingValue(chatUrl)}");
            AppendOutput($"    Check: {DescribeUrlCheck(chatUrl, expectedPathHint: "/chat/completions")}");
            AppendOutput($"  Server text URL: {FormatSettingValue(textUrl)}");
            AppendOutput($"    Check: {DescribeUrlCheck(textUrl, expectedPathHint: "/completions")}");
            AppendOutput($"  Server image URL: {FormatSettingValue(imageUrl)}");
            AppendOutput($"    Check: {DescribeUrlCheck(imageUrl, expectedPathHint: "/images")}");
            AppendOutput($"  Server model: {FormatSettingValue(serverModel)}");
            AppendOutput($"  Server API key: {(string.IsNullOrWhiteSpace(apiKey) ? "not set" : $"set ({apiKey.Length:N0} characters, hidden)")}");
            AppendOutput($"  Server prompt: {FormatSettingValue(TrimForOutput(prompt, 500))}");
        }

        private void UpdateServerModelDropdownFromModelsResponse(string body)
        {
            IReadOnlyList<string> modelIds;
            try
            {
                modelIds = ReadServerModelIdsFromModelsResponse(body);
            }
            catch (System.Text.Json.JsonException ex)
            {
                AppendOutput($"  Model dropdown: response was not valid model-list JSON ({ShortError(ex)}).");
                return;
            }

            if (modelIds.Count == 0)
            {
                AppendOutput("  Model dropdown: no model names found in the response.");
                return;
            }

            string previous = cboServerModel.Text.Trim();
            string loadedModel = CreateLocalModelApiSnapshot().LoadedModelName;
            cboServerModel.BeginUpdate();
            try
            {
                cboServerModel.Items.Clear();
                foreach (string modelId in modelIds)
                {
                    cboServerModel.Items.Add(modelId);
                }
            }
            finally
            {
                cboServerModel.EndUpdate();
            }

            string selected = modelIds.FirstOrDefault(modelId => modelId.Equals(previous, StringComparison.OrdinalIgnoreCase))
                ?? modelIds.FirstOrDefault(modelId => modelId.Equals(loadedModel, StringComparison.OrdinalIgnoreCase))
                ?? modelIds[0];
            cboServerModel.Text = selected;
            AppendOutput($"  Model dropdown: loaded {modelIds.Count:N0} model name(s).");
        }

        private static IReadOnlyList<string> ReadServerModelIdsFromModelsResponse(string body)
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(body);
            System.Text.Json.JsonElement root = document.RootElement;
            List<string> modelIds = new();

            if (root.ValueKind == System.Text.Json.JsonValueKind.Object
                && root.TryGetProperty("data", out System.Text.Json.JsonElement data)
                && data.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (System.Text.Json.JsonElement item in data.EnumerateArray())
                {
                    AddServerModelId(modelIds, item, "id");
                }
            }

            if (root.ValueKind == System.Text.Json.JsonValueKind.Object
                && root.TryGetProperty("models", out System.Text.Json.JsonElement models)
                && models.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (System.Text.Json.JsonElement item in models.EnumerateArray())
                {
                    AddServerModelId(modelIds, item, "name");
                    AddServerModelId(modelIds, item, "model");
                    AddServerModelId(modelIds, item, "id");
                }
            }

            return modelIds
                .Where(modelId => !string.IsNullOrWhiteSpace(modelId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(modelId => modelId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static void AddServerModelId(List<string> modelIds, System.Text.Json.JsonElement item, string propertyName)
        {
            if (item.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                string? value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    modelIds.Add(value.Trim());
                }

                return;
            }

            if (item.ValueKind == System.Text.Json.JsonValueKind.Object
                && item.TryGetProperty(propertyName, out System.Text.Json.JsonElement property)
                && property.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                string? value = property.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    modelIds.Add(value.Trim());
                }
            }
        }

        private void btnShowServerSettings_Click(object? sender, EventArgs e)
        {
            ShowServerSettings();
        }

        private void AddOptionalBearerToken(System.Net.Http.HttpRequestMessage request)
        {
            string key = txtTestApiKey?.Text.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(key))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
            }
        }

        private void AppendChatResponse(string body)
        {
            try
            {
                using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(body);
                System.Text.Json.JsonElement root = document.RootElement;
                if (root.TryGetProperty("error", out System.Text.Json.JsonElement error)
                    && error.ValueKind == System.Text.Json.JsonValueKind.Object
                    && error.TryGetProperty("message", out System.Text.Json.JsonElement errorMessage)
                    && errorMessage.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    AppendOutput($"  Server says: {errorMessage.GetString()}");
                    return;
                }

                if (root.TryGetProperty("choices", out System.Text.Json.JsonElement choices)
                    && choices.ValueKind == System.Text.Json.JsonValueKind.Array
                    && choices.GetArrayLength() > 0
                    && choices[0].TryGetProperty("message", out System.Text.Json.JsonElement message)
                    && message.TryGetProperty("content", out System.Text.Json.JsonElement content)
                    && content.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    AppendOutput($"  Reply: {content.GetString()}");
                    return;
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                AppendOutput($"  Reply was not valid chat JSON ({ShortError(ex)}); showing the raw body.");
            }

            AppendOutput($"  Body: {TrimForOutput(body, 1200)}");
        }

        private void AppendServerJsonResponse(string body)
        {
            try
            {
                using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(body);
                System.Text.Json.JsonElement root = document.RootElement;
                if (root.TryGetProperty("error", out System.Text.Json.JsonElement error)
                    && error.ValueKind == System.Text.Json.JsonValueKind.Object
                    && error.TryGetProperty("message", out System.Text.Json.JsonElement errorMessage)
                    && errorMessage.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    AppendOutput($"  Server says: {errorMessage.GetString()}");
                    return;
                }

                if (root.TryGetProperty("choices", out System.Text.Json.JsonElement choices)
                    && choices.ValueKind == System.Text.Json.JsonValueKind.Array
                    && choices.GetArrayLength() > 0
                    && choices[0].TryGetProperty("text", out System.Text.Json.JsonElement text)
                    && text.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    AppendOutput($"  Text: {text.GetString()}");
                    return;
                }

                if (root.TryGetProperty("data", out System.Text.Json.JsonElement data)
                    && data.ValueKind == System.Text.Json.JsonValueKind.Array
                    && data.GetArrayLength() > 0)
                {
                    System.Text.Json.JsonElement first = data[0];
                    if (first.TryGetProperty("url", out System.Text.Json.JsonElement url)
                        && url.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        AppendOutput($"  Image URL: {url.GetString()}");
                        return;
                    }

                    if (first.TryGetProperty("b64_json", out System.Text.Json.JsonElement b64)
                        && b64.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        AppendOutput($"  Image data: {b64.GetString()?.Length ?? 0:N0} base64 characters");
                        return;
                    }
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                AppendOutput($"  Response was not valid JSON ({ShortError(ex)}); showing the raw body.");
            }

            AppendOutput($"  Body: {TrimForOutput(body, 1200)}");
        }

        private static bool TryCreateAbsoluteUri(string text, out Uri? uri)
        {
            return Uri.TryCreate(text, UriKind.Absolute, out uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static string FormatSettingValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(blank)" : value.Trim();
        }

        private static string DescribeUrlCheck(string value, string expectedPathHint)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "blank";
            }

            if (!TryCreateAbsoluteUri(value, out Uri? uri) || uri is null)
            {
                return "not a valid http or https URL";
            }

            string result = $"valid URL, host {uri.Host}";
            if (!uri.AbsolutePath.Contains(expectedPathHint, StringComparison.OrdinalIgnoreCase))
            {
                result += $"; expected a path containing {expectedPathHint}";
            }

            return result;
        }

        private static string TrimForOutput(string value, int maxLength)
        {
            string normalized = value.Replace("\r", string.Empty).Replace("\n", " ").Trim();
            return normalized.Length <= maxLength
                ? normalized
                : $"{normalized[..maxLength]}...";
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
