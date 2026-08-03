using System.Security.Cryptography;
using System.Text;

namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// A redactor that replaces the value with a deterministic, keyed HMAC-SHA256 hash. This preserves
/// the ability to correlate identical values across records without exposing the original content.
/// </summary>
public sealed class HmacRedactor : IStringRedactor, IDisposable
{
    #region Field Declarations

    private readonly HMACSHA256 _hmac;

    #endregion

    #region Constructor Declarations

    /// <summary>Creates an HMAC redactor using the supplied key.</summary>
    /// <param name="key">The secret key used to compute the HMAC. Must be non-empty.</param>
    public HmacRedactor(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 0)
        {
            throw new ArgumentException("The HMAC key must not be empty.", nameof(key));
        }

        _hmac = new HMACSHA256(key);
    }

    /// <summary>Creates an HMAC redactor using the UTF-8 bytes of the supplied key.</summary>
    /// <param name="key">The secret key used to compute the HMAC. Must be non-empty.</param>
    public HmacRedactor(string key)
        : this(Encoding.UTF8.GetBytes(key ?? throw new ArgumentNullException(nameof(key))))
    {
    }

    #endregion

    #region Public Method Declarations

    public string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        byte[] hash;
        lock (_hmac)
        {
            hash = _hmac.ComputeHash(Encoding.UTF8.GetBytes(value!));
        }

        return Convert.ToBase64String(hash);
    }

    public void Dispose() => _hmac.Dispose();

    #endregion
}
