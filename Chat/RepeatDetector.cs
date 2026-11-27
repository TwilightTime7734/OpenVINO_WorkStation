using System.Text;

namespace WorkStationC_;

// Raised when a reply keeps repeating the same text, so the chat stops instead of running on.
internal sealed class RepeatingReplyException(bool thinking) : Exception(thinking
    ? "Stopped: the model kept repeating the same thoughts. Try rephrasing, or start a New Chat."
    : "Stopped: the model kept repeating the same text. Try rephrasing, or start a New Chat.");

// Finds a block of text repeated back-to-back at the end of a reply, which is how a model loop looks.
internal static class RepeatDetector
{
    private const int Window = 8000;
    private const int MinPeriod = 8;
    private const int MinRepeatedCharacters = 300;

    internal static bool IsRepeating(StringBuilder text)
    {
        if (text.Length < MinRepeatedCharacters) return false;
        int start = Math.Max(0, text.Length - Window);
        return IsRepeating(text.ToString(start, text.Length - start));
    }

    internal static bool IsRepeating(string text)
    {
        ReadOnlySpan<char> tail = text.AsSpan();
        for (int period = MinPeriod; period * 3 <= tail.Length; period++)
        {
            int repeats = period < 50 ? 8 : period < 200 ? 4 : 3;
            if (period * repeats < MinRepeatedCharacters) repeats = (MinRepeatedCharacters + period - 1) / period;
            if (period * repeats > tail.Length) continue;
            ReadOnlySpan<char> last = tail[^period..];
            bool same = true;
            for (int copy = 2; copy <= repeats && same; copy++)
                same = tail.Slice(tail.Length - copy * period, period).SequenceEqual(last);
            if (same && LooksLikeText(last)) return true;
        }
        return false;
    }

    // A long divider line such as "=====" repeats too; only treat blocks that contain words as a loop.
    private static bool LooksLikeText(ReadOnlySpan<char> block)
    {
        int letters = 0;
        HashSet<char> distinct = [];
        foreach (char character in block)
        {
            if (char.IsLetter(character)) letters++;
            distinct.Add(character);
        }
        return letters >= 3 && distinct.Count >= 4;
    }
}
