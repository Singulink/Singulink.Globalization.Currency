#pragma warning disable SA1649 // File name should match first type name

// TargetDependentStringKey allows the primary implementation of a member to use span keys on .NET 9+ and string keys on older targets.

#if NET9_0_OR_GREATER
global using TargetDependentStringKey = System.ReadOnlySpan<char>;
#else
global using TargetDependentStringKey = System.String;
#endif

// TargetDependentString allows the primary implementation of a member to use spans on .NET and strings .NET Standard.

#if NET
global using TargetDependentString = System.ReadOnlySpan<char>;
#else
global using TargetDependentString = System.String;
#endif

namespace Singulink.Globalization.Utilities;

using RuntimeNullables;
using Singulink.Collections;

[NullChecks(false)]
internal readonly struct StringKeyDictionaryLookupSwitcher<TValue>
{
#if NET9_0_OR_GREATER
    public StringKeyDictionaryLookupSwitcher(Dictionary<string, TValue> d)
    {
        TargetDependent = d.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public Dictionary<string, TValue>.AlternateLookup<ReadOnlySpan<char>> TargetDependent { get; }

    public Dictionary<string, TValue> Dictionary => TargetDependent.Dictionary;
#else
    public StringKeyDictionaryLookupSwitcher(Dictionary<string, TValue> d)
    {
        TargetDependent = d;
    }

    public Dictionary<string, TValue> TargetDependent { get; }

    public Dictionary<string, TValue> Dictionary => TargetDependent;
#endif
}

[NullChecks(false)]
internal readonly struct StringKeyListDictionaryLookupSwitcher<TValue>
{
#if NET9_0_OR_GREATER
    public StringKeyListDictionaryLookupSwitcher(ListDictionary<string, TValue> d)
    {
        TargetDependent = d.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public ListDictionary<string, TValue>.AlternateLookup<ReadOnlySpan<char>> TargetDependent { get; }

    public ListDictionary<string, TValue> Dictionary => TargetDependent.Dictionary;
#else
    public StringKeyListDictionaryLookupSwitcher(ListDictionary<string, TValue> d)
    {
        TargetDependent = d;
    }

    public ListDictionary<string, TValue> TargetDependent { get; }

    public ListDictionary<string, TValue> Dictionary => TargetDependent;
#endif
}
