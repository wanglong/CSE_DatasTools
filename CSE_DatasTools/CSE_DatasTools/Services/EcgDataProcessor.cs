using CSE_DatasTools.Models;
using Serilog;

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

            EcgMeasurement? m1 = new EcgMeasurement();
            EcgMeasurement? m2 = new EcgMeasurement();
            EcgMeasurement? m3 = new EcgMeasurement();
            EcgMeasurement? m4 = new EcgMeasurement();
            EcgMeasurement? m5 = new EcgMeasurement();

            void AddSummary(string lead, string type, Func<EcgMeasurement, double> getValue)
            {
                summaries.Add(new AmplitudeSummary
                {
                    Lead = lead,
                    MeasurementType = type,
                    File1Value = getValue(m1),
                    File2Value = getValue(m2),
                    File3Value = getValue(m3),
                    File4Value = getValue(m4),
                    File5Value = getValue(m5),
                    AverageValue = Math.Round(new[] { m1, m2, m3, m4, m5 }.Average(getValue), 0)
                });
            }

            foreach (var lead in TargetLeads)
            {
                m1 = file1Measurements.FirstOrDefault(m => m.Lead == lead);
                m2 = file2Measurements.FirstOrDefault(m => m.Lead == lead);
                m3 = file3Measurements.FirstOrDefault(m => m.Lead == lead);
                m4 = file4Measurements.FirstOrDefault(m => m.Lead == lead);
                m5 = file5Measurements.FirstOrDefault(m => m.Lead == lead);

                if (m1 != null && m2 != null && m3 != null && m4 != null && m5 != null)
                {
                    // P1                    
                    AddSummary(lead, "P1", m => m.P1Amplitude);

                    // Q
                    AddSummary(lead, "Q", m => m.QAmplitude);

                    // R
                    AddSummary(lead, "R", m => m.RAmplitude);

                    // S
                    AddSummary(lead, "S", m => m.SAmplitude);

                    // ST20
                    AddSummary(lead, "ST20", m => m.ST20Amplitude);

                    // ST40
                    AddSummary(lead, "ST40", m => m.ST40Amplitude);

                    // ST60
                    AddSummary(lead, "ST60", m => m.ST60Amplitude);

                    // ST80
                    AddSummary(lead, "ST80", m => m.ST80Amplitude);

                    // T
                    AddSummary(lead, "T", m => m.TAmplitude);
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

            void AddSummaryInterval(string lead, string type, Func<IntervalMeasurement, double> getValue)
            {
                summaries.Add(new IntervalSummary
                {
                    Lead = lead,
                    MeasurementType = type,
                    File1Value = getValue(file1Interval),
                    File2Value = getValue(file2Interval),
                    File3Value = getValue(file3Interval),
                    File4Value = getValue(file4Interval),
                    File5Value = getValue(file5Interval),
                    AverageValue = Math.Round(new[] { file1Interval, file2Interval, file3Interval, file4Interval, file5Interval }.Average(getValue), 0)
                });
            }

            // 创建7种间期类型的摘要（P宽、Q宽、R宽、S宽、QRS宽、PR间期、QT间期）
            // 每种类型占一行
            if (file1Interval != null && file2Interval != null && file3Interval != null && file4Interval != null && file5Interval != null)
            {
                // P宽
                AddSummaryInterval("全部导联", "P波时限", m => m.PWidth);

                // Q宽
                AddSummaryInterval("全部导联", "Q波时限", m => m.QWidth);

                // R宽
                AddSummaryInterval("全部导联", "R波时限", m => m.RWidth);

                // S宽
                AddSummaryInterval("全部导联", "S波时限", m => m.SWidth);

                // QRS宽
                AddSummaryInterval("全部导联", "QRS波时限", m => m.QRSWidth);

                // PR间期
                AddSummaryInterval("全部导联", "PQ间期", m => m.PRInterval);

                // QT间期
                AddSummaryInterval("全部导联", "QT间期", m => m.QTInterval);
            }            

            return summaries;
        }

        /// <summary>
        /// 批量处理Data2文件夹中的CSV文件，提取P宽、QRS宽、PR间期、QT间期
        /// 文件命名模式：年月日时分表-MA1_001.CSV 到 MA1_125.CSV
        /// </summary>
        public List<IntervalMeasurementSummary> ProcessData2Folder(string folderPath, EcgFileReader reader, int batchSize = 100)
        {
            // 获取文件夹下所有CSV文件
            var csvFiles = Directory.GetFiles(folderPath, "*.csv")
                                    .OrderBy(f => f)
                                    .ToList();

            if (csvFiles.Count == 0)
            {
                Log.Warning("警告：文件夹 {FolderPath} 中没有找到CSV文件", folderPath);
                return new List<IntervalMeasurementSummary>();
            }

            Log.Information("找到 {CsvFileCount} 个CSV文件，开始分批处理（批次大小: {BatchSize}）...", csvFiles.Count, batchSize);

            var summaries = new List<IntervalMeasurementSummary>();
            var totalFiles = csvFiles.Count;
            var processedFiles = 0;

            // 分批处理文件
            for (int i = 0; i < csvFiles.Count; i += batchSize)
            {
                var batch = csvFiles.Skip(i).Take(batchSize).ToList();
                var batchSummaries = new List<IntervalMeasurementSummary>();

                // 处理当前批次
                foreach (var csvFile in batch)
                {
                    try
                    {
                        // 从文件名中提取编号（如 MA1_001）
                        var fileName = Path.GetFileNameWithoutExtension(csvFile);
                        var fileNumber = ExtractFileNumber(fileName);

                        if (string.IsNullOrEmpty(fileNumber))
                        {
                            Log.Warning("  跳过文件：{FileName}（无法提取编号）", fileName);
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

                        batchSummaries.Add(summary);
                        processedFiles++;
                        Log.Information("  已处理：{FileNumber} ({Processed}/{Total})", fileNumber, processedFiles, totalFiles);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "  处理文件 {CsvFile} 时出错", csvFile);
                    }
                }

                // 将当前批次的处理结果添加到总结果中
                summaries.AddRange(batchSummaries);

                // 监控内存使用情况
                var memoryBefore = GC.GetTotalMemory(false);
                Log.Information("  批次 {BatchNumber} 处理完成，当前内存使用: {MemoryUsed} bytes",
                    (i / batchSize) + 1, memoryBefore);

                // 强制垃圾回收，释放当前批次的内存
                GC.Collect();
                GC.WaitForPendingFinalizers();

                var memoryAfter = GC.GetTotalMemory(false);
                var memoryFreed = memoryBefore - memoryAfter;
                Log.Information("  内存回收完成，释放了 {MemoryFreed} bytes", memoryFreed);
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