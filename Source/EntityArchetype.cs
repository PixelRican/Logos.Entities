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
        private readonly int[] m_componentBitmap;
        private readonly int m_managedComponentCount;
        private readonly int m_unmanagedComponentCount;
        private readonly int m_tagComponentCount;
        private readonly int m_entitySize;

        private EntityArchetype()
        {
            m_componentTypes = Array.Empty<ComponentType>();
            m_componentBitmap = Array.Empty<int>();
            m_entitySize = Unsafe.SizeOf<Entity>();
        }

        private EntityArchetype(ComponentType[] componentTypes, int[] componentBitmap,
            int managedComponentCount, int unmanagedComponentCount, int tagComponentCount,
            int entitySize)
        {
            m_componentTypes = componentTypes;
            m_componentBitmap = componentBitmap;
            m_managedComponentCount = managedComponentCount;
            m_unmanagedComponentCount = unmanagedComponentCount;
            m_tagComponentCount = tagComponentCount;
            m_entitySize = entitySize;
        }

        /// <summary>
        /// Gets an <see cref="EntityArchetype"/> that models entities with no components.
        /// </summary>
        /// <returns>
        /// An <see cref="EntityArchetype"/> that models entities with no components.
        /// </returns>
        public static EntityArchetype Base
        {
            get => s_base;
        }

        /// <summary>
        /// Gets a read-only span of component types that compose entities modelled by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        /// <returns>
        /// A read-only span of component types that compose entities modelled by the
        /// <see cref="EntityArchetype"/>.
        /// </returns>
        public ReadOnlySpan<ComponentType> ComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_componentTypes);
        }

        /// <summary>
        /// Gets a read-only bitmap that compactly stores flags indicating whether a specific
        /// component type is contained by the <see cref="EntityArchetype"/>.
        /// </summary>
        /// <returns>
        /// A read-only bitmap that compactly stores flags indicating whether a specific component
        /// type is contained by the <see cref="EntityArchetype"/>.
        /// </returns>
        public ReadOnlySpan<int> ComponentBitmap
        {
            get => new ReadOnlySpan<int>(m_componentBitmap);
        }

        /// <summary>
        /// Gets the total number of component types contained by the <see cref="EntityArchetype"/>.
        /// </summary>
        /// <returns>
        /// The total number of component types contained by the <see cref="EntityArchetype"/>.
        /// </returns>
        public int ComponentCount
        {
            get => m_componentTypes.Length;
        }

        /// <summary>
        /// Gets the number of managed component types contained by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        /// <returns>
        /// The number of managed component types contained by the <see cref="EntityArchetype"/>.
        /// </returns>
        public int ManagedComponentCount
        {
            get => m_managedComponentCount;
        }

        /// <summary>
        /// Gets the number of unmanaged component types contained by the
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        /// <returns>
        /// The number of unmanaged component types contained by the <see cref="EntityArchetype"/>.
        /// </returns>
        public int UnmanagedComponentCount
        {
            get => m_unmanagedComponentCount;
        }

        /// <summary>
        /// Gets the number of tag component types contained by the <see cref="EntityArchetype"/>.
        /// </summary>
        /// <returns>
        /// The number of tag component types contained by the <see cref="EntityArchetype"/>.
        /// </returns>
        public int TagComponentCount
        {
            get => m_tagComponentCount;
        }

        /// <summary>
        /// Gets the size of an entity modelled by the <see cref="EntityArchetype"/>.
        /// </summary>
        /// <returns>
        /// The size, in bytes, of an entity modelled by the <see cref="EntityArchetype"/>.
        /// </returns>
        public int EntitySize
        {
            get => m_entitySize;
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> that contains component types copied from the
        /// specified array.
        /// </summary>
        /// <param name="array">
        /// The array of component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityArchetype"/> that contains component types copied from
        /// <paramref name="array"/>, or <see cref="Base"/> if <paramref name="array"/> does not
        /// contain any component types.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        public static EntityArchetype Create(ComponentType[] array)
        {
            ArgumentNullException.ThrowIfNull(array);
            return CreateInstance(new ReadOnlySpan<ComponentType>(array).ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> that contains component types copied from the
        /// specified collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityArchetype"/> that contains component types copied from
        /// <paramref name="collection"/>, or <see cref="Base"/> if <paramref name="collection"/>
        /// does not contain any component types.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public static EntityArchetype Create(IEnumerable<ComponentType> collection)
        {
            return CreateInstance(collection.ToArray());
        }

        /// <summary>
        /// Creates an <see cref="EntityArchetype"/> that contains component types copied from the
        /// specified span.
        /// </summary>
        /// <param name="span">
        /// The span of component types.
        /// </param>
        /// <returns>
        /// An <see cref="EntityArchetype"/> that contains component types copied from
        /// <paramref name="span"/>, or <see cref="Base"/> if <paramref name="span"/> does not
        /// contain any component types.
        /// </returns>
        public static EntityArchetype Create(ReadOnlySpan<ComponentType> span)
        {
            return CreateInstance(span.ToArray());
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityArchetype"/> and adds the specified component
        /// type to it. If the <see cref="EntityArchetype"/> already contains the component type,
        /// the method returns the <see cref="EntityArchetype"/>.
        /// </summary>
        /// <param name="componentType">
        /// The component type to add.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityArchetype"/> with <paramref name="componentType"/> added
        /// to it, or the <see cref="EntityArchetype"/> if it already contains
        /// <paramref name="componentType"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentType"/> is <see langword="null"/>.
        /// </exception>
        public EntityArchetype Add(ComponentType componentType)
        {
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<int> sourceBitmap = ComponentBitmap;
            int commponentTypeIndex = componentType.Index;

            // Return this instance if it already contains the component type.
            if (BitmapOperations.Test(sourceBitmap, commponentTypeIndex))
            {
                return this;
            }

            // Update component type array.
            ReadOnlySpan<ComponentType> sourceSpan = ComponentTypes;
            ComponentType[] componentTypes = new ComponentType[sourceSpan.Length + 1];
            Span<ComponentType> destinationSpan = new Span<ComponentType>(componentTypes);
            int index = ~BinarySearch(componentType);

            sourceSpan.Slice(0, index).CopyTo(destinationSpan);
            destinationSpan[index] = componentType;
            sourceSpan.Slice(index).CopyTo(destinationSpan.Slice(index + 1));

            // Update component bitmap.
            int[] componentBitmap = new int[destinationSpan[^1].Index + 32 >> 5];
            Span<int> destinationBitmap = new Span<int>(componentBitmap);

            sourceBitmap.CopyTo(destinationBitmap);
            destinationBitmap[commponentTypeIndex >> 5] |= 1 << commponentTypeIndex;

            // Update component count and entity size based on the category of the component type.
            int managedComponentCount = m_managedComponentCount;
            int unmanagedComponentCount = m_unmanagedComponentCount;
            int tagComponentCount = m_tagComponentCount;
            int entitySize = m_entitySize;

            switch (componentType.Category)
            {
                case ComponentTypeCategory.Managed:
                    managedComponentCount++;
                    goto default;
                case ComponentTypeCategory.Unmanaged:
                    unmanagedComponentCount++;
                    goto default;
                case ComponentTypeCategory.Tag:
                    tagComponentCount++;
                    break;
                default:
                    entitySize += componentType.Size;
                    break;
            }

            return new EntityArchetype(componentTypes, componentBitmap, managedComponentCount,
                unmanagedComponentCount, tagComponentCount, entitySize);
        }

        /// <summary>
        /// Determines whether the <see cref="EntityArchetype"/> contains the specified component
        /// type.
        /// </summary>
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityArchetype"/> contains
        /// <paramref name="componentType"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(ComponentType componentType)
        {
            return componentType != null
                && BitmapOperations.Test(ComponentBitmap, componentType.Index);
        }

        /// <summary>
        /// Searches for the specified component type and returns the zero-based index in
        /// <see cref="ComponentTypes"/>.
        /// </summary>
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// <returns>
        /// The zero-based index of <paramref name="componentType"/> in
        /// <see cref="ComponentTypes"/>, if found; otherwise, -1.
        /// </returns>
        public int IndexOf(ComponentType componentType)
        {
            if (Contains(componentType))
            {
                int index = BinarySearch(componentType);

                if (index >= 0)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityArchetype"/> and removes the specified component
        /// type from it. If the <see cref="EntityArchetype"/> does not contain the component type,
        /// the method returns the <see cref="EntityArchetype"/>.
        /// </summary>
        /// <param name="componentType">
        /// The component type to remove.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityArchetype"/> with <paramref name="componentType"/>
        /// removed from it, or the <see cref="EntityArchetype"/> if it does not contain
        /// <paramref name="componentType"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentType"/> is <see langword="null"/>.
        /// </exception>
        public EntityArchetype Remove(ComponentType componentType)
        {
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<int> sourceBitmap = ComponentBitmap;
            int componentTypeIndex = componentType.Index;

            // Return this instance if it does not contain the component type.
            if (!BitmapOperations.Test(sourceBitmap, componentTypeIndex))
            {
                return this;
            }

            ReadOnlySpan<ComponentType> sourceSpan = ComponentTypes;

            // Return the base archetype if this instance only contains the component type.
            if (sourceSpan.Length == 1)
            {
                return s_base;
            }

            // Update component type array.
            ComponentType[] componentTypes = new ComponentType[sourceSpan.Length - 1];
            Span<ComponentType> destinationSpan = new Span<ComponentType>(componentTypes);
            int index = BinarySearch(componentType);

            sourceSpan.Slice(0, index).CopyTo(destinationSpan);
            sourceSpan.Slice(index + 1).CopyTo(destinationSpan.Slice(index));

            // Update component bitmap.
            int[] componentBitmap = new int[destinationSpan[^1].Index + 32 >> 5];
            Span<int> destinationBitmap = new Span<int>(componentBitmap);

            sourceBitmap.Slice(0, destinationBitmap.Length).CopyTo(destinationBitmap);
            index = componentTypeIndex >> 5;

            if (index < destinationBitmap.Length)
            {
                destinationBitmap[index] ^= 1 << componentTypeIndex;
            }

            // Update component count and entity size based on the category of the component type.
            int managedComponentCount = m_managedComponentCount;
            int unmanagedComponentCount = m_unmanagedComponentCount;
            int tagComponentCount = m_tagComponentCount;
            int entitySize = m_entitySize;

            switch (componentType.Category)
            {
                case ComponentTypeCategory.Managed:
                    managedComponentCount--;
                    goto default;
                case ComponentTypeCategory.Unmanaged:
                    unmanagedComponentCount--;
                    goto default;
                case ComponentTypeCategory.Tag:
                    tagComponentCount--;
                    break;
                default:
                    entitySize -= componentType.Size;
                    break;
            }

            return new EntityArchetype(componentTypes, componentBitmap, managedComponentCount,
                unmanagedComponentCount, tagComponentCount, entitySize);
        }

        /// <inheritdoc cref="IEquatable{T}.Equals"/>
        public bool Equals([NotNullWhen(true)] EntityArchetype? other)
        {
            return ReferenceEquals(other, this)
                || other is not null
                && ComponentBitmap.SequenceEqual(other.ComponentBitmap);
        }

        /// <inheritdoc cref="object.Equals"/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj as EntityArchetype);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return BitmapOperations.GetHashCode(ComponentBitmap);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            string componentTypes = string.Join(", ", (object[])m_componentTypes);

            return $"EntityArchetype {{ ComponentTypes = [{componentTypes}] }}";
        }

        private static EntityArchetype CreateInstance(ComponentType[] componentTypes)
        {
            if (componentTypes.Length == 0)
            {
                return s_base;
            }

            Array.Sort(componentTypes);

            ComponentType? previousComponentType = componentTypes[^1];

            if (previousComponentType == null)
            {
                return s_base;
            }

            int[] componentBitmap = new int[previousComponentType.Index + 32 >> 5];
            int componentCount = 0;
            int managedComponentCount = 0;
            int unmanagedComponentCount = 0;
            int tagComponentCount = 0;
            int entitySize = Unsafe.SizeOf<Entity>();

            previousComponentType = null;

            foreach (ComponentType? currentComponentType in componentTypes)
            {
                if (currentComponentType != previousComponentType)
                {
                    int componentTypeIndex = currentComponentType.Index;

                    componentTypes[componentCount++] = previousComponentType = currentComponentType;
                    componentBitmap[componentTypeIndex >> 5] |= 1 << componentTypeIndex;

                    switch (currentComponentType.Category)
                    {
                        case ComponentTypeCategory.Managed:
                            managedComponentCount++;
                            break;
                        case ComponentTypeCategory.Unmanaged:
                            unmanagedComponentCount++;
                            break;
                        case ComponentTypeCategory.Tag:
                            tagComponentCount++;
                            continue;
                    }

                    entitySize += currentComponentType.Size;
                }
            }

            Array.Resize(ref componentTypes, componentCount);
            return new EntityArchetype(componentTypes, componentBitmap, managedComponentCount,
                unmanagedComponentCount, tagComponentCount, entitySize);
        }

        private int BinarySearch(ComponentType componentType)
        {
            ComponentType[] componentTypes = m_componentTypes;
            int targetComponentTypeIndex = componentType.Index;
            int lowerBound;
            int upperBound;

            // Determine the search range based on the category of the component type.
            switch (componentType.Category)
            {
                case ComponentTypeCategory.Managed:
                    lowerBound = 0;
                    upperBound = m_managedComponentCount - 1;
                    break;
                case ComponentTypeCategory.Unmanaged:
                    lowerBound = m_managedComponentCount;
                    upperBound = lowerBound + m_unmanagedComponentCount - 1;
                    break;
                case ComponentTypeCategory.Tag:
                    lowerBound = m_managedComponentCount + m_unmanagedComponentCount;
                    upperBound = lowerBound + m_tagComponentCount - 1;
                    break;
                default:
                    // This case should not be reachable under normal circumstances. However, this
                    // was needed to prevent the compiler from complaining about the lower and upper
                    // bound variables being uninitialized should the other cases fail somehow.
                    return -1;
            }

            // Find the component type using binary search.
            while (lowerBound <= upperBound)
            {
                int index = lowerBound + (upperBound - lowerBound >> 1);
                int sourceComponentTypeIndex = componentTypes[index].Index;

                if (targetComponentTypeIndex == sourceComponentTypeIndex)
                {
                    return index;
                }

                if (targetComponentTypeIndex < sourceComponentTypeIndex)
                {
                    upperBound = index - 1;
                }
                else
                {
                    lowerBound = index + 1;
                }
            }

            // Return the complement of the index where the component type would have been in if
            // this instance contained it.
            return ~lowerBound;
        }

        /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.operator =="/>
        public static bool operator ==(EntityArchetype? left, EntityArchetype? right)
        {
            return ReferenceEquals(left, right)
                || left is not null
                && right is not null
                && left.ComponentBitmap.SequenceEqual(right.ComponentBitmap);
        }

        /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.operator !="/>
        public static bool operator !=(EntityArchetype? left, EntityArchetype? right)
        {
            return !(left == right);
        }
    }
}
