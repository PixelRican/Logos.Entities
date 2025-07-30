// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a thread-safe collection of entity tables whose entities are modelled by a
    /// common <see cref="EntityArchetype"/>.
    /// </summary>
    public class EntityTableGrouping : IGrouping<EntityArchetype, EntityTable>,
        ICollection<EntityTable>, ICollection, IReadOnlyCollection<EntityTable>
    {
        private readonly object m_lock;
        private readonly EntityArchetype m_key;
        private volatile EntityTable[] m_tables;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTableGrouping"/> class that stores
        /// entity tables whose entities are modeled by the specified
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        /// 
        /// <param name="key">
        /// The <see cref="EntityArchetype"/> that models the entity tables stored in the
        /// <see cref="EntityTableGrouping"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public EntityTableGrouping(EntityArchetype key)
        {
            ArgumentNullException.ThrowIfNull(key);

            m_lock = new object();
            m_key = key;
            m_tables = Array.Empty<EntityTable>();
        }

        /// <summary>
        /// Gets the key of the <see cref="EntityTableGrouping"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The key of the <see cref="EntityTableGrouping"/>.
        /// </returns>
        public EntityArchetype Key
        {
            get => m_key;
        }

        /// <summary>
        /// Gets the number of entity tables contained in the <see cref="EntityTableGrouping"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The number of entity tables contained in the <see cref="EntityTableGrouping"/>.
        /// </returns>
        public int Count
        {
            get => m_tables.Length;
        }

        bool ICollection<EntityTable>.IsReadOnly
        {
            get => false;
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
        /// Adds an <see cref="EntityTable"/> to the <see cref="EntityTableGrouping"/>.
        /// </summary>
        /// 
        /// <param name="item">
        /// The <see cref="EntityTable"/> to add to the <see cref="EntityTableGrouping"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// The <see cref="EntityArchetype"/> that models entities stored by
        /// <paramref name="item"/> does not match <see cref="Key"/>.
        /// </exception>
        public void Add(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!m_key.Equals(item.Archetype))
            {
                throw new ArgumentException("The EntityArchetype that models entities stored by " +
                    "item does not match Key.", nameof(item));
            }

            lock (m_lock)
            {
                EntityTable[] tables = m_tables;
                int size = tables.Length;

                Array.Resize(ref tables, size + 1);
                tables[size] = item;
                m_tables = tables;
            }
        }

        /// <summary>
        /// Removes all entity tables from the <see cref="EntityTableGrouping"/>.
        /// </summary>
        public void Clear()
        {
            lock (m_lock)
            {
                m_tables = Array.Empty<EntityTable>();
            }
        }

        /// <summary>
        /// Determines whether the <see cref="EntityTableGrouping"/> contains a specific
        /// <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="item">
        /// The <see cref="EntityTable"/> to locate in the <see cref="EntityTableGrouping"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> is found in the
        /// <see cref="EntityTableGrouping"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(EntityTable item)
        {
            return Array.IndexOf(m_tables, item) != -1;
        }

        /// <summary>
        /// Copies the elements of the <see cref="EntityTableGrouping"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// 
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        /// from <see cref="EntityTableGrouping"/>. The <see cref="Array"/> must have zero-based
        /// indexing.
        /// </param>
        /// 
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        public void CopyTo(EntityTable[] array, int arrayIndex)
        {
            EntityTable[] tables = m_tables;
            Array.Copy(tables, 0, array, arrayIndex, tables.Length);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
            {
                throw new ArgumentException(
                    "Multi-dimensional arrays are not supported.", nameof(array));
            }

            try
            {
                EntityTable[] tables = m_tables;
                Array.Copy(tables, 0, array!, index, tables.Length);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException("Array is not of type EntityTable[].", nameof(array));
            }
        }

        /// <summary>
        /// Returns an <see cref="Enumerator"/> that iterates through the
        /// <see cref="EntityTableGrouping"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="Enumerator"/> that can be used to iterate through the
        /// <see cref="EntityTableGrouping"/>.
        /// </returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_tables);
        }

        IEnumerator<EntityTable> IEnumerable<EntityTable>.GetEnumerator()
        {
            return new Enumerator(m_tables);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(m_tables);
        }

        /// <summary>
        /// Removes the first occurrence of a specific <see cref="EntityTable"/> from the
        /// <see cref="EntityTableGrouping"/>.
        /// </summary>
        /// 
        /// <param name="item">
        /// The <see cref="EntityTable"/> to remove from the <see cref="EntityTableGrouping"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the
        /// <see cref="EntityTableGrouping"/>; otherwise, <see langword="false"/>. This method also
        /// returns <see langword="false"/> if <paramref name="item"/> is not found in the original
        /// <see cref="EntityTableGrouping"/>.
        /// </returns>
        public bool Remove(EntityTable item)
        {
            if (item == null || !m_key.Equals(item.Archetype))
            {
                return false;
            }

            lock (m_lock)
            {
                EntityTable[] tables = m_tables;
                int index = Array.IndexOf(tables, item);

                if (index == -1)
                {
                    return false;
                }

                if (tables.Length > 1)
                {
                    EntityTable[] array = new EntityTable[tables.Length - 1];
                    Array.Copy(tables, array, index);

                    if (index < array.Length)
                    {
                        Array.Copy(tables, index + 1, array, index, array.Length - index);
                    }

                    m_tables = array;
                }
                else
                {
                    m_tables = Array.Empty<EntityTable>();
                }

                return true;
            }
        }

        /// <summary>
        /// Enumerates through the elements of the <see cref="EntityTableGrouping"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<EntityTable>
        {
            private readonly EntityTable[] m_tables;
            private readonly int m_length;
            private int m_index;

            internal Enumerator(EntityTable[] tables)
            {
                m_tables = tables;
                m_length = tables.Length;
                m_index = -1;
            }

            /// <summary>
            /// Gets the <see cref="EntityTable"/> in the <see cref="EntityTableGrouping"/> at the
            /// current position of the <see cref="Enumerator"/>.
            /// </summary>
            /// 
            /// <returns>
            /// The <see cref="EntityTable"/> in the <see cref="EntityTableGrouping"/> at the
            /// current position of the <see cref="Enumerator"/>.
            /// </returns>
            public readonly EntityTable Current
            {
                get => m_tables[m_index];
            }

            readonly object IEnumerator.Current
            {
                get => m_tables[m_index];
            }

            /// <inheritdoc cref="IDisposable.Dispose"/>
            public readonly void Dispose()
            {
            }

            /// <summary>
            /// Advances the <see cref="Enumerator"/> to the next element of the
            /// <see cref="EntityTableGrouping"/>.
            /// </summary>
            /// 
            /// <returns>
            /// <see langword="true"/> if the <see cref="Enumerator"/> was successfully advanced to
            /// the next element; <see langword="false"/> if the <see cref="Enumerator"/> has
            /// passed the end of the <see cref="EntityTableGrouping"/>.
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
            /// Sets the <see cref="Enumerator"/> to its initial position, which is before the
            /// first element in the <see cref="EntityTableGrouping"/>.
            /// </summary>
            public void Reset()
            {
                m_index = -1;
            }
        }
    }
}
