// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

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

            m_archetype = archetype;
            m_registry = registry;

            ImmutableArray<ComponentType> componentTypes = archetype.ComponentTypes;
            int componentCount = archetype.ManagedComponentCount + archetype.UnmanagedComponentCount;

            if (componentCount > 0)
            {
                m_components = new Array[componentCount--];

                do
                {
                    m_components[componentCount] = Array.CreateInstance(
                        componentTypes[componentCount].Type, capacity);
                }
                while (--componentCount >= 0);
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
        /// Gets an array of components stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The type of the components.
        /// </param>
        /// 
        /// <returns>
        /// An array of components stored by the <see cref="EntityTable"/>.
        /// </returns>
        public Array GetComponents(ComponentType componentType)
        {
            ArgumentNullException.ThrowIfNull(componentType);
            return FindComponents(componentType, throwIfNotFound: true)!;
        }

        /// <summary>
        /// Gets an array of components stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the components.
        /// </typeparam>
        /// 
        /// <returns>
        /// An array of components stored by the <see cref="EntityTable"/>.
        /// </returns>
        public T[] GetComponents<T>()
        {
            return (T[])FindComponents(ComponentType.TypeOf<T>(), throwIfNotFound: true)!;
        }

        /// <summary>
        /// Attempts to get an array of components stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The type of the components.
        /// </param>
        /// 
        /// <param name="components">
        /// An array of components stored by the <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the array was successfully obtained; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetComponents(ComponentType componentType, [NotNullWhen(true)] out Array? components)
        {
            ArgumentNullException.ThrowIfNull(componentType);
            components = FindComponents(componentType, throwIfNotFound: false);
            return components != null;
        }

        /// <summary>
        /// Attempts to get an array of components stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the components.
        /// </typeparam>
        /// 
        /// <param name="components">
        /// An array of components stored by the <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the array was successfully obtained; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetComponents<T>([NotNullWhen(true)] out T[]? components)
        {
            components = FindComponents(ComponentType.TypeOf<T>(), throwIfNotFound: false) as T[];
            return components != null;
        }

        private Array? FindComponents(ComponentType componentType, bool throwIfNotFound)
        {
            int index = m_archetype.IndexOf(componentType);

            if ((uint)index >= (uint)m_components.Length)
            {
                if (throwIfNotFound)
                {
                    throw new ArgumentException(
                        $"The EntityTable does not store components of type {componentType.Type.Name}.");
                }

                return null;
            }

            return m_components[index];
        }

        /// <summary>
        /// Gets an array of entities stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An array of entities stored by the <see cref="EntityTable"/>.
        /// </returns>
        public Entity[] GetEntities()
        {
            return m_entities;
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

            Entity[] entities = m_entities;
            int size = m_size;

            if (entities.Length - count < size)
            {
                throw new ArgumentException("Count exceeds the capacity of the EntityTable.", nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            ImmutableArray<ComponentType> sourceComponentTypes = table.m_archetype.ComponentTypes;
            ImmutableArray<ComponentType> destinationComponentTypes = m_archetype.ComponentTypes;
            Array[] sourceComponents = table.m_components;
            Array[] destinationComponents = m_components;
            ComponentType sourceComponentType = null!;
            int sourceIndex = -1;

            for (int destinationIndex = 0; destinationIndex < destinationComponents.Length; destinationIndex++)
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

                            if (recompare = nextSourceIndex < sourceComponents.Length)
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
            int managedPartitionLength = m_archetype.ManagedComponentCount;
            Array[] components = m_components;

            // Frees references to managed objects.
            for (int i = 0; i < managedPartitionLength; i++)
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

            int managedPartitionLength = m_archetype.ManagedComponentCount;

            // Frees references to managed objects.
            for (int i = 0; i < managedPartitionLength; i++)
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

            int managedPartitionLength = m_archetype.ManagedComponentCount;

            // Frees references to managed objects.
            for (int i = 0; i < managedPartitionLength; i++)
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
