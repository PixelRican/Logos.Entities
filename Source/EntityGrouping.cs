// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Logos.Entities
{
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

        public EntityArchetype Key
        {
            get => m_key;
        }

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

        public static EntityGrouping Create(EntityArchetype key)
        {
            ArgumentNullException.ThrowIfNull(key);

            return new EntityGrouping(key, Array.Empty<EntityTable>());
        }

        public static EntityGrouping Create(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return new EntityGrouping(item.Archetype, new EntityTable[] { item });
        }

        public EntityGrouping Add(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EntityArchetype key = m_key;

            if (key.Equals(item.Archetype))
            {
                EntityTable[] values = m_values;
                int index = values.Length;

                Array.Resize(ref values, index + 1);
                values[index] = item;

                return new EntityGrouping(key, values);
            }

            return this;
        }

        public EntityGrouping Clear()
        {
            if (m_values.Length > 0)
            {
                return new EntityGrouping(m_key, Array.Empty<EntityTable>());
            }

            return this;
        }

        public bool Contains(EntityTable item)
        {
            return Array.IndexOf(m_values, item) != -1;
        }

        public void CopyTo(EntityTable[] array, int arrayIndex)
        {
            Array.Copy(m_values, 0, array, arrayIndex, m_values.Length);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public EntityGrouping Remove(EntityTable item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EntityArchetype key = m_key;

            if (key.Equals(item.Archetype))
            {
                EntityTable[] values = m_values;
                int index = Array.IndexOf(values, item);

                if (index != -1)
                {
                    if (values.Length == 1)
                    {
                        return new EntityGrouping(key, Array.Empty<EntityTable>());
                    }

                    EntityTable[] result = new EntityTable[values.Length - 1];
                    Array.Copy(values, result, index);

                    if (index < result.Length)
                    {
                        Array.Copy(values, index + 1, result, index, result.Length - index);
                    }

                    return new EntityGrouping(key, result);
                }
            }

            return this;
        }

        void ICollection<EntityTable>.Add(EntityTable item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<EntityTable>.Remove(EntityTable item)
        {
            throw new NotSupportedException();
        }

        void ICollection<EntityTable>.Clear()
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
            throw new ArgumentException(
                "The array is multidimensional.", "array");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayType()
        {
            throw new ArgumentException(
                "EntityTable cannot be cast automatically to the type of the " +
                "destination array.", "array");
        }

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

            public readonly EntityTable Current
            {
                get => m_values[m_index];
            }

            readonly object IEnumerator.Current
            {
                get => m_values[m_index];
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

            public void Reset()
            {
                m_index = -1;
            }
        }
    }
}
