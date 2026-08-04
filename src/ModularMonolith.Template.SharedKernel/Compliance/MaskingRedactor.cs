using Microsoft.Extensions.Compliance.Redaction;

namespace ModularMonolith.Template.SharedKernel.Compliance;

public sealed class MaskingRedactor : Redactor
{
    #region Field Declarations

    private const char MaskCharacter = '*';

    #endregion

    #region Public Method Declarations

    public override int GetRedactedLength(ReadOnlySpan<char> input) => input.Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        destination[..source.Length].Fill(MaskCharacter);
        return source.Length;
    }

    #endregion
}
