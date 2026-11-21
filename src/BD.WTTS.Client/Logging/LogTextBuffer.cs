namespace BD.WTTS.Logging;

/// <summary>Thread-safe, bounded history. The viewer reads only newly appended text.</summary>
public sealed class LogTextBuffer
{
    internal const int MaxCharacters = 256 * 1024;

    readonly object sync = new();
    char[] ring = [];
    int head;
    int count;
    long end;

    public void Append(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        lock (sync)
            AppendCore(text);
    }

    public void Clear() => Replace(null);

    public void Replace(string? text)
    {
        lock (sync)
        {
            head = count = 0;
            end++; // Leave a gap so readers discard the old document even on a same-size replacement.
            if (!string.IsNullOrEmpty(text))
                AppendCore(text);
        }
    }

    void AppendCore(string text)
    {
        end += text.Length;
        var incoming = text.AsSpan();
        if (incoming.Length >= MaxCharacters)
        {
            incoming = incoming[^MaxCharacters..];
            head = count = 0;
        }
        var discard = Math.Max(0, count + incoming.Length - MaxCharacters);
        if (discard > 0)
        {
            head = (head + discard) % ring.Length;
            count -= discard;
        }
        var required = count + incoming.Length;
        if (required > ring.Length)
        {
            var grown = new char[Math.Min(MaxCharacters, Math.Max(required, Math.Max(1024, ring.Length * 2)))];
            if (count != 0)
                CopyTo(ring, head, grown.AsSpan(0, count));
            ring = grown;
            head = 0;
        }
        var tail = (head + count) % ring.Length;
        var first = Math.Min(incoming.Length, ring.Length - tail);
        incoming[..first].CopyTo(ring.AsSpan(tail));
        incoming[first..].CopyTo(ring);
        count = required;
        if (char.IsLowSurrogate(ring[head]))
        {
            head = (head + 1) % ring.Length;
            count--;
        }
    }

    internal (long First, long End, string Text) ReadSince(long offset)
    {
        lock (sync)
        {
            var first = end - count;
            var start = Math.Min(end, Math.Max(first, offset));
            var length = (int)(end - start);
            return (first, end, length == 0 ? string.Empty : string.Create(length,
                (ring, index: (head + (int)(start - first)) % ring.Length),
                static (destination, state) => CopyTo(state.ring, state.index, destination)));
        }
    }

    static void CopyTo(char[] source, int offset, Span<char> destination)
    {
        var first = Math.Min(destination.Length, source.Length - offset);
        source.AsSpan(offset, first).CopyTo(destination);
        source.AsSpan(0, destination.Length - first).CopyTo(destination[first..]);
    }
}
