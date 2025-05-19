using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
	public class EntityRegistry
	{
		private const int DefaultCapacity = 8;
		private const int TargetTableSize = 16384;

		private readonly EntityTableLookup m_lookup;
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
			m_lookup = new EntityTableLookup();
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

		private Entity CreateEntity(EntityTableGrouping grouping)
		{
			lock (m_lookup)
			{
				Container container = m_container;

				if (container.Count == container.Entries.Length)
				{
					m_container = container = container.Grow();
				}

				int index = container.Count++ < container.NextId
					? container.FreeIds[container.NextId - container.Count]
					: container.NextId++;
				ref Entry entry = ref container.Entries[index];
				Entity entity = new Entity(index, entry.Version);

				entry.Table = GetTable(grouping);
				entry.Index = entry.Table.Count;
				entry.Table.Add(entity);
				return entity;
			}
		}

		private EntityTable GetTable(EntityTableGrouping grouping)
		{
			foreach (EntityTable current in grouping)
			{
				if (!current.IsFull)
				{
					return current;
				}
			}

			EntityTable table = new EntityTable(grouping.Key, m_lookup, TargetTableSize / grouping.Key.EntitySize);
			grouping.Add(table);
			return table;
		}

		public bool DestroyEntity(Entity entity)
		{
			lock (m_lookup)
			{
				Container container = m_container;
				ref Entry entry = ref Unsafe.NullRef<Entry>();

				if ((uint)entity.Id >= (uint)container.NextId
					|| (entry = ref container.Entries[entity.Id]).Table == null
					|| entry.Version != entity.Version)
				{
					return false;
				}

				entry.Table.RemoveAt(entry.Index);

				if (entry.Table.IsEmpty)
				{
					m_lookup.GetGrouping(entry.Table.Archetype).Remove(entry.Table);
				}
				else
				{
					container.Entries[entry.Table.GetEntities()[entry.Index].Id].Index = entry.Index;
				}

				container.FreeIds[container.NextId - container.Count--] = entity.Id;
				entry.Table = null!;
				entry.Index = -1;
				entry.Version++;
				return true;
			}
		}

		public bool ContainsEntity(Entity entity)
		{
			Container container = m_container;
			ref Entry entry = ref Unsafe.NullRef<Entry>();
			return (uint)entity.Id < (uint)container.NextId
				&& (entry = ref container.Entries[entity.Id]).Table != null
				&& entry.Index >= 0
				&& entity.Version == entity.Version;
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
					|| (entry = ref container.Entries[entity.Id]).Table == null
					|| entry.Version != entity.Version)
				{
					throw new ArgumentException("The entity does not exist.", nameof(entity));
				}

				EntityTableGrouping groupingToMoveTo = m_lookup.GetSubgrouping(entry.Table.Archetype, ComponentType.TypeOf<T>());

				if (!EntityArchetype.Equals(entry.Table.Archetype, groupingToMoveTo.Key))
				{
					MoveEntry(ref entry, container, GetTable(groupingToMoveTo));
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
					|| (entry = ref container.Entries[entity.Id]).Table == null
					|| entry.Version != entity.Version)
				{
					throw new ArgumentException("The entity does not exist.", nameof(entity));
				}

				EntityTableGrouping groupingToMoveTo = m_lookup.GetSupergrouping(entry.Table.Archetype, ComponentType.TypeOf<T>());

				if (!EntityArchetype.Equals(entry.Table.Archetype, groupingToMoveTo.Key))
				{
					MoveEntry(ref entry, container, GetTable(groupingToMoveTo));
				}

				if (entry.Table.TryGetComponents(out Span<T> components))
				{
					components[entry.Index] = component;
				}
			}
		}

		private void MoveEntry(ref Entry entry, Container container, EntityTable destination)
		{
			destination.AddRange(entry.Table, entry.Index, 1);
			entry.Table.RemoveAt(entry.Index);

			if (entry.Table.IsEmpty)
			{
				m_lookup.GetGrouping(entry.Table.Archetype).Remove(entry.Table);
			}
			else
			{
				container.Entries[entry.Table.GetEntities()[entry.Index].Id].Index = entry.Index;
			}

			entry.Table = destination;
			entry.Index = destination.Count - 1;
		}

		public bool TryGetComponent<T>(Entity entity, out T? component)
		{
			Container container = m_container;
			ref Entry entry = ref Unsafe.NullRef<Entry>();
			EntityTable table;
			int index;

			if ((uint)entity.Id < (uint)container.NextId
				&& (table = (entry = ref container.Entries[entity.Id]).Table) != null
				&& table.TryGetComponents(out Span<T> components)
				&& (uint)(index = entry.Index) < (uint)components.Length
				&& entity.Version == entry.Version)
			{
				component = components[index];
				return true;
			}

			component = default;
			return false;
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

		public ReadOnlySpan<EntityTable> GetTables(EntityArchetype archetype)
		{
			return m_lookup.TryGetGrouping(archetype, out EntityTableGrouping? grouping)
				? grouping.AsSpan()
				: ReadOnlySpan<EntityTable>.Empty;
		}

		public bool TryGetTable(Entity entity, out EntityTable? table)
		{
			Container container = m_container;
			ref Entry entry = ref Unsafe.NullRef<Entry>();

			if ((uint)entity.Id < (uint)container.NextId
				&& (table = (entry = ref container.Entries[entity.Id]).Table) != null
				&& entry.Index >= 0
				&& entry.Version == entity.Version)
			{
				return true;
			}

			table = null;
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
			public EntityTable Table;
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

			public Container Grow()
			{
				int capacity = Count * 2;

				if ((uint)capacity > (uint)Array.MaxLength)
				{
					capacity = Array.MaxLength;
				}

				if (capacity <= Count)
				{
					capacity = Count + 1;
				}

				Container container = new Container(capacity);
				Array.Copy(Entries, container.Entries, Count);
				container.NextId = container.Count = Count;
				return container;
			}
		}
	}
}
