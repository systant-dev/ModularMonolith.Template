namespace ModularMonolith.Template.SharedKernel.Paging;

public sealed class PagingOptions
{
    #region Constant Declarations

    public const string SectionName = "Paging";

    #endregion

    #region Property Declarations

    public int DefaultLimit { get; set; } = 20;

    public int MaxLimit { get; set; } = 100;

    #endregion
}
