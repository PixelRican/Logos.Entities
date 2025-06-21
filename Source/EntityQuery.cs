// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Monophyll.Entities
{
    /// <summary>
    /// Represents a query that matches entity tables based on their entity archetypes.
    /// </summary>
    public class EntityQuery : IEnumerable<EntityTable>
    {
        private readonly EntityTableLookup m_lookup;
        private readonly EntityFilter m_filter;
        private readonly Cache? m_cache;

        /// <summary>
        /// Initializes an uncached entity query that matches all entity tables from the specified
        /// entity table lookup.
        /// </summary>
        /// 
        /// <param name="lookup">
        /// The entity table lookup.
        /// </param>
        public EntityQuery(EntityTableLookup lookup)
            : this(lookup, EntityFilter.Universal, false)
        {
        }

        /// <summary>
        /// Initializes an entity query that matches all entity tables from the specified entity
        /// table lookup, and optionally caches the results.
        /// </summary>
        /// 
        /// <param name="lookup">
        /// The entity table lookup.
        /// </param>
        /// 
        /// <param name="cached">
        /// <see langword="true"/> to enable caching; <see langword="false"/> to disable caching.
        /// </param>
        public EntityQuery(EntityTableLookup lookup, bool cached)
            : this(lookup, EntityFilter.Universal, cached)
        {
        }

        /// <summary>
        /// Initializes an uncached entity query that matches entity tables from the specified
        /// entity table lookup using the specified entity filter.
        /// </summary>
        /// 
        /// <param name="lookup">
        /// The entity table lookup.
        /// </param>
        /// 
        /// <param name="filter">
        /// The entity filter.
        /// </param>
        public EntityQuery(EntityTableLookup lookup, EntityFilter? filter)
            : this(lookup, filter, false)
        {
        }

        /// <summary>
        /// Initializes an entity query that matches entity tables from the specified entity table
        /// lookup using the specified entity filter, and optionally caches the results.
        /// </summary>
        /// 
        /// <param name="lookup">
        /// The entity table lookup.
        /// </param>
        /// 
        /// <param name="filter">
        /// The entity filter.
        /// </param>
        /// 
        /// <param name="cached">
        /// <see langword="true"/> to enable caching; <see langword="false"/> to disable caching.
        /// </param>
        public EntityQuery(EntityTableLookup lookup, EntityFilter? filter, bool cached)
        {
            ArgumentNullException.ThrowIfNull(lookup);

            m_lookup = lookup;
            m_filter = filter ?? EntityFilter.Universal;

            if (cached)
            {
                m_cache = new Cache();
                m_cache.Refresh(m_lookup, m_filter);
            }
        }

        /// <summary>
        /// Gets the entity filter used by the entity query used to match entity tables based on
        /// their entity archetypes.
        /// </summary>
        public EntityFilter Filter
        {
            get => m_filter;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the entity query.
        /// </summary>
        /// 
        /// <returns>
        /// An enumerator that can be used to iterate through the entity query.
        /// </returns>
        public Enumerator GetEnumerator()
        {
            if (m_cache == null)
            {
                return new Enumerator(this, m_lookup.Count);
            }

            if (m_lookup.Count > m_cache.Version)
            {
                lock (m_cache)
                {
                    m_cache.Refresh(m_lookup, m_filter);
                }
            }

            return new Enumerator(this, m_cache.Count);
        }

        IEnumerator<EntityTable> IEnumerable<EntityTable>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerates through the elements of the entity query.
        /// </summary>
        public struct Enumerator : IEnumerator<EntityTable>
        {
            private readonly EntityQuery m_query;
            private readonly int m_count;
            private int m_index;
            private EntityTableGrouping.Enumerator m_enumerator;

            internal Enumerator(EntityQuery query, int count)
            {
                m_query = query;
                m_count = count;
                m_index = 0;
                m_enumerator = default;
            }

            public readonly EntityTable Current
            {
                get => m_enumerator.Current;
            }

            readonly object IEnumerator.Current
            {
                get => m_enumerator.Current;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                return m_enumerator.MoveNext() || MoveNextRare();
            }

            private bool MoveNextRare()
            {
                Cache? cache = m_query.m_cache;

                if (cache != null)
                {
                    while (m_index < m_count)
                    {
                        m_enumerator = cache[m_index++].GetEnumerator();

                        if (m_enumerator.MoveNext())
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    EntityTableLookup lookup = m_query.m_lookup;
                    EntityFilter filter = m_query.m_filter;

                    while (m_index < m_count)
                    {
                        EntityTableGrouping grouping = lookup[m_index++];

                        if (filter.Matches(grouping.Key))
                        {
                            m_enumerator = grouping.GetEnumerator();

                            if (m_enumerator.MoveNext())
                            {
                                return true;
                            }
                        }
                    }
                }

                m_enumerator = default;
                return false;
            }

            void IEnumerator.Reset()
            {
                m_index = 0;
                m_enumerator = default;
            }
        }

        private sealed class Cache
        {
            private const int DefaultCapacity = 4;

            private EntityTableGrouping[] m_items;
            private int m_size;
            private int m_version;

            public Cache()
            {
                m_items = Array.Empty<EntityTableGrouping>();
            }

            public EntityTableGrouping this[int index]
            {
                get => m_items[index];
            }

            public int Count
            {
                get => m_size;
            }

            public int Version
            {
                get => m_version;
            }

            public void Refresh(EntityTableLookup lookup, EntityFilter filter)
            {
                while (lookup.Count > m_version)
                {
                    EntityTableGrouping grouping = lookup[m_version++];

                    if (filter.Matches(grouping.Key))
                    {
                        if (m_size == m_items.Length)
                        {
                            int newCapacity = m_items.Length == 0 ? DefaultCapacity : m_items.Length * 2;

                            if ((uint)newCapacity > (uint)Array.MaxLength)
                            {
                                newCapacity = Array.MaxLength;
                            }

                            if (newCapacity <= m_size)
                            {
                                newCapacity = m_size + 1;
                            }

                            Array.Resize(ref m_items, newCapacity);
                        }

                        m_items[m_size++] = grouping;
                    }
                }
            }
        }
    }
}
