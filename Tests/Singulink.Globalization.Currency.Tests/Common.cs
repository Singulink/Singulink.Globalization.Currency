using System.Collections;

namespace Singulink.Globalization.Tests;

internal static class Common
{
    public static readonly Currency CurrencyX = new("XXXX", "Long Currency", "XXXX");
    public static readonly Currency CurrencyY = new("YYYY", "Long Currency", "YYYY");
    public static readonly Currency DisallowedCurrency = new("BBB", "Blah blah blah", "$$");

    public static void ShouldBeReadOnlyCollection<T>(this ICollection<T> collection)
    {
        collection.IsReadOnly.ShouldBeTrue();

        Should.NotThrow(collection.GetEnumerator().MoveNext);
        Should.NotThrow(((IEnumerable)collection).GetEnumerator().MoveNext);

        Should.Throw<NotSupportedException>(() => collection.Add(default!));
        Should.Throw<NotSupportedException>(() => collection.Clear());
        Should.Throw<NotSupportedException>(() => collection.Remove(default!));
    }

    public static void ShouldBeMutableCollection<T>(this ICollection<T> collection)
    {
        collection.IsReadOnly.ShouldBeFalse();

        Should.NotThrow(collection.GetEnumerator().MoveNext);
        Should.NotThrow(((IEnumerable)collection).GetEnumerator().MoveNext);
        Should.NotThrow(collection.Clear);
    }
}
