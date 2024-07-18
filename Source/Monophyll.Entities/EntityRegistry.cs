using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
	public class EntityRegistry
	{
		private const int InitialEntityGroupCapacity = 32;
		private const int InitialEntityRecordCapacity = 128;

		private readonly Dictionary<ReadOnlyMemory<uint>, EntityArchetypeChunkGroup> m_componentBitsToEntityGroups;
		private readonly Queue<int> m_freeEntityIds;
		private EntityArchetypeChunkGroup[] m_entityGroups;
		private EntityRecord[] m_entityRecords;
		private int m_nextEntityId;

		public EntityRegistry()
		{
			m_componentBitsToEntityGroups = new Dictionary<ReadOnlyMemory<uint>, EntityArchetypeChunkGroup>(InitialEntityGroupCapacity, ComponentBitEqualityComparer.Instance);
			m_entityGroups = new EntityArchetypeChunkGroup[InitialEntityGroupCapacity];
			m_freeEntityIds = new Queue<int>(InitialEntityRecordCapacity);
			m_entityRecords = new EntityRecord[InitialEntityRecordCapacity];
			m_componentBitsToEntityGroups.Add(ReadOnlyMemory<uint>.Empty, m_entityGroups[0] = new EntityArchetypeChunkGroup(EntityArchetype.Base));
			m_nextEntityId = -1;
		}

		public event EventHandler<Entity>? EntityCreated;

		public event EventHandler<Entity>? EntityDestroyed;

		public Entity CreateEntity()
		{
			return CreateEntity(m_entityGroups[0]);
		}

		public Entity CreateEntity(ReadOnlySpan<ComponentType> componentTypes)
		{
			return CreateEntity(GetOrCreateEntityGroup(componentTypes));
		}

		public Entity CreateEntity(EntityArchetype archetype)
		{
			return CreateEntity(GetOrCreateEntityGroup(archetype));
		}

		private Entity CreateEntity(EntityArchetypeChunkGroup group)
		{
			Entity entity;

			lock (m_freeEntityIds)
			{
				if (!m_freeEntityIds.TryDequeue(out int entityId))
				{
					entityId = Interlocked.Increment(ref m_nextEntityId);

					if (entityId >= m_entityRecords.Length)
					{
						GrowEntityRecordArray(entityId + 1);
					}
				}

				ref EntityRecord record = ref m_entityRecords[entityId];
				EntityArchetypeChunk? chunk = group.Top;
				entity = new Entity(entityId, record.Version);

				if (chunk == null || !chunk.TryPush(entity))
				{
					chunk = group.Allocate();
					chunk.Push(entity);
				}

				record.Chunk = chunk;
				record.Index = chunk.Count - 1;

				EntityCreated?.Invoke(this, entity);
				return entity;
			}
		}

		private void GrowEntityRecordArray(int capacity)
		{
			int newCapacity = m_entityRecords.Length * 2;

			if ((uint)newCapacity > (uint)Array.MaxLength)
			{
				newCapacity = Array.MaxLength;
			}

			if (newCapacity < capacity)
			{
				newCapacity = capacity;
			}

			Array.Resize(ref m_entityRecords, newCapacity);
		}

		public bool DestroyEntity(Entity entity)
		{
			lock (m_freeEntityIds)
			{
				ref EntityRecord record = ref FindEntityRecord(entity);

				if (Unsafe.IsNullRef(ref record))
				{
					return false;
				}

				EntityArchetypeChunkGroup group = m_entityGroups[record.Chunk!.Archetype.Id];
				ref EntityRecord recordToMove = ref m_entityRecords[group.Top![^1].Id];

				recordToMove.Chunk!.PopRange(record.Chunk, record.Index, 1);

				if (recordToMove.Chunk.Count == 0)
				{
					group.Deallocate();
				}

				recordToMove.Chunk = record.Chunk;
				recordToMove.Index = record.Index;

				record.Chunk = null;
				record.Index = 0;
				record.Version++;

				m_freeEntityIds.Enqueue(entity.Id);
				EntityDestroyed?.Invoke(this, entity);
				return true;
			}
		}

		public bool IsEntityAlive(Entity entity)
		{
			return !Unsafe.IsNullRef(ref FindEntityRecord(entity));
		}

		private ref EntityRecord FindEntityRecord(Entity entity)
		{
			EntityRecord[] entityRecords = m_entityRecords;

			if (entity.Id < entityRecords.Length)
			{
				ref EntityRecord record = ref entityRecords[entity.Id];

				if (record.Chunk != null && record.Version == entity.Version)
				{
					return ref record;
				}
			}

			return ref Unsafe.NullRef<EntityRecord>();
		}

		public EntityArchetype CreateArchetype(ReadOnlySpan<ComponentType> componentTypes)
		{
			return GetOrCreateEntityGroup(componentTypes).Archetype;
		}

		private EntityArchetypeChunkGroup GetOrCreateEntityGroup(ReadOnlySpan<ComponentType> componentTypes)
		{
			uint[] buffer = ArrayPool<uint>.Shared.Rent(0);
			int bufferLength = 0;

			for (int i = 0; i < componentTypes.Length; i++)
			{
				ComponentType componentType = componentTypes[i];

				if (componentType != null)
				{
					int typeId = componentType.Id;
					int bufferIndex = typeId >> 5;

					if (bufferIndex >= bufferLength)
					{
						int newBufferLength = bufferIndex + 1;

						if (newBufferLength > buffer.Length)
						{
							uint[] oldBuffer = buffer;
							buffer = ArrayPool<uint>.Shared.Rent(newBufferLength);
							Array.Copy(oldBuffer, buffer, bufferLength);
							ArrayPool<uint>.Shared.Return(oldBuffer);
						}

						Array.Clear(buffer, bufferLength, newBufferLength);
						bufferLength = newBufferLength;
					}

					buffer[bufferIndex] |= 1u << typeId;
				}
			}

			EntityArchetypeChunkGroup? group;

			lock (m_componentBitsToEntityGroups)
			{
				if (!m_componentBitsToEntityGroups.TryGetValue(new ReadOnlyMemory<uint>(buffer, 0, bufferLength), out group))
				{
					EntityArchetype archetype = new(componentTypes) { Id = m_componentBitsToEntityGroups.Count };
					group = new EntityArchetypeChunkGroup(archetype);
					m_componentBitsToEntityGroups.Add(archetype.ComponentBits.AsMemory(), group);

					if (m_componentBitsToEntityGroups.Count > m_entityGroups.Length)
					{
						GrowEntityGroupArray(m_componentBitsToEntityGroups.Count);
					}

					m_entityGroups[archetype.Id] = group;
				}
			}

			ArrayPool<uint>.Shared.Return(buffer);
			return group;
		}

		public EntityArchetype CreateArchetype(EntityArchetype archetype)
		{
			return GetOrCreateEntityGroup(archetype).Archetype;
		}

		private EntityArchetypeChunkGroup GetOrCreateEntityGroup(EntityArchetype archetype)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			EntityArchetypeChunkGroup[] groups = m_entityGroups;
			EntityArchetypeChunkGroup? group;

			if (archetype.Id >= groups.Length || !archetype.Equals((group = groups[archetype.Id])?.Archetype))
			{
				lock (m_componentBitsToEntityGroups)
				{
					if (!m_componentBitsToEntityGroups.TryGetValue(archetype.ComponentBits.AsMemory(), out group))
					{
						EntityArchetype clone = new(archetype) { Id = m_componentBitsToEntityGroups.Count };
						group = new EntityArchetypeChunkGroup(clone);
						m_componentBitsToEntityGroups.Add(clone.ComponentBits.AsMemory(), group);

						if (m_componentBitsToEntityGroups.Count > m_entityGroups.Length)
						{
							GrowEntityGroupArray(m_componentBitsToEntityGroups.Count);
						}

						m_entityGroups[clone.Id] = group;
					}
				}
			}

			return group;
		}

		private void GrowEntityGroupArray(int capacity)
		{
			int newCapacity = m_entityGroups.Length * 2;

			if ((uint)newCapacity > (uint)Array.MaxLength)
			{
				newCapacity = Array.MaxLength;
			}

			if (newCapacity < capacity)
			{
				newCapacity = capacity;
			}

			Array.Resize(ref m_entityGroups, newCapacity);
		}

		private struct EntityRecord
		{
			public EntityArchetypeChunk? Chunk;
			public int Index;
			public int Version;
		}
	}
}
