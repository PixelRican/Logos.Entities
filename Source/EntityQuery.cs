// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a query that selects tables from a registry whose archetypes match a set of
    /// criteria defined by a predicate.
    /// </summary>
    public class EntityQuery : IEnumerable<EntityTable>
    {
        private readonly EntityPredicate m_predicate;
        private readonly EntityRegistry m_registry;
        private EntityGrouping[] m_cache;
        private EntityLookup m_lookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQuery"/> class that selects tables
        /// from the specified registry whose archetypes match a set of criteria defined by the
        /// specified predicate.
        /// </summary>
        /// <param name="predicate">
        /// The predicate that is used to determine whether a table from <paramref name="registry"/>
        /// will be selected by the <see cref="EntityQuery"/> based on its archetype.
        /// </param>
        /// <param name="registry">
        /// The registry that contains tables for the <see cref="EntityQuery"/> to select.
        /// </param>
        public EntityQuery(EntityPredicate predicate, EntityRegistry registry)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(registry);

            m_predicate = predicate;
            m_registry = registry;
            m_cache = Array.Empty<EntityGrouping>();
            m_lookup = EntityLookup.Empty;
        }

        /// <summary>
        /// Gets the predicate that is used to determine whether a table will be selected by the
        /// <see cref="EntityQuery"/> based on its archetype.
        /// </summary>
        /// <returns>
        /// The predicate that is used to determine whether a table will be selected by the
        /// <see cref="EntityQuery"/> based on its archetype.
        /// </returns>
        public EntityPredicate Predicate
        {
            get => m_predicate;
        }

        /// <summary>
        /// Gets the registry that contains tables for the <see cref="EntityQuery"/> to select.
        /// </summary>
        /// <returns>
        /// The registry that contains tables for the <see cref="EntityQuery"/> to select.
        /// </returns>
        public EntityRegistry Registry
        {
            get => m_registry;
        }

        /// <summary>
        /// Returns an <see cref="Enumerator"/> that iterates through the <see cref="EntityQuery"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Enumerator"/> that can be used to iterate through the
        /// <see cref="EntityQuery"/>.
        /// </returns>
        public Enumerator GetEnumerator()
        {
            EntityLookup lookup = m_registry.Lookup;

            if (lookup != m_lookup)
            {
                RebuildCache(lookup);
            }

            return new Enumerator(this);
        }

        IEnumerator<EntityTable> IEnumerable<EntityTable>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void RebuildCache(EntityLookup lookup)
        {
            EntityPredicate predicate = m_predicate;
            EntityGrouping[] array = ArrayPool<EntityGrouping>.Shared.Rent(lookup.Count);
            int count = 0;

            foreach (EntityGrouping grouping in lookup)
            {
                if (predicate.Matches(grouping.Key))
                {
                    array[count++] = grouping;
                }
            }

            m_cache = new ReadOnlySpan<EntityGrouping>(array, 0, count).ToArray();
            m_lookup = lookup;
            ArrayPool<EntityGrouping>.Shared.Return(array, clearArray: true);
        }

        /// <summary>
        /// Enumerates the elements of an <see cref="EntityQuery"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<EntityTable>
        {
            private readonly EntityGrouping[] m_cache;
            private readonly int m_length;
            private int m_index;
            private EntityGrouping.Enumerator m_enumerator;

            internal Enumerator(EntityQuery query)
            {
                m_cache = query.m_cache;
                m_length = m_cache.Length;
                m_index = -1;
                m_enumerator = default;
            }

            /// <summary>
            /// Gets the element in the <see cref="EntityQuery"/> at the current position of the
            /// <see cref="Enumerator"/>.
            /// </summary>
            /// <returns>
            /// The element in the <see cref="EntityQuery"/> at the current position of the
            /// <see cref="Enumerator"/>.
            /// </returns>
            public readonly EntityTable Current
            {
                get => m_enumerator.Current;
            }

            readonly object IEnumerator.Current
            {
                get => m_enumerator.Current;
            }

            /// <inheritdoc cref="IDisposable.Dispose"/>
            public readonly void Dispose()
            {
            }

            /// <summary>
            /// Advances the <see cref="Enumerator"/> to the next element of the
            /// <see cref="EntityQuery"/>.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the <see cref="Enumerator"/> was successfully advanced to
            /// the next element; <see langword="false"/> if the <see cref="Enumerator"/> has passed
            /// the end of the <see cref="EntityQuery"/>.
            /// </returns>
            public bool MoveNext()
            {
                if (m_enumerator.MoveNext())
                {
                    return true;
                }

                int index = m_index + 1;

                if (index < m_length)
                {
                    m_enumerator = m_cache[index].GetEnumerator();
                    m_index = index;
                    return true;
                }

                m_enumerator = default;
                return false;
            }

            /// <summary>
            /// Sets the <see cref="Enumerator"/> to its initial position, which is before the first
            /// element in the <see cref="EntityQuery"/>.
            /// </summary>
            public void Reset()
            {
                m_index = -1;
                m_enumerator = default;
            }
        }
    }
}
