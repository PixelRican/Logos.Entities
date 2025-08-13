// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a collection of tables that have a common archetype.
    /// </summary>
    public sealed class EntityGrouping : IGrouping<EntityArchetype, EntityTable>,
        ICollection<EntityTable>, ICollection, IReadOnlyCollection<EntityTable>
    {
        private readonly EntityArchetype m_key;
        private readonly EntityTable[] m_values;

        private EntityGrouping(EntityArchetype key, EntityTable[] values)
        {
            m_key = key;
            m_values = values;
        }

        /// <summary>
        /// Gets the key of the <see cref="EntityGrouping"/>.
        /// </summary>
        /// <returns>
        /// The key of the <see cref="EntityGrouping"/>.
        /// </returns>
        public EntityArchetype Key
        {
            get => m_key;
        }

        /// <summary>
        /// Gets the number of tables contained in the <see cref="EntityGrouping"/>.
        /// </summary>
        /// <returns>
        /// The number of tables contained in the <see cref="EntityGrouping"/>.
        /// </returns>
        public int Count
        {
            get => m_values.Length;
        }

        bool ICollection<EntityTable>.IsReadOnly
        {
            get => true;
        }

        bool ICollection.IsSynchronized
        {
            get => false;
        }

        object ICollection.SyncRoot
        {
            get => this;
        }

        /// <summary>
        /// Creates an empty <see cref="EntityGrouping"/> that groups tables by the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to group tables by.
        /// </param>
        /// <returns>
        /// An empty <see cref="EntityGrouping"/> that groups tables by <paramref name="key"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public static EntityGrouping Create(EntityArchetype key)
        {
            ArgumentNullException.ThrowIfNull(key);
            return new EntityGrouping(key, Array.Empty<EntityTable>());
        }

        /// <summary>
        /// Creates an <see cref="EntityGrouping"/> that groups tables by the archetype that models
        /// entities in the specified table and adds the table to the <see cref="EntityGrouping"/>.
        /// </summary>
        /// <param name="item">
        /// The table to add to the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// An <see cref="EntityGrouping"/> that groups tables by the archetype that models entities
        /// in <paramref name="item"/> and adds <paramref name="item"/> to the
        /// <see cref="EntityGrouping"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static EntityGrouping Create(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return new EntityGrouping(item.Archetype, new EntityTable[] { item });
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityGrouping"/> with the specified table added to it.
        /// </summary>
        /// <param name="item">
        /// The table to add to the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityGrouping"/> with <paramref name="item"/> added to it, or
        /// the <see cref="EntityGrouping"/> if the archetype that models entities stored in
        /// <paramref name="item"/> does not match the key of the <see cref="EntityGrouping"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public EntityGrouping Add(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EntityArchetype key = m_key;

            if (key.Equals(item.Archetype))
            {
                ReadOnlySpan<EntityTable> sourceSpan = new ReadOnlySpan<EntityTable>(m_values);
                EntityTable[] values = new EntityTable[sourceSpan.Length + 1];

                sourceSpan.CopyTo(new Span<EntityTable>(values));
                values[sourceSpan.Length] = item;
                return new EntityGrouping(key, values);
            }

            return this;
        }

        /// <summary>
        /// Determines whether the <see cref="EntityGrouping"/> contains a specific table.
        /// </summary>
        /// <param name="item">
        /// The table to locate in the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> is found in the
        /// <see cref="EntityGrouping"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(EntityTable item)
        {
            return Array.IndexOf(m_values, item) != -1;
        }

        /// <summary>
        /// Copies the elements of the <see cref="EntityGrouping"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the copied from the
        /// <see cref="EntityGrouping"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the <see cref="EntityGrouping"/> is greater than the available
        /// space from <paramref name="arrayIndex"/> to the end of <paramref name="array"/>. 
        /// </exception>
        public void CopyTo(EntityTable[] array, int arrayIndex)
        {
            Array.Copy(m_values, 0, array, arrayIndex, m_values.Length);
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityGrouping"/> with the specified table removed from
        /// it.
        /// </summary>
        /// <param name="item">
        /// The table to remove from the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityGrouping"/> with <paramref name="item"/> removed from it,
        /// or the <see cref="EntityGrouping"/> if it does not contain <paramref name="item"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public EntityGrouping Remove(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EntityArchetype key = m_key;

            if (key.Equals(item.Archetype))
            {
                ReadOnlySpan<EntityTable> sourceSpan = new ReadOnlySpan<EntityTable>(m_values);

                // The MemoryExtensions.IndexOf method only works for spans that store types derived
                // from IEquatable for some reason, hence this workaround. The Array.IndexOf method
                // may seem like a reasonable replacement, though the span does offer better
                // performance.
                for (int i = 0; i < sourceSpan.Length; i++)
                {
                    if (sourceSpan[i] == item)
                    {
                        if (sourceSpan.Length == 1)
                        {
                            return new EntityGrouping(key, Array.Empty<EntityTable>());
                        }

                        EntityTable[] values = new EntityTable[sourceSpan.Length - 1];
                        Span<EntityTable> destinationSpan = new Span<EntityTable>(values);

                        sourceSpan.Slice(0, i).CopyTo(destinationSpan);
                        sourceSpan.Slice(i + 1).CopyTo(destinationSpan.Slice(i));
                        return new EntityGrouping(key, values);
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Returns an <see cref="Enumerator"/> that iterates through the
        /// <see cref="EntityGrouping"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Enumerator"/> that can be used to iterate through the
        /// <see cref="EntityGrouping"/>.
        /// </returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        void ICollection<EntityTable>.Add(EntityTable item)
        {
            throw new NotSupportedException();
        }

        void ICollection<EntityTable>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<EntityTable>.Remove(EntityTable item)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
            {
                ThrowForInvalidArrayRank();
            }

            try
            {
                Array.Copy(m_values, 0, array!, index, m_values.Length);
            }
            catch (ArrayTypeMismatchException)
            {
                ThrowForInvalidArrayType();
            }
        }

        IEnumerator<EntityTable> IEnumerable<EntityTable>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayRank()
        {
            throw new ArgumentException("The array is multidimensional.", "array");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayType()
        {
            throw new ArgumentException(
                "EntityTable cannot be cast automatically to the type of the destination array.",
                "array");
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="EntityGrouping"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<EntityTable>
        {
            private readonly EntityTable[] m_values;
            private readonly int m_length;
            private int m_index;

            internal Enumerator(EntityGrouping grouping)
            {
                m_values = grouping.m_values;
                m_length = m_values.Length;
                m_index = -1;
            }

            /// <summary>
            /// Gets the element in the <see cref="EntityGrouping"/> at the current position of the
            /// <see cref="Enumerator"/>.
            /// </summary>
            /// <returns>
            /// The element in the <see cref="EntityGrouping"/> at the current position of the
            /// <see cref="Enumerator"/>.
            /// </returns>
            public readonly EntityTable Current
            {
                get => m_values[m_index];
            }

            readonly object IEnumerator.Current
            {
                get => m_values[m_index];
            }

            /// <inheritdoc cref="IDisposable.Dispose"/>
            public readonly void Dispose()
            {
            }

            /// <summary>
            /// Advances the <see cref="Enumerator"/> to the next element of the
            /// <see cref="EntityGrouping"/>.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the <see cref="Enumerator"/> was successfully advanced to
            /// the next element; <see langword="false"/> if the <see cref="Enumerator"/> has passed
            /// the end of the <see cref="EntityGrouping"/>.
            /// </returns>
            public bool MoveNext()
            {
                int index = m_index + 1;

                if (index < m_length)
                {
                    m_index = index;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Sets the <see cref="Enumerator"/> to its initial position, which is before the first
            /// element in the <see cref="EntityGrouping"/>.
            /// </summary>
            public void Reset()
            {
                m_index = -1;
            }
        }
    }
}
