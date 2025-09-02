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
        IList<EntityTable>, IList, IReadOnlyList<EntityTable>
    {
        private static readonly EntityGrouping s_empty = new EntityGrouping();

        private readonly EntityArchetype m_key;
        private readonly EntityTable[] m_items;

        private EntityGrouping()
        {
            m_key = EntityArchetype.Base;
            m_items = Array.Empty<EntityTable>();
        }

        private EntityGrouping(EntityArchetype key, EntityTable[] items)
        {
            m_key = key;
            m_items = items;
        }

        /// <summary>
        /// Gets an empty <see cref="EntityGrouping"/> that groups tables by the base archetype.
        /// </summary>
        /// <returns>
        /// An empty <see cref="EntityGrouping"/> that groups tables by the base archetype.
        /// </returns>
        public static EntityGrouping Empty
        {
            get => s_empty;
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
        /// Gets the number of elements contained in the <see cref="EntityGrouping"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="EntityGrouping"/>.
        /// </returns>
        public int Count
        {
            get => m_items.Length;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityGrouping"/> is empty.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityGrouping"/> is empty; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsEmpty
        {
            get => m_items.Length == 0;
        }

        /// <summary>
        /// Gets the element at the specified index in the <see cref="EntityGrouping"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get.
        /// </param>
        /// <returns>
        /// The element at the specified index in the <see cref="EntityGrouping"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> exceeds the bounds of the <see cref="EntityGrouping"/>.
        /// </exception>
        public EntityTable this[int index]
        {
            get => m_items[index];
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

        bool IList.IsFixedSize
        {
            get => true;
        }

        bool IList.IsReadOnly
        {
            get => true;
        }

        EntityTable IList<EntityTable>.this[int index]
        {
            get => m_items[index];
            set => throw new NotSupportedException();
        }

        object? IList.this[int index]
        {
            get => m_items[index];
            set => throw new NotSupportedException();
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

            if (key.Equals(EntityArchetype.Base))
            {
                return s_empty;
            }

            return new EntityGrouping(key, Array.Empty<EntityTable>());
        }

        /// <summary>
        /// Creates an <see cref="EntityGrouping"/> that contains the specified table and groups
        /// additional tables by the archetype that models entities in it.
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// An <see cref="EntityGrouping"/> that contains the specified table and groups additional
        /// tables by the archetype that models entities in it.
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
        /// Creates a copy of the <see cref="EntityGrouping"/> with the specified item added to it.
        /// </summary>
        /// <param name="item">
        /// The object to add to the copy of the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityGrouping"/> with <paramref name="item"/> added to it.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The archetype that models entities in <paramref name="item"/> does not match the key of
        /// the <see cref="EntityGrouping"/>.
        /// </exception>
        public EntityGrouping Add(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!m_key.Equals(item.Archetype))
            {
                ThrowForKeyMismatch();
            }

            ReadOnlySpan<EntityTable> source = new ReadOnlySpan<EntityTable>(m_items);
            EntityTable[] items = new EntityTable[source.Length + 1];
            Span<EntityTable> destination = new Span<EntityTable>(items);

            source.CopyTo(destination);
            destination[source.Length] = item;
            return new EntityGrouping(m_key, items);
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityGrouping"/> with all items removed from it.
        /// </summary>
        /// <returns>
        /// A copy of the <see cref="EntityGrouping"/> with all items removed from it, or the
        /// <see cref="EntityGrouping"/> if it is empty.
        /// </returns>
        public EntityGrouping Clear()
        {
            if (m_items.Length == 0)
            {
                return this;
            }

            if (m_key.Equals(EntityArchetype.Base))
            {
                return s_empty;
            }

            return new EntityGrouping(m_key, Array.Empty<EntityTable>());
        }

        /// <summary>
        /// Determines whether the <see cref="EntityGrouping"/> contains a specific value.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> is found in the
        /// <see cref="EntityGrouping"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(EntityTable item)
        {
            return Array.IndexOf(m_items, item) != -1;
        }

        /// <summary>
        /// Copies the elements of the <see cref="EntityGrouping"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        /// from the <see cref="EntityGrouping"/>. The <see cref="Array"/> must have zero-based
        /// indexing.
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
            Array.Copy(m_items, 0, array, arrayIndex, m_items.Length);
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityGrouping"/> with the specified item removed from
        /// it.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the copy of the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityGrouping"/> with <paramref name="item"/> removed from it,
        /// or the <see cref="EntityGrouping"/> if <paramref name="item"/> is not found in it.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The archetype that models entities in <paramref name="item"/> does not match the key of
        /// the <see cref="EntityGrouping"/>.
        /// </exception>
        public EntityGrouping Remove(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!m_key.Equals(item.Archetype))
            {
                ThrowForKeyMismatch();
            }

            int index = Array.IndexOf(m_items, item);

            if (index != -1)
            {
                return RemoveAt(index);
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

        /// <summary>
        /// Determines the index of a specific item in the <see cref="EntityGrouping"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the <see cref="EntityGrouping"/>;
        /// otherwise, -1.
        /// </returns>
        public int IndexOf(EntityTable item)
        {
            return Array.IndexOf(m_items, item);
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityGrouping"/> with the specified item inserted at
        /// the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item"/> should be inserted in the copy of
        /// the <see cref="EntityGrouping"/>.
        /// </param>
        /// <param name="item">
        /// The object to insert into the copy of the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityGrouping"/> with <paramref name="item"/> inserted at
        /// <paramref name="index"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The archetype that models entities in <paramref name="item"/> does not match the key of
        /// the <see cref="EntityGrouping"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> exceeds the bounds of the <see cref="EntityGrouping"/>.
        /// </exception>
        public EntityGrouping Insert(int index, EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!m_key.Equals(item.Archetype))
            {
                ThrowForKeyMismatch();
            }

            ReadOnlySpan<EntityTable> source = new ReadOnlySpan<EntityTable>(m_items);

            if ((uint)index > (uint)source.Length)
            {
                ThrowForIndexOutOfRange(index);
            }

            EntityTable[] items = new EntityTable[source.Length + 1];
            Span<EntityTable> destination = new Span<EntityTable>(items);

            source.Slice(0, index).CopyTo(destination);
            source.Slice(index).CopyTo(destination.Slice(index + 1));
            destination[index] = item;
            return new EntityGrouping(m_key, items);
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityGrouping"/> with the item at the specified index
        /// removed from it.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the item to remove from the copy of the
        /// <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityGrouping"/> with the item at <paramref name="index"/>
        /// removed from it.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> exceeds the bounds of the <see cref="EntityGrouping"/>.
        /// </exception>
        public EntityGrouping RemoveAt(int index)
        {
            ReadOnlySpan<EntityTable> source = new ReadOnlySpan<EntityTable>(m_items);

            if ((uint)index >= (uint)source.Length)
            {
                ThrowForIndexOutOfRange(index);
            }

            if (source.Length == 1)
            {
                if (m_key.Equals(EntityArchetype.Base))
                {
                    return s_empty;
                }

                return new EntityGrouping(m_key, Array.Empty<EntityTable>());
            }

            EntityTable[] items = new EntityTable[source.Length - 1];
            Span<EntityTable> destination = new Span<EntityTable>(items);

            source.Slice(0, index).CopyTo(destination);
            source.Slice(index + 1).CopyTo(destination.Slice(index));
            return new EntityGrouping(m_key, items);
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityGrouping"/> with the specified item set at the
        /// specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item"/> should be set in the copy of the
        /// <see cref="EntityGrouping"/>.
        /// </param>
        /// <param name="item">
        /// The object to set in the copy of the <see cref="EntityGrouping"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityGrouping"/> with <paramref name="item"/> set at
        /// <paramref name="index"/>, or the <see cref="EntityGrouping"/> if it contains
        /// <paramref name="item"/> at <paramref name="index"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The archetype that models entities in <paramref name="item"/> does not match the key of
        /// the <see cref="EntityGrouping"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> exceeds the bounds of the <see cref="EntityGrouping"/>.
        /// </exception>
        public EntityGrouping Set(int index, EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!m_key.Equals(item.Archetype))
            {
                ThrowForKeyMismatch();
            }

            ReadOnlySpan<EntityTable> source = new ReadOnlySpan<EntityTable>(m_items);

            if ((uint)index >= (uint)source.Length)
            {
                ThrowForIndexOutOfRange(index);
            }

            if (source[index] == item)
            {
                return this;
            }

            EntityTable[] items = source.ToArray();

            items[index] = item;
            return new EntityGrouping(m_key, items);
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
                Array.Copy(m_items, 0, array!, index, m_items.Length);
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

        void IList<EntityTable>.Insert(int index, EntityTable item)
        {
            throw new NotSupportedException();
        }

        void IList<EntityTable>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        int IList.Add(object? value)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object? value)
        {
            return Contains((value as EntityTable)!);
        }

        int IList.IndexOf(object? value)
        {
            return IndexOf((value as EntityTable)!);
        }

        void IList.Insert(int index, object? value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object? value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        [DoesNotReturn]
        private static void ThrowForKeyMismatch()
        {
            throw new ArgumentException(
                "The archetype that models entities in the table does not match the key of the " +
                "EntityGrouping.", "item");
        }

        [DoesNotReturn]
        private static void ThrowForIndexOutOfRange(int index)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, message: (index < 0)
                ? "The index is negative."
                : "The index exceeds the bounds of the EntityGrouping.");
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
            private readonly EntityTable[] m_items;
            private readonly int m_length;
            private int m_index;

            internal Enumerator(EntityGrouping grouping)
            {
                m_items = grouping.m_items;
                m_length = m_items.Length;
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
                get => m_items[m_index];
            }

            readonly object IEnumerator.Current
            {
                get => m_items[m_index];
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
