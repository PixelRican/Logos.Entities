using Monophyll.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Monophyll.Entities
{
	public class EntityQuery : IReadOnlyCollection<EntityTable>
	{
		private const int DefaultCapacity = 4;

		private readonly object m_lock;
		private readonly EntityTableLookup m_lookup;
		private readonly EntityFilter m_filter;
		private EntityTableGrouping[] m_groupings;
		private int m_size;
		private int m_lookupIndex;

        public int Count
		{
			get
			{
				int count = 0;

				for (int i = 0; i < m_size; i++)
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
            ArgumentNullException.ThrowIfNull(lookup);
            ArgumentNullException.ThrowIfNull(filter);

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
                EntityTableLookup lookup = m_lookup;
                int index = m_lookupIndex;

				if (index < lookup.Count)
                {
                    int size = m_size;
					EntityTableGrouping[] groupings = m_groupings;
					EntityFilter filter = m_filter;

                    do
                    {
                        EntityTableGrouping grouping = lookup[index++];

                        if (filter.Matches(grouping.Key))
                        {
                            if (size == groupings.Length)
                            {
                                int newCapacity = groupings.Length == 0 ? DefaultCapacity : groupings.Length * 2;

                                if ((uint)newCapacity > (uint)Array.MaxLength)
                                {
                                    newCapacity = Array.MaxLength;
                                }

                                if (newCapacity <= size)
                                {
                                    newCapacity = size + 1;
                                }

                                Array.Resize(ref groupings, newCapacity);
                            }

                            groupings[size++] = grouping;
                        }
                    }
                    while (index < lookup.Count);

					m_groupings = groupings;
					m_size = size;
                    Volatile.Write(ref m_lookupIndex, index);
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
				m_count = query.m_size;
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
