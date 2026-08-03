using System.Collections;
using System.Collections.Immutable;

namespace ModularMonolith.Template.SourceGenerators.Shared.Collections;

public readonly struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>>, IEnumerable<T> where T : IEquatable<T>
{
    #region Field Declarations

    public static readonly EquatableArray<T> Empty = new([]);

    private readonly ImmutableArray<T> _array = array.IsDefault ? [] : array;

    #endregion

    #region Property Declarations

    public int Length => _array.IsDefault ? 0 : _array.Length;

    public T this[int index] => _array[index];

    #endregion

    #region Public Method Declarations

    public bool Equals(EquatableArray<T> other)
    {
        if (Length != other.Length)
        {
            return false;
        }

        for (int index = 0; index < Length; index++)
        {
            if (!EqualityComparer<T>.Default.Equals(_array[index], other._array[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            for (int index = 0; index < Length; index++)
            {
                T item = _array[index];
                hash = (hash * 31) + (item is null ? 0 : item.GetHashCode());
            }

            return hash;
        }
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_array.IsDefault ? [] : _array)).GetEnumerator();

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new(array);

    #endregion

    #region Private Method Declarations

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}