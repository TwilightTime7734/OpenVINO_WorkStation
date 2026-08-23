namespace WorkStationC_.Common
{
    internal sealed class WorkbenchModel
    {
        public ModelParameters Parameters { get; set; } = new ModelParameters();
        public ModelCapabilities Capabilities { get; set; } = new ModelCapabilities();
        public ServerParameters ServerParams { get; set; } = new ServerParameters();
    }

}
