using System.Collections;
using System.Collections.Immutable;

namespace Singulink.Globalization;

/// <content>
/// Contains the <see cref="CurrencyCollection"/> implementation for <see cref="ImmutableMoneyBag"/>.
/// </content>
partial class ImmutableMoneyBag
{
    /// <summary>
    /// Represents a collection of currencies in a <see cref="ImmutableMoneyBag"/>.
    /// </summary>
    public sealed partial class CurrencyCollection : ICollection<Currency>, IReadOnlyCollection<Currency>
    {
        private readonly ImmutableMoneyBag _bag;

        internal CurrencyCollection(ImmutableMoneyBag bag)
        {
            _bag = bag;
        }

        /// <summary>
        /// Gets the number of currencies in this collection.
        /// </summary>
        public int Count => _bag.Count;

        /// <summary>
        /// Determines whether the currency collection contains the specified key.
        /// </summary>
        public bool Contains(Currency currency) => _bag.ContainsCurrency(currency);

        /// <summary>
        /// Returns an enumerator that iterates through the currencies in this collection.
        /// </summary>
        public Enumerator GetEnumerator() => new(_bag);

        #region Explicit Interface Implementations

        /// <inheritdoc/>
        bool ICollection<Currency>.IsReadOnly => true;

        /// <inheritdoc/>
        void ICollection<Currency>.CopyTo(Currency[] array, int arrayIndex)
        {
            CollectionCopy.CheckParams(Count, array, arrayIndex);

            foreach (var entry in _bag._amountLookup)
                array[arrayIndex++] = entry.Key;
        }

        /// <inheritdoc/>
        IEnumerator<Currency> IEnumerable<Currency>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Not Supported

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        void ICollection<Currency>.Clear() => throw new NotSupportedException();

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        void ICollection<Currency>.Add(Currency? item) => throw new NotSupportedException();

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        bool ICollection<Currency>.Remove(Currency? item) => throw new NotSupportedException();

        #endregion

        /// <summary>
        /// Enumerates the elements of a <see cref="CurrencyCollection"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<Currency>
        {
            private ImmutableDictionary<Currency, decimal>.Enumerator _entryEnumerator;

            internal Enumerator(ImmutableMoneyBag bag)
            {
                _entryEnumerator = bag._amountLookup.GetEnumerator();
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public Currency Current => _entryEnumerator.Current.Key;

            /// <inheritdoc/>
            object? IEnumerator.Current => Current;

            /// <summary>
            /// Releases all the resources used by the enumerator.
            /// </summary>
            public void Dispose() => _entryEnumerator.Dispose();

            /// <summary>
            /// Advances the enumerator to the next element.
            /// </summary>
            public bool MoveNext() => _entryEnumerator.MoveNext();

            /// <summary>
            /// Not supported.
            /// </summary>
            /// <exception cref="NotSupportedException">This operation is not supported.</exception>
            void IEnumerator.Reset() => throw new NotSupportedException();
        }
    }
}