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