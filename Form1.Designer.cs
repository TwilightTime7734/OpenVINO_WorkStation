namespace WorkStationC_
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            mainLayout = new TableLayoutPanel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            mainSplit = new SplitContainer();
            tabMain = new TabControl();
            tabWorkstation = new TabPage();
            workstationLayout = new TableLayoutPanel();
            lblWorkstationHeading = new Label();
            modelRootLayout = new TableLayoutPanel();
            lblWorkstationTarget = new Label();
            txtLocalServerStatus = new TextBox();
            lblLocalServerStatus = new Label();
            btnStartLocalServer = new Button();
            lblLocalServerUrl = new Label();
            btnStopLocalServer = new Button();
            btnLoadModel = new Button();
            txtLocalServerUrl = new TextBox();
            lblModelRoot = new Label();
            txtModelRoot = new TextBox();
            btnBrowseModelRoot = new Button();
            lblInferModel = new Label();
            txtInferModel = new TextBox();
            btnBrowseInferModel = new Button();
            btnUseSampleModel = new Button();
            splitContainer1 = new SplitContainer();
            cboWorkstationTarget = new ComboBox();
            lblWorkstationMultiTarget = new Label();
            cboWorkstationMultiTarget = new ComboBox();
            btnCheckWorkstation = new Button();
            lblModelCapabilities = new Label();
            txtModelCapabilities = new TextBox();
            tabDownloads = new TabPage();
            modelsScroll = new Panel();
            modelsLayout = new Panel();
            lblDownloadUrl = new Label();
            txtDownloadUrl = new TextBox();
            btnClearFileSelection = new Button();
            btnLoadFiles = new Button();
            btnCancelDownload = new Button();
            lblHfQuery = new Label();
            txtHfQuery = new TextBox();
            lblModelWebsites = new Label();
            cboModelWebsites = new ComboBox();
            btnCopyDownloadUrl = new Button();
            btnPresetHuggingFace = new Button();
            btnPresetOpenVino = new Button();
            btnPresetIntel = new Button();
            btnPresetUltralytics = new Button();
            btnPresetOllama = new Button();
            btnOfficialFiles = new Button();
            btnRefreshOfficialIndex = new Button();
            btnOpenModelWebsite = new Button();
            btnAddModelWebsite = new Button();
            btnDeleteModelWebsite = new Button();
            btnSearchRepos = new Button();
            btnSearchAllModels = new Button();
            progressDownload = new ProgressBar();
            lblDownloadStatus = new Label();
            tabLoadModel = new TabPage();
            loadModelLayout = new Panel();
            lblOptimumHeading = new Label();
            lblOptimumSource = new Label();
            cboOptimumSourceModel = new ComboBox();
            btnRefreshOptimumModels = new Button();
            lblOptimumTask = new Label();
            cboOptimumTask = new ComboBox();
            lblOptimumWeights = new Label();
            cboOptimumWeightFormat = new ComboBox();
            chkOptimumTrustRemoteCode = new CheckBox();
            chkOptimumCreateBlob = new CheckBox();
            btnRunOptimumExport = new Button();
            lblOptimumStatus = new Label();
            tabConvert = new TabPage();
            convertLayout = new Panel();
            lblOmzBuilderHeading = new Label();
            cboConvertSourceModel = new ComboBox();
            btnRefreshConvertModels = new Button();
            grpConvertOutputs = new GroupBox();
            chkConvertBlob = new CheckBox();
            chkConvertIr = new CheckBox();
            btnConvertToIr = new Button();
            pnlIrVisualizer = new Panel();
            lblIrPlaceholder = new Label();
            lblModelsStatus = new Label();
            tabBenchmark = new TabPage();
            benchmarkLayout = new TableLayoutPanel();
            lblBenchIterations = new Label();
            numBenchIterations = new NumericUpDown();
            lblBenchIterationsHelp = new Label();
            lblBenchWarmup = new Label();
            numBenchWarmup = new NumericUpDown();
            lblBenchWarmupHelp = new Label();
            lblBenchRuns = new Label();
            numBenchRuns = new NumericUpDown();
            lblBenchRunsHelp = new Label();
            lblPerformanceHint = new Label();
            cboPerformanceHint = new ComboBox();
            btnRunGenericBenchmark = new Button();
            tabVisualization = new TabPage();
            visualizationLayout = new TableLayoutPanel();
            lblVizCsv = new Label();
            txtVizCsv = new TextBox();
            btnBrowseVizCsv = new Button();
            lblVizMetric = new Label();
            cboVizMetric = new ComboBox();
            vizButtons = new FlowLayoutPanel();
            btnRenderChart = new Button();
            lblVizStatus = new Label();
            pnlChart = new Panel();
            lblChartPlaceholder = new Label();
            tabTests = new TabPage();
            testsLayout = new TableLayoutPanel();
            lblTestsHeading = new Label();
            lblTestModelPath = new Label();
            txtTestModelPath = new TextBox();
            lblInferImage = new Label();
            txtInferImage = new TextBox();
            imageTestButtons = new FlowLayoutPanel();
            btnBrowseInferImage = new Button();
            btnRunImageTest = new Button();
            lblInferText = new Label();
            txtInferText = new TextBox();
            lblInferLabels = new Label();
            txtInferLabels = new TextBox();
            btnBrowseInferLabels = new Button();
            lblInferPreset = new Label();
            cboInferPreset = new ComboBox();
            btnInspectOpenVinoModel = new Button();
            lblTestsNote = new Label();
            lblImageResult = new Label();
            picImageResult = new PictureBox();
            btnRunOpenVinoModelTest = new Button();
            tabServerTests = new TabPage();
            serverTestsLayout = new TableLayoutPanel();
            lblOpenVinoRuntimeHeading = new Label();
            txtServerEndpointList = new TextBox();
            lblTestApiKey = new Label();
            txtTestApiKey = new TextBox();
            lblServerNoInputHeading = new Label();
            lblTestEndpoint = new Label();
            txtTestEndpoint = new TextBox();
            btnTestEndpoint = new Button();
            lblModelsEndpoint = new Label();
            txtModelsEndpoint = new TextBox();
            btnTestModelsEndpoint = new Button();
            lblServerUserInputHeading = new Label();
            lblChatModel = new Label();
            cboServerModel = new ComboBox();
            lblChatPrompt = new Label();
            txtChatPrompt = new TextBox();
            lblChatEndpoint = new Label();
            txtChatEndpoint = new TextBox();
            btnTestChat = new Button();
            lblTextEndpoint = new Label();
            txtTextEndpoint = new TextBox();
            btnTestTextEndpoint = new Button();
            lblImageEndpoint = new Label();
            txtImageEndpoint = new TextBox();
            btnTestImageEndpoint = new Button();
            btnShowServerSettings = new Button();
            outputLayout = new TableLayoutPanel();
            grpOutput = new GroupBox();
            outputContentLayout = new TableLayoutPanel();
            lblOutputInfo = new Label();
            txtOutput = new RichTextBox();
            outputButtons = new TableLayoutPanel();
            btnClearOutput = new Button();
            activityLayout = new TableLayoutPanel();
            lblActivityStatus = new Label();
            progressActivity = new ProgressBar();
            btnCancelActivity = new Button();
            btnHelp = new Button();
            btnDeleteDownload = new Button();
            lstHfRepos = new ListView();
            colModelId = new ColumnHeader();
            colDownloads = new ColumnHeader();
            colLikes = new ColumnHeader();
            colModelSource = new ColumnHeader();
            colTask = new ColumnHeader();
            colLibrary = new ColumnHeader();
            colQuantization = new ColumnHeader();
            btnDownloadHelp = new Button();
            lblRepoId = new Label();
            txtRepoId = new TextBox();
            lblRevision = new Label();
            txtRevision = new TextBox();
            lstHfFiles = new ListView();
            colFilePath = new ColumnHeader();
            colFileType = new ColumnHeader();
            colFileExtension = new ColumnHeader();
            colFileSize = new ColumnHeader();
            colFileNotes = new ColumnHeader();
            modelsSeparator2 = new Label();
            mainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
            mainSplit.Panel1.SuspendLayout();
            mainSplit.Panel2.SuspendLayout();
            mainSplit.SuspendLayout();
            tabMain.SuspendLayout();
            tabWorkstation.SuspendLayout();
            workstationLayout.SuspendLayout();
            modelRootLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabDownloads.SuspendLayout();
            modelsScroll.SuspendLayout();
            modelsLayout.SuspendLayout();
            tabLoadModel.SuspendLayout();
            loadModelLayout.SuspendLayout();
            tabConvert.SuspendLayout();
            convertLayout.SuspendLayout();
            grpConvertOutputs.SuspendLayout();
            pnlIrVisualizer.SuspendLayout();
            tabBenchmark.SuspendLayout();
            benchmarkLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBenchIterations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numBenchWarmup).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numBenchRuns).BeginInit();
            tabVisualization.SuspendLayout();
            visualizationLayout.SuspendLayout();
            vizButtons.SuspendLayout();
            pnlChart.SuspendLayout();
            tabTests.SuspendLayout();
            testsLayout.SuspendLayout();
            imageTestButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picImageResult).BeginInit();
            tabServerTests.SuspendLayout();
            serverTestsLayout.SuspendLayout();
            outputLayout.SuspendLayout();
            grpOutput.SuspendLayout();
            outputContentLayout.SuspendLayout();
            outputButtons.SuspendLayout();
            activityLayout.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblTitle, 0, 0);
            mainLayout.Controls.Add(lblSubtitle, 0, 1);
            mainLayout.Controls.Add(mainSplit, 0, 2);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(12, 10, 12, 12);
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.Size = new Size(1103, 881);
            mainLayout.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            lblTitle.Location = new Point(15, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(257, 31);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "OpenVINO Workbench";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.ForeColor = SystemColors.GrayText;
            lblSubtitle.Location = new Point(15, 44);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(514, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Tabbed runner for device checks, inference, benchmarks, visualization, and model management.";
            // 
            // mainSplit
            // 
            mainSplit.BorderStyle = BorderStyle.FixedSingle;
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.FixedPanel = FixedPanel.Panel2;
            mainSplit.Location = new Point(15, 75);
            mainSplit.Name = "mainSplit";
            mainSplit.Orientation = Orientation.Horizontal;
            // 
            // mainSplit.Panel1
            // 
            mainSplit.Panel1.Controls.Add(tabMain);
            // 
            // mainSplit.Panel2
            // 
            mainSplit.Panel2.Controls.Add(outputLayout);
            mainSplit.Panel2MinSize = 120;
            mainSplit.Size = new Size(1073, 791);
            mainSplit.SplitterDistance = 360;
            mainSplit.SplitterWidth = 12;
            mainSplit.TabIndex = 2;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabWorkstation);
            tabMain.Controls.Add(tabDownloads);
            tabMain.Controls.Add(tabConvert);
            tabMain.Controls.Add(tabLoadModel);
            tabMain.Controls.Add(tabBenchmark);
            tabMain.Controls.Add(tabVisualization);
            tabMain.Controls.Add(tabTests);
            tabMain.Controls.Add(tabServerTests);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("Segoe UI", 10F);
            tabMain.Location = new Point(0, 0);
            tabMain.Name = "tabMain";
            tabMain.Padding = new Point(14, 6);
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(1071, 358);
            tabMain.TabIndex = 0;
            tabMain.DrawItem += tabMain_DrawItem;
            tabMain.SelectedIndexChanged += tabMain_SelectedIndexChanged;
            tabMain.Selecting += tabMain_Selecting;
            // 
            // tabWorkstation
            // 
            tabWorkstation.Controls.Add(workstationLayout);
            tabWorkstation.Location = new Point(4, 32);
            tabWorkstation.Name = "tabWorkstation";
            tabWorkstation.Padding = new Padding(12);
            tabWorkstation.Size = new Size(1063, 322);
            tabWorkstation.TabIndex = 0;
            tabWorkstation.Text = "Workstation";
            // 
            // workstationLayout
            // 
            workstationLayout.ColumnCount = 1;
            workstationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            workstationLayout.Controls.Add(lblWorkstationHeading, 0, 0);
            workstationLayout.Controls.Add(modelRootLayout, 0, 1);
            workstationLayout.Location = new Point(12, 12);
            workstationLayout.Name = "workstationLayout";
            workstationLayout.RowCount = 2;
            workstationLayout.RowStyles.Add(new RowStyle());
            workstationLayout.RowStyles.Add(new RowStyle());
            workstationLayout.Size = new Size(1039, 462);
            workstationLayout.TabIndex = 0;
            // 
            // lblWorkstationHeading
            // 
            lblWorkstationHeading.AutoSize = true;
            lblWorkstationHeading.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblWorkstationHeading.Location = new Point(3, 0);
            lblWorkstationHeading.Name = "lblWorkstationHeading";
            lblWorkstationHeading.Size = new Size(260, 20);
            lblWorkstationHeading.TabIndex = 0;
            lblWorkstationHeading.Text = "Environment and device diagnostics";
            // 
            // modelRootLayout
            // 
            modelRootLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            modelRootLayout.ColumnCount = 4;
            modelRootLayout.ColumnStyles.Add(new ColumnStyle());
            modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
            modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 141F));
            modelRootLayout.Controls.Add(lblWorkstationTarget, 0, 2);
            modelRootLayout.Controls.Add(txtLocalServerStatus, 1, 6);
            modelRootLayout.Controls.Add(lblLocalServerStatus, 0, 6);
            modelRootLayout.Controls.Add(btnStartLocalServer, 2, 5);
            modelRootLayout.Controls.Add(lblLocalServerUrl, 0, 5);
            modelRootLayout.Controls.Add(btnStopLocalServer, 3, 5);
            modelRootLayout.Controls.Add(btnLoadModel, 2, 2);
            modelRootLayout.Controls.Add(txtLocalServerUrl, 1, 5);
            modelRootLayout.Controls.Add(lblModelRoot, 0, 0);
            modelRootLayout.Controls.Add(txtModelRoot, 1, 0);
            modelRootLayout.Controls.Add(btnBrowseModelRoot, 2, 0);
            modelRootLayout.Controls.Add(lblInferModel, 0, 1);
            modelRootLayout.Controls.Add(txtInferModel, 1, 1);
            modelRootLayout.Controls.Add(btnBrowseInferModel, 2, 1);
            modelRootLayout.Controls.Add(btnUseSampleModel, 3, 1);
            modelRootLayout.Controls.Add(splitContainer1, 1, 2);
            modelRootLayout.Controls.Add(lblWorkstationMultiTarget, 0, 3);
            modelRootLayout.Controls.Add(cboWorkstationMultiTarget, 1, 3);
            modelRootLayout.Controls.Add(lblModelCapabilities, 0, 4);
            modelRootLayout.Controls.Add(txtModelCapabilities, 1, 4);
            modelRootLayout.Location = new Point(3, 23);
            modelRootLayout.Name = "modelRootLayout";
            modelRootLayout.RowCount = 7;
            modelRootLayout.RowStyles.Add(new RowStyle());
            modelRootLayout.RowStyles.Add(new RowStyle());
            modelRootLayout.RowStyles.Add(new RowStyle());
            modelRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            modelRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            modelRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
            modelRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            modelRootLayout.Size = new Size(982, 290);
            modelRootLayout.TabIndex = 1;
            // 
            // lblWorkstationTarget
            // 
            lblWorkstationTarget.AutoSize = true;
            lblWorkstationTarget.Dock = DockStyle.Fill;
            lblWorkstationTarget.Location = new Point(3, 75);
            lblWorkstationTarget.Margin = new Padding(3, 8, 3, 0);
            lblWorkstationTarget.Name = "lblWorkstationTarget";
            lblWorkstationTarget.Size = new Size(124, 33);
            lblWorkstationTarget.TabIndex = 1;
            lblWorkstationTarget.Text = "Model's target:";
            lblWorkstationTarget.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtLocalServerStatus
            // 
            txtLocalServerStatus.Anchor = AnchorStyles.Left;
            txtLocalServerStatus.Location = new Point(133, 259);
            txtLocalServerStatus.Name = "txtLocalServerStatus";
            txtLocalServerStatus.ReadOnly = true;
            txtLocalServerStatus.Size = new Size(590, 25);
            txtLocalServerStatus.TabIndex = 29;
            txtLocalServerStatus.Text = "Stopped";
            // 
            // lblLocalServerStatus
            // 
            lblLocalServerStatus.AutoSize = true;
            lblLocalServerStatus.Dock = DockStyle.Fill;
            lblLocalServerStatus.Location = new Point(3, 253);
            lblLocalServerStatus.Name = "lblLocalServerStatus";
            lblLocalServerStatus.Size = new Size(124, 37);
            lblLocalServerStatus.TabIndex = 28;
            lblLocalServerStatus.Text = "Status:";
            lblLocalServerStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnStartLocalServer
            // 
            btnStartLocalServer.Dock = DockStyle.Fill;
            btnStartLocalServer.Location = new Point(729, 217);
            btnStartLocalServer.Name = "btnStartLocalServer";
            btnStartLocalServer.Size = new Size(109, 33);
            btnStartLocalServer.TabIndex = 27;
            btnStartLocalServer.Text = "Start Server";
            btnStartLocalServer.Click += btnStartLocalServer_Click;
            // 
            // lblLocalServerUrl
            // 
            lblLocalServerUrl.AutoSize = true;
            lblLocalServerUrl.Dock = DockStyle.Fill;
            lblLocalServerUrl.Location = new Point(3, 214);
            lblLocalServerUrl.Name = "lblLocalServerUrl";
            lblLocalServerUrl.Size = new Size(124, 39);
            lblLocalServerUrl.TabIndex = 25;
            lblLocalServerUrl.Text = "Server address:";
            lblLocalServerUrl.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnStopLocalServer
            // 
            btnStopLocalServer.Dock = DockStyle.Fill;
            btnStopLocalServer.Location = new Point(844, 217);
            btnStopLocalServer.Name = "btnStopLocalServer";
            btnStopLocalServer.Size = new Size(135, 33);
            btnStopLocalServer.TabIndex = 30;
            btnStopLocalServer.Text = "Stop Server";
            btnStopLocalServer.Click += btnStopLocalServer_Click;
            // 
            // btnLoadModel
            // 
            modelRootLayout.SetColumnSpan(btnLoadModel, 2);
            btnLoadModel.Dock = DockStyle.Fill;
            btnLoadModel.Location = new Point(729, 70);
            btnLoadModel.Name = "btnLoadModel";
            btnLoadModel.Padding = new Padding(8, 3, 8, 3);
            btnLoadModel.Size = new Size(250, 35);
            btnLoadModel.TabIndex = 15;
            btnLoadModel.Text = "Load Model";
            btnLoadModel.Click += btnLoadModel_Click;
            // 
            // txtLocalServerUrl
            // 
            txtLocalServerUrl.Dock = DockStyle.Fill;
            txtLocalServerUrl.Location = new Point(133, 217);
            txtLocalServerUrl.Name = "txtLocalServerUrl";
            txtLocalServerUrl.ReadOnly = true;
            txtLocalServerUrl.Size = new Size(590, 25);
            txtLocalServerUrl.TabIndex = 26;
            txtLocalServerUrl.Text = "http://192.168.0.11:11434";
            // 
            // lblModelRoot
            // 
            lblModelRoot.AutoSize = true;
            lblModelRoot.Dock = DockStyle.Fill;
            lblModelRoot.Location = new Point(3, 0);
            lblModelRoot.Name = "lblModelRoot";
            lblModelRoot.Size = new Size(124, 31);
            lblModelRoot.TabIndex = 0;
            lblModelRoot.Text = "Main model folder:";
            lblModelRoot.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtModelRoot
            // 
            txtModelRoot.Dock = DockStyle.Fill;
            txtModelRoot.Location = new Point(133, 3);
            txtModelRoot.Name = "txtModelRoot";
            txtModelRoot.Size = new Size(590, 25);
            txtModelRoot.TabIndex = 1;
            txtModelRoot.KeyDown += txtModelRoot_KeyDown;
            txtModelRoot.Leave += txtModelRoot_Leave;
            // 
            // btnBrowseModelRoot
            // 
            modelRootLayout.SetColumnSpan(btnBrowseModelRoot, 2);
            btnBrowseModelRoot.Dock = DockStyle.Fill;
            btnBrowseModelRoot.Location = new Point(729, 2);
            btnBrowseModelRoot.Margin = new Padding(3, 2, 3, 2);
            btnBrowseModelRoot.Name = "btnBrowseModelRoot";
            btnBrowseModelRoot.Size = new Size(250, 27);
            btnBrowseModelRoot.TabIndex = 2;
            btnBrowseModelRoot.Text = "Browse...";
            btnBrowseModelRoot.Click += btnBrowseModelRoot_Click;
            // 
            // lblInferModel
            // 
            lblInferModel.AutoSize = true;
            lblInferModel.Dock = DockStyle.Fill;
            lblInferModel.Location = new Point(3, 31);
            lblInferModel.Name = "lblInferModel";
            lblInferModel.Size = new Size(124, 36);
            lblInferModel.TabIndex = 0;
            lblInferModel.Text = "Model:";
            lblInferModel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtInferModel
            // 
            txtInferModel.Dock = DockStyle.Fill;
            txtInferModel.Location = new Point(133, 34);
            txtInferModel.Name = "txtInferModel";
            txtInferModel.Size = new Size(590, 25);
            txtInferModel.TabIndex = 1;
            txtInferModel.TextChanged += txtInferModel_TextChanged;
            // 
            // btnBrowseInferModel
            // 
            btnBrowseInferModel.Dock = DockStyle.Fill;
            btnBrowseInferModel.Location = new Point(729, 33);
            btnBrowseInferModel.Margin = new Padding(3, 2, 3, 2);
            btnBrowseInferModel.Name = "btnBrowseInferModel";
            btnBrowseInferModel.Size = new Size(109, 32);
            btnBrowseInferModel.TabIndex = 2;
            btnBrowseInferModel.Text = "Select";
            btnBrowseInferModel.Click += btnBrowseInferModel_Click;
            // 
            // btnUseSampleModel
            // 
            btnUseSampleModel.Dock = DockStyle.Fill;
            btnUseSampleModel.Location = new Point(844, 33);
            btnUseSampleModel.Margin = new Padding(3, 2, 3, 2);
            btnUseSampleModel.Name = "btnUseSampleModel";
            btnUseSampleModel.Size = new Size(135, 32);
            btnUseSampleModel.TabIndex = 5;
            btnUseSampleModel.Text = "Sample";
            btnUseSampleModel.Click += btnUseSampleModel_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(133, 70);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(cboWorkstationTarget);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btnCheckWorkstation);
            splitContainer1.Size = new Size(590, 35);
            splitContainer1.SplitterDistance = 284;
            splitContainer1.TabIndex = 16;
            // 
            // cboWorkstationTarget
            // 
            cboWorkstationTarget.Dock = DockStyle.Fill;
            cboWorkstationTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWorkstationTarget.Location = new Point(0, 0);
            cboWorkstationTarget.Margin = new Padding(3, 5, 3, 0);
            cboWorkstationTarget.Name = "cboWorkstationTarget";
            cboWorkstationTarget.Size = new Size(284, 25);
            cboWorkstationTarget.TabIndex = 2;
            cboWorkstationTarget.SelectedIndexChanged += cboWorkstationTarget_SelectedIndexChanged;
            // 
            // lblWorkstationMultiTarget
            // 
            lblWorkstationMultiTarget.AutoSize = true;
            lblWorkstationMultiTarget.Dock = DockStyle.Fill;
            lblWorkstationMultiTarget.Location = new Point(3, 116);
            lblWorkstationMultiTarget.Margin = new Padding(3, 8, 3, 0);
            lblWorkstationMultiTarget.Name = "lblWorkstationMultiTarget";
            lblWorkstationMultiTarget.Size = new Size(124, 28);
            lblWorkstationMultiTarget.TabIndex = 31;
            lblWorkstationMultiTarget.Text = "Multi devices:";
            lblWorkstationMultiTarget.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboWorkstationMultiTarget
            // 
            modelRootLayout.SetColumnSpan(cboWorkstationMultiTarget, 3);
            cboWorkstationMultiTarget.Dock = DockStyle.Fill;
            cboWorkstationMultiTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWorkstationMultiTarget.Enabled = false;
            cboWorkstationMultiTarget.Location = new Point(133, 111);
            cboWorkstationMultiTarget.Name = "cboWorkstationMultiTarget";
            cboWorkstationMultiTarget.Size = new Size(846, 25);
            cboWorkstationMultiTarget.TabIndex = 32;
            cboWorkstationMultiTarget.SelectedIndexChanged += cboWorkstationMultiTarget_SelectedIndexChanged;
            // 
            // btnCheckWorkstation
            // 
            btnCheckWorkstation.Dock = DockStyle.Fill;
            btnCheckWorkstation.Location = new Point(0, 0);
            btnCheckWorkstation.Margin = new Padding(3, 1, 3, 1);
            btnCheckWorkstation.Name = "btnCheckWorkstation";
            btnCheckWorkstation.Size = new Size(302, 35);
            btnCheckWorkstation.TabIndex = 0;
            btnCheckWorkstation.Text = "Check Workstation";
            btnCheckWorkstation.Click += btnCheckWorkstation_Click;
            // 
            // lblModelCapabilities
            // 
            lblModelCapabilities.AutoSize = true;
            lblModelCapabilities.Dock = DockStyle.Fill;
            lblModelCapabilities.Location = new Point(3, 152);
            lblModelCapabilities.Margin = new Padding(3, 8, 3, 0);
            lblModelCapabilities.Name = "lblModelCapabilities";
            lblModelCapabilities.Size = new Size(124, 62);
            lblModelCapabilities.TabIndex = 17;
            lblModelCapabilities.Text = "Model can use:";
            lblModelCapabilities.TextAlign = ContentAlignment.TopRight;
            // 
            // txtModelCapabilities
            // 
            modelRootLayout.SetColumnSpan(txtModelCapabilities, 3);
            txtModelCapabilities.Dock = DockStyle.Fill;
            txtModelCapabilities.Location = new Point(133, 147);
            txtModelCapabilities.Multiline = true;
            txtModelCapabilities.Name = "txtModelCapabilities";
            txtModelCapabilities.ReadOnly = true;
            txtModelCapabilities.ScrollBars = ScrollBars.Vertical;
            txtModelCapabilities.Size = new Size(846, 64);
            txtModelCapabilities.TabIndex = 18;
            txtModelCapabilities.Text = "Load a model to see what it can use.";
            // 
            // tabDownloads
            // 
            tabDownloads.Controls.Add(modelsScroll);
            tabDownloads.Location = new Point(4, 32);
            tabDownloads.Name = "tabDownloads";
            tabDownloads.Padding = new Padding(3);
            tabDownloads.Size = new Size(1063, 322);
            tabDownloads.TabIndex = 4;
            tabDownloads.Text = "Downloads";
            // 
            // modelsScroll
            // 
            modelsScroll.AutoScroll = true;
            modelsScroll.Controls.Add(modelsLayout);
            modelsScroll.Dock = DockStyle.Fill;
            modelsScroll.Location = new Point(3, 3);
            modelsScroll.Name = "modelsScroll";
            modelsScroll.Size = new Size(1057, 316);
            modelsScroll.TabIndex = 0;
            // 
            // modelsLayout
            // 
            modelsLayout.Controls.Add(lblDownloadUrl);
            modelsLayout.Controls.Add(txtDownloadUrl);
            modelsLayout.Controls.Add(btnClearFileSelection);
            modelsLayout.Controls.Add(btnLoadFiles);
            modelsLayout.Controls.Add(btnCancelDownload);
            modelsLayout.Controls.Add(lblHfQuery);
            modelsLayout.Controls.Add(txtHfQuery);
            modelsLayout.Controls.Add(lblModelWebsites);
            modelsLayout.Controls.Add(cboModelWebsites);
            modelsLayout.Controls.Add(btnCopyDownloadUrl);
            modelsLayout.Controls.Add(btnPresetHuggingFace);
            modelsLayout.Controls.Add(btnPresetOpenVino);
            modelsLayout.Controls.Add(btnPresetIntel);
            modelsLayout.Controls.Add(btnPresetUltralytics);
            modelsLayout.Controls.Add(btnPresetOllama);
            modelsLayout.Controls.Add(btnOfficialFiles);
            modelsLayout.Controls.Add(btnRefreshOfficialIndex);
            modelsLayout.Controls.Add(btnOpenModelWebsite);
            modelsLayout.Controls.Add(btnAddModelWebsite);
            modelsLayout.Controls.Add(btnDeleteModelWebsite);
            modelsLayout.Controls.Add(btnSearchRepos);
            modelsLayout.Controls.Add(btnSearchAllModels);
            modelsLayout.Controls.Add(progressDownload);
            modelsLayout.Controls.Add(lblDownloadStatus);
            modelsLayout.Location = new Point(0, 3);
            modelsLayout.Name = "modelsLayout";
            modelsLayout.Padding = new Padding(10);
            modelsLayout.Size = new Size(1035, 310);
            modelsLayout.TabIndex = 0;
            // 
            // lblDownloadUrl
            // 
            lblDownloadUrl.Location = new Point(20, 25);
            lblDownloadUrl.Name = "lblDownloadUrl";
            lblDownloadUrl.Size = new Size(100, 25);
            lblDownloadUrl.TabIndex = 18;
            lblDownloadUrl.Text = "Web Address";
            lblDownloadUrl.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtDownloadUrl
            // 
            txtDownloadUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDownloadUrl.Location = new Point(125, 25);
            txtDownloadUrl.Name = "txtDownloadUrl";
            txtDownloadUrl.Size = new Size(570, 25);
            txtDownloadUrl.TabIndex = 19;
            txtDownloadUrl.Enter += txtDownloadUrl_Enter;
            // 
            // btnClearFileSelection
            // 
            btnClearFileSelection.Location = new Point(705, 21);
            btnClearFileSelection.Name = "btnClearFileSelection";
            btnClearFileSelection.Size = new Size(105, 34);
            btnClearFileSelection.TabIndex = 14;
            btnClearFileSelection.Text = "Find";
            btnClearFileSelection.Click += btnFindDownloadAddress_Click;
            // 
            // btnLoadFiles
            // 
            btnLoadFiles.Location = new Point(820, 21);
            btnLoadFiles.Name = "btnLoadFiles";
            btnLoadFiles.Size = new Size(105, 34);
            btnLoadFiles.TabIndex = 13;
            btnLoadFiles.Text = "Download";
            btnLoadFiles.Click += btnLoadFiles_Click;
            // 
            // btnCancelDownload
            // 
            btnCancelDownload.Enabled = false;
            btnCancelDownload.Location = new Point(930, 21);
            btnCancelDownload.Name = "btnCancelDownload";
            btnCancelDownload.Size = new Size(105, 34);
            btnCancelDownload.TabIndex = 1;
            btnCancelDownload.Text = "Cancel";
            btnCancelDownload.Click += btnCancelDownload_Click;
            // 
            // lblHfQuery
            // 
            lblHfQuery.Anchor = AnchorStyles.None;
            lblHfQuery.Location = new Point(20, 65);
            lblHfQuery.Name = "lblHfQuery";
            lblHfQuery.Size = new Size(100, 25);
            lblHfQuery.TabIndex = 12;
            lblHfQuery.Text = "Button Label";
            lblHfQuery.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtHfQuery
            // 
            txtHfQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtHfQuery.Location = new Point(125, 65);
            txtHfQuery.Name = "txtHfQuery";
            txtHfQuery.Size = new Size(360, 25);
            txtHfQuery.TabIndex = 13;
            // 
            // lblModelWebsites
            // 
            lblModelWebsites.Location = new Point(500, 65);
            lblModelWebsites.Name = "lblModelWebsites";
            lblModelWebsites.Size = new Size(70, 25);
            lblModelWebsites.TabIndex = 8;
            lblModelWebsites.Text = "Save To";
            lblModelWebsites.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cboModelWebsites
            // 
            cboModelWebsites.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboModelWebsites.DropDownStyle = ComboBoxStyle.DropDownList;
            cboModelWebsites.FormattingEnabled = true;
            cboModelWebsites.Location = new Point(575, 65);
            cboModelWebsites.Name = "cboModelWebsites";
            cboModelWebsites.Size = new Size(100, 25);
            cboModelWebsites.TabIndex = 8;
            // 
            // btnCopyDownloadUrl
            // 
            btnCopyDownloadUrl.Location = new Point(690, 61);
            btnCopyDownloadUrl.Name = "btnCopyDownloadUrl";
            btnCopyDownloadUrl.Size = new Size(130, 34);
            btnCopyDownloadUrl.TabIndex = 20;
            btnCopyDownloadUrl.Text = "Save Site";
            btnCopyDownloadUrl.Click += btnSaveFavoriteSite_Click;
            // 
            // btnPresetHuggingFace
            // 
            btnPresetHuggingFace.AutoEllipsis = true;
            btnPresetHuggingFace.Location = new Point(20, 120);
            btnPresetHuggingFace.Name = "btnPresetHuggingFace";
            btnPresetHuggingFace.Size = new Size(235, 34);
            btnPresetHuggingFace.TabIndex = 1;
            btnPresetHuggingFace.Text = "Site 1";
            btnPresetHuggingFace.Click += btnPresetHuggingFace_Click;
            // 
            // btnPresetOpenVino
            // 
            btnPresetOpenVino.AutoEllipsis = true;
            btnPresetOpenVino.Location = new Point(270, 120);
            btnPresetOpenVino.Name = "btnPresetOpenVino";
            btnPresetOpenVino.Size = new Size(235, 34);
            btnPresetOpenVino.TabIndex = 2;
            btnPresetOpenVino.Text = "Site 2";
            btnPresetOpenVino.Click += btnPresetOpenVino_Click;
            // 
            // btnPresetIntel
            // 
            btnPresetIntel.AutoEllipsis = true;
            btnPresetIntel.Location = new Point(520, 120);
            btnPresetIntel.Name = "btnPresetIntel";
            btnPresetIntel.Size = new Size(235, 34);
            btnPresetIntel.TabIndex = 3;
            btnPresetIntel.Text = "Site 3";
            btnPresetIntel.Click += btnPresetIntel_Click;
            // 
            // btnPresetUltralytics
            // 
            btnPresetUltralytics.AutoEllipsis = true;
            btnPresetUltralytics.Location = new Point(770, 120);
            btnPresetUltralytics.Name = "btnPresetUltralytics";
            btnPresetUltralytics.Size = new Size(235, 34);
            btnPresetUltralytics.TabIndex = 4;
            btnPresetUltralytics.Text = "Site 4";
            btnPresetUltralytics.Click += btnPresetUltralytics_Click;
            // 
            // btnPresetOllama
            // 
            btnPresetOllama.AutoEllipsis = true;
            btnPresetOllama.Location = new Point(20, 165);
            btnPresetOllama.Name = "btnPresetOllama";
            btnPresetOllama.Size = new Size(235, 34);
            btnPresetOllama.TabIndex = 5;
            btnPresetOllama.Text = "Site 5";
            btnPresetOllama.Click += btnPresetOllama_Click;
            // 
            // btnOfficialFiles
            // 
            btnOfficialFiles.AutoEllipsis = true;
            btnOfficialFiles.Location = new Point(270, 165);
            btnOfficialFiles.Name = "btnOfficialFiles";
            btnOfficialFiles.Size = new Size(235, 34);
            btnOfficialFiles.TabIndex = 6;
            btnOfficialFiles.Text = "Site 6";
            btnOfficialFiles.Click += btnOfficialFiles_Click;
            // 
            // btnRefreshOfficialIndex
            // 
            btnRefreshOfficialIndex.AutoEllipsis = true;
            btnRefreshOfficialIndex.Location = new Point(520, 165);
            btnRefreshOfficialIndex.Name = "btnRefreshOfficialIndex";
            btnRefreshOfficialIndex.Size = new Size(235, 34);
            btnRefreshOfficialIndex.TabIndex = 7;
            btnRefreshOfficialIndex.Text = "Site 7";
            btnRefreshOfficialIndex.Click += btnRefreshOfficialIndex_Click;
            // 
            // btnOpenModelWebsite
            // 
            btnOpenModelWebsite.AutoEllipsis = true;
            btnOpenModelWebsite.Location = new Point(770, 165);
            btnOpenModelWebsite.Name = "btnOpenModelWebsite";
            btnOpenModelWebsite.Size = new Size(235, 34);
            btnOpenModelWebsite.TabIndex = 9;
            btnOpenModelWebsite.Text = "Site 8";
            btnOpenModelWebsite.Click += btnOpenModelWebsite_Click;
            // 
            // btnAddModelWebsite
            // 
            btnAddModelWebsite.AutoEllipsis = true;
            btnAddModelWebsite.Location = new Point(20, 210);
            btnAddModelWebsite.Name = "btnAddModelWebsite";
            btnAddModelWebsite.Size = new Size(235, 34);
            btnAddModelWebsite.TabIndex = 10;
            btnAddModelWebsite.Text = "Site 9";
            btnAddModelWebsite.Click += btnAddModelWebsite_Click;
            // 
            // btnDeleteModelWebsite
            // 
            btnDeleteModelWebsite.AutoEllipsis = true;
            btnDeleteModelWebsite.Location = new Point(270, 210);
            btnDeleteModelWebsite.Name = "btnDeleteModelWebsite";
            btnDeleteModelWebsite.Size = new Size(235, 34);
            btnDeleteModelWebsite.TabIndex = 11;
            btnDeleteModelWebsite.Text = "Site 10";
            btnDeleteModelWebsite.Click += btnDeleteModelWebsite_Click;
            // 
            // btnSearchRepos
            // 
            btnSearchRepos.Anchor = AnchorStyles.Left;
            btnSearchRepos.AutoEllipsis = true;
            btnSearchRepos.Location = new Point(520, 210);
            btnSearchRepos.Name = "btnSearchRepos";
            btnSearchRepos.Size = new Size(235, 34);
            btnSearchRepos.TabIndex = 14;
            btnSearchRepos.Text = "Site 11";
            btnSearchRepos.Click += btnSearchRepos_Click;
            // 
            // btnSearchAllModels
            // 
            btnSearchAllModels.Anchor = AnchorStyles.Left;
            btnSearchAllModels.AutoEllipsis = true;
            btnSearchAllModels.Location = new Point(770, 210);
            btnSearchAllModels.Name = "btnSearchAllModels";
            btnSearchAllModels.Size = new Size(235, 34);
            btnSearchAllModels.TabIndex = 35;
            btnSearchAllModels.Text = "Site 12";
            btnSearchAllModels.Click += btnSearchAllModels_Click;
            // 
            // progressDownload
            // 
            progressDownload.Location = new Point(20, 260);
            progressDownload.Margin = new Padding(0, 7, 10, 0);
            progressDownload.Name = "progressDownload";
            progressDownload.Size = new Size(420, 18);
            progressDownload.TabIndex = 0;
            // 
            // lblDownloadStatus
            // 
            lblDownloadStatus.AutoEllipsis = true;
            lblDownloadStatus.Location = new Point(455, 253);
            lblDownloadStatus.Name = "lblDownloadStatus";
            lblDownloadStatus.Size = new Size(560, 28);
            lblDownloadStatus.TabIndex = 31;
            lblDownloadStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabLoadModel
            // 
            tabLoadModel.Controls.Add(loadModelLayout);
            tabLoadModel.Location = new Point(4, 32);
            tabLoadModel.Name = "tabLoadModel";
            tabLoadModel.Padding = new Padding(12);
            tabLoadModel.Size = new Size(1063, 322);
            tabLoadModel.TabIndex = 6;
            tabLoadModel.Text = "Optimum Intel";
            // 
            // loadModelLayout
            // 
            loadModelLayout.AutoScroll = true;
            loadModelLayout.Controls.Add(lblOptimumHeading);
            loadModelLayout.Controls.Add(lblOptimumSource);
            loadModelLayout.Controls.Add(cboOptimumSourceModel);
            loadModelLayout.Controls.Add(btnRefreshOptimumModels);
            loadModelLayout.Controls.Add(lblOptimumTask);
            loadModelLayout.Controls.Add(cboOptimumTask);
            loadModelLayout.Controls.Add(lblOptimumWeights);
            loadModelLayout.Controls.Add(cboOptimumWeightFormat);
            loadModelLayout.Controls.Add(chkOptimumTrustRemoteCode);
            loadModelLayout.Controls.Add(chkOptimumCreateBlob);
            loadModelLayout.Controls.Add(btnRunOptimumExport);
            loadModelLayout.Controls.Add(lblOptimumStatus);
            loadModelLayout.Dock = DockStyle.Fill;
            loadModelLayout.Location = new Point(12, 12);
            loadModelLayout.Name = "loadModelLayout";
            loadModelLayout.Padding = new Padding(10);
            loadModelLayout.Size = new Size(1039, 298);
            loadModelLayout.TabIndex = 0;
            // 
            // lblOptimumHeading
            // 
            lblOptimumHeading.AutoSize = true;
            lblOptimumHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOptimumHeading.Location = new Point(20, 25);
            lblOptimumHeading.Name = "lblOptimumHeading";
            lblOptimumHeading.Size = new Size(142, 19);
            lblOptimumHeading.TabIndex = 0;
            lblOptimumHeading.Text = "Optimum Intel export";
            // 
            // lblOptimumSource
            // 
            lblOptimumSource.Location = new Point(20, 72);
            lblOptimumSource.Name = "lblOptimumSource";
            lblOptimumSource.Size = new Size(105, 25);
            lblOptimumSource.TabIndex = 1;
            lblOptimumSource.Text = "Source model";
            // 
            // cboOptimumSourceModel
            // 
            cboOptimumSourceModel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboOptimumSourceModel.Location = new Point(135, 70);
            cboOptimumSourceModel.Name = "cboOptimumSourceModel";
            cboOptimumSourceModel.Size = new Size(693, 25);
            cboOptimumSourceModel.TabIndex = 2;
            cboOptimumSourceModel.SelectedIndexChanged += cboOptimumSourceModel_SelectedIndexChanged;
            // 
            // btnRefreshOptimumModels
            // 
            btnRefreshOptimumModels.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefreshOptimumModels.Location = new Point(838, 67);
            btnRefreshOptimumModels.Name = "btnRefreshOptimumModels";
            btnRefreshOptimumModels.Size = new Size(145, 34);
            btnRefreshOptimumModels.TabIndex = 3;
            btnRefreshOptimumModels.Text = "Refresh Models";
            btnRefreshOptimumModels.Click += btnRefreshOptimumModels_Click;
            // 
            // lblOptimumTask
            // 
            lblOptimumTask.Location = new Point(20, 118);
            lblOptimumTask.Name = "lblOptimumTask";
            lblOptimumTask.Size = new Size(105, 25);
            lblOptimumTask.TabIndex = 4;
            lblOptimumTask.Text = "Task";
            // 
            // cboOptimumTask
            // 
            cboOptimumTask.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOptimumTask.Items.AddRange(new object[] { "Text generation with memory", "Auto", "Text generation", "Text to text", "Image classification", "Image to text" });
            cboOptimumTask.Location = new Point(135, 115);
            cboOptimumTask.Name = "cboOptimumTask";
            cboOptimumTask.Size = new Size(275, 25);
            cboOptimumTask.TabIndex = 5;
            cboOptimumTask.SelectedIndexChanged += cboOptimumTask_SelectedIndexChanged;
            // 
            // lblOptimumWeights
            // 
            lblOptimumWeights.Location = new Point(430, 118);
            lblOptimumWeights.Name = "lblOptimumWeights";
            lblOptimumWeights.Size = new Size(95, 25);
            lblOptimumWeights.TabIndex = 6;
            lblOptimumWeights.Text = "Weights";
            // 
            // cboOptimumWeightFormat
            // 
            cboOptimumWeightFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOptimumWeightFormat.Items.AddRange(new object[] { "Auto", "INT8", "INT4", "FP16", "FP32" });
            cboOptimumWeightFormat.Location = new Point(530, 115);
            cboOptimumWeightFormat.Name = "cboOptimumWeightFormat";
            cboOptimumWeightFormat.Size = new Size(160, 25);
            cboOptimumWeightFormat.TabIndex = 7;
            cboOptimumWeightFormat.SelectedIndexChanged += cboOptimumWeightFormat_SelectedIndexChanged;
            // 
            // chkOptimumTrustRemoteCode
            // 
            chkOptimumTrustRemoteCode.Location = new Point(135, 160);
            chkOptimumTrustRemoteCode.Name = "chkOptimumTrustRemoteCode";
            chkOptimumTrustRemoteCode.Size = new Size(190, 25);
            chkOptimumTrustRemoteCode.TabIndex = 8;
            chkOptimumTrustRemoteCode.Text = "Allow model code";
            chkOptimumTrustRemoteCode.CheckedChanged += chkOptimumTrustRemoteCode_CheckedChanged;
            // 
            // chkOptimumCreateBlob
            // 
            chkOptimumCreateBlob.Location = new Point(335, 160);
            chkOptimumCreateBlob.Name = "chkOptimumCreateBlob";
            chkOptimumCreateBlob.Size = new Size(185, 25);
            chkOptimumCreateBlob.TabIndex = 9;
            chkOptimumCreateBlob.Text = "Create .blob after export";
            chkOptimumCreateBlob.CheckedChanged += chkOptimumCreateBlob_CheckedChanged;
            // 
            // btnRunOptimumExport
            // 
            btnRunOptimumExport.Location = new Point(20, 205);
            btnRunOptimumExport.Name = "btnRunOptimumExport";
            btnRunOptimumExport.Size = new Size(110, 34);
            btnRunOptimumExport.TabIndex = 10;
            btnRunOptimumExport.Text = "Export";
            btnRunOptimumExport.Click += btnRunOptimumExport_Click;
            // 
            // lblOptimumStatus
            // 
            lblOptimumStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblOptimumStatus.AutoEllipsis = true;
            lblOptimumStatus.ForeColor = SystemColors.GrayText;
            lblOptimumStatus.Location = new Point(145, 206);
            lblOptimumStatus.Name = "lblOptimumStatus";
            lblOptimumStatus.Size = new Size(838, 34);
            lblOptimumStatus.TabIndex = 11;
            lblOptimumStatus.Text = "Choose a downloaded Hugging Face source model, then export it to OpenVINO.";
            lblOptimumStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabConvert
            // 
            tabConvert.Controls.Add(convertLayout);
            tabConvert.Location = new Point(4, 32);
            tabConvert.Name = "tabConvert";
            tabConvert.Padding = new Padding(12);
            tabConvert.Size = new Size(1063, 322);
            tabConvert.TabIndex = 5;
            tabConvert.Text = "Convert";
            // 
            // convertLayout
            // 
            convertLayout.AutoScroll = true;
            convertLayout.Controls.Add(lblOmzBuilderHeading);
            convertLayout.Controls.Add(cboConvertSourceModel);
            convertLayout.Controls.Add(btnRefreshConvertModels);
            convertLayout.Controls.Add(grpConvertOutputs);
            convertLayout.Controls.Add(btnConvertToIr);
            convertLayout.Controls.Add(pnlIrVisualizer);
            convertLayout.Controls.Add(lblModelsStatus);
            convertLayout.Dock = DockStyle.Fill;
            convertLayout.Location = new Point(12, 12);
            convertLayout.Name = "convertLayout";
            convertLayout.Padding = new Padding(10);
            convertLayout.Size = new Size(1039, 298);
            convertLayout.TabIndex = 0;
            // 
            // lblOmzBuilderHeading
            // 
            lblOmzBuilderHeading.AutoSize = true;
            lblOmzBuilderHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOmzBuilderHeading.Location = new Point(20, 25);
            lblOmzBuilderHeading.Name = "lblOmzBuilderHeading";
            lblOmzBuilderHeading.Size = new Size(148, 19);
            lblOmzBuilderHeading.TabIndex = 22;
            lblOmzBuilderHeading.Text = "GGUF model convert";
            // 
            // cboConvertSourceModel
            // 
            cboConvertSourceModel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboConvertSourceModel.DropDownStyle = ComboBoxStyle.DropDownList;
            cboConvertSourceModel.Location = new Point(20, 70);
            cboConvertSourceModel.Name = "cboConvertSourceModel";
            cboConvertSourceModel.Size = new Size(808, 25);
            cboConvertSourceModel.TabIndex = 32;
            cboConvertSourceModel.SelectedIndexChanged += cboConvertSourceModel_SelectedIndexChanged;
            // 
            // btnRefreshConvertModels
            // 
            btnRefreshConvertModels.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefreshConvertModels.Location = new Point(838, 67);
            btnRefreshConvertModels.Name = "btnRefreshConvertModels";
            btnRefreshConvertModels.Size = new Size(145, 34);
            btnRefreshConvertModels.TabIndex = 33;
            btnRefreshConvertModels.Text = "Refresh Models";
            btnRefreshConvertModels.Click += btnRefreshConvertModels_Click;
            // 
            // grpConvertOutputs
            // 
            grpConvertOutputs.Controls.Add(chkConvertBlob);
            grpConvertOutputs.Controls.Add(chkConvertIr);
            grpConvertOutputs.Location = new Point(20, 115);
            grpConvertOutputs.Name = "grpConvertOutputs";
            grpConvertOutputs.Size = new Size(320, 78);
            grpConvertOutputs.TabIndex = 34;
            grpConvertOutputs.TabStop = false;
            grpConvertOutputs.Text = "Create";
            // 
            // chkConvertBlob
            // 
            chkConvertBlob.Location = new Point(18, 32);
            chkConvertBlob.Name = "chkConvertBlob";
            chkConvertBlob.Size = new Size(85, 25);
            chkConvertBlob.TabIndex = 0;
            chkConvertBlob.Text = ".blob";
            chkConvertBlob.CheckedChanged += chkConvertBlob_CheckedChanged;
            // 
            // chkConvertIr
            // 
            chkConvertIr.Checked = true;
            chkConvertIr.CheckState = CheckState.Checked;
            chkConvertIr.Location = new Point(120, 32);
            chkConvertIr.Name = "chkConvertIr";
            chkConvertIr.Size = new Size(110, 25);
            chkConvertIr.TabIndex = 1;
            chkConvertIr.Text = ".xml/.bin";
            chkConvertIr.CheckedChanged += chkConvertIr_CheckedChanged;
            // 
            // btnConvertToIr
            // 
            btnConvertToIr.Location = new Point(20, 215);
            btnConvertToIr.Name = "btnConvertToIr";
            btnConvertToIr.Size = new Size(110, 34);
            btnConvertToIr.TabIndex = 28;
            btnConvertToIr.Text = "Convert";
            btnConvertToIr.Click += btnConvertToIr_Click;
            // 
            // pnlIrVisualizer
            // 
            pnlIrVisualizer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlIrVisualizer.BackColor = Color.FromArgb(251, 251, 251);
            pnlIrVisualizer.BorderStyle = BorderStyle.FixedSingle;
            pnlIrVisualizer.Controls.Add(lblIrPlaceholder);
            pnlIrVisualizer.Location = new Point(20, 270);
            pnlIrVisualizer.Margin = new Padding(3, 8, 3, 8);
            pnlIrVisualizer.Name = "pnlIrVisualizer";
            pnlIrVisualizer.Size = new Size(963, 250);
            pnlIrVisualizer.TabIndex = 29;
            // 
            // lblIrPlaceholder
            // 
            lblIrPlaceholder.AutoSize = true;
            lblIrPlaceholder.ForeColor = SystemColors.GrayText;
            lblIrPlaceholder.Location = new Point(18, 18);
            lblIrPlaceholder.Name = "lblIrPlaceholder";
            lblIrPlaceholder.Size = new Size(328, 19);
            lblIrPlaceholder.TabIndex = 0;
            lblIrPlaceholder.Text = "Select a GGUF model, choose outputs, then convert.";
            // 
            // lblModelsStatus
            // 
            lblModelsStatus.AutoSize = true;
            lblModelsStatus.ForeColor = SystemColors.GrayText;
            lblModelsStatus.Location = new Point(20, 535);
            lblModelsStatus.Name = "lblModelsStatus";
            lblModelsStatus.Size = new Size(359, 19);
            lblModelsStatus.TabIndex = 30;
            lblModelsStatus.Text = "Select a GGUF model from the model root and convert it.";
            // 
            // tabBenchmark
            // 
            tabBenchmark.Controls.Add(benchmarkLayout);
            tabBenchmark.Location = new Point(4, 32);
            tabBenchmark.Name = "tabBenchmark";
            tabBenchmark.Padding = new Padding(12);
            tabBenchmark.Size = new Size(1063, 322);
            tabBenchmark.TabIndex = 2;
            tabBenchmark.Text = "Benchmark";
            // 
            // benchmarkLayout
            // 
            benchmarkLayout.ColumnCount = 3;
            benchmarkLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175F));
            benchmarkLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            benchmarkLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F));
            benchmarkLayout.Controls.Add(lblBenchIterations, 0, 0);
            benchmarkLayout.Controls.Add(numBenchIterations, 1, 0);
            benchmarkLayout.Controls.Add(lblBenchIterationsHelp, 2, 0);
            benchmarkLayout.Controls.Add(lblBenchWarmup, 0, 1);
            benchmarkLayout.Controls.Add(numBenchWarmup, 1, 1);
            benchmarkLayout.Controls.Add(lblBenchWarmupHelp, 2, 1);
            benchmarkLayout.Controls.Add(lblBenchRuns, 0, 2);
            benchmarkLayout.Controls.Add(numBenchRuns, 1, 2);
            benchmarkLayout.Controls.Add(lblBenchRunsHelp, 2, 2);
            benchmarkLayout.Controls.Add(lblPerformanceHint, 0, 3);
            benchmarkLayout.Controls.Add(cboPerformanceHint, 1, 3);
            benchmarkLayout.Controls.Add(btnRunGenericBenchmark, 2, 3);
            benchmarkLayout.Dock = DockStyle.Fill;
            benchmarkLayout.Location = new Point(12, 12);
            benchmarkLayout.Name = "benchmarkLayout";
            benchmarkLayout.RowCount = 4;
            benchmarkLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            benchmarkLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            benchmarkLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            benchmarkLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            benchmarkLayout.Size = new Size(1039, 298);
            benchmarkLayout.TabIndex = 0;
            // 
            // lblBenchIterations
            // 
            lblBenchIterations.AutoSize = true;
            lblBenchIterations.Location = new Point(3, 0);
            lblBenchIterations.Name = "lblBenchIterations";
            lblBenchIterations.Size = new Size(67, 19);
            lblBenchIterations.TabIndex = 5;
            lblBenchIterations.Text = "Iterations";
            // 
            // numBenchIterations
            // 
            numBenchIterations.Location = new Point(178, 3);
            numBenchIterations.Name = "numBenchIterations";
            numBenchIterations.Size = new Size(75, 25);
            numBenchIterations.TabIndex = 6;
            // 
            // lblBenchIterationsHelp
            // 
            lblBenchIterationsHelp.Anchor = AnchorStyles.Left;
            lblBenchIterationsHelp.AutoSize = true;
            lblBenchIterationsHelp.ForeColor = SystemColors.GrayText;
            lblBenchIterationsHelp.Location = new Point(592, 9);
            lblBenchIterationsHelp.Name = "lblBenchIterationsHelp";
            lblBenchIterationsHelp.Size = new Size(283, 19);
            lblBenchIterationsHelp.TabIndex = 7;
            lblBenchIterationsHelp.Text = "Timed inferences performed during each run.";
            // 
            // lblBenchWarmup
            // 
            lblBenchWarmup.AutoSize = true;
            lblBenchWarmup.Location = new Point(3, 38);
            lblBenchWarmup.Name = "lblBenchWarmup";
            lblBenchWarmup.Size = new Size(61, 19);
            lblBenchWarmup.TabIndex = 7;
            lblBenchWarmup.Text = "Warmup";
            // 
            // numBenchWarmup
            // 
            numBenchWarmup.Location = new Point(178, 41);
            numBenchWarmup.Name = "numBenchWarmup";
            numBenchWarmup.Size = new Size(75, 25);
            numBenchWarmup.TabIndex = 8;
            // 
            // lblBenchWarmupHelp
            // 
            lblBenchWarmupHelp.Anchor = AnchorStyles.Left;
            lblBenchWarmupHelp.AutoSize = true;
            lblBenchWarmupHelp.ForeColor = SystemColors.GrayText;
            lblBenchWarmupHelp.Location = new Point(592, 47);
            lblBenchWarmupHelp.Name = "lblBenchWarmupHelp";
            lblBenchWarmupHelp.Size = new Size(367, 19);
            lblBenchWarmupHelp.TabIndex = 9;
            lblBenchWarmupHelp.Text = "Untimed inferences used to stabilize the model and device.";
            // 
            // lblBenchRuns
            // 
            lblBenchRuns.AutoSize = true;
            lblBenchRuns.Location = new Point(3, 76);
            lblBenchRuns.Name = "lblBenchRuns";
            lblBenchRuns.Size = new Size(39, 19);
            lblBenchRuns.TabIndex = 9;
            lblBenchRuns.Text = "Runs";
            // 
            // numBenchRuns
            // 
            numBenchRuns.Location = new Point(178, 79);
            numBenchRuns.Name = "numBenchRuns";
            numBenchRuns.Size = new Size(75, 25);
            numBenchRuns.TabIndex = 10;
            // 
            // lblBenchRunsHelp
            // 
            lblBenchRunsHelp.Anchor = AnchorStyles.Left;
            lblBenchRunsHelp.AutoSize = true;
            lblBenchRunsHelp.ForeColor = SystemColors.GrayText;
            lblBenchRunsHelp.Location = new Point(592, 85);
            lblBenchRunsHelp.Name = "lblBenchRunsHelp";
            lblBenchRunsHelp.Size = new Size(132, 19);
            lblBenchRunsHelp.TabIndex = 11;
            lblBenchRunsHelp.Text = "= Iterations × Runs.";
            // 
            // lblPerformanceHint
            // 
            lblPerformanceHint.AutoSize = true;
            lblPerformanceHint.Location = new Point(3, 114);
            lblPerformanceHint.Name = "lblPerformanceHint";
            lblPerformanceHint.Size = new Size(113, 19);
            lblPerformanceHint.TabIndex = 11;
            lblPerformanceHint.Text = "Performance hint";
            // 
            // cboPerformanceHint
            // 
            cboPerformanceHint.Dock = DockStyle.Fill;
            cboPerformanceHint.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPerformanceHint.Items.AddRange(new object[] { "LATENCY", "THROUGHPUT", "CUMULATIVE_THROUGHPUT" });
            cboPerformanceHint.Location = new Point(178, 117);
            cboPerformanceHint.Name = "cboPerformanceHint";
            cboPerformanceHint.Size = new Size(408, 25);
            cboPerformanceHint.TabIndex = 12;
            cboPerformanceHint.SelectedIndexChanged += cboPerformanceHint_SelectedIndexChanged;
            // 
            // btnRunGenericBenchmark
            // 
            btnRunGenericBenchmark.AutoSize = true;
            btnRunGenericBenchmark.Dock = DockStyle.Top;
            btnRunGenericBenchmark.Location = new Point(592, 117);
            btnRunGenericBenchmark.Name = "btnRunGenericBenchmark";
            btnRunGenericBenchmark.Padding = new Padding(8, 3, 8, 3);
            btnRunGenericBenchmark.Size = new Size(444, 35);
            btnRunGenericBenchmark.TabIndex = 13;
            btnRunGenericBenchmark.Text = "Run Generic Benchmark";
            btnRunGenericBenchmark.Click += btnRunGenericBenchmark_Click;
            // 
            // tabVisualization
            // 
            tabVisualization.Controls.Add(visualizationLayout);
            tabVisualization.Location = new Point(4, 32);
            tabVisualization.Name = "tabVisualization";
            tabVisualization.Padding = new Padding(12);
            tabVisualization.Size = new Size(1063, 322);
            tabVisualization.TabIndex = 3;
            tabVisualization.Text = "Visualization";
            // 
            // visualizationLayout
            // 
            visualizationLayout.ColumnCount = 3;
            visualizationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175F));
            visualizationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            visualizationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            visualizationLayout.Controls.Add(lblVizCsv, 0, 0);
            visualizationLayout.Controls.Add(txtVizCsv, 1, 0);
            visualizationLayout.Controls.Add(btnBrowseVizCsv, 2, 0);
            visualizationLayout.Controls.Add(lblVizMetric, 0, 1);
            visualizationLayout.Controls.Add(cboVizMetric, 1, 1);
            visualizationLayout.Controls.Add(vizButtons, 1, 2);
            visualizationLayout.Controls.Add(lblVizStatus, 0, 3);
            visualizationLayout.Controls.Add(pnlChart, 0, 4);
            visualizationLayout.Dock = DockStyle.Fill;
            visualizationLayout.Location = new Point(12, 12);
            visualizationLayout.Name = "visualizationLayout";
            visualizationLayout.RowCount = 10;
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            visualizationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            visualizationLayout.Size = new Size(1039, 298);
            visualizationLayout.TabIndex = 0;
            // 
            // lblVizCsv
            // 
            lblVizCsv.AutoSize = true;
            lblVizCsv.Location = new Point(3, 0);
            lblVizCsv.Name = "lblVizCsv";
            lblVizCsv.Size = new Size(55, 19);
            lblVizCsv.TabIndex = 0;
            lblVizCsv.Text = "CSV file";
            // 
            // txtVizCsv
            // 
            txtVizCsv.Dock = DockStyle.Fill;
            txtVizCsv.Location = new Point(178, 3);
            txtVizCsv.Name = "txtVizCsv";
            txtVizCsv.Size = new Size(753, 25);
            txtVizCsv.TabIndex = 1;
            // 
            // btnBrowseVizCsv
            // 
            btnBrowseVizCsv.AutoSize = true;
            btnBrowseVizCsv.Location = new Point(937, 3);
            btnBrowseVizCsv.Name = "btnBrowseVizCsv";
            btnBrowseVizCsv.Size = new Size(99, 32);
            btnBrowseVizCsv.TabIndex = 2;
            btnBrowseVizCsv.Text = "Browse...";
            btnBrowseVizCsv.Click += btnBrowseVizCsv_Click;
            // 
            // lblVizMetric
            // 
            lblVizMetric.AutoSize = true;
            lblVizMetric.Location = new Point(3, 38);
            lblVizMetric.Name = "lblVizMetric";
            lblVizMetric.Size = new Size(48, 19);
            lblVizMetric.TabIndex = 3;
            lblVizMetric.Text = "Metric";
            // 
            // cboVizMetric
            // 
            cboVizMetric.Dock = DockStyle.Fill;
            cboVizMetric.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVizMetric.Items.AddRange(new object[] { "throughput", "avg_ms", "median_ms", "p95_ms", "compile_ms", "first_infer_ms" });
            cboVizMetric.Location = new Point(178, 41);
            cboVizMetric.Name = "cboVizMetric";
            cboVizMetric.Size = new Size(753, 25);
            cboVizMetric.TabIndex = 4;
            cboVizMetric.SelectedIndexChanged += cboVizMetric_SelectedIndexChanged;
            // 
            // vizButtons
            // 
            vizButtons.Controls.Add(btnRenderChart);
            vizButtons.Dock = DockStyle.Fill;
            vizButtons.Location = new Point(178, 79);
            vizButtons.Name = "vizButtons";
            vizButtons.Size = new Size(753, 42);
            vizButtons.TabIndex = 5;
            // 
            // btnRenderChart
            // 
            btnRenderChart.Location = new Point(3, 3);
            btnRenderChart.Name = "btnRenderChart";
            btnRenderChart.Size = new Size(135, 36);
            btnRenderChart.TabIndex = 0;
            btnRenderChart.Text = "Render Chart";
            btnRenderChart.Click += btnRenderChart_Click;
            // 
            // lblVizStatus
            // 
            lblVizStatus.AutoSize = true;
            visualizationLayout.SetColumnSpan(lblVizStatus, 3);
            lblVizStatus.ForeColor = SystemColors.GrayText;
            lblVizStatus.Location = new Point(3, 124);
            lblVizStatus.Name = "lblVizStatus";
            lblVizStatus.Size = new Size(286, 19);
            lblVizStatus.TabIndex = 6;
            lblVizStatus.Text = "Load a benchmark CSV and render by metric.";
            // 
            // pnlChart
            // 
            pnlChart.BackColor = Color.White;
            pnlChart.BorderStyle = BorderStyle.FixedSingle;
            visualizationLayout.SetColumnSpan(pnlChart, 3);
            pnlChart.Controls.Add(lblChartPlaceholder);
            pnlChart.Dock = DockStyle.Fill;
            pnlChart.Location = new Point(3, 161);
            pnlChart.Name = "pnlChart";
            pnlChart.Size = new Size(1033, 34);
            pnlChart.TabIndex = 7;
            // 
            // lblChartPlaceholder
            // 
            lblChartPlaceholder.AutoSize = true;
            lblChartPlaceholder.ForeColor = SystemColors.GrayText;
            lblChartPlaceholder.Location = new Point(20, 20);
            lblChartPlaceholder.Name = "lblChartPlaceholder";
            lblChartPlaceholder.Size = new Size(213, 19);
            lblChartPlaceholder.TabIndex = 0;
            lblChartPlaceholder.Text = "Benchmark chart will render here.";
            // 
            // tabTests
            // 
            tabTests.Controls.Add(testsLayout);
            tabTests.Location = new Point(4, 32);
            tabTests.Name = "tabTests";
            tabTests.Padding = new Padding(12);
            tabTests.Size = new Size(1063, 322);
            tabTests.TabIndex = 7;
            tabTests.Text = "Tests";
            // 
            // testsLayout
            // 
            testsLayout.AutoScroll = true;
            testsLayout.ColumnCount = 3;
            testsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            testsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            testsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            testsLayout.Controls.Add(lblTestsHeading, 0, 0);
            testsLayout.Controls.Add(lblTestModelPath, 0, 1);
            testsLayout.Controls.Add(txtTestModelPath, 1, 1);
            testsLayout.Controls.Add(lblInferImage, 0, 2);
            testsLayout.Controls.Add(txtInferImage, 1, 2);
            testsLayout.Controls.Add(imageTestButtons, 2, 2);
            testsLayout.Controls.Add(lblInferText, 0, 3);
            testsLayout.Controls.Add(txtInferText, 1, 3);
            testsLayout.Controls.Add(lblInferLabels, 0, 4);
            testsLayout.Controls.Add(txtInferLabels, 1, 4);
            testsLayout.Controls.Add(btnBrowseInferLabels, 2, 4);
            testsLayout.Controls.Add(lblInferPreset, 0, 5);
            testsLayout.Controls.Add(cboInferPreset, 1, 5);
            testsLayout.Controls.Add(btnInspectOpenVinoModel, 1, 6);
            testsLayout.Controls.Add(lblTestsNote, 1, 7);
            testsLayout.Controls.Add(lblImageResult, 0, 8);
            testsLayout.Controls.Add(picImageResult, 1, 8);
            testsLayout.Controls.Add(btnRunOpenVinoModelTest, 2, 3);
            testsLayout.Dock = DockStyle.Fill;
            testsLayout.Location = new Point(12, 12);
            testsLayout.Name = "testsLayout";
            testsLayout.Padding = new Padding(10);
            testsLayout.RowCount = 9;
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
            testsLayout.Size = new Size(1039, 298);
            testsLayout.TabIndex = 0;
            // 
            // lblTestsHeading
            // 
            lblTestsHeading.AutoSize = true;
            testsLayout.SetColumnSpan(lblTestsHeading, 3);
            lblTestsHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTestsHeading.Location = new Point(13, 10);
            lblTestsHeading.Name = "lblTestsHeading";
            lblTestsHeading.Size = new Size(140, 19);
            lblTestsHeading.TabIndex = 0;
            lblTestsHeading.Text = "Loaded model tests";
            // 
            // lblTestModelPath
            // 
            lblTestModelPath.Anchor = AnchorStyles.Left;
            lblTestModelPath.AutoSize = true;
            lblTestModelPath.Location = new Point(13, 63);
            lblTestModelPath.Name = "lblTestModelPath";
            lblTestModelPath.Size = new Size(96, 19);
            lblTestModelPath.TabIndex = 14;
            lblTestModelPath.Text = "Loaded model";
            // 
            // txtTestModelPath
            // 
            testsLayout.SetColumnSpan(txtTestModelPath, 2);
            txtTestModelPath.Dock = DockStyle.Fill;
            txtTestModelPath.Location = new Point(163, 55);
            txtTestModelPath.Name = "txtTestModelPath";
            txtTestModelPath.ReadOnly = true;
            txtTestModelPath.Size = new Size(846, 25);
            txtTestModelPath.TabIndex = 15;
            // 
            // lblInferImage
            // 
            lblInferImage.Anchor = AnchorStyles.Right;
            lblInferImage.AutoSize = true;
            lblInferImage.Location = new Point(44, 125);
            lblInferImage.Name = "lblInferImage";
            lblInferImage.Size = new Size(113, 19);
            lblInferImage.TabIndex = 3;
            lblInferImage.Text = "Image drop zone";
            // 
            // txtInferImage
            // 
            txtInferImage.AllowDrop = true;
            txtInferImage.Dock = DockStyle.Fill;
            txtInferImage.Location = new Point(163, 96);
            txtInferImage.Margin = new Padding(3, 2, 3, 2);
            txtInferImage.Name = "txtInferImage";
            txtInferImage.PlaceholderText = "Drop one image here, or use Browse...";
            txtInferImage.Size = new Size(696, 25);
            txtInferImage.TabIndex = 4;
            txtInferImage.DragDrop += txtInferImage_DragDrop;
            txtInferImage.DragEnter += txtInferImage_DragEnter;
            txtInferImage.DragLeave += txtInferImage_DragLeave;
            // 
            // imageTestButtons
            // 
            imageTestButtons.Controls.Add(btnBrowseInferImage);
            imageTestButtons.Controls.Add(btnRunImageTest);
            imageTestButtons.Dock = DockStyle.Fill;
            imageTestButtons.FlowDirection = FlowDirection.TopDown;
            imageTestButtons.Location = new Point(865, 95);
            imageTestButtons.Margin = new Padding(3, 1, 3, 1);
            imageTestButtons.Name = "imageTestButtons";
            imageTestButtons.Size = new Size(144, 80);
            imageTestButtons.TabIndex = 5;
            imageTestButtons.WrapContents = false;
            // 
            // btnBrowseInferImage
            // 
            btnBrowseInferImage.Location = new Point(0, 0);
            btnBrowseInferImage.Margin = new Padding(0, 0, 0, 4);
            btnBrowseInferImage.Name = "btnBrowseInferImage";
            btnBrowseInferImage.Size = new Size(144, 34);
            btnBrowseInferImage.TabIndex = 0;
            btnBrowseInferImage.Text = "Browse...";
            btnBrowseInferImage.Click += btnBrowseInferImage_Click;
            // 
            // btnRunImageTest
            // 
            btnRunImageTest.Location = new Point(0, 38);
            btnRunImageTest.Margin = new Padding(0);
            btnRunImageTest.Name = "btnRunImageTest";
            btnRunImageTest.Size = new Size(144, 34);
            btnRunImageTest.TabIndex = 1;
            btnRunImageTest.Text = "Run Image Test";
            btnRunImageTest.Click += btnRunImageTest_Click;
            // 
            // lblInferText
            // 
            lblInferText.Anchor = AnchorStyles.Right;
            lblInferText.AutoSize = true;
            lblInferText.Location = new Point(96, 187);
            lblInferText.Name = "lblInferText";
            lblInferText.Size = new Size(61, 19);
            lblInferText.TabIndex = 6;
            lblInferText.Text = "Test Text";
            lblInferText.Click += lblInferText_Click;
            // 
            // txtInferText
            // 
            txtInferText.Dock = DockStyle.Fill;
            txtInferText.Location = new Point(163, 178);
            txtInferText.Margin = new Padding(3, 2, 3, 2);
            txtInferText.Name = "txtInferText";
            txtInferText.Size = new Size(696, 25);
            txtInferText.TabIndex = 7;
            // 
            // lblInferLabels
            // 
            lblInferLabels.Anchor = AnchorStyles.Right;
            lblInferLabels.Location = new Point(110, 229);
            lblInferLabels.Name = "lblInferLabels";
            lblInferLabels.Size = new Size(47, 19);
            lblInferLabels.TabIndex = 8;
            lblInferLabels.Text = "Labels";
            // 
            // txtInferLabels
            // 
            txtInferLabels.Dock = DockStyle.Fill;
            txtInferLabels.Location = new Point(163, 220);
            txtInferLabels.Margin = new Padding(3, 2, 3, 2);
            txtInferLabels.Name = "txtInferLabels";
            txtInferLabels.Size = new Size(696, 25);
            txtInferLabels.TabIndex = 9;
            // 
            // btnBrowseInferLabels
            // 
            btnBrowseInferLabels.Dock = DockStyle.Fill;
            btnBrowseInferLabels.Location = new Point(865, 219);
            btnBrowseInferLabels.Margin = new Padding(3, 1, 3, 1);
            btnBrowseInferLabels.Name = "btnBrowseInferLabels";
            btnBrowseInferLabels.Size = new Size(144, 40);
            btnBrowseInferLabels.TabIndex = 10;
            btnBrowseInferLabels.Text = "Browse...";
            btnBrowseInferLabels.Click += btnBrowseInferLabels_Click;
            // 
            // lblInferPreset
            // 
            lblInferPreset.Anchor = AnchorStyles.Right;
            lblInferPreset.AutoSize = true;
            lblInferPreset.Location = new Point(67, 271);
            lblInferPreset.Name = "lblInferPreset";
            lblInferPreset.Size = new Size(90, 19);
            lblInferPreset.TabIndex = 13;
            lblInferPreset.Text = "Model Preset";
            lblInferPreset.Click += lblInferPreset_Click;
            // 
            // cboInferPreset
            // 
            testsLayout.SetColumnSpan(cboInferPreset, 2);
            cboInferPreset.Dock = DockStyle.Fill;
            cboInferPreset.DropDownStyle = ComboBoxStyle.DropDownList;
            cboInferPreset.Items.AddRange(new object[] { "None / Generic", "OMZ MobileNet v2 PyTorch" });
            cboInferPreset.Location = new Point(163, 262);
            cboInferPreset.Margin = new Padding(3, 2, 3, 2);
            cboInferPreset.Name = "cboInferPreset";
            cboInferPreset.Size = new Size(846, 25);
            cboInferPreset.TabIndex = 14;
            cboInferPreset.SelectedIndexChanged += cboInferPreset_SelectedIndexChanged;
            // 
            // btnInspectOpenVinoModel
            // 
            btnInspectOpenVinoModel.Location = new Point(163, 305);
            btnInspectOpenVinoModel.Name = "btnInspectOpenVinoModel";
            btnInspectOpenVinoModel.Size = new Size(130, 34);
            btnInspectOpenVinoModel.TabIndex = 18;
            btnInspectOpenVinoModel.Text = "Inspect Loaded Model";
            btnInspectOpenVinoModel.Click += btnInspectOpenVinoModel_Click;
            // 
            // lblTestsNote
            // 
            lblTestsNote.AutoSize = true;
            testsLayout.SetColumnSpan(lblTestsNote, 2);
            lblTestsNote.Location = new Point(163, 348);
            lblTestsNote.Name = "lblTestsNote";
            lblTestsNote.Size = new Size(280, 19);
            lblTestsNote.TabIndex = 20;
            lblTestsNote.Text = "This tab uses the loaded Workstation model.";
            // 
            // lblImageResult
            // 
            lblImageResult.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblImageResult.AutoSize = true;
            lblImageResult.Location = new Point(51, 394);
            lblImageResult.Margin = new Padding(3, 10, 3, 0);
            lblImageResult.Name = "lblImageResult";
            lblImageResult.Size = new Size(106, 19);
            lblImageResult.TabIndex = 22;
            lblImageResult.Text = "Returned image";
            // 
            // picImageResult
            // 
            picImageResult.BackColor = Color.Black;
            picImageResult.BorderStyle = BorderStyle.FixedSingle;
            testsLayout.SetColumnSpan(picImageResult, 2);
            picImageResult.Dock = DockStyle.Fill;
            picImageResult.Location = new Point(163, 387);
            picImageResult.Name = "picImageResult";
            picImageResult.Size = new Size(846, 214);
            picImageResult.SizeMode = PictureBoxSizeMode.Zoom;
            picImageResult.TabIndex = 23;
            picImageResult.TabStop = false;
            // 
            // btnRunOpenVinoModelTest
            // 
            btnRunOpenVinoModelTest.Dock = DockStyle.Fill;
            btnRunOpenVinoModelTest.Location = new Point(865, 179);
            btnRunOpenVinoModelTest.Name = "btnRunOpenVinoModelTest";
            btnRunOpenVinoModelTest.Size = new Size(144, 36);
            btnRunOpenVinoModelTest.TabIndex = 19;
            btnRunOpenVinoModelTest.Text = "Send Message";
            btnRunOpenVinoModelTest.Click += btnRunOpenVinoModelTest_Click;
            // 
            // tabServerTests
            // 
            tabServerTests.Controls.Add(serverTestsLayout);
            tabServerTests.Location = new Point(4, 32);
            tabServerTests.Name = "tabServerTests";
            tabServerTests.Padding = new Padding(12);
            tabServerTests.Size = new Size(1063, 322);
            tabServerTests.TabIndex = 8;
            tabServerTests.Text = "Server Tests";
            // 
            // serverTestsLayout
            // 
            serverTestsLayout.AutoScroll = true;
            serverTestsLayout.ColumnCount = 3;
            serverTestsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            serverTestsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            serverTestsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            serverTestsLayout.Controls.Add(lblOpenVinoRuntimeHeading, 0, 0);
            serverTestsLayout.Controls.Add(txtServerEndpointList, 0, 1);
            serverTestsLayout.Controls.Add(lblTestApiKey, 0, 2);
            serverTestsLayout.Controls.Add(txtTestApiKey, 1, 2);
            serverTestsLayout.Controls.Add(lblServerNoInputHeading, 0, 3);
            serverTestsLayout.Controls.Add(lblTestEndpoint, 0, 4);
            serverTestsLayout.Controls.Add(txtTestEndpoint, 1, 4);
            serverTestsLayout.Controls.Add(btnTestEndpoint, 2, 4);
            serverTestsLayout.Controls.Add(lblModelsEndpoint, 0, 5);
            serverTestsLayout.Controls.Add(txtModelsEndpoint, 1, 5);
            serverTestsLayout.Controls.Add(btnTestModelsEndpoint, 2, 5);
            serverTestsLayout.Controls.Add(lblServerUserInputHeading, 0, 6);
            serverTestsLayout.Controls.Add(lblChatModel, 0, 7);
            serverTestsLayout.Controls.Add(cboServerModel, 1, 7);
            serverTestsLayout.Controls.Add(lblChatPrompt, 0, 8);
            serverTestsLayout.Controls.Add(txtChatPrompt, 1, 8);
            serverTestsLayout.Controls.Add(lblChatEndpoint, 0, 9);
            serverTestsLayout.Controls.Add(txtChatEndpoint, 1, 9);
            serverTestsLayout.Controls.Add(btnTestChat, 2, 9);
            serverTestsLayout.Controls.Add(lblTextEndpoint, 0, 10);
            serverTestsLayout.Controls.Add(txtTextEndpoint, 1, 10);
            serverTestsLayout.Controls.Add(btnTestTextEndpoint, 2, 10);
            serverTestsLayout.Controls.Add(lblImageEndpoint, 0, 11);
            serverTestsLayout.Controls.Add(txtImageEndpoint, 1, 11);
            serverTestsLayout.Controls.Add(btnTestImageEndpoint, 2, 11);
            serverTestsLayout.Controls.Add(btnShowServerSettings, 2, 12);
            serverTestsLayout.Dock = DockStyle.Fill;
            serverTestsLayout.Location = new Point(12, 12);
            serverTestsLayout.Name = "serverTestsLayout";
            serverTestsLayout.Padding = new Padding(10);
            serverTestsLayout.RowCount = 13;
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            serverTestsLayout.Size = new Size(1039, 298);
            serverTestsLayout.TabIndex = 0;
            // 
            // lblOpenVinoRuntimeHeading
            // 
            lblOpenVinoRuntimeHeading.AutoSize = true;
            serverTestsLayout.SetColumnSpan(lblOpenVinoRuntimeHeading, 3);
            lblOpenVinoRuntimeHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOpenVinoRuntimeHeading.Location = new Point(13, 10);
            lblOpenVinoRuntimeHeading.Name = "lblOpenVinoRuntimeHeading";
            lblOpenVinoRuntimeHeading.Size = new Size(102, 19);
            lblOpenVinoRuntimeHeading.TabIndex = 13;
            lblOpenVinoRuntimeHeading.Text = "API endpoints";
            // 
            // txtServerEndpointList
            // 
            serverTestsLayout.SetColumnSpan(txtServerEndpointList, 3);
            txtServerEndpointList.Dock = DockStyle.Fill;
            txtServerEndpointList.Location = new Point(13, 55);
            txtServerEndpointList.Multiline = true;
            txtServerEndpointList.Name = "txtServerEndpointList";
            txtServerEndpointList.ReadOnly = true;
            txtServerEndpointList.ScrollBars = ScrollBars.Vertical;
            txtServerEndpointList.Size = new Size(996, 112);
            txtServerEndpointList.TabIndex = 37;
            txtServerEndpointList.Text = resources.GetString("txtServerEndpointList.Text");
            // 
            // lblTestApiKey
            // 
            lblTestApiKey.Anchor = AnchorStyles.Left;
            lblTestApiKey.AutoSize = true;
            lblTestApiKey.Location = new Point(13, 181);
            lblTestApiKey.Name = "lblTestApiKey";
            lblTestApiKey.Size = new Size(97, 19);
            lblTestApiKey.TabIndex = 4;
            lblTestApiKey.Text = "Server API key";
            // 
            // txtTestApiKey
            // 
            serverTestsLayout.SetColumnSpan(txtTestApiKey, 2);
            txtTestApiKey.Dock = DockStyle.Fill;
            txtTestApiKey.Location = new Point(163, 173);
            txtTestApiKey.Name = "txtTestApiKey";
            txtTestApiKey.Size = new Size(846, 25);
            txtTestApiKey.TabIndex = 5;
            txtTestApiKey.UseSystemPasswordChar = true;
            // 
            // lblServerNoInputHeading
            // 
            lblServerNoInputHeading.AutoSize = true;
            serverTestsLayout.SetColumnSpan(lblServerNoInputHeading, 3);
            lblServerNoInputHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblServerNoInputHeading.Location = new Point(13, 212);
            lblServerNoInputHeading.Name = "lblServerNoInputHeading";
            lblServerNoInputHeading.Size = new Size(262, 19);
            lblServerNoInputHeading.TabIndex = 38;
            lblServerNoInputHeading.Text = "Server tests without prompt or image";
            // 
            // lblTestEndpoint
            // 
            lblTestEndpoint.Anchor = AnchorStyles.Left;
            lblTestEndpoint.AutoSize = true;
            lblTestEndpoint.Location = new Point(13, 265);
            lblTestEndpoint.Name = "lblTestEndpoint";
            lblTestEndpoint.Size = new Size(64, 19);
            lblTestEndpoint.TabIndex = 1;
            lblTestEndpoint.Text = "Tags URL";
            // 
            // txtTestEndpoint
            // 
            txtTestEndpoint.Dock = DockStyle.Fill;
            txtTestEndpoint.Location = new Point(163, 257);
            txtTestEndpoint.Name = "txtTestEndpoint";
            txtTestEndpoint.Size = new Size(696, 25);
            txtTestEndpoint.TabIndex = 2;
            txtTestEndpoint.Text = "http://192.168.0.11:11434/api/tags";
            // 
            // btnTestEndpoint
            // 
            btnTestEndpoint.Location = new Point(865, 257);
            btnTestEndpoint.Name = "btnTestEndpoint";
            btnTestEndpoint.Size = new Size(130, 34);
            btnTestEndpoint.TabIndex = 3;
            btnTestEndpoint.Text = "Test Tags URL";
            btnTestEndpoint.Click += btnTestEndpoint_Click;
            // 
            // lblModelsEndpoint
            // 
            lblModelsEndpoint.Anchor = AnchorStyles.Left;
            lblModelsEndpoint.AutoSize = true;
            lblModelsEndpoint.Location = new Point(13, 307);
            lblModelsEndpoint.Name = "lblModelsEndpoint";
            lblModelsEndpoint.Size = new Size(83, 19);
            lblModelsEndpoint.TabIndex = 40;
            lblModelsEndpoint.Text = "Models URL";
            // 
            // txtModelsEndpoint
            // 
            txtModelsEndpoint.Dock = DockStyle.Fill;
            txtModelsEndpoint.Location = new Point(163, 299);
            txtModelsEndpoint.Name = "txtModelsEndpoint";
            txtModelsEndpoint.Size = new Size(696, 25);
            txtModelsEndpoint.TabIndex = 41;
            txtModelsEndpoint.Text = "http://192.168.0.11:11434/v1/models";
            // 
            // btnTestModelsEndpoint
            // 
            btnTestModelsEndpoint.Location = new Point(865, 299);
            btnTestModelsEndpoint.Name = "btnTestModelsEndpoint";
            btnTestModelsEndpoint.Size = new Size(130, 34);
            btnTestModelsEndpoint.TabIndex = 42;
            btnTestModelsEndpoint.Text = "Test Models";
            btnTestModelsEndpoint.Click += btnTestModelsEndpoint_Click;
            // 
            // lblServerUserInputHeading
            // 
            lblServerUserInputHeading.AutoSize = true;
            serverTestsLayout.SetColumnSpan(lblServerUserInputHeading, 3);
            lblServerUserInputHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblServerUserInputHeading.Location = new Point(13, 338);
            lblServerUserInputHeading.Name = "lblServerUserInputHeading";
            lblServerUserInputHeading.Size = new Size(240, 19);
            lblServerUserInputHeading.TabIndex = 39;
            lblServerUserInputHeading.Text = "Server tests with prompt or image";
            // 
            // lblChatModel
            // 
            lblChatModel.Anchor = AnchorStyles.Left;
            lblChatModel.AutoSize = true;
            lblChatModel.Location = new Point(13, 391);
            lblChatModel.Name = "lblChatModel";
            lblChatModel.Size = new Size(127, 19);
            lblChatModel.TabIndex = 8;
            lblChatModel.Text = "Server model name";
            // 
            // cboServerModel
            // 
            cboServerModel.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboServerModel.AutoCompleteSource = AutoCompleteSource.ListItems;
            serverTestsLayout.SetColumnSpan(cboServerModel, 2);
            cboServerModel.Dock = DockStyle.Fill;
            cboServerModel.FormattingEnabled = true;
            cboServerModel.Location = new Point(163, 383);
            cboServerModel.Name = "cboServerModel";
            cboServerModel.Size = new Size(846, 25);
            cboServerModel.TabIndex = 9;
            // 
            // lblChatPrompt
            // 
            lblChatPrompt.Anchor = AnchorStyles.Left;
            lblChatPrompt.AutoSize = true;
            lblChatPrompt.Location = new Point(13, 467);
            lblChatPrompt.Name = "lblChatPrompt";
            lblChatPrompt.Size = new Size(97, 19);
            lblChatPrompt.TabIndex = 10;
            lblChatPrompt.Text = "Server prompt";
            // 
            // txtChatPrompt
            // 
            serverTestsLayout.SetColumnSpan(txtChatPrompt, 2);
            txtChatPrompt.Dock = DockStyle.Fill;
            txtChatPrompt.Location = new Point(163, 425);
            txtChatPrompt.Multiline = true;
            txtChatPrompt.Name = "txtChatPrompt";
            txtChatPrompt.ScrollBars = ScrollBars.Vertical;
            txtChatPrompt.Size = new Size(846, 104);
            txtChatPrompt.TabIndex = 11;
            txtChatPrompt.Text = "Reply with one short sentence confirming the chat endpoint is working.";
            // 
            // lblChatEndpoint
            // 
            lblChatEndpoint.Anchor = AnchorStyles.Left;
            lblChatEndpoint.AutoSize = true;
            lblChatEndpoint.Location = new Point(13, 543);
            lblChatEndpoint.Name = "lblChatEndpoint";
            lblChatEndpoint.Size = new Size(115, 19);
            lblChatEndpoint.TabIndex = 6;
            lblChatEndpoint.Text = "Completions URL";
            // 
            // txtChatEndpoint
            // 
            txtChatEndpoint.Dock = DockStyle.Fill;
            txtChatEndpoint.Location = new Point(163, 535);
            txtChatEndpoint.Name = "txtChatEndpoint";
            txtChatEndpoint.Size = new Size(696, 25);
            txtChatEndpoint.TabIndex = 7;
            txtChatEndpoint.Text = "http://192.168.0.11:11434/v1/chat/completions";
            // 
            // btnTestChat
            // 
            btnTestChat.Location = new Point(865, 535);
            btnTestChat.Name = "btnTestChat";
            btnTestChat.Size = new Size(130, 34);
            btnTestChat.TabIndex = 12;
            btnTestChat.Text = "Test Completions";
            btnTestChat.Click += btnTestChat_Click;
            // 
            // lblTextEndpoint
            // 
            lblTextEndpoint.Anchor = AnchorStyles.Left;
            lblTextEndpoint.AutoSize = true;
            lblTextEndpoint.Location = new Point(13, 585);
            lblTextEndpoint.Name = "lblTextEndpoint";
            lblTextEndpoint.Size = new Size(62, 19);
            lblTextEndpoint.TabIndex = 31;
            lblTextEndpoint.Text = "Text URL";
            // 
            // txtTextEndpoint
            // 
            txtTextEndpoint.Dock = DockStyle.Fill;
            txtTextEndpoint.Location = new Point(163, 577);
            txtTextEndpoint.Name = "txtTextEndpoint";
            txtTextEndpoint.Size = new Size(696, 25);
            txtTextEndpoint.TabIndex = 32;
            txtTextEndpoint.Text = "http://192.168.0.11:11434/v1/completions";
            // 
            // btnTestTextEndpoint
            // 
            btnTestTextEndpoint.Location = new Point(865, 577);
            btnTestTextEndpoint.Name = "btnTestTextEndpoint";
            btnTestTextEndpoint.Size = new Size(130, 34);
            btnTestTextEndpoint.TabIndex = 33;
            btnTestTextEndpoint.Text = "Test Text";
            btnTestTextEndpoint.Click += btnTestTextEndpoint_Click;
            // 
            // lblImageEndpoint
            // 
            lblImageEndpoint.Anchor = AnchorStyles.Left;
            lblImageEndpoint.AutoSize = true;
            lblImageEndpoint.Location = new Point(13, 627);
            lblImageEndpoint.Name = "lblImageEndpoint";
            lblImageEndpoint.Size = new Size(76, 19);
            lblImageEndpoint.TabIndex = 34;
            lblImageEndpoint.Text = "Image URL";
            // 
            // txtImageEndpoint
            // 
            txtImageEndpoint.Dock = DockStyle.Fill;
            txtImageEndpoint.Location = new Point(163, 619);
            txtImageEndpoint.Name = "txtImageEndpoint";
            txtImageEndpoint.Size = new Size(696, 25);
            txtImageEndpoint.TabIndex = 35;
            txtImageEndpoint.Text = "http://192.168.0.11:11434/v1/images/generations";
            // 
            // btnTestImageEndpoint
            // 
            btnTestImageEndpoint.Location = new Point(865, 619);
            btnTestImageEndpoint.Name = "btnTestImageEndpoint";
            btnTestImageEndpoint.Size = new Size(130, 34);
            btnTestImageEndpoint.TabIndex = 36;
            btnTestImageEndpoint.Text = "Test Image";
            btnTestImageEndpoint.Click += btnTestImageEndpoint_Click;
            // 
            // btnShowServerSettings
            // 
            btnShowServerSettings.Location = new Point(865, 661);
            btnShowServerSettings.Name = "btnShowServerSettings";
            btnShowServerSettings.Size = new Size(130, 34);
            btnShowServerSettings.TabIndex = 21;
            btnShowServerSettings.Text = "Show URLs";
            btnShowServerSettings.Click += btnShowServerSettings_Click;
            // 
            // outputLayout
            // 
            outputLayout.ColumnCount = 1;
            outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            outputLayout.Controls.Add(grpOutput, 0, 0);
            outputLayout.Controls.Add(outputButtons, 0, 1);
            outputLayout.Dock = DockStyle.Fill;
            outputLayout.Location = new Point(0, 0);
            outputLayout.Name = "outputLayout";
            outputLayout.RowCount = 2;
            outputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            outputLayout.Size = new Size(1071, 417);
            outputLayout.TabIndex = 0;
            // 
            // grpOutput
            // 
            grpOutput.Controls.Add(outputContentLayout);
            grpOutput.Dock = DockStyle.Fill;
            grpOutput.Location = new Point(3, 3);
            grpOutput.Name = "grpOutput";
            grpOutput.Size = new Size(1065, 353);
            grpOutput.TabIndex = 0;
            grpOutput.TabStop = false;
            grpOutput.Text = "Output";
            // 
            // outputContentLayout
            // 
            outputContentLayout.ColumnCount = 1;
            outputContentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            outputContentLayout.Controls.Add(lblOutputInfo, 0, 0);
            outputContentLayout.Controls.Add(txtOutput, 0, 1);
            outputContentLayout.Dock = DockStyle.Fill;
            outputContentLayout.Location = new Point(3, 19);
            outputContentLayout.Name = "outputContentLayout";
            outputContentLayout.RowCount = 2;
            outputContentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            outputContentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outputContentLayout.Size = new Size(1059, 331);
            outputContentLayout.TabIndex = 0;
            // 
            // lblOutputInfo
            // 
            lblOutputInfo.AutoSize = true;
            lblOutputInfo.Dock = DockStyle.Fill;
            lblOutputInfo.Location = new Point(3, 0);
            lblOutputInfo.Name = "lblOutputInfo";
            lblOutputInfo.Size = new Size(1053, 24);
            lblOutputInfo.TabIndex = 0;
            lblOutputInfo.Text = "Messages, help, errors, download status, model details, and benchmark results appear here.";
            lblOutputInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtOutput
            // 
            txtOutput.BackColor = Color.FromArgb(20, 20, 20);
            txtOutput.BorderStyle = BorderStyle.None;
            txtOutput.Dock = DockStyle.Fill;
            txtOutput.Font = new Font("Cascadia Mono", 9F);
            txtOutput.ForeColor = Color.Gainsboro;
            txtOutput.Location = new Point(3, 27);
            txtOutput.Name = "txtOutput";
            txtOutput.ReadOnly = true;
            txtOutput.Size = new Size(1053, 301);
            txtOutput.TabIndex = 1;
            txtOutput.Text = "OpenVINO Workbench layout initialized.\n";
            // 
            // outputButtons
            // 
            outputButtons.ColumnCount = 3;
            outputButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 156F));
            outputButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            outputButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 106F));
            outputButtons.Controls.Add(btnClearOutput, 0, 0);
            outputButtons.Controls.Add(activityLayout, 1, 0);
            outputButtons.Controls.Add(btnHelp, 2, 0);
            outputButtons.Dock = DockStyle.Fill;
            outputButtons.Location = new Point(3, 362);
            outputButtons.Name = "outputButtons";
            outputButtons.Padding = new Padding(0, 6, 0, 0);
            outputButtons.RowCount = 1;
            outputButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outputButtons.Size = new Size(1065, 52);
            outputButtons.TabIndex = 1;
            // 
            // btnClearOutput
            // 
            btnClearOutput.Location = new Point(3, 9);
            btnClearOutput.Name = "btnClearOutput";
            btnClearOutput.Size = new Size(150, 36);
            btnClearOutput.TabIndex = 0;
            btnClearOutput.Text = "Clear Output";
            btnClearOutput.Click += btnClearOutput_Click;
            // 
            // activityLayout
            // 
            activityLayout.ColumnCount = 2;
            activityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            activityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            activityLayout.Controls.Add(lblActivityStatus, 0, 0);
            activityLayout.Controls.Add(progressActivity, 0, 1);
            activityLayout.Controls.Add(btnCancelActivity, 1, 0);
            activityLayout.Dock = DockStyle.Fill;
            activityLayout.Location = new Point(159, 9);
            activityLayout.Margin = new Padding(3, 3, 8, 3);
            activityLayout.Name = "activityLayout";
            activityLayout.RowCount = 2;
            activityLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            activityLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
            activityLayout.Size = new Size(792, 40);
            activityLayout.TabIndex = 2;
            activityLayout.Visible = false;
            // 
            // lblActivityStatus
            // 
            lblActivityStatus.AutoEllipsis = true;
            lblActivityStatus.Dock = DockStyle.Fill;
            lblActivityStatus.Location = new Point(3, 0);
            lblActivityStatus.Name = "lblActivityStatus";
            lblActivityStatus.Size = new Size(690, 20);
            lblActivityStatus.TabIndex = 0;
            lblActivityStatus.Text = "Working...";
            lblActivityStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressActivity
            // 
            progressActivity.Dock = DockStyle.Fill;
            progressActivity.Location = new Point(3, 22);
            progressActivity.Margin = new Padding(3, 2, 3, 0);
            progressActivity.MarqueeAnimationSpeed = 28;
            progressActivity.Name = "progressActivity";
            progressActivity.Size = new Size(690, 18);
            progressActivity.Style = ProgressBarStyle.Marquee;
            progressActivity.TabIndex = 1;
            // 
            // btnCancelActivity
            // 
            btnCancelActivity.Dock = DockStyle.Fill;
            btnCancelActivity.Enabled = false;
            btnCancelActivity.Location = new Point(699, 0);
            btnCancelActivity.Margin = new Padding(3, 0, 3, 0);
            btnCancelActivity.Name = "btnCancelActivity";
            activityLayout.SetRowSpan(btnCancelActivity, 2);
            btnCancelActivity.Size = new Size(90, 40);
            btnCancelActivity.TabIndex = 2;
            btnCancelActivity.Text = "Cancel";
            btnCancelActivity.Visible = false;
            btnCancelActivity.Click += btnCancelActivity_Click;
            // 
            // btnHelp
            // 
            btnHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHelp.Location = new Point(962, 9);
            btnHelp.Name = "btnHelp";
            btnHelp.Size = new Size(100, 36);
            btnHelp.TabIndex = 2;
            btnHelp.Text = "Help";
            btnHelp.Click += btnHelp_Click;
            // 
            // btnDeleteDownload
            // 
            btnDeleteDownload.Location = new Point(935, 510);
            btnDeleteDownload.Name = "btnDeleteDownload";
            btnDeleteDownload.Size = new Size(130, 34);
            btnDeleteDownload.TabIndex = 32;
            btnDeleteDownload.Text = "Delete Download";
            btnDeleteDownload.Click += btnDeleteDownload_Click;
            // 
            // 
            // lstHfRepos
            // 
            lstHfRepos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstHfRepos.Columns.AddRange(new ColumnHeader[] { colModelId, colDownloads, colLikes, colModelSource, colTask, colLibrary, colQuantization });
            lstHfRepos.FullRowSelect = true;
            lstHfRepos.GridLines = true;
            lstHfRepos.Location = new Point(20, 150);
            lstHfRepos.MultiSelect = false;
            lstHfRepos.Name = "lstHfRepos";
            lstHfRepos.Size = new Size(1055, 130);
            lstHfRepos.TabIndex = 15;
            lstHfRepos.UseCompatibleStateImageBehavior = false;
            lstHfRepos.View = View.Details;
            lstHfRepos.DrawColumnHeader += modelListView_DrawColumnHeader;
            lstHfRepos.DrawSubItem += modelListView_DrawSubItem;
            lstHfRepos.SelectedIndexChanged += lstHfRepos_SelectedIndexChanged;
            // 
            // colModelId
            // 
            colModelId.Text = "Model";
            colModelId.Width = 350;
            // 
            // colDownloads
            // 
            colDownloads.Text = "Downloads";
            colDownloads.TextAlign = HorizontalAlignment.Right;
            colDownloads.Width = 90;
            // 
            // colLikes
            // 
            colLikes.Text = "Likes";
            colLikes.TextAlign = HorizontalAlignment.Right;
            colLikes.Width = 70;
            // 
            // colModelSource
            // 
            colModelSource.Text = "Source";
            colModelSource.Width = 120;
            // 
            // colTask
            // 
            colTask.Text = "Task";
            colTask.Width = 140;
            // 
            // colLibrary
            // 
            colLibrary.Text = "Library";
            colLibrary.Width = 135;
            // 
            // colQuantization
            // 
            colQuantization.Text = "Quant";
            colQuantization.Width = 110;
            // 
            // btnDownloadHelp
            // 
            btnDownloadHelp.Location = new Point(960, 296);
            btnDownloadHelp.Name = "btnDownloadHelp";
            btnDownloadHelp.Size = new Size(90, 34);
            btnDownloadHelp.TabIndex = 33;
            btnDownloadHelp.Text = "Help";
            btnDownloadHelp.Click += btnDownloadHelp_Click;
            // 
            // lblRepoId
            // 
            lblRepoId.Location = new Point(20, 300);
            lblRepoId.Name = "lblRepoId";
            lblRepoId.Size = new Size(65, 25);
            lblRepoId.TabIndex = 14;
            lblRepoId.Text = "Repo ID";
            lblRepoId.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtRepoId
            // 
            txtRepoId.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRepoId.Location = new Point(90, 300);
            txtRepoId.Name = "txtRepoId";
            txtRepoId.Size = new Size(320, 23);
            txtRepoId.TabIndex = 15;
            txtRepoId.TextChanged += txtRepoId_TextChanged;
            // 
            // lblRevision
            // 
            lblRevision.Location = new Point(430, 300);
            lblRevision.Name = "lblRevision";
            lblRevision.Size = new Size(70, 25);
            lblRevision.TabIndex = 16;
            lblRevision.Text = "Revision";
            lblRevision.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtRevision
            // 
            txtRevision.Location = new Point(505, 300);
            txtRevision.Name = "txtRevision";
            txtRevision.Size = new Size(150, 23);
            txtRevision.TabIndex = 17;
            txtRevision.Text = "main";
            txtRevision.TextChanged += txtRevision_TextChanged;
            // 
            // lstHfFiles
            // 
            lstHfFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstHfFiles.Columns.AddRange(new ColumnHeader[] { colFilePath, colFileType, colFileExtension, colFileSize, colFileNotes });
            lstHfFiles.FullRowSelect = true;
            lstHfFiles.GridLines = true;
            lstHfFiles.Location = new Point(20, 370);
            lstHfFiles.MultiSelect = false;
            lstHfFiles.Name = "lstHfFiles";
            lstHfFiles.Size = new Size(1055, 130);
            lstHfFiles.TabIndex = 21;
            lstHfFiles.UseCompatibleStateImageBehavior = false;
            lstHfFiles.View = View.Details;
            lstHfFiles.DrawColumnHeader += modelListView_DrawColumnHeader;
            lstHfFiles.DrawSubItem += modelListView_DrawSubItem;
            lstHfFiles.SelectedIndexChanged += lstHfFiles_SelectedIndexChanged;
            // 
            // colFilePath
            // 
            colFilePath.Text = "Path";
            colFilePath.Width = 580;
            // 
            // colFileType
            // 
            colFileType.Text = "Type";
            colFileType.Width = 120;
            // 
            // colFileExtension
            // 
            colFileExtension.Text = "Extension";
            colFileExtension.Width = 90;
            // 
            // colFileSize
            // 
            colFileSize.Text = "Size";
            colFileSize.TextAlign = HorizontalAlignment.Right;
            colFileSize.Width = 110;
            // 
            // colFileNotes
            // 
            colFileNotes.Text = "Notes";
            colFileNotes.Width = 150;
            // 
            // modelsSeparator2
            // 
            modelsSeparator2.Location = new Point(13, 605);
            modelsSeparator2.Name = "modelsSeparator2";
            modelsSeparator2.Size = new Size(100, 23);
            modelsSeparator2.TabIndex = 21;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1103, 881);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(570, 307);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "OpenVINO Workbench";
            Shown += Form1_Shown;
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            mainSplit.Panel1.ResumeLayout(false);
            mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
            mainSplit.ResumeLayout(false);
            tabMain.ResumeLayout(false);
            tabWorkstation.ResumeLayout(false);
            workstationLayout.ResumeLayout(false);
            workstationLayout.PerformLayout();
            modelRootLayout.ResumeLayout(false);
            modelRootLayout.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabDownloads.ResumeLayout(false);
            modelsScroll.ResumeLayout(false);
            modelsLayout.ResumeLayout(false);
            modelsLayout.PerformLayout();
            tabLoadModel.ResumeLayout(false);
            loadModelLayout.ResumeLayout(false);
            loadModelLayout.PerformLayout();
            tabConvert.ResumeLayout(false);
            convertLayout.ResumeLayout(false);
            convertLayout.PerformLayout();
            grpConvertOutputs.ResumeLayout(false);
            pnlIrVisualizer.ResumeLayout(false);
            pnlIrVisualizer.PerformLayout();
            tabBenchmark.ResumeLayout(false);
            benchmarkLayout.ResumeLayout(false);
            benchmarkLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBenchIterations).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBenchWarmup).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBenchRuns).EndInit();
            tabVisualization.ResumeLayout(false);
            visualizationLayout.ResumeLayout(false);
            visualizationLayout.PerformLayout();
            vizButtons.ResumeLayout(false);
            pnlChart.ResumeLayout(false);
            pnlChart.PerformLayout();
            tabTests.ResumeLayout(false);
            testsLayout.ResumeLayout(false);
            testsLayout.PerformLayout();
            imageTestButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picImageResult).EndInit();
            tabServerTests.ResumeLayout(false);
            serverTestsLayout.ResumeLayout(false);
            serverTestsLayout.PerformLayout();
            outputLayout.ResumeLayout(false);
            grpOutput.ResumeLayout(false);
            outputContentLayout.ResumeLayout(false);
            outputContentLayout.PerformLayout();
            outputButtons.ResumeLayout(false);
            activityLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static void ConfigureThreeColumnLayout(TableLayoutPanel layout)
        {
            layout.ColumnCount = 3;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 10;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        }

        private static void ConfigureNumber(NumericUpDown control, decimal minimum, decimal maximum, decimal value)
        {
            control.Minimum = minimum; control.Maximum = maximum; control.Value = value; control.Width = 130; control.Anchor = AnchorStyles.Left;
        }

        private static void ConfigureSeparator(Label separator)
        {
            separator.BorderStyle = BorderStyle.Fixed3D; separator.Dock = DockStyle.Fill; separator.Height = 2; separator.Margin = new Padding(0, 12, 0, 12);
        }

        #endregion

        private TableLayoutPanel mainLayout; private Label lblTitle; private Label lblSubtitle; private SplitContainer mainSplit; private TabControl tabMain;
        private TabPage tabWorkstation; private TableLayoutPanel workstationLayout; private Label lblWorkstationHeading; private TableLayoutPanel modelRootLayout; private Label lblModelRoot; private TextBox txtModelRoot; private Button btnBrowseModelRoot; private Button btnCheckWorkstation; private Label lblWorkstationTarget; private ComboBox cboWorkstationTarget; private Label lblWorkstationMultiTarget; private ComboBox cboWorkstationMultiTarget; private Label lblModelCapabilities; private TextBox txtModelCapabilities;
