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
    /// Represents a data model that describes the composition of entities.
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

        private EntityArchetype(ComponentType[] componentTypes, int[] componentBitmask,
            int managedComponentCount, int unmanagedComponentCount, int tagComponentCount,
            int entitySize)
        {
            m_componentTypes = componentTypes;
            m_componentBitmask = componentBitmask;
            m_managedComponentCount = managedComponentCount;
            m_unmanagedComponentCount = unmanagedComponentCount;
            m_tagComponentCount = tagComponentCount;
            m_entitySize = entitySize;
        }

        /// <summary>
        /// Gets an <see cref="EntityArchetype"/> that models entities with no components.
        /// </summary>
        public static EntityArchetype Base
        {
            get => s_base;
        }

        /// <summary>
        /// Gets a read-only span of component types that compose entities modeled by the
        /// <see cref="EntityArchetype"/>.
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
        /// Gets the total number of component types that compose entities modeled by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        public int ComponentCount
        {
            get => m_componentTypes.Length;
        }

        /// <summary>
        /// Gets the number of managed component types that compose entities modeled by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        public int ManagedComponentCount
        {
            get => m_managedComponentCount;
        }

        /// <summary>
        /// Gets the number of unmanaged component types that compose entities modeled by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        public int UnmanagedComponentCount
        {
            get => m_unmanagedComponentCount;
        }

        /// <summary>
        /// Gets the number of tag component types that compose entities modeled by the
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
        /// Creates an <see cref="EntityArchetype"/> that contains component types copied from the
        /// specified array.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityArchetype"/> that contains component types copied from the array,
        /// or <see cref="Base"/> if the array does not contain any component types.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentTypes"/> is <see langword="null"/>.
        /// </exception>
        public static EntityArchetype Create(ComponentType[] componentTypes)
        {
            ArgumentNullException.ThrowIfNull(componentTypes);

            if (componentTypes.Length == 0)
            {
                return s_base;
            }

            ComponentType[] arguments = new ComponentType[componentTypes.Length];
            Array.Copy(componentTypes, arguments, componentTypes.Length);
            return CreateIfNonEmpty(arguments);
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> that contains component types copied from the
        /// specified sequence.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityArchetype"/> that contains component types copied from the
        /// sequence, or <see cref="Base"/> if the sequence does not contain any component types.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentTypes"/> is <see langword="null"/>.
        /// </exception>
        public static EntityArchetype Create(IEnumerable<ComponentType> componentTypes)
        {
            ComponentType[] arguments = componentTypes.ToArray();

            if (arguments.Length == 0)
            {
                return s_base;
            }

            return CreateIfNonEmpty(arguments);
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> that contains component types copied from the
        /// specified span.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityArchetype"/> that contains component types copied from the span, or
        /// <see cref="Base"/> if the span does not contain any component types.
        /// </returns>
        public static EntityArchetype Create(ReadOnlySpan<ComponentType> componentTypes)
        {
            if (componentTypes.IsEmpty)
            {
                return s_base;
            }

            return CreateIfNonEmpty(componentTypes.ToArray());
        }

        private static EntityArchetype CreateIfNonEmpty(ComponentType[] componentTypes)
        {
            Array.Sort(componentTypes);

            ComponentType? currentComponentType = componentTypes[^1];

            if (currentComponentType == null)
            {
                return s_base;
            }

            ComponentType? previousComponentType = null;
            int[] componentBitmask = new int[currentComponentType.Id + 32 >> 5];
            int entitySize = Unsafe.SizeOf<Entity>();
            int componentCount = 0;
            int managedComponentCount = 0;
            int unmanagedComponentCount = 0;
            int tagComponentCount = 0;

            for (int i = 0; i < componentTypes.Length; i++)
            {
                if ((currentComponentType = componentTypes[i]) != previousComponentType)
                {
                    componentTypes[componentCount++] = previousComponentType = currentComponentType;
                    componentBitmask[currentComponentType.Id >> 5] |= 1 << currentComponentType.Id;
                    entitySize += currentComponentType.Size;

                    switch (currentComponentType.Category)
                    {
                        case ComponentTypeCategory.Managed:
                            managedComponentCount++;
                            continue;
                        case ComponentTypeCategory.Unmanaged:
                            unmanagedComponentCount++;
                            continue;
                        case ComponentTypeCategory.Tag:
                            tagComponentCount++;
                            continue;
                    }
                }
            }

            Array.Resize(ref componentTypes, componentCount);
            return new EntityArchetype(componentTypes, componentBitmask, managedComponentCount,
                unmanagedComponentCount, tagComponentCount, entitySize);
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityArchetype"/> and adds the specified component
        /// type to it.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to add.
        /// </param>
        /// 
        /// <returns>
        /// A copy of the <see cref="EntityArchetype"/> with the component type added to it, or the
        /// <see cref="EntityArchetype"/> itself if it either contains the component type or if the
        /// component type is <see langword="null"/>.
        /// </returns>
        public EntityArchetype Add(ComponentType componentType)
        {
            if (componentType == null || BitmaskOperations.Test(ComponentBitmask, componentType.Id))
            {
                return this;
            }

            // Build component type array.
            ComponentType[] sourceArray = m_componentTypes;
            ComponentType[] destinationArray = new ComponentType[sourceArray.Length + 1];
            int index = 0;

            while (index < sourceArray.Length)
            {
                ComponentType currentComponentType = sourceArray[index];

                if (currentComponentType.CompareTo(componentType) > 0)
                {
                    break;
                }

                destinationArray[index++] = currentComponentType;
            }

            destinationArray[index] = componentType;

            while (index < sourceArray.Length)
            {
                ComponentType currentComponentType = sourceArray[index];
                destinationArray[++index] = currentComponentType;
            }

            // Build component bitmask.
            int[] sourceBitmask = m_componentBitmask;
            int[] destinationBitmask = new int[destinationArray[index].Id + 32 >> 5];

            Array.Copy(sourceBitmask, destinationBitmask, sourceBitmask.Length);
            destinationBitmask[componentType.Id >> 5] |= 1 << componentType.Id;

            // Increase component count based on component type category.
            int managedComponentCount = m_managedComponentCount;
            int unmanagedComponentCount = m_unmanagedComponentCount;
            int tagComponentCount = m_tagComponentCount;

            switch (componentType.Category)
            {
                case ComponentTypeCategory.Managed:
                    managedComponentCount++;
                    break;
                case ComponentTypeCategory.Unmanaged:
                    unmanagedComponentCount++;
                    break;
                case ComponentTypeCategory.Tag:
                    tagComponentCount++;
                    break;
            }

            // Create superarchetype.
            return new EntityArchetype(destinationArray, destinationBitmask, managedComponentCount,
                unmanagedComponentCount, tagComponentCount, m_entitySize + componentType.Size);
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

            // Clamp search range based on the component type's categorical order.
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

            // Find the component type using binary search.
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

            // This line should be unreachable under normal circumstances.
            return -1;
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityArchetype"/> and removes the specified component
        /// type from it.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to remove.
        /// </param>
        /// 
        /// <returns>
        /// A copy of the <see cref="EntityArchetype"/> with the component type removed from it, or
        /// the <see cref="EntityArchetype"/> itself if it either does not contain the component
        /// type or if the component type is <see langword="null"/>.
        /// </returns>
        public EntityArchetype Remove(ComponentType componentType)
        {
            if (componentType == null || !BitmaskOperations.Test(ComponentBitmask, componentType.Id))
            {
                return this;
            }

            ComponentType[] sourceArray = m_componentTypes;

            if (sourceArray.Length == 1)
            {
                return s_base;
            }

            // Build component type array.
            ComponentType[] destinationArray = new ComponentType[sourceArray.Length - 1];
            int index = 0;

            while (index < destinationArray.Length)
            {
                ComponentType currentComponentType = sourceArray[index];

                if (currentComponentType == componentType)
                {
                    break;
                }

                destinationArray[index++] = currentComponentType;
            }

            while (index < destinationArray.Length)
            {
                destinationArray[index] = sourceArray[++index];
            }

            // Build component bitmask.
            int[] sourceBitmask = m_componentBitmask;
            int[] destinationBitmask = new int[destinationArray[index - 1].Id + 32 >> 5];

            Array.Copy(sourceBitmask, destinationBitmask, destinationBitmask.Length);

            if ((index = componentType.Id >> 5) < destinationBitmask.Length)
            {
                destinationBitmask[index] &= ~(1 << componentType.Id);
            }

            // Decrease component count based on component type category.
            int managedComponentCount = m_managedComponentCount;
            int unmanagedComponentCount = m_unmanagedComponentCount;
            int tagComponentCount = m_tagComponentCount;

            switch (componentType.Category)
            {
                case ComponentTypeCategory.Managed:
                    managedComponentCount--;
                    break;
                case ComponentTypeCategory.Unmanaged:
                    unmanagedComponentCount--;
                    break;
                case ComponentTypeCategory.Tag:
                    tagComponentCount--;
                    break;
            }

            // Create subarchetype.
            return new EntityArchetype(destinationArray, destinationBitmask, managedComponentCount,
                unmanagedComponentCount, tagComponentCount, m_entitySize - componentType.Size);
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
