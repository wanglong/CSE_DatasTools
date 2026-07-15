namespace CSE_DatasTools.Models
{
    public class EcgMeasurement
    {
        public string Lead { get; set; } = string.Empty;
        public double P1Amplitude { get; set; }
        public double QAmplitude { get; set; }
        public double RAmplitude { get; set; }
        public double SAmplitude { get; set; }
        public double ST20Amplitude { get; set; }
        public double ST40Amplitude { get; set; }
        public double ST60Amplitude { get; set; }
        public double ST80Amplitude { get; set; }
        public double TAmplitude { get; set; }
    }
}