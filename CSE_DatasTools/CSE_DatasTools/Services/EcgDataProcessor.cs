using CSE_DatasTools.Models;

namespace CSE_DatasTools.Services
{
    public class EcgDataProcessor
    {
        private static readonly string[] TargetLeads = { "I", "II", "V1", "V2", "V3", "V4", "V5", "V6" };

        public List<AmplitudeSummary> ProcessFolder(string folderPath, EcgFileReader reader)
        {
            var csvFiles = Directory.GetFiles(folderPath, "*.csv")
                                    .OrderBy(f => f)
                                    .Take(5)
                                    .ToList();

            if (csvFiles.Count != 5)
            {
                throw new InvalidOperationException($"Folder {folderPath} must contain at least 5 CSV files");
            }

            var file1Measurements = ReadAndFilterMeasurements(csvFiles[0], reader);
            var file2Measurements = ReadAndFilterMeasurements(csvFiles[1], reader);
            var file3Measurements = ReadAndFilterMeasurements(csvFiles[2], reader);
            var file4Measurements = ReadAndFilterMeasurements(csvFiles[3], reader);
            var file5Measurements = ReadAndFilterMeasurements(csvFiles[4], reader);

            var summaries = new List<AmplitudeSummary>();

            foreach (var lead in TargetLeads)
            {
                var m1 = file1Measurements.FirstOrDefault(m => m.Lead == lead);
                var m2 = file2Measurements.FirstOrDefault(m => m.Lead == lead);
                var m3 = file3Measurements.FirstOrDefault(m => m.Lead == lead);
                var m4 = file4Measurements.FirstOrDefault(m => m.Lead == lead);
                var m5 = file5Measurements.FirstOrDefault(m => m.Lead == lead);

                if (m1 != null && m2 != null && m3 != null && m4 != null && m5 != null)
                {
                    // P1
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "P1",
                        File1Value = m1.P1Amplitude,
                        File2Value = m2.P1Amplitude,
                        File3Value = m3.P1Amplitude,
                        File4Value = m4.P1Amplitude,
                        File5Value = m5.P1Amplitude,
                        AverageValue = Math.Round((m1.P1Amplitude + m2.P1Amplitude + m3.P1Amplitude + m4.P1Amplitude + m5.P1Amplitude) / 5, 0),
                        StandardValue = 0
                    });

                    // Q
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "Q",
                        File1Value = m1.QAmplitude,
                        File2Value = m2.QAmplitude,
                        File3Value = m3.QAmplitude,
                        File4Value = m4.QAmplitude,
                        File5Value = m5.QAmplitude,
                        AverageValue = Math.Round((m1.QAmplitude + m2.QAmplitude + m3.QAmplitude + m4.QAmplitude + m5.QAmplitude) / 5, 0),
                        StandardValue = 0
                    });

