using System.Collections.ObjectModel;

namespace ApiTemplate.Application.Results;

/// <summary>
/// Represents an RFC 9457 "Problem Details" response payload for HTTP APIs.
///
/// Spec: RFC 9457 - Problem Details for HTTP APIs
/// https://datatracker.ietf.org/doc/html/rfc9457
/// </summary>
public sealed class ProblemResult(
    int status,
    string title,
    string type = "about:blank",
    IReadOnlyDictionary<string, object?>? extensions = null)
{
    /// <summary>RFC 9457: problem type URI.</summary>
    public string Type { get; } = string.IsNullOrWhiteSpace(type) ? "about:blank" : type;

    /// <summary>RFC 9457: short, human-readable summary of the problem type.</summary>
    public string Title { get; } = string.IsNullOrWhiteSpace(title) ? "Error" : title;

    /// <summary>RFC 9457: HTTP status code.</summary>
    public int Status => status;

    /// <summary>RFC 9457 extension members.</summary>
    public IReadOnlyDictionary<string, object?> Extensions { get; } = NormalizeExtensions(extensions);

    private static IReadOnlyDictionary<string, object?> NormalizeExtensions(IReadOnlyDictionary<string, object?>? extensions)
    {
        if (extensions == null || extensions.Count == 0)
            return EmptyExtensions.Instance;

        var copy = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var kv in extensions)
        {
            if (IsStandardMember(kv.Key))
                continue;

            copy[kv.Key] = kv.Value;
        }

        if (copy.Count == 0)
            return EmptyExtensions.Instance;

        return new ReadOnlyDictionary<string, object?>(copy);
    }

    private static bool IsStandardMember(string key)
    {
        return key.Equals("type", StringComparison.OrdinalIgnoreCase)
            || key.Equals("title", StringComparison.OrdinalIgnoreCase)
            || key.Equals("status", StringComparison.OrdinalIgnoreCase)
            || key.Equals("detail", StringComparison.OrdinalIgnoreCase)
            || key.Equals("instance", StringComparison.OrdinalIgnoreCase)
            || key.Equals("extensions", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class EmptyExtensions : ReadOnlyDictionary<string, object?>
    {
        public static readonly EmptyExtensions Instance = new();
        private EmptyExtensions() : base(new Dictionary<string, object?>(0)) { }
    }
}