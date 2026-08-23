namespace WorkStationC_.Common
{
    internal class ModelCapabilities
    {
        public bool CodeGeneration { get; set; }
        public bool ImageClassification { get; set; }
        public bool ImageToImage { get; set; }
        public bool ObjectDetection { get; set; }
        public bool Rerank { get; set; }
        public bool SpeechRecognition { get; set; }
        public bool TextEmbedding { get; set; }
        public bool TextGeneration { get; set; }
        public bool TextToImage { get; set; }
        public bool TextToSpeech { get; set; }
        public List<string> ToolUse { get; set; } = new List<string> { };
        public bool VideoGeneration { get; set; }
        public bool VisionLanguage { get; set; }
    }
}
