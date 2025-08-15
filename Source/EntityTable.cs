// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a table containing components for entities modelled by a common archetype.
    /// </summary>
    public class EntityTable
    {
        private readonly EntityArchetype m_archetype;
        private readonly EntityRegistry? m_registry;
        private readonly Array[] m_components;
        private readonly Entity[] m_entities;
        private int m_size;
        private int m_version;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTable"/> class that stores entities
        /// modelled by the specified archetype and has the specified capacity.
        /// </summary>
        /// <param name="archetype">
        /// The archetype that models entities in the <see cref="EntityTable"/>.
        /// </param>
        /// <param name="capacity">
        /// The capacity of the <see cref="EntityTable"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is negative.
        /// </exception>
        public EntityTable(EntityArchetype archetype, int capacity)
            : this(archetype, null, capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTable"/> class that stores entities
        /// modelled by the specified archetype and has the specified capacity. If the specified
        /// registry is not <see langword="null"/>, any attempt to modify the structure of the
        /// <see cref="EntityTable"/> will throw an exception, unless the attempt was made by the
        /// registry when it has entered a sync point.
        /// </summary>
        /// <param name="archetype">
        /// The archetype that models entities in the <see cref="EntityTable"/>.
        /// </param>
        /// <param name="registry">
        /// The registry that controls structure modifications to the <see cref="EntityTable"/>, if
        /// not <see langword="null"/>.
        /// </param>
        /// <param name="capacity">
        /// The capacity of the <see cref="EntityTable"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is negative.
        /// </exception>
        public EntityTable(EntityArchetype archetype, EntityRegistry? registry, int capacity)
        {
            ArgumentNullException.ThrowIfNull(archetype);
            ArgumentOutOfRangeException.ThrowIfNegative(capacity);

            m_archetype = archetype;
            m_registry = registry;

            if (capacity == 0)
            {
                // Although the archetype may contain managed and unmanaged component types, it is
                // pretty wasteful to allocate space for empty component arrays when the caller
                // specifies a capacity of zero. However, this will cause the Try/GetComponents
                // methods to return/throw a failure when they would have otherwise succeeded with a
                // capacity greater than zero.
                m_components = Array.Empty<Array>();
                m_entities = Array.Empty<Entity>();
            }
            else
            {
                // Only managed and unmanaged components will be stored in the EntityTable. This is
                // done to eliminate allocations of tag component arrays, which needlessly consumes
                // memory despite the elements not having instance fields.
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
                        // Arrays are allocated through dynamic dispatch instead of reflection,
                        // allowing the library to be AOT compatible.
                        m_components[i] = componentTypes[i].CreateArray(capacity);
                    }
                }

                m_entities = new Entity[capacity];
            }
        }

        /// <summary>
        /// Gets the archetype that models entities in the <see cref="EntityTable"/>.
        /// </summary>
        /// <returns>
        /// The archetype that models entities in the <see cref="EntityTable"/>.
        /// </returns>
        public EntityArchetype Archetype
        {
            get => m_archetype;
        }

        /// <summary>
        /// Gets the registry that controls structure modifications to the
        /// <see cref="EntityTable"/>.
        /// </summary>
        /// <returns>
        /// The registry that controls structure modifications to the <see cref="EntityTable"/>,
        /// or <see langword="null"/> if no such registry was provided.
        /// </returns>
        public EntityRegistry? Registry
        {
            get => m_registry;
        }

        /// <summary>
        /// Gets the total number of entities the <see cref="EntityTable"/> can hold before it
        /// becomes full.
        /// </summary>
        /// <returns>
        /// The total number of entities the <see cref="EntityTable"/> can hold before it becomes
        /// full.
        /// </returns>
        public int Capacity
        {
            get => m_entities.Length;
        }

        /// <summary>
        /// Gets the number of entities contained in the <see cref="EntityTable"/>.
        /// </summary>
        /// <returns>
        /// The number of entities contained in the <see cref="EntityTable"/>.
        /// </returns>
        public int Count
        {
            get => m_size;
        }

        /// <summary>
        /// Gets the version of the <see cref="EntityTable"/>, which is incremented after a
        /// structure modification has occurred.
        /// </summary>
        /// <returns>
        /// The version of the <see cref="EntityTable"/>.
        /// </returns>
        public int Version
        {
            get => m_version;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityTable"/> is empty.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityTable"/> is empty; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsEmpty
        {
            get => m_size == 0;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityTable"/> is full.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityTable"/> is full; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsFull
        {
            get => m_size == m_entities.Length;
        }

        /// <summary>
        /// Gets a span over the column containing components of type <typeparamref name="T"/> in
        /// the <see cref="EntityTable"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the components.
        /// </typeparam>
        /// <returns>
        /// A span over the column containing components of type <typeparamref name="T"/> in the
        /// <see cref="EntityTable"/>.
        /// </returns>
        /// <exception cref="ComponentNotFoundException">
        /// Unable to find a column containing components of type <typeparamref name="T"/> in the
        /// <see cref="EntityTable"/>.
        /// </exception>
        /// <exception cref="ComponentNotFoundException">
        /// <typeparamref name="T"/> is a tag component type.
        /// </exception>
        public Span<T> GetComponentColumn<T>()
        {
            Array[] components = m_components;
            int index = m_archetype.IndexOf(ComponentType.TypeOf<T>());

            if ((uint)index >= (uint)components.Length)
            {
                ThrowForComponentNotFound(index, typeof(T));
            }

            return new Span<T>((T[])components[index]);
        }

        /// <summary>
        /// Gets a span over the column containing components of type <typeparamref name="T"/> in
        /// the <see cref="EntityTable"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the components.
        /// </typeparam>
        /// <param name="column">
        /// When this method returns, contains the span over the column containing components of
        /// type <typeparamref name="T"/> in the <see cref="EntityTable"/>, if it is found;
        /// otherwise, an empty span. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the column containing components of type
        /// <typeparamref name="T"/> is found in the <see cref="EntityTable"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetComponentColumn<T>(out Span<T> column)
        {
            Array[] components = m_components;
            int index = m_archetype.IndexOf(ComponentType.TypeOf<T>());

            if ((uint)index >= (uint)components.Length)
            {
                column = default;
                return false;
            }

            column = new Span<T>((T[])components[index]);
            return true;
        }

        /// <summary>
        /// Gets a read-only span over the column containing entities in the
        /// <see cref="EntityTable"/>.
        /// </summary>
        /// <returns>
        /// A read-only span over the column containing entities in the <see cref="EntityTable"/>.
        /// </returns>
        public ReadOnlySpan<Entity> GetEntityColumn()
        {
            return new ReadOnlySpan<Entity>(m_entities);
        }

        /// <summary>
        /// Adds the specified entity to a new row at the end of the <see cref="EntityTable"/> and
        /// zero-initializes its components.
        /// </summary>
        /// <param name="entity">
        /// The entity to be added to the new row at the end of the <see cref="EntityTable"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The structure of the <see cref="EntityTable"/> cannot be modified unless
        /// <see cref="Registry"/> has entered its sync point.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="EntityTable"/> is full.
        /// </exception>
        public void Add(Entity entity)
        {
            ThrowIfInvalidStructureModification();

            Entity[] entities = m_entities;
            int size = m_size;

            if ((uint)size >= (uint)entities.Length)
            {
                ThrowForFullCapacity();
            }

            Array[] components = m_components;
            int length = components.Length;

            // Zero-initialize unmanaged components. Managed components are cleared on removal.
            for (int i = m_archetype.ManagedComponentCount; i < length; i++)
            {
                Array.Clear(components[i], size, 1);
            }

            entities[size] = entity;
            m_size = size + 1;
            m_version++;
        }

        /// <summary>
        /// Adds the specified entity to a new row at the end of the <see cref="EntityTable"/> and
        /// copies its components from the specified table at the specified row index. Any
        /// components that could not be copied from the table are zero-initialized instead.
        /// </summary>
        /// <param name="entity">
        /// The entity to be added to the new row at the end of the <see cref="EntityTable"/>.
        /// </param>
        /// <param name="source">
        /// The table containing the row of components that should be added to the end of the
        /// <see cref="EntityTable"/> alongside <paramref name="entity"/>.
        /// </param>
        /// <param name="index">
        /// The zero-based row index in <paramref name="source"/> at which components should be
        /// copied.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The structure of the <see cref="EntityTable"/> cannot be modified unless
        /// <see cref="Registry"/> has entered its sync point.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="EntityTable"/> is full.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> exceeds the bounds of <paramref name="source"/>.
        /// </exception>
        public void Add(Entity entity, EntityTable source, int index)
        {
            ThrowIfInvalidStructureModification();

            Entity[] entities = m_entities;
            int size = m_size;

            if ((uint)size >= (uint)entities.Length)
            {
                ThrowForFullCapacity();
            }

            ArgumentNullException.ThrowIfNull(source);

            if ((uint)index >= (uint)source.m_size)
            {
                ThrowForIndexOutOfRange(index);
            }

            Array[] sourceComponents = source.m_components;
            Array[] destinationComponents = m_components;
            ReadOnlySpan<ComponentType> sourceComponentTypes =
                source.m_archetype.ComponentTypes.Slice(0, sourceComponents.Length);
            ReadOnlySpan<ComponentType> destinationComponentTypes =
                m_archetype.ComponentTypes.Slice(0, destinationComponents.Length);
            int sourceIndex = 0;
            int destinationIndex = 0;
            Array sourceColumn = null!;
            ComponentType sourceComponentType = null!;

            while (destinationIndex < destinationComponentTypes.Length)
            {
                Array destinationColumn = destinationComponents[destinationIndex];
                ComponentType destinationComponentType = destinationComponentTypes[destinationIndex++];
                int comparison = destinationComponentType.CompareTo(sourceComponentType);

                // Search for a source column that stores components of the same type as the
                // destination column.
                while (comparison > 0 && sourceIndex < sourceComponentTypes.Length)
                {
                    sourceColumn = sourceComponents[sourceIndex];
                    sourceComponentType = sourceComponentTypes[sourceIndex++];
                    comparison = destinationComponentType.CompareTo(sourceComponentType);
                }

                if (comparison == 0)
                {
                    // If a matching column is found, copy the components from the source column to
                    // the destination column.
                    Array.Copy(sourceColumn, index, destinationColumn, size, 1);
                }
                else if (destinationComponentType.Category == ComponentTypeCategory.Unmanaged)
                {
                    // If no matching column is found, zero-initialize the unmanaged components in
                    // the destination column.
                    Array.Clear(destinationColumn, size, 1);
                }
            }

            entities[size] = entity;
            m_size = size + 1;
            m_version++;
        }

        /// <summary>
        /// Removes all entities from the <see cref="EntityTable"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The structure of the <see cref="EntityTable"/> cannot be modified unless
        /// <see cref="Registry"/> has entered its sync point.
        /// </exception>
        public void Clear()
        {
            ThrowIfInvalidStructureModification();

            Array[] components = m_components;
            int managedComponentCount = m_archetype.ManagedComponentCount;
            int size = m_size;

            // Free references in managed components. Unmanaged components are left untouched.
            for (int i = 0; i < managedComponentCount; i++)
            {
                Array.Clear(components[i], 0, size);
            }

            m_size = 0;
            m_version++;
        }

        /// <summary>
        /// Removes the entity at the specified row index of the <see cref="EntityTable"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based row index of the entity to remove.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The structure of the <see cref="EntityTable"/> cannot be modified unless
        /// <see cref="Registry"/> has entered its sync point.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> exceeds the bounds of the <see cref="EntityTable"/>.
        /// </exception>
        public void Delete(int index)
        {
            ThrowIfInvalidStructureModification();
            
            int size = m_size;

            if ((uint)index >= (uint)size)
            {
                ThrowForIndexOutOfRange(index);
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

            // Free references in managed components. Unmanaged components are left untouched.
            for (int i = 0; i < managedComponentCount; i++)
            {
                Array.Clear(components[i], size, 1);
            }

            m_size = size;
            m_version++;
        }

        [DoesNotReturn]
        private static void ThrowForComponentNotFound(int index, Type type)
        {
            throw new ComponentNotFoundException(message: (index < 0)
                ? $"Unable to find a column containing components of type {type} in the EntityTable."
                : $"{type} is a tag component type.");
        }

        [DoesNotReturn]
        private static void ThrowForFullCapacity()
        {
            throw new InvalidOperationException("The EntityTable is full.");
        }

        [DoesNotReturn]
        private static void ThrowForIndexOutOfRange(int index)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, message: (index < 0)
                ? "The index is negative."
                : "The index exceeds the bounds of the EntityTable.");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidStructureModification()
        {
            throw new InvalidOperationException(
                "The structure of the EntityTable cannot be modified unless its registry has " +
                "entered its sync point.");
        }

        private void ThrowIfInvalidStructureModification()
        {
            EntityRegistry? registry = m_registry;

            if (registry != null && !registry.IsSyncPointEntered)
            {
                ThrowForInvalidStructureModification();
            }
        }
    }
}
