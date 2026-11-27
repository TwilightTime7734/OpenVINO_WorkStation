namespace WorkStationC_;

internal enum ModelFileKind
{
    Unknown,
    OpenVinoGenAiDirectory,
    OpenVinoIr,
    OpenVinoWeights,
    OpenVinoCoreReadable,
    Gguf,
    PyTorchSource,
    SupportFile,
}

internal enum ModelRunPath
{
    Unknown,
    TextGeneration,
    VisionLanguage,
    TextToImage,
    ImageToImage,
    ImageClassification,
    ObjectDetection,
    SpeechRecognition,
    TextEmbedding,
    OpenVinoCore,
    Unsupported,
}
