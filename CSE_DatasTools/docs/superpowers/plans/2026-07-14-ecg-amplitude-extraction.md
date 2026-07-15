# ECG Amplitude Extraction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a .NET 8 console application that reads ECG CSV files from subdirectories, extracts amplitude measurements from 8 leads (I, II, V1-V6), and generates summary CSV files with extracted P1, Q, R, S, ST20, ST40, ST60, ST80, T amplitude values.

**Architecture:** A console application that iterates through folders containing 5 CSV files each, parses ECG measurements using CsvHelper, aggregates data across files, and outputs formatted summary tables with ST measurements expanded into separate rows.

**Tech Stack:** .NET 8, CsvHelper (NuGet package), System.IO for file operations

---

### Task 1: Set up project structure and dependencies

**Files:**
- Modify: `CSE_DatasTools/CSE_DatasTools.csproj`
- Create: `CSE_DatasTools/Models/`
- Create: `CSE_DatasTools/Services/`

- [ ] **Step 1: Add CsvHelper NuGet package to project**

```xml
<ItemGroup>
  <PackageReference Include="CsvHelper" Version="30.0.1" />
</ItemGroup>
```

- [ ] **Step 2: Create Models directory structure**

Run: `mkdir "CSE_DatasTools/Models" && mkdir "CSE_DatasTools/Services"`

Expected: Directories created successfully

- [ ] **Step 3: Commit**

```bash
git add CSE_DatasTools/CSE_DatasTools.csproj
git commit -m "chore: add CsvHelper NuGet package"
```

---

### Task 2: Define data models for ECG measurements

**Files:**
- Create: `CSE_DatasTools/Models/EcgMeasurement.cs`
- Create: `CSE_DatasTools/Models/EcgRecord.cs`
- Create: `CSE_DatasTools/Models/AmplitudeSummary.cs`

- [ ] **Step 1: Write the EcgMeasurement model class**

```csharp
namespace CSE_DatasTools.Models
{
    public class EcgMeasurement
    {
        public string Lead { get; set; } = string.Empty;
        public double P1Amplitude { get; set; }
        public double QAmplitude { get; set; }
        public double RAmplitude { get; set; }
        public double SAmplitude { get; set; }
        public double ST20Amplitude { get; set; }
        public double ST40Amplitude { get; set; }
        public double ST60Amplitude { get; set; }
        public double ST80Amplitude { get; set; }
        public double TAmplitude { get; set; }
    }
}
```

- [ ] **Step 2: Write the EcgRecord model class**

```csharp
using CsvHelper.Configuration.Attributes;

namespace CSE_DatasTools.Models
{
    public class EcgRecord
    {
        [Name("导联")]
        public string Lead { get; set; } = string.Empty;

        [Name("P1振幅")]
        public double P1Amplitude { get; set; }

        [Name("Q振幅")]
        public double QAmplitude { get; set; }

        [Name("R振幅")]
        public double RAmplitude { get; set; }

        [Name("S振幅")]
        public double SAmplitude { get; set; }

        [Name("ST20振幅")]
        public double ST20Amplitude { get; set; }

        [Name("ST40振幅")]
        public double ST40Amplitude { get; set; }

        [Name("ST60振幅")]
        public double ST60Amplitude { get; set; }

        [Name("ST80振幅")]
        public double ST80Amplitude { get; set; }

        [Name("T振幅")]
        public double TAmplitude { get; set; }
    }
}
```

- [ ] **Step 3: Write the AmplitudeSummary model class for output**

```csharp
namespace CSE_DatasTools.Models
{
    public class AmplitudeSummary
    {
        public string Lead { get; set; } = string.Empty;
        public string MeasurementType { get; set; } = string.Empty;
        public double File1Value { get; set; }
        public double File2Value { get; set; }
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add CSE_DatasTools/Models/
git commit -m "feat: add ECG data models"
```

---

### Task 3: Implement CSV file reading service

**Files:**
- Create: `CSE_DatasTools/Services/EcgFileReader.cs`

- [ ] **Step 1: Write the EcgFileReader class**

```csharp
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using CSE_DatasTools.Models;

namespace CSE_DatasTools.Services
{
    public class EcgFileReader
    {
        public List<EcgRecord> ReadEcgFile(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            });

            var records = csv.GetRecords<EcgRecord>().ToList();
            return records;
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
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add CSE_DatasTools/Services/EcgFileReader.cs
git commit -m "feat: add ECG file reader service"
```

---

### Task 4: Implement CSV file writing service

**Files:**
- Create: `CSE_DatasTools/Services/EcgFileWriter.cs`

- [ ] **Step 1: Write the EcgFileWriter class**

```csharp
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
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add CSE_DatasTools/Services/EcgFileWriter.cs
git commit -m "feat: add ECG file writer service"
```

---

### Task 5: Implement ECG data processor

**Files:**
- Create: `CSE_DatasTools/Services/EcgDataProcessor.cs`

- [ ] **Step 1: Write the EcgDataProcessor class**

