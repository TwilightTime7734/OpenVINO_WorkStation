using System.Drawing.Imaging;
using System.Net.Http;
using System.Runtime.InteropServices;
using OpenVinoSharp;

namespace WorkStationC_;

// Owns decoded image tensors until native generation has returned.
internal sealed class ModelImages : IDisposable
{
    internal const int MaxImages = 8;
    private const int MaxImageBytes = 20 * 1024 * 1024;
    private const long MaxPixels = 40_000_000;
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(30) };
    private readonly List<Tensor> _tensors = [];
    public Tensor[] Tensors => _tensors.ToArray();

    public async Task LoadAsync(IReadOnlyList<string> urls, CancellationToken cancellationToken)
    {
        if (urls.Count > MaxImages) throw new InvalidDataException($"A request supports at most {MaxImages} images.");
        foreach (string url in urls)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] bytes;
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                int comma = url.IndexOf(',');
                if (comma < 0 || !url[..comma].StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)
                    || !url[..comma].EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Images must be base64 image data URLs or HTTP(S) URLs.");
                if (url.Length - comma - 1 > (MaxImageBytes + 2L) / 3 * 4)
                    throw new InvalidDataException("Each image must be at most 20 MiB.");
                try { bytes = Convert.FromBase64String(url[(comma + 1)..]); }
                catch (FormatException) { throw new InvalidDataException("The image contains invalid base64 data."); }
            }
            else
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
                    throw new InvalidDataException("Images must be base64 image data URLs or HTTP(S) URLs.");
                try
                {
                    using var download = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    download.CancelAfter(TimeSpan.FromSeconds(30));
                    using var response = await Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, download.Token).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    await response.Content.LoadIntoBufferAsync(MaxImageBytes, download.Token).ConfigureAwait(false);
                    bytes = await response.Content.ReadAsByteArrayAsync(download.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                { throw new InvalidDataException("Downloading the image timed out."); }
            }
            if (bytes.Length > MaxImageBytes) throw new InvalidDataException("Each image must be at most 20 MiB.");
            _tensors.Add(Decode(bytes));
        }
    }

    internal static Tensor Decode(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            using var image = Image.FromStream(stream, useEmbeddedColorManagement: true, validateImageData: true);
            if ((long)image.Width * image.Height > MaxPixels)
                throw new InvalidDataException("Image dimensions exceed 40 million pixels.");
            using var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }
            byte[] rgb = new byte[checked(bitmap.Width * bitmap.Height * 3)];
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte[] row = new byte[bitmap.Width * 3];
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, row.Length);
                    for (int x = 0; x < row.Length; x += 3)
                    {
                        int offset = y * row.Length + x;
                        rgb[offset] = row[x + 2];
                        rgb[offset + 1] = row[x + 1];
                        rgb[offset + 2] = row[x];
                    }
                }
            }
            finally { bitmap.UnlockBits(data); }
            using Shape shape = new([1, bitmap.Height, bitmap.Width, 3]);
            Tensor tensor = new(shape, ElementType.U8);
            try { tensor.SetData(rgb); return tensor; }
            catch { tensor.Dispose(); throw; }
        }
        catch (Exception ex) when (ex is ArgumentException or ExternalException or OutOfMemoryException)
        { throw new InvalidDataException("The image could not be decoded. Use a valid PNG or JPEG image.", ex); }
    }

    public void Dispose()
    {
        foreach (var tensor in _tensors) tensor.Dispose();
        _tensors.Clear();
    }
}
