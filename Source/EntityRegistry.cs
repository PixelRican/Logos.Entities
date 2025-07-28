// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Logos.Entities
{
    /// <summary>
    /// Provides methods that manage entities and their components, maintain the tables they are
    /// stored in, and create queries to iterate across them.
    /// </summary>
    public class EntityRegistry
    {
        private const int MinimumCapacity = 8;
        private const int TargetTableSize = 16384;

        private readonly EntityTableLookup m_lookup;
        private volatile Container m_container;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRegistry"/> class that has the
        /// default capacity.
        /// </summary>
        public EntityRegistry()
            : this(MinimumCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRegistry"/> class that has the
        /// specified capacity.
        /// </summary>
        /// 
        /// <param name="capacity">
        /// The capacity of the <see cref="EntityRegistry"/>.
        /// </param>
        public EntityRegistry(int capacity)
        {
            if (capacity < MinimumCapacity)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(capacity);
                capacity = MinimumCapacity;
            }

            m_lookup = new EntityTableLookup();
            m_container = new Container(capacity);
        }

        /// <summary>
        /// Gets the total number of entities the internal data structure can hold without
        /// resizing.
        /// </summary>
        public int Capacity
        {
            get => m_container.Capacity;
        }

        /// <summary>
        /// Gets the number of entities in the <see cref="EntityRegistry"/>.
        /// </summary>
        public int Count
        {
            get => m_container.Count;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityRegistry"/> has entered its
        /// internal lock in the current thread.
        /// </summary>
        public bool IsLockHeld
        {
            get => Monitor.IsEntered(m_lookup);
        }

        /// <summary>
        /// Returns an <see cref="EntityArchetype"/> that contains component types from the
        /// specified array.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity archetype that is composed of component types from the array.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentTypes"/> is <see langword="null"/>.
        /// </exception>
        public EntityArchetype CreateArchetype(ComponentType[] componentTypes)
        {
            return m_lookup.GetGrouping(componentTypes).Key;
        }

        /// <summary>
        /// Returns an <see cref="EntityArchetype"/> that contains component types from the
        /// specified sequence.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity archetype that is composed of component types from the sequence.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentTypes"/> is <see langword="null"/>.
        /// </exception>
        public EntityArchetype CreateArchetype(IEnumerable<ComponentType> componentTypes)
        {
            return m_lookup.GetGrouping(componentTypes).Key;
        }

        /// <summary>
        /// Returns an <see cref="EntityArchetype"/> that contains component types from the
        /// specified span.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity archetype that is composed of component types from the span.
        /// </returns>
        public EntityArchetype CreateArchetype(ReadOnlySpan<ComponentType> componentTypes)
        {
            return m_lookup.GetGrouping(componentTypes).Key;
        }

        /// <summary>
        /// Creates an <see cref="EntityQuery"/> that selects entity tables stored by the
        /// <see cref="EntityRegistry"/> using the specified <see cref="EntityFilter"/>, and, if
        /// enabled, stores them in a cache for faster iteration speeds.
        /// </summary>
        /// 
        /// <param name="filter">
        /// The <see cref="EntityFilter"/>.
        /// </param>
        /// 
        /// <param name="enableCache">
        /// <see langword="true"/> to enable caching; <see langword="false"/> to disable caching.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityQuery"/> that selects entity tables stored by the
        /// <see cref="EntityRegistry"/> using the <see cref="EntityFilter"/>.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is <see langword="null"/>.
        /// </exception>
        public EntityQuery CreateQuery(EntityFilter filter, bool enableCache)
        {
            return new EntityQuery(m_lookup, filter, enableCache);
        }

        /// <summary>
        /// Creates an entity with no components.
        /// </summary>
        /// 
        /// <returns>
        /// An entity with no components.
        /// </returns>
        public Entity CreateEntity()
        {
            return CreateEntity(m_lookup.GetGrouping(EntityArchetype.Base));
        }

        /// <summary>
        /// Creates an entity that is composed of component types from the specified array.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity that is composed of component types from the array.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentTypes"/> is <see langword="null"/>.
        /// </exception>
        public Entity CreateEntity(ComponentType[] componentTypes)
        {
            return CreateEntity(m_lookup.GetGrouping(componentTypes));
        }

        /// <summary>
        /// Creates an entity that is composed of component types from the specified sequence.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity that is composed of component types from the sequence.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentTypes"/> is <see langword="null"/>.
        /// </exception>
        public Entity CreateEntity(IEnumerable<ComponentType> componentTypes)
        {
            return CreateEntity(m_lookup.GetGrouping(componentTypes));
        }

        /// <summary>
        /// Creates an entity that is composed of component types from the specified span.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity that is composed of component types from the span.
        /// </returns>
        public Entity CreateEntity(ReadOnlySpan<ComponentType> componentTypes)
        {
            return CreateEntity(m_lookup.GetGrouping(componentTypes));
        }

        /// <summary>
        /// Creates an entity that is modelled by the specified <see cref="EntityArchetype"/>.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The <see cref="EntityArchetype"/>.
        /// </param>
        /// 
        /// <returns>
        /// An entity that is modelled by the <see cref="EntityArchetype"/>.
        /// </returns>
        public Entity CreateEntity(EntityArchetype archetype)
        {
            return CreateEntity(m_lookup.GetGrouping(archetype));
        }

        private Entity CreateEntity(EntityTableGrouping grouping)
        {
            lock (m_lookup)
            {
                Container container = m_container;

                if (container.IsFull)
                {
                    m_container = container = container.Grow();
                }

                return container.Create(GetUnfilledTable(grouping));
            }
        }

        /// <summary>
        /// Adds a new entity to the specified <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="table">
        /// The <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <returns>
        /// The entity that was added to the <see cref="EntityTable"/>.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="table"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="table"/> cannot be modified by the <see cref="EntityRegistry"/> or
        /// <paramref name="table"/> is full.
        /// </exception>
        public Entity CreateEntity(EntityTable table)
        {
            ArgumentNullException.ThrowIfNull(table);

            if (table.Registry != this)
            {
                throw new ArgumentException(
                    "The EntityTable cannot be modified by the EntityRegistry.", nameof(table));
            }

            lock (m_lookup)
            {
                if (table.IsFull)
                {
                    throw new ArgumentException("The EntityTable is full.", nameof(table));
                }

                if (table.IsEmpty)
                {
                    m_lookup.GetGrouping(table.Archetype).Add(table);
                }

                Container container = m_container;

                if (container.IsFull)
                {
                    m_container = container = container.Grow();
                }

                return container.Create(table);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="EntityRegistry"/> contains the specified entity.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to search for.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity is in the <see cref="EntityRegistry"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool ContainsEntity(Entity entity)
        {
            return m_container.Contains(entity);
        }

        /// <summary>
        /// Removes the specified entity from <see cref="EntityRegistry"/> and invalidates its
        /// reference.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to destroy.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity was sucessfully destroyed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
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

        /// <summary>
        /// Gets the <see cref="EntityTable"/> the specified entity is stored in.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to search for.
        /// </param>
        /// 
        /// <param name="index">
        /// When this method returns, contains the index of the entity in the
        /// <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <returns>
        /// The <see cref="EntityTable"/> the entity is stored in.
        /// </returns>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public EntityTable FindEntity(Entity entity, out int index)
        {
            return m_container.Find(entity, out index);
        }

        /// <summary>
        /// Adds and removes components from the specified entity to match the model represented by
        /// the specified <see cref="EntityArchetype"/>.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to modify.
        /// </param>
        /// 
        /// <param name="archetype">
        /// The <see cref="EntityArchetype"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public void ModifyEntity(Entity entity, EntityArchetype archetype)
        {
            EntityTableGrouping grouping = m_lookup.GetGrouping(archetype);

            lock (m_lookup)
            {
                Container container = m_container;
                EntityTable table = container.Find(entity, out _);

                if (!archetype.Equals(table.Archetype))
                {
                    container.Move(entity.Id, GetUnfilledTable(grouping));

                    if (table.IsEmpty)
                    {
                        m_lookup.GetGrouping(table.Archetype).Remove(table);
                    }
                }
            }
        }

        /// <summary>
        /// Moves the specified entity to the specified <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to move.
        /// </param>
        /// 
        /// <param name="destination">
        /// The <see cref="EntityTable"/> to move the entity into.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="destination"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="destination"/> cannot be modified by the <see cref="EntityRegistry"/>
        /// or <paramref name="destination"/> is full.
        /// </exception>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public void MoveEntity(Entity entity, EntityTable destination)
        {
            ArgumentNullException.ThrowIfNull(destination);

            if (destination.Registry != this)
            {
                throw new ArgumentException(
                    "The EntityTable cannot be modified by the EntityRegistry.", nameof(destination));
            }

            lock (m_lookup)
            {
                if (destination.IsFull)
                {
                    throw new ArgumentException("The EntityTable is full.", nameof(destination));
                }

                Container container = m_container;
                EntityTable source = container.Find(entity, out _);

                if (source != destination)
                {
                    if (destination.IsEmpty)
                    {
                        m_lookup.GetGrouping(destination.Archetype).Add(destination);
                    }

                    container.Move(entity.Id, destination);

                    if (source.IsEmpty)
                    {
                        m_lookup.GetGrouping(source.Archetype).Remove(source);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the default value of the specified <see cref="ComponentType"/> to the specified
        /// entity.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to modify.
        /// </param>
        /// 
        /// <param name="componentType">
        /// The type of the component to add.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the component was successfully added to the entity;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool AddComponent(Entity entity, ComponentType componentType)
        {
            return ModifyComponent(entity, componentType, adding: true);
        }

        /// <summary>
        /// Determines whether the specified entity has a component of the specified type.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// 
        /// <param name="componentType">
        /// The type of the component to search for.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity has a component of the component type; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool HasComponent(Entity entity, ComponentType componentType)
        {
            return m_container.Find(entity, out _).Archetype.Contains(componentType);
        }

        /// <summary>
        /// Removes a component of the specified type from the specified entity.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to modify.
        /// </param>
        /// 
        /// <param name="componentType">
        /// The type of the component to remove.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the component was successfully removed from the entity;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool RemoveComponent(Entity entity, ComponentType componentType)
        {
            return ModifyComponent(entity, componentType, adding: false);
        }

        /// <summary>
        /// Adds the specified component to the specified entity.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the component.
        /// </typeparam>
        /// 
        /// <param name="entity">
        /// The entity to modify.
        /// </param>
        /// 
        /// <param name="component">
        /// The component to add.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the component was successfully added to the entity;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool AddComponent<T>(Entity entity, T? component)
        {
            lock (m_lookup)
            {
                Container container = m_container;
                EntityTable table = container.Find(entity, out int index);
                EntityArchetype archetype = table.Archetype;
                EntityTableGrouping grouping = m_lookup.GetSupergrouping(archetype, ComponentType.TypeOf<T>());

                if (archetype.Equals(grouping.Key))
                {
                    return false;
                }

                if (table.Count == 1)
                {
                    m_lookup.GetGrouping(archetype).Remove(table);
                }

                table = GetUnfilledTable(grouping);
                container.Move(entity.Id, table);
                table.GetComponents<T>()[index] = component!;
                return true;
            }
        }

        /// <summary>
        /// Removes a component of the specified type from the specified entity.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the component to remove.
        /// </typeparam>
        /// 
        /// <param name="entity">
        /// The entity to modify.
        /// </param>
        /// 
        /// <param name="component">
        /// When this method returns, contains the component removed from the entity.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the component was successfully removed from the entity;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool RemoveComponent<T>(Entity entity, out T? component)
        {
            lock (m_lookup)
            {
                Container container = m_container;
                EntityTable table = container.Find(entity, out int index);
                EntityArchetype archetype = table.Archetype;
                EntityTableGrouping grouping = m_lookup.GetSupergrouping(
                    archetype, ComponentType.TypeOf<T>());

                if (archetype.Equals(grouping.Key))
                {
                    component = default;
                    return false;
                }

                if (table.Count == 1)
                {
                    m_lookup.GetGrouping(archetype).Remove(table);
                }

                component = table.GetComponents<T>()[index];
                table = GetUnfilledTable(grouping);
                container.Move(entity.Id, table);
                return true;
            }
        }

        /// <summary>
        /// Sets the specified component to the specified entity.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the component to set.
        /// </typeparam>
        /// 
        /// <param name="entity">
        /// The entity to modify.
        /// </param>
        /// 
        /// <param name="component">
        /// The component to set.
        /// </param>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public void SetComponent<T>(Entity entity, T? component)
        {
            lock (m_lookup)
            {
                Container container = m_container;
                EntityTable table = container.Find(entity, out int index);
                EntityArchetype archetype = table.Archetype;
                EntityTableGrouping grouping = m_lookup.GetSupergrouping(
                    archetype, ComponentType.TypeOf<T>());

                if (!archetype.Equals(grouping.Key))
                {
                    if (table.Count == 1)
                    {
                        m_lookup.GetGrouping(archetype).Remove(table);
                    }

                    table = GetUnfilledTable(grouping);
                    index = table.Count;
                    container.Move(entity.Id, table);
                }

                table.GetComponents<T>()[index] = component!;
            }
        }

        /// <summary>
        /// Gets the component of the specified <see cref="ComponentType"/> from the specified
        /// entity.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the component.
        /// </typeparam>
        /// 
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// 
        /// <returns>
        /// A component from the entity.
        /// </returns>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool TryGetComponent<T>(Entity entity, out T? component)
        {
            if (m_container.Find(entity, out int index).TryGetComponents(out Span<T> components))
            {
                component = components[index];
                return true;
            }

            component = default;
            return false;
        }

        private bool ModifyComponent(Entity entity, ComponentType componentType, bool adding)
        {
            lock (m_lookup)
            {
                Container container = m_container;
                EntityTable table = container.Find(entity, out _);
                EntityArchetype archetype = table.Archetype;
                EntityTableGrouping grouping = adding
                    ? m_lookup.GetSupergrouping(archetype, componentType)
                    : m_lookup.GetSubgrouping(archetype, componentType);

                if (archetype.Equals(grouping.Key))
                {
                    return false;
                }

                container.Move(entity.Id, GetUnfilledTable(grouping));

                if (table.IsEmpty)
                {
                    m_lookup.GetGrouping(archetype).Remove(table);
                }

                return true;
            }
        }

        private EntityTable GetUnfilledTable(EntityTableGrouping grouping)
        {
            foreach (EntityTable current in grouping)
            {
                if (!current.IsFull)
                {
                    return current;
                }
            }

            EntityTable table = new EntityTable(grouping.Key, this,
                TargetTableSize / grouping.Key.EntitySize);
            grouping.Add(table);
            return table;
        }

        private sealed class Container
        {
            private readonly Entry[] m_entries;
            private readonly int[] m_freeIds;
            private int m_nextId;
            private int m_size;

            public Container(int capacity)
            {
                m_entries = new Entry[capacity];
                m_freeIds = new int[capacity];
            }

            private Container(int capacity, int size)
            {
                m_entries = new Entry[capacity];
                m_freeIds = new int[capacity];
                m_nextId = m_size = size;
            }

            public int Capacity
            {
                get => m_entries.Length;
            }

            public int Count
            {
                get => m_size;
            }

            public bool IsFull
            {
                get => m_size == m_entries.Length;
            }

            public Entity Create(EntityTable table)
            {
                int index = m_size++ < m_nextId
                    ? m_freeIds[m_nextId - m_size]
                    : m_nextId++;
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
                    m_entries[table.GetEntities()[index].Id].Index = index;
                }

                m_freeIds[m_nextId - m_size--] = entity.Id;
                return table;
            }

            public bool Contains(Entity entity)
            {
                return !Unsafe.IsNullRef(ref FindEntry(entity));
            }

            public EntityTable Find(Entity entity, out int index)
            {
                ref Entry entry = ref FindEntry(entity);

                if (Unsafe.IsNullRef(ref entry))
                {
                    throw new EntityNotFoundException(
                        "Entity does not exist within the EntityRegistry.");
                }

                index = entry.Index;
                return entry.Table;
            }

            private ref Entry FindEntry(Entity entity)
            {
                ref Entry entry = ref Unsafe.NullRef<Entry>();

                if ((uint)entity.Id < (uint)m_nextId &&
                    (entry = ref m_entries[entity.Id]).Table != null &&
                    entity.Version == entry.Version)
                {
                    return ref entry;
                }

                return ref Unsafe.NullRef<Entry>();
            }

            public void Move(int index, EntityTable destination)
            {
                ref Entry entry = ref m_entries[index];
                EntityTable table = entry.Table;
                index = entry.Index;

                entry.Table = destination;
                entry.Index = destination.Count;

                destination.AddRange(table, index, 1);
                table.RemoveAt(index);

                if (index < table.Count)
                {
                    m_entries[table.GetEntities()[index].Id].Index = index;
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
