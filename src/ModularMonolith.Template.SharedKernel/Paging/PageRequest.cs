namespace ModularMonolith.Template.SharedKernel.Paging;

public readonly record struct PageRequest(int? AfterId, int? Limit, bool IncludeCount = false);
