// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a data model that describes how an entity's components should be laid out in
    /// memory.
    /// </summary>
    public sealed class EntityArchetype : IEquatable<EntityArchetype>
    {
        private static readonly EntityArchetype s_base = new EntityArchetype();

        private readonly ComponentType[] m_componentTypes;
        private readonly int[] m_componentBitmask;
        private readonly int m_managedComponentCount;
        private readonly int m_unmanagedComponentCount;
        private readonly int m_tagComponentCount;
        private readonly int m_entitySize;

        private EntityArchetype()
        {
            m_componentTypes = Array.Empty<ComponentType>();
            m_componentBitmask = Array.Empty<int>();
            m_entitySize = Unsafe.SizeOf<Entity>();
        }

        private EntityArchetype(ComponentType[] componentTypes)
        {
            m_componentTypes = componentTypes;
            m_componentBitmask = new int[componentTypes[^1].Id + 32 >> 5];
            m_entitySize = Unsafe.SizeOf<Entity>();

            int count = 0;
            ComponentType? previous = null;

            foreach (ComponentType current in m_componentTypes)
            {
                if (previous != current)
                {
                    m_componentTypes[count++] = previous = current;
                    m_componentBitmask[current.Id >> 5] |= 1 << current.Id;
                    m_entitySize += current.Size;

                    switch (current.Category)
                    {
                        case ComponentTypeCategory.Managed:
                            m_managedComponentCount++;
                            continue;
                        case ComponentTypeCategory.Unmanaged:
                            m_unmanagedComponentCount++;
                            continue;
                        case ComponentTypeCategory.Tag:
                            m_tagComponentCount++;
                            continue;
                        default:
                            throw new ArgumentException(
                                "An invalid component type was found in the array.", nameof(componentTypes));
                    }
                }
            }

            Array.Resize(ref m_componentTypes, count);
        }

        /// <summary>
        /// Gets an <see cref="EntityArchetype"/> that models entities with no components.
        /// </summary>
        public static EntityArchetype Base
        {
            get => s_base;
        }

        /// <summary>
        /// Gets a read-only span of component types that compose the <see cref="EntityArchetype"/>.
        /// </summary>
        public ReadOnlySpan<ComponentType> ComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_componentTypes);
        }

        /// <summary>
        /// Gets a read-only bitmask that compactly stores flags indicating whether a component
        /// type can be found within <see cref="ComponentTypes"/>.
        /// </summary>
        public ReadOnlySpan<int> ComponentBitmask
        {
            get => new ReadOnlySpan<int>(m_componentBitmask);
        }

        /// <summary>
        /// Gets the total number of components associated with entities modeled by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        public int ComponentCount
        {
            get => m_componentTypes.Length;
        }

        /// <summary>
        /// Gets the number of managed components associated with entities modeled by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        public int ManagedComponentCount
        {
            get => m_managedComponentCount;
        }

        /// <summary>
        /// Gets the number of unmanaged components associated with entities modeled by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        public int UnmanagedComponentCount
        {
            get => m_unmanagedComponentCount;
        }

        /// <summary>
        /// Gets the number of tag components associated with entities modeled by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        public int TagComponentCount
        {
            get => m_tagComponentCount;
        }

        /// <summary>
        /// Gets the size of entities modeled by the <see cref="EntityArchetype"/>.
        /// </summary>
        public int EntitySize
        {
            get => m_entitySize;
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> that is composed of component types from the
        /// specified array.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityArchetype"/> that is composed of component types from the array, or
        /// <see cref="Base"/> if the array does not contain component types.
        /// </returns>
        public static EntityArchetype Create(ComponentType[] componentTypes)
        {
            ArgumentNullException.ThrowIfNull(componentTypes);

            if (componentTypes.Length > 0)
            {
                ComponentType[] array = new ComponentType[componentTypes.Length];
                Array.Copy(componentTypes, array, componentTypes.Length);
                Array.Sort(array);

                if (array[^1] != null)
                {
                    return new EntityArchetype(array);
                }
            }

            return s_base;
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> that is composed of component types from the
        /// specified sequence.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityArchetype"/> that is composed of component types from the sequence,
        /// or <see cref="Base"/> if the sequence does not contain component types.
        /// </returns>
        public static EntityArchetype Create(IEnumerable<ComponentType> componentTypes)
        {
            ComponentType[] array = componentTypes.ToArray();

            if (array.Length > 0)
            {
                Array.Sort(array);

                if (array[^1] != null)
                {
                    return new EntityArchetype(array);
                }
            }

            return s_base;
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> that is composed of component types from the
        /// specified span.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityArchetype"/> that is composed of component types from the span, or
        /// <see cref="Base"/> if the span does not contain component types.
        /// </returns>
        public static EntityArchetype Create(ReadOnlySpan<ComponentType> componentTypes)
        {
            if (componentTypes.Length > 0)
            {
                ComponentType[] array = componentTypes.ToArray();
                Array.Sort(array);

                if (array[^1] != null)
                {
                    return new EntityArchetype(array);
                }
            }

            return s_base;
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> with the specified component type added to it.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to add.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityArchetype"/> with the component type added, or the
        /// <see cref="EntityArchetype"/> itself if it contains the component type or if the
        /// component type is <see langword="null"/>.
        /// </returns>
        public EntityArchetype Add(ComponentType componentType)
        {
            if (componentType == null || BitmaskOperations.Test(ComponentBitmask, componentType.Id))
            {
                return this;
            }

            ComponentType[] source = m_componentTypes;
            ComponentType[] destination = new ComponentType[source.Length + 1];
            int index = 0;
            ComponentType current;

            while (index < source.Length && (current = source[index]).CompareTo(componentType) < 0)
            {
                destination[index++] = current;
            }

            destination[index] = componentType;

            while (index < source.Length)
            {
                current = source[index];
                destination[++index] = current;
            }

            return new EntityArchetype(destination);
        }

        /// <summary>
        /// Determines whether the <see cref="EntityArchetype"/> contains the specified component
        /// type.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityArchetype"/> contains the component
        /// type; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Test(ComponentBitmask, componentType.Id);
        }

        /// <summary>
        /// Searches for the specified component type and returns the zero-based index within
        /// <see cref="ComponentTypes"/>.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// 
        /// <returns>
        /// The zero-based index of the component type within <see cref="ComponentTypes"/>, if
        /// found; otherwise, -1.
        /// </returns>
        public int IndexOf(ComponentType componentType)
        {
            if (!Contains(componentType))
            {
                return -1;
            }

            ComponentType[] source = m_componentTypes;
            int targetId = componentType.Id;
            int low;
            int high;

            switch (componentType.Category)
            {
                case ComponentTypeCategory.Managed:
                    low = 0;
                    high = m_managedComponentCount - 1;
                    break;
                case ComponentTypeCategory.Unmanaged:
                    low = m_managedComponentCount;
                    high = low + m_unmanagedComponentCount - 1;
                    break;
                case ComponentTypeCategory.Tag:
                    low = m_managedComponentCount + m_unmanagedComponentCount;
                    high = low + m_tagComponentCount - 1;
                    break;
                default:
                    return -1;
            }

            while (low <= high)
            {
                int index = low + (high - low >> 1);
                int sourceId = source[index].Id;

                if (targetId == sourceId)
                {
                    return index;
                }

                if (targetId < sourceId)
                {
                    high = index - 1;
                }
                else
                {
                    low = index + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> with the specified component type removed from
        /// it.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to remove.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityArchetype"/> with the component type removed, or the
        /// <see cref="EntityArchetype"/> itself if it does not contain the component type or if
        /// the component type is <see langword="null"/>.
        /// </returns>
        public EntityArchetype Remove(ComponentType componentType)
        {
            if (componentType == null || !BitmaskOperations.Test(ComponentBitmask, componentType.Id))
            {
                return this;
            }

            ComponentType[] source = m_componentTypes;

            if (source.Length == 1)
            {
                return s_base;
            }

            ComponentType[] destination = new ComponentType[source.Length - 1];
            int index = 0;
            ComponentType current;

            while ((current = source[index]) != componentType)
            {
                destination[index++] = current;
            }

            while (index < destination.Length)
            {
                destination[index] = source[++index];
            }

            return new EntityArchetype(destination);
        }

        public bool Equals([NotNullWhen(true)] EntityArchetype? other)
        {
            return this == other
                || other != null
                && ComponentBitmask.SequenceEqual(other.ComponentBitmask);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return this == obj
                || obj is EntityArchetype other
                && ComponentBitmask.SequenceEqual(other.ComponentBitmask);
        }

        public override int GetHashCode()
        {
            return BitmaskOperations.GetHashCode(ComponentBitmask);
        }

        public override string ToString()
        {
            return $"EntityArchetype {{ ComponentTypes = [{string.Join(", ", (object[])m_componentTypes)}] }}";
        }
    }
}
