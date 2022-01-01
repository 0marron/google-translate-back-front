namespace recorder_asp_net
{
    public class AudioModel
    {
        public string audiofile { get; set; }
        public string lang { get; set; }
    }
    public class TranslatedWord
    {
        public string orig { get; set; }
        public string trans { get; set; }
    }

}
