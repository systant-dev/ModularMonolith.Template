using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ModularMonolith.Template.SourceGenerators.Shared.Models;

public readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    #region Public Method Declarations

    public Location ToLocation()
    {
        return Location.Create(FilePath, TextSpan, LineSpan);
    }

    public static LocationInfo? CreateFrom(SyntaxNode node)
    {
        return CreateFrom(node.GetLocation());
    }

    public static LocationInfo? CreateFrom(Location location)
    {
        if (location.SourceTree is null)
        {
            return null;
        }

        LocationInfo locationInfo = new(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);

        return locationInfo;
    }

    #endregion
}
