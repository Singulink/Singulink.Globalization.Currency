using System.Collections;

namespace Singulink.Globalization.Tests;

internal static class Common
{
    public static readonly Currency CurrencyX = new("Currency X", "XXXX", "X", 2);
    public static readonly Currency CurrencyY = new("Currency Y", "YYYY", "Y", 2);

    public static void CheckReadOnlyCollection<T>(this ICollection<T> collection)
    {
        collection.IsReadOnly.ShouldBeTrue();

        collection.GetEnumerator().MoveNext();
        ((IEnumerable)collection).GetEnumerator().MoveNext();

        Should.Throw<NotSupportedException>(() => collection.Add(default!));
        Should.Throw<NotSupportedException>(() => collection.Clear());
        Should.Throw<NotSupportedException>(() => collection.Remove(default!));
    }

    public static void CheckMutableCollection<T>(this ICollection<T> collection)
    {
        collection.IsReadOnly.ShouldBeFalse();

        collection.GetEnumerator().MoveNext();
        ((IEnumerable)collection).GetEnumerator().MoveNext();

        AnyResultExceptNotSupportedException(() => collection.Add(default!));
        AnyResultExceptNotSupportedException(() => collection.Clear());
        AnyResultExceptNotSupportedException(() => collection.Remove(default!));
    }

    private static void AnyResultExceptNotSupportedException(Action action)
    {
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception
        try
        {
            action();
        }
        catch (NotSupportedException)
        {
            Assert.Fail("Non-readonly method");
        }
        catch (Exception) { }
#pragma warning restore RCS1075
    }
}
