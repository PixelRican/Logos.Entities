// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Monophyll.Entities
{
    /// <summary>
    /// Represents a collection of entity tables that share a common entity archetype.
    /// </summary>
    public class EntityTableGrouping : IGrouping<EntityArchetype, EntityTable>, IList<EntityTable>, IList, IReadOnlyList<EntityTable>
    {
        private readonly object m_lock;
        private readonly EntityArchetype m_key;
        private volatile EntityTable[] m_tables;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTableGrouping"/> class that groups
        /// entity tables by the specified entity archetype.
        /// </summary>
        /// 
        /// <param name="key">
        /// The entity archetype to group entity tables by.
        /// </param>
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

        bool IList.IsReadOnly
        {
            get => false;
        }

        bool IList.IsFixedSize
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

        public EntityTable this[int index]
        {
            get => m_tables[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                if (!EntityArchetype.Equals(m_key, value.Archetype))
                {
                    throw new ArgumentException(
                        "Value's archetype does not match the EntityTableGrouping's key.", nameof(value));
                }

                lock (m_lock)
                {
                    EntityTable[] tables = m_tables;

                    if ((uint)index >= (uint)tables.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), index,
                            "Index was out of range. Must be non-negative and less than the size of the EntityTableGrouping.");
                    }

                    if (tables[index] != value)
                    {
                        EntityTable[] array = new EntityTable[tables.Length];

                        Array.Copy(tables, array, tables.Length);
                        array[index] = value;
                        m_tables = array;
                    }
                }
            }
        }

        object? IList.this[int index]
        {
            get => m_tables[index];
            set
            {
                try
                {
                    this[index] = (EntityTable)value!;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Value is not of type EntityTable", nameof(value));
                }
            }
        }

        public void Add(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!EntityArchetype.Equals(m_key, item.Archetype))
            {
                throw new ArgumentException(
                    "Item's archetype does not match the EntityTableGrouping's key.", nameof(item));
            }

            lock (m_lock)
            {
                EntityTable[] tables = m_tables;
                EntityTable[] newTables = new EntityTable[tables.Length + 1];

                Array.Copy(tables, newTables, tables.Length);
                newTables[tables.Length] = item;
                m_tables = newTables;
            }
        }

        int IList.Add(object? value)
        {
            try
            {
                Add((EntityTable)value!);
                return m_tables.Length;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Value is not of type EntityTable", nameof(value));
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

        bool IList.Contains(object? value)
        {
            return Array.IndexOf(m_tables, value) != -1;
        }

        public void CopyTo(EntityTable[] array, int index)
        {
            EntityTable[] items = m_tables;
            Array.Copy(items, 0, array, index, items.Length);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
            {
                throw new ArgumentException("Multi-dimensional arrays are not supported.", nameof(array));
            }

            try
            {
                EntityTable[] items = m_tables;
                Array.Copy(items, 0, array!, index, items.Length);
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

        public int IndexOf(EntityTable item)
        {
            return Array.IndexOf(m_tables, item);
        }

        int IList.IndexOf(object? value)
        {
            return Array.IndexOf(m_tables, value);
        }

        public void Insert(int index, EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (!EntityArchetype.Equals(m_key, item.Archetype))
            {
                throw new ArgumentException(
                    "Item's archetype does not match the EntityTableGrouping's key.", nameof(item));
            }

            lock (m_lock)
            {
                EntityTable[] tables = m_tables;

                if ((uint)index > (uint)tables.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Index was out of range. Must be non-negative and less than or equal to the size of the EntityTableGrouping.");
                }

                EntityTable[] newTables = new EntityTable[tables.Length + 1];
                Array.Copy(tables, newTables, index);
                newTables[index] = item;

                if (index < tables.Length)
                {
                    Array.Copy(tables, index, newTables, index + 1, tables.Length - index);
                }

                m_tables = newTables;
            }
        }

        void IList.Insert(int index, object? value)
        {
            try
            {
                Insert(index, (EntityTable)value!);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Value is not of type EntityTable", nameof(value));
            }
        }

        public bool Remove(EntityTable item)
        {
            lock (m_lock)
            {
                EntityTable[] tables = m_tables;
                int index = Array.IndexOf(tables, item);

                if (index != -1)
                {
                    m_tables = RemoveAt(tables, index);
                    return true;
                }

                return false;
            }
        }

        void IList.Remove(object? value)
        {
            Remove((value as EntityTable)!);
        }

        public void RemoveAt(int index)
        {
            lock (m_lock)
            {
                EntityTable[] tables = m_tables;

                if ((uint)index >= (uint)tables.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Index was out of range. Must be non-negative and less than the size of the EntityTableGrouping.");
                }

                m_tables = RemoveAt(tables, index);
            }
        }

        private static EntityTable[] RemoveAt(EntityTable[] tables, int index)
        {
            if (tables.Length == 1)
            {
                return Array.Empty<EntityTable>();
            }

            EntityTable[] newTables = new EntityTable[tables.Length - 1];
            Array.Copy(tables, newTables, index);

            if (index < newTables.Length)
            {
                Array.Copy(tables, index + 1, newTables, index, newTables.Length - index);
            }

            return newTables;
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
