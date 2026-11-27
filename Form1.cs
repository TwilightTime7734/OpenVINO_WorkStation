namespace WorkStationC_
{
    public partial class Form1 : Form
    {
        private readonly OvmsServer _ovmsServer;
        private readonly WorkstationTab _workstation;
        private readonly ChatTab _chat;
        private readonly DownloadTab _downloads;
        private readonly OvmsTab _ovms;
        private CancellationTokenSource? _operation;
        private Task _running = Task.CompletedTask;
        private bool _closing;
        private bool _canClose;

        public Form1()
        {
            InitializeComponent();
            _ovmsServer = new(AppendOutput);
            _workstation = new(_ovmsServer, txtModelRoot, txtInferModel, lblModelSize, cboWorkstationTarget,
                cboWorkstationMultiTarget, txtModelCapabilities, txtServerAddress, txtServerStatus,
                txtOvmsExecutable, txtOvmsPythonPath, AppendOutput);
            _chat = new(new OvmsChat(), WebTools.Load(), GetServerAddress, cboChatModel, txtChatTranscript, txtChatInput,
                lblChatImages, lblChatFolder, btnChatSend, chkChatTools, chkChatRunCommands, GetContextTokens, AppendOutput);
            _downloads = new(txtDownloadUrl, progressDownload, lblDownloadStatus, AppendOutput);
            _ovms = new(txtOvmsExecutable, txtOvmsPythonPath, txtOvmsExportToolsPath,
                cboOvmsSourceFormat, cboOvmsTask, cboOvmsPipelineType, cboOvmsSourceModel, cboOvmsWeightFormat,
                txtOvmsGgufFilename, txtOvmsModelRepository, txtOvmsLogPath, chkOvmsOverwrite, chkOvmsDebug,
                lblOvmsPullStatus, AppendOutput);
        }

        internal async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _workstation.Initialize();
            _workstation.ShowStatus();
            _ovms.Initialize();
            _chat.ShowToolStatus();
            await _workstation.CheckAsync(cancellationToken);
            await _chat.RefreshAsync(cancellationToken);
        }

        private async void Form1_Shown(object? sender, EventArgs e) => await RunAsync(InitializeAsync);

        private async void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_canClose) return;
            e.Cancel = true;
            if (_closing) return;
            _closing = true;
            _operation?.Cancel();
            Enabled = false;
            await _running;
            await _ovmsServer.DisposeAsync();
            _downloads.Dispose();
            _canClose = true;
            Close();
        }

        // lockTabs is false for chat so the reply stays readable while it streams in.
        private Task RunAsync(Func<CancellationToken, Task> action, bool lockTabs = true)
        {
            if (_operation is not null || _closing) return Task.CompletedTask;
            return _running = ExecuteAsync(action, lockTabs);
        }

        private async Task ExecuteAsync(Func<CancellationToken, Task> action, bool lockTabs)
        {
            using var cancellation = new CancellationTokenSource();
            _operation = cancellation;
            tabMain.Enabled = !lockTabs;
            btnCancelActivity.Enabled = true;
            lblActivityStatus.Text = "Working...";
            progressActivity.Style = ProgressBarStyle.Marquee;
            try { await action(cancellation.Token); }
            catch (OperationCanceledException) { AppendOutput("Canceled."); }
            catch (Exception ex) { AppendOutput($"Error: {ex.Message}"); }
            finally
            {
                _operation = null;
                tabMain.Enabled = true;
                btnCancelActivity.Enabled = false;
                progressActivity.Style = ProgressBarStyle.Blocks;
                lblActivityStatus.Text = "Ready";
            }
        }

        private void Run(Action action) => _ = RunAsync(_ => { action(); return Task.CompletedTask; });

        private void AppendOutput(string message)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired) { BeginInvoke(() => AppendOutput(message)); return; }
            txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtOutput.SelectionStart = txtOutput.TextLength;
            txtOutput.ScrollToCaret();
        }

        private Uri GetServerAddress() => _workstation.Address;
        private int? GetContextTokens() => _ovmsServer.IsRunning ? _ovmsServer.ContextTokens : null;

        private async Task LoadModelAsync(CancellationToken cancellationToken)
        {
            await _workstation.LoadAsync(cancellationToken);
            _chat.NewChat();
            await _chat.RefreshAsync(cancellationToken);
        }

        private async Task ConnectServerAsync(CancellationToken cancellationToken)
        {
            await _workstation.ConnectAsync(cancellationToken);
            await _chat.RefreshAsync(cancellationToken);
        }

        private async void btnConnectServer_Click(object? sender, EventArgs e) => await RunAsync(ConnectServerAsync);
        private async void btnUnloadModel_Click(object? sender, EventArgs e) => await RunAsync(_ => _workstation.UnloadAsync());

        private void btnBrowseModelRoot_Click(object? sender, EventArgs e) => Run(_workstation.BrowseRoot);
        private void txtModelRoot_Leave(object? sender, EventArgs e) => Run(_workstation.SaveRoot);
        private void txtModelRoot_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            Run(_workstation.SaveRoot);
        }
        private void btnBrowseInferModel_Click(object? sender, EventArgs e) => Run(_workstation.BrowseModel);
        private void cboWorkstationTarget_SelectedIndexChanged(object? sender, EventArgs e) => _workstation?.UpdateTarget();
        private async void btnCheckWorkstation_Click(object? sender, EventArgs e) => await RunAsync(_workstation.CheckAsync);
        private async void btnLoadModel_Click(object? sender, EventArgs e) => await RunAsync(LoadModelAsync);
        private async void btnChatRefresh_Click(object? sender, EventArgs e) => await RunAsync(_chat.RefreshAsync);
        private void btnChatNew_Click(object? sender, EventArgs e) => _chat.NewChat();
        private void btnChatAttach_Click(object? sender, EventArgs e) => _chat.AttachImages();
        private void btnChatFolder_Click(object? sender, EventArgs e) => _chat.ChooseFolder();
        private void btnChatFolderClear_Click(object? sender, EventArgs e) => _chat.ClearFolder();
        private async void btnChatSend_Click(object? sender, EventArgs e)
        {
            if (_chat.IsReplying) { _operation?.Cancel(); return; }
            await RunAsync(_chat.SendAsync, lockTabs: false);
        }
        private async void txtChatInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _chat.IsReplying)
            {
                e.SuppressKeyPress = true;
                _operation?.Cancel();
                return;
            }
            if (e.KeyCode != Keys.Enter || e.Shift) return;
            e.SuppressKeyPress = true;
            if (_chat.IsReplying) return;
            await RunAsync(_chat.SendAsync, lockTabs: false);
        }
        private async void btnFindDownloadAddress_Click(object? sender, EventArgs e) => await RunAsync(_downloads.FindAsync);
        private async void btnDownload_Click(object? sender, EventArgs e) => await RunAsync(_downloads.DownloadAsync);
        private async void btnPullFromDownloads_Click(object? sender, EventArgs e) => await RunAsync(SendToOvmsPullAsync);

        private async Task SendToOvmsPullAsync(CancellationToken cancellationToken)
        {
            var advice = await _downloads.GetAdviceAsync(cancellationToken);
            if (advice is null || !advice.CanPull)
            {
                AppendOutput(advice?.Message ?? "Paste a Hugging Face model address first.");
                return;
            }
            _ovms.Prefill(advice);
            // Opening the OVMS Pull tab is what this button is for: the user reviews the settings there.
            tabMain.SelectedTab = tabOvmsPull;
        }
        private void cboOvmsSourceModel_SelectedIndexChanged(object? sender, EventArgs e) => _ovms?.SelectPreset();
        private void cboOvmsSourceModel_TextChanged(object? sender, EventArgs e) => _ovms?.SelectPreset();
        private void btnPreviewOvmsPull_Click(object? sender, EventArgs e) => Run(_ovms.Preview);
        private async void btnCheckOvmsVersions_Click(object? sender, EventArgs e) => await RunAsync(_ => _ovms.CheckAsync());
        private async void btnRunOvmsPull_Click(object? sender, EventArgs e) => await RunAsync(_ovms.StartAsync);
        private void btnCancelActivity_Click(object? sender, EventArgs e) => _operation?.Cancel();
        private void btnClearOutput_Click(object? sender, EventArgs e) => txtOutput.Clear();

        private void btnHelp_Click(object? sender, EventArgs e) => AppendOutput(tabMain.SelectedTab?.Name switch
        {
            "tabWorkstation" => "Select a model folder and a device, then Load Model. The app starts ovms.exe with that model at the server address. Unload Model stops it. Connect checks a server that is already running.",
            "tabChat" => "Type a message and press Enter or Send Message (Shift+Enter adds a new line). Add Images sends pictures to vision models. New Chat forgets the conversation. Cancel stops a reply. With 'Let the model use tools' on, the model can search the web and read pages. Working Folder lets it list, read, and write files in that folder and its subfolders without asking; anything outside asks you first. With a Working Folder the model can also run commands there; each command asks you first unless 'Run commands without asking' is checked.",
            "tabDownloads" => "Paste a Hugging Face model or direct file URL. Find lists files and recommends Download or OVMS Pull; Download saves them under the main model folder. Convert with OVMS Pull fills in the OVMS Pull tab for original (.safetensors) or GGUF models, which creates the files OVMS needs. Cancel stops the current operation.",
            "tabOvmsPull" => "Choose a source and settings. Preview shows the command; Start Pull checks dependencies and opens Command Prompt. Defaults are in ovms-settings.json.",
            _ => "Choose a tab to see its help.",
        });
    }
}
