using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ModularMonolith.Template.MappingGen.Models;

internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    #region Public Method Declarations

    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

    public static LocationInfo? CreateFrom(SyntaxNode node) => CreateFrom(node.GetLocation());

    public static LocationInfo? CreateFrom(Location location)
    {
        if (location.SourceTree is null)
        {
            return null;
        }

        LinePositionSpan lineSpan = location.GetLineSpan().Span;
        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, lineSpan);
    }

    #endregion
}