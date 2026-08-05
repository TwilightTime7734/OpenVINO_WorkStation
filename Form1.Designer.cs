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
            btnCheckWorkstation = new Button();
            lblWorkstationMultiTarget = new Label();
            cboWorkstationMultiTarget = new ComboBox();
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
            tabConvert = new TabPage();
            convertLayout = new Panel();
            lblOmzBuilderHeading = new Label();
            cboConvertSourceModel = new ComboBox();
            btnRefreshConvertModels = new Button();
            grpConvertOutputs = new GroupBox();
            chkConvertBlob = new CheckBox();
            btnConvertToIr = new Button();
            pnlIrVisualizer = new Panel();
            lblIrPlaceholder = new Label();
            lblModelsStatus = new Label();
            tabOvmsPull = new TabPage();
            ovmsPullLayout = new Panel();
            lblOvmsPullHeading = new Label();
            lblOvmsExecutable = new Label();
            txtOvmsExecutable = new TextBox();
            lblOvmsPythonPath = new Label();
            txtOvmsPythonPath = new TextBox();
            lblOvmsExportToolsPath = new Label();
            txtOvmsExportToolsPath = new TextBox();
            lblOvmsSourceFormat = new Label();
            cboOvmsSourceFormat = new ComboBox();
            lblOvmsTask = new Label();
            cboOvmsTask = new ComboBox();
            lblOvmsPipelineType = new Label();
            cboOvmsPipelineType = new ComboBox();
            lblOvmsWeightFormat = new Label();
            cboOvmsWeightFormat = new ComboBox();
            lblOvmsSourceModel = new Label();
            cboOvmsSourceModel = new ComboBox();
            lblOvmsGgufFilename = new Label();
            txtOvmsGgufFilename = new TextBox();
            lblOvmsModelRepository = new Label();
            txtOvmsModelRepository = new TextBox();
            lblOvmsLogPath = new Label();
            txtOvmsLogPath = new TextBox();
            chkOvmsOverwrite = new CheckBox();
            chkOvmsDebug = new CheckBox();
            btnCheckOvmsVersions = new Button();
            btnPreviewOvmsPull = new Button();
            btnRunOvmsPull = new Button();
            lblOvmsPullStatus = new Label();
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
            tabConvert.SuspendLayout();
            convertLayout.SuspendLayout();
            grpConvertOutputs.SuspendLayout();
            pnlIrVisualizer.SuspendLayout();
            tabOvmsPull.SuspendLayout();
            ovmsPullLayout.SuspendLayout();
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
            mainLayout.Margin = new Padding(4, 5, 4, 5);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(17, 17, 17, 20);
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 57F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 47F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.Size = new Size(1576, 1468);
            mainLayout.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            lblTitle.Location = new Point(21, 17);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(385, 46);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "OpenVINO Workbench";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.ForeColor = SystemColors.GrayText;
            lblSubtitle.Location = new Point(21, 74);
            lblSubtitle.Margin = new Padding(4, 0, 4, 0);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(753, 25);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Tabbed runner for device checks, model tests, conversion, downloads, and local server checks.";
            // 
            // mainSplit
            // 
            mainSplit.BorderStyle = BorderStyle.FixedSingle;
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.FixedPanel = FixedPanel.Panel2;
            mainSplit.Location = new Point(21, 126);
            mainSplit.Margin = new Padding(4, 5, 4, 5);
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
            mainSplit.Size = new Size(1534, 1317);
            mainSplit.SplitterDistance = 521;
            mainSplit.SplitterWidth = 20;
            mainSplit.TabIndex = 2;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabWorkstation);
            tabMain.Controls.Add(tabDownloads);
            tabMain.Controls.Add(tabConvert);
            tabMain.Controls.Add(tabOvmsPull);
            tabMain.Controls.Add(tabTests);
            tabMain.Controls.Add(tabServerTests);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("Segoe UI", 10F);
            tabMain.Location = new Point(0, 0);
            tabMain.Margin = new Padding(4, 5, 4, 5);
            tabMain.Name = "tabMain";
            tabMain.Padding = new Point(14, 6);
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(1532, 519);
            tabMain.TabIndex = 0;
            tabMain.DrawItem += tabMain_DrawItem;
            tabMain.SelectedIndexChanged += tabMain_SelectedIndexChanged;
            tabMain.Selecting += tabMain_Selecting;
            // 
            // tabWorkstation
            // 
            tabWorkstation.Controls.Add(workstationLayout);
            tabWorkstation.Location = new Point(4, 43);
            tabWorkstation.Margin = new Padding(4, 5, 4, 5);
            tabWorkstation.Name = "tabWorkstation";
            tabWorkstation.Padding = new Padding(17, 20, 17, 20);
            tabWorkstation.Size = new Size(1524, 472);
            tabWorkstation.TabIndex = 0;
            tabWorkstation.Text = "Workstation";
            // 
            // workstationLayout
            // 
            workstationLayout.ColumnCount = 1;
            workstationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            workstationLayout.Controls.Add(lblWorkstationHeading, 0, 0);
            workstationLayout.Controls.Add(modelRootLayout, 0, 1);
            workstationLayout.Location = new Point(17, 20);
            workstationLayout.Margin = new Padding(4, 5, 4, 5);
            workstationLayout.Name = "workstationLayout";
            workstationLayout.RowCount = 2;
            workstationLayout.RowStyles.Add(new RowStyle());
            workstationLayout.RowStyles.Add(new RowStyle());
            workstationLayout.Size = new Size(1484, 770);
            workstationLayout.TabIndex = 0;
            // 
            // lblWorkstationHeading
            // 
            lblWorkstationHeading.AutoSize = true;
            lblWorkstationHeading.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblWorkstationHeading.Location = new Point(4, 0);
            lblWorkstationHeading.Margin = new Padding(4, 0, 4, 0);
            lblWorkstationHeading.Name = "lblWorkstationHeading";
            lblWorkstationHeading.Size = new Size(387, 30);
            lblWorkstationHeading.TabIndex = 0;
            lblWorkstationHeading.Text = "Environment and device diagnostics";
            // 
            // modelRootLayout
            // 
            modelRootLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            modelRootLayout.ColumnCount = 4;
            modelRootLayout.ColumnStyles.Add(new ColumnStyle());
            modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 164F));
            modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 201F));
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
            modelRootLayout.Location = new Point(4, 35);
            modelRootLayout.Margin = new Padding(4, 5, 4, 5);
            modelRootLayout.Name = "modelRootLayout";
            modelRootLayout.RowCount = 7;
            modelRootLayout.RowStyles.Add(new RowStyle());
            modelRootLayout.RowStyles.Add(new RowStyle());
            modelRootLayout.RowStyles.Add(new RowStyle());
            modelRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            modelRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 117F));
            modelRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
            modelRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            modelRootLayout.Size = new Size(1403, 483);
            modelRootLayout.TabIndex = 1;
            // 
            // lblWorkstationTarget
            // 
            lblWorkstationTarget.AutoSize = true;
            lblWorkstationTarget.Dock = DockStyle.Fill;
            lblWorkstationTarget.Location = new Point(4, 145);
            lblWorkstationTarget.Margin = new Padding(4, 13, 4, 0);
            lblWorkstationTarget.Name = "lblWorkstationTarget";
            lblWorkstationTarget.Size = new Size(178, 55);
            lblWorkstationTarget.TabIndex = 1;
            lblWorkstationTarget.Text = "Model's target:";
            lblWorkstationTarget.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtLocalServerStatus
            // 
            txtLocalServerStatus.Anchor = AnchorStyles.Left;
            txtLocalServerStatus.Location = new Point(190, 455);
            txtLocalServerStatus.Margin = new Padding(4, 5, 4, 5);
            txtLocalServerStatus.Name = "txtLocalServerStatus";
            txtLocalServerStatus.ReadOnly = true;
            txtLocalServerStatus.Size = new Size(764, 34);
            txtLocalServerStatus.TabIndex = 29;
            txtLocalServerStatus.Text = "Stopped";
            // 
            // lblLocalServerStatus
            // 
            lblLocalServerStatus.AutoSize = true;
            lblLocalServerStatus.Dock = DockStyle.Fill;
            lblLocalServerStatus.Location = new Point(4, 442);
            lblLocalServerStatus.Margin = new Padding(4, 0, 4, 0);
            lblLocalServerStatus.Name = "lblLocalServerStatus";
            lblLocalServerStatus.Size = new Size(178, 60);
            lblLocalServerStatus.TabIndex = 28;
            lblLocalServerStatus.Text = "Status:";
            lblLocalServerStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnStartLocalServer
            // 
            btnStartLocalServer.Dock = DockStyle.Fill;
            btnStartLocalServer.Location = new Point(1042, 382);
            btnStartLocalServer.Margin = new Padding(4, 5, 4, 5);
            btnStartLocalServer.Name = "btnStartLocalServer";
            btnStartLocalServer.Size = new Size(156, 55);
            btnStartLocalServer.TabIndex = 27;
            btnStartLocalServer.Text = "Start Server";
            btnStartLocalServer.Click += btnStartLocalServer_Click;
            // 
            // lblLocalServerUrl
            // 
            lblLocalServerUrl.AutoSize = true;
            lblLocalServerUrl.Dock = DockStyle.Fill;
            lblLocalServerUrl.Location = new Point(4, 377);
            lblLocalServerUrl.Margin = new Padding(4, 0, 4, 0);
            lblLocalServerUrl.Name = "lblLocalServerUrl";
            lblLocalServerUrl.Size = new Size(178, 65);
            lblLocalServerUrl.TabIndex = 25;
            lblLocalServerUrl.Text = "Server address:";
            lblLocalServerUrl.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnStopLocalServer
            // 
            btnStopLocalServer.Dock = DockStyle.Fill;
            btnStopLocalServer.Location = new Point(1206, 382);
            btnStopLocalServer.Margin = new Padding(4, 5, 4, 5);
            btnStopLocalServer.Name = "btnStopLocalServer";
            btnStopLocalServer.Size = new Size(193, 55);
            btnStopLocalServer.TabIndex = 30;
            btnStopLocalServer.Text = "Stop Server";
            btnStopLocalServer.Click += btnStopLocalServer_Click;
            // 
            // btnLoadModel
            // 
            modelRootLayout.SetColumnSpan(btnLoadModel, 2);
            btnLoadModel.Dock = DockStyle.Fill;
            btnLoadModel.Location = new Point(1042, 137);
            btnLoadModel.Margin = new Padding(4, 5, 4, 5);
            btnLoadModel.Name = "btnLoadModel";
            btnLoadModel.Padding = new Padding(11, 5, 11, 5);
            btnLoadModel.Size = new Size(357, 58);
            btnLoadModel.TabIndex = 15;
            btnLoadModel.Text = "Load Model";
            btnLoadModel.Click += btnLoadModel_Click;
            // 
            // txtLocalServerUrl
            // 
            txtLocalServerUrl.Dock = DockStyle.Fill;
            txtLocalServerUrl.Location = new Point(190, 382);
            txtLocalServerUrl.Margin = new Padding(4, 5, 4, 5);
            txtLocalServerUrl.Name = "txtLocalServerUrl";
            txtLocalServerUrl.ReadOnly = true;
            txtLocalServerUrl.Size = new Size(844, 34);
            txtLocalServerUrl.TabIndex = 26;
            txtLocalServerUrl.Text = "http://192.168.0.11:11434";
            // 
            // lblModelRoot
            // 
            lblModelRoot.AutoSize = true;
            lblModelRoot.Dock = DockStyle.Fill;
            lblModelRoot.Location = new Point(4, 0);
            lblModelRoot.Margin = new Padding(4, 0, 4, 0);
            lblModelRoot.Name = "lblModelRoot";
            lblModelRoot.Size = new Size(178, 66);
            lblModelRoot.TabIndex = 0;
            lblModelRoot.Text = "Main model folder:";
            lblModelRoot.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtModelRoot
            // 
            txtModelRoot.Dock = DockStyle.Fill;
            txtModelRoot.Location = new Point(190, 5);
            txtModelRoot.Margin = new Padding(4, 5, 4, 5);
            txtModelRoot.Name = "txtModelRoot";
            txtModelRoot.Size = new Size(844, 34);
            txtModelRoot.TabIndex = 1;
            txtModelRoot.KeyDown += txtModelRoot_KeyDown;
            txtModelRoot.Leave += txtModelRoot_Leave;
            // 
            // btnBrowseModelRoot
            // 
            modelRootLayout.SetColumnSpan(btnBrowseModelRoot, 2);
            btnBrowseModelRoot.Dock = DockStyle.Fill;
            btnBrowseModelRoot.Location = new Point(1042, 3);
            btnBrowseModelRoot.Margin = new Padding(4, 3, 4, 3);
            btnBrowseModelRoot.Name = "btnBrowseModelRoot";
            btnBrowseModelRoot.Size = new Size(357, 60);
            btnBrowseModelRoot.TabIndex = 2;
            btnBrowseModelRoot.Text = "Browse...";
            btnBrowseModelRoot.Click += btnBrowseModelRoot_Click;
            // 
            // lblInferModel
            // 
            lblInferModel.AutoSize = true;
            lblInferModel.Dock = DockStyle.Fill;
            lblInferModel.Location = new Point(4, 66);
            lblInferModel.Margin = new Padding(4, 0, 4, 0);
            lblInferModel.Name = "lblInferModel";
            lblInferModel.Size = new Size(178, 66);
            lblInferModel.TabIndex = 0;
            lblInferModel.Text = "Model:";
            lblInferModel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtInferModel
            // 
            txtInferModel.Dock = DockStyle.Fill;
            txtInferModel.Location = new Point(190, 71);
            txtInferModel.Margin = new Padding(4, 5, 4, 5);
            txtInferModel.Name = "txtInferModel";
            txtInferModel.Size = new Size(844, 34);
            txtInferModel.TabIndex = 1;
            txtInferModel.TextChanged += txtInferModel_TextChanged;
            // 
            // btnBrowseInferModel
            // 
            btnBrowseInferModel.Dock = DockStyle.Fill;
            btnBrowseInferModel.Location = new Point(1042, 69);
            btnBrowseInferModel.Margin = new Padding(4, 3, 4, 3);
            btnBrowseInferModel.Name = "btnBrowseInferModel";
            btnBrowseInferModel.Size = new Size(156, 60);
            btnBrowseInferModel.TabIndex = 2;
            btnBrowseInferModel.Text = "Select";
            btnBrowseInferModel.Click += btnBrowseInferModel_Click;
            // 
            // btnUseSampleModel
            // 
            btnUseSampleModel.Dock = DockStyle.Fill;
            btnUseSampleModel.Location = new Point(1206, 69);
            btnUseSampleModel.Margin = new Padding(4, 3, 4, 3);
            btnUseSampleModel.Name = "btnUseSampleModel";
            btnUseSampleModel.Size = new Size(193, 60);
            btnUseSampleModel.TabIndex = 5;
            btnUseSampleModel.Text = "Sample";
            btnUseSampleModel.Click += btnUseSampleModel_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(190, 137);
            splitContainer1.Margin = new Padding(4, 5, 4, 5);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(cboWorkstationTarget);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btnCheckWorkstation);
            splitContainer1.Size = new Size(844, 58);
            splitContainer1.SplitterDistance = 406;
            splitContainer1.SplitterWidth = 6;
            splitContainer1.TabIndex = 16;
            // 
            // cboWorkstationTarget
            // 
            cboWorkstationTarget.Dock = DockStyle.Fill;
            cboWorkstationTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWorkstationTarget.Location = new Point(0, 0);
            cboWorkstationTarget.Margin = new Padding(4, 8, 4, 0);
            cboWorkstationTarget.Name = "cboWorkstationTarget";
            cboWorkstationTarget.Size = new Size(406, 36);
            cboWorkstationTarget.TabIndex = 2;
            cboWorkstationTarget.SelectedIndexChanged += cboWorkstationTarget_SelectedIndexChanged;
            // 
            // btnCheckWorkstation
            // 
            btnCheckWorkstation.Dock = DockStyle.Fill;
            btnCheckWorkstation.Location = new Point(0, 0);
            btnCheckWorkstation.Margin = new Padding(4, 2, 4, 2);
            btnCheckWorkstation.Name = "btnCheckWorkstation";
            btnCheckWorkstation.Size = new Size(432, 58);
            btnCheckWorkstation.TabIndex = 0;
            btnCheckWorkstation.Text = "Check Workstation";
            btnCheckWorkstation.Click += btnCheckWorkstation_Click;
            // 
            // lblWorkstationMultiTarget
            // 
            lblWorkstationMultiTarget.AutoSize = true;
            lblWorkstationMultiTarget.Dock = DockStyle.Fill;
            lblWorkstationMultiTarget.Location = new Point(4, 213);
            lblWorkstationMultiTarget.Margin = new Padding(4, 13, 4, 0);
            lblWorkstationMultiTarget.Name = "lblWorkstationMultiTarget";
            lblWorkstationMultiTarget.Size = new Size(178, 47);
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
            cboWorkstationMultiTarget.Location = new Point(190, 205);
            cboWorkstationMultiTarget.Margin = new Padding(4, 5, 4, 5);
            cboWorkstationMultiTarget.Name = "cboWorkstationMultiTarget";
            cboWorkstationMultiTarget.Size = new Size(1209, 36);
            cboWorkstationMultiTarget.TabIndex = 32;
            cboWorkstationMultiTarget.SelectedIndexChanged += cboWorkstationMultiTarget_SelectedIndexChanged;
            // 
            // lblModelCapabilities
            // 
            lblModelCapabilities.AutoSize = true;
            lblModelCapabilities.Dock = DockStyle.Fill;
            lblModelCapabilities.Location = new Point(4, 273);
            lblModelCapabilities.Margin = new Padding(4, 13, 4, 0);
            lblModelCapabilities.Name = "lblModelCapabilities";
            lblModelCapabilities.Size = new Size(178, 104);
            lblModelCapabilities.TabIndex = 17;
            lblModelCapabilities.Text = "Model can use:";
            lblModelCapabilities.TextAlign = ContentAlignment.TopRight;
            // 
            // txtModelCapabilities
            // 
            modelRootLayout.SetColumnSpan(txtModelCapabilities, 3);
            txtModelCapabilities.Dock = DockStyle.Fill;
            txtModelCapabilities.Location = new Point(190, 265);
            txtModelCapabilities.Margin = new Padding(4, 5, 4, 5);
            txtModelCapabilities.Multiline = true;
            txtModelCapabilities.Name = "txtModelCapabilities";
            txtModelCapabilities.ReadOnly = true;
            txtModelCapabilities.ScrollBars = ScrollBars.Vertical;
            txtModelCapabilities.Size = new Size(1209, 107);
            txtModelCapabilities.TabIndex = 18;
            txtModelCapabilities.Text = "Load a model to see what it can use.";
            // 
            // tabDownloads
            // 
            tabDownloads.Controls.Add(modelsScroll);
            tabDownloads.Location = new Point(4, 43);
            tabDownloads.Margin = new Padding(4, 5, 4, 5);
            tabDownloads.Name = "tabDownloads";
            tabDownloads.Padding = new Padding(4, 5, 4, 5);
            tabDownloads.Size = new Size(1524, 488);
            tabDownloads.TabIndex = 4;
            tabDownloads.Text = "Downloads";
            // 
            // modelsScroll
            // 
            modelsScroll.AutoScroll = true;
            modelsScroll.Controls.Add(modelsLayout);
            modelsScroll.Dock = DockStyle.Fill;
            modelsScroll.Location = new Point(4, 5);
            modelsScroll.Margin = new Padding(4, 5, 4, 5);
            modelsScroll.Name = "modelsScroll";
            modelsScroll.Size = new Size(1516, 478);
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
            modelsLayout.Location = new Point(0, 5);
            modelsLayout.Margin = new Padding(4, 5, 4, 5);
            modelsLayout.Name = "modelsLayout";
            modelsLayout.Padding = new Padding(14, 17, 14, 17);
            modelsLayout.Size = new Size(1479, 567);
            modelsLayout.TabIndex = 0;
            // 
            // lblDownloadUrl
            // 
            lblDownloadUrl.Location = new Point(29, 42);
            lblDownloadUrl.Margin = new Padding(4, 0, 4, 0);
            lblDownloadUrl.Name = "lblDownloadUrl";
            lblDownloadUrl.Size = new Size(143, 42);
            lblDownloadUrl.TabIndex = 18;
            lblDownloadUrl.Text = "Web Address";
            lblDownloadUrl.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtDownloadUrl
            // 
            txtDownloadUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDownloadUrl.Location = new Point(179, 42);
            txtDownloadUrl.Margin = new Padding(4, 5, 4, 5);
            txtDownloadUrl.Name = "txtDownloadUrl";
            txtDownloadUrl.Size = new Size(813, 34);
            txtDownloadUrl.TabIndex = 19;
            txtDownloadUrl.Enter += txtDownloadUrl_Enter;
            // 
            // btnClearFileSelection
            // 
            btnClearFileSelection.Location = new Point(1007, 35);
            btnClearFileSelection.Margin = new Padding(4, 5, 4, 5);
            btnClearFileSelection.Name = "btnClearFileSelection";
            btnClearFileSelection.Size = new Size(150, 57);
            btnClearFileSelection.TabIndex = 14;
            btnClearFileSelection.Text = "Find";
            btnClearFileSelection.Click += btnFindDownloadAddress_Click;
            // 
            // btnLoadFiles
            // 
            btnLoadFiles.Location = new Point(1171, 35);
            btnLoadFiles.Margin = new Padding(4, 5, 4, 5);
            btnLoadFiles.Name = "btnLoadFiles";
            btnLoadFiles.Size = new Size(150, 57);
            btnLoadFiles.TabIndex = 13;
            btnLoadFiles.Text = "Download";
            btnLoadFiles.Click += btnLoadFiles_Click;
            // 
            // btnCancelDownload
            // 
            btnCancelDownload.Enabled = false;
            btnCancelDownload.Location = new Point(1329, 35);
            btnCancelDownload.Margin = new Padding(4, 5, 4, 5);
            btnCancelDownload.Name = "btnCancelDownload";
            btnCancelDownload.Size = new Size(150, 57);
            btnCancelDownload.TabIndex = 1;
            btnCancelDownload.Text = "Cancel";
            btnCancelDownload.Click += btnCancelDownload_Click;
            // 
            // lblHfQuery
            // 
            lblHfQuery.Anchor = AnchorStyles.None;
            lblHfQuery.Location = new Point(29, 108);
            lblHfQuery.Margin = new Padding(4, 0, 4, 0);
            lblHfQuery.Name = "lblHfQuery";
            lblHfQuery.Size = new Size(143, 42);
            lblHfQuery.TabIndex = 12;
            lblHfQuery.Text = "Button Label";
            lblHfQuery.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtHfQuery
            // 
            txtHfQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtHfQuery.Location = new Point(179, 108);
            txtHfQuery.Margin = new Padding(4, 5, 4, 5);
            txtHfQuery.Name = "txtHfQuery";
            txtHfQuery.Size = new Size(513, 34);
            txtHfQuery.TabIndex = 13;
            // 
            // lblModelWebsites
            // 
            lblModelWebsites.Location = new Point(714, 108);
            lblModelWebsites.Margin = new Padding(4, 0, 4, 0);
            lblModelWebsites.Name = "lblModelWebsites";
            lblModelWebsites.Size = new Size(100, 42);
            lblModelWebsites.TabIndex = 8;
            lblModelWebsites.Text = "Save To";
            lblModelWebsites.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cboModelWebsites
            // 
            cboModelWebsites.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboModelWebsites.DropDownStyle = ComboBoxStyle.DropDownList;
            cboModelWebsites.FormattingEnabled = true;
            cboModelWebsites.Location = new Point(821, 108);
            cboModelWebsites.Margin = new Padding(4, 5, 4, 5);
            cboModelWebsites.Name = "cboModelWebsites";
            cboModelWebsites.Size = new Size(141, 36);
            cboModelWebsites.TabIndex = 8;
            // 
            // btnCopyDownloadUrl
            // 
            btnCopyDownloadUrl.Location = new Point(986, 102);
            btnCopyDownloadUrl.Margin = new Padding(4, 5, 4, 5);
            btnCopyDownloadUrl.Name = "btnCopyDownloadUrl";
            btnCopyDownloadUrl.Size = new Size(186, 57);
            btnCopyDownloadUrl.TabIndex = 20;
            btnCopyDownloadUrl.Text = "Save Address";
            btnCopyDownloadUrl.Click += btnSaveFavoriteSite_Click;
            // 
            // btnPresetHuggingFace
            // 
            btnPresetHuggingFace.AutoEllipsis = true;
            btnPresetHuggingFace.Location = new Point(29, 200);
            btnPresetHuggingFace.Margin = new Padding(4, 5, 4, 5);
            btnPresetHuggingFace.Name = "btnPresetHuggingFace";
            btnPresetHuggingFace.Size = new Size(450, 57);
            btnPresetHuggingFace.TabIndex = 1;
            btnPresetHuggingFace.Text = "Site 1";
            btnPresetHuggingFace.Click += btnPresetHuggingFace_Click;
            // 
            // btnPresetOpenVino
            // 
            btnPresetOpenVino.AutoEllipsis = true;
            btnPresetOpenVino.Location = new Point(507, 200);
            btnPresetOpenVino.Margin = new Padding(4, 5, 4, 5);
            btnPresetOpenVino.Name = "btnPresetOpenVino";
            btnPresetOpenVino.Size = new Size(450, 57);
            btnPresetOpenVino.TabIndex = 2;
            btnPresetOpenVino.Text = "Site 2";
            btnPresetOpenVino.Click += btnPresetOpenVino_Click;
            // 
            // btnPresetIntel
            // 
            btnPresetIntel.AutoEllipsis = true;
            btnPresetIntel.Location = new Point(986, 200);
            btnPresetIntel.Margin = new Padding(4, 5, 4, 5);
            btnPresetIntel.Name = "btnPresetIntel";
            btnPresetIntel.Size = new Size(450, 57);
            btnPresetIntel.TabIndex = 3;
            btnPresetIntel.Text = "Site 3";
            btnPresetIntel.Click += btnPresetIntel_Click;
            // 
            // btnPresetUltralytics
            // 
            btnPresetUltralytics.AutoEllipsis = true;
            btnPresetUltralytics.Location = new Point(29, 275);
            btnPresetUltralytics.Margin = new Padding(4, 5, 4, 5);
            btnPresetUltralytics.Name = "btnPresetUltralytics";
            btnPresetUltralytics.Size = new Size(450, 57);
            btnPresetUltralytics.TabIndex = 4;
            btnPresetUltralytics.Text = "Site 4";
            btnPresetUltralytics.Click += btnPresetUltralytics_Click;
            // 
            // btnPresetOllama
            // 
            btnPresetOllama.AutoEllipsis = true;
            btnPresetOllama.Location = new Point(507, 275);
            btnPresetOllama.Margin = new Padding(4, 5, 4, 5);
            btnPresetOllama.Name = "btnPresetOllama";
            btnPresetOllama.Size = new Size(450, 57);
            btnPresetOllama.TabIndex = 5;
            btnPresetOllama.Text = "Site 5";
            btnPresetOllama.Click += btnPresetOllama_Click;
            // 
            // btnOfficialFiles
            // 
            btnOfficialFiles.AutoEllipsis = true;
            btnOfficialFiles.Location = new Point(986, 275);
            btnOfficialFiles.Margin = new Padding(4, 5, 4, 5);
            btnOfficialFiles.Name = "btnOfficialFiles";
            btnOfficialFiles.Size = new Size(450, 57);
            btnOfficialFiles.TabIndex = 6;
            btnOfficialFiles.Text = "Site 6";
            btnOfficialFiles.Click += btnOfficialFiles_Click;
            // 
            // btnRefreshOfficialIndex
            // 
            btnRefreshOfficialIndex.AutoEllipsis = true;
            btnRefreshOfficialIndex.Location = new Point(29, 350);
            btnRefreshOfficialIndex.Margin = new Padding(4, 5, 4, 5);
            btnRefreshOfficialIndex.Name = "btnRefreshOfficialIndex";
            btnRefreshOfficialIndex.Size = new Size(450, 57);
            btnRefreshOfficialIndex.TabIndex = 7;
            btnRefreshOfficialIndex.Text = "Site 7";
            btnRefreshOfficialIndex.Click += btnRefreshOfficialIndex_Click;
            // 
            // btnOpenModelWebsite
            // 
            btnOpenModelWebsite.AutoEllipsis = true;
            btnOpenModelWebsite.Location = new Point(507, 350);
            btnOpenModelWebsite.Margin = new Padding(4, 5, 4, 5);
            btnOpenModelWebsite.Name = "btnOpenModelWebsite";
            btnOpenModelWebsite.Size = new Size(450, 57);
            btnOpenModelWebsite.TabIndex = 9;
            btnOpenModelWebsite.Text = "Site 8";
            btnOpenModelWebsite.Click += btnOpenModelWebsite_Click;
            // 
            // btnAddModelWebsite
            // 
            btnAddModelWebsite.AutoEllipsis = true;
            btnAddModelWebsite.Location = new Point(986, 350);
            btnAddModelWebsite.Margin = new Padding(4, 5, 4, 5);
            btnAddModelWebsite.Name = "btnAddModelWebsite";
            btnAddModelWebsite.Size = new Size(450, 57);
            btnAddModelWebsite.TabIndex = 10;
            btnAddModelWebsite.Text = "Site 9";
            btnAddModelWebsite.Click += btnAddModelWebsite_Click;
            // 
            // btnDeleteModelWebsite
            // 
            btnDeleteModelWebsite.AutoEllipsis = true;
            btnDeleteModelWebsite.Location = new Point(29, 425);
            btnDeleteModelWebsite.Margin = new Padding(4, 5, 4, 5);
            btnDeleteModelWebsite.Name = "btnDeleteModelWebsite";
            btnDeleteModelWebsite.Size = new Size(450, 57);
            btnDeleteModelWebsite.TabIndex = 11;
            btnDeleteModelWebsite.Text = "Site 10";
            btnDeleteModelWebsite.Click += btnDeleteModelWebsite_Click;
            // 
            // btnSearchRepos
            // 
            btnSearchRepos.Anchor = AnchorStyles.Left;
            btnSearchRepos.AutoEllipsis = true;
            btnSearchRepos.Location = new Point(507, 425);
            btnSearchRepos.Margin = new Padding(4, 5, 4, 5);
            btnSearchRepos.Name = "btnSearchRepos";
            btnSearchRepos.Size = new Size(450, 57);
            btnSearchRepos.TabIndex = 14;
            btnSearchRepos.Text = "Site 11";
            btnSearchRepos.Click += btnSearchRepos_Click;
            // 
            // btnSearchAllModels
            // 
            btnSearchAllModels.Anchor = AnchorStyles.Left;
            btnSearchAllModels.AutoEllipsis = true;
            btnSearchAllModels.Location = new Point(986, 425);
            btnSearchAllModels.Margin = new Padding(4, 5, 4, 5);
            btnSearchAllModels.Name = "btnSearchAllModels";
            btnSearchAllModels.Size = new Size(450, 57);
            btnSearchAllModels.TabIndex = 35;
            btnSearchAllModels.Text = "Site 12";
            btnSearchAllModels.Click += btnSearchAllModels_Click;
            // 
            // progressDownload
            // 
            progressDownload.Location = new Point(29, 503);
            progressDownload.Margin = new Padding(0, 12, 14, 0);
            progressDownload.Name = "progressDownload";
            progressDownload.Size = new Size(600, 30);
            progressDownload.TabIndex = 0;
            // 
            // lblDownloadStatus
            // 
            lblDownloadStatus.AutoEllipsis = true;
            lblDownloadStatus.Location = new Point(650, 492);
            lblDownloadStatus.Margin = new Padding(4, 0, 4, 0);
            lblDownloadStatus.Name = "lblDownloadStatus";
            lblDownloadStatus.Size = new Size(800, 47);
            lblDownloadStatus.TabIndex = 31;
            lblDownloadStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabConvert
            // 
            tabConvert.Controls.Add(convertLayout);
            tabConvert.Location = new Point(4, 43);
            tabConvert.Margin = new Padding(4, 5, 4, 5);
            tabConvert.Name = "tabConvert";
            tabConvert.Padding = new Padding(17, 20, 17, 20);
            tabConvert.Size = new Size(1524, 488);
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
            convertLayout.Location = new Point(17, 20);
            convertLayout.Margin = new Padding(4, 5, 4, 5);
            convertLayout.Name = "convertLayout";
            convertLayout.Padding = new Padding(14, 17, 14, 17);
            convertLayout.Size = new Size(1490, 448);
            convertLayout.TabIndex = 0;
            // 
            // lblOmzBuilderHeading
            // 
            lblOmzBuilderHeading.AutoSize = true;
            lblOmzBuilderHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOmzBuilderHeading.Location = new Point(29, 42);
            lblOmzBuilderHeading.Margin = new Padding(4, 0, 4, 0);
            lblOmzBuilderHeading.Name = "lblOmzBuilderHeading";
            lblOmzBuilderHeading.Size = new Size(211, 28);
            lblOmzBuilderHeading.TabIndex = 22;
            lblOmzBuilderHeading.Text = "OpenVINO IR to blob";
            // 
            // cboConvertSourceModel
            // 
            cboConvertSourceModel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboConvertSourceModel.DropDownStyle = ComboBoxStyle.DropDownList;
            cboConvertSourceModel.Location = new Point(29, 117);
            cboConvertSourceModel.Margin = new Padding(4, 5, 4, 5);
            cboConvertSourceModel.Name = "cboConvertSourceModel";
            cboConvertSourceModel.Size = new Size(1017, 36);
            cboConvertSourceModel.TabIndex = 32;
            cboConvertSourceModel.SelectedIndexChanged += cboConvertSourceModel_SelectedIndexChanged;
            // 
            // btnRefreshConvertModels
            // 
            btnRefreshConvertModels.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefreshConvertModels.Location = new Point(1062, 112);
            btnRefreshConvertModels.Margin = new Padding(4, 5, 4, 5);
            btnRefreshConvertModels.Name = "btnRefreshConvertModels";
            btnRefreshConvertModels.Size = new Size(207, 57);
            btnRefreshConvertModels.TabIndex = 33;
            btnRefreshConvertModels.Text = "Refresh Models";
            btnRefreshConvertModels.Click += btnRefreshConvertModels_Click;
            // 
            // grpConvertOutputs
            // 
            grpConvertOutputs.Controls.Add(chkConvertBlob);
            grpConvertOutputs.Location = new Point(29, 192);
            grpConvertOutputs.Margin = new Padding(4, 5, 4, 5);
            grpConvertOutputs.Name = "grpConvertOutputs";
            grpConvertOutputs.Padding = new Padding(4, 5, 4, 5);
            grpConvertOutputs.Size = new Size(229, 130);
            grpConvertOutputs.TabIndex = 34;
            grpConvertOutputs.TabStop = false;
            grpConvertOutputs.Text = "Create";
            // 
            // chkConvertBlob
            // 
            chkConvertBlob.Checked = true;
            chkConvertBlob.CheckState = CheckState.Checked;
            chkConvertBlob.Location = new Point(26, 53);
            chkConvertBlob.Margin = new Padding(4, 5, 4, 5);
            chkConvertBlob.Name = "chkConvertBlob";
            chkConvertBlob.Size = new Size(121, 42);
            chkConvertBlob.TabIndex = 0;
            chkConvertBlob.Text = ".blob";
            chkConvertBlob.CheckedChanged += chkConvertBlob_CheckedChanged;
            // 
            // btnConvertToIr
            // 
            btnConvertToIr.Location = new Point(29, 358);
            btnConvertToIr.Margin = new Padding(4, 5, 4, 5);
            btnConvertToIr.Name = "btnConvertToIr";
            btnConvertToIr.Size = new Size(157, 57);
            btnConvertToIr.TabIndex = 28;
            btnConvertToIr.Text = "Create Blob";
            btnConvertToIr.Click += btnConvertToIr_Click;
            // 
            // pnlIrVisualizer
            // 
            pnlIrVisualizer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlIrVisualizer.BackColor = Color.FromArgb(251, 251, 251);
            pnlIrVisualizer.BorderStyle = BorderStyle.FixedSingle;
            pnlIrVisualizer.Controls.Add(lblIrPlaceholder);
            pnlIrVisualizer.Location = new Point(29, 450);
            pnlIrVisualizer.Margin = new Padding(4, 13, 4, 13);
            pnlIrVisualizer.Name = "pnlIrVisualizer";
            pnlIrVisualizer.Size = new Size(1240, 415);
            pnlIrVisualizer.TabIndex = 29;
            // 
            // lblIrPlaceholder
            // 
            lblIrPlaceholder.AutoSize = true;
            lblIrPlaceholder.ForeColor = SystemColors.GrayText;
            lblIrPlaceholder.Location = new Point(26, 30);
            lblIrPlaceholder.Margin = new Padding(4, 0, 4, 0);
            lblIrPlaceholder.Name = "lblIrPlaceholder";
            lblIrPlaceholder.Size = new Size(491, 28);
            lblIrPlaceholder.TabIndex = 0;
            lblIrPlaceholder.Text = "Select an OpenVINO .xml/.bin model, then create .blob.";
            // 
            // lblModelsStatus
            // 
            lblModelsStatus.AutoSize = true;
            lblModelsStatus.ForeColor = SystemColors.GrayText;
            lblModelsStatus.Location = new Point(29, 892);
            lblModelsStatus.Margin = new Padding(4, 0, 4, 0);
            lblModelsStatus.Name = "lblModelsStatus";
            lblModelsStatus.Size = new Size(679, 28);
            lblModelsStatus.TabIndex = 30;
            lblModelsStatus.Text = "Select an OpenVINO IR model from the model root and create a device blob.";
            // 
            // tabOvmsPull
            // 
            tabOvmsPull.Controls.Add(ovmsPullLayout);
            tabOvmsPull.Location = new Point(4, 43);
            tabOvmsPull.Margin = new Padding(4, 5, 4, 5);
            tabOvmsPull.Name = "tabOvmsPull";
            tabOvmsPull.Padding = new Padding(17, 20, 17, 20);
            tabOvmsPull.Size = new Size(1524, 488);
            tabOvmsPull.TabIndex = 8;
            tabOvmsPull.Text = "OVMS Pull";
            // 
            // ovmsPullLayout
            // 
            ovmsPullLayout.AutoScroll = true;
            ovmsPullLayout.Controls.Add(lblOvmsPullHeading);
            ovmsPullLayout.Controls.Add(lblOvmsExecutable);
            ovmsPullLayout.Controls.Add(txtOvmsExecutable);
            ovmsPullLayout.Controls.Add(lblOvmsPythonPath);
            ovmsPullLayout.Controls.Add(txtOvmsPythonPath);
            ovmsPullLayout.Controls.Add(lblOvmsExportToolsPath);
            ovmsPullLayout.Controls.Add(txtOvmsExportToolsPath);
            ovmsPullLayout.Controls.Add(lblOvmsSourceFormat);
            ovmsPullLayout.Controls.Add(cboOvmsSourceFormat);
            ovmsPullLayout.Controls.Add(lblOvmsTask);
            ovmsPullLayout.Controls.Add(cboOvmsTask);
            ovmsPullLayout.Controls.Add(lblOvmsPipelineType);
            ovmsPullLayout.Controls.Add(cboOvmsPipelineType);
            ovmsPullLayout.Controls.Add(lblOvmsWeightFormat);
            ovmsPullLayout.Controls.Add(cboOvmsWeightFormat);
            ovmsPullLayout.Controls.Add(lblOvmsSourceModel);
            ovmsPullLayout.Controls.Add(cboOvmsSourceModel);
            ovmsPullLayout.Controls.Add(lblOvmsGgufFilename);
            ovmsPullLayout.Controls.Add(txtOvmsGgufFilename);
            ovmsPullLayout.Controls.Add(lblOvmsModelRepository);
            ovmsPullLayout.Controls.Add(txtOvmsModelRepository);
            ovmsPullLayout.Controls.Add(lblOvmsLogPath);
            ovmsPullLayout.Controls.Add(txtOvmsLogPath);
            ovmsPullLayout.Controls.Add(chkOvmsOverwrite);
            ovmsPullLayout.Controls.Add(chkOvmsDebug);
            ovmsPullLayout.Controls.Add(btnCheckOvmsVersions);
            ovmsPullLayout.Controls.Add(btnPreviewOvmsPull);
            ovmsPullLayout.Controls.Add(btnRunOvmsPull);
            ovmsPullLayout.Controls.Add(lblOvmsPullStatus);
            ovmsPullLayout.Dock = DockStyle.Fill;
            ovmsPullLayout.Location = new Point(17, 20);
            ovmsPullLayout.Margin = new Padding(4, 5, 4, 5);
            ovmsPullLayout.Name = "ovmsPullLayout";
            ovmsPullLayout.Padding = new Padding(14, 17, 14, 17);
            ovmsPullLayout.Size = new Size(1490, 448);
            ovmsPullLayout.TabIndex = 0;
            // 
            // lblOvmsPullHeading
            // 
            lblOvmsPullHeading.AutoSize = true;
            lblOvmsPullHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOvmsPullHeading.Location = new Point(29, 33);
            lblOvmsPullHeading.Margin = new Padding(4, 0, 4, 0);
            lblOvmsPullHeading.Name = "lblOvmsPullHeading";
            lblOvmsPullHeading.Size = new Size(177, 28);
            lblOvmsPullHeading.TabIndex = 0;
            lblOvmsPullHeading.Text = "OVMS model pull";
            // 
            // lblOvmsExecutable
            // 
            lblOvmsExecutable.Location = new Point(29, 103);
            lblOvmsExecutable.Margin = new Padding(4, 0, 4, 0);
            lblOvmsExecutable.Name = "lblOvmsExecutable";
            lblOvmsExecutable.Size = new Size(171, 42);
            lblOvmsExecutable.TabIndex = 1;
            lblOvmsExecutable.Text = "OVMS program";
            // 
            // txtOvmsExecutable
            // 
            txtOvmsExecutable.Location = new Point(214, 100);
            txtOvmsExecutable.Margin = new Padding(4, 5, 4, 5);
            txtOvmsExecutable.Name = "txtOvmsExecutable";
            txtOvmsExecutable.Size = new Size(513, 34);
            txtOvmsExecutable.TabIndex = 2;
            txtOvmsExecutable.TextChanged += txtOvmsExecutable_TextChanged;
            // 
            // lblOvmsPythonPath
            // 
            lblOvmsPythonPath.Location = new Point(757, 103);
            lblOvmsPythonPath.Margin = new Padding(4, 0, 4, 0);
            lblOvmsPythonPath.Name = "lblOvmsPythonPath";
            lblOvmsPythonPath.Size = new Size(171, 42);
            lblOvmsPythonPath.TabIndex = 3;
            lblOvmsPythonPath.Text = "Python DLL folder";
            // 
            // txtOvmsPythonPath
            // 
            txtOvmsPythonPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOvmsPythonPath.Location = new Point(943, 100);
            txtOvmsPythonPath.Margin = new Padding(4, 5, 4, 5);
            txtOvmsPythonPath.Name = "txtOvmsPythonPath";
            txtOvmsPythonPath.Size = new Size(414, 34);
            txtOvmsPythonPath.TabIndex = 4;
            txtOvmsPythonPath.TextChanged += txtOvmsPythonPath_TextChanged;
            // 
            // lblOvmsExportToolsPath
            // 
            lblOvmsExportToolsPath.Location = new Point(29, 162);
            lblOvmsExportToolsPath.Margin = new Padding(4, 0, 4, 0);
            lblOvmsExportToolsPath.Name = "lblOvmsExportToolsPath";
            lblOvmsExportToolsPath.Size = new Size(171, 42);
            lblOvmsExportToolsPath.TabIndex = 5;
            lblOvmsExportToolsPath.Text = "Export tools folder";
            // 
            // txtOvmsExportToolsPath
            // 
            txtOvmsExportToolsPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOvmsExportToolsPath.Location = new Point(214, 158);
            txtOvmsExportToolsPath.Margin = new Padding(4, 5, 4, 5);
            txtOvmsExportToolsPath.Name = "txtOvmsExportToolsPath";
            txtOvmsExportToolsPath.Size = new Size(1142, 34);
            txtOvmsExportToolsPath.TabIndex = 6;
            txtOvmsExportToolsPath.TextChanged += txtOvmsExportToolsPath_TextChanged;
            // 
            // lblOvmsSourceFormat
            // 
            lblOvmsSourceFormat.Location = new Point(29, 228);
            lblOvmsSourceFormat.Margin = new Padding(4, 0, 4, 0);
            lblOvmsSourceFormat.Name = "lblOvmsSourceFormat";
            lblOvmsSourceFormat.Size = new Size(171, 42);
            lblOvmsSourceFormat.TabIndex = 7;
            lblOvmsSourceFormat.Text = "Source type";
            // 
            // cboOvmsSourceFormat
            // 
            cboOvmsSourceFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOvmsSourceFormat.Location = new Point(214, 225);
            cboOvmsSourceFormat.Margin = new Padding(4, 5, 4, 5);
            cboOvmsSourceFormat.Name = "cboOvmsSourceFormat";
            cboOvmsSourceFormat.Size = new Size(205, 36);
            cboOvmsSourceFormat.TabIndex = 8;
            cboOvmsSourceFormat.SelectedIndexChanged += cboOvmsSourceFormat_SelectedIndexChanged;
            cboOvmsSourceFormat.TextChanged += cboOvmsSourceFormat_TextChanged;
            // 
            // lblOvmsTask
            // 
            lblOvmsTask.Location = new Point(450, 228);
            lblOvmsTask.Margin = new Padding(4, 0, 4, 0);
            lblOvmsTask.Name = "lblOvmsTask";
            lblOvmsTask.Size = new Size(79, 42);
            lblOvmsTask.TabIndex = 9;
            lblOvmsTask.Text = "Task";
            // 
            // cboOvmsTask
            // 
            cboOvmsTask.Location = new Point(529, 225);
            cboOvmsTask.Margin = new Padding(4, 5, 4, 5);
            cboOvmsTask.Name = "cboOvmsTask";
            cboOvmsTask.Size = new Size(263, 36);
            cboOvmsTask.TabIndex = 10;
            cboOvmsTask.SelectedIndexChanged += cboOvmsTask_SelectedIndexChanged;
            cboOvmsTask.TextChanged += cboOvmsTask_TextChanged;
            // 
            // lblOvmsPipelineType
            // 
            lblOvmsPipelineType.Location = new Point(821, 228);
            lblOvmsPipelineType.Margin = new Padding(4, 0, 4, 0);
            lblOvmsPipelineType.Name = "lblOvmsPipelineType";
            lblOvmsPipelineType.Size = new Size(100, 42);
            lblOvmsPipelineType.TabIndex = 11;
            lblOvmsPipelineType.Text = "Pipeline";
            // 
            // cboOvmsPipelineType
            // 
            cboOvmsPipelineType.Location = new Point(929, 225);
            cboOvmsPipelineType.Margin = new Padding(4, 5, 4, 5);
            cboOvmsPipelineType.Name = "cboOvmsPipelineType";
            cboOvmsPipelineType.Size = new Size(120, 36);
            cboOvmsPipelineType.TabIndex = 12;
            cboOvmsPipelineType.SelectedIndexChanged += cboOvmsPipelineType_SelectedIndexChanged;
            cboOvmsPipelineType.TextChanged += cboOvmsPipelineType_TextChanged;
            // 
            // lblOvmsWeightFormat
            // 
            lblOvmsWeightFormat.Location = new Point(1079, 228);
            lblOvmsWeightFormat.Margin = new Padding(4, 0, 4, 0);
            lblOvmsWeightFormat.Name = "lblOvmsWeightFormat";
            lblOvmsWeightFormat.Size = new Size(100, 42);
            lblOvmsWeightFormat.TabIndex = 13;
            lblOvmsWeightFormat.Text = "Weights";
            // 
            // cboOvmsWeightFormat
            // 
            cboOvmsWeightFormat.Location = new Point(1193, 225);
            cboOvmsWeightFormat.Margin = new Padding(4, 5, 4, 5);
            cboOvmsWeightFormat.Name = "cboOvmsWeightFormat";
            cboOvmsWeightFormat.Size = new Size(170, 36);
            cboOvmsWeightFormat.TabIndex = 14;
            cboOvmsWeightFormat.SelectedIndexChanged += cboOvmsWeightFormat_SelectedIndexChanged;
            cboOvmsWeightFormat.TextChanged += cboOvmsWeightFormat_TextChanged;
            // 
            // lblOvmsSourceModel
            // 
            lblOvmsSourceModel.Location = new Point(29, 295);
            lblOvmsSourceModel.Margin = new Padding(4, 0, 4, 0);
            lblOvmsSourceModel.Name = "lblOvmsSourceModel";
            lblOvmsSourceModel.Size = new Size(171, 42);
            lblOvmsSourceModel.TabIndex = 15;
            lblOvmsSourceModel.Text = "Source model";
            // 
            // cboOvmsSourceModel
            // 
            cboOvmsSourceModel.Location = new Point(214, 292);
            cboOvmsSourceModel.Margin = new Padding(4, 5, 4, 5);
            cboOvmsSourceModel.Name = "cboOvmsSourceModel";
            cboOvmsSourceModel.Size = new Size(513, 36);
            cboOvmsSourceModel.TabIndex = 16;
            cboOvmsSourceModel.SelectedIndexChanged += cboOvmsSourceModel_SelectedIndexChanged;
            cboOvmsSourceModel.TextChanged += cboOvmsSourceModel_TextChanged;
            // 
            // lblOvmsGgufFilename
            // 
            lblOvmsGgufFilename.Location = new Point(29, 362);
            lblOvmsGgufFilename.Margin = new Padding(4, 0, 4, 0);
            lblOvmsGgufFilename.Name = "lblOvmsGgufFilename";
            lblOvmsGgufFilename.Size = new Size(171, 42);
            lblOvmsGgufFilename.TabIndex = 17;
            lblOvmsGgufFilename.Text = "GGUF filename";
            // 
            // txtOvmsGgufFilename
            // 
            txtOvmsGgufFilename.Location = new Point(214, 358);
            txtOvmsGgufFilename.Margin = new Padding(4, 5, 4, 5);
            txtOvmsGgufFilename.Name = "txtOvmsGgufFilename";
            txtOvmsGgufFilename.PlaceholderText = "Qwen3.6-27B-UD-Q8_K_XL.gguf";
            txtOvmsGgufFilename.Size = new Size(513, 34);
            txtOvmsGgufFilename.TabIndex = 18;
            txtOvmsGgufFilename.TextChanged += txtOvmsGgufFilename_TextChanged;
            // 
            // lblOvmsModelRepository
            // 
            lblOvmsModelRepository.Location = new Point(757, 295);
            lblOvmsModelRepository.Margin = new Padding(4, 0, 4, 0);
            lblOvmsModelRepository.Name = "lblOvmsModelRepository";
            lblOvmsModelRepository.Size = new Size(171, 42);
            lblOvmsModelRepository.TabIndex = 19;
            lblOvmsModelRepository.Text = "Model folder";
            // 
            // txtOvmsModelRepository
            // 
            txtOvmsModelRepository.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOvmsModelRepository.Location = new Point(943, 292);
            txtOvmsModelRepository.Margin = new Padding(4, 5, 4, 5);
            txtOvmsModelRepository.Name = "txtOvmsModelRepository";
            txtOvmsModelRepository.Size = new Size(414, 34);
            txtOvmsModelRepository.TabIndex = 20;
            txtOvmsModelRepository.TextChanged += txtOvmsModelRepository_TextChanged;
            // 
            // lblOvmsLogPath
            // 
            lblOvmsLogPath.Location = new Point(757, 362);
            lblOvmsLogPath.Margin = new Padding(4, 0, 4, 0);
            lblOvmsLogPath.Name = "lblOvmsLogPath";
            lblOvmsLogPath.Size = new Size(171, 42);
            lblOvmsLogPath.TabIndex = 21;
            lblOvmsLogPath.Text = "Log file";
            // 
            // txtOvmsLogPath
            // 
            txtOvmsLogPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOvmsLogPath.Location = new Point(943, 358);
            txtOvmsLogPath.Margin = new Padding(4, 5, 4, 5);
            txtOvmsLogPath.Name = "txtOvmsLogPath";
            txtOvmsLogPath.Size = new Size(414, 34);
            txtOvmsLogPath.TabIndex = 22;
            txtOvmsLogPath.TextChanged += txtOvmsLogPath_TextChanged;
            // 
            // chkOvmsOverwrite
            // 
            chkOvmsOverwrite.Checked = true;
            chkOvmsOverwrite.CheckState = CheckState.Checked;
            chkOvmsOverwrite.Location = new Point(29, 425);
            chkOvmsOverwrite.Margin = new Padding(4, 5, 4, 5);
            chkOvmsOverwrite.Name = "chkOvmsOverwrite";
            chkOvmsOverwrite.Size = new Size(236, 42);
            chkOvmsOverwrite.TabIndex = 23;
            chkOvmsOverwrite.Text = "Replace existing files";
            chkOvmsOverwrite.CheckedChanged += chkOvmsOverwrite_CheckedChanged;
            // 
            // chkOvmsDebug
            // 
            chkOvmsDebug.Checked = true;
            chkOvmsDebug.CheckState = CheckState.Checked;
            chkOvmsDebug.Location = new Point(279, 425);
            chkOvmsDebug.Margin = new Padding(4, 5, 4, 5);
            chkOvmsDebug.Name = "chkOvmsDebug";
            chkOvmsDebug.Size = new Size(157, 42);
            chkOvmsDebug.TabIndex = 24;
            chkOvmsDebug.Text = "Debug log";
            chkOvmsDebug.CheckedChanged += chkOvmsDebug_CheckedChanged;
            // 
            // btnCheckOvmsVersions
            // 
            btnCheckOvmsVersions.Location = new Point(457, 420);
            btnCheckOvmsVersions.Margin = new Padding(4, 5, 4, 5);
            btnCheckOvmsVersions.Name = "btnCheckOvmsVersions";
            btnCheckOvmsVersions.Size = new Size(171, 57);
            btnCheckOvmsVersions.TabIndex = 25;
            btnCheckOvmsVersions.Text = "Check Stack";
            btnCheckOvmsVersions.Click += btnCheckOvmsVersions_Click;
            // 
            // btnPreviewOvmsPull
            // 
            btnPreviewOvmsPull.Location = new Point(643, 420);
            btnPreviewOvmsPull.Margin = new Padding(4, 5, 4, 5);
            btnPreviewOvmsPull.Name = "btnPreviewOvmsPull";
            btnPreviewOvmsPull.Size = new Size(171, 57);
            btnPreviewOvmsPull.TabIndex = 26;
            btnPreviewOvmsPull.Text = "Preview";
            btnPreviewOvmsPull.Click += btnPreviewOvmsPull_Click;
            // 
            // btnRunOvmsPull
            // 
            btnRunOvmsPull.Location = new Point(829, 420);
            btnRunOvmsPull.Margin = new Padding(4, 5, 4, 5);
            btnRunOvmsPull.Name = "btnRunOvmsPull";
            btnRunOvmsPull.Size = new Size(171, 57);
            btnRunOvmsPull.TabIndex = 27;
            btnRunOvmsPull.Text = "Start Pull";
            btnRunOvmsPull.Click += btnRunOvmsPull_Click;
            // 
            // lblOvmsPullStatus
            // 
            lblOvmsPullStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblOvmsPullStatus.AutoEllipsis = true;
            lblOvmsPullStatus.ForeColor = SystemColors.GrayText;
            lblOvmsPullStatus.Location = new Point(1029, 422);
            lblOvmsPullStatus.Margin = new Padding(4, 0, 4, 0);
            lblOvmsPullStatus.Name = "lblOvmsPullStatus";
            lblOvmsPullStatus.Size = new Size(330, 57);
            lblOvmsPullStatus.TabIndex = 28;
            lblOvmsPullStatus.Text = "Ready to build an OVMS pull command.";
            lblOvmsPullStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabTests
            // 
            tabTests.Controls.Add(testsLayout);
            tabTests.Location = new Point(4, 43);
            tabTests.Margin = new Padding(4, 5, 4, 5);
            tabTests.Name = "tabTests";
            tabTests.Padding = new Padding(17, 20, 17, 20);
            tabTests.Size = new Size(1524, 488);
            tabTests.TabIndex = 7;
            tabTests.Text = "Tests";
            // 
            // testsLayout
            // 
            testsLayout.AutoScroll = true;
            testsLayout.ColumnCount = 3;
            testsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 214F));
            testsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            testsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 214F));
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
            testsLayout.Location = new Point(17, 20);
            testsLayout.Margin = new Padding(4, 5, 4, 5);
            testsLayout.Name = "testsLayout";
            testsLayout.Padding = new Padding(14, 17, 14, 17);
            testsLayout.RowCount = 9;
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 137F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 77F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            testsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 367F));
            testsLayout.Size = new Size(1490, 448);
            testsLayout.TabIndex = 0;
            // 
            // lblTestsHeading
            // 
            lblTestsHeading.AutoSize = true;
            testsLayout.SetColumnSpan(lblTestsHeading, 3);
            lblTestsHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTestsHeading.Location = new Point(18, 17);
            lblTestsHeading.Margin = new Padding(4, 0, 4, 0);
            lblTestsHeading.Name = "lblTestsHeading";
            lblTestsHeading.Size = new Size(196, 28);
            lblTestsHeading.TabIndex = 0;
            lblTestsHeading.Text = "Loaded model tests";
            // 
            // lblTestModelPath
            // 
            lblTestModelPath.Anchor = AnchorStyles.Left;
            lblTestModelPath.AutoSize = true;
            lblTestModelPath.Location = new Point(18, 108);
            lblTestModelPath.Margin = new Padding(4, 0, 4, 0);
            lblTestModelPath.Name = "lblTestModelPath";
            lblTestModelPath.Size = new Size(138, 28);
            lblTestModelPath.TabIndex = 14;
            lblTestModelPath.Text = "Loaded model";
            // 
            // txtTestModelPath
            // 
            testsLayout.SetColumnSpan(txtTestModelPath, 2);
            txtTestModelPath.Dock = DockStyle.Fill;
            txtTestModelPath.Location = new Point(232, 92);
            txtTestModelPath.Margin = new Padding(4, 5, 4, 5);
            txtTestModelPath.Name = "txtTestModelPath";
            txtTestModelPath.ReadOnly = true;
            txtTestModelPath.Size = new Size(1214, 34);
            txtTestModelPath.TabIndex = 15;
            // 
            // lblInferImage
            // 
            lblInferImage.Anchor = AnchorStyles.Right;
            lblInferImage.AutoSize = true;
            lblInferImage.Location = new Point(63, 211);
            lblInferImage.Margin = new Padding(4, 0, 4, 0);
            lblInferImage.Name = "lblInferImage";
            lblInferImage.Size = new Size(161, 28);
            lblInferImage.TabIndex = 3;
            lblInferImage.Text = "Image drop zone";
            // 
            // txtInferImage
            // 
            txtInferImage.AllowDrop = true;
            txtInferImage.Dock = DockStyle.Fill;
            txtInferImage.Location = new Point(232, 160);
            txtInferImage.Margin = new Padding(4, 3, 4, 3);
            txtInferImage.Name = "txtInferImage";
            txtInferImage.PlaceholderText = "Drop one image here, or use Browse...";
            txtInferImage.Size = new Size(1000, 34);
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
            imageTestButtons.Location = new Point(1240, 159);
            imageTestButtons.Margin = new Padding(4, 2, 4, 2);
            imageTestButtons.Name = "imageTestButtons";
            imageTestButtons.Size = new Size(206, 133);
            imageTestButtons.TabIndex = 5;
            imageTestButtons.WrapContents = false;
            // 
            // btnBrowseInferImage
            // 
            btnBrowseInferImage.Location = new Point(0, 0);
            btnBrowseInferImage.Margin = new Padding(0, 0, 0, 7);
            btnBrowseInferImage.Name = "btnBrowseInferImage";
            btnBrowseInferImage.Size = new Size(206, 57);
            btnBrowseInferImage.TabIndex = 0;
            btnBrowseInferImage.Text = "Browse...";
            btnBrowseInferImage.Click += btnBrowseInferImage_Click;
            // 
            // btnRunImageTest
            // 
            btnRunImageTest.Location = new Point(0, 64);
            btnRunImageTest.Margin = new Padding(0);
            btnRunImageTest.Name = "btnRunImageTest";
            btnRunImageTest.Size = new Size(206, 57);
            btnRunImageTest.TabIndex = 1;
            btnRunImageTest.Text = "Run Image Test";
            btnRunImageTest.Click += btnRunImageTest_Click;
            // 
            // lblInferText
            // 
            lblInferText.Anchor = AnchorStyles.Right;
            lblInferText.AutoSize = true;
            lblInferText.Location = new Point(140, 315);
            lblInferText.Margin = new Padding(4, 0, 4, 0);
            lblInferText.Name = "lblInferText";
            lblInferText.Size = new Size(84, 28);
            lblInferText.TabIndex = 6;
            lblInferText.Text = "Test Text";
            lblInferText.Click += lblInferText_Click;
            // 
            // txtInferText
            // 
            txtInferText.Dock = DockStyle.Fill;
            txtInferText.Location = new Point(232, 297);
            txtInferText.Margin = new Padding(4, 3, 4, 3);
            txtInferText.Name = "txtInferText";
            txtInferText.Size = new Size(1000, 34);
            txtInferText.TabIndex = 7;
            // 
            // lblInferLabels
            // 
            lblInferLabels.Anchor = AnchorStyles.Right;
            lblInferLabels.Location = new Point(157, 383);
            lblInferLabels.Margin = new Padding(4, 0, 4, 0);
            lblInferLabels.Name = "lblInferLabels";
            lblInferLabels.Size = new Size(67, 32);
            lblInferLabels.TabIndex = 8;
            lblInferLabels.Text = "Labels";
            // 
            // txtInferLabels
            // 
            txtInferLabels.Dock = DockStyle.Fill;
            txtInferLabels.Location = new Point(232, 367);
            txtInferLabels.Margin = new Padding(4, 3, 4, 3);
            txtInferLabels.Name = "txtInferLabels";
            txtInferLabels.Size = new Size(1000, 34);
            txtInferLabels.TabIndex = 9;
            // 
            // btnBrowseInferLabels
            // 
            btnBrowseInferLabels.Dock = DockStyle.Fill;
            btnBrowseInferLabels.Location = new Point(1240, 366);
            btnBrowseInferLabels.Margin = new Padding(4, 2, 4, 2);
            btnBrowseInferLabels.Name = "btnBrowseInferLabels";
            btnBrowseInferLabels.Size = new Size(206, 66);
            btnBrowseInferLabels.TabIndex = 10;
            btnBrowseInferLabels.Text = "Browse...";
            btnBrowseInferLabels.Click += btnBrowseInferLabels_Click;
            // 
            // lblInferPreset
            // 
            lblInferPreset.Anchor = AnchorStyles.Right;
            lblInferPreset.AutoSize = true;
            lblInferPreset.Location = new Point(97, 455);
            lblInferPreset.Margin = new Padding(4, 0, 4, 0);
            lblInferPreset.Name = "lblInferPreset";
            lblInferPreset.Size = new Size(127, 28);
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
            cboInferPreset.Location = new Point(232, 437);
            cboInferPreset.Margin = new Padding(4, 3, 4, 3);
            cboInferPreset.Name = "cboInferPreset";
            cboInferPreset.Size = new Size(1214, 36);
            cboInferPreset.TabIndex = 14;
            cboInferPreset.SelectedIndexChanged += cboInferPreset_SelectedIndexChanged;
            // 
            // btnInspectOpenVinoModel
            // 
            btnInspectOpenVinoModel.Location = new Point(232, 509);
            btnInspectOpenVinoModel.Margin = new Padding(4, 5, 4, 5);
            btnInspectOpenVinoModel.Name = "btnInspectOpenVinoModel";
            btnInspectOpenVinoModel.Size = new Size(186, 57);
            btnInspectOpenVinoModel.TabIndex = 18;
            btnInspectOpenVinoModel.Text = "Inspect Loaded Model";
            btnInspectOpenVinoModel.Click += btnInspectOpenVinoModel_Click;
            // 
            // lblTestsNote
            // 
            lblTestsNote.AutoSize = true;
            testsLayout.SetColumnSpan(lblTestsNote, 2);
            lblTestsNote.Location = new Point(232, 581);
            lblTestsNote.Margin = new Padding(4, 0, 4, 0);
            lblTestsNote.Name = "lblTestsNote";
            lblTestsNote.Size = new Size(398, 28);
            lblTestsNote.TabIndex = 20;
            lblTestsNote.Text = "This tab uses the loaded Workstation model.";
            // 
            // lblImageResult
            // 
            lblImageResult.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblImageResult.AutoSize = true;
            lblImageResult.Location = new Point(74, 658);
            lblImageResult.Margin = new Padding(4, 17, 4, 0);
            lblImageResult.Name = "lblImageResult";
            lblImageResult.Size = new Size(150, 28);
            lblImageResult.TabIndex = 22;
            lblImageResult.Text = "Returned image";
            // 
            // picImageResult
            // 
            picImageResult.BackColor = Color.Black;
            picImageResult.BorderStyle = BorderStyle.FixedSingle;
            testsLayout.SetColumnSpan(picImageResult, 2);
            picImageResult.Dock = DockStyle.Fill;
            picImageResult.Location = new Point(232, 646);
            picImageResult.Margin = new Padding(4, 5, 4, 5);
            picImageResult.Name = "picImageResult";
            picImageResult.Size = new Size(1214, 357);
            picImageResult.SizeMode = PictureBoxSizeMode.Zoom;
            picImageResult.TabIndex = 23;
            picImageResult.TabStop = false;
            // 
            // btnRunOpenVinoModelTest
            // 
            btnRunOpenVinoModelTest.Dock = DockStyle.Fill;
            btnRunOpenVinoModelTest.Location = new Point(1240, 299);
            btnRunOpenVinoModelTest.Margin = new Padding(4, 5, 4, 5);
            btnRunOpenVinoModelTest.Name = "btnRunOpenVinoModelTest";
            btnRunOpenVinoModelTest.Size = new Size(206, 60);
            btnRunOpenVinoModelTest.TabIndex = 19;
            btnRunOpenVinoModelTest.Text = "Send Message";
            btnRunOpenVinoModelTest.Click += btnRunOpenVinoModelTest_Click;
            // 
            // tabServerTests
            // 
            tabServerTests.Controls.Add(serverTestsLayout);
            tabServerTests.Location = new Point(4, 43);
            tabServerTests.Margin = new Padding(4, 5, 4, 5);
            tabServerTests.Name = "tabServerTests";
            tabServerTests.Padding = new Padding(17, 20, 17, 20);
            tabServerTests.Size = new Size(1524, 488);
            tabServerTests.TabIndex = 8;
            tabServerTests.Text = "Server Tests";
            // 
            // serverTestsLayout
            // 
            serverTestsLayout.AutoScroll = true;
            serverTestsLayout.ColumnCount = 3;
            serverTestsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 214F));
            serverTestsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            serverTestsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 214F));
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
            serverTestsLayout.Location = new Point(17, 20);
            serverTestsLayout.Margin = new Padding(4, 5, 4, 5);
            serverTestsLayout.Name = "serverTestsLayout";
            serverTestsLayout.Padding = new Padding(14, 17, 14, 17);
            serverTestsLayout.RowCount = 13;
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 197F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 183F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            serverTestsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 77F));
            serverTestsLayout.Size = new Size(1490, 448);
            serverTestsLayout.TabIndex = 0;
            // 
            // lblOpenVinoRuntimeHeading
            // 
            lblOpenVinoRuntimeHeading.AutoSize = true;
            serverTestsLayout.SetColumnSpan(lblOpenVinoRuntimeHeading, 3);
            lblOpenVinoRuntimeHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOpenVinoRuntimeHeading.Location = new Point(18, 17);
            lblOpenVinoRuntimeHeading.Margin = new Padding(4, 0, 4, 0);
            lblOpenVinoRuntimeHeading.Name = "lblOpenVinoRuntimeHeading";
            lblOpenVinoRuntimeHeading.Size = new Size(144, 28);
            lblOpenVinoRuntimeHeading.TabIndex = 13;
            lblOpenVinoRuntimeHeading.Text = "API endpoints";
            // 
            // txtServerEndpointList
            // 
            serverTestsLayout.SetColumnSpan(txtServerEndpointList, 3);
            txtServerEndpointList.Dock = DockStyle.Fill;
            txtServerEndpointList.Location = new Point(18, 92);
            txtServerEndpointList.Margin = new Padding(4, 5, 4, 5);
            txtServerEndpointList.Multiline = true;
            txtServerEndpointList.Name = "txtServerEndpointList";
            txtServerEndpointList.ReadOnly = true;
            txtServerEndpointList.ScrollBars = ScrollBars.Vertical;
            txtServerEndpointList.Size = new Size(1428, 187);
            txtServerEndpointList.TabIndex = 37;
            txtServerEndpointList.Text = resources.GetString("txtServerEndpointList.Text");
            // 
            // lblTestApiKey
            // 
            lblTestApiKey.Anchor = AnchorStyles.Left;
            lblTestApiKey.AutoSize = true;
            lblTestApiKey.Location = new Point(18, 305);
            lblTestApiKey.Margin = new Padding(4, 0, 4, 0);
            lblTestApiKey.Name = "lblTestApiKey";
            lblTestApiKey.Size = new Size(136, 28);
            lblTestApiKey.TabIndex = 4;
            lblTestApiKey.Text = "Server API key";
            // 
            // txtTestApiKey
            // 
            serverTestsLayout.SetColumnSpan(txtTestApiKey, 2);
            txtTestApiKey.Dock = DockStyle.Fill;
            txtTestApiKey.Location = new Point(232, 289);
            txtTestApiKey.Margin = new Padding(4, 5, 4, 5);
            txtTestApiKey.Name = "txtTestApiKey";
            txtTestApiKey.Size = new Size(1214, 34);
            txtTestApiKey.TabIndex = 5;
            txtTestApiKey.UseSystemPasswordChar = true;
            // 
            // lblServerNoInputHeading
            // 
            lblServerNoInputHeading.AutoSize = true;
            serverTestsLayout.SetColumnSpan(lblServerNoInputHeading, 3);
            lblServerNoInputHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblServerNoInputHeading.Location = new Point(18, 354);
            lblServerNoInputHeading.Margin = new Padding(4, 0, 4, 0);
            lblServerNoInputHeading.Name = "lblServerNoInputHeading";
            lblServerNoInputHeading.Size = new Size(370, 28);
            lblServerNoInputHeading.TabIndex = 38;
            lblServerNoInputHeading.Text = "Server tests without prompt or image";
            // 
            // lblTestEndpoint
            // 
            lblTestEndpoint.Anchor = AnchorStyles.Left;
            lblTestEndpoint.AutoSize = true;
            lblTestEndpoint.Location = new Point(18, 445);
            lblTestEndpoint.Margin = new Padding(4, 0, 4, 0);
            lblTestEndpoint.Name = "lblTestEndpoint";
            lblTestEndpoint.Size = new Size(90, 28);
            lblTestEndpoint.TabIndex = 1;
            lblTestEndpoint.Text = "Tags URL";
            // 
            // txtTestEndpoint
            // 
            txtTestEndpoint.Dock = DockStyle.Fill;
            txtTestEndpoint.Location = new Point(232, 429);
            txtTestEndpoint.Margin = new Padding(4, 5, 4, 5);
            txtTestEndpoint.Name = "txtTestEndpoint";
            txtTestEndpoint.Size = new Size(1000, 34);
            txtTestEndpoint.TabIndex = 2;
            txtTestEndpoint.Text = "http://192.168.0.11:11434/api/tags";
            // 
            // btnTestEndpoint
            // 
            btnTestEndpoint.Location = new Point(1240, 429);
            btnTestEndpoint.Margin = new Padding(4, 5, 4, 5);
            btnTestEndpoint.Name = "btnTestEndpoint";
            btnTestEndpoint.Size = new Size(186, 57);
            btnTestEndpoint.TabIndex = 3;
            btnTestEndpoint.Text = "Test Tags URL";
            btnTestEndpoint.Click += btnTestEndpoint_Click;
            // 
            // lblModelsEndpoint
            // 
            lblModelsEndpoint.Anchor = AnchorStyles.Left;
            lblModelsEndpoint.AutoSize = true;
            lblModelsEndpoint.Location = new Point(18, 515);
            lblModelsEndpoint.Margin = new Padding(4, 0, 4, 0);
            lblModelsEndpoint.Name = "lblModelsEndpoint";
            lblModelsEndpoint.Size = new Size(117, 28);
            lblModelsEndpoint.TabIndex = 40;
            lblModelsEndpoint.Text = "Models URL";
            // 
            // txtModelsEndpoint
            // 
            txtModelsEndpoint.Dock = DockStyle.Fill;
            txtModelsEndpoint.Location = new Point(232, 499);
            txtModelsEndpoint.Margin = new Padding(4, 5, 4, 5);
            txtModelsEndpoint.Name = "txtModelsEndpoint";
            txtModelsEndpoint.Size = new Size(1000, 34);
            txtModelsEndpoint.TabIndex = 41;
            txtModelsEndpoint.Text = "http://192.168.0.11:11434/v1/models";
            // 
            // btnTestModelsEndpoint
            // 
            btnTestModelsEndpoint.Location = new Point(1240, 499);
            btnTestModelsEndpoint.Margin = new Padding(4, 5, 4, 5);
            btnTestModelsEndpoint.Name = "btnTestModelsEndpoint";
            btnTestModelsEndpoint.Size = new Size(186, 57);
            btnTestModelsEndpoint.TabIndex = 42;
            btnTestModelsEndpoint.Text = "Test Models";
            btnTestModelsEndpoint.Click += btnTestModelsEndpoint_Click;
            // 
            // lblServerUserInputHeading
            // 
            lblServerUserInputHeading.AutoSize = true;
            serverTestsLayout.SetColumnSpan(lblServerUserInputHeading, 3);
            lblServerUserInputHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblServerUserInputHeading.Location = new Point(18, 564);
            lblServerUserInputHeading.Margin = new Padding(4, 0, 4, 0);
            lblServerUserInputHeading.Name = "lblServerUserInputHeading";
            lblServerUserInputHeading.Size = new Size(338, 28);
            lblServerUserInputHeading.TabIndex = 39;
            lblServerUserInputHeading.Text = "Server tests with prompt or image";
            // 
            // lblChatModel
            // 
            lblChatModel.Anchor = AnchorStyles.Left;
            lblChatModel.AutoSize = true;
            lblChatModel.Location = new Point(18, 655);
            lblChatModel.Margin = new Padding(4, 0, 4, 0);
            lblChatModel.Name = "lblChatModel";
            lblChatModel.Size = new Size(181, 28);
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
            cboServerModel.Location = new Point(232, 639);
            cboServerModel.Margin = new Padding(4, 5, 4, 5);
            cboServerModel.Name = "cboServerModel";
            cboServerModel.Size = new Size(1214, 36);
            cboServerModel.TabIndex = 9;
            // 
            // lblChatPrompt
            // 
            lblChatPrompt.Anchor = AnchorStyles.Left;
            lblChatPrompt.AutoSize = true;
            lblChatPrompt.Location = new Point(18, 781);
            lblChatPrompt.Margin = new Padding(4, 0, 4, 0);
            lblChatPrompt.Name = "lblChatPrompt";
            lblChatPrompt.Size = new Size(139, 28);
            lblChatPrompt.TabIndex = 10;
            lblChatPrompt.Text = "Server prompt";
            // 
            // txtChatPrompt
            // 
            serverTestsLayout.SetColumnSpan(txtChatPrompt, 2);
            txtChatPrompt.Dock = DockStyle.Fill;
            txtChatPrompt.Location = new Point(232, 709);
            txtChatPrompt.Margin = new Padding(4, 5, 4, 5);
            txtChatPrompt.Multiline = true;
            txtChatPrompt.Name = "txtChatPrompt";
            txtChatPrompt.ScrollBars = ScrollBars.Vertical;
            txtChatPrompt.Size = new Size(1214, 173);
            txtChatPrompt.TabIndex = 11;
            txtChatPrompt.Text = "Reply with one short sentence confirming the chat endpoint is working.";
            // 
            // lblChatEndpoint
            // 
            lblChatEndpoint.Anchor = AnchorStyles.Left;
            lblChatEndpoint.AutoSize = true;
            lblChatEndpoint.Location = new Point(18, 908);
            lblChatEndpoint.Margin = new Padding(4, 0, 4, 0);
            lblChatEndpoint.Name = "lblChatEndpoint";
            lblChatEndpoint.Size = new Size(163, 28);
            lblChatEndpoint.TabIndex = 6;
            lblChatEndpoint.Text = "Completions URL";
            // 
            // txtChatEndpoint
            // 
            txtChatEndpoint.Dock = DockStyle.Fill;
            txtChatEndpoint.Location = new Point(232, 892);
            txtChatEndpoint.Margin = new Padding(4, 5, 4, 5);
            txtChatEndpoint.Name = "txtChatEndpoint";
            txtChatEndpoint.Size = new Size(1000, 34);
            txtChatEndpoint.TabIndex = 7;
            txtChatEndpoint.Text = "http://192.168.0.11:11434/v1/chat/completions";
            // 
            // btnTestChat
            // 
            btnTestChat.Location = new Point(1240, 892);
            btnTestChat.Margin = new Padding(4, 5, 4, 5);
            btnTestChat.Name = "btnTestChat";
            btnTestChat.Size = new Size(186, 57);
            btnTestChat.TabIndex = 12;
            btnTestChat.Text = "Test Completions";
            btnTestChat.Click += btnTestChat_Click;
            // 
            // lblTextEndpoint
            // 
            lblTextEndpoint.Anchor = AnchorStyles.Left;
            lblTextEndpoint.AutoSize = true;
            lblTextEndpoint.Location = new Point(18, 978);
            lblTextEndpoint.Margin = new Padding(4, 0, 4, 0);
            lblTextEndpoint.Name = "lblTextEndpoint";
            lblTextEndpoint.Size = new Size(86, 28);
            lblTextEndpoint.TabIndex = 31;
            lblTextEndpoint.Text = "Text URL";
            // 
            // txtTextEndpoint
            // 
            txtTextEndpoint.Dock = DockStyle.Fill;
            txtTextEndpoint.Location = new Point(232, 962);
            txtTextEndpoint.Margin = new Padding(4, 5, 4, 5);
            txtTextEndpoint.Name = "txtTextEndpoint";
            txtTextEndpoint.Size = new Size(1000, 34);
            txtTextEndpoint.TabIndex = 32;
            txtTextEndpoint.Text = "http://192.168.0.11:11434/v1/completions";
            // 
            // btnTestTextEndpoint
            // 
            btnTestTextEndpoint.Location = new Point(1240, 962);
            btnTestTextEndpoint.Margin = new Padding(4, 5, 4, 5);
            btnTestTextEndpoint.Name = "btnTestTextEndpoint";
            btnTestTextEndpoint.Size = new Size(186, 57);
            btnTestTextEndpoint.TabIndex = 33;
            btnTestTextEndpoint.Text = "Test Text";
            btnTestTextEndpoint.Click += btnTestTextEndpoint_Click;
            // 
            // lblImageEndpoint
            // 
            lblImageEndpoint.Anchor = AnchorStyles.Left;
            lblImageEndpoint.AutoSize = true;
            lblImageEndpoint.Location = new Point(18, 1048);
            lblImageEndpoint.Margin = new Padding(4, 0, 4, 0);
            lblImageEndpoint.Name = "lblImageEndpoint";
            lblImageEndpoint.Size = new Size(106, 28);
            lblImageEndpoint.TabIndex = 34;
            lblImageEndpoint.Text = "Image URL";
            // 
            // txtImageEndpoint
            // 
            txtImageEndpoint.Dock = DockStyle.Fill;
            txtImageEndpoint.Location = new Point(232, 1032);
            txtImageEndpoint.Margin = new Padding(4, 5, 4, 5);
            txtImageEndpoint.Name = "txtImageEndpoint";
            txtImageEndpoint.Size = new Size(1000, 34);
            txtImageEndpoint.TabIndex = 35;
            txtImageEndpoint.Text = "http://192.168.0.11:11434/v1/images/generations";
            // 
            // btnTestImageEndpoint
            // 
            btnTestImageEndpoint.Location = new Point(1240, 1032);
            btnTestImageEndpoint.Margin = new Padding(4, 5, 4, 5);
            btnTestImageEndpoint.Name = "btnTestImageEndpoint";
            btnTestImageEndpoint.Size = new Size(186, 57);
            btnTestImageEndpoint.TabIndex = 36;
            btnTestImageEndpoint.Text = "Test Image";
            btnTestImageEndpoint.Click += btnTestImageEndpoint_Click;
            // 
            // btnShowServerSettings
            // 
            btnShowServerSettings.Location = new Point(1240, 1102);
            btnShowServerSettings.Margin = new Padding(4, 5, 4, 5);
            btnShowServerSettings.Name = "btnShowServerSettings";
            btnShowServerSettings.Size = new Size(186, 57);
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
            outputLayout.Margin = new Padding(4, 5, 4, 5);
            outputLayout.Name = "outputLayout";
            outputLayout.RowCount = 2;
            outputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 97F));
            outputLayout.Size = new Size(1532, 774);
            outputLayout.TabIndex = 0;
            // 
            // grpOutput
            // 
            grpOutput.Controls.Add(outputContentLayout);
            grpOutput.Dock = DockStyle.Fill;
            grpOutput.Location = new Point(4, 5);
            grpOutput.Margin = new Padding(4, 5, 4, 5);
            grpOutput.Name = "grpOutput";
            grpOutput.Padding = new Padding(4, 5, 4, 5);
            grpOutput.Size = new Size(1524, 667);
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
            outputContentLayout.Location = new Point(4, 29);
            outputContentLayout.Margin = new Padding(4, 5, 4, 5);
            outputContentLayout.Name = "outputContentLayout";
            outputContentLayout.RowCount = 2;
            outputContentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            outputContentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outputContentLayout.Size = new Size(1516, 633);
            outputContentLayout.TabIndex = 0;
            // 
            // lblOutputInfo
            // 
            lblOutputInfo.AutoSize = true;
            lblOutputInfo.Dock = DockStyle.Fill;
            lblOutputInfo.Location = new Point(4, 0);
            lblOutputInfo.Margin = new Padding(4, 0, 4, 0);
            lblOutputInfo.Name = "lblOutputInfo";
            lblOutputInfo.Size = new Size(1508, 40);
            lblOutputInfo.TabIndex = 0;
            lblOutputInfo.Text = "Messages, help, errors, download status, and model details appear here.";
            lblOutputInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtOutput
            // 
            txtOutput.BackColor = Color.FromArgb(20, 20, 20);
            txtOutput.BorderStyle = BorderStyle.None;
            txtOutput.Dock = DockStyle.Fill;
            txtOutput.Font = new Font("Cascadia Mono", 9F);
            txtOutput.ForeColor = Color.Gainsboro;
            txtOutput.Location = new Point(4, 45);
            txtOutput.Margin = new Padding(4, 5, 4, 5);
            txtOutput.Name = "txtOutput";
            txtOutput.ReadOnly = true;
            txtOutput.Size = new Size(1508, 583);
            txtOutput.TabIndex = 1;
            txtOutput.Text = "OpenVINO Workbench layout initialized.\n";
            // 
            // outputButtons
            // 
            outputButtons.ColumnCount = 3;
            outputButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 223F));
            outputButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            outputButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 151F));
            outputButtons.Controls.Add(btnClearOutput, 0, 0);
            outputButtons.Controls.Add(activityLayout, 1, 0);
            outputButtons.Controls.Add(btnHelp, 2, 0);
            outputButtons.Dock = DockStyle.Fill;
            outputButtons.Location = new Point(4, 682);
            outputButtons.Margin = new Padding(4, 5, 4, 5);
            outputButtons.Name = "outputButtons";
            outputButtons.Padding = new Padding(0, 10, 0, 0);
            outputButtons.RowCount = 1;
            outputButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outputButtons.Size = new Size(1524, 87);
            outputButtons.TabIndex = 1;
            // 
            // btnClearOutput
            // 
            btnClearOutput.Location = new Point(4, 15);
            btnClearOutput.Margin = new Padding(4, 5, 4, 5);
            btnClearOutput.Name = "btnClearOutput";
            btnClearOutput.Size = new Size(214, 60);
            btnClearOutput.TabIndex = 0;
            btnClearOutput.Text = "Clear Output";
            btnClearOutput.Click += btnClearOutput_Click;
            // 
            // activityLayout
            // 
            activityLayout.ColumnCount = 2;
            activityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            activityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 137F));
            activityLayout.Controls.Add(lblActivityStatus, 0, 0);
            activityLayout.Controls.Add(progressActivity, 0, 1);
            activityLayout.Controls.Add(btnCancelActivity, 1, 0);
            activityLayout.Dock = DockStyle.Fill;
            activityLayout.Location = new Point(227, 15);
            activityLayout.Margin = new Padding(4, 5, 11, 5);
            activityLayout.Name = "activityLayout";
            activityLayout.RowCount = 2;
            activityLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            activityLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            activityLayout.Size = new Size(1135, 67);
            activityLayout.TabIndex = 2;
            activityLayout.Visible = false;
            // 
            // lblActivityStatus
            // 
            lblActivityStatus.AutoEllipsis = true;
            lblActivityStatus.Dock = DockStyle.Fill;
            lblActivityStatus.Location = new Point(4, 0);
            lblActivityStatus.Margin = new Padding(4, 0, 4, 0);
            lblActivityStatus.Name = "lblActivityStatus";
            lblActivityStatus.Size = new Size(990, 33);
            lblActivityStatus.TabIndex = 0;
            lblActivityStatus.Text = "Working...";
            lblActivityStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressActivity
            // 
            progressActivity.Dock = DockStyle.Fill;
            progressActivity.Location = new Point(4, 36);
            progressActivity.Margin = new Padding(4, 3, 4, 0);
            progressActivity.MarqueeAnimationSpeed = 28;
            progressActivity.Name = "progressActivity";
            progressActivity.Size = new Size(990, 31);
            progressActivity.Style = ProgressBarStyle.Marquee;
            progressActivity.TabIndex = 1;
            // 
            // btnCancelActivity
            // 
            btnCancelActivity.Dock = DockStyle.Fill;
            btnCancelActivity.Enabled = false;
            btnCancelActivity.Location = new Point(1002, 0);
            btnCancelActivity.Margin = new Padding(4, 0, 4, 0);
            btnCancelActivity.Name = "btnCancelActivity";
            activityLayout.SetRowSpan(btnCancelActivity, 2);
            btnCancelActivity.Size = new Size(129, 67);
            btnCancelActivity.TabIndex = 2;
            btnCancelActivity.Text = "Cancel";
            btnCancelActivity.Visible = false;
            btnCancelActivity.Click += btnCancelActivity_Click;
            // 
            // btnHelp
            // 
            btnHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHelp.Location = new Point(1377, 15);
            btnHelp.Margin = new Padding(4, 5, 4, 5);
            btnHelp.Name = "btnHelp";
            btnHelp.Size = new Size(143, 60);
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
            txtRepoId.Size = new Size(320, 31);
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
            txtRevision.Size = new Size(150, 31);
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
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1576, 1468);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            Margin = new Padding(4, 5, 4, 5);
            MinimumSize = new Size(805, 474);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "OpenVINO Workbench";
            Load += Form1_Load;
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
            tabConvert.ResumeLayout(false);
            convertLayout.ResumeLayout(false);
            convertLayout.PerformLayout();
            grpConvertOutputs.ResumeLayout(false);
            pnlIrVisualizer.ResumeLayout(false);
            pnlIrVisualizer.PerformLayout();
            tabOvmsPull.ResumeLayout(false);
            ovmsPullLayout.ResumeLayout(false);
            ovmsPullLayout.PerformLayout();
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
private Label lblInferModel; private TextBox txtInferModel; private Button btnBrowseInferModel; private Button btnUseSampleModel; private Label lblInferImage; private TextBox txtInferImage; private FlowLayoutPanel imageTestButtons; private Button btnBrowseInferImage; private Button btnRunImageTest; private Label lblInferText; private TextBox txtInferText; private Button btnBrowseInferLabels; private Label lblInferLabels; private TextBox txtInferLabels; private Label lblInferPreset; private ComboBox cboInferPreset;
        private TabPage tabDownloads; private Panel modelsScroll; private Panel modelsLayout; private TabPage tabConvert; private Panel convertLayout; private Button btnDeleteDownload; private Label lblModelWebsites; private ComboBox cboModelWebsites; private Button btnOpenModelWebsite; private Button btnAddModelWebsite; private Button btnDeleteModelWebsite; private Label lblHfQuery; private TextBox txtHfQuery; private Button btnSearchRepos; private Button btnSearchAllModels; private Button btnPresetHuggingFace; private Button btnPresetOpenVino; private Button btnPresetIntel; private Button btnPresetUltralytics; private Button btnPresetOllama; private Button btnOfficialFiles; private Button btnRefreshOfficialIndex; private ListView lstHfRepos; private ColumnHeader colModelId; private ColumnHeader colDownloads; private ColumnHeader colLikes; private ColumnHeader colModelSource; private ColumnHeader colTask; private ColumnHeader colLibrary; private ColumnHeader colQuantization; private Button btnLoadFiles; private Button btnClearFileSelection; private Button btnDownloadHelp; private Label lblRepoId; private TextBox txtRepoId; private Label lblRevision; private TextBox txtRevision; private Label lblDownloadUrl; private TextBox txtDownloadUrl; private Button btnCopyDownloadUrl; private ListView lstHfFiles; private ColumnHeader colFilePath; private ColumnHeader colFileType; private ColumnHeader colFileExtension; private ColumnHeader colFileSize; private ColumnHeader colFileNotes; private ProgressBar progressDownload; private Label lblDownloadStatus; private Button btnCancelDownload; private Label modelsSeparator2; private Label lblOmzBuilderHeading; private ComboBox cboConvertSourceModel; private Button btnRefreshConvertModels; private GroupBox grpConvertOutputs; private CheckBox chkConvertBlob; private Button btnConvertToIr; private Panel pnlIrVisualizer; private Label lblIrPlaceholder; private Label lblModelsStatus;
        private TabPage tabOvmsPull; private Panel ovmsPullLayout; private Label lblOvmsPullHeading; private Label lblOvmsExecutable; private TextBox txtOvmsExecutable; private Label lblOvmsPythonPath; private TextBox txtOvmsPythonPath; private Label lblOvmsExportToolsPath; private TextBox txtOvmsExportToolsPath; private Label lblOvmsSourceFormat; private ComboBox cboOvmsSourceFormat; private Label lblOvmsTask; private ComboBox cboOvmsTask; private Label lblOvmsPipelineType; private ComboBox cboOvmsPipelineType; private Label lblOvmsWeightFormat; private ComboBox cboOvmsWeightFormat; private Label lblOvmsSourceModel; private ComboBox cboOvmsSourceModel; private Label lblOvmsGgufFilename; private TextBox txtOvmsGgufFilename; private Label lblOvmsModelRepository; private TextBox txtOvmsModelRepository; private Label lblOvmsLogPath; private TextBox txtOvmsLogPath; private CheckBox chkOvmsOverwrite; private CheckBox chkOvmsDebug; private Button btnCheckOvmsVersions; private Button btnPreviewOvmsPull; private Button btnRunOvmsPull; private Label lblOvmsPullStatus;
        private TabPage tabTests; private TableLayoutPanel testsLayout; private Label lblTestsHeading; private Label lblTestModelPath; private TextBox txtTestModelPath; private Button btnInspectOpenVinoModel; private Button btnRunOpenVinoModelTest; private Label lblTestsNote; private Label lblImageResult; private PictureBox picImageResult; private TabPage tabServerTests; private TableLayoutPanel serverTestsLayout; private TextBox txtServerEndpointList; private Label lblServerNoInputHeading; private Label lblServerUserInputHeading; private Label lblTestEndpoint; private TextBox txtTestEndpoint; private Button btnTestEndpoint; private Label lblModelsEndpoint; private TextBox txtModelsEndpoint; private Button btnTestModelsEndpoint; private Label lblTestApiKey; private TextBox txtTestApiKey; private Label lblChatEndpoint; private TextBox txtChatEndpoint; private Label lblTextEndpoint; private TextBox txtTextEndpoint; private Button btnTestTextEndpoint; private Label lblImageEndpoint; private TextBox txtImageEndpoint; private Button btnTestImageEndpoint; private Label lblChatModel; private ComboBox cboServerModel; private Label lblChatPrompt; private TextBox txtChatPrompt; private Button btnTestChat; private Button btnShowServerSettings; private Label lblOpenVinoRuntimeHeading; private Label lblLocalServerUrl; private TextBox txtLocalServerUrl; private Button btnStartLocalServer; private Label lblLocalServerStatus; private TextBox txtLocalServerStatus; private Button btnStopLocalServer;
        private TableLayoutPanel outputLayout; private GroupBox grpOutput; private TableLayoutPanel outputContentLayout; private Label lblOutputInfo; private RichTextBox txtOutput; private TableLayoutPanel outputButtons; private Button btnClearOutput; private TableLayoutPanel activityLayout; private Label lblActivityStatus; private ProgressBar progressActivity; private Button btnCancelActivity; private Button btnHelp;
        private Button btnLoadModel;
        private SplitContainer splitContainer1;
    }
}






