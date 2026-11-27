
using System.Diagnostics;
using static WorkStationC_.AppFiles;

namespace WorkStationC_;

internal sealed record DownloadFileResult(string TargetPath, bool Skipped);

internal static class FileDownload
{
    // Direct links and Hugging Face files share this download path.
    internal static async Task<DownloadFileResult> DownloadAsync(
        HttpClient client, Uri address, string targetPath, string label,
        Action<string, long, long?, TimeSpan> reportProgress, CancellationToken cancellationToken)
    {
        ValidateDownloadDestination(targetPath);
        using HttpResponseMessage response = await client.GetAsync(
            address, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        long? expectedSize = response.Content.Headers.ContentLength;
        if (expectedSize.HasValue && File.Exists(targetPath) && new FileInfo(targetPath).Length == expectedSize)
        {
            reportProgress(label, expectedSize.Value, expectedSize, TimeSpan.Zero);
            return new(targetPath, Skipped: true);
        }
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        ValidateDownloadDestination(targetPath);
        string temporaryPath = targetPath + ".partial";
        try
        {
            long downloaded = 0;
            Stopwatch watch = Stopwatch.StartNew();
            long lastUpdate = 0;
            await using (Stream input = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (FileStream output = new(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
            {
                byte[] buffer = new byte[81920];
                int count;
                while ((count = await input.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
                    downloaded += count;
                    if (watch.ElapsedMilliseconds - lastUpdate >= 250)
                    {
                        reportProgress(label, downloaded, expectedSize, watch.Elapsed);
                        lastUpdate = watch.ElapsedMilliseconds;
                    }
                }
            }
            if (expectedSize.HasValue && downloaded != expectedSize)
                throw new InvalidDataException($"Download incomplete: received {downloaded:N0} of {expectedSize:N0} bytes.");
            if (Path.GetExtension(targetPath).Equals(".gguf", StringComparison.OrdinalIgnoreCase))
            {
                using FileStream file = File.OpenRead(temporaryPath);
                byte[] header = new byte[4];
                if (file.Read(header) != 4 || !header.AsSpan().SequenceEqual("GGUF"u8))
                    throw new InvalidDataException("The downloaded file is not a GGUF model.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, targetPath, overwrite: true);
            reportProgress(label, downloaded, expectedSize, watch.Elapsed);
            return new(targetPath, Skipped: false);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
