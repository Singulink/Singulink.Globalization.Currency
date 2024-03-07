namespace Singulink.Globalization.Utilities;

internal static class CollectionCopy
{
    public static void CheckParams<TDestination>(int sourceCount, TDestination[] array, int arrayIndex)
    {
        if ((uint)arrayIndex > array.Length)
        {
            static void Throw() => throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            Throw();
        }

        if (array.Length - arrayIndex < sourceCount)
        {
            static void Throw() => throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
            Throw();
        }
    }
}
