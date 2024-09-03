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
			m_chunks.GetOrCreate(EntityArchetype.Base);
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
			return CreateEntity(EntityArchetype.Base);
		}

		public Entity CreateEntity(EntityArchetype archetype)
		{
			return CreateEntity(m_chunks.GetOrCreate(archetype));
		}

		public Entity CreateEntity(params ComponentType[] componentTypes)
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
					grouping.TryAdd(chunk = new EntityRegistryChunk(grouping.Key));
				}

				entry.Chunk = (EntityRegistryChunk)chunk;
				entry.Index = chunk.Count;
				entry.Chunk.Push(entity);
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

				m_chunks[entry.Chunk.Archetype].TryPeek(out EntityArchetypeChunk? chunkToPop);
				entry.Chunk.SetRange(entry.Index, chunkToPop!, 1);

				ref Entry entryToPop = ref m_entries[chunkToPop![^1].Id];
				entryToPop.Chunk.Pop();
				entryToPop.Chunk = entry.Chunk;
				entryToPop.Index = entry.Index;

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

					if (newCapacity < m_count)
					{
						newCapacity = m_count;
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

		public EntityArchetype GetEntityArchetype(params ComponentType[] componentTypes)
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

		public bool AddComponent(Entity entity, ComponentType componentType)
		{
			throw new NotImplementedException();
		}

		public bool AddComponent<T>(Entity entity, T component) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		public bool RemoveComponent(Entity entity, ComponentType componentType)
		{
			throw new NotImplementedException();
		}

		public bool RemoveComponent<T>(Entity entity, out T component) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		public ref T GetComponent<T>(Entity entity) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		public bool TryGetComponent<T>(Entity entity, out T component) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		private struct Entry
		{
			public EntityRegistryChunk Chunk;
			public int Index;
			public int Version;
		}

		private sealed class EntityRegistryChunk : EntityArchetypeChunk
		{
			public EntityRegistryChunk(EntityArchetype archetype) : base(archetype)
			{
			}

			public void Push(Entity entity)
			{
				base.InsertEntity(Count, entity);
			}

			public void Pop()
			{
				base.RemoveEntity(Count - 1);
			}

			public void SetRange(int index, EntityArchetypeChunk chunk, int length)
			{
				base.SetEntities(index, chunk, chunk.Count - length, length);
			}

			protected override void ClearEntities()
			{
				ThrowNotSupportedException();
			}

			protected override void InsertEntities(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
			{
				ThrowNotSupportedException();
			}

			protected override void InsertEntity(int index, Entity entity)
			{
				ThrowNotSupportedException();
			}

			protected override void RemoveEntity(int index)
			{
				ThrowNotSupportedException();
			}

			protected override void SetEntities(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
			{
				ThrowNotSupportedException();
			}

			protected override void SetEntity(int index, Entity entity)
			{
				ThrowNotSupportedException();
			}

			private static void ThrowNotSupportedException()
			{
				throw new NotSupportedException(
					"EntityArchetypeChunks created by an EntityRegistry cannot be modified directly.");
			}
		}
	}
}
