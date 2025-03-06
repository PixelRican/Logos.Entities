using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
	public class EntityRegistry
	{
		private const int DefaultCapacity = 8;
		private const int TargetChunkSize = 16384;

		private readonly EntityArchetypeLookup m_lookup;
		private volatile Container m_container;
		private EntityQuery? m_universalQuery;

		public int Capacity
		{
			get => m_container.Entries.Length;
		}

		public int Count
		{
			get => m_container.Count;
		}

		public EntityQuery UniversalQuery
		{
			get => m_universalQuery ??= new EntityQuery(m_lookup);
		}

		public EntityRegistry()
		{
			m_lookup = new EntityArchetypeLookup();
			m_container = new Container(DefaultCapacity);
		}

		public Entity CreateEntity(params ComponentType[] componentTypes)
		{
			return CreateEntity(m_lookup.GetGrouping(componentTypes));
		}

		public Entity CreateEntity(IEnumerable<ComponentType> componentTypes)
		{
			return CreateEntity(m_lookup.GetGrouping(componentTypes));
		}

		public Entity CreateEntity(ReadOnlySpan<ComponentType> componentTypes)
		{
			return CreateEntity(m_lookup.GetGrouping(componentTypes));
		}

		public Entity CreateEntity(EntityArchetype archetype)
		{
			return CreateEntity(m_lookup.GetGrouping(archetype));
		}

		private Entity CreateEntity(EntityArchetypeGrouping grouping)
		{
			lock (m_lookup)
			{
				Container container = m_container;

				if (container.Count == container.Entries.Length)
				{
					int newCapacity = container.Count * 2;

					if ((uint)newCapacity > (uint)Array.MaxLength)
					{
						newCapacity = Array.MaxLength;
					}

					if (newCapacity <= container.Count)
					{
						newCapacity = container.Count + 1;
					}

					Container newContainer = new Container(newCapacity);

					Array.Copy(container.Entries, newContainer.Entries, container.Count);
					newContainer.NextId = newContainer.Count = container.Count;
					m_container = container = newContainer;
				}

				int index = container.Count++ < container.NextId ?
							container.FreeIds[container.NextId - container.Count] :
							container.NextId++;
				ref Entry entry = ref container.Entries[index];
				Entity entity = new Entity(index, entry.Version);

				if (!grouping.TryPeek(out EntityArchetypeChunk? chunk) || chunk.IsFull)
				{
					grouping.TryAdd(chunk = new EntityArchetypeChunk(grouping.Key,
						m_lookup, TargetChunkSize / grouping.Key.EntitySize));
				}

				index = chunk.Count;
				chunk.Push(entity);
				entry.Chunk = chunk;
				entry.Index = index;

				return entity;
			}
		}

		public bool DestroyEntity(Entity entity)
		{
			lock (m_lookup)
			{
				Container container = m_container;
				ref Entry entryToDestroy = ref Unsafe.NullRef<Entry>();

				if ((uint)entity.Id >= (uint)container.NextId
					|| entity.Version != (entryToDestroy = ref container.Entries[entity.Id]).Version
					|| entryToDestroy.Chunk == null)
				{
					return false;
				}

				EntityArchetypeGrouping grouping = m_lookup.GetGrouping(entryToDestroy.Chunk.Archetype);
				grouping.TryPeek(out EntityArchetypeChunk? chunkToPop);
				ref Entry entryToPop = ref container.Entries[chunkToPop!.GetEntities()[^1].Id];

				entryToDestroy.Chunk.SetRange(entryToDestroy.Index, chunkToPop, entryToPop.Index, 1);
				chunkToPop.Pop();

				entryToPop.Chunk = entryToDestroy.Chunk;
				entryToPop.Index = entryToDestroy.Index;
				entryToDestroy.Chunk = null!;
				entryToDestroy.Index = -1;
				entryToDestroy.Version++;
				container.FreeIds[container.NextId - container.Count--] = entity.Id;

				if (chunkToPop.IsEmpty)
				{
					grouping.TryTake(out _);
				}

				return true;
			}
		}

		public bool ContainsEntity(Entity entity)
		{
			Container container = m_container;
			ref Entry entry = ref Unsafe.NullRef<Entry>();

			return (uint)entity.Id < (uint)container.NextId
					&& entity.Version == (entry = ref container.Entries[entity.Id]).Version
					&& entry.Chunk != null
					&& entry.Index >= 0;
		}

		public void AddComponent<T>(Entity entity)
		{
			SetComponent<T>(entity, default!);
		}

		public void RemoveComponent<T>(Entity entity)
		{
			lock (m_lookup)
			{
				Container container = m_container;
				ref Entry entry = ref Unsafe.NullRef<Entry>();

				if ((uint)entity.Id >= (uint)container.NextId
					|| entity.Version != (entry = ref container.Entries[entity.Id]).Version
					|| entry.Chunk == null)
				{
					throw new ArgumentException("The entity does not exist.", nameof(entity));
				}

				EntityArchetypeGrouping groupingToMoveTo = m_lookup.GetSubgrouping(entry.Chunk.Archetype, ComponentType.TypeOf<T>());

				if (entry.Chunk.Archetype != groupingToMoveTo.Key)
				{
					MoveEntry(container, groupingToMoveTo, ref entry);
				}
			}
		}

		public void SetComponent<T>(Entity entity, T component)
		{
			lock (m_lookup)
			{
				Container container = m_container;
				ref Entry entry = ref Unsafe.NullRef<Entry>();

				if ((uint)entity.Id >= (uint)container.NextId
					|| entity.Version != (entry = ref container.Entries[entity.Id]).Version
					|| entry.Chunk == null)
				{
					throw new ArgumentException("The entity does not exist.", nameof(entity));
				}

				EntityArchetypeGrouping groupingToMoveTo = m_lookup.GetSupergrouping(entry.Chunk.Archetype, ComponentType.TypeOf<T>());

				if (entry.Chunk.Archetype != groupingToMoveTo.Key)
				{
					MoveEntry(container, groupingToMoveTo, ref entry);
				}

				if (!ComponentType.TypeOf<T>().IsTag)
				{
					entry.Chunk.GetComponents<T>()[entry.Index] = component;
				}
			}
		}

		public bool TryGetComponent<T>(Entity entity, out T? component)
		{
			Container container = m_container;
			ref Entry entry = ref Unsafe.NullRef<Entry>();
			EntityArchetypeChunk chunk;
			int index;

			if ((uint)entity.Id < (uint)container.NextId
				&& entity.Version != (entry = ref container.Entries[entity.Id]).Version
				&& (chunk = entry.Chunk) != null
				&& (index = entry.Index) >= 0)
			{
				component = chunk.GetComponents<T>()[index];
				return true;
			}

			component = default;
			return false;
		}

		private void MoveEntry(Container container, EntityArchetypeGrouping groupingToMoveTo, ref Entry entryToMove)
		{
			EntityArchetypeGrouping groupingToMoveFrom = m_lookup.GetGrouping(entryToMove.Chunk.Archetype);
			groupingToMoveFrom.TryPeek(out EntityArchetypeChunk? chunkToPop);
			ref Entry entryToPop = ref container.Entries[chunkToPop!.GetEntities()[^1].Id];

			if (!groupingToMoveTo.TryPeek(out EntityArchetypeChunk? chunkToMoveTo) || chunkToMoveTo.IsFull)
			{
				groupingToMoveTo.TryAdd(chunkToMoveTo = new EntityArchetypeChunk(groupingToMoveTo.Key,
					TargetChunkSize / groupingToMoveTo.Key.EntitySize));
			}

			int indexToMoveTo = chunkToMoveTo.Count;

			chunkToMoveTo.PushRange(entryToMove.Chunk, entryToMove.Index, 1);
			entryToMove.Chunk.SetRange(entryToMove.Index, chunkToPop, entryToPop.Index, 1);
			chunkToPop.Pop();

			if (chunkToPop.IsEmpty)
			{
				groupingToMoveFrom.TryTake(out _);
			}

			entryToPop.Chunk = entryToMove.Chunk;
			entryToPop.Index = entryToMove.Index;
			entryToMove.Chunk = chunkToMoveTo;
			entryToMove.Index = indexToMoveTo;
		}

		public EntityArchetype GetArchetype(params ComponentType[] componentTypes)
		{
			return m_lookup.GetGrouping(componentTypes).Key;
		}

		public EntityArchetype GetArchetype(IEnumerable<ComponentType> componentTypes)
		{
			return m_lookup.GetGrouping(componentTypes).Key;
		}

		public EntityArchetype GetArchetype(ReadOnlySpan<ComponentType> componentTypes)
		{
			return m_lookup.GetGrouping(componentTypes).Key;
		}

		public EntityArchetype GetArchetype(EntityArchetype archetype)
		{
			return m_lookup.GetGrouping(archetype).Key;
		}

		public bool TryGetChunk(Entity entity, out EntityArchetypeChunk? chunk)
		{
			Container container = m_container;
			ref Entry entry = ref Unsafe.NullRef<Entry>();

			if ((uint)entity.Id < (uint)container.NextId
				&& entity.Version == (entry = ref container.Entries[entity.Id]).Version
				&& (chunk = entry.Chunk) != null
				&& entry.Index >= 0)
			{
				return true;
			}

			chunk = null;
			return false;
		}

		public EntityQuery GetQuery(params ComponentType[] componentTypes)
		{
			return GetQuery(EntityFilter.Create(componentTypes, Array.Empty<ComponentType>(), Array.Empty<ComponentType>()));
		}

		public EntityQuery GetQuery(IEnumerable<ComponentType> componentTypes)
		{
			return GetQuery(EntityFilter.Create(componentTypes, Enumerable.Empty<ComponentType>(), Enumerable.Empty<ComponentType>()));
		}

		public EntityQuery GetQuery(ReadOnlySpan<ComponentType> componentTypes)
		{
			return GetQuery(EntityFilter.Create(componentTypes, ReadOnlySpan<ComponentType>.Empty, ReadOnlySpan<ComponentType>.Empty));
		}

		public EntityQuery GetQuery(EntityFilter filter)
		{
			return filter == EntityFilter.Universal ? UniversalQuery : new EntityQuery(m_lookup, filter);
		}

		private struct Entry
		{
			public EntityArchetypeChunk Chunk;
			public int Index;
			public int Version;
		}

		private sealed class Container
		{
			public readonly Entry[] Entries;
			public readonly int[] FreeIds;
			public int Count;
			public int NextId;

			public Container(int capacity)
			{
				Entries = new Entry[capacity];
				FreeIds = new int[capacity];
			}
		}
	}
}