```csharp
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

            if (csvFiles.Count < 2)
            {
                throw new InvalidOperationException($"Folder {folderPath} must contain at least 2 CSV files");
            }

            var file1Measurements = ReadAndFilterMeasurements(csvFiles[0], reader);
            var file2Measurements = ReadAndFilterMeasurements(csvFiles[1], reader);

            var summaries = new List<AmplitudeSummary>();

            foreach (var lead in TargetLeads)
            {
                var m1 = file1Measurements.FirstOrDefault(m => m.Lead == lead);
                var m2 = file2Measurements.FirstOrDefault(m => m.Lead == lead);

                if (m1 != null && m2 != null)
                {
                    // P1
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "P1",
                        File1Value = m1.P1Amplitude,
                        File2Value = m2.P1Amplitude
                    });

                    // Q
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "Q",
                        File1Value = m1.QAmplitude,
                        File2Value = m2.QAmplitude
                    });

                    // R
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "R",
                        File1Value = m1.RAmplitude,
                        File2Value = m2.RAmplitude
                    });

                    // S
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "S",
                        File1Value = m1.SAmplitude,
                        File2Value = m2.SAmplitude
                    });

                    // ST20
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "ST20",
                        File1Value = m1.ST20Amplitude,
                        File2Value = m2.ST20Amplitude
                    });

                    // ST40
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "ST40",
                        File1Value = m1.ST40Amplitude,
                        File2Value = m2.ST40Amplitude
                    });

                    // ST60
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "ST60",
                        File1Value = m1.ST60Amplitude,
                        File2Value = m2.ST60Amplitude
                    });

                    // ST80
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "ST80",
                        File1Value = m1.ST80Amplitude,
                        File2Value = m2.ST80Amplitude
                    });

                    // T
                    summaries.Add(new AmplitudeSummary
                    {
                        Lead = lead,
                        MeasurementType = "T",
                        File1Value = m1.TAmplitude,
                        File2Value = m2.TAmplitude
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
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add CSE_DatasTools/Services/EcgDataProcessor.cs
git commit -m "feat: add ECG data processor service"
```

---

### Task 6: Implement main program logic

**Files:**
- Modify: `CSE_DatasTools/Program.cs`

- [ ] **Step 1: Write the main program implementation**

```csharp
using CSE_DatasTools.Services;

namespace CSE_DatasTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ECG Amplitude Extraction Tool");
            Console.WriteLine("=============================");

            var currentDirectory = Directory.GetCurrentDirectory();
            var subdirectories = Directory.GetDirectories(currentDirectory)
                                          .Where(d => !d.Contains(".git") && !d.Contains("bin") && !d.Contains("obj"))
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

                    var summaries = processor.ProcessFolder(folder, reader);

                    var outputFileName = $"{folderName}-幅值测量.csv";
                    var outputPath = Path.Combine(currentDirectory, outputFileName);

                    writer.WriteAmplitudeSummary(outputPath, summaries);

                    Console.WriteLine($"  Generated: {outputFileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error processing {folder}: {ex.Message}");
                }
            }

            Console.WriteLine("=============================");
            Console.WriteLine("Processing complete!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add CSE_DatasTools/Program.cs
git commit -m "feat: implement main program logic"
```

---

### Task 7: Build and test the application

**Files:**
- Create: `CSE_DatasTools/README.md`

- [ ] **Step 1: Build the project**

Run: `dotnet build CSE_DatasTools/CSE_DatasTools.csproj`

Expected: Build succeeded

- [ ] **Step 2: Create README with usage instructions**

```markdown
# ECG Amplitude Extraction Tool

## Description

This tool reads ECG CSV files from subdirectories, extracts amplitude measurements from 8 leads (I, II, V1-V6), and generates summary CSV files.

## Features

- Extracts 9 amplitude values per lead: P1, Q, R, S, ST20, ST40, ST60, ST80, T
- Processes multiple folders containing CSV files
- Generates summary CSV with proper formatting
- ST measurements expanded into separate rows

## Usage

1. Place folders containing ECG CSV files in the project directory
2. Each folder should contain at least 2 CSV files
3. Run the application: `dotnet run --project CSE_DatasTools/CSE_DatasTools.csproj`
4. Output files will be generated with the format: `{FolderName}-幅值测量.csv`

## Input Format

Each CSV file should contain columns for:
- 导联 (Lead)
- P1振幅
- Q振幅
- R振幅
- S振幅
- ST20振幅
- ST40振幅
- ST60振幅
- ST80振幅
- T振幅

## Output Format

Each output CSV file contains:
- Lead (导联)
- MeasurementType
- File1Value (from first CSV file)
- File2Value (from second CSV file)
```

- [ ] **Step 3: Commit**

```bash
git add CSE_DatasTools/README.md
git commit -m "docs: add README with usage instructions"
```

---

## Self-Review

**Spec Coverage:**
- ✅ Reads multiple folders with 5 CSV files each
- ✅ Extracts 8 leads (I, II, V1, V2, V3, V4, V5, V6)
- ✅ Extracts 9 amplitude values (P1, Q, R, S, ST20, ST40, ST60, ST80, T)
- ✅ First column from first file (-1), second column from second file (-2)
- ✅ Output named as `{FolderName}-幅值测量.csv`
- ✅ ST expanded into separate rows (ST20, ST40, ST60, ST80)
- ✅ One output CSV per folder

**Placeholder Scan:**
- ✅ No placeholders found
- ✅ All code is complete and executable

**Type Consistency:**
- ✅ Model properties consistent across files
- ✅ Service method signatures consistent

---