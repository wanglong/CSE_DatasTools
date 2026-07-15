using CsvHelper.Configuration.Attributes;

namespace CSE_DatasTools.Models
{
    public class EcgRecord
    {
        [Name("导联")]
        public string Lead { get; set; } = string.Empty;

        [Name("P1振幅")]
        public double P1Amplitude { get; set; }

        [Name("Q振幅")]
        public double QAmplitude { get; set; }

        [Name("R振幅")]
        public double RAmplitude { get; set; }

        [Name("S振幅")]
        public double SAmplitude { get; set; }

        [Name("ST20振幅")]
        public double ST20Amplitude { get; set; }

        [Name("ST40振幅")]
        public double ST40Amplitude { get; set; }

        [Name("ST60振幅")]
        public double ST60Amplitude { get; set; }

        [Name("ST80振幅")]
        public double ST80Amplitude { get; set; }

        [Name("T振幅")]
        public double TAmplitude { get; set; }
    }
}