using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
	public class EntityRegistry
	{
		private const int DefaultCapacity = 128;

		private readonly EntityArchetypeChunkLookup m_chunks;
		private Entry[] m_entries;
		private int[] m_freeEntityIds;
		private int m_nextEntityId;
		private int m_count;

		public EntityRegistry()
		{
			m_chunks = new EntityArchetypeChunkLookup();
			m_entries = Array.Empty<Entry>();
			m_freeEntityIds = Array.Empty<int>();
		}

		public int Capacity
		{
			get => m_entries.Length;
		}

		public int Count
		{
			get => m_count;
		}

		public Entity CreateEntity()
		{
			return CreateEntity(m_chunks.GetOrCreate(ReadOnlySpan<ComponentType>.Empty));
		}

		public Entity CreateEntity(EntityArchetype archetype)
		{
			return CreateEntity(m_chunks.GetOrCreate(archetype));
		}

		public Entity CreateEntity(ComponentType[] componentTypes)
		{
			return CreateEntity(m_chunks.GetOrCreate(componentTypes));
		}

		public Entity CreateEntity(IEnumerable<ComponentType> componentTypes)
		{
			return CreateEntity(m_chunks.GetOrCreate(componentTypes));
		}

		public Entity CreateEntity(ReadOnlySpan<ComponentType> componentTypes)
		{
			return CreateEntity(m_chunks.GetOrCreate(componentTypes));
		}

		private Entity CreateEntity(EntityArchetypeChunkGrouping grouping)
		{
			lock (m_chunks)
			{
				int entityId = m_count++ < m_nextEntityId ?
							   m_freeEntityIds[m_nextEntityId - m_count] :
							   m_nextEntityId++;

				if (entityId >= m_entries.Length)
				{
					int newCapacity = m_entries.Length == 0 ? DefaultCapacity : 2 * m_entries.Length;

					if ((uint)newCapacity > (uint)Array.MaxLength)
					{
						newCapacity = Array.MaxLength;
					}

					if (newCapacity < m_count)
					{
						newCapacity = m_count;
					}

					Array.Resize(ref m_entries, newCapacity);
				}

				ref Entry entry = ref m_entries[entityId];
				Entity entity = new Entity(entityId, entry.Version);

				if (!grouping.TryPeek(out EntityArchetypeChunk? chunk) || chunk.IsFull)
				{
					grouping.TryAdd(chunk = new EntityArchetypeChunk(grouping.Key));
				}

				entry.Chunk = chunk;
				entry.Index = chunk.Count;
				chunk.Push(entity);
				return entity;
			}
		}

		public bool DestroyEntity(Entity entity)
		{
			lock (m_chunks)
			{
				ref Entry entry = ref FindEntry(entity);

				if (Unsafe.IsNullRef(ref entry))
				{
					return false;
				}

				EntityArchetypeChunkGrouping grouping = m_chunks[entry.Chunk.Archetype.Id];
				grouping.TryPeek(out EntityArchetypeChunk? lastChunk);
				entry.Chunk.SetRange(entry.Index, lastChunk!, lastChunk!.Count - 1, 1);

				ref Entry lastEntry = ref m_entries[lastChunk!.GetEntities()[^1].Id];
				lastEntry.Chunk.Pop();
				lastEntry.Chunk = entry.Chunk;
				lastEntry.Index = entry.Index;

				if (lastChunk.IsEmpty)
				{
					grouping.TryTake(out _);
				}

				entry.Chunk = null!;
				entry.Index = 0;
				entry.Version++;

				int freeIndex = m_nextEntityId - m_count--;

				if (freeIndex >= m_freeEntityIds.Length)
				{
					int newCapacity = m_freeEntityIds.Length == 0 ? DefaultCapacity : 2 * m_freeEntityIds.Length;

					if ((uint)newCapacity > (uint)Array.MaxLength)
					{
						newCapacity = Array.MaxLength;
					}

					if (newCapacity <= freeIndex)
					{
						newCapacity = freeIndex + 1;
					}

					Array.Resize(ref m_freeEntityIds, newCapacity);
				}

				m_freeEntityIds[freeIndex] = entity.Id;
				return true;
			}
		}

		public bool HasEntity(Entity entity)
		{
			return !Unsafe.IsNullRef(ref FindEntry(entity));
		}

		public EntityQuery CreateEntityQuery(EntityFilter filter)
		{
			return new EntityQuery(m_chunks, filter);
		}

		public EntityArchetype GetEntityArchetype(ComponentType[] componentTypes)
		{
			return m_chunks.GetOrCreate(componentTypes).Key;
		}

		public EntityArchetype GetEntityArchetype(IEnumerable<ComponentType> componentTypes)
		{
			return m_chunks.GetOrCreate(componentTypes).Key;
		}

		public EntityArchetype GetEntityArchetype(ReadOnlySpan<ComponentType> componentTypes)
		{
			return m_chunks.GetOrCreate(componentTypes).Key;
		}

		public bool TryGetEntityArchetypeChunk(Entity entity, [MaybeNullWhen(false)] out EntityArchetypeChunk chunk)
		{
			ref Entry entry = ref FindEntry(entity);

			if (Unsafe.IsNullRef(ref entry))
			{
				chunk = null;
				return false;
			}

			chunk = entry.Chunk;
			return true;
		}

		private ref Entry FindEntry(Entity entity)
		{
			if ((uint)entity.Id < (uint)m_nextEntityId)
			{
				ref Entry entry = ref m_entries[entity.Id];

				if (entry.Chunk != null && entry.Version == entity.Version)
				{
					return ref entry;
				}
			}

			return ref Unsafe.NullRef<Entry>();
		}

		private struct Entry
		{
			public EntityArchetypeChunk Chunk;
			public int Index;
			public int Version;
		}
	}
}
