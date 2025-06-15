// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public EntityRegistry()
            : this(DefaultCapacity)
        {
        }

        public EntityRegistry(int capacity)
        {
            if (capacity < DefaultCapacity)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(capacity);
                capacity = DefaultCapacity;
            }

            m_lookup = new EntityTableLookup();
            m_container = new Container(capacity);
        }

        public int Capacity
        {
            get => m_container.Capacity;
        }

        public int Count
        {
            get => m_container.Count;
        }

        public EntityQuery UniversalQuery
        {
            get => m_universalQuery ??= new EntityQuery(m_lookup);
        }

        public Entity CreateEntity()
        {
            return CreateEntity(m_lookup.GetGrouping(EntityArchetype.Base));
        }

        public Entity CreateEntity(ComponentType[] componentTypes)
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

                if (container.Isfull)
                {
                    m_container = container = container.Grow();
                }

                return container.Create(GetNextAvailableTable(grouping));
            }
        }

        private EntityTable GetNextAvailableTable(EntityTableGrouping grouping)
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
                EntityTable? table = m_container.Destroy(entity);

                if (table == null)
                {
                    return false;
                }

                if (table.IsEmpty)
                {
                    m_lookup.GetGrouping(table.Archetype).Remove(table);
                }

                return true;
            }
        }

        public bool ContainsEntity(Entity entity)
        {
            return m_container.Contains(entity);
        }

        public bool AddComponent(Entity entity, ComponentType componentType)
        {
            return ModifyEntity(entity, componentType, adding: true);
        }

        public bool RemoveComponent(Entity entity, ComponentType componentType)
        {
            return ModifyEntity(entity, componentType, adding: false);
        }

        private bool ModifyEntity(Entity entity, ComponentType componentType, bool adding)
        {
            lock (m_lookup)
            {
                Container container = m_container;
                EntityTable? table = container.Find(entity, out _);

                if (table == null)
                {
                    throw new ArgumentException(
                        "Entity does not exist within the EntityRegistry.", nameof(entity));
                }

                EntityArchetype archetype = table.Archetype;
                EntityTableGrouping grouping = adding
                    ? m_lookup.GetSupergrouping(archetype, componentType)
                    : m_lookup.GetSubgrouping(archetype, componentType);

                if (EntityArchetype.Equals(archetype, grouping.Key))
                {
                    return false;
                }

                container.Move(entity.ID, GetNextAvailableTable(grouping));

                if (table.IsEmpty)
                {
                    m_lookup.GetGrouping(archetype).Remove(table);
                }

                return true;
            }
        }

        public bool ContainsComponent(Entity entity, ComponentType componentType)
        {
            EntityTable? table = m_container.Find(entity, out _);
            return table != null && table.Archetype.Contains(componentType);
        }

        public void SetComponent<T>(Entity entity, T? component)
        {
            lock (m_lookup)
            {
                Container container = m_container;
                EntityTable? table = container.Find(entity, out int index);

                if (table == null)
                {
                    throw new ArgumentException(
                        "Entity does not exist within the EntityRegistry.", nameof(entity));
                }

                EntityArchetype archetype = table.Archetype;
                EntityTableGrouping grouping = m_lookup.GetSupergrouping(archetype, ComponentType.TypeOf<T>());

                if (!EntityArchetype.Equals(archetype, grouping.Key))
                {
                    EntityTable source = table;

                    table = GetNextAvailableTable(grouping);
                    index = table.Count;
                    container.Move(entity.ID, table);

                    if (source.IsEmpty)
                    {
                        m_lookup.GetGrouping(archetype).Remove(source);
                    }
                }

                table.GetComponents<T>()[index] = component!;
            }
        }

        public bool RemoveComponent<T>(Entity entity, out T? component)
        {
            lock (m_lookup)
            {
                Container container = m_container;
                EntityTable? table = container.Find(entity, out int index);

                if (table == null)
                {
                    throw new ArgumentException(
                        "Entity does not exist within the EntityRegistry.", nameof(entity));
                }

                EntityArchetype archetype = table.Archetype;
                EntityTableGrouping grouping = m_lookup.GetSubgrouping(
                    archetype, ComponentType.TypeOf<T>());

                if (EntityArchetype.Equals(archetype, grouping.Key))
                {
                    component = default;
                    return false;
                }

                component = table.GetComponents<T>()[index];
                container.Move(entity.ID, GetNextAvailableTable(grouping));

                if (table.IsEmpty)
                {
                    m_lookup.GetGrouping(archetype).Remove(table);
                }

                return true;
            }
        }

        public bool TryGetComponent<T>(Entity entity, out T? component)
        {
            EntityTable? table = m_container.Find(entity, out int index);

            if (table != null &&
                table.TryGetComponents(out Span<T> components) &&
                (uint)index < (uint)components.Length)
            {
                component = components[index];
                return true;
            }

            component = default;
            return false;
        }

        public EntityArchetype GetArchetype(ComponentType[] componentTypes)
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
            if (m_lookup.TryGetGrouping(archetype, out EntityTableGrouping? grouping))
            {
                return grouping.AsSpan();
            }

            return ReadOnlySpan<EntityTable>.Empty;
        }

        public bool TryGetTable(Entity entity, [NotNullWhen(true)] out EntityTable? table)
        {
            return (table = m_container.Find(entity, out _)) != null;
        }

        public EntityQuery GetQuery(EntityFilter filter)
        {
            if (filter == EntityFilter.Universal)
            {
                return UniversalQuery;
            }

            return new EntityQuery(m_lookup, filter);
        }

        private sealed class Container
        {
            private readonly int[] m_freeIDs;
            private readonly Entry[] m_entries;
            private int m_nextID;
            private int m_size;

            public Container(int capacity)
            {
                m_freeIDs = new int[capacity];
                m_entries = new Entry[capacity];
            }

            private Container(int capacity, int size)
            {
                m_freeIDs = new int[capacity];
                m_entries = new Entry[capacity];
                m_nextID = m_size = size;
            }

            public int Capacity
            {
                get => m_entries.Length;
            }

            public int Count
            {
                get => m_size;
            }

            public bool Isfull
            {
                get => m_size == m_entries.Length;
            }

            public Entity Create(EntityTable table)
            {
                int index = m_size++ < m_nextID
                    ? m_freeIDs[m_nextID - m_size]
                    : m_nextID++;
                ref Entry entry = ref m_entries[index];
                Entity entity = new Entity(index, entry.Version);

                entry.Table = table;
                entry.Index = table.Count;
                table.Add(entity);

                return entity;
            }

            public EntityTable? Destroy(Entity entity)
            {
                ref Entry entry = ref FindEntry(entity);

                if (Unsafe.IsNullRef(ref entry))
                {
                    return null;
                }

                EntityTable table = entry.Table;
                int index = entry.Index;

                entry.Table = null!;
                entry.Index = 0;
                entry.Version++;
                table.RemoveAt(index);

                if (index < table.Count)
                {
                    m_entries[table.GetEntities()[index].ID].Index = index;
                }

                m_freeIDs[m_nextID - m_size--] = entity.ID;
                return table;
            }

            public bool Contains(Entity entity)
            {
                return !Unsafe.IsNullRef(ref FindEntry(entity));
            }

            public EntityTable? Find(Entity entity, out int index)
            {
                ref Entry entry = ref FindEntry(entity);

                if (Unsafe.IsNullRef(ref entry))
                {
                    index = 0;
                    return null;
                }

                index = entry.Index;
                return entry.Table;
            }

            private ref Entry FindEntry(Entity entity)
            {
                ref Entry entry = ref Unsafe.NullRef<Entry>();

                if ((uint)entity.ID < (uint)m_nextID &&
                    (entry = ref m_entries[entity.ID]).Table != null &&
                    entity.Version == entry.Version)
                {
                    return ref entry;
                }

                return ref Unsafe.NullRef<Entry>();
            }

            public void Move(int entityID, EntityTable destination)
            {
                ref Entry entry = ref m_entries[entityID];
                EntityTable table = entry.Table;
                int index = entry.Index;

                entry.Table = destination;
                entry.Index = destination.Count;

                destination.AddRange(table, index, 1);
                table.RemoveAt(index);

                if (index < table.Count)
                {
                    m_entries[table.GetEntities()[index].ID].Index = index;
                }
            }

            public Container Grow()
            {
                int size = m_size;
                int capacity = size * 2;

                if ((uint)capacity > (uint)Array.MaxLength)
                {
                    capacity = Array.MaxLength;
                }

                if (capacity <= size)
                {
                    capacity = size + 1;
                }

                Container container = new Container(capacity, size);
                Array.Copy(m_entries, container.m_entries, size);
                return container;
            }

            private struct Entry
            {
                public EntityTable Table;
                public int Index;
                public int Version;
            }
        }
    }
}
