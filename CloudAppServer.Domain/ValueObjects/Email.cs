using System;
using System.Text.RegularExpressions;

namespace CloudApp.Domain.ValueObjects;

public sealed partial class Email : IEquatable<Email>
{
    private static readonly Regex Regex = EmailRegex();

    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        var normalized = email.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(normalized))
            throw new ArgumentException("Invalid email format.", nameof(email));

        return new Email(normalized);
    }

    public override bool Equals(object? obj) => Equals(obj as Email);
    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode(StringComparison.InvariantCulture);
    public override string ToString() => Value;

    public static bool operator ==(Email? a, Email? b) => Equals(a, b);
    public static bool operator !=(Email? a, Email? b) => !Equals(a, b);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
