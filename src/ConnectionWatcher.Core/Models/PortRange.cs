namespace ConnectionWatcher.Core.Models;

public sealed record PortRange(int? Start, int? End)
{
    public static PortRange Any { get; } = new(null, null);

    public bool IsAny => Start is null && End is null;

    public bool IsValid => IsAny ||
        (Start is >= 1 and <= 65535 &&
         End is >= 1 and <= 65535 &&
         Start <= End);

    public bool Contains(int port)
    {
        return IsAny ||
            (Start is not null && End is not null &&
             port >= Start.Value && port <= End.Value);
    }

    public override string ToString()
    {
        if (IsAny)
        {
            return "*";
        }

        return Start == End ? Start!.Value.ToString() : $"{Start}-{End}";
    }

    public static bool TryParse(string? value, bool any, out PortRange range)
    {
        if (any)
        {
            range = Any;
            return true;
        }

        string text = (value ?? string.Empty).Trim();
        string[] parts = text.Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length == 1 && int.TryParse(parts[0], out int single))
        {
            range = new PortRange(single, single);
            return range.IsValid;
        }

        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int start) &&
            int.TryParse(parts[1], out int end))
        {
            range = new PortRange(start, end);
            return range.IsValid;
        }

        range = new PortRange(0, 0);
        return false;
    }
}
