namespace WorkStationC_.Common;

internal class ServerParameters
{
    // Boolean Parameters (Defaults set to safe fallbacks)
    public bool OpenAICompatible { get; set; } = true;
    public bool ServerToolUse { get; set; } = false;

    // Integer Parameters
    public int ContextWindow { get; set; } = 4096;

    // String Parameters
    public string baseUrl { get; set; } = "http://localhost:11434";
    public string health { get; set; } = "v1/health";
    public string models { get; set; } = "v1/models";
    public string chat_completions { get; set; } = "v1/chat/completions";
    public string completions { get; set; } = "v1/completions";
    public string image_generations { get; set; } = "v1/images/generations";
    public string image_edits { get; set; } = "v1/images/edits";

    public string responses { get; set; } = ""; //"v1/responses";
    public string embeddings { get; set; } = ""; //"v1/embeddings";
    public string audio_transcriptions { get; set; } = ""; //"v1/audio/transcriptions";
    public string audio_translations { get; set; } = ""; //"v1/audio/translations";
    public string audio_speech { get; set; } = ""; //"v1/audio/speech";

    public string SystemPrompt { get; set; } = string.Empty;
    public string TargetDevice { get; set; } = "CPU";
    public string LogLevel { get; set; } = "INFO"; // Options: "INFO", "DEBUG", "ERROR"
}
