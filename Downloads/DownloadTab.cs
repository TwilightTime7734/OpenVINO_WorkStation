using System.Net.Http;
using static WorkStationC_.AppFiles;

namespace WorkStationC_;

internal sealed class DownloadTab(TextBox address, ProgressBar progress, Label status, Action<string> output) : IDisposable
{
    private readonly HttpClient _client = new() { Timeout = Timeout.InfiniteTimeSpan };
    private PullAdvice? _advice;
    private string _adviceAddress = string.Empty;

    public async Task FindAsync(CancellationToken cancellationToken)
    {
        var result = await new ModelDownload(_client).FindAsync(address.Text, cancellationToken);
        var files = result.Files;
        _advice = result.Advice;
        _adviceAddress = address.Text.Trim();
        status.Text = $"{files.Count} file(s) ready to download.";
        output(status.Text);
        foreach (var file in files.Take(20)) output(file.Destination);
        if (files.Count > 20) output($"...and {files.Count - 20} more files.");
        if (_advice is not null)
        {
            output($"Recommendation: {_advice.Message}");
            if (_advice.Route != DownloadRoute.DirectDownload) status.Text += " See the recommendation in Output.";
        }
    }

    // The advice for the address in the box, looking it up when Find has not been pressed for it yet.
    public async Task<PullAdvice?> GetAdviceAsync(CancellationToken cancellationToken)
    {
        if (_advice is null || !_adviceAddress.Equals(address.Text.Trim(), StringComparison.Ordinal))
            await FindAsync(cancellationToken);
        return _advice;
    }

    public async Task DownloadAsync(CancellationToken cancellationToken)
    {
        var downloader = new ModelDownload(_client);
        var files = (await downloader.FindAsync(address.Text, cancellationToken)).Files;
        bool finished = false;
        var updates = new Progress<(string Label, long Bytes, long? Total, TimeSpan Elapsed)>(value =>
        {
            if (finished) return;
            progress.Style = value.Total > 0 ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
            if (value.Total > 0) progress.Value = (int)Math.Clamp(100.0 * value.Bytes / value.Total.Value, 0, 100);
            status.Text = $"{value.Label}: {FormatBytes((ulong)value.Bytes)}";
        });
        try
        {
            await downloader.DownloadAsync(files,
                (label, bytes, total, elapsed) => ((IProgress<(string, long, long?, TimeSpan)>)updates).Report((label, bytes, total, elapsed)),
                output, cancellationToken);
            status.Text = "Download complete.";
        }
        finally
        {
            finished = true;
            progress.Style = ProgressBarStyle.Blocks;
            progress.Value = 0;
        }
    }

    public void Dispose() => _client.Dispose();
}
