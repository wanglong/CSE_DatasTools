using CSE_DatasTools.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace CSE_DatasTools.Services
{
    public class EcgFileReader
    {
        public List<EcgRecord> ReadEcgFile(string filePath)
        {
            // 兼容多种 CSV 编码与非标准表头：按列索引解析数据行，避免依赖文件中可能损坏或不同编码的表头文本。
            var results = new List<EcgRecord>();

            // 先用默认编码读取；如果需要可以在外部调用时确保文件编码正确
            using var reader = new StreamReader(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using var csv = new CsvReader(reader, config);

            if(csv == null)
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
                if(validLeads.Any(c => c == lead))
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

        public IntervalMeasurement ExtractIntervalMeasurements(string filePath)
        {
            var result = new IntervalMeasurement();

            // 尝试使用多种编码读取文件
            var encodings = new[]
            {
                //System.Text.Encoding.UTF8,
                //System.Text.Encoding.GetEncoding("GB2312"),
                //System.Text.Encoding.GetEncoding("GBK"),
                //System.Text.Encoding.Default,
                System.Text.Encoding.ASCII
            };

            string[] lines = Array.Empty<string>();
            bool readSuccess = false;

            foreach (var encoding in encodings)
            {
                try
                {                    
                    var testLinesAll = File.ReadAllLines(filePath, encoding);
                    // 跳过文件开头的 13 行（按要求忽略前 13 行内容），防止这些行被 CsvReader 解析为数据或表头
                    string[] testLines = Array.Empty<string>();
                    List<string> filter = new List<string>();
                    for (int i = 14; i < testLinesAll.Length && i < 21; i++)
                    {
                        filter.Add(testLinesAll[i]);
                    }

                    testLines = filter.ToArray();
                    // 检查是否包含预期的中文文本
                    var hasExpectedText = testLines.Any(l => 
                       l.Contains("P", StringComparison.OrdinalIgnoreCase) 
                    || l.Contains("Q", StringComparison.OrdinalIgnoreCase)
                    || l.Contains("R", StringComparison.OrdinalIgnoreCase)
                    || l.Contains("S", StringComparison.OrdinalIgnoreCase)
                    || l.Contains("QRS", StringComparison.OrdinalIgnoreCase)
                    || l.Contains("PR", StringComparison.OrdinalIgnoreCase)
                    || l.Contains("QT", StringComparison.OrdinalIgnoreCase));
                    if (hasExpectedText)
                    {
                        lines = testLines;
                        readSuccess = true;
                        break;
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (!readSuccess || lines.Length == 0)
            {
                return result; // 返回默认值
            }

            // 从CSV底部的总结部分提取间期值
            // 格式: P时限：115 ms, Q时限：74 ms, R时限：49 ms, S时限：46 ms
            // QRS时限：100 ms, PR间期：178 ms, QT间期：398 ms, QTC间期：398 ms
            foreach (var line in lines)
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

            return result;
        }

        private double ExtractValue(string line)
        {
            // 从类似 "P时限：115 ms" 的行中提取数字 115
            // 提取数字部分
            var match = System.Text.RegularExpressions.Regex.Match(line, @"(\d+(\.\d+)?)");
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