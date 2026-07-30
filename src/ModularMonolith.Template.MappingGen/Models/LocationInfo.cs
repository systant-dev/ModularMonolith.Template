using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ModularMonolith.Template.MappingGen.Models;

/// <summary>
/// A value-equatable representation of a <see cref="Location"/> that is safe to store in the
/// incremental generator pipeline without breaking output caching.
/// </summary>
internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

    public static LocationInfo? CreateFrom(SyntaxNode node) => CreateFrom(node.GetLocation());

    public static LocationInfo? CreateFrom(Location location)
    {
        if (location.SourceTree is null)
        {
            return null;
        }

        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }
}
