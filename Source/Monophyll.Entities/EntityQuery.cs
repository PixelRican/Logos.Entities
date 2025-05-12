using Monophyll.Entities.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Monophyll.Entities
{
	public sealed class EntityQuery : IEnumerable<EntityArchetypeChunk>
	{
		private const int DefaultCapacity = 4;

		private readonly object m_lock;
		private readonly EntityArchetypeLookup m_lookup;
		private readonly EntityFilter m_filter;
		private EntityArchetypeGrouping[] m_groupings;
		private int m_count;
		private int m_lookupIndex;

		public EntityQuery(EntityArchetypeLookup lookup) : this(lookup, EntityFilter.Universal)
		{
		}

		public EntityQuery(EntityArchetypeLookup lookup, EntityFilter filter)
		{
			ArgumentNullException.ThrowIfNull(lookup);
			ArgumentNullException.ThrowIfNull(filter);

			m_lock = new object();
			m_lookup = lookup;
			m_filter = filter;
			m_groupings = Array.Empty<EntityArchetypeGrouping>();
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
				if (m_lookupIndex < m_lookup.Count)
				{
					int lookupIndex = m_lookupIndex;

					do
					{
						EntityArchetypeGrouping grouping = m_lookup[lookupIndex++];

						if (m_filter.Matches(grouping.Key))
						{
							if (m_count == m_groupings.Length)
							{
								int newCapacity = m_groupings.Length == 0 ? DefaultCapacity : m_groupings.Length * 2;

								if ((uint)newCapacity > (uint)Array.MaxLength)
								{
									newCapacity = Array.MaxLength;
								}

								if (newCapacity >= m_count)
								{
									newCapacity = m_count + 1;
								}

								Array.Resize(ref m_groupings, newCapacity);
							}

							m_groupings[m_count++] = grouping;
						}
					}
					while (lookupIndex < m_lookup.Count);

					Volatile.Write(ref m_lookupIndex, lookupIndex);
				}
			}
		}

		IEnumerator<EntityArchetypeChunk> IEnumerable<EntityArchetypeChunk>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public struct Enumerator : IEnumerator<EntityArchetypeChunk>
		{
			private readonly EntityQuery m_query;
			private readonly int m_count;
			private int m_index;
			private ArrayEnumerator<EntityArchetypeChunk> m_enumerator;

			internal Enumerator(EntityQuery query)
			{
				m_query = query;
				m_count = query.m_count;
				m_index = 0;
				m_enumerator = ArrayEnumerator<EntityArchetypeChunk>.Empty;
			}

			public readonly EntityArchetypeChunk Current
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

				m_enumerator = ArrayEnumerator<EntityArchetypeChunk>.Empty;
				return false;
			}

			void IEnumerator.Reset()
			{
				m_index = 0;
				m_enumerator = ArrayEnumerator<EntityArchetypeChunk>.Empty;
			}
		}
	}
}
