// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities
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
        /// Initializes new instance of the <see cref="EntityTable"/> class that stores entities
        /// modeled by the specified <see cref="EntityArchetype"/> and has the default capacity.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The <see cref="EntityArchetype"/> that models entities stored by the
        /// <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        public EntityTable(EntityArchetype archetype)
            : this(archetype, null, MinimumCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTable"/> class that stores entities
        /// modeled by the specified <see cref="EntityArchetype"/>, gives the specified
        /// <see cref="EntityRegistry"/> exclusive write permissions, and has the default capacity.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The <see cref="EntityArchetype"/> that models entities stored by the
        /// <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <param name="registry">
        /// The <see cref="EntityRegistry"/> to give exclusive write permissions to, if not
        /// <see langword="null"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        public EntityTable(EntityArchetype archetype, EntityRegistry? registry)
            : this(archetype, registry, MinimumCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTable"/> class that stores entities
        /// modeled by the specified <see cref="EntityArchetype"/> and has the specified capacity.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The <see cref="EntityArchetype"/> that models entities stored by the
        /// <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <param name="capacity">
        /// The capacity of the <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is negative.
        /// </exception>
        public EntityTable(EntityArchetype archetype, int capacity)
            : this(archetype, null, capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTable"/> class that stores entities
        /// modeled by the specified <see cref="EntityArchetype"/>, gives the specified
        /// <see cref="EntityRegistry"/> exclusive write permissions, and has the specified
        /// capacity.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The <see cref="EntityArchetype"/> that models entities stored by the
        /// <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <param name="registry">
        /// The <see cref="EntityRegistry"/> to give exclusive write permissions to, if not
        /// <see langword="null"/>.
        /// </param>
        /// 
        /// <param name="capacity">
        /// The capacity of the <see cref="EntityTable"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is negative.
        /// </exception>
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

            ReadOnlySpan<ComponentType> componentTypes = archetype.ComponentTypes.Slice(0,
                archetype.ManagedComponentCount + archetype.UnmanagedComponentCount);

            if (componentTypes.IsEmpty)
            {
                m_components = Array.Empty<Array>();
            }
            else
            {
                m_components = new Array[componentTypes.Length];

                for (int i = 0; i < componentTypes.Length; i++)
                {
                    m_components[i] = componentTypes[i].CreateArray(capacity);
                }
            }

            m_entities = new Entity[capacity];
        }

        /// <summary>
        /// Gets the <see cref="EntityArchetype"/> that models entities stored by the
        /// <see cref="EntityTable"/>.
        /// </summary>
        public EntityArchetype Archetype
        {
            get => m_archetype;
        }

        /// <summary>
        /// Gets the <see cref="EntityRegistry"/> that was given exclusive write permissions by the
        /// <see cref="EntityTable"/>, or <see langword="null"/> if no such permissions were given.
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
        /// Gets a value that indicates whether the <see cref="EntityTable"/> is empty.
        /// </summary>
        public bool IsEmpty
        {
            get => m_size == 0;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityTable"/> is full.
        /// </summary>
        public bool IsFull
        {
            get => m_size == m_entities.Length;
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
        /// 
        /// <exception cref="ComponentNotFoundException">
        /// The EntityTable does not store components of type <typeparamref name="T"/>.
        /// </exception>
        public Span<T> GetComponents<T>()
        {
            int index = m_archetype.IndexOf(ComponentType.TypeOf<T>());

            if ((uint)index >= (uint)m_components.Length)
            {
                throw new ComponentNotFoundException(
                    $"The EntityTable does not store components of type {typeof(T)}.");
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

            if ((uint)index >= (uint)m_components.Length)
            {
                components = default;
                return false;
            }

            components = new Span<T>((T[])m_components[index]);
            return true;
        }

        /// <summary>
        /// Gets a read-only span of entities stored by the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A read-only span of entities stored by the <see cref="EntityTable"/>.
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
        /// 
        /// <exception cref="InvalidOperationException">
        /// The <see cref="EntityTable"/> can only be modified by its <see cref="Registry"/>, or
        /// the <see cref="EntityTable"/> is full.
        /// </exception>
        public void Add(Entity entity)
        {
            VerifyCallerWritePermissions();

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
        /// Copies a range of entities from the specified table and adds them to the end of the
        /// <see cref="EntityTable"/>.
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
        /// 
        /// <exception cref="InvalidOperationException">
        /// The <see cref="EntityTable"/> can only be modified by its <see cref="Registry"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="table"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="tableIndex"/> is negative, or <paramref name="tableIndex"/> is greater
        /// than or equal to the size of <paramref name="table"/>, or <paramref name="count"/> is
        /// negative.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="tableIndex"/> and <paramref name="count"/> do not denote a valid range
        /// of entities in <paramref name="table"/>, or <paramref name="count"/> exceeds the
        /// capacity of the <see cref="EntityTable"/>.
        /// </exception>
        public void AddRange(EntityTable table, int tableIndex, int count)
        {
            VerifyCallerWritePermissions();
            ArgumentNullException.ThrowIfNull(table);

            if ((uint)tableIndex >= (uint)table.m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(tableIndex), tableIndex,
                    "Table index was out of range. Must be non-negative and less than the size " +
                    "of the table.");
            }

            ArgumentOutOfRangeException.ThrowIfNegative(count);
            
            if (table.m_size - count < tableIndex)
            {
                throw new ArgumentException(
                    "Table index and count do not denote a valid range of entities in the " +
                    "EntityTable.");
            }

            int size = m_size;
            Entity[] entities = m_entities;

            if (entities.Length - count < size)
            {
                throw new ArgumentException("Count exceeds the capacity of the EntityTable.");
            }

            if (count > 0)
            {
                Array[] sourceComponents = table.m_components;
                Array[] destinationComponents = m_components;
                ReadOnlySpan<ComponentType> sourceComponentTypes =
                    table.m_archetype.ComponentTypes.Slice(0, sourceComponents.Length);
                ReadOnlySpan<ComponentType> destinationComponentTypes =
                    m_archetype.ComponentTypes.Slice(0, destinationComponents.Length);
                int sourceIndex = -1;
                int destinationIndex = 0;
                ComponentType sourceComponentType = null!;

                while (destinationIndex < destinationComponentTypes.Length)
                {
                    ComponentType destinationComponentType = destinationComponentTypes[destinationIndex];
                    bool recompare;

                    do
                    {
                        switch (destinationComponentType.CompareTo(sourceComponentType))
                        {
                            case 0:
                                Array.Copy(sourceComponents[sourceIndex], tableIndex,
                                    destinationComponents[destinationIndex], size, count);
                                recompare = false;
                                continue;
                            case -1:
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

                    destinationIndex++;
                }

                Array.Copy(table.m_entities, tableIndex, entities, size, count);
                m_size = size + count;
                m_version++;
            }
        }

        /// <summary>
        /// Removes all entities from the <see cref="EntityTable"/>.
        /// </summary>
        /// 
        /// <exception cref="InvalidOperationException">
        /// The <see cref="EntityTable"/> can only be modified by its <see cref="Registry"/>.
        /// </exception>
        public void Clear()
        {
            VerifyCallerWritePermissions();

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

            if ((m_registry == null || m_registry.IsLockHeld) &&
                (index = Array.IndexOf(m_entities, entity, 0, m_size)) != -1)
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
        /// 
        /// <exception cref="InvalidOperationException">
        /// The <see cref="EntityTable"/> can only be modified by its <see cref="Registry"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative, or <paramref name="index"/> is greater than or
        /// equal to the size of <see cref="EntityTable"/>.
        /// </exception>
        public void RemoveAt(int index)
        {
            VerifyCallerWritePermissions();
            
            int size = m_size;

            if ((uint)index >= (uint)size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    "Index was out of range. Must be non-negative and less than the size of the " +
                    "EntityTable.");
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
        /// 
        /// <exception cref="InvalidOperationException">
        /// The <see cref="EntityTable"/> can only be modified by its <see cref="Registry"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative or <paramref name="count"/> is negative.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote a valid range of
        /// entities in <see cref="EntityTable"/>.
        /// </exception>
        public void RemoveRange(int index, int count)
        {
            VerifyCallerWritePermissions();
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            int size = m_size - count;

            if (size < index)
            {
                throw new ArgumentException(
                    "Index and count do not denote a valid range of entities in the EntityTable.");
            }

            if (count > 0)
            {
                Array[] components = m_components;

                if (index < size)
                {
                    Entity[] entities = m_entities;
                    int copyIndex = index + count;
                    int copyLength = size - index;

                    for (int i = 0; i < components.Length; i++)
                    {
                        Array array = components[i];
                        Array.Copy(array, copyIndex, array, index, copyLength);
                    }

                    Array.Copy(entities, copyIndex, entities, index, copyLength);
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
        }

        private void VerifyCallerWritePermissions()
        {
            if (m_registry != null && !m_registry.IsLockHeld)
            {
                throw new InvalidOperationException(
                    "The EntityTable can only be modified by its Registry.");
            }
        }
    }
}
