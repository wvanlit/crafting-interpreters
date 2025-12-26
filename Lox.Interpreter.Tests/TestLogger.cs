using Microsoft.Extensions.Logging;

namespace Lox.Interpreter.Tests;

public sealed class TestLogger<T> : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    public LogExpectation Errors => new(this, LogLevel.Error);
    public LogExpectation Warnings => new(this, LogLevel.Warning);
    public LogExpectation Information => new(this, LogLevel.Information);
    public LogExpectation Debug => new(this, LogLevel.Debug);
    public LogExpectation Trace => new(this, LogLevel.Trace);

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry(logLevel, formatter(state, exception), ExtractTemplate(state), ExtractProperties(state)));
    }

    public LogExpectation Expect(LogLevel logLevel) => new(this, logLevel);

    public sealed class LogExpectation(TestLogger<T> Logger, LogLevel LogLevel)
    {
        public IReadOnlyList<LogEntry> MatchingEntries =>
            Logger.Entries.Where(entry => entry.LogLevel == LogLevel).ToList();

        public LogExpectation Any()
        {
            Assert.True(MatchingEntries.Count > 0,
                $"Expected at least one {LogLevel} log entry. Logged:{FormatEntries(Logger.Entries)}");
            return this;
        }

        public LogExpectation None()
        {
            Assert.True(MatchingEntries.Count == 0,
                $"Expected no {LogLevel} log entries. Logged:{FormatEntries(MatchingEntries)}");
            return this;
        }

        public LogExpectation Count(int expected)
        {
            Assert.True(MatchingEntries.Count == expected,
                $"Expected {expected} {LogLevel} log entries, got {MatchingEntries.Count}. Logged:{FormatEntries(MatchingEntries)}");
            return this;
        }

        public LogExpectation MessageContains(string substring)
        {
            var matches = MatchingEntries.Any(entry => entry.Message.Contains(substring, StringComparison.Ordinal));

            Assert.True(matches,
                $"Expected a {LogLevel} log entry containing '{substring}'. Logged:{FormatEntries(MatchingEntries)}");

            return this;
        }

        public LogExpectation AllMessagesContain(string substring)
        {
            var missing = MatchingEntries
                .Where(entry => !entry.Message.Contains(substring, StringComparison.Ordinal))
                .ToList();
            Assert.True(missing.Count == 0,
                $"Expected all {LogLevel} log entries to contain '{substring}'. Missing:{FormatEntries(missing)}");
            return this;
        }

        public TemplateExpectation Template(string template)
        {
            var matches = MatchingEntries
                .Where(entry => string.Equals(entry.Template, template, StringComparison.Ordinal))
                .ToList();
            Assert.True(matches.Count > 0,
                $"Expected a {LogLevel} log entry with template '{template}'. " +
                $"Available templates:{FormatTemplates(MatchingEntries)}");
            return new TemplateExpectation(template, matches);
        }
    }

    public sealed class TemplateExpectation(string Template, IReadOnlyList<LogEntry> Entries)
    {
        public TemplateExpectation With(string name, object? value)
        {
            var matches = Entries
                .Where(entry => entry.Properties.TryGetValue(name, out var entryValue) &&
                                Equals(entryValue, value))
                .ToList();
            Assert.True(matches.Count > 0,
                $"Expected a log entry with template '{Template}' to have '{name}' = {FormatValue(value)}. " +
                $"Logged:{FormatEntries(Entries)}");
            return new TemplateExpectation(Template, matches);
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }

    public sealed record LogEntry(
        LogLevel LogLevel,
        string Message,
        string? Template,
        IReadOnlyDictionary<string, object?> Properties);

    private static string FormatEntries(IEnumerable<LogEntry> entries, int maxEntries = 5)
    {
        var entryList = entries as IReadOnlyList<LogEntry> ?? entries.ToList();
        if (entryList.Count == 0)
        {
            return " <none>";
        }

        var rendered = entryList
            .Take(maxEntries)
            .Select(entry =>
            $"{entry.LogLevel}: \"{entry.Message}\" Template={FormatValue(entry.Template)} Props={FormatProperties(entry.Properties)}");
        var suffix = entryList.Count > maxEntries ? " ..." : string.Empty;
        return Environment.NewLine + string.Join(Environment.NewLine, rendered) + suffix;
    }

    private static string FormatTemplates(IEnumerable<LogEntry> entries)
    {
        var templates = entries
            .Select(entry => entry.Template ?? "<null>")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        if (templates.Count == 0)
        {
            return " <none>";
        }

        return " " + string.Join(", ", templates.Select(FormatValue));
    }

    private static string FormatProperties(IReadOnlyDictionary<string, object?> properties)
    {
        if (properties.Count == 0)
        {
            return "<none>";
        }

        var rendered = properties
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key}={FormatValue(pair.Value)}");
        return "[" + string.Join("; ", rendered) + "]";
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "<null>",
            string s => $"\"{s}\"",
            _ => value.ToString() ?? "<null>"
        };
    }

    private static string? ExtractTemplate<TState>(TState state)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            return null;
        }

        foreach (var pair in pairs)
        {
            if (string.Equals(pair.Key, "{OriginalFormat}", StringComparison.Ordinal) &&
                pair.Value is string template)
            {
                return template;
            }
        }

        return null;
    }

    private static IReadOnlyDictionary<string, object?> ExtractProperties<TState>(TState state)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            return new Dictionary<string, object?>(StringComparer.Ordinal);
        }

        var properties = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in pairs)
        {
            if (!string.Equals(pair.Key, "{OriginalFormat}", StringComparison.Ordinal))
            {
                properties[pair.Key] = pair.Value;
            }
        }

        return properties;
    }
}
