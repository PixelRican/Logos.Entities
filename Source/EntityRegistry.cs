// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Logos.Entities
{
    /// <summary>
    /// Provides methods that manage entities and their components.
    /// </summary>
    public class EntityRegistry
    {
        private const int MinimumTableCapacity = 128;
        private const int TargetTableSize = 16384;

        private readonly object m_lock;
        private volatile Container m_container;
        private volatile EntityLookup m_lookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRegistry"/> class that has the
        /// default capacity.
        /// </summary>
        public EntityRegistry()
        {
            m_lock = new object();
            m_container = Container.Empty;
            m_lookup = EntityLookup.Empty;
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
            ArgumentOutOfRangeException.ThrowIfNegative(capacity);

            m_lock = new object();
            m_container = (capacity > 0)
                ? new Container(capacity)
                : Container.Empty;
            m_lookup = EntityLookup.Empty;
        }

        /// <summary>
        /// Gets the <see cref="EntityLookup"/> that stores the entity tables populated by entities
        /// within the <see cref="EntityRegistry"/>.
        /// </summary>
        public EntityLookup Lookup
        {
            get => m_lookup;
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
        public bool IsSyncPointEntered
        {
            get => Monitor.IsEntered(m_lock);
        }

        /// <summary>
        /// Creates an entity with no components.
        /// </summary>
        /// 
        /// <returns>
        /// An entity with no components.
        /// </returns>
        public Entity Create()
        {
            lock (m_lock)
            {
                Container container = m_container;
                EntityTable table = GetTable(EntityArchetype.Base);

                if (container.IsFull)
                {
                    m_container = container = container.Resize();
                }

                return container.CreateEntity(table);
            }
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
        public Entity Create(EntityArchetype archetype)
        {
            ArgumentNullException.ThrowIfNull(archetype);

            lock (m_lock)
            {
                Container container = m_container;
                EntityTable table = GetTable(archetype);

                if (container.IsFull)
                {
                    m_container = container = container.Resize();
                }

                return container.CreateEntity(table);
            }
        }

        /// <summary>
        /// Adds a new entity to the specified <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="destination">
        /// The <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <returns>
        /// The entity that was added to the <see cref="EntityTable"/>.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="destination"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="destination"/> cannot be modified by the <see cref="EntityRegistry"/> or
        /// <paramref name="destination"/> is full.
        /// </exception>
        public Entity Create(EntityTable destination)
        {
            ArgumentNullException.ThrowIfNull(destination);

            if (destination.Registry != this)
            {
                ThrowForUnmodifiableEntityTable();
            }

            lock (m_lock)
            {
                if (destination.IsFull)
                {
                    ThrowForFullEntityTable();
                }

                if (destination.IsEmpty)
                {
                    AddTable(destination);
                }

                Container container = m_container;

                if (container.IsFull)
                {
                    m_container = container = container.Resize();
                }

                return container.CreateEntity(destination);
            }
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
        public bool Destroy(Entity entity)
        {
            lock (m_lock)
            {
                EntityTable? table = m_container.DestroyEntity(entity);

                if (table == null)
                {
                    return false;
                }

                if (table.IsEmpty)
                {
                    RemoveTable(table);
                }

                return true;
            }
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
        public void Transform(Entity entity, EntityArchetype archetype)
        {
            ArgumentNullException.ThrowIfNull(archetype);

            lock (m_lock)
            {
                Container container = m_container;
                EntityTable source = container.FindEntity(entity, out _);

                if (!archetype.Equals(source.Archetype))
                {
                    container.MoveEntity(entity, GetTable(archetype));

                    if (source.IsEmpty)
                    {
                        RemoveTable(source);
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
        public void Move(Entity entity, EntityTable destination)
        {
            ArgumentNullException.ThrowIfNull(destination);

            if (destination.Registry != this)
            {
                ThrowForUnmodifiableEntityTable();
            }

            lock (m_lock)
            {
                if (destination.IsFull)
                {
                    ThrowForFullEntityTable();
                }

                Container container = m_container;
                EntityTable source = container.FindEntity(entity, out _);

                if (source != destination)
                {
                    if (destination.IsEmpty)
                    {
                        AddTable(destination);
                    }

                    container.MoveEntity(entity, destination);

                    if (source.IsEmpty)
                    {
                        RemoveTable(source);
                    }
                }
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
        public bool Contains(Entity entity)
        {
            return m_container.ContainsEntity(entity);
        }

        /// <summary>
        /// Gets the <see cref="EntityTable"/> the specified entity is stored in.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to search for.
        /// </param>
        /// 
        /// <param name="tableIndex">
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
        public EntityTable Find(Entity entity, out int tableIndex)
        {
            return m_container.FindEntity(entity, out tableIndex);
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
            ArgumentNullException.ThrowIfNull(componentType);

            lock (m_lock)
            {
                Container container = m_container;
                EntityTable source = container.FindEntity(entity, out _);

                if (source.Archetype.Contains(componentType))
                {
                    return false;
                }

                container.MoveEntity(entity, GetInclusiveTable(source.Archetype, componentType));

                if (source.IsEmpty)
                {
                    RemoveTable(source);
                }

                return true;
            }
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
            ArgumentNullException.ThrowIfNull(componentType);

            lock (m_lock)
            {
                Container container = m_container;
                EntityTable source = container.FindEntity(entity, out _);

                if (source.Archetype.Contains(componentType))
                {
                    container.MoveEntity(entity, GetExclusiveTable(source.Archetype, componentType));

                    if (source.IsEmpty)
                    {
                        RemoveTable(source);
                    }

                    return true;
                }

                return false;
            }
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
            return m_container.FindEntity(entity, out _).Archetype.Contains(componentType);
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
            lock (m_lock)
            {
                Container container = m_container;
                EntityTable source = container.FindEntity(entity, out _);
                ComponentType componentType = ComponentType.TypeOf<T>();

                if (source.Archetype.Contains(componentType))
                {
                    return false;
                }

                EntityTable destination = GetInclusiveTable(source.Archetype, componentType);
                int index = destination.Count;

                container.MoveEntity(entity, destination);

                if (destination.TryGetComponents(out Span<T?> components))
                {
                    components[index] = component;
                }

                if (source.IsEmpty)
                {
                    RemoveTable(source);
                }

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
            lock (m_lock)
            {
                Container container = m_container;
                EntityTable source = container.FindEntity(entity, out int index);
                ComponentType componentType = ComponentType.TypeOf<T>();

                if (source.Archetype.Contains(componentType))
                {
                    component = source.TryGetComponents(out Span<T?> components)
                        ? components[index]
                        : default;
                    container.MoveEntity(entity, GetInclusiveTable(source.Archetype, componentType));

                    if (source.IsEmpty)
                    {
                        RemoveTable(source);
                    }

                    return true;
                }

                component = default;
                return false;
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
            lock (m_lock)
            {
                Container container = m_container;
                EntityTable source = container.FindEntity(entity, out int index);
                ComponentType componentType = ComponentType.TypeOf<T>();
                Span<T?> components;
                bool hasDataMembers;

                if (source.Archetype.Contains(componentType))
                {
                    hasDataMembers = source.TryGetComponents(out components);
                }
                else
                {
                    EntityTable destination = GetInclusiveTable(source.Archetype, componentType);

                    index = destination.Count;
                    hasDataMembers = destination.TryGetComponents(out components);
                    container.MoveEntity(entity, destination);

                    if (source.IsEmpty)
                    {
                        RemoveTable(source);
                    }
                }

                if (hasDataMembers)
                {
                    components[index] = component;
                }
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
        /// <param name="component">
        /// A component from the entity.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the component was successfully obtained; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// 
        /// <exception cref="EntityNotFoundException">
        /// <paramref name="entity"/> does not exist within the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool TryGetComponent<T>(Entity entity, out T? component)
        {
            EntityTable source = m_container.FindEntity(entity, out int index);

            if (source.TryGetComponents(out Span<T?> components))
            {
                component = components[index];
                return true;
            }

            component = default;
            return source.Archetype.Contains(ComponentType.TypeOf<T>());
        }

        [DoesNotReturn]
        private static void ThrowForUnmodifiableEntityTable()
        {
            throw new ArgumentException(
                "The destination EntityTable cannot be modified by the EntityRegistry.",
                "destination");
        }

        [DoesNotReturn]
        private static void ThrowForFullEntityTable()
        {
            throw new ArgumentException(
                "The destination EntityTable is full.", "destination");
        }

        [DoesNotReturn]
        private static void ThrowForEntityNotFound()
        {
            throw new EntityNotFoundException(
                "The specified entity could not be found in the EntityRegistry.");
        }

        private EntityTable GetTable(EntityArchetype archetype)
        {
            EntityLookup lookup = m_lookup;

            if (lookup.TryGetGrouping(archetype, out EntityGrouping? grouping))
            {
                foreach (EntityTable value in grouping)
                {
                    if (!value.IsFull)
                    {
                        return value;
                    }
                }
            }

            return CreateTable(archetype, lookup, grouping);
        }

        private EntityTable GetInclusiveTable(EntityArchetype archetype, ComponentType componentType)
        {
            EntityLookup lookup = m_lookup;

            if (lookup.TryGetSupergrouping(archetype, componentType, out EntityGrouping? grouping))
            {
                foreach (EntityTable value in grouping)
                {
                    if (!value.IsFull)
                    {
                        return value;
                    }
                }
            }

            return CreateTable(archetype.Add(componentType), lookup, grouping);
        }

        private EntityTable GetExclusiveTable(EntityArchetype archetype, ComponentType componentType)
        {
            EntityLookup lookup = m_lookup;

            if (lookup.TryGetSubgrouping(archetype, componentType, out EntityGrouping? grouping))
            {
                foreach (EntityTable value in grouping)
                {
                    if (!value.IsFull)
                    {
                        return value;
                    }
                }
            }

            return CreateTable(archetype.Remove(componentType), lookup, grouping);
        }

        private EntityTable CreateTable(EntityArchetype archetype,
            EntityLookup lookup, EntityGrouping? grouping)
        {
            EntityTable table = new EntityTable(archetype, this,
                Math.Max(TargetTableSize / archetype.EntitySize, MinimumTableCapacity));

            m_lookup = (grouping != null)
                ? lookup.AddOrUpdate(grouping.Add(table))
                : lookup.Add(EntityGrouping.Create(table));
            return table;
        }

        private void AddTable(EntityTable table)
        {
            EntityLookup lookup = m_lookup;

            m_lookup = lookup.TryGetGrouping(table.Archetype, out EntityGrouping? grouping)
                ? lookup.AddOrUpdate(grouping.Add(table))
                : lookup.Add(EntityGrouping.Create(table));
        }

        private void RemoveTable(EntityTable table)
        {
            EntityLookup lookup = m_lookup;
            EntityGrouping grouping = lookup[table.Archetype];

            m_lookup = (grouping.Count > 1)
                ? lookup.AddOrUpdate(grouping.Remove(table))
                : lookup.Remove(grouping);
        }

        private sealed class Container
        {
            private const int DefaultCapacity = 4;

            public static readonly Container Empty = new Container();

            private readonly Entry[] m_entries;
            private readonly int[] m_indexPool;
            private int m_size;
            private int m_nextIndex;

            public Container(int capacity)
            {
                m_entries = new Entry[capacity];
                m_indexPool = new int[capacity];
            }

            private Container()
            {
                m_entries = Array.Empty<Entry>();
                m_indexPool = Array.Empty<int>();
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

            public Entity CreateEntity(EntityTable destination)
            {
                Entity entity = CreateEntry(destination);

                destination.Add(entity);
                return entity;
            }

            public EntityTable? DestroyEntity(Entity entity)
            {
                ref Entry entry = ref FindEntry(entity);

                if (Unsafe.IsNullRef(ref entry))
                {
                    return null;
                }

                EntityTable table = entry.Table;
                int tableIndex = entry.TableIndex;

                entry.Table = null!;
                entry.TableIndex = -1;
                entry.Version++;
                table.Delete(tableIndex);

                if (tableIndex < table.Count)
                {
                    m_entries[table.GetEntities()[tableIndex].Index].TableIndex = tableIndex;
                }

                m_indexPool[m_nextIndex - m_size--] = entity.Index;
                return table;
            }

            public void MoveEntity(Entity entity, EntityTable destination)
            {
                ref Entry entry = ref m_entries[entity.Index];
                EntityTable source = entry.Table;
                int sourceIndex = entry.TableIndex;

                entry.Table = destination;
                entry.TableIndex = destination.Count;
                destination.Import(entity, source, sourceIndex);
                source.Delete(sourceIndex);

                if (sourceIndex < source.Count)
                {
                    m_entries[source.GetEntities()[sourceIndex].Index].TableIndex = sourceIndex;
                }
            }

            public bool ContainsEntity(Entity entity)
            {
                return !Unsafe.IsNullRef(ref FindEntry(entity));
            }

            public EntityTable FindEntity(Entity entity, out int tableIndex)
            {
                ref Entry entry = ref FindEntry(entity);

                if (Unsafe.IsNullRef(ref entry))
                {
                    ThrowForEntityNotFound();
                }

                tableIndex = entry.TableIndex;
                return entry.Table;
            }

            public Container Resize()
            {
                int size = m_size;
                int capacity;

                if (size == 0)
                {
                    capacity = DefaultCapacity;
                }
                else
                {
                    capacity = size * 2;

                    if ((uint)capacity > (uint)Array.MaxLength)
                    {
                        capacity = Array.MaxLength;

                        if (capacity <= size)
                        {
                            capacity = size + 1;
                        }
                    }
                }

                Container container = new Container(capacity)
                {
                    m_size = size,
                    m_nextIndex = size
                };

                Array.Copy(m_entries, container.m_entries, size);
                return container;
            }

            private Entity CreateEntry(EntityTable table)
            {
                int index = (m_size++ < m_nextIndex)
                    ? m_indexPool[m_nextIndex - m_size]
                    : m_nextIndex++;
                ref Entry entry = ref m_entries[index];

                entry.Table = table;
                entry.TableIndex = table.Count;
                return new Entity(index, entry.Version);
            }

            private ref Entry FindEntry(Entity entity)
            {
                if ((uint)entity.Index < (uint)m_nextIndex)
                {
                    ref Entry entry = ref m_entries[entity.Index];

                    if (entry.Table != null && entry.TableIndex >= 0 && entry.Version == entity.Version)
                    {
                        return ref entry;
                    }
                }

                return ref Unsafe.NullRef<Entry>();
            }

            private struct Entry
            {
                public EntityTable Table;
                public int TableIndex;
                public int Version;
            }
        }
    }
}
