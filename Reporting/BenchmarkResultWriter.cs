using System.Globalization;
using System.Text;
using System.Text.Json;
using WorkStationC_.Models;

namespace WorkStationC_.Reporting
{
    internal sealed record BenchmarkReportPaths(string CsvPath, string JsonPath);

    internal static class BenchmarkResultWriter
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private static readonly char[] CsvSpecialCharacters = { ',', '"', '\r', '\n' };

        public static BenchmarkReportPaths Write(BenchmarkResult result, string resultsDirectory)
        {
            Directory.CreateDirectory(resultsDirectory);
            string stamp = result.CompletedAtUtc.ToLocalTime().ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
            string baseName = $"benchmark_{SanitizeFileName(result.Device)}_{stamp}";
            string csvPath = Path.Combine(resultsDirectory, $"{baseName}.csv");
            string jsonPath = Path.Combine(resultsDirectory, $"{baseName}.json");

            WriteTextAtomically(csvPath, BuildCsv(result));
            WriteTextAtomically(jsonPath, JsonSerializer.Serialize(result, JsonOptions));
            return new BenchmarkReportPaths(csvPath, jsonPath);
        }

        private static void WriteTextAtomically(string path, string contents)
        {
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath)
                ?? throw new InvalidOperationException($"No parent directory exists for: {fullPath}");
            Directory.CreateDirectory(directory);
            string temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.partial");
            try
            {
                File.WriteAllText(
                    temporaryPath,
                    contents,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                File.Move(temporaryPath, fullPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        private static string BuildCsv(BenchmarkResult result)
        {
            string[] headers =
            {
                "completed_at_utc", "label", "model", "device", "performance_hint",
                "iterations", "warmup", "runs", "samples", "read_ms", "compile_ms",
                "first_infer_ms", "min_ms", "avg_ms", "median_ms", "p90_ms", "p95_ms",
                "p99_ms", "max_ms", "throughput",
            };

            string[] values =
            {
                result.CompletedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                result.Label,
                result.ModelPath,
                result.Device,
                result.PerformanceHint,
                result.Iterations.ToString(CultureInfo.InvariantCulture),
                result.Warmup.ToString(CultureInfo.InvariantCulture),
                result.Runs.ToString(CultureInfo.InvariantCulture),
                result.SampleCount.ToString(CultureInfo.InvariantCulture),
                Format(result.ReadMilliseconds),
                Format(result.CompileMilliseconds),
                Format(result.FirstInferenceMilliseconds),
                Format(result.MinimumMilliseconds),
                Format(result.AverageMilliseconds),
                Format(result.MedianMilliseconds),
                Format(result.P90Milliseconds),
                Format(result.P95Milliseconds),
                Format(result.P99Milliseconds),
                Format(result.MaximumMilliseconds),
                Format(result.ThroughputPerSecond),
            };

            return string.Join(',', headers) + Environment.NewLine
                + string.Join(',', values.Select(EscapeCsv)) + Environment.NewLine;
        }

        private static string Format(double value) => value.ToString("0.######", CultureInfo.InvariantCulture);

        private static string EscapeCsv(string value)
        {
            if (value.IndexOfAny(CsvSpecialCharacters) < 0)
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        private static string SanitizeFileName(string value)
        {
            string sanitized = value;
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                sanitized = sanitized.Replace(invalid, '_');
            }

            return string.IsNullOrWhiteSpace(sanitized) ? "device" : sanitized;
        }
    }
}
