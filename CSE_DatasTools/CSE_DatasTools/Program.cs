using CSE_DatasTools.Services;
using CSE_DatasTools.Models;

namespace CSE_DatasTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ECG Amplitude Extraction Tool");
            Console.WriteLine("=============================");

            // 将当前工作目录切换到 D:\Datas（按用户要求）
            var targetDirectory = @"D:\Datas";
            if (!Directory.Exists(targetDirectory))
            {
                Console.WriteLine($"目标目录不存在: {targetDirectory}");
                Console.WriteLine("请在继续前创建该目录，或修改为存在的路径。");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                return;
            }

            // 设置进程当前目录为目标目录，然后从该目录枚举子文件夹
            targetDirectory = @"D:\Datas\Data1";
            Directory.SetCurrentDirectory(targetDirectory);
            var currentDirectory = Directory.GetCurrentDirectory();
            var subdirectories = Directory.GetDirectories(currentDirectory)
                                          .ToList();

            var reader = new EcgFileReader();
            var writer = new EcgFileWriter();
            var processor = new EcgDataProcessor();

            Console.WriteLine($"Found {subdirectories.Count} folders to process.");

            foreach (var folder in subdirectories)
            {
                try
                {
                    var folderName = Path.GetFileName(folder);
                    Console.WriteLine($"Processing folder: {folderName}");

                    // 生成幅值测量 CSV
                    var summaries = processor.ProcessFolder(folder, reader);

                    var outputFileName = $"{folderName}-幅值测量.csv";
                    var outputPath = Path.Combine(currentDirectory, outputFileName);

                    writer.WriteAmplitudeSummary(outputPath, summaries);

                    Console.WriteLine($"  Generated: {outputFileName}");

                    // 生成间期测量 CSV
                    var intervalSummaries = processor.ProcessFolderForIntervals(folder, reader);

                    var intervalOutputFileName = $"{folderName}-间期测量.csv";
                    var intervalOutputPath = Path.Combine(currentDirectory, intervalOutputFileName);

                    writer.WriteIntervalSummary(intervalOutputPath, intervalSummaries);

                    Console.WriteLine($"  Generated: {intervalOutputFileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error processing {folder}: {ex.Message}");
                }
            }

            Console.WriteLine("=============================");

            // 处理 Data2 文件夹 - 批量提取间期测量值并生成汇总统计
            targetDirectory = @"D:\Datas\Data2";

            if (Directory.Exists(targetDirectory))
            {
                Console.WriteLine("开始处理 Data2 文件夹...");
                Directory.SetCurrentDirectory(targetDirectory);
                var data2CurrentDirectory = Directory.GetCurrentDirectory();

                try
                {
                    // 获取所有CSV文件
                    var csvFiles = Directory.GetFiles(data2CurrentDirectory, "*.csv")
                                            .OrderBy(f => f)
                                            .ToList();

                    Console.WriteLine($"找到 {csvFiles.Count} 个CSV文件");

                    if (csvFiles.Count > 0)
                    {
                        // 批量提取间期测量值
                        var measurementSummaries = processor.ProcessData2Folder(data2CurrentDirectory, reader);

                        if (measurementSummaries.Count > 0)
                        {
                            // 生成汇总统计CSV
                            var summaryOutputFileName = "汇总统计.csv";
                            var summaryOutputPath = Path.Combine(data2CurrentDirectory, summaryOutputFileName);

                            writer.WriteIntervalMeasurementSummary(summaryOutputPath, measurementSummaries);

                            Console.WriteLine($"  已生成：{summaryOutputFileName}");
                            Console.WriteLine($"  共汇总 {measurementSummaries.Count} 个文件的测量数据");
                        }
                        else
                        {
                            Console.WriteLine("  未提取到任何测量数据");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  Data2 文件夹中没有CSV文件");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  处理 Data2 文件夹时出错：{ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Data2 文件夹不存在: {targetDirectory}");
            }

            Console.WriteLine("=============================");
            Console.WriteLine("Processing complete!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}