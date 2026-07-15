using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using CSE_DatasTools.Models;

namespace CSE_DatasTools.Services
{
    public class EcgFileWriter
    {
        public void WriteAmplitudeSummary(string outputPath, List<AmplitudeSummary> summaries)
        {
            using var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            csv.WriteRecords(summaries);
        }

        public void WriteIntervalSummary(string outputPath, List<IntervalSummary> summaries)
        {
            using var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            csv.WriteRecords(summaries);
        }

        public void WriteIntervalMeasurementSummary(string outputPath, List<IntervalMeasurementSummary> summaries)
        {
            using var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            csv.WriteRecords(summaries);
        }
    }
}