private Label lblInferModel; private TextBox txtInferModel; private Button btnBrowseInferModel; private Button btnUseSampleModel; private Label lblInferImage; private TextBox txtInferImage; private FlowLayoutPanel imageTestButtons; private Button btnBrowseInferImage; private Button btnRunImageTest; private Label lblInferText; private TextBox txtInferText; private Button btnBrowseInferLabels; private Label lblInferLabels; private TextBox txtInferLabels; private Label lblInferPreset; private ComboBox cboInferPreset;         private TabPage tabBenchmark; private TableLayoutPanel benchmarkLayout; private Label lblBenchIterations; private NumericUpDown numBenchIterations; private Label lblBenchIterationsHelp; private Label lblBenchWarmup; private NumericUpDown numBenchWarmup; private Label lblBenchWarmupHelp; private Label lblBenchRuns; private NumericUpDown numBenchRuns; private Label lblBenchRunsHelp; private Label lblPerformanceHint; private ComboBox cboPerformanceHint; private Button btnRunGenericBenchmark;         private TabPage tabVisualization; private TableLayoutPanel visualizationLayout; private Label lblVizCsv; private TextBox txtVizCsv; private Button btnBrowseVizCsv; private Label lblVizMetric; private ComboBox cboVizMetric; private FlowLayoutPanel vizButtons; private Button btnRenderChart; private Label lblVizStatus; private Panel pnlChart; private Label lblChartPlaceholder;
        private TabPage tabDownloads; private Panel modelsScroll; private Panel modelsLayout; private TabPage tabConvert; private Panel convertLayout; private TabPage tabLoadModel; private Panel loadModelLayout; private Label lblOptimumHeading; private Label lblOptimumSource; private ComboBox cboOptimumSourceModel; private Button btnRefreshOptimumModels; private Label lblOptimumTask; private ComboBox cboOptimumTask; private Label lblOptimumWeights; private ComboBox cboOptimumWeightFormat; private CheckBox chkOptimumTrustRemoteCode; private CheckBox chkOptimumCreateBlob; private Button btnRunOptimumExport; private Label lblOptimumStatus; private Button btnDeleteDownload; private Label lblModelWebsites; private ComboBox cboModelWebsites; private Button btnOpenModelWebsite; private Button btnAddModelWebsite; private Button btnDeleteModelWebsite; private Label lblHfQuery; private TextBox txtHfQuery; private Button btnSearchRepos; private Button btnSearchAllModels; private Button btnPresetHuggingFace; private Button btnPresetOpenVino; private Button btnPresetIntel; private Button btnPresetUltralytics; private Button btnPresetOllama; private Button btnOfficialFiles; private Button btnRefreshOfficialIndex; private ListView lstHfRepos; private ColumnHeader colModelId; private ColumnHeader colDownloads; private ColumnHeader colLikes; private ColumnHeader colModelSource; private ColumnHeader colTask; private ColumnHeader colLibrary; private ColumnHeader colQuantization; private Button btnLoadFiles; private Button btnClearFileSelection; private Button btnDownloadHelp; private Label lblRepoId; private TextBox txtRepoId; private Label lblRevision; private TextBox txtRevision; private Label lblDownloadUrl; private TextBox txtDownloadUrl; private Button btnCopyDownloadUrl; private ListView lstHfFiles; private ColumnHeader colFilePath; private ColumnHeader colFileType; private ColumnHeader colFileExtension; private ColumnHeader colFileSize; private ColumnHeader colFileNotes; private ProgressBar progressDownload; private Label lblDownloadStatus; private Button btnCancelDownload; private Label modelsSeparator2; private Label lblOmzBuilderHeading; private ComboBox cboConvertSourceModel; private Button btnRefreshConvertModels; private GroupBox grpConvertOutputs; private CheckBox chkConvertBlob; private CheckBox chkConvertIr; private Button btnConvertToIr; private Panel pnlIrVisualizer; private Label lblIrPlaceholder; private Label lblModelsStatus;
        private TabPage tabTests; private TableLayoutPanel testsLayout; private Label lblTestsHeading; private Label lblTestModelPath; private TextBox txtTestModelPath; private Button btnInspectOpenVinoModel; private Button btnRunOpenVinoModelTest; private Label lblTestsNote; private Label lblImageResult; private PictureBox picImageResult; private TabPage tabServerTests; private TableLayoutPanel serverTestsLayout; private TextBox txtServerEndpointList; private Label lblServerNoInputHeading; private Label lblServerUserInputHeading; private Label lblTestEndpoint; private TextBox txtTestEndpoint; private Button btnTestEndpoint; private Label lblModelsEndpoint; private TextBox txtModelsEndpoint; private Button btnTestModelsEndpoint; private Label lblTestApiKey; private TextBox txtTestApiKey; private Label lblChatEndpoint; private TextBox txtChatEndpoint; private Label lblTextEndpoint; private TextBox txtTextEndpoint; private Button btnTestTextEndpoint; private Label lblImageEndpoint; private TextBox txtImageEndpoint; private Button btnTestImageEndpoint; private Label lblChatModel; private ComboBox cboServerModel; private Label lblChatPrompt; private TextBox txtChatPrompt; private Button btnTestChat; private Button btnShowServerSettings; private Label lblOpenVinoRuntimeHeading; private Label lblLocalServerUrl; private TextBox txtLocalServerUrl; private Button btnStartLocalServer; private Label lblLocalServerStatus; private TextBox txtLocalServerStatus; private Button btnStopLocalServer;
        private TableLayoutPanel outputLayout; private GroupBox grpOutput; private TableLayoutPanel outputContentLayout; private Label lblOutputInfo; private RichTextBox txtOutput; private TableLayoutPanel outputButtons; private Button btnClearOutput; private TableLayoutPanel activityLayout; private Label lblActivityStatus; private ProgressBar progressActivity; private Button btnCancelActivity; private Button btnHelp;
        private Button btnLoadModel;
        private SplitContainer splitContainer1;
    }
}






