// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities
{
    /// <summary>
    /// Represents a table of entities each associated with a set of components.
    /// </summary>
    public class EntityTable
    {
        private const int MinimumCapacity = 8;

        private readonly EntityArchetype m_archetype;
        private readonly EntityRegistry? m_registry;
        private readonly Array[] m_components;
        private readonly Entity[] m_entities;
        private int m_size;
        private int m_version;

        /// <summary>
        /// Initializes new instance of the <see cref="EntityTable"/> class that contains non-tag
        /// components described by the specified entity archetype and has the default capacity.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The entity archetype.
        /// </param>
        public EntityTable(EntityArchetype archetype)
            : this(archetype, null, MinimumCapacity)
        {
        }

        /// <summary>
        /// Initializes new instance of the <see cref="EntityTable"/> class that contains non-tag
        /// components described by the specified entity archetype and has the specified capacity.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The entity archetype.
        /// </param>
        /// 
        /// <param name="capacity">
        /// The capacity of the <see cref="EntityTable"/>.
        /// </param>
        public EntityTable(EntityArchetype archetype, int capacity)
            : this(archetype, null, capacity)
        {
        }

        /// <summary>
        /// Initializes new instance of the <see cref="EntityTable"/> class that contains non-tag
        /// components described by the specified entity archetype, is managed by the specified
        /// entity registry, and has the default capacity.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The entity archetype.
        /// </param>
        /// 
        /// <param name="registry">
        /// The entity registry that manages the entity table, if not <see langword="null"/>.
        /// </param>
        public EntityTable(EntityArchetype archetype, EntityRegistry? registry)
            : this(archetype, registry, MinimumCapacity)
        {
        }

        /// <summary>
        /// Initializes new instance of the <see cref="EntityTable"/> class that contains non-tag
        /// components described by the specified entity archetype, is managed by the specified
        /// entity registry, and has the specified capacity.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The entity archetype.
        /// </param>
        /// 
        /// <param name="registry">
        /// The entity registry that manages the entity table, if not <see langword="null"/>.
        /// </param>
        /// 
        /// <param name="capacity">
        /// The capacity of the <see cref="EntityTable"/>.
        /// </param>
        public EntityTable(EntityArchetype archetype, EntityRegistry? registry, int capacity)
        {
            ArgumentNullException.ThrowIfNull(archetype);

            if (capacity < MinimumCapacity)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(capacity);
                capacity = MinimumCapacity;
            }

            ReadOnlySpan<ComponentType> componentTypes = archetype.ComponentTypes.Slice(0,
                archetype.ManagedComponentCount + archetype.UnmanagedComponentCount);
            m_archetype = archetype;
            m_registry = registry;

            if (componentTypes.Length > 0)
            {
                m_components = new Array[componentTypes.Length];

                for (int i = 0; i < componentTypes.Length; i++)
                {
                    m_components[i] = Array.CreateInstance(componentTypes[i].Type, capacity);
                }
            }
            else
            {
                m_components = Array.Empty<Array>();
            }

            m_entities = new Entity[capacity];
        }

        /// <summary>
        /// Gets the entity archetype that describes the layout of the <see cref="EntityTable"/>.
        /// </summary>
        public EntityArchetype Archetype
        {
            get => m_archetype;
        }

        /// <summary>
        /// Gets the entity registry that manages the <see cref="EntityTable"/>, or
        /// <see langword="null"/> if the <see cref="EntityTable"/> is not managed by an entity
        /// registry.
        /// </summary>
        public EntityRegistry? Registry
        {
            get => m_registry;
        }

        /// <summary>
        /// Gets the total number of entities the <see cref="EntityTable"/> can hold before it
        /// becomes full.
        /// </summary>
        public int Capacity
        {
            get => m_entities.Length;
        }

        /// <summary>
        /// Get the number of entities in the <see cref="EntityTable"/>.
        /// </summary>
        public int Count
        {
            get => m_size;
        }

        /// <summary>
        /// Gets the current version of the <see cref="EntityTable"/>.
        /// </summary>
        public int Version
        {
            get => m_version;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityTable"/> does not contain any
        /// entities.
        /// </summary>
        public bool IsEmpty
        {
            get => m_size == 0;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityTable"/> has reach its
        /// maximum capacity.
        /// </summary>
        public bool IsFull
        {
            get => m_size == m_entities.Length;
        }

        /// <summary>
        /// Returns a value that indicates whether the caller is allowed to make structure changes
        /// to the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <returns>
        /// <see langword="true"/> if the caller is allowed to make structure changes to the
        /// <see cref="EntityTable"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool CheckAccess()
        {
            return m_registry == null || m_registry.AllowStructureChanges;
        }

        /// <summary>
        /// Gets a span of components stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the components.
        /// </typeparam>
        /// 
        /// <returns>
        /// A span of components stored by the <see cref="EntityTable"/>.
        /// </returns>
        public Span<T> GetComponents<T>()
        {
            int index = m_archetype.IndexOf(ComponentType.TypeOf<T>());

            if (index == -1)
            {
                throw new ArgumentException(
                    $"The EntityTable does not store components of type {typeof(T).Name}.");
            }

            return new Span<T>((T[])m_components[index]);
        }

        /// <summary>
        /// Attempts to get a span of components stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the components.
        /// </typeparam>
        /// 
        /// <param name="components">
        /// A span of components stored by the <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the span was successfully obtained; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetComponents<T>(out Span<T> components)
        {
            int index = m_archetype.IndexOf(ComponentType.TypeOf<T>());

            if (index == -1)
            {
                components = default;
                return false;
            }

            components = new Span<T>((T[])m_components[index]);
            return true;
        }

        /// <summary>
        /// Gets a span of entities stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A span of entities stored by the <see cref="EntityTable"/>.
        /// </returns>
        public ReadOnlySpan<Entity> GetEntities()
        {
            return new ReadOnlySpan<Entity>(m_entities);
        }

        /// <summary>
        /// Adds an entity to the end of the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to be added to the end of the <see cref="EntityTable"/>.
        /// </param>
        public void Add(Entity entity)
        {
            VerifyAccess();

            int size = m_size;
            Entity[] entities = m_entities;

            if ((uint)size >= (uint)entities.Length)
            {
                throw new InvalidOperationException("The EntityTable is full.");
            }

            Array[] components = m_components;

            // Zero-initialize unmanaged components.
            for (int i = m_archetype.ManagedComponentCount; i < components.Length; i++)
            {
                Array.Clear(components[i], size, 1);
            }

            entities[size] = entity;
            m_size = size + 1;
            m_version++;
        }

        /// <summary>
        /// Copies a range of entities from the specified entity table and adds them to the end of
        /// the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="table">
        /// The entity table to copy from.
        /// </param>
        /// 
        /// <param name="tableIndex">
        /// The index at which entities will be copied from.
        /// </param>
        /// 
        /// <param name="count">
        /// The number of entities to add.
        /// </param>
        public void AddRange(EntityTable table, int tableIndex, int count)
        {
            VerifyAccess();
            ArgumentNullException.ThrowIfNull(table);

            if ((uint)tableIndex >= (uint)table.m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(tableIndex), tableIndex,
                    "Table index was out of range. Must be non-negative and less than the size of the table.");
            }

            ArgumentOutOfRangeException.ThrowIfNegative(count);
            
            if (table.m_size - count < tableIndex)
            {
                throw new ArgumentException("Count exceeds the size of the table.", nameof(count));
            }

            int size = m_size;
            Entity[] entities = m_entities;

            if (entities.Length - count < size)
            {
                throw new ArgumentException("Count exceeds the capacity of the EntityTable.", nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            Array[] sourceComponents = table.m_components;
            Array[] destinationComponents = m_components;
            ReadOnlySpan<ComponentType> sourceComponentTypes = table.m_archetype.ComponentTypes.Slice(0, sourceComponents.Length);
            ReadOnlySpan<ComponentType> destinationComponentTypes = m_archetype.ComponentTypes.Slice(0, destinationComponents.Length);
            ComponentType sourceComponentType = null!;
            int sourceIndex = -1;

            for (int destinationIndex = 0; destinationIndex < destinationComponentTypes.Length; destinationIndex++)
            {
                ComponentType destinationComponentType = destinationComponentTypes[destinationIndex];
                bool recompare;

                do
                {
                    switch (ComponentType.Compare(sourceComponentType, destinationComponentType))
                    {
                        case 0:
                            Array.Copy(sourceComponents[sourceIndex], tableIndex, destinationComponents[destinationIndex], size, count);
                            recompare = false;
                            continue;
                        case 1:
                            if (destinationComponentType.Category == ComponentTypeCategory.Unmanaged)
                            {
                                Array.Clear(destinationComponents[destinationIndex], size, count);
                            }

                            recompare = false;
                            continue;
                        default:
                            int nextSourceIndex = sourceIndex + 1;

                            if (recompare = nextSourceIndex < sourceComponentTypes.Length)
                            {
                                sourceIndex = nextSourceIndex;
                                sourceComponentType = sourceComponentTypes[sourceIndex];
                            }

                            continue;
                    }
                }
                while (recompare);
            }

            Array.Copy(table.m_entities, tableIndex, entities, size, count);
            m_size = size + count;
            m_version++;
        }

        /// <summary>
        /// Removes all entities from the <see cref="EntityTable"/>.
        /// </summary>
        public void Clear()
        {
            VerifyAccess();

            int size = m_size;
            int managedComponentCount = m_archetype.ManagedComponentCount;
            Array[] components = m_components;

            // Free references in managed components.
            for (int i = 0; i < managedComponentCount; i++)
            {
                Array.Clear(components[i], 0, size);
            }

            m_size = 0;
            m_version++;
        }

        /// <summary>
        /// Removes the first occurance of the specified entity from the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="entity">
        /// The entity to remove.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity was sucessfully removed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Remove(Entity entity)
        {
            int index;

            if (CheckAccess() && (index = Array.IndexOf(m_entities, entity, 0, m_size)) != -1)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the entity at the specified index of the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="index">
        /// The zero-based index of the entity to remove.
        /// </param>
        public void RemoveAt(int index)
        {
            VerifyAccess();
            
            int size = m_size;

            if ((uint)index >= (uint)size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    "Index was out of range. Must be non-negative and less than the size of the entity table.");
            }

            Array[] components = m_components;

            if (index < --size)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    Array array = components[i];
                    Array.Copy(array, size, array, index, 1);
                }

                m_entities[index] = m_entities[size];
            }

            int managedComponentCount = m_archetype.ManagedComponentCount;

            // Free references in managed components.
            for (int i = 0; i < managedComponentCount; i++)
            {
                Array.Clear(components[i], size, 1);
            }

            m_size = size;
            m_version++;
        }

        /// <summary>
        /// Removes a range of entities from the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="index">
        /// The zero-based starting index of the range of entities to remove.
        /// </param>
        /// 
        /// <param name="count">
        /// The number of entities to remove.
        /// </param>
        public void RemoveRange(int index, int count)
        {
            VerifyAccess();
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            int size = m_size - count;

            if (size < index)
            {
                throw new ArgumentException("Count exceeds the size of the EntityTable.", nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            Array[] components = m_components;

            if (index < size)
            {
                int copyIndex = index + count;
                int copyLength = size - index;

                for (int i = 0; i < components.Length; i++)
                {
                    Array array = components[i];
                    Array.Copy(array, copyIndex, array, index, copyLength);
                }

                Array.Copy(m_entities, copyIndex, m_entities, index, copyLength);
            }

            int managedComponentCount = m_archetype.ManagedComponentCount;

            // Free references in managed components.
            for (int i = 0; i < managedComponentCount; i++)
            {
                Array.Clear(components[i], size, count);
            }

            m_size = size;
            m_version++;
        }

        /// <summary>
        /// Throws an InvalidOperationException if the caller is not allowed to make structure
        /// changes to the <see cref="EntityTable"/>.
        /// </summary>
        public void VerifyAccess()
        {
            if (m_registry != null && !m_registry.AllowStructureChanges)
            {
                throw new InvalidOperationException(
                    "The entity table cannot be modified while structure changes are disallowed by its entity registry.");
            }
        }
    }
}
