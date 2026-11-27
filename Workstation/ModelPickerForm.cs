namespace WorkStationC_
{
    // Lists the model folders found in the main model folder: names ending in -ov, and models pulled with OVMS Pull.
    internal partial class ModelPickerForm : Form
    {
        public ModelPickerForm()
        {
            InitializeComponent();
        }

        public ModelPickerForm(string modelRoot, IEnumerable<string> modelNames, string currentModel) : this()
        {
            // The folder path is only known at runtime.
            lblModelPicker.Text = $"Models in {modelRoot}:";
            lstModels.Items.AddRange(modelNames.ToArray());
            int current = lstModels.Items.IndexOf(currentModel);
            if (lstModels.Items.Count > 0) lstModels.SelectedIndex = Math.Max(0, current);
        }

        public string SelectedModel => lstModels.SelectedItem as string ?? string.Empty;

        private void lstModels_DoubleClick(object? sender, EventArgs e)
        {
            if (lstModels.SelectedItem is null) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
