namespace WorkStationC_
{
    partial class ModelPickerForm
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
            pickerLayout = new TableLayoutPanel();
            lblModelPicker = new Label();
            lstModels = new ListBox();
            pickerButtons = new FlowLayoutPanel();
            btnPickerCancel = new Button();
            btnPickerSelect = new Button();
            pickerLayout.SuspendLayout();
            pickerButtons.SuspendLayout();
            SuspendLayout();
            //
            // pickerLayout
            //
            pickerLayout.ColumnCount = 1;
            pickerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pickerLayout.Controls.Add(lblModelPicker, 0, 0);
            pickerLayout.Controls.Add(lstModels, 0, 1);
            pickerLayout.Controls.Add(pickerButtons, 0, 2);
            pickerLayout.Dock = DockStyle.Fill;
            pickerLayout.Location = new Point(0, 0);
            pickerLayout.Name = "pickerLayout";
            pickerLayout.Padding = new Padding(10);
            pickerLayout.RowCount = 3;
            pickerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            pickerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pickerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            pickerLayout.Size = new Size(484, 461);
            pickerLayout.TabIndex = 0;
            //
            // lblModelPicker
            //
            lblModelPicker.AutoSize = true;
            lblModelPicker.Dock = DockStyle.Fill;
            lblModelPicker.Location = new Point(13, 10);
            lblModelPicker.Name = "lblModelPicker";
            lblModelPicker.Size = new Size(458, 28);
            lblModelPicker.TabIndex = 0;
            lblModelPicker.Text = "Models:";
            lblModelPicker.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lstModels
            //
            lstModels.Dock = DockStyle.Fill;
            lstModels.Font = new Font("Segoe UI", 10F);
            lstModels.IntegralHeight = false;
            lstModels.Location = new Point(13, 41);
            lstModels.Name = "lstModels";
            lstModels.Size = new Size(458, 361);
            lstModels.TabIndex = 1;
            lstModels.DoubleClick += lstModels_DoubleClick;
            //
            // pickerButtons
            //
            pickerButtons.Controls.Add(btnPickerCancel);
            pickerButtons.Controls.Add(btnPickerSelect);
            pickerButtons.Dock = DockStyle.Fill;
            pickerButtons.FlowDirection = FlowDirection.RightToLeft;
            pickerButtons.Location = new Point(13, 408);
            pickerButtons.Name = "pickerButtons";
            pickerButtons.Size = new Size(458, 40);
            pickerButtons.TabIndex = 2;
            //
            // btnPickerCancel
            //
            btnPickerCancel.DialogResult = DialogResult.Cancel;
            btnPickerCancel.Location = new Point(355, 3);
            btnPickerCancel.Name = "btnPickerCancel";
            btnPickerCancel.Size = new Size(100, 34);
            btnPickerCancel.TabIndex = 1;
            btnPickerCancel.Text = "Cancel";
            //
            // btnPickerSelect
            //
            btnPickerSelect.DialogResult = DialogResult.OK;
            btnPickerSelect.Location = new Point(249, 3);
            btnPickerSelect.Name = "btnPickerSelect";
            btnPickerSelect.Size = new Size(100, 34);
            btnPickerSelect.TabIndex = 0;
            btnPickerSelect.Text = "Select";
            //
            // ModelPickerForm
            //
            AcceptButton = btnPickerSelect;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnPickerCancel;
            ClientSize = new Size(484, 461);
            Controls.Add(pickerLayout);
            Font = new Font("Segoe UI", 9F);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(360, 300);
            Name = "ModelPickerForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Choose a Model";
            pickerLayout.ResumeLayout(false);
            pickerLayout.PerformLayout();
            pickerButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel pickerLayout;
        private Label lblModelPicker;
        private ListBox lstModels;
        private FlowLayoutPanel pickerButtons;
        private Button btnPickerCancel;
        private Button btnPickerSelect;
    }
}
