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

    private void InitializeComponent() {
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
      txtServerStatus = new TextBox();
      lblServerStatus = new Label();
      btnConnectServer = new Button();
      lblServerAddress = new Label();
      btnUnloadModel = new Button();
      btnLoadModel = new Button();
      txtServerAddress = new TextBox();
      lblModelRoot = new Label();
      txtModelRoot = new TextBox();
      btnBrowseModelRoot = new Button();
      lblInferModel = new Label();
      txtInferModel = new TextBox();
      btnBrowseInferModel = new Button();
      lblModelSize = new Label();
      splitContainer1 = new SplitContainer();
      cboWorkstationTarget = new ComboBox();
      btnCheckWorkstation = new Button();
      lblWorkstationMultiTarget = new Label();
      cboWorkstationMultiTarget = new ComboBox();
      lblModelCapabilities = new Label();
      txtModelCapabilities = new TextBox();
      tabChat = new TabPage();
      chatLayout = new TableLayoutPanel();
      lblChatModel = new Label();
      cboChatModel = new ComboBox();
      btnChatRefresh = new Button();
      btnChatNew = new Button();
      txtChatTranscript = new RichTextBox();
      txtChatInput = new TextBox();
      btnChatAttach = new Button();
      btnChatSend = new Button();
      lblChatImages = new Label();
      chkChatTools = new CheckBox();
      lblChatFolder = new Label();
      btnChatFolder = new Button();
      btnChatFolderClear = new Button();
      chkChatRunCommands = new CheckBox();
      tabDownloads = new TabPage();
      modelsScroll = new Panel();
      modelsLayout = new Panel();
      lblDownloadUrl = new Label();
      txtDownloadUrl = new TextBox();
      btnFindDownloadAddress = new Button();
      btnDownload = new Button();
      btnPullFromDownloads = new Button();
      progressDownload = new ProgressBar();
      lblDownloadStatus = new Label();
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
      tabChat.SuspendLayout();
      chatLayout.SuspendLayout();
      tabDownloads.SuspendLayout();
      modelsScroll.SuspendLayout();
      modelsLayout.SuspendLayout();
      tabOvmsPull.SuspendLayout();
      ovmsPullLayout.SuspendLayout();
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
      mainLayout.Size = new Size(1103, 853);
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
      lblSubtitle.Size = new Size(288, 15);
      lblSubtitle.TabIndex = 1;
      lblSubtitle.Text = "Download models, serve them with OVMS, and chat with them.";
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
      mainSplit.Size = new Size(1073, 763);
      mainSplit.SplitterDistance = 470;
      mainSplit.SplitterWidth = 12;
      mainSplit.TabIndex = 2;
      // 
      // tabMain
      // 
      tabMain.Controls.Add(tabWorkstation);
      tabMain.Controls.Add(tabChat);
      tabMain.Controls.Add(tabDownloads);
      tabMain.Controls.Add(tabOvmsPull);
      tabMain.Dock = DockStyle.Fill;
      tabMain.Font = new Font("Segoe UI", 10F);
      tabMain.Location = new Point(0, 0);
      tabMain.Name = "tabMain";
      tabMain.Padding = new Point(14, 6);
      tabMain.SelectedIndex = 0;
      tabMain.Size = new Size(1071, 329);
      tabMain.TabIndex = 0;
      // 
      // tabWorkstation
      // 
      tabWorkstation.AutoScroll = true;
      tabWorkstation.Controls.Add(workstationLayout);
      tabWorkstation.Location = new Point(4, 32);
      tabWorkstation.Name = "tabWorkstation";
      tabWorkstation.Padding = new Padding(12);
      tabWorkstation.Size = new Size(1063, 293);
      tabWorkstation.TabIndex = 0;
      tabWorkstation.Text = "Workstation";
      // 
      // workstationLayout
      // 
      workstationLayout.AutoSize = true;
      workstationLayout.ColumnCount = 1;
      workstationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      workstationLayout.Controls.Add(lblWorkstationHeading, 0, 0);
      workstationLayout.Controls.Add(modelRootLayout, 0, 1);
      workstationLayout.Dock = DockStyle.Top;
      workstationLayout.Location = new Point(12, 12);
      workstationLayout.Name = "workstationLayout";
      workstationLayout.RowCount = 2;
      workstationLayout.RowStyles.Add(new RowStyle());
      workstationLayout.RowStyles.Add(new RowStyle());
      workstationLayout.Size = new Size(1022, 328);
      workstationLayout.TabIndex = 0;
      // 
      // lblWorkstationHeading
      // 
      lblWorkstationHeading.AutoSize = true;
      lblWorkstationHeading.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
      lblWorkstationHeading.Location = new Point(3, 0);
      lblWorkstationHeading.Name = "lblWorkstationHeading";
      lblWorkstationHeading.Size = new Size(167, 20);
      lblWorkstationHeading.TabIndex = 0;
      lblWorkstationHeading.Text = "Serve a model with OVMS";
      // 
      // modelRootLayout
      // 
      modelRootLayout.AutoSize = true;
      modelRootLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      modelRootLayout.ColumnCount = 4;
      modelRootLayout.ColumnStyles.Add(new ColumnStyle());
      modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
      modelRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 141F));
      modelRootLayout.Controls.Add(lblWorkstationTarget, 0, 2);
      modelRootLayout.Controls.Add(txtServerStatus, 1, 6);
      modelRootLayout.Controls.Add(lblServerStatus, 0, 6);
      modelRootLayout.Controls.Add(btnConnectServer, 2, 5);
      modelRootLayout.Controls.Add(lblServerAddress, 0, 5);
      modelRootLayout.Controls.Add(btnUnloadModel, 3, 5);
      modelRootLayout.Controls.Add(btnLoadModel, 2, 2);
      modelRootLayout.Controls.Add(txtServerAddress, 1, 5);
      modelRootLayout.Controls.Add(lblModelRoot, 0, 0);
      modelRootLayout.Controls.Add(txtModelRoot, 1, 0);
      modelRootLayout.Controls.Add(btnBrowseModelRoot, 2, 0);
      modelRootLayout.Controls.Add(lblInferModel, 0, 1);
      modelRootLayout.Controls.Add(txtInferModel, 1, 1);
      modelRootLayout.Controls.Add(btnBrowseInferModel, 2, 1);
      modelRootLayout.Controls.Add(lblModelSize, 3, 1);
      modelRootLayout.Controls.Add(splitContainer1, 1, 2);
      modelRootLayout.Controls.Add(lblWorkstationMultiTarget, 0, 3);
      modelRootLayout.Controls.Add(cboWorkstationMultiTarget, 1, 3);
      modelRootLayout.Controls.Add(lblModelCapabilities, 0, 4);
      modelRootLayout.Controls.Add(txtModelCapabilities, 1, 4);
      modelRootLayout.Dock = DockStyle.Top;
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
      modelRootLayout.Size = new Size(1016, 302);
      modelRootLayout.TabIndex = 1;
      // 
      // lblWorkstationTarget
      // 
      lblWorkstationTarget.AutoSize = true;
      lblWorkstationTarget.Dock = DockStyle.Fill;
      lblWorkstationTarget.Location = new Point(3, 88);
      lblWorkstationTarget.Margin = new Padding(3, 8, 3, 0);
      lblWorkstationTarget.Name = "lblWorkstationTarget";
      lblWorkstationTarget.Size = new Size(124, 33);
      lblWorkstationTarget.TabIndex = 1;
      lblWorkstationTarget.Text = "Model target:";
      lblWorkstationTarget.TextAlign = ContentAlignment.MiddleRight;
      // 
      // txtServerStatus
      // 
      txtServerStatus.Anchor = AnchorStyles.Left;
      txtServerStatus.Location = new Point(133, 271);
      txtServerStatus.Name = "txtServerStatus";
      txtServerStatus.ReadOnly = true;
      txtServerStatus.Size = new Size(536, 25);
      txtServerStatus.TabIndex = 29;
      txtServerStatus.Text = "Stopped";
      // 
      // lblServerStatus
      // 
      lblServerStatus.AutoSize = true;
      lblServerStatus.Dock = DockStyle.Fill;
      lblServerStatus.Location = new Point(3, 266);
      lblServerStatus.Name = "lblServerStatus";
      lblServerStatus.Size = new Size(124, 36);
      lblServerStatus.TabIndex = 28;
      lblServerStatus.Text = "Status:";
      lblServerStatus.TextAlign = ContentAlignment.MiddleRight;
      // 
      // btnConnectServer
      // 
      btnConnectServer.Dock = DockStyle.Fill;
      btnConnectServer.Location = new Point(763, 230);
      btnConnectServer.Name = "btnConnectServer";
      btnConnectServer.Size = new Size(109, 33);
      btnConnectServer.TabIndex = 27;
      btnConnectServer.Text = "Connect";
      btnConnectServer.Click += btnConnectServer_Click;
      // 
      // lblServerAddress
      // 
      lblServerAddress.AutoSize = true;
      lblServerAddress.Dock = DockStyle.Fill;
      lblServerAddress.Location = new Point(3, 227);
      lblServerAddress.Name = "lblServerAddress";
      lblServerAddress.Size = new Size(124, 39);
      lblServerAddress.TabIndex = 25;
      lblServerAddress.Text = "Server address:";
      lblServerAddress.TextAlign = ContentAlignment.MiddleRight;
      // 
      // btnUnloadModel
      // 
      btnUnloadModel.Dock = DockStyle.Fill;
      btnUnloadModel.Location = new Point(878, 230);
      btnUnloadModel.Name = "btnUnloadModel";
      btnUnloadModel.Size = new Size(135, 33);
      btnUnloadModel.TabIndex = 30;
      btnUnloadModel.Text = "Unload Model";
      btnUnloadModel.Click += btnUnloadModel_Click;
      // 
      // btnLoadModel
      // 
      modelRootLayout.SetColumnSpan(btnLoadModel, 2);
      btnLoadModel.Dock = DockStyle.Fill;
      btnLoadModel.Location = new Point(763, 83);
      btnLoadModel.Name = "btnLoadModel";
      btnLoadModel.Padding = new Padding(8, 3, 8, 3);
      btnLoadModel.Size = new Size(250, 35);
      btnLoadModel.TabIndex = 15;
      btnLoadModel.Text = "Load Model";
      btnLoadModel.Click += btnLoadModel_Click;
      // 
      // txtServerAddress
      // 
      txtServerAddress.Dock = DockStyle.Fill;
      txtServerAddress.Location = new Point(133, 230);
      txtServerAddress.Name = "txtServerAddress";
      txtServerAddress.Size = new Size(624, 25);
      txtServerAddress.TabIndex = 26;
      txtServerAddress.Text = "http://localhost:8000";
      // 
      // lblModelRoot
      // 
      lblModelRoot.AutoSize = true;
      lblModelRoot.Dock = DockStyle.Fill;
      lblModelRoot.Location = new Point(3, 0);
      lblModelRoot.Name = "lblModelRoot";
      lblModelRoot.Size = new Size(124, 40);
      lblModelRoot.TabIndex = 0;
      lblModelRoot.Text = "Main model folder:";
      lblModelRoot.TextAlign = ContentAlignment.MiddleRight;
      // 
      // txtModelRoot
      // 
      txtModelRoot.Dock = DockStyle.Fill;
      txtModelRoot.Location = new Point(133, 3);
      txtModelRoot.Name = "txtModelRoot";
      txtModelRoot.Size = new Size(624, 25);
      txtModelRoot.TabIndex = 1;
      txtModelRoot.KeyDown += txtModelRoot_KeyDown;
      txtModelRoot.Leave += txtModelRoot_Leave;
      // 
      // btnBrowseModelRoot
      // 
      modelRootLayout.SetColumnSpan(btnBrowseModelRoot, 2);
      btnBrowseModelRoot.Dock = DockStyle.Fill;
      btnBrowseModelRoot.Location = new Point(763, 2);
      btnBrowseModelRoot.Margin = new Padding(3, 2, 3, 2);
      btnBrowseModelRoot.Name = "btnBrowseModelRoot";
      btnBrowseModelRoot.Size = new Size(250, 36);
      btnBrowseModelRoot.TabIndex = 2;
      btnBrowseModelRoot.Text = "Browse...";
      btnBrowseModelRoot.Click += btnBrowseModelRoot_Click;
      // 
      // lblInferModel
      // 
      lblInferModel.AutoSize = true;
      lblInferModel.Dock = DockStyle.Fill;
      lblInferModel.Location = new Point(3, 40);
      lblInferModel.Name = "lblInferModel";
      lblInferModel.Size = new Size(124, 40);
      lblInferModel.TabIndex = 0;
      lblInferModel.Text = "Model:";
      lblInferModel.TextAlign = ContentAlignment.MiddleRight;
      // 
      // txtInferModel
      // 
      txtInferModel.Dock = DockStyle.Fill;
      txtInferModel.Location = new Point(133, 43);
      txtInferModel.Name = "txtInferModel";
      txtInferModel.Size = new Size(624, 25);
      txtInferModel.TabIndex = 1;
      //
      // lblModelSize
      //
      lblModelSize.Dock = DockStyle.Fill;
      lblModelSize.Location = new Point(878, 40);
      lblModelSize.Name = "lblModelSize";
      lblModelSize.Size = new Size(135, 40);
      lblModelSize.TabIndex = 3;
      lblModelSize.TextAlign = ContentAlignment.MiddleLeft;
      //
      // btnBrowseInferModel
      //
      btnBrowseInferModel.Dock = DockStyle.Fill;
      btnBrowseInferModel.Location = new Point(763, 42);
      btnBrowseInferModel.Margin = new Padding(3, 2, 3, 2);
      btnBrowseInferModel.Name = "btnBrowseInferModel";
      btnBrowseInferModel.Size = new Size(109, 36);
      btnBrowseInferModel.TabIndex = 2;
      btnBrowseInferModel.Text = "Select";
      btnBrowseInferModel.Click += btnBrowseInferModel_Click;
      // 
      // splitContainer1
      // 
      splitContainer1.Dock = DockStyle.Fill;
      splitContainer1.Location = new Point(133, 83);
      splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      splitContainer1.Panel1.Controls.Add(cboWorkstationTarget);
      // 
      // splitContainer1.Panel2
      // 
      splitContainer1.Panel2.Controls.Add(btnCheckWorkstation);
      splitContainer1.Size = new Size(624, 35);
      splitContainer1.SplitterDistance = 299;
      splitContainer1.TabIndex = 16;
      // 
      // cboWorkstationTarget
      // 
      cboWorkstationTarget.Dock = DockStyle.Fill;
      cboWorkstationTarget.DropDownStyle = ComboBoxStyle.DropDownList;
      cboWorkstationTarget.Location = new Point(0, 0);
      cboWorkstationTarget.Margin = new Padding(3, 5, 3, 0);
      cboWorkstationTarget.Name = "cboWorkstationTarget";
      cboWorkstationTarget.Size = new Size(299, 25);
      cboWorkstationTarget.TabIndex = 2;
      cboWorkstationTarget.SelectedIndexChanged += cboWorkstationTarget_SelectedIndexChanged;
      // 
      // btnCheckWorkstation
      // 
      btnCheckWorkstation.Dock = DockStyle.Fill;
      btnCheckWorkstation.Location = new Point(0, 0);
      btnCheckWorkstation.Margin = new Padding(3, 1, 3, 1);
      btnCheckWorkstation.Name = "btnCheckWorkstation";
      btnCheckWorkstation.Size = new Size(321, 35);
      btnCheckWorkstation.TabIndex = 0;
      btnCheckWorkstation.Text = "Check Workstation";
      btnCheckWorkstation.Click += btnCheckWorkstation_Click;
      // 
      // lblWorkstationMultiTarget
      // 
      lblWorkstationMultiTarget.AutoSize = true;
      lblWorkstationMultiTarget.Dock = DockStyle.Fill;
      lblWorkstationMultiTarget.Location = new Point(3, 129);
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
      cboWorkstationMultiTarget.Location = new Point(133, 124);
      cboWorkstationMultiTarget.Name = "cboWorkstationMultiTarget";
      cboWorkstationMultiTarget.Size = new Size(880, 25);
      cboWorkstationMultiTarget.TabIndex = 32;
      // 
      // lblModelCapabilities
      // 
      lblModelCapabilities.AutoSize = true;
      lblModelCapabilities.Dock = DockStyle.Fill;
      lblModelCapabilities.Location = new Point(3, 165);
      lblModelCapabilities.Margin = new Padding(3, 8, 3, 0);
      lblModelCapabilities.Name = "lblModelCapabilities";
      lblModelCapabilities.Size = new Size(124, 62);
      lblModelCapabilities.TabIndex = 17;
      lblModelCapabilities.Text = "Loaded model:";
      lblModelCapabilities.TextAlign = ContentAlignment.TopRight;
      // 
      // txtModelCapabilities
      // 
      modelRootLayout.SetColumnSpan(txtModelCapabilities, 3);
      txtModelCapabilities.Dock = DockStyle.Fill;
      txtModelCapabilities.Location = new Point(133, 160);
      txtModelCapabilities.Multiline = true;
      txtModelCapabilities.Name = "txtModelCapabilities";
      txtModelCapabilities.ReadOnly = true;
      txtModelCapabilities.ScrollBars = ScrollBars.Vertical;
      txtModelCapabilities.Size = new Size(880, 64);
      txtModelCapabilities.TabIndex = 18;
      txtModelCapabilities.Text = "No model loaded by this app.";
      //
      // tabChat
      //
      tabChat.Controls.Add(chatLayout);
      tabChat.Location = new Point(4, 32);
      tabChat.Name = "tabChat";
      tabChat.Padding = new Padding(8);
      tabChat.Size = new Size(1063, 432);
      tabChat.TabIndex = 3;
      tabChat.Text = "Chat";
      //
      // chatLayout
      //
      chatLayout.ColumnCount = 4;
      chatLayout.ColumnStyles.Add(new ColumnStyle());
      chatLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      chatLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
      chatLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
      chatLayout.Controls.Add(lblChatModel, 0, 0);
      chatLayout.Controls.Add(cboChatModel, 1, 0);
      chatLayout.Controls.Add(btnChatRefresh, 2, 0);
      chatLayout.Controls.Add(btnChatNew, 3, 0);
      chatLayout.Controls.Add(txtChatTranscript, 0, 1);
      chatLayout.Controls.Add(txtChatInput, 0, 2);
      chatLayout.Controls.Add(btnChatAttach, 2, 2);
      chatLayout.Controls.Add(btnChatSend, 3, 2);
      chatLayout.Controls.Add(lblChatImages, 0, 3);
      chatLayout.Controls.Add(chkChatTools, 2, 3);
      chatLayout.Controls.Add(lblChatFolder, 0, 4);
      chatLayout.Controls.Add(btnChatFolder, 2, 4);
      chatLayout.Controls.Add(btnChatFolderClear, 3, 4);
      chatLayout.Controls.Add(chkChatRunCommands, 0, 5);
      chatLayout.Dock = DockStyle.Fill;
      chatLayout.Location = new Point(8, 8);
      chatLayout.Name = "chatLayout";
      chatLayout.RowCount = 6;
      chatLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
      chatLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      chatLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
      chatLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
      chatLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
      chatLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
      chatLayout.Size = new Size(1047, 416);
      chatLayout.TabIndex = 0;
      //
      // lblChatModel
      //
      lblChatModel.AutoSize = true;
      lblChatModel.Dock = DockStyle.Fill;
      lblChatModel.Location = new Point(3, 0);
      lblChatModel.Name = "lblChatModel";
      lblChatModel.Size = new Size(50, 38);
      lblChatModel.TabIndex = 0;
      lblChatModel.Text = "Model:";
      lblChatModel.TextAlign = ContentAlignment.MiddleRight;
      //
      // cboChatModel
      //
      cboChatModel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
      cboChatModel.DropDownStyle = ComboBoxStyle.DropDownList;
      cboChatModel.Location = new Point(59, 6);
      cboChatModel.Name = "cboChatModel";
      cboChatModel.Size = new Size(725, 25);
      cboChatModel.TabIndex = 1;
      //
      // btnChatRefresh
      //
      btnChatRefresh.Dock = DockStyle.Fill;
      btnChatRefresh.Location = new Point(790, 3);
      btnChatRefresh.Name = "btnChatRefresh";
      btnChatRefresh.Size = new Size(124, 32);
      btnChatRefresh.TabIndex = 2;
      btnChatRefresh.Text = "Refresh";
      btnChatRefresh.Click += btnChatRefresh_Click;
      //
      // btnChatNew
      //
      btnChatNew.Dock = DockStyle.Fill;
      btnChatNew.Location = new Point(920, 3);
      btnChatNew.Name = "btnChatNew";
      btnChatNew.Size = new Size(124, 32);
      btnChatNew.TabIndex = 3;
      btnChatNew.Text = "New Chat";
      btnChatNew.Click += btnChatNew_Click;
      //
      // txtChatTranscript
      //
      chatLayout.SetColumnSpan(txtChatTranscript, 4);
      txtChatTranscript.BackColor = SystemColors.Window;
      txtChatTranscript.Dock = DockStyle.Fill;
      txtChatTranscript.Font = new Font("Segoe UI", 10F);
      txtChatTranscript.Location = new Point(3, 41);
      txtChatTranscript.Name = "txtChatTranscript";
      txtChatTranscript.ReadOnly = true;
      txtChatTranscript.Size = new Size(1041, 272);
      txtChatTranscript.TabIndex = 4;
      txtChatTranscript.Text = "";
      //
      // txtChatInput
      //
      chatLayout.SetColumnSpan(txtChatInput, 2);
      txtChatInput.AcceptsReturn = true;
      txtChatInput.Dock = DockStyle.Fill;
      txtChatInput.Location = new Point(3, 319);
      txtChatInput.Multiline = true;
      txtChatInput.Name = "txtChatInput";
      txtChatInput.PlaceholderText = "Type a message. Enter sends; Shift+Enter starts a new line.";
      txtChatInput.ScrollBars = ScrollBars.Vertical;
      txtChatInput.Size = new Size(781, 70);
      txtChatInput.TabIndex = 5;
      txtChatInput.KeyDown += txtChatInput_KeyDown;
      //
      // btnChatAttach
      //
      btnChatAttach.Dock = DockStyle.Fill;
      btnChatAttach.Location = new Point(790, 319);
      btnChatAttach.Name = "btnChatAttach";
      btnChatAttach.Size = new Size(124, 70);
      btnChatAttach.TabIndex = 6;
      btnChatAttach.Text = "Add Images";
      btnChatAttach.Click += btnChatAttach_Click;
      //
      // btnChatSend
      //
      btnChatSend.Dock = DockStyle.Fill;
      btnChatSend.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
      btnChatSend.Location = new Point(920, 319);
      btnChatSend.Name = "btnChatSend";
      btnChatSend.Size = new Size(124, 70);
      btnChatSend.TabIndex = 7;
      btnChatSend.Text = "Send Message";
      btnChatSend.Click += btnChatSend_Click;
      //
      // lblChatImages
      //
      lblChatImages.AutoEllipsis = true;
      chatLayout.SetColumnSpan(lblChatImages, 2);
      lblChatImages.Dock = DockStyle.Fill;
      lblChatImages.ForeColor = SystemColors.GrayText;
      lblChatImages.Location = new Point(3, 392);
      lblChatImages.Name = "lblChatImages";
      lblChatImages.Size = new Size(1041, 24);
      lblChatImages.TabIndex = 8;
      lblChatImages.Text = "No images attached.";
      //
      // chkChatTools
      //
      chkChatTools.AutoSize = true;
      chkChatTools.Checked = true;
      chkChatTools.CheckState = CheckState.Checked;
      chatLayout.SetColumnSpan(chkChatTools, 2);
      chkChatTools.Dock = DockStyle.Fill;
      chkChatTools.Location = new Point(790, 392);
      chkChatTools.Name = "chkChatTools";
      chkChatTools.Size = new Size(254, 24);
      chkChatTools.TabIndex = 9;
      chkChatTools.Text = "Let the model use tools";
      chkChatTools.UseVisualStyleBackColor = true;
      //
      // lblChatFolder
      //
      lblChatFolder.AutoEllipsis = true;
      chatLayout.SetColumnSpan(lblChatFolder, 2);
      lblChatFolder.Dock = DockStyle.Fill;
      lblChatFolder.Location = new Point(3, 416);
      lblChatFolder.Name = "lblChatFolder";
      lblChatFolder.Size = new Size(781, 36);
      lblChatFolder.TabIndex = 10;
      lblChatFolder.Text = "Working folder: none (the model cannot use your files)";
      lblChatFolder.TextAlign = ContentAlignment.MiddleLeft;
      //
      // btnChatFolder
      //
      btnChatFolder.Dock = DockStyle.Fill;
      btnChatFolder.Location = new Point(790, 419);
      btnChatFolder.Name = "btnChatFolder";
      btnChatFolder.Size = new Size(124, 30);
      btnChatFolder.TabIndex = 11;
      btnChatFolder.Text = "Working Folder...";
      btnChatFolder.Click += btnChatFolder_Click;
      //
      // btnChatFolderClear
      //
      btnChatFolderClear.Dock = DockStyle.Fill;
      btnChatFolderClear.Location = new Point(920, 419);
      btnChatFolderClear.Name = "btnChatFolderClear";
      btnChatFolderClear.Size = new Size(124, 30);
      btnChatFolderClear.TabIndex = 12;
      btnChatFolderClear.Text = "No Folder";
      btnChatFolderClear.Click += btnChatFolderClear_Click;
      //
      // chkChatRunCommands
      //
      chkChatRunCommands.AutoSize = true;
      chatLayout.SetColumnSpan(chkChatRunCommands, 4);
      chkChatRunCommands.Dock = DockStyle.Fill;
      chkChatRunCommands.Location = new Point(3, 455);
      chkChatRunCommands.Name = "chkChatRunCommands";
      chkChatRunCommands.Size = new Size(1041, 20);
      chkChatRunCommands.TabIndex = 13;
      chkChatRunCommands.Text = "Run commands without asking (commands can change anything on this PC, not only the working folder)";
      chkChatRunCommands.UseVisualStyleBackColor = true;
      // 
      // tabDownloads
      // 
      tabDownloads.Controls.Add(modelsScroll);
      tabDownloads.Location = new Point(4, 32);
      tabDownloads.Name = "tabDownloads";
      tabDownloads.Padding = new Padding(3);
      tabDownloads.Size = new Size(1063, 301);
      tabDownloads.TabIndex = 1;
      tabDownloads.Text = "Downloads";
      // 
      // modelsScroll
      // 
      modelsScroll.AutoScroll = true;
      modelsScroll.Controls.Add(modelsLayout);
      modelsScroll.Dock = DockStyle.Fill;
      modelsScroll.Location = new Point(3, 3);
      modelsScroll.Name = "modelsScroll";
      modelsScroll.Size = new Size(1057, 295);
      modelsScroll.TabIndex = 0;
      // 
      // modelsLayout
      // 
      modelsLayout.Controls.Add(lblDownloadUrl);
      modelsLayout.Controls.Add(txtDownloadUrl);
      modelsLayout.Controls.Add(btnFindDownloadAddress);
      modelsLayout.Controls.Add(btnDownload);
      modelsLayout.Controls.Add(btnPullFromDownloads);
      modelsLayout.Controls.Add(progressDownload);
      modelsLayout.Controls.Add(lblDownloadStatus);
      modelsLayout.Location = new Point(0, 3);
      modelsLayout.Name = "modelsLayout";
      modelsLayout.Padding = new Padding(10);
      modelsLayout.Size = new Size(1043, 156);
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
      // 
      // btnFindDownloadAddress
      // 
      btnFindDownloadAddress.Location = new Point(705, 21);
      btnFindDownloadAddress.Name = "btnFindDownloadAddress";
      btnFindDownloadAddress.Size = new Size(105, 34);
      btnFindDownloadAddress.TabIndex = 14;
      btnFindDownloadAddress.Text = "Find";
      btnFindDownloadAddress.Click += btnFindDownloadAddress_Click;
      // 
      // btnDownload
      // 
      btnDownload.Location = new Point(820, 21);
      btnDownload.Name = "btnDownload";
      btnDownload.Size = new Size(105, 34);
      btnDownload.TabIndex = 13;
      btnDownload.Text = "Download";
      btnDownload.Click += btnDownload_Click;
      //
      // btnPullFromDownloads
      //
      btnPullFromDownloads.Location = new Point(705, 66);
      btnPullFromDownloads.Name = "btnPullFromDownloads";
      btnPullFromDownloads.Size = new Size(220, 34);
      btnPullFromDownloads.TabIndex = 15;
      btnPullFromDownloads.Text = "Convert with OVMS Pull";
      btnPullFromDownloads.Click += btnPullFromDownloads_Click;
      // 
      // progressDownload
      // 
      progressDownload.Location = new Point(20, 78);
      progressDownload.Margin = new Padding(0, 7, 10, 0);
      progressDownload.Name = "progressDownload";
      progressDownload.Size = new Size(420, 18);
      progressDownload.TabIndex = 0;
      // 
      // lblDownloadStatus
      // 
      lblDownloadStatus.AutoEllipsis = true;
      lblDownloadStatus.Location = new Point(20, 107);
      lblDownloadStatus.Name = "lblDownloadStatus";
      lblDownloadStatus.Size = new Size(560, 28);
      lblDownloadStatus.TabIndex = 31;
      lblDownloadStatus.Text = "Paste a model or file address to begin.";
      lblDownloadStatus.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // tabOvmsPull
      // 
      tabOvmsPull.Controls.Add(ovmsPullLayout);
      tabOvmsPull.Location = new Point(4, 32);
      tabOvmsPull.Name = "tabOvmsPull";
      tabOvmsPull.Padding = new Padding(12);
      tabOvmsPull.Size = new Size(1063, 301);
      tabOvmsPull.TabIndex = 2;
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
      ovmsPullLayout.Location = new Point(12, 12);
      ovmsPullLayout.Name = "ovmsPullLayout";
      ovmsPullLayout.Padding = new Padding(10);
      ovmsPullLayout.Size = new Size(1039, 277);
      ovmsPullLayout.TabIndex = 0;
      // 
      // lblOvmsPullHeading
      // 
      lblOvmsPullHeading.AutoSize = true;
      lblOvmsPullHeading.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
      lblOvmsPullHeading.Location = new Point(20, 20);
      lblOvmsPullHeading.Name = "lblOvmsPullHeading";
      lblOvmsPullHeading.Size = new Size(126, 19);
      lblOvmsPullHeading.TabIndex = 0;
      lblOvmsPullHeading.Text = "OVMS model pull";
      // 
      // lblOvmsExecutable
      // 
      lblOvmsExecutable.Location = new Point(20, 62);
      lblOvmsExecutable.Name = "lblOvmsExecutable";
      lblOvmsExecutable.Size = new Size(120, 25);
      lblOvmsExecutable.TabIndex = 1;
      lblOvmsExecutable.Text = "OVMS program";
      // 
      // txtOvmsExecutable
      // 
      txtOvmsExecutable.Location = new Point(150, 60);
      txtOvmsExecutable.Name = "txtOvmsExecutable";
      txtOvmsExecutable.Size = new Size(360, 25);
      txtOvmsExecutable.TabIndex = 2;
      // 
      // lblOvmsPythonPath
      // 
      lblOvmsPythonPath.Location = new Point(530, 62);
      lblOvmsPythonPath.Name = "lblOvmsPythonPath";
      lblOvmsPythonPath.Size = new Size(120, 25);
      lblOvmsPythonPath.TabIndex = 3;
      lblOvmsPythonPath.Text = "Python DLL folder";
      // 
      // txtOvmsPythonPath
      // 
      txtOvmsPythonPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtOvmsPythonPath.Location = new Point(660, 60);
      txtOvmsPythonPath.Name = "txtOvmsPythonPath";
      txtOvmsPythonPath.Size = new Size(168, 25);
      txtOvmsPythonPath.TabIndex = 4;
      // 
      // lblOvmsExportToolsPath
      // 
      lblOvmsExportToolsPath.Location = new Point(20, 97);
      lblOvmsExportToolsPath.Name = "lblOvmsExportToolsPath";
      lblOvmsExportToolsPath.Size = new Size(120, 25);
      lblOvmsExportToolsPath.TabIndex = 5;
      lblOvmsExportToolsPath.Text = "Export tools folder";
      // 
      // txtOvmsExportToolsPath
      // 
      txtOvmsExportToolsPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtOvmsExportToolsPath.Location = new Point(150, 95);
      txtOvmsExportToolsPath.Name = "txtOvmsExportToolsPath";
      txtOvmsExportToolsPath.Size = new Size(678, 25);
      txtOvmsExportToolsPath.TabIndex = 6;
      // 
      // lblOvmsSourceFormat
      // 
      lblOvmsSourceFormat.Location = new Point(20, 137);
      lblOvmsSourceFormat.Name = "lblOvmsSourceFormat";
      lblOvmsSourceFormat.Size = new Size(120, 25);
      lblOvmsSourceFormat.TabIndex = 7;
      lblOvmsSourceFormat.Text = "Source type";
      // 
      // cboOvmsSourceFormat
      // 
      cboOvmsSourceFormat.DropDownStyle = ComboBoxStyle.DropDownList;
      cboOvmsSourceFormat.Location = new Point(150, 135);
      cboOvmsSourceFormat.Name = "cboOvmsSourceFormat";
      cboOvmsSourceFormat.Size = new Size(145, 25);
      cboOvmsSourceFormat.TabIndex = 8;
      // 
      // lblOvmsTask
      // 
      lblOvmsTask.Location = new Point(315, 137);
      lblOvmsTask.Name = "lblOvmsTask";
      lblOvmsTask.Size = new Size(55, 25);
      lblOvmsTask.TabIndex = 9;
      lblOvmsTask.Text = "Task";
      // 
      // cboOvmsTask
      // 
      cboOvmsTask.Location = new Point(370, 135);
      cboOvmsTask.Name = "cboOvmsTask";
      cboOvmsTask.Size = new Size(185, 25);
      cboOvmsTask.TabIndex = 10;
      // 
      // lblOvmsPipelineType
      // 
      lblOvmsPipelineType.Location = new Point(575, 137);
      lblOvmsPipelineType.Name = "lblOvmsPipelineType";
      lblOvmsPipelineType.Size = new Size(70, 25);
      lblOvmsPipelineType.TabIndex = 11;
      lblOvmsPipelineType.Text = "Pipeline";
      // 
      // cboOvmsPipelineType
      // 
      cboOvmsPipelineType.Location = new Point(650, 135);
      cboOvmsPipelineType.Name = "cboOvmsPipelineType";
      cboOvmsPipelineType.Size = new Size(85, 25);
      cboOvmsPipelineType.TabIndex = 12;
      // 
      // lblOvmsWeightFormat
      // 
      lblOvmsWeightFormat.Location = new Point(755, 137);
      lblOvmsWeightFormat.Name = "lblOvmsWeightFormat";
      lblOvmsWeightFormat.Size = new Size(70, 25);
      lblOvmsWeightFormat.TabIndex = 13;
      lblOvmsWeightFormat.Text = "Weights";
      // 
      // cboOvmsWeightFormat
      // 
      cboOvmsWeightFormat.Location = new Point(835, 135);
      cboOvmsWeightFormat.Name = "cboOvmsWeightFormat";
      cboOvmsWeightFormat.Size = new Size(120, 25);
      cboOvmsWeightFormat.TabIndex = 14;
      // 
      // lblOvmsSourceModel
      // 
      lblOvmsSourceModel.Location = new Point(20, 177);
      lblOvmsSourceModel.Name = "lblOvmsSourceModel";
      lblOvmsSourceModel.Size = new Size(120, 25);
      lblOvmsSourceModel.TabIndex = 15;
      lblOvmsSourceModel.Text = "Source model";
      // 
      // cboOvmsSourceModel
      // 
      cboOvmsSourceModel.Location = new Point(150, 175);
      cboOvmsSourceModel.Name = "cboOvmsSourceModel";
      cboOvmsSourceModel.Size = new Size(360, 25);
      cboOvmsSourceModel.TabIndex = 16;
      cboOvmsSourceModel.SelectedIndexChanged += cboOvmsSourceModel_SelectedIndexChanged;
      cboOvmsSourceModel.TextChanged += cboOvmsSourceModel_TextChanged;
      // 
      // lblOvmsGgufFilename
      // 
      lblOvmsGgufFilename.Location = new Point(20, 217);
      lblOvmsGgufFilename.Name = "lblOvmsGgufFilename";
      lblOvmsGgufFilename.Size = new Size(120, 25);
      lblOvmsGgufFilename.TabIndex = 17;
      lblOvmsGgufFilename.Text = "GGUF filename";
      // 
      // txtOvmsGgufFilename
      // 
      txtOvmsGgufFilename.Location = new Point(150, 215);
      txtOvmsGgufFilename.Name = "txtOvmsGgufFilename";
      txtOvmsGgufFilename.PlaceholderText = "Qwen3.6-27B-UD-Q8_K_XL.gguf";
      txtOvmsGgufFilename.Size = new Size(360, 25);
      txtOvmsGgufFilename.TabIndex = 18;
      // 
      // lblOvmsModelRepository
      // 
      lblOvmsModelRepository.Location = new Point(530, 177);
      lblOvmsModelRepository.Name = "lblOvmsModelRepository";
      lblOvmsModelRepository.Size = new Size(120, 25);
      lblOvmsModelRepository.TabIndex = 19;
      lblOvmsModelRepository.Text = "Model folder";
      // 
      // txtOvmsModelRepository
      // 
      txtOvmsModelRepository.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtOvmsModelRepository.Location = new Point(660, 175);
      txtOvmsModelRepository.Name = "txtOvmsModelRepository";
      txtOvmsModelRepository.Size = new Size(168, 25);
      txtOvmsModelRepository.TabIndex = 20;
      // 
      // lblOvmsLogPath
      // 
      lblOvmsLogPath.Location = new Point(530, 217);
      lblOvmsLogPath.Name = "lblOvmsLogPath";
      lblOvmsLogPath.Size = new Size(120, 25);
      lblOvmsLogPath.TabIndex = 21;
      lblOvmsLogPath.Text = "Log file";
      // 
      // txtOvmsLogPath
      // 
      txtOvmsLogPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      txtOvmsLogPath.Location = new Point(660, 215);
      txtOvmsLogPath.Name = "txtOvmsLogPath";
      txtOvmsLogPath.Size = new Size(168, 25);
      txtOvmsLogPath.TabIndex = 22;
      // 
      // chkOvmsOverwrite
      // 
      chkOvmsOverwrite.Checked = true;
      chkOvmsOverwrite.CheckState = CheckState.Checked;
      chkOvmsOverwrite.Location = new Point(20, 255);
      chkOvmsOverwrite.Name = "chkOvmsOverwrite";
      chkOvmsOverwrite.Size = new Size(165, 25);
      chkOvmsOverwrite.TabIndex = 23;
      chkOvmsOverwrite.Text = "Replace existing files";
      // 
      // chkOvmsDebug
      // 
      chkOvmsDebug.Checked = true;
      chkOvmsDebug.CheckState = CheckState.Checked;
      chkOvmsDebug.Location = new Point(195, 255);
      chkOvmsDebug.Name = "chkOvmsDebug";
      chkOvmsDebug.Size = new Size(110, 25);
      chkOvmsDebug.TabIndex = 24;
      chkOvmsDebug.Text = "Debug log";
      // 
      // btnCheckOvmsVersions
      // 
      btnCheckOvmsVersions.Location = new Point(320, 252);
      btnCheckOvmsVersions.Name = "btnCheckOvmsVersions";
      btnCheckOvmsVersions.Size = new Size(120, 34);
      btnCheckOvmsVersions.TabIndex = 25;
      btnCheckOvmsVersions.Text = "Check Stack";
      btnCheckOvmsVersions.Click += btnCheckOvmsVersions_Click;
      // 
      // btnPreviewOvmsPull
      // 
      btnPreviewOvmsPull.Location = new Point(450, 252);
      btnPreviewOvmsPull.Name = "btnPreviewOvmsPull";
      btnPreviewOvmsPull.Size = new Size(120, 34);
      btnPreviewOvmsPull.TabIndex = 26;
      btnPreviewOvmsPull.Text = "Preview";
      btnPreviewOvmsPull.Click += btnPreviewOvmsPull_Click;
      // 
      // btnRunOvmsPull
      // 
      btnRunOvmsPull.Location = new Point(580, 252);
      btnRunOvmsPull.Name = "btnRunOvmsPull";
      btnRunOvmsPull.Size = new Size(120, 34);
      btnRunOvmsPull.TabIndex = 27;
      btnRunOvmsPull.Text = "Start Pull";
      btnRunOvmsPull.Click += btnRunOvmsPull_Click;
      // 
      // lblOvmsPullStatus
      // 
      lblOvmsPullStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      lblOvmsPullStatus.AutoEllipsis = true;
      lblOvmsPullStatus.ForeColor = SystemColors.GrayText;
      lblOvmsPullStatus.Location = new Point(720, 253);
      lblOvmsPullStatus.Name = "lblOvmsPullStatus";
      lblOvmsPullStatus.Size = new Size(108, 34);
      lblOvmsPullStatus.TabIndex = 28;
      lblOvmsPullStatus.Text = "Ready";
      lblOvmsPullStatus.TextAlign = ContentAlignment.MiddleLeft;
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
      outputLayout.Size = new Size(1071, 418);
      outputLayout.TabIndex = 0;
      // 
      // grpOutput
      // 
      grpOutput.Controls.Add(outputContentLayout);
      grpOutput.Dock = DockStyle.Fill;
      grpOutput.Location = new Point(3, 3);
      grpOutput.Name = "grpOutput";
      grpOutput.Size = new Size(1065, 354);
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
      outputContentLayout.Size = new Size(1059, 332);
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
      txtOutput.Location = new Point(3, 27);
      txtOutput.Name = "txtOutput";
      txtOutput.ReadOnly = true;
      txtOutput.Size = new Size(1053, 302);
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
      outputButtons.Location = new Point(3, 363);
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
      // 
      // lblActivityStatus
      // 
      lblActivityStatus.AutoEllipsis = true;
      lblActivityStatus.Dock = DockStyle.Fill;
      lblActivityStatus.Location = new Point(3, 0);
      lblActivityStatus.Name = "lblActivityStatus";
      lblActivityStatus.Size = new Size(690, 20);
      lblActivityStatus.TabIndex = 0;
      lblActivityStatus.Text = "Ready";
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
      // Form1
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(1103, 853);
      Controls.Add(mainLayout);
      Font = new Font("Segoe UI", 9F);
      MinimumSize = new Size(568, 300);
      Name = "Form1";
      StartPosition = FormStartPosition.CenterScreen;
      Text = "OpenVINO Workbench";
      FormClosing += Form1_FormClosing;
      Shown += Form1_Shown;
      mainLayout.ResumeLayout(false);
      mainLayout.PerformLayout();
      mainSplit.Panel1.ResumeLayout(false);
      mainSplit.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
      mainSplit.ResumeLayout(false);
      tabMain.ResumeLayout(false);
      tabWorkstation.ResumeLayout(false);
      tabWorkstation.PerformLayout();
      workstationLayout.ResumeLayout(false);
      workstationLayout.PerformLayout();
      modelRootLayout.ResumeLayout(false);
      modelRootLayout.PerformLayout();
      splitContainer1.Panel1.ResumeLayout(false);
      splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
      splitContainer1.ResumeLayout(false);
      tabChat.ResumeLayout(false);
      chatLayout.ResumeLayout(false);
      chatLayout.PerformLayout();
      tabDownloads.ResumeLayout(false);
      modelsScroll.ResumeLayout(false);
      modelsLayout.ResumeLayout(false);
      modelsLayout.PerformLayout();
      tabOvmsPull.ResumeLayout(false);
      ovmsPullLayout.ResumeLayout(false);
      ovmsPullLayout.PerformLayout();
      outputLayout.ResumeLayout(false);
      grpOutput.ResumeLayout(false);
      outputContentLayout.ResumeLayout(false);
      outputContentLayout.PerformLayout();
      outputButtons.ResumeLayout(false);
      activityLayout.ResumeLayout(false);
      ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel mainLayout;
        private Label lblTitle;
        private Label lblSubtitle;
        private SplitContainer mainSplit;
        private TabControl tabMain;
        private TabPage tabWorkstation;
        private TableLayoutPanel workstationLayout;
        private Label lblWorkstationHeading;
        private TableLayoutPanel modelRootLayout;
        private Label lblModelRoot;
        private TextBox txtModelRoot;
        private Button btnBrowseModelRoot;
        private Button btnCheckWorkstation;
        private Label lblWorkstationTarget;
        private ComboBox cboWorkstationTarget;
        private Label lblWorkstationMultiTarget;
        private ComboBox cboWorkstationMultiTarget;
        private Label lblModelCapabilities;
        private TextBox txtModelCapabilities;
        private Label lblInferModel;
        private TextBox txtInferModel;
        private Button btnBrowseInferModel;
        private Label lblModelSize;
        private TabPage tabChat;
        private TableLayoutPanel chatLayout;
        private Label lblChatModel;
        private ComboBox cboChatModel;
        private Button btnChatRefresh;
        private Button btnChatNew;
        private RichTextBox txtChatTranscript;
        private TextBox txtChatInput;
        private Button btnChatAttach;
        private Button btnChatSend;
        private Label lblChatImages;
        private CheckBox chkChatTools;
        private Label lblChatFolder;
        private Button btnChatFolder;
        private Button btnChatFolderClear;
        private CheckBox chkChatRunCommands;
        private TabPage tabDownloads;
        private Panel modelsScroll;
        private Panel modelsLayout;
        private Button btnDownload;
        private Button btnPullFromDownloads;
        private Button btnFindDownloadAddress;
        private Label lblDownloadUrl;
        private TextBox txtDownloadUrl;
        private ProgressBar progressDownload;
        private Label lblDownloadStatus;
        private TabPage tabOvmsPull;
        private Panel ovmsPullLayout;
        private Label lblOvmsPullHeading;
        private Label lblOvmsExecutable;
        private TextBox txtOvmsExecutable;
        private Label lblOvmsPythonPath;
        private TextBox txtOvmsPythonPath;
        private Label lblOvmsExportToolsPath;
        private TextBox txtOvmsExportToolsPath;
        private Label lblOvmsSourceFormat;
        private ComboBox cboOvmsSourceFormat;
        private Label lblOvmsTask;
        private ComboBox cboOvmsTask;
        private Label lblOvmsPipelineType;
        private ComboBox cboOvmsPipelineType;
        private Label lblOvmsWeightFormat;
        private ComboBox cboOvmsWeightFormat;
        private Label lblOvmsSourceModel;
        private ComboBox cboOvmsSourceModel;
        private Label lblOvmsGgufFilename;
        private TextBox txtOvmsGgufFilename;
        private Label lblOvmsModelRepository;
        private TextBox txtOvmsModelRepository;
        private Label lblOvmsLogPath;
        private TextBox txtOvmsLogPath;
        private CheckBox chkOvmsOverwrite;
        private CheckBox chkOvmsDebug;
        private Button btnCheckOvmsVersions;
        private Button btnPreviewOvmsPull;
        private Button btnRunOvmsPull;
        private Label lblOvmsPullStatus;
        private Label lblServerAddress;
        private TextBox txtServerAddress;
        private Button btnConnectServer;
        private Label lblServerStatus;
        private TextBox txtServerStatus;
        private Button btnUnloadModel;
        private TableLayoutPanel outputLayout;
        private GroupBox grpOutput;
        private TableLayoutPanel outputContentLayout;
        private Label lblOutputInfo;
        private RichTextBox txtOutput;
        private TableLayoutPanel outputButtons;
        private Button btnClearOutput;
        private TableLayoutPanel activityLayout;
        private Label lblActivityStatus;
        private ProgressBar progressActivity;
        private Button btnCancelActivity;
        private Button btnHelp;
        private Button btnLoadModel;
        private SplitContainer splitContainer1;
    }
}
