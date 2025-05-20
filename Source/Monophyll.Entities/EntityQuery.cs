using Monophyll.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Monophyll.Entities
{
	public sealed class EntityQuery : IReadOnlyCollection<EntityTable>
	{
		private const int DefaultCapacity = 4;

		private readonly object m_lock;
		private readonly EntityTableLookup m_lookup;
		private readonly EntityFilter m_filter;
		private EntityTableGrouping[] m_groupings;
		private int m_count;
		private int m_lookupIndex;

        public int Count
		{
			get
			{
				int count = 0;

				for (int i = 0; i < m_count; i++)
				{
					count += m_groupings[i].Count;
				}

				return count;
			}
		}

        public EntityQuery(EntityTableLookup lookup) : this(lookup, EntityFilter.Universal)
		{
		}

		public EntityQuery(EntityTableLookup lookup, EntityFilter filter)
		{
			if (lookup == null)
			{
				throw new ArgumentNullException(nameof(lookup));
			}

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            m_lock = new object();
			m_lookup = lookup;
			m_filter = filter;
			m_groupings = Array.Empty<EntityTableGrouping>();
		}

		public Enumerator GetEnumerator()
		{
			if (Volatile.Read(ref m_lookupIndex) < m_lookup.Count)
			{
				UpdateCache();
			}

			return new Enumerator(this);
		}

		private void UpdateCache()
		{
			lock (m_lock)
			{
                int lookupIndex = m_lookupIndex;

				if (lookupIndex < m_lookup.Count)
                {
                    int count = m_count;

                    do
                    {
                        EntityTableGrouping grouping = m_lookup[lookupIndex++];

                        if (m_filter.Matches(grouping.Key))
                        {
                            if (count == m_groupings.Length)
                            {
                                int newCapacity = m_groupings.Length == 0 ? DefaultCapacity : m_groupings.Length * 2;

                                if ((uint)newCapacity > (uint)Array.MaxLength)
                                {
                                    newCapacity = Array.MaxLength;
                                }

                                if (newCapacity >= count)
                                {
                                    newCapacity = count + 1;
                                }

                                Array.Resize(ref m_groupings, newCapacity);
                            }

                            m_groupings[count++] = grouping;
                        }
                    }
                    while (lookupIndex < m_lookup.Count);

					m_count = count;
                    Volatile.Write(ref m_lookupIndex, lookupIndex);
                }
            }
		}

		IEnumerator<EntityTable> IEnumerable<EntityTable>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public struct Enumerator : IEnumerator<EntityTable>
		{
			private readonly EntityQuery m_query;
			private readonly int m_count;
			private int m_index;
			private ArrayEnumerator<EntityTable> m_enumerator;

			internal Enumerator(EntityQuery query)
			{
				m_query = query;
				m_count = query.m_count;
				m_index = 0;
				m_enumerator = ArrayEnumerator<EntityTable>.Empty;
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
				while (m_index < m_count)
				{
					m_enumerator = m_query.m_groupings[m_index++].GetEnumerator();

					if (m_enumerator.MoveNext())
					{
						return true;
					}
				}

				m_enumerator = ArrayEnumerator<EntityTable>.Empty;
				return false;
			}

			void IEnumerator.Reset()
			{
				m_index = 0;
				m_enumerator = ArrayEnumerator<EntityTable>.Empty;
			}
		}
	}
}
