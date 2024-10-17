using System.Collections;

namespace Singulink.Globalization;

/// <content>
/// Contains the <see cref="CurrencyCollection"/> implementation for <see cref="MoneyBag"/>.
/// </content>
partial class MoneyBag
{
    /// <summary>
    /// Represents a collection of currencies in a <see cref="MoneyBag"/>.
    /// </summary>
    public sealed partial class CurrencyCollection : ICollection<Currency>, IReadOnlyCollection<Currency>
    {
        private readonly MoneyBag _bag;

        internal CurrencyCollection(MoneyBag bag)
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

            foreach (var currency in _bag._amountLookup.Keys)
                array[arrayIndex++] = currency;
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
            private Dictionary<Currency, decimal>.KeyCollection.Enumerator _currencyEnumerator;

            internal Enumerator(MoneyBag bag)
            {
                _currencyEnumerator = bag._amountLookup.Keys.GetEnumerator();
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public Currency Current => _currencyEnumerator.Current;

            /// <inheritdoc/>
            object? IEnumerator.Current => Current;

            /// <summary>
            /// Releases all the resources used by the enumerator.
            /// </summary>
            public void Dispose() => _currencyEnumerator.Dispose();

            /// <summary>
            /// Advances the enumerator to the next element.
            /// </summary>
            public bool MoveNext() => _currencyEnumerator.MoveNext();

            /// <summary>
            /// Not supported.
            /// </summary>
            /// <exception cref="NotSupportedException">This operation is not supported.</exception>
            void IEnumerator.Reset() => throw new NotSupportedException();
        }
    }
}