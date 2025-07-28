// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a collection of entity tables whose entities are modeled by a common
    /// <see cref="EntityArchetype"/>.
    /// </summary>
    public class EntityTableGrouping : IGrouping<EntityArchetype, EntityTable>, ICollection<EntityTable>, ICollection, IReadOnlyCollection<EntityTable>
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

        public EntityArchetype Key
        {
            get => m_key;
        }

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

        public void Clear()
        {
            lock (m_lock)
            {
                m_tables = Array.Empty<EntityTable>();
            }
        }

        public bool Contains(EntityTable item)
        {
            return Array.IndexOf(m_tables, item) != -1;
        }

        public void CopyTo(EntityTable[] array, int index)
        {
            EntityTable[] tables = m_tables;
            Array.Copy(tables, 0, array, index, tables.Length);
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
        /// Returns an enumerator that iterates through the <see cref="EntityTableGrouping"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An enumerator that can be used to iterate through the
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

        public bool Remove(EntityTable item)
        {
            if (item == null)
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

            public readonly EntityTable Current
            {
                get => m_tables[m_index];
            }

            readonly object IEnumerator.Current
            {
                get => m_tables[m_index]!;
            }

            public readonly void Dispose()
            {
            }

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

            void IEnumerator.Reset()
            {
                if (m_tables != null)
                {
                    m_index = -1;
                }
            }
        }
    }
}
