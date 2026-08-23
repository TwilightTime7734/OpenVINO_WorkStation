namespace WorkStationC_.Common;

internal class ModelParameters
{
    public string Name { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty;
    public ModelTaskTypes TaskType { get; set; } = new ModelTaskTypes();
    public string ModelRepositoryPath { get; set; } = string.Empty;
    public string ConfigPath { get; set; } = string.Empty;
    public string BatchSize { get; set; } = "auto"; // "auto" or "1", "4", etc.
    public string WeightFormat { get; set; } = string.Empty;
    public double Ratio { get; set; } = 1.0;
    public string BackupPrecision { get; set; } = string.Empty;
    public string DataSet { get; set; } = string.Empty;
    public int NumSamples { get; set; } = 0;
    public string DestinationPath { get; set; } = string.Empty;
}

internal class ModelTaskTypes
{
    public bool text_generation { get; set; } = true;
    public bool image_generation { get; set; } = false;
    public bool text2speech { get; set; } = false;
    public bool speech2text { get; set; } = false;
    public bool text2image { get; set; } = false;
    public bool image2image { get; set; } = false;
    public bool image2text { get; set; } = false;
}

internal class ModelCapabilities
{
    public bool CodeGeneration { get; set; } = false;
    public bool ImageClassification { get; set; } = false;
    public bool ImageToImage { get; set; } = false;
    public bool ObjectDetection { get; set; } = false;
    public bool Rerank { get; set; } = false;
    public bool SpeechRecognition { get; set; } = false;
    public bool TextEmbedding { get; set; } = false;
    public bool TextGeneration { get; set; } = true;
    public bool TextToImage { get; set; } = false;
    public bool TextToSpeech { get; set; } = false;
    public List<string> ToolUse { get; set; } = new List<string>{"web_search", "code_execution", "file_search"};
    public bool VideoGeneration { get; set; } = false;
    public bool VisionLanguage { get; set; } = false;
}


internal class ServerParameters
{
    // Boolean Parameters (Defaults set to safe fallbacks)
    public bool OpenAICompatible { get; set; } = true;
    public bool ServerToolUse { get; set; } = false;

    // Integer Parameters
    public int ContextWindow { get; set; } = 4096;
    public int RestPort { get; set; } = 8000;
    public int GrpcPort { get; set; } = 9000;

    // String Parameters
    public string baseUrl { get; set; } = "http://localhost:11434";
    public string responses { get; set; } = "v1/responses";
    public string chat_completions { get; set; } = "v1/chat/completions";
    public string completions { get; set; } = "v1/completions";
    public string models { get; set; } = "v1/models";
    public string embeddings { get; set; } = "v1/embeddings";
    public string image_generations { get; set; } = "v1/images/generations";
    public string image_edits { get; set; } = "v1/images/edits";
    public string audio_transcriptions { get; set; } = "v1/audio/transcriptions";
    public string audio_translations { get; set; } = "v1/audio/translations";
    public string audio_speech { get; set; } = "v1/audio/speech";

    public string SystemPrompt { get; set; } = string.Empty;
    public string TargetDevice { get; set; } = "CPU";
    public string LogLevel { get; set; } = "INFO"; // Options: "INFO", "DEBUG", "ERROR"
}
