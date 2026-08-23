namespace WorkStationC_.Common;

internal sealed class ModelParameters
{
    public string Name { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty;
    public string ModelRepositoryPath { get; set; } = string.Empty;
    public string ConfigPath { get; set; } = string.Empty;
    public string BatchSize { get; set; } = "auto"; // "auto" or "1", "4", etc.
    public string WeightFormat { get; set; } = string.Empty;
    public double Ratio { get; set; } = 1.0;
    public string BackupPrecision { get; set; } = string.Empty;
    public string DataSet { get; set; } = string.Empty;
    public int NumSamples { get; set; } = 0;
    public string DestinationPath { get; set; } = string.Empty;
    public ModelTaskType TaskType { get; set; } = ModelTaskType.Unknown;
}

internal enum ModelTaskType
{
    Unknown,
    TextGeneration,
    ImageGeneration,
    TextToSpeech,
    SpeechToText,
    TextToImage,
    ImageToImage,
    ImageToText
}
