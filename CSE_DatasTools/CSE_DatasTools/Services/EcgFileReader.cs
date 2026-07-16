using CSE_DatasTools.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Reflection.PortableExecutable;
using Serilog;
using System.Text.RegularExpressions;

namespace CSE_DatasTools.Services
{
    public class EcgFileReader
    {
        // 预编译正则表达式，用于高效提取数字
        private static readonly Regex NumberRegex = new Regex(@"(\d+(\.\d+)?)", RegexOptions.Compiled);

        public List<EcgRecord> ReadEcgFile(string filePath)
        {
            // 兼容多种 CSV 编码与非标准表头：按列索引解析数据行，避免依赖文件中可能损坏或不同编码的表头文本。
            var results = new List<EcgRecord>();

            // 使用缓冲读取优化文件I/O性能
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            using var reader = new StreamReader(fileStream, leaveOpen: false);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using var csv = new CsvReader(reader, config);

            if (csv == null)
            {
                return results;
            }

            // 读取头行（如果存在），但不依赖其值；随后按列索引提取需要的字段。
            if (!csv.Read())
            {
                return results;
            }

            csv.ReadHeader();

            while (csv!.Read())
            {
                // 获取原始字段数组
                var record = csv?.Context?.Parser?.Record;
                if (record == null)
                    continue;

                // 跳过明显不是数据行的简短行（例如文件末尾的人类可读注释）
                if (record.Length < 10)
                    continue;

                // 按经验从样例 CSV 推断字段索引：
                // 0: 导联, 2: P1振幅, 7: Q振幅, 9: R振幅, 11: S振幅,
                // 14: ST20振幅, 15: ST40振幅, 16: ST60振幅, 17: ST80振幅, 18: T振幅
                string lead = record.Length > 0 ? record[0] : string.Empty;

                double ParseDoubleAt(int idx)
                {
                    if (idx < 0 || idx >= record.Length)
                        return 0;
                    var s = record[idx];
                    if (string.IsNullOrWhiteSpace(s))
                        return 0;
                    // 尝试多种文化解析（有时小数或负号格式受区域设置影响）
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        return v;
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("zh-CN"), out v))
                        return v;
                    // 去掉非数字字符再试一次
                    var filtered = new string(s.Where(c => char.IsDigit(c) || c == '.' || c == '-' || c == ',').ToArray());
                    filtered = filtered.Replace(",", "");
                    if (double.TryParse(filtered, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                        return v;
                    return 0;
                }

                // 过滤导联
                var validLeads = new HashSet<string> { "I", "II", "V1", "V2", "V3", "V4", "V5", "V6" };
                if (validLeads.Any(c => c == lead))
                {
                    var rec = new EcgRecord
                    {
                        Lead = lead,
                        P1Amplitude = Math.Abs(ParseDoubleAt(2)), // 取绝对值，因为振幅不应为负
                        QAmplitude = Math.Abs(ParseDoubleAt(7)),
                        RAmplitude = Math.Abs(ParseDoubleAt(9)),
                        SAmplitude = Math.Abs(ParseDoubleAt(11)),
                        ST20Amplitude = Math.Abs(ParseDoubleAt(14)),
                        ST40Amplitude = Math.Abs(ParseDoubleAt(15)),
                        ST60Amplitude = Math.Abs(ParseDoubleAt(16)),
                        ST80Amplitude = Math.Abs(ParseDoubleAt(17)),
                        TAmplitude = Math.Abs(ParseDoubleAt(18))
                    };

                    results.Add(rec);
                }
            }

            return results;
        }

        public List<EcgMeasurement> ExtractMeasurements(List<EcgRecord> records)
        {
            var measurements = new List<EcgMeasurement>();

            foreach (var record in records)
            {
                measurements.Add(new EcgMeasurement
                {
                    Lead = record.Lead,
                    P1Amplitude = record.P1Amplitude,
                    QAmplitude = record.QAmplitude,
                    RAmplitude = record.RAmplitude,
                    SAmplitude = record.SAmplitude,
                    ST20Amplitude = record.ST20Amplitude,
                    ST40Amplitude = record.ST40Amplitude,
                    ST60Amplitude = record.ST60Amplitude,
                    ST80Amplitude = record.ST80Amplitude,
                    TAmplitude = record.TAmplitude
                });
            }

            return measurements;
        }

        public async Task<IntervalMeasurement> ExtractIntervalMeasurementsAsync(string filePath)
        {
            var result = new IntervalMeasurement();

            // 使用异步I/O和缓冲读取优化性能
            try
            {
                // 使用异步文件读取
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                using var reader = new StreamReader(fileStream, leaveOpen: false);
                var content = await reader.ReadToEndAsync();

                // 按行分割
                var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // 跳过文件开头的 14 行
                var relevantLines = lines.Skip(14).Take(7).ToArray();

                // 检查是否包含预期的中文文本
                var hasExpectedText = relevantLines.Any(l =>
                   l.Contains('P', StringComparison.OrdinalIgnoreCase)
                || l.Contains('Q', StringComparison.OrdinalIgnoreCase)
                || l.Contains('R', StringComparison.OrdinalIgnoreCase)
                || l.Contains('S', StringComparison.OrdinalIgnoreCase)
                || l.Contains("QRS", StringComparison.OrdinalIgnoreCase)
                || l.Contains("PR", StringComparison.OrdinalIgnoreCase)
                || l.Contains("QT", StringComparison.OrdinalIgnoreCase));

                if (!hasExpectedText || relevantLines.Length == 0)
                {
                    return result; // 返回默认值
                }

                // 从CSV底部的总结部分提取间期值
                // 格式: P时限：115 ms, Q时限：74 ms, R时限：49 ms, S时限：46 ms
                // QRS时限：100 ms, PR间期：178 ms, QT间期：398 ms, QTC间期：398 ms
                foreach (var line in relevantLines)
                {
                    if (line.StartsWith("P") && !line.StartsWith("PR"))
                    {
                        result.PWidth = ExtractValue(line);
                    }
                    else if (line.StartsWith("Q") && !line.StartsWith("QT") && !line.StartsWith("QRS"))
                    {
                        result.QWidth = ExtractValue(line);
                    }
                    else if (line.StartsWith("R"))
                    {
                        result.RWidth = ExtractValue(line);
                    }
                    else if (line.StartsWith("S"))
                    {
                        result.SWidth = ExtractValue(line);
                    }
                    else if (line.StartsWith("QRS"))
                    {
                        result.QRSWidth = ExtractValue(line);
                    }
                    else if (line.StartsWith("PR"))
                    {
                        result.PRInterval = ExtractValue(line);
                    }
                    else if (line.StartsWith("QT") && !line.StartsWith("QTC"))
                    {
                        result.QTInterval = ExtractValue(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "读取文件 {FilePath} 时出错", filePath);
            }

            return result;
        }

        public IntervalMeasurement ExtractIntervalMeasurements(string filePath)
        {
            // 同步版本，调用异步版本
            return ExtractIntervalMeasurementsAsync(filePath).GetAwaiter().GetResult();
        }

        private double ExtractValue(string line)
        {
            // 从类似 "P时限：115 ms" 的行中提取数字 115
            // 使用预编译的正则表达式提高性能
            var match = NumberRegex.Match(line);
            if (match.Success)
            {
                if (double.TryParse(match.Groups[1].Value, out var value))
                {
                    return value;
                }
            }

            return 0;
        }
    }
}