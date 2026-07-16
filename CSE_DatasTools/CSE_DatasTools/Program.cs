using CSE_DatasTools.Models;
using CSE_DatasTools.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Runtime;

namespace CSE_DatasTools
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // 设置内存限制
            //GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            
            // 生成唯一的 TraceId
            var traceId = Guid.NewGuid().ToString("N");

            // ============================================================
            // 1. 加载配置文件 appsettings.json，获取数据路径配置
            // ============================================================
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var dataPaths = configuration.GetSection("DataPaths").Get<DataPathConfig>()
                           ?? new DataPathConfig();

            // 如果配置项为空或未配置，则使用代码中的默认值
            var baseDirectory    = !string.IsNullOrWhiteSpace(dataPaths.BaseDirectory)
                                       ? dataPaths.BaseDirectory
                                       : Path.Combine(Environment.CurrentDirectory, "Datas"); 
            var data1Directory   = !string.IsNullOrWhiteSpace(dataPaths.Data1)
                                       ? dataPaths.Data1
                                       : Path.Combine(Environment.CurrentDirectory, "Datas","Data1");
            var data2Directory   = !string.IsNullOrWhiteSpace(dataPaths.Data2)
                                       ? dataPaths.Data2
                                       : Path.Combine(Environment.CurrentDirectory, "Datas", "Data2");

            var data2DirectoryResult = !string.IsNullOrWhiteSpace(dataPaths.Data2Result)
                                       ? dataPaths.Data2Result
                                       : Path.Combine(Environment.CurrentDirectory, "Datas", "Data2", "Result");

            var logDirectory     = !string.IsNullOrWhiteSpace(dataPaths.LogDirectory)
                                       ? dataPaths.LogDirectory
                                       : Path.Combine(Environment.CurrentDirectory, "Datas", "logs");

            // ============================================================
            // 2. 配置 Serilog — 日志路径从配置读取
            // ============================================================
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithCorrelationIdHeader("TraceId")
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [TraceId:{CorrelationId}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code
                )
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "app-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [TraceId:{CorrelationId}] {Message:lj}{NewLine}{Exception}",
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1)
                )
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "app-.json"),
                    rollingInterval: RollingInterval.Day,
                    formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1)
                )
                // Optional: Uncomment for Seq integration
                //.WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            try
            {
                // Push the TraceId to the LogContext for all logs in this execution
                using (LogContext.PushProperty("CorrelationId", traceId))
                {
                    Log.Information("ECG Amplitude Extraction Tool - Starting");
                    Log.Information("TraceId: {TraceId}", traceId);
                    Log.Information("配置文件路径: {ConfigPath}",
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"));
                    Log.Information("DataPaths -> Base: {Base}, Data1: {Data1}, Data2: {Data2}, Log: {Log}",
                        baseDirectory, data1Directory, data2Directory, logDirectory);
                    Log.Information("=============================");

                    // 检查基础目录是否存在
                    if (!Directory.Exists(baseDirectory))
                    {
                        Log.Warning("基础目录不存在: {BaseDirectory}", baseDirectory);
                        Log.Warning("请在 appsettings.json 中配置正确的 DataPaths.BaseDirectory，或创建该目录。");
                        Log.Warning("按任意键退出...");
                        Console.ReadKey();
                        return;
                    }

                    // 设置进程当前目录为 Data1 目录，然后枚举子文件夹
                    if (!Directory.Exists(data1Directory))
                    {
                        Log.Warning("Data1 目录不存在: {Data1Directory}", data1Directory);
                        Log.Warning("按任意键退出...");
                        Console.ReadKey();
                        return;
                    }

                    Directory.SetCurrentDirectory(data1Directory);
                    var currentDirectory = Directory.GetCurrentDirectory();
                    var subdirectories = Directory.GetDirectories(currentDirectory)
                                                  .ToList();

                    var reader = new EcgFileReader();
                    var writer = new EcgFileWriter();
                    var processor = new EcgDataProcessor();

                    Log.Information("Found {FolderCount} folders to process", subdirectories.Count);

                    // 添加Data1处理性能监控
                    var data1ProcessingStopwatch = Stopwatch.StartNew();
                    var memoryBeforeData1 = GC.GetTotalMemory(false);

                    // 1. 并行处理文件夹
                    Parallel.ForEach(subdirectories, folder =>
                    {
                        try
                        {
                            var folderName = Path.GetFileName(folder);
                            Log.Information("Processing folder: {FolderName}", folderName);

                            // 生成幅值测量 CSV
                            var summaries = processor.ProcessFolder(folder, reader);

                            var outputFileName = $"{folderName}-幅值测量.csv";
                            var outputPath = Path.Combine(currentDirectory, outputFileName);

                            writer.WriteAmplitudeSummary(outputPath, summaries);

                            Log.Information("  Generated: {OutputFileName}", outputFileName);

                            // 生成间期测量 CSV
                            var intervalSummaries = processor.ProcessFolderForIntervals(folder, reader);

                            var intervalOutputFileName = $"{folderName}-间期测量.csv";
                            var intervalOutputPath = Path.Combine(currentDirectory, intervalOutputFileName);

                            writer.WriteIntervalSummary(intervalOutputPath, intervalSummaries);

                            Log.Information("  Generated: {IntervalOutputFileName}", intervalOutputFileName);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error processing folder {FolderPath}", folder);
                        }
                    });

                    data1ProcessingStopwatch.Stop();
                    var memoryAfterData1 = GC.GetTotalMemory(false);
                    var memoryUsedData1 = memoryAfterData1 - memoryBeforeData1;

                    Log.Information("=============================");
                    Log.Information("Data1 处理完成，耗时: {ElapsedMilliseconds}ms, 内存使用: {MemoryUsed} bytes",
                        data1ProcessingStopwatch.ElapsedMilliseconds, memoryUsedData1);

                    // 处理 Data2 文件夹 - 批量提取间期测量值并生成汇总统计
                    if (Directory.Exists(data2Directory))
                    {
                        Log.Information("开始处理 Data2 文件夹");
                        Directory.SetCurrentDirectory(data2Directory);
                        var data2CurrentDirectory = Directory.GetCurrentDirectory();

                        try
                        {
                            // 获取所有CSV文件
                            var csvFiles = Directory.GetFiles(data2CurrentDirectory, "*.csv")
                                                    .OrderBy(f => f)
                                                    .ToList();

                            Log.Information("找到 {CsvFileCount} 个CSV文件", csvFiles.Count);

                            if (csvFiles.Count > 0)
                            {
                                // 添加Data2处理性能监控
                                var data2ProcessingStopwatch = Stopwatch.StartNew();
                                var memoryBeforeData2 = GC.GetTotalMemory(false);

                                // 批量提取间期测量值 - 使用分批处理，每次处理100个文件
                                var measurementSummaries = processor.ProcessData2Folder(data2CurrentDirectory, reader, 100);

                                data2ProcessingStopwatch.Stop();
                                var memoryAfterData2 = GC.GetTotalMemory(false);
                                var memoryUsedData2 = memoryAfterData2 - memoryBeforeData2;

                                if (measurementSummaries.Count > 0)
                                {
                                    // 生成汇总统计CSV
                                    var summaryOutputFileName = "汇总统计.csv";

                                    // 确保结果目录存在，优先使用 data2DirectoryResult
                                    if (!Directory.Exists(data2DirectoryResult))
                                    {
                                        Directory.CreateDirectory(data2DirectoryResult);
                                        Log.Information("已创建 Data2 结果目录: {Data2DirectoryResult}", data2DirectoryResult);
                                    }

                                    var summaryOutputPath = Path.Combine(data2DirectoryResult, summaryOutputFileName);

                                    writer.WriteIntervalMeasurementSummary(summaryOutputPath, measurementSummaries);

                                    Log.Information("  已生成：{SummaryOutputFileName} (路径: {SummaryOutputPath})", summaryOutputFileName, summaryOutputPath);
                                    Log.Information("  共汇总 {MeasurementCount} 个文件的测量数据", measurementSummaries.Count);
                                }
                                else
                                {
                                    Log.Warning("  未提取到任何测量数据");
                                }

                                Log.Information("Data2 处理完成，耗时: {ElapsedMilliseconds}ms, 内存使用: {MemoryUsed} bytes",
                                    data2ProcessingStopwatch.ElapsedMilliseconds, memoryUsedData2);
                            }
                            else
                            {
                                Log.Warning("  Data2 文件夹中没有CSV文件");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "处理 Data2 文件夹时出错");
                        }
                    }
                    else
                    {
                        Log.Warning("Data2 文件夹不存在: {Data2Directory}", data2Directory);
                    }

                    Log.Information("=============================");
                    Log.Information("Processing complete!");
                    Log.Information("TraceId: {TraceId}", traceId);
                    Log.Information("Press any key to exit...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "应用程序发生致命错误");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}