using CsvHelper.Configuration.Attributes;

namespace CSE_DatasTools.Models
{
    /// <summary>
    /// 间期测量汇总模型 - 用于汇总Data2文件夹中各CSV文件的P宽、QRS宽、PR间期、QT间期
    /// </summary>
    public class IntervalMeasurementSummary
    {
        [Name("文件编号")]
        public string FileNumber { get; set; } = string.Empty;

        [Name("P波时限")]
        public double PWidth { get; set; }

        [Name("P波时限标准值")]
        public double PWidthStandValue { get; set; }

        [Name("P波时限误差值")]
        public double PWidthErrorValue { get; set; }

        [Name("PQ波间期")]
        public double PRInterval { get; set; }

        [Name("PQ波间期标准值")]
        public double PRIntervalStandValue { get; set; }

        [Name("PQ波间期误差值")]
        public double PRIntervalErrorValue { get; set; }

        [Name("QRS波间期")]
        public double QRSWidth { get; set; }

        [Name("QRS波间期标准值")]
        public double QRSWidthStandValue { get; set; }

        [Name("QRS波间期误差值")]
        public double QRSWidthErrorValue { get; set; }

        [Name("QT波间期")]
        public double QTInterval { get; set; }

        [Name("QT波间期标准值")]
        public double QTIntervalStandValue { get; set; }

        [Name("QT波间期误差值")]
        public double QTIntervalErrorValue { get; set; }
    }
}