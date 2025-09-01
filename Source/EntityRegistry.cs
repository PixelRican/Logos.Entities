// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a registry that stores and maintains a record of entities and their components.
    /// </summary>
    public class EntityRegistry
    {
        private const int MinimumTableCapacity = 128;
        private const int TargetTableSize = 16384;

        private readonly object m_lock;
        private volatile EntityLookup m_lookup;
        private volatile RecordContainer m_container;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRegistry"/> class that is empty and
        /// has the default initial capacity.
        /// </summary>
        public EntityRegistry()
        {
            m_lock = new object();
            m_lookup = EntityLookup.Empty;
            m_container = RecordContainer.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRegistry"/> class that has the
        /// specified capacity.
        /// </summary>
        /// <param name="capacity">
        /// The capacity of the <see cref="EntityRegistry"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is negative.
        /// </exception>
        public EntityRegistry(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(capacity);

            m_lock = new object();
            m_lookup = EntityLookup.Empty;
            m_container = (capacity > 0)
                ? new RecordContainer(capacity)
                : RecordContainer.Empty;
        }

        /// <summary>
        /// Gets the total number of entities the internal data structure can hold without resizing.
        /// </summary>
        /// <returns>
        /// The total number of entities the internal data structure can hold without resizing.
        /// </returns>
        public int Capacity
        {
            get => m_container.Capacity;
        }

        /// <summary>
        /// Gets the number of entities in the <see cref="EntityRegistry"/>.
        /// </summary>
        /// <returns>
        /// The number of entities in the <see cref="EntityRegistry"/>.
        /// </returns>
        public int Count
        {
            get => m_container.Count;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityRegistry"/> has entered a sync
        /// point in the current thread.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityRegistry"/> has entered a sync point in
        /// the current thread; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsSyncPointEntered
        {
            get => Monitor.IsEntered(m_lock);
        }

        /// <summary>
        /// Gets a lookup that contains groupings of tables populated with entities from the
        /// <see cref="EntityRegistry"/>.
        /// </summary>
        /// <returns>
        /// A lookup that contains groupings of tables populated with entities from the
        /// <see cref="EntityRegistry"/>.
        /// </returns>
        public EntityLookup Lookup
        {
            get => m_lookup;
        }

        /// <summary>
        /// Creates an entity with no components.
        /// </summary>
        /// <returns>
        /// An entity with no components.
        /// </returns>
        public Entity Create()
        {
            lock (m_lock)
            {
                RecordContainer container = m_container;
                EntityTable table = GetTable(EntityArchetype.Base);

                if (container.IsFull)
                {
                    m_container = container = container.Resize();
                }

                return container.CreateEntity(table);
            }
        }

        /// <summary>
        /// Creates an entity that is modelled by the specified archetype.
        /// </summary>
        /// <param name="archetype">
        /// The archetype that describes the composition of the entity to create.
        /// </param>
        /// <returns>
        /// An entity that is modelled by <paramref name="archetype"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        public Entity Create(EntityArchetype archetype)
        {
            ArgumentNullException.ThrowIfNull(archetype);

            lock (m_lock)
            {
                RecordContainer container = m_container;
                EntityTable table = GetTable(archetype);

                if (container.IsFull)
                {
                    m_container = container = container.Resize();
                }

                return container.CreateEntity(table);
            }
        }

        /// <summary>
        /// Creates an entity and adds it to a new row at the end of the specified table.
        /// </summary>
        /// <param name="destination">
        /// The table that is the destination of the created entity.
        /// </param>
        /// <returns>
        /// The entity that was added to <paramref name="destination"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="destination"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="destination"/> cannot be modified by the <see cref="EntityRegistry"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="destination"/> is full.
        /// </exception>
        public Entity Create(EntityTable destination)
        {
            ThrowIfNullOrUnmodifiable(destination);

            lock (m_lock)
            {
                if (destination.IsFull)
                {
                    ThrowForFullTable();
                }

                if (destination.IsEmpty)
                {
                    AddTable(destination);
                }

                RecordContainer container = m_container;

                if (container.IsFull)
                {
                    m_container = container = container.Resize();
                }

                return container.CreateEntity(destination);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="EntityRegistry"/> contains a specified entity.
        /// </summary>
        /// <param name="entity">
        /// The entity to locate in the <see cref="EntityRegistry"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="entity"/> is found in the
        /// <see cref="EntityRegistry"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(Entity entity)
        {
            return m_container.ContainsEntity(entity);
        }

        /// <summary>
        /// Gets the table that contains the specified entity in the
        /// <see cref="EntityRegistry"/>.
        /// </summary>
        /// <param name="entity">
        /// The entity to locate in the <see cref="EntityRegistry"/>.
        /// </param>
        /// <param name="index">
        /// When this method returns, contains the row index of <paramref name="entity"/> in the
        /// returned table.
        /// </param>
        /// <returns>
        /// The table that contains <paramref name="entity"/>.
        /// </returns>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public EntityTable Find(Entity entity, out int index)
        {
            return m_container.FindEntity(entity, out index);
        }

        /// <summary>
        /// Exports the specified entity to a new row at the end of the specified table.
        /// </summary>
        /// <param name="entity">
        /// The entity to export to a new row at the end of <paramref name="destination"/>.
        /// </param>
        /// <param name="destination">
        /// The destination table to export <paramref name="entity"/> into.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="destination"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="destination"/> cannot be modified by the <see cref="EntityRegistry"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="destination"/> is full.
        /// </exception>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public void Export(Entity entity, EntityTable destination)
        {
            ThrowIfNullOrUnmodifiable(destination);

            lock (m_lock)
            {
                if (destination.IsFull)
                {
                    ThrowForFullTable();
                }

                RecordContainer container = m_container;
                EntityTable source = container.FindEntity(entity, out _);

                if (source != destination)
                {
                    if (destination.IsEmpty)
                    {
                        AddTable(destination);
                    }

                    container.ExportEntity(entity, destination);

                    if (source.IsEmpty)
                    {
                        RemoveTable(source);
                    }
                }
            }
        }

        /// <summary>
        /// Transforms the composition of the specified entity according to the model represented by
        /// the specified archetype.
        /// </summary>
        /// <param name="entity">
        /// The entity to transform.
        /// </param>
        /// <param name="archetype">
        /// The archetype that describes the composition of <paramref name="entity"/> after its
        /// transformation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public void Transform(Entity entity, EntityArchetype archetype)
        {
            ArgumentNullException.ThrowIfNull(archetype);

            lock (m_lock)
            {
                RecordContainer container = m_container;
                EntityTable source = container.FindEntity(entity, out _);

                if (!archetype.Equals(source.Archetype))
                {
                    container.ExportEntity(entity, GetTable(archetype));

                    if (source.IsEmpty)
                    {
                        RemoveTable(source);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the specified entity from the <see cref="EntityRegistry"/>.
        /// </summary>
        /// <param name="entity">
        /// The entity to delete from the <see cref="EntityRegistry"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="entity"/> was sucessfully deleted; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Delete(Entity entity)
        {
            lock (m_lock)
            {
                EntityTable? table = m_container.DeleteEntity(entity);

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
        /// Adds the default value of the specified component type to the specified entity.
        /// </summary>
        /// <param name="entity">
        /// The entity to add the default value of <paramref name="componentType"/> to.
        /// </param>
        /// <param name="componentType">
        /// The type of the component to add to <paramref name="entity"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the default value of <paramref name="componentType"/> was
        /// successfully added to <paramref name="entity"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool AddComponent(Entity entity, ComponentType componentType)
        {
            ArgumentNullException.ThrowIfNull(componentType);

            lock (m_lock)
            {
                RecordContainer container = m_container;
                EntityTable source = container.FindEntity(entity, out _);

                if (source.Archetype.Contains(componentType))
                {
                    return false;
                }

                container.ExportEntity(entity, GetTableWithComponent(source.Archetype, componentType));

                if (source.IsEmpty)
                {
                    RemoveTable(source);
                }

                return true;
            }
        }

        /// <summary>
        /// Removes an instance of the specified component type from the specified entity.
        /// </summary>
        /// <param name="entity">
        /// The entity to remove an instance of <paramref name="componentType"/> from.
        /// </param>
        /// <param name="componentType">
        /// The type of the component to remove from <paramref name="entity"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an instance of <paramref name="componentType"/> was
        /// successfully removed from <paramref name="entity"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool RemoveComponent(Entity entity, ComponentType componentType)
        {
            ArgumentNullException.ThrowIfNull(componentType);

            lock (m_lock)
            {
                RecordContainer container = m_container;
                EntityTable source = container.FindEntity(entity, out _);

                if (source.Archetype.Contains(componentType))
                {
                    container.ExportEntity(entity, GetTableWithoutComponent(source.Archetype, componentType));

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
        /// Determines whether the specified entity contains an instance of the specified component
        /// type.
        /// </summary>
        /// <param name="entity">
        /// The entity to locate an instance of <paramref name="componentType"/> in.
        /// </param>
        /// <param name="componentType">
        /// The type of the component to locate in <paramref name="entity"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="entity"/> contains an instance of
        /// <paramref name="componentType"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool HasComponent(Entity entity, ComponentType componentType)
        {
            return m_container.FindEntity(entity, out _).Archetype.Contains(componentType);
        }

        /// <summary>
        /// Adds the specified component to the specified entity.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the component to add.
        /// </typeparam>
        /// <param name="entity">
        /// The entity to add <paramref name="component"/> to.
        /// </param>
        /// <param name="component">
        /// The component to add to <paramref name="entity"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="component"/> was successfully added to
        /// <paramref name="entity"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool AddComponent<T>(Entity entity, T? component)
        {
            lock (m_lock)
            {
                RecordContainer container = m_container;
                EntityTable source = container.FindEntity(entity, out _);
                ComponentType componentType = ComponentType.TypeOf<T>();

                if (source.Archetype.Contains(componentType))
                {
                    return false;
                }

                EntityTable destination = GetTableWithComponent(source.Archetype, componentType);
                int index = destination.Count;

                container.ExportEntity(entity, destination);

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
        /// <typeparam name="T">
        /// The type of the component to remove.
        /// </typeparam>
        /// <param name="entity">
        /// The entity to remove a component of type <typeparamref name="T"/> from.
        /// </param>
        /// <param name="component">
        /// When this method returns, contains the component removed from <paramref name="entity"/>,
        /// if it contained a component of type <typeparamref name="T"/>; otherwise, the default
        /// value of <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a component of type <typeparamref name="T"/> was successfully
        /// removed from <paramref name="entity"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public bool RemoveComponent<T>(Entity entity, out T? component)
        {
            lock (m_lock)
            {
                RecordContainer container = m_container;
                EntityTable source = container.FindEntity(entity, out int index);
                ComponentType componentType = ComponentType.TypeOf<T>();

                if (source.Archetype.Contains(componentType))
                {
                    component = source.TryGetComponents(out Span<T?> components)
                        ? components[index]
                        : default;
                    container.ExportEntity(entity, GetTableWithComponent(source.Archetype, componentType));

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
        /// <typeparam name="T">
        /// The type of the component to set.
        /// </typeparam>
        /// <param name="entity">
        /// The entity to set <paramref name="component"/> to.
        /// </param>
        /// <param name="component">
        /// The component to set to <paramref name="entity"/>.
        /// </param>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
        /// </exception>
        public void SetComponent<T>(Entity entity, T? component)
        {
            lock (m_lock)
            {
                RecordContainer container = m_container;
                EntityTable source = container.FindEntity(entity, out int index);
                ComponentType componentType = ComponentType.TypeOf<T>();
                Span<T?> components;
                bool isWriteable;

                if (source.Archetype.Contains(componentType))
                {
                    isWriteable = source.TryGetComponents(out components);
                }
                else
                {
                    EntityTable destination = GetTableWithComponent(source.Archetype, componentType);

                    index = destination.Count;
                    isWriteable = destination.TryGetComponents(out components);
                    container.ExportEntity(entity, destination);

                    if (source.IsEmpty)
                    {
                        RemoveTable(source);
                    }
                }

                if (isWriteable)
                {
                    components[index] = component;
                }
            }
        }

        /// <summary>
        /// Gets a component of the specified type from the specified entity.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the component to get.
        /// </typeparam>
        /// <param name="entity">
        /// The entity to get a component of type <typeparamref name="T"/> from.
        /// </param>
        /// <param name="component">
        /// When this method returns, contains the component held by <paramref name="entity"/>, if
        /// it contained a component of type <typeparamref name="T"/>; otherwise, the default value
        /// of <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="entity"/> contained a component of type
        /// <typeparamref name="T"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="EntityNotFoundException">
        /// Unable to find <paramref name="entity"/> in the <see cref="EntityRegistry"/>.
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
        private static void ThrowForUnmodifiableTable()
        {
            throw new ArgumentException(
                "The destination table cannot be modified by the EntityRegistry.", "destination");
        }

        [DoesNotReturn]
        private static void ThrowForFullTable()
        {
            throw new ArgumentException("The destination table is full.", "destination");
        }

        [DoesNotReturn]
        private static void ThrowForEntityNotFound()
        {
            throw new EntityNotFoundException(
                "Unable to find the specified entity in the EntityRegistry.");
        }

        private void ThrowIfNullOrUnmodifiable(EntityTable destination)
        {
            ArgumentNullException.ThrowIfNull(destination);

            if (destination.Registry != this)
            {
                ThrowForUnmodifiableTable();
            }
        }

        private void AddTable(EntityTable table)
        {
            EntityLookup lookup = m_lookup;

            m_lookup = lookup.TryGetValue(table.Archetype, out EntityGrouping? grouping)
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

        private EntityTable GetTable(EntityArchetype archetype)
        {
            EntityLookup lookup = m_lookup;

            if (lookup.TryGetValue(archetype, out EntityGrouping? grouping))
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

        private EntityTable GetTableWithComponent(EntityArchetype archetype, ComponentType componentType)
        {
            EntityLookup lookup = m_lookup;

            if (lookup.TryGetValueWith(archetype, componentType, out EntityGrouping? grouping))
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

        private EntityTable GetTableWithoutComponent(EntityArchetype archetype, ComponentType componentType)
        {
            EntityLookup lookup = m_lookup;

            if (lookup.TryGetValueWithout(archetype, componentType, out EntityGrouping? grouping))
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

        private EntityTable CreateTable(EntityArchetype archetype, EntityLookup lookup, EntityGrouping? grouping)
        {
            int capacity = TargetTableSize / archetype.EntitySize;

            if (capacity < MinimumTableCapacity)
            {
                capacity = MinimumTableCapacity;
            }

            EntityTable table = new EntityTable(archetype, this, capacity);

            m_lookup = (grouping != null)
                ? lookup.AddOrUpdate(grouping.Add(table))
                : lookup.Add(EntityGrouping.Create(table));
            return table;
        }

        private sealed class RecordContainer
        {
            private const int DefaultCapacity = 4;

            public static readonly RecordContainer Empty = new RecordContainer();

            private readonly Record[] m_records;
            private readonly int[] m_freeIndices;
            private int m_size;
            private int m_nextIndex;

            public RecordContainer(int capacity)
            {
                m_records = new Record[capacity];
                m_freeIndices = new int[capacity];
            }

            private RecordContainer()
            {
                m_records = Array.Empty<Record>();
                m_freeIndices = Array.Empty<int>();
            }

            public int Capacity
            {
                get => m_records.Length;
            }

            public int Count
            {
                get => m_size;
            }

            public bool IsFull
            {
                get => m_size == m_records.Length;
            }

            public Entity CreateEntity(EntityTable table)
            {
                int index = (m_size++ < m_nextIndex)
                    ? m_freeIndices[m_nextIndex - m_size]
                    : m_nextIndex++;
                ref Record record = ref m_records[index];
                int version = record.Version;

                record.Table = table;
                record.Index = table.Count;
                table.CreateRow(new Entity(index, version));
                return new Entity(index, version);
            }

            public EntityTable? DeleteEntity(Entity entity)
            {
                ref Record record = ref FindRecord(entity);

                if (Unsafe.IsNullRef(ref record))
                {
                    return null;
                }

                EntityTable table = record.Table;
                int tableIndex = record.Index;

                record.Table = null!;
                record.Index = -1;
                record.Version++;
                table.DeleteRow(tableIndex);

                if (tableIndex < table.Count)
                {
                    m_records[table.GetEntities()[tableIndex].Index].Index = tableIndex;
                }

                m_freeIndices[m_nextIndex - m_size--] = entity.Index;
                return table;
            }

            public void ExportEntity(Entity entity, EntityTable destination)
            {
                ref Record record = ref m_records[entity.Index];
                EntityTable source = record.Table;
                int sourceIndex = record.Index;

                record.Table = destination;
                record.Index = destination.Count;
                destination.ImportRow(source, sourceIndex);
                source.DeleteRow(sourceIndex);

                if (sourceIndex < source.Count)
                {
                    m_records[source.GetEntities()[sourceIndex].Index].Index = sourceIndex;
                }
            }

            public bool ContainsEntity(Entity entity)
            {
                return !Unsafe.IsNullRef(ref FindRecord(entity));
            }

            public EntityTable FindEntity(Entity entity, out int index)
            {
                ref Record record = ref FindRecord(entity);

                if (Unsafe.IsNullRef(ref record))
                {
                    ThrowForEntityNotFound();
                }

                index = record.Index;
                return record.Table;
            }

            public RecordContainer Resize()
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

                RecordContainer container = new RecordContainer(capacity)
                {
                    m_size = size,
                    m_nextIndex = size
                };

                Array.Copy(m_records, container.m_records, size);
                return container;
            }

            private ref Record FindRecord(Entity entity)
            {
                if ((uint)entity.Index < (uint)m_nextIndex)
                {
                    ref Record record = ref m_records[entity.Index];

                    if (record.Table != null && record.Index >= 0 && record.Version == entity.Version)
                    {
                        return ref record;
                    }
                }

                return ref Unsafe.NullRef<Record>();
            }

            private struct Record
            {
                public EntityTable Table;
                public int Index;
                public int Version;
            }
        }
    }
}
