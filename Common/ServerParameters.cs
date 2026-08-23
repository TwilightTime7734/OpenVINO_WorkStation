namespace WorkStationC_.Common;

internal sealed class ServerParameters
{
    public string Host { get; set; } = "localhost";

    public string BindAddress { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 11434;

    public int ContextWindow { get; set; } = 32768;

    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Health { get; set; } = "/health";

    public string Models { get; set; } = "/v1/models";

    public string ChatCompletions { get; set; } = "/v1/chat/completions";

    public string Completions { get; set; } = "/v1/completions";

    public string ImageGenerations { get; set; } = "/v1/images/generations";

    public string ImageEdits { get; set; } = "/v1/images/edits";
}





//namespace WorkStationC_.Common;

//internal class ServerParameters
//{
//    // Boolean Parameters (Defaults set to safe fallbacks)
//    public bool OpenAICompatible { get; set; } = true;
//    public bool ServerToolUse { get; set; } = false;

//    // Integer Parameters
//    public int ContextWindow { get; set; } = 4096;

//    // String Parameters
//    public string baseUrl { get; set; } = "http://localhost:11434";
//    public string health { get; set; } = "v1/health";
//    public string models { get; set; } = "v1/models";
//    public string chat_completions { get; set; } = "v1/chat/completions";
//    public string completions { get; set; } = "v1/completions";
//    public string image_generations { get; set; } = "v1/images/generations";
//    public string image_edits { get; set; } = "v1/images/edits";

//    public string responses { get; set; } = ""; //"v1/responses";
//    public string embeddings { get; set; } = ""; //"v1/embeddings";
//    public string audio_transcriptions { get; set; } = ""; //"v1/audio/transcriptions";
//    public string audio_translations { get; set; } = ""; //"v1/audio/translations";
//    public string audio_speech { get; set; } = ""; //"v1/audio/speech";

//    public string SystemPrompt { get; set; } = string.Empty;
//    public string TargetDevice { get; set; } = "CPU";
//    public string LogLevel { get; set; } = "INFO"; // Options: "INFO", "DEBUG", "ERROR"
//}
