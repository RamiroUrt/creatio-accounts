namespace CreatioAccounts.Api.Infrastructure;

public static class EnvFileLoader
{
    public static Dictionary<string, string?> LoadFrom()
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roots = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
        foreach (var root in roots)
        {
            var current = Path.GetFullPath(root);
            for (var level = 0; level < 6; level++)
            {
                var candidate = Path.Combine(current, ".env");
                if (!seen.Contains(candidate) && File.Exists(candidate))
                {
                    seen.Add(candidate);
                    result = Merge(result, ParseFile(candidate));
                }

                var parent = Path.GetDirectoryName(current);
                if (parent is null || parent == current)
                {
                    break;
                }
                current = parent;
            }
        }

        return result;
    }

    private static Dictionary<string, string?> ParseFile(string path)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = Unquote(line[(separator + 1)..].Trim());
            var configKey = ToConfigKey(key);

            if (configKey is null)
            {
                continue;
            }

            values[configKey] = Substitute(value, values);
        }

        return values;
    }

    private static string? ToConfigKey(string key)
    {
        if (key.Contains("__", StringComparison.Ordinal))
        {
            return key.Replace("__", ":", StringComparison.Ordinal);
        }

        if (key.StartsWith("CREATIO_", StringComparison.OrdinalIgnoreCase))
        {
            var rest = key["CREATIO_".Length..];
            var segments = rest.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return "Creatio:" + string.Join("", segments.Select(ToPascalCase));
        }

        return null;
    }

    private static string ToPascalCase(string segment)
    {
        if (string.IsNullOrEmpty(segment))
        {
            return segment;
        }

        return char.ToUpperInvariant(segment[0]) + segment[1..].ToLowerInvariant();
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 && ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
        {
            return value[1..^1];
        }

        return value;
    }

    private static string Substitute(string value, Dictionary<string, string?> known)
    {
        int start;
        while ((start = value.IndexOf("${", StringComparison.Ordinal)) >= 0)
        {
            var end = value.IndexOf('}', start);
            if (end < 0)
            {
                break;
            }

            var varName = value[(start + 2)..end];
            var resolved = known.TryGetValue(varName, out var v) ? v : Environment.GetEnvironmentVariable(varName);
            value = value[..start] + (resolved ?? "") + value[(end + 1)..];
        }

        return value;
    }

    private static Dictionary<string, string?> Merge(Dictionary<string, string?> target, Dictionary<string, string?> source)
    {
        foreach (var (key, value) in source)
        {
            if (!target.ContainsKey(key))
            {
                target[key] = value;
            }
        }

        return target;
    }
}