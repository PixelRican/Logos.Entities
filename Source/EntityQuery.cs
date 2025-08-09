// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Logos.Entities
{
    public class EntityQuery : IEnumerable<EntityTable>
    {
        private readonly EntityRegistry m_registry;
        private readonly EntityFilter m_filter;

        public EntityQuery(EntityRegistry registry, EntityFilter filter)
        {
            ArgumentNullException.ThrowIfNull(registry);
            ArgumentNullException.ThrowIfNull(filter);

            m_registry = registry;
            m_filter = filter;
        }

        public EntityRegistry Registry
        {
            get => m_registry;
        }

        public EntityFilter Filter
        {
            get => m_filter;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<EntityTable> IEnumerable<EntityTable>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<EntityTable>
        {
            private readonly EntityFilter m_filter;
            private EntityLookup.Enumerator m_lookupEnumerator;
            private EntityGrouping.Enumerator m_groupingEnumerator;

            internal Enumerator(EntityQuery query)
            {
                m_filter = query.m_filter;
                m_lookupEnumerator = query.m_registry.Lookup.GetEnumerator();
                m_groupingEnumerator = default;
            }

            public readonly EntityTable Current
            {
                get => m_groupingEnumerator.Current;
            }

            readonly object IEnumerator.Current
            {
                get => m_groupingEnumerator.Current;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                return m_groupingEnumerator.MoveNext() || MoveNextRare();
            }

            private bool MoveNextRare()
            {
                while (m_lookupEnumerator.MoveNext())
                {
                    EntityGrouping grouping = m_lookupEnumerator.Current;

                    if (m_filter.Matches(grouping.Key))
                    {
                        m_groupingEnumerator = grouping.GetEnumerator();
                        return true;
                    }
                }

                m_groupingEnumerator = default;
                return false;
            }

            public void Reset()
            {
                m_lookupEnumerator.Reset();
                m_groupingEnumerator = default;
            }
        }
    }
}
