namespace AIWorkspace.Core.Utils;

public class LogParser
{
    private static readonly string[] Keywords = new[]
    {
        "ERROR", "WARN", "FATAL", "CRIT", "Exception", "Traceback",
        "panic", "deadlock", "segfault", "oom-killer",
        "upstream", "502", "503", "504",
        "CrashLoopBackOff", "ImagePullBackOff", "OOMKilled",
        "ORA-", "SQLSTATE", "timeout", "lock wait"
    };

    public string ExtractKeySections(string content, int contextLines = 5)
    {
        var lines = content.Split('\n');
        var result = new List<string>();
        var included = new HashSet<int>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (Keywords.Any(k => line.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                for (int j = Math.Max(0, i - contextLines);
                     j <= Math.Min(lines.Length - 1, i + contextLines); j++)
                {
                    if (included.Add(j))
                        result.Add(lines[j]);
                }
                if (i < lines.Length - 1)
                    result.Add("---");
            }
        }

        return result.Count > 0 ? string.Join("\n", result) : content;
    }

    public string Deduplicate(string content, int maxOccurrences = 3)
    {
        var lines = content.Split('\n');
        var counts = new Dictionary<string, int>();
        var result = new List<string>();

        foreach (var line in lines)
        {
            var normalized = line.Trim();
            if (string.IsNullOrEmpty(normalized) || normalized == "---")
            {
                result.Add(line);
                continue;
            }

            counts.TryGetValue(normalized, out var count);
            if (count < maxOccurrences)
            {
                result.Add(line);
                counts[normalized] = count + 1;
            }
        }

        return string.Join("\n", result);
    }

    public static int EstimateTokens(string text)
    {
        // Rough estimation: English ~4 chars/token, CJK ~1 char/token, mixed ~3 chars/token
        var cjkCount = text.Count(c => c >= 0x4E00 && c <= 0x9FFF);
        var otherCount = text.Length - cjkCount;
        return (int)(cjkCount * 1.0 + otherCount / 4.0) + 1;
    }
}
