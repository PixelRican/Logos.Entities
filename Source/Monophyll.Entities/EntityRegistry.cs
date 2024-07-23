using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
	public class EntityRegistry
	{
		private const int InitialEntityGroupCapacity = 16;
		private const int InitialEntityRecordCapacity = 128;

		private readonly Dictionary<ReadOnlyMemory<uint>, EntityGroup> m_componentBitsToEntityGroups;
		private readonly Queue<int> m_freeEntityIds;
		private EntityGroup[] m_entityGroups;
		private EntityRecord[] m_entityRecords;
		private int m_entityCount;
		private int m_nextEntityId;

		public EntityRegistry()
		{
			m_componentBitsToEntityGroups = new Dictionary<ReadOnlyMemory<uint>, EntityGroup>(InitialEntityGroupCapacity, ComponentBitEqualityComparer.Instance);
			m_entityGroups = new EntityGroup[InitialEntityGroupCapacity];
			m_freeEntityIds = new Queue<int>(InitialEntityRecordCapacity);
			m_entityRecords = new EntityRecord[InitialEntityRecordCapacity];
			m_componentBitsToEntityGroups.Add(ReadOnlyMemory<uint>.Empty, m_entityGroups[0] = new EntityGroup(EntityArchetype.Base));
		}

		public event EventHandler<Entity>? EntityCreated;

		public event EventHandler<Entity>? EntityDestroyed;

		public int EntityCapacity
		{
			get => m_entityRecords.Length;
		}

		public int EntityCount
		{
			get => m_entityCount;
		}

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

		private Entity CreateEntity(EntityGroup group)
		{
			lock (m_freeEntityIds)
			{
				if (!m_freeEntityIds.TryDequeue(out int entityId))
				{
					entityId = m_nextEntityId++;

					if (entityId >= m_entityRecords.Length)
					{
						GrowEntityRecordArray(entityId + 1);
					}
				}

				ref EntityRecord record = ref m_entityRecords[entityId];
				EntityArchetypeChunk? chunk = group.LastChunk;
				Entity entity = new(entityId, record.Version);

				if (chunk == null || !chunk.TryPush(entity))
				{
					chunk = group.Allocate();
					chunk.Push(entity);
				}

				record.Chunk = chunk;
				record.Index = chunk.Count - 1;

				m_entityCount++;
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

				EntityGroup group = m_entityGroups[record.Chunk.Archetype.Id];
				ref EntityRecord recordToMove = ref m_entityRecords[group.LastChunk![^1].Id];

				recordToMove.Chunk!.PopRange(record.Chunk, record.Index, 1);

				if (recordToMove.Chunk.Count == 0)
				{
					group.Deallocate();
				}

				recordToMove.Chunk = record.Chunk;
				recordToMove.Index = record.Index;

				record.Chunk = null!;
				record.Index = 0;
				record.Version++;

				m_entityCount--;
				m_freeEntityIds.Enqueue(entity.Id);
				EntityDestroyed?.Invoke(this, entity);
				return true;
			}
		}

		public bool IsEntityAlive(Entity entity)
		{
			return !Unsafe.IsNullRef(ref FindEntityRecord(entity));
		}

		public EntityQueryResults LocateEntity(Entity entity, out int index)
		{
			ref EntityRecord record = ref FindEntityRecord(entity);

			if (Unsafe.IsNullRef(ref record))
			{
				throw new ArgumentException($"{entity} does not exist within the EntityRegistry.", nameof(entity));
			}

			index = record.Index;
			return new EntityQueryResults(record.Chunk);
		}

		public (EntityQueryResults Results, int Index) LocateEntity(Entity entity)
		{
			ref EntityRecord record = ref FindEntityRecord(entity);

			if (Unsafe.IsNullRef(ref record))
			{
				throw new ArgumentException($"{entity} does not exist within the EntityRegistry.", nameof(entity));
			}

			return (new EntityQueryResults(record.Chunk), record.Index);
		}

		private ref EntityRecord FindEntityRecord(Entity entity)
		{
			EntityRecord[] entityRecords = m_entityRecords;

			if ((uint)entity.Id < (uint)entityRecords.Length)
			{
				ref EntityRecord record = ref entityRecords[entity.Id];

				if (record.Chunk != null && record.Version == entity.Version)
				{
					return ref record;
				}
			}

			return ref Unsafe.NullRef<EntityRecord>();
		}

		public EntityArchetype CreateEntityArchetype(ReadOnlySpan<ComponentType> componentTypes)
		{
			return GetOrCreateEntityGroup(componentTypes).Archetype;
		}

		private EntityGroup GetOrCreateEntityGroup(ReadOnlySpan<ComponentType> componentTypes)
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

			EntityGroup? group;

			lock (m_componentBitsToEntityGroups)
			{
				if (!m_componentBitsToEntityGroups.TryGetValue(new ReadOnlyMemory<uint>(buffer, 0, bufferLength), out group))
				{
					EntityArchetype archetype = new(componentTypes) { Id = m_componentBitsToEntityGroups.Count };
					group = new EntityGroup(archetype);
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

		public EntityArchetype CreateEntityArchetype(EntityArchetype archetype)
		{
			return GetOrCreateEntityGroup(archetype).Archetype;
		}

		private EntityGroup GetOrCreateEntityGroup(EntityArchetype archetype)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			EntityGroup[] groups = m_entityGroups;
			EntityGroup? group;

			if (archetype.Id >= groups.Length || !archetype.Equals((group = groups[archetype.Id])?.Archetype))
			{
				lock (m_componentBitsToEntityGroups)
				{
					if (!m_componentBitsToEntityGroups.TryGetValue(archetype.ComponentBits.AsMemory(), out group))
					{
						EntityArchetype clone = new(archetype) { Id = m_componentBitsToEntityGroups.Count };
						group = new EntityGroup(clone);
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

		public void AddComponent(Entity entity, ComponentType componentType)
		{
			ArgumentNullException.ThrowIfNull(componentType);

			lock (m_freeEntityIds)
			{
				ref EntityRecord record = ref FindEntityRecord(entity);

				if (Unsafe.IsNullRef(ref record))
				{
					throw new ArgumentException($"{entity} does not exist within the EntityRegistry.", nameof(entity));
				}

				if (!record.Chunk.TryGetComponentData(componentType, out Span<byte> componentData))
				{
					MoveEntity(ref record, GetOrCreateEntityGroupSuperset(record.Chunk.Archetype, componentType));
					componentData = record.Chunk.GetComponentData(componentType);
				}

				Unsafe.InitBlock(ref componentData[record.Index * componentType.ByteSize], 0, (uint)componentType.ByteSize);
			}
		}

		public bool TryAddComponent(Entity entity, ComponentType componentType)
		{
			ArgumentNullException.ThrowIfNull(componentType);

			lock (m_freeEntityIds)
			{
				ref EntityRecord record = ref FindEntityRecord(entity);

				if (Unsafe.IsNullRef(ref record))
				{
					return false;
				}

				if (!record.Chunk.TryGetComponentData(componentType, out Span<byte> componentData))
				{
					MoveEntity(ref record, GetOrCreateEntityGroupSuperset(record.Chunk.Archetype, componentType));
					componentData = record.Chunk.GetComponentData(componentType);
				}

				Unsafe.InitBlock(ref componentData[record.Index * componentType.ByteSize], 0, (uint)componentType.ByteSize);
				return true;
			}
		}

		public bool RemoveComponent(Entity entity, ComponentType componentType)
		{
			ArgumentNullException.ThrowIfNull(componentType);

			lock (m_freeEntityIds)
			{
				ref EntityRecord record = ref FindEntityRecord(entity);

				if (Unsafe.IsNullRef(ref record) || !record.Chunk.TryGetComponentData(componentType, out Span<byte> componentData))
				{
					return false;
				}

				MoveEntity(ref record, GetOrCreateEntityGroupSubset(record.Chunk.Archetype, componentType));
				return true;
			}
		}

		public bool RemoveComponent<T>(Entity entity, out T component) where T : unmanaged
		{
			lock (m_freeEntityIds)
			{
				ref EntityRecord record = ref FindEntityRecord(entity);

				if (Unsafe.IsNullRef(ref record) || !record.Chunk.TryGetComponents(out Span<T> components))
				{
					component = default;
					return false;
				}

				component = components[record.Index];
				MoveEntity(ref record, GetOrCreateEntityGroupSubset(record.Chunk.Archetype, ComponentType.TypeOf<T>()));
				return true;
			}
		}

		public void SetComponent<T>(Entity entity, T component) where T : unmanaged
		{
			lock (m_freeEntityIds)
			{
				ref EntityRecord record = ref FindEntityRecord(entity);

				if (Unsafe.IsNullRef(ref record))
				{
					throw new ArgumentException($"{entity} does not exist within the EntityRegistry.", nameof(entity));
				}

				if (!record.Chunk.TryGetComponents(out Span<T> components))
				{
					MoveEntity(ref record, GetOrCreateEntityGroupSuperset(record.Chunk.Archetype, ComponentType.TypeOf<T>()));
					components = record.Chunk.GetComponents<T>();
				}

				components[record.Index] = component;
			}
		}

		public bool TrySetComponent<T>(Entity entity, T component) where T : unmanaged
		{
			lock (m_freeEntityIds)
			{
				ref EntityRecord record = ref FindEntityRecord(entity);

				if (Unsafe.IsNullRef(ref record))
				{
					return false;
				}

				if (!record.Chunk.TryGetComponents(out Span<T> components))
				{
					MoveEntity(ref record, GetOrCreateEntityGroupSuperset(record.Chunk.Archetype, ComponentType.TypeOf<T>()));
					components = record.Chunk.GetComponents<T>();
				}

				components[record.Index] = component;
				return true;
			}
		}

		private EntityGroup GetOrCreateEntityGroupSuperset(EntityArchetype archetype, ComponentType componentType)
		{
			ImmutableArray<uint> componentBits = archetype.ComponentBits;
			int bufferLength = Math.Max(componentBits.Length, componentType.Id + 32 >> 5);
			uint[] buffer = ArrayPool<uint>.Shared.Rent(bufferLength);
			EntityGroup? group;

			componentBits.CopyTo(buffer);

			if (componentBits.Length < bufferLength)
			{
				Array.Clear(buffer, componentBits.Length, bufferLength - componentBits.Length);
			}

			buffer[componentType.Id >> 5] |= 1u << componentType.Id;

			lock (m_componentBitsToEntityGroups)
			{
				if (!m_componentBitsToEntityGroups.TryGetValue(new ReadOnlyMemory<uint>(buffer, 0, bufferLength), out group))
				{
					archetype = archetype.Add(componentType, m_componentBitsToEntityGroups.Count);
					group = new EntityGroup(archetype);
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

		private EntityGroup GetOrCreateEntityGroupSubset(EntityArchetype archetype, ComponentType componentType)
		{
			ImmutableArray<ComponentType> componentTypes = archetype.ComponentTypes;

			if (componentTypes.Length <= 1)
			{
				return m_entityGroups[0];
			}

			ImmutableArray<uint> componentBits = archetype.ComponentBits;
			int bufferLength = componentBits.Length;
			uint[] buffer = ArrayPool<uint>.Shared.Rent(bufferLength);
			EntityGroup? group;

			componentBits.CopyTo(buffer);
			buffer[componentType.Id >> 5] &= ~(1u << componentType.Id);

			if (componentTypes[^1] == componentType)
			{
				bufferLength = componentTypes[^2].Id + 32 >> 5;
			}

			lock (m_componentBitsToEntityGroups)
			{
				if (!m_componentBitsToEntityGroups.TryGetValue(new ReadOnlyMemory<uint>(buffer, 0, bufferLength), out group))
				{
					archetype = archetype.Add(componentType, m_componentBitsToEntityGroups.Count);
					group = new EntityGroup(archetype);
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

		private void MoveEntity(ref EntityRecord record, EntityGroup groupToMoveTo)
		{
			EntityGroup groupToMoveFrom = m_entityGroups[record.Chunk.Archetype.Id];
			EntityArchetypeChunk? chunkToMoveTo = groupToMoveTo.LastChunk;

			if (chunkToMoveTo == null || !chunkToMoveTo.TryPushRange(record.Chunk, record.Index, 1))
			{
				chunkToMoveTo = groupToMoveTo.Allocate();
				chunkToMoveTo.PushRange(record.Chunk, record.Index, 1);
			}

			EntityArchetypeChunk lastChunkInGroup = groupToMoveFrom.LastChunk!;
			ref EntityRecord lastRecordInGroup = ref m_entityRecords[lastChunkInGroup[^1].Id];

			lastChunkInGroup.PopRange(record.Chunk, record.Index, 1);

			if (lastChunkInGroup.Count == 0)
			{
				groupToMoveFrom.Deallocate();
			}

			lastRecordInGroup.Chunk = record.Chunk;
			lastRecordInGroup.Index = record.Index;
			record.Chunk = chunkToMoveTo;
			record.Index = chunkToMoveTo.Count - 1;
		}

		private sealed class EntityGroup
		{
			private readonly EntityArchetype m_archetype;
			private EntityArchetypeChunk? m_lastChunk;
			private int m_count;
			private int m_version;

			public EntityGroup(EntityArchetype archetype)
			{
				m_archetype = archetype;
			}

			public EntityArchetype Archetype
			{
				get => m_archetype;
			}

			public EntityArchetypeChunk? LastChunk
			{
				get => m_lastChunk;
			}

			public int Count
			{
				get => m_count;
			}

			public int Version
			{
				get => m_version;
			}

			public EntityArchetypeChunk Allocate()
			{
				EntityArchetypeChunk result = m_lastChunk == null ? new(m_archetype) : new(m_lastChunk);
				m_lastChunk = result;
				m_count++;
				m_version++;
				return result;
			}

			public bool Deallocate()
			{
				if (m_lastChunk == null)
				{
					return false;
				}

				m_lastChunk = m_lastChunk.Next;
				m_count--;
				m_version++;
				return true;
			}
		}

		private struct EntityRecord
		{
			public EntityArchetypeChunk Chunk;
			public int Index;
			public int Version;
		}
	}
}
