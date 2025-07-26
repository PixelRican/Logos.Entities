// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a query that searches through an <see cref="EntityTableLookup"/> and selects
    /// entity tables matched by an <see cref="EntityFilter"/>.
    /// </summary>
    public class EntityQuery : IEnumerable<EntityTable>
    {
        private readonly EntityTableLookup m_lookup;
        private readonly EntityFilter m_filter;
        private readonly Cache? m_cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQuery"/> class that selects all
        /// entity tables from the specified <see cref="EntityTableLookup"/>.
        /// </summary>
        /// 
        /// <param name="lookup">
        /// The <see cref="EntityTableLookup"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="lookup"/> is <see langword="null"/>.
        /// </exception>
        public EntityQuery(EntityTableLookup lookup)
            : this(lookup, EntityFilter.Universal, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQuery"/> class that searches through
        /// the specified <see cref="EntityTableLookup"/> and selects entity tables matched by the
        /// specified <see cref="EntityFilter"/>.
        /// </summary>
        /// 
        /// <param name="lookup">
        /// The <see cref="EntityTableLookup"/>.
        /// </param>
        /// 
        /// <param name="filter">
        /// The <see cref="EntityFilter"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="lookup"/> is <see langword="null"/> or <paramref name="filter"/> is
        /// <see langword="null"/>.
        /// </exception>
        public EntityQuery(EntityTableLookup lookup, EntityFilter filter)
            : this(lookup, filter, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQuery"/> class that selects all
        /// entity tables from the specified <see cref="EntityTableLookup"/> and, if enabled,
        /// stores them in a cache for faster iteration speeds.
        /// </summary>
        /// 
        /// <param name="lookup">
        /// The <see cref="EntityTableLookup"/>.
        /// </param>
        /// 
        /// <param name="enableCache">
        /// <see langword="true"/> to enable caching; <see langword="false"/> to disable caching.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="lookup"/> is <see langword="null"/>.
        /// </exception>
        public EntityQuery(EntityTableLookup lookup, bool enableCache)
            : this(lookup, EntityFilter.Universal, enableCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQuery"/> class that searches through
        /// the specified <see cref="EntityTableLookup"/>, selects entity tables matched by the
        /// specified <see cref="EntityFilter"/>, and, if enabled, stores them in a cache for
        /// faster iteration speeds. 
        /// </summary>
        /// 
        /// <param name="lookup">
        /// The <see cref="EntityTableLookup"/>.
        /// </param>
        /// 
        /// <param name="filter">
        /// The <see cref="EntityFilter"/>.
        /// </param>
        /// 
        /// <param name="enableCache">
        /// <see langword="true"/> to enable caching; <see langword="false"/> to disable caching.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="lookup"/> is <see langword="null"/> or <paramref name="filter"/> is
        /// <see langword="null"/>.
        /// </exception>
        public EntityQuery(EntityTableLookup lookup, EntityFilter filter, bool enableCache)
        {
            ArgumentNullException.ThrowIfNull(lookup);
            ArgumentNullException.ThrowIfNull(filter);

            m_lookup = lookup;
            m_filter = filter;

            if (enableCache)
            {
                m_cache = new Cache();
            }
        }

        /// <summary>
        /// Gets the <see cref="EntityFilter"/> used by the <see cref="EntityQuery"/> to match and
        /// select entity tables.
        /// </summary>
        public EntityFilter Filter
        {
            get => m_filter;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityQuery"/> caches its selected
        /// entity tables.
        /// </summary>
        public bool IsCacheEnabled
        {
            get => m_cache != null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="EntityQuery"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An enumerator that can be used to iterate through the <see cref="EntityQuery"/>.
        /// </returns>
        public Enumerator GetEnumerator()
        {
            if (m_cache == null)
            {
                return new Enumerator(this, m_lookup.Count);
            }

            if (m_cache.PreviousLookupCount < m_lookup.Count)
            {
                lock (m_cache)
                {
                    m_cache.Select(m_lookup, m_filter);
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
        /// Enumerates through the elements of the <see cref="EntityQuery"/>.
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

            private EntityTableGrouping[] m_results;
            private int m_size;
            private int m_previousLookupCount;

            public Cache()
            {
                m_results = Array.Empty<EntityTableGrouping>();
            }

            public EntityTableGrouping this[int index]
            {
                get => m_results[index];
            }

            public int Count
            {
                get => m_size;
            }

            public int PreviousLookupCount
            {
                get => m_previousLookupCount;
            }

            public void Select(EntityTableLookup lookup, EntityFilter filter)
            {
                while (m_previousLookupCount < lookup.Count)
                {
                    EntityTableGrouping grouping = lookup[m_previousLookupCount++];

                    if (filter.Matches(grouping.Key))
                    {
                        if (m_size >= m_results.Length)
                        {
                            int newCapacity = m_results.Length * 2;

                            if (newCapacity == 0)
                            {
                                newCapacity = DefaultCapacity;
                            }
                            else if ((uint)newCapacity > (uint)Array.MaxLength)
                            {
                                newCapacity = Math.Max(Array.MaxLength, m_size + 1);
                            }

                            Array.Resize(ref m_results, newCapacity);
                        }

                        m_results[m_size++] = grouping;
                    }
                }
            }
        }
    }
}