                    // R
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "R",
                        File1Value = m1.RAmplitude,
                        File2Value = m2.RAmplitude,
                        File3Value = m3.RAmplitude,
                        File4Value = m4.RAmplitude,
                        File5Value = m5.RAmplitude,
                        AverageValue = Math.Round((m1.RAmplitude + m2.RAmplitude + m3.RAmplitude + m4.RAmplitude + m5.RAmplitude) / 5, 0),
                        StandardValue = 0
                    });

                    // S
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "S",
                        File1Value = m1.SAmplitude,
                        File2Value = m2.SAmplitude,
                        File3Value = m3.SAmplitude,
                        File4Value = m4.SAmplitude,
                        File5Value = m5.SAmplitude,
                        AverageValue = Math.Round((m1.SAmplitude + m2.SAmplitude + m3.SAmplitude + m4.SAmplitude + m5.SAmplitude) / 5, 0),
                        StandardValue = 0
                    });

                    // ST20
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "ST20",
                        File1Value = m1.ST20Amplitude,
                        File2Value = m2.ST20Amplitude,
                        File3Value = m3.ST20Amplitude,
                        File4Value = m4.ST20Amplitude,
                        File5Value = m5.ST20Amplitude,
                        AverageValue = Math.Round((m1.ST20Amplitude + m2.ST20Amplitude + m3.ST20Amplitude + m4.ST20Amplitude + m5.ST20Amplitude) / 5, 0),
                        StandardValue = 0
                    });

                    // ST40
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "ST40",
                        File1Value = m1.ST40Amplitude,
                        File2Value = m2.ST40Amplitude,
                        File3Value = m3.ST40Amplitude,
                        File4Value = m4.ST40Amplitude,
                        File5Value = m5.ST40Amplitude,
                        AverageValue = Math.Round((m1.ST40Amplitude + m2.ST40Amplitude + m3.ST40Amplitude + m4.ST40Amplitude + m5.ST40Amplitude) / 5, 0),
                        StandardValue = 0
                    });

                    // ST60
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "ST60",
                        File1Value = m1.ST60Amplitude,
                        File2Value = m2.ST60Amplitude,
                        File3Value = m3.ST60Amplitude,
                        File4Value = m4.ST60Amplitude,
                        File5Value = m5.ST60Amplitude,
                        AverageValue = Math.Round((m1.ST60Amplitude + m2.ST60Amplitude + m3.ST60Amplitude + m4.ST60Amplitude + m5.ST60Amplitude) / 5, 0),
                        StandardValue = 0
                    });

                    // ST80
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "ST80",
                        File1Value = m1.ST80Amplitude,
                        File2Value = m2.ST80Amplitude,
                        File3Value = m3.ST80Amplitude,
                        File4Value = m4.ST80Amplitude,
                        File5Value = m5.ST80Amplitude,
                        AverageValue = Math.Round((m1.ST80Amplitude + m2.ST80Amplitude + m3.ST80Amplitude + m4.ST80Amplitude + m5.ST80Amplitude) / 5, 0),
                        StandardValue = 0
                    });

                    // T
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "T",
                        File1Value = m1.TAmplitude,
                        File2Value = m2.TAmplitude,
                        File3Value = m3.TAmplitude,
                        File4Value = m4.TAmplitude,
                        File5Value = m5.TAmplitude,
                        AverageValue = Math.Round((m1.TAmplitude + m2.TAmplitude + m3.TAmplitude + m4.TAmplitude + m5.TAmplitude) / 5, 0),
                        StandardValue = 0
                    });
                }
            }

            return summaries;
        }

        private List<EcgMeasurement> ReadAndFilterMeasurements(string filePath, EcgFileReader reader)
        {
            var records = reader.ReadEcgFile(filePath);
            var measurements = reader.ExtractMeasurements(records);
            return measurements.Where(m => TargetLeads.Contains(m.Lead)).ToList();
        }

        public List<IntervalSummary> ProcessFolderForIntervals(string folderPath, EcgFileReader reader)
        {
            var csvFiles = Directory.GetFiles(folderPath, "*.csv")
                                    .OrderBy(f => f)
                                    .Take(5)
                                    .ToList();

            if (csvFiles.Count != 5)
            {
                throw new InvalidOperationException($"Folder {folderPath} must contain at least 5 CSV files");
            }

            // 提取5个文件的间期测量值
            var file1Interval = reader.ExtractIntervalMeasurements(csvFiles[0]);
            var file2Interval = reader.ExtractIntervalMeasurements(csvFiles[1]);
            var file3Interval = reader.ExtractIntervalMeasurements(csvFiles[2]);
            var file4Interval = reader.ExtractIntervalMeasurements(csvFiles[3]);
            var file5Interval = reader.ExtractIntervalMeasurements(csvFiles[4]);

            var summaries = new List<IntervalSummary>();

            // 创建7种间期类型的摘要（P宽、Q宽、R宽、S宽、QRS宽、PR间期、QT间期）
            // 每种类型占一行

            // P宽
            summaries.Add(new IntervalSummary
            {
                Lead = "全部导联",
                MeasurementType = "P波时限",
                File1Value = file1Interval.PWidth,
                File2Value = file2Interval.PWidth,
                File3Value = file3Interval.PWidth,
                File4Value = file4Interval.PWidth,
                File5Value = file5Interval.PWidth,
                AverageValue = Math.Round((file1Interval.PWidth + file2Interval.PWidth + file3Interval.PWidth + file4Interval.PWidth + file5Interval.PWidth) / 5, 0),
                StandardValue = 0
            });

            // Q宽
            summaries.Add(new IntervalSummary
            {
                Lead = "全部导联",
                MeasurementType = "Q波时限",
                File1Value = file1Interval.QWidth,
                File2Value = file2Interval.QWidth,
                File3Value = file3Interval.QWidth,
                File4Value = file4Interval.QWidth,
                File5Value = file5Interval.QWidth,
                AverageValue = Math.Round((file1Interval.QWidth + file2Interval.QWidth + file3Interval.QWidth + file4Interval.QWidth + file5Interval.QWidth) / 5, 0),
                StandardValue = 0
            });

            // R宽
            summaries.Add(new IntervalSummary
            {
                Lead = "全部导联",
                MeasurementType = "R波时限",
                File1Value = file1Interval.RWidth,
                File2Value = file2Interval.RWidth,
                File3Value = file3Interval.RWidth,
                File4Value = file4Interval.RWidth,
                File5Value = file5Interval.RWidth,
                AverageValue = Math.Round((file1Interval.RWidth + file2Interval.RWidth + file3Interval.RWidth + file4Interval.RWidth + file5Interval.RWidth) / 5, 0),
                StandardValue = 0
            });

            // S宽
            summaries.Add(new IntervalSummary
            {
                Lead = "全部导联",
                MeasurementType = "S波时限",
                File1Value = file1Interval.SWidth,
                File2Value = file2Interval.SWidth,
                File3Value = file3Interval.SWidth,
                File4Value = file4Interval.SWidth,
                File5Value = file5Interval.SWidth,
                AverageValue = Math.Round((file1Interval.SWidth + file2Interval.SWidth + file3Interval.SWidth + file4Interval.SWidth + file5Interval.SWidth) / 5, 0),
                StandardValue = 0
            });

            // QRS宽
            summaries.Add(new IntervalSummary
            {
                Lead = "全部导联",
                MeasurementType = "QRS波时限",
                File1Value = file1Interval.QRSWidth,
                File2Value = file2Interval.QRSWidth,
                File3Value = file3Interval.QRSWidth,
                File4Value = file4Interval.QRSWidth,
                File5Value = file5Interval.QRSWidth,
                AverageValue = Math.Round((file1Interval.QRSWidth + file2Interval.QRSWidth + file3Interval.QRSWidth + file4Interval.QRSWidth + file5Interval.QRSWidth) / 5, 0),
                StandardValue = 0
            });

            // PR间期
            summaries.Add(new IntervalSummary
            {
                Lead = "全部导联",
                MeasurementType = "PQ间期",
                File1Value = file1Interval.PRInterval,
                File2Value = file2Interval.PRInterval,
                File3Value = file3Interval.PRInterval,
                File4Value = file4Interval.PRInterval,
                File5Value = file5Interval.PRInterval,
                AverageValue = Math.Round((file1Interval.PRInterval + file2Interval.PRInterval + file3Interval.PRInterval + file4Interval.PRInterval + file5Interval.PRInterval) / 5, 0),
                StandardValue = 0
            });

            // QT间期
            summaries.Add(new IntervalSummary
            {
                Lead = "全部导联",
                MeasurementType = "QT间期",
                File1Value = file1Interval.QTInterval,
                File2Value = file2Interval.QTInterval,
                File3Value = file3Interval.QTInterval,
                File4Value = file4Interval.QTInterval,
                File5Value = file5Interval.QTInterval,
                AverageValue = Math.Round((file1Interval.QTInterval + file2Interval.QTInterval + file3Interval.QTInterval + file4Interval.QTInterval + file5Interval.QTInterval) / 5, 0),
                StandardValue = 0
            });

            return summaries;
        }

        /// <summary>
        /// 批量处理Data2文件夹中的CSV文件，提取P宽、QRS宽、PR间期、QT间期
        /// 文件命名模式：年月日时分表-MA1_001.CSV 到 MA1_125.CSV
        /// </summary>
        public List<IntervalMeasurementSummary> ProcessData2Folder(string folderPath, EcgFileReader reader)
        {
            // 获取文件夹下所有CSV文件
            var csvFiles = Directory.GetFiles(folderPath, "*.csv")
                                    .OrderBy(f => f)
                                    .ToList();

            if (csvFiles.Count == 0)
            {
                Console.WriteLine($"警告：文件夹 {folderPath} 中没有找到CSV文件");
                return new List<IntervalMeasurementSummary>();
            }

            Console.WriteLine($"找到 {csvFiles.Count} 个CSV文件，开始处理...");

            var summaries = new List<IntervalMeasurementSummary>();

            foreach (var csvFile in csvFiles)
            {
                try
                {
                    // 从文件名中提取编号（如 MA1_001）
                    var fileName = Path.GetFileNameWithoutExtension(csvFile);
                    var fileNumber = ExtractFileNumber(fileName);

                    if (string.IsNullOrEmpty(fileNumber))
                    {
                        Console.WriteLine($"  跳过文件：{fileName}（无法提取编号）");
                        continue;
                    }

                    // 提取间期测量值
                    var intervalMeasurement = reader.ExtractIntervalMeasurements(csvFile);

                    // 创建汇总记录
                    var summary = new IntervalMeasurementSummary
                    {
                        FileNumber = fileNumber,
                        PWidth = intervalMeasurement.PWidth,
                        QRSWidth = intervalMeasurement.QRSWidth,
                        PRInterval = intervalMeasurement.PRInterval,
                        QTInterval = intervalMeasurement.QTInterval
                    };

                    summaries.Add(summary);
                    Console.WriteLine($"  已处理：{fileNumber}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  处理文件 {csvFile} 时出错：{ex.Message}");
                }
            }

            return summaries;
        }

        /// <summary>
        /// 从文件名中提取编号（如从 "260708140891093-CAL20000-1" 提取 "CAL20000-1"）
        /// </summary>
        private string ExtractFileNumber(string fileName)
        {
            // 文件名格式：年月日时分表-MA1_001.CSV 或类似格式
            // 我们需要提取最后的编号部分

            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // 按连字符分割，取最后一部分作为编号
            var parts = fileName.Split('-');
            if (parts.Length > 0)
            {
                var lastPart = parts[^1];
                // 如果最后一部分包含.CSV（如果是完整文件名），则去除
                lastPart = lastPart.Replace(".CSV", "").Replace(".csv", "");
                return lastPart;
            }

            // 如果没有连字符，尝试直接使用文件名（去除可能的扩展名）
            return fileName.Replace(".CSV", "").Replace(".csv", "");
        }
    }
}