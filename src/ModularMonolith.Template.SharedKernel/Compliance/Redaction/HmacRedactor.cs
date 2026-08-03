using System.Security.Cryptography;
using System.Text;

namespace ModularMonolith.Template.SharedKernel.Compliance.Redaction;

public sealed class HmacRedactor : IStringRedactor, IDisposable
{
    #region Field Declarations

    private readonly HMACSHA256 _hmac;

    #endregion

    #region Constructor Declarations

    public HmacRedactor(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 0)
        {
            throw new ArgumentException("The HMAC key must not be empty.", nameof(key));
        }

        _hmac = new HMACSHA256(key);
    }

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
