namespace CSE_DatasTools.Models
{
    /// <summary>
    /// 数据路径配置模型，对应 appsettings.json 中 "DataPaths" 节的配置。
    /// 所有属性均为可空，读取后若为 null 则使用代码中的默认值。
    /// </summary>
    public class DataPathConfig
    {
        /// <summary>
        /// 基础目录，默认: D:\Datas
        /// </summary>
        public string? BaseDirectory { get; set; }

        /// <summary>
        /// Data1 子目录，默认: D:\Datas\Data1
        /// </summary>
        public string? Data1 { get; set; }

        /// <summary>
        /// Data2 子目录，默认: D:\Datas\Data2
        /// </summary>
        public string? Data2 { get; set; }

        /// <summary>
        /// Data2 子目录，默认: D:\Datas\Data2\Result
        /// </summary>
        public string? Data2Result { get; set; }

        /// <summary>
        /// 日志输出目录，默认: D:\Datas\logs
        /// </summary>
        public string? LogDirectory { get; set; }
    }
}
