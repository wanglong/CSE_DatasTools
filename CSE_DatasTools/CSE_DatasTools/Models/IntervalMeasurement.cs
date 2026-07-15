namespace CSE_DatasTools.Models
{
    public class IntervalMeasurement
    {
        public string Lead { get; set; } = string.Empty;
        public double PWidth { get; set; }          // P波时限 (P width)
        public double QWidth { get; set; }          // Q波时限 (Q width)
        public double RWidth { get; set; }          // R波时限 (R width)
        public double SWidth { get; set; }          // S波时限 (S width)
        public double QRSWidth { get; set; }        // QRS波时限 (QRS width)
        public double PRInterval { get; set; }      // PR间期 (PR interval, same as PQ间期)
        public double QTInterval { get; set; }      // QT间期 (QT interval)
    }
}