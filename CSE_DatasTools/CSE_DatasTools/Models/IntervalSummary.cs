using CsvHelper.Configuration.Attributes;

namespace CSE_DatasTools.Models
{
    public class IntervalSummary
    {
        [Name("导联")]
        public string Lead { get; set; } = string.Empty;

        [Name("ECG")]
        public string MeasurementType { get; set; } = string.Empty;

        [Name("第一列")]
        public double File1Value { get; set; }

        [Name("第二列")]
        public double File2Value { get; set; }

        [Name("第三列")]
        public double File3Value { get; set; }

        [Name("第四列")]
        public double File4Value { get; set; }

        [Name("第五列")]
        public double File5Value { get; set; }

        [Name("平均值")]
        public double AverageValue { get; set; }

        [Name("标准值")]
        public double StandardValue { get; set; }
    }
}