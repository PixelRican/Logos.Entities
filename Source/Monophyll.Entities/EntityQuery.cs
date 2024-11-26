using System;
using System.Diagnostics;

namespace Monophyll.Entities
{
	public sealed class EntityQuery
	{
		private readonly Cache m_cache;
		private EntityArchetypeChunkGrouping.Enumerator m_groupingEnumerator;
		private int m_count;
		private int m_index;

		public EntityQuery(EntityArchetypeChunkLookup lookup, EntityFilter filter)
		{
			ArgumentNullException.ThrowIfNull(lookup);
			ArgumentNullException.ThrowIfNull(filter);

			m_cache = new Cache(lookup, filter);
		}

		public bool Matches(EntityArchetype archetype)
		{
			return m_cache.Contains(archetype);
		}

		public bool MoveNext()
		{
			if (m_count == 0)
			{
				m_count = m_cache.Refresh();
			}

			while (!m_groupingEnumerator.MoveNext())
			{
				if ((uint)m_index >= (uint)m_count)
				{
					m_groupingEnumerator = default;
					m_index = m_count = 0;
					return false;
				}

				m_groupingEnumerator = m_cache.Items[m_index++].GetEnumerator();
			}

			return true;
		}

		public void Rematch()
		{
			m_count = m_cache.Refresh();
		}

		public void Reset()
		{
			m_groupingEnumerator = default;
			m_index = m_count = 0;
		}

		public Span<T> GetComponents<T>()
		{
			return GetEntityArchetypeChunk().GetComponents<T>();
		}

		public ref T GetComponentReference<T>()
		{
			return ref GetEntityArchetypeChunk().GetComponentReference<T>();
		}

		public ReadOnlySpan<Entity> GetEntities()
		{
			return GetEntityArchetypeChunk().GetEntities();
		}

		private EntityArchetypeChunk GetEntityArchetypeChunk()
		{
			return m_groupingEnumerator.Current ?? throw new InvalidCastException("The EntityQuery has not yet begun iteration.");
		}

		private sealed class Cache
		{
			private const int DefaultCapacity = 16;

			private readonly EntityArchetypeChunkLookup m_lookup;
			private readonly EntityFilter m_filter;
			private EntityArchetypeChunkGrouping[] m_items;
			private int m_size;
			private int m_lookupIndex;

			public Cache(EntityArchetypeChunkLookup lookup, EntityFilter filter)
			{
				Debug.Assert(lookup != null);
				Debug.Assert(filter != null);

				m_lookup = lookup;
				m_filter = filter;
				m_items = Array.Empty<EntityArchetypeChunkGrouping>();
			}

			public EntityArchetypeChunkGrouping[] Items
			{
				get => m_items;
			}

			public bool Contains(EntityArchetype archetype)
			{
				return m_lookup.Contains(archetype) && m_filter.Matches(archetype);
			}

			public int Refresh()
			{
				if (m_lookupIndex == m_lookup.Count)
				{
					return m_size;
				}

				lock (this)
				{
					while (m_lookupIndex < m_lookup.Count)
					{
						if (m_size >= m_items.Length)
						{
							int newCapacity = m_items.Length == 0 ? DefaultCapacity : 2 * m_items.Length;

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

						EntityArchetypeChunkGrouping grouping = m_lookup[m_lookupIndex++];

						if (m_filter.Matches(grouping.Key))
						{
							m_items[m_size++] = grouping;
						}
					}

					return m_size;
				}
			}
		}
	}
}
