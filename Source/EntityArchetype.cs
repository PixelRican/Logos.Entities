// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
    /// <summary>
    /// Represents a data model that describes how an entity's components are laid out in entity
    /// tables.
    /// </summary>
    public sealed class EntityArchetype : IEquatable<EntityArchetype>
    {
        private static readonly EntityArchetype s_base = new EntityArchetype();

        private readonly ComponentType[] m_componentTypes;
        private readonly uint[] m_componentBitmask;
        private readonly int m_managedPartitionLength;
        private readonly int m_unmanagedPartitionLength;
        private readonly int m_tagPartitionLength;
        private readonly int m_entitySize;

        private EntityArchetype()
        {
            m_componentTypes = Array.Empty<ComponentType>();
            m_componentBitmask = Array.Empty<uint>();
            m_entitySize = Unsafe.SizeOf<Entity>();
        }

        private EntityArchetype(ComponentType[] componentTypes)
        {
            m_componentTypes = componentTypes;
            m_componentBitmask = new uint[componentTypes[^1].ID + 32 >> 5];
            m_entitySize = Unsafe.SizeOf<Entity>();

            int freeIndex = 0;
            ComponentType? previous = null;

            foreach (ComponentType current in m_componentTypes)
            {
                if (!ComponentType.Equals(previous, current))
                {
                    m_componentTypes[freeIndex++] = previous = current;
                    m_componentBitmask[current.ID >> 5] |= 1u << current.ID;
                    m_entitySize += current.Size;

                    switch (current.Category)
                    {
                        case ComponentTypeCategory.Managed:
                            m_managedPartitionLength++;
                            continue;
                        case ComponentTypeCategory.Unmanaged:
                            m_unmanagedPartitionLength++;
                            continue;
                        default:
                            m_tagPartitionLength++;
                            continue;
                    }
                }
            }

            Array.Resize(ref m_componentTypes, freeIndex);
        }

        /// <summary>
        /// Gets an entity archetype instance that models entities without any components.
        /// </summary>
        public static EntityArchetype Base
        {
            get => s_base;
        }

        /// <summary>
        /// Gets a read-only span of component types that compose the entity archetype.
        /// </summary>
        /// 
        /// <remarks>
        /// The order in which the component types appear in the span is used to determine the
        /// layout of component types in entity tables. This is done to allow entity tables to
        /// efficiently manage component data upon the addition/removal of entities.
        /// </remarks>
        public ReadOnlySpan<ComponentType> ComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_componentTypes);
        }

        /// <summary>
        /// Gets a read-only bitmask that compactly stores information about the component types
        /// that compose the entity archetype.
        /// </summary>
        /// 
        /// <remarks>
        /// Each individual bit set in the bitmask corresponds to a given component type based on
        /// its ID. This allows entity filters to quickly match entity archetypes with large sets
        /// of component types using bitwise operations.
        /// </remarks>
        public ReadOnlySpan<uint> ComponentBitmask
        {
            get => new ReadOnlySpan<uint>(m_componentBitmask);
        }

        /// <summary>
        /// Gets an integer that indicates how many managed component types compose the entity
        /// archetype.
        /// </summary>
        /// 
        /// <remarks>
        /// Managed component types precede unmanaged and tag component types within the span
        /// returned by the <see cref="ComponentTypes"/> property.
        /// </remarks>
        public int ManagedPartitionLength
        {
            get => m_managedPartitionLength;
        }

        /// <summary>
        /// Gets an integer that indicates how many unmanaged component types compose the entity
        /// archetype.
        /// </summary>
        /// 
        /// <remarks>
        /// Unmanaged component types supercede managed component types and precede tag component
        /// types within the span returned by the <see cref="ComponentTypes"/> property.
        /// </remarks>
        public int UnmanagedPartitionLength
        {
            get => m_unmanagedPartitionLength;
        }

        /// <summary>
        /// Gets an integer that indicates how many tag component types compose the entity
        /// archetype.
        /// </summary>
        /// 
        /// <remarks>
        /// Tag component types supercede managed and unmanaged component types within the span
        /// returned by the <see cref="ComponentTypes"/> property.
        /// </remarks>
        public int TagPartitionLength
        {
            get => m_tagPartitionLength;
        }

        /// <summary>
        /// Gets the total size of an entity and its components in bytes.
        /// </summary>
        public int EntitySize
        {
            get => m_entitySize;
        }

        /// <summary>
        /// Creates an entity archetype that is composed of component types from the specified
        /// array.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity archetype that is composed of component types from the array, or
        /// <see cref="EntityArchetype.Base"/> if the array contains no component types.
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
        /// Creates an entity archetype that is composed of component types from the specified
        /// sequence.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity archetype that is composed of component types from the sequence, or
        /// <see cref="EntityArchetype.Base"/> if the sequence contains no component types.
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
        /// Creates an entity archetype that is composed of component types from the specified
        /// span.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of component types.
        /// </param>
        /// 
        /// <returns>
        /// An entity archetype that is composed of component types from the span, or
        /// <see cref="EntityArchetype.Base"/> if the span contains no component types.
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
        /// Determines whether two specified entity archetype objects have the same value.
        /// </summary>
        /// 
        /// <param name="a">
        /// The first entity archetype to compare, or <see langword="null"/>.
        /// </param>
        /// 
        /// <param name="b">
        /// The second entity archetype to compare, or <see langword="null"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the value of <paramref name="a"/> is the same as the value of
        /// <paramref name="b"/>; otherwise, <see langword="false"/>. If both <paramref name="a"/>
        /// and <paramref name="b"/> are <see langword="null"/>, the method returns
        /// <see langword="true"/>.
        /// </returns>
        public static bool Equals(EntityArchetype? a, EntityArchetype? b)
        {
            return a == b
                || a != null
                && b != null
                && a.ComponentBitmask.SequenceEqual(b.ComponentBitmask);
        }

        /// <summary>
        /// Creates an entity archetype with a component type added to it.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to add.
        /// </param>
        /// 
        /// <returns>
        /// An entity archetype with the component type added, or the calling entity archetype if
        /// it contains the component type or if the component type is <see langword="null"/>.
        /// </returns>
        public EntityArchetype Add(ComponentType componentType)
        {
            if (componentType == null || BitmaskOperations.Test(ComponentBitmask, componentType.ID))
            {
                return this;
            }

            ComponentType[] source = m_componentTypes;
            ComponentType[] destination = new ComponentType[source.Length + 1];
            int index = 0;
            ComponentType current;

            while (index < source.Length && ComponentType.Compare(current = source[index], componentType) < 0)
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
        /// Creates an entity archetype with a component type removed from it.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to remove.
        /// </param>
        /// 
        /// <returns>
        /// An entity archetype with the component type removed, or the calling entity archetype if
        /// it does not contain the component type or if the component type is
        /// <see langword="null"/>.
        /// </returns>
        public EntityArchetype Remove(ComponentType componentType)
        {
            if (componentType == null || !BitmaskOperations.Test(ComponentBitmask, componentType.ID))
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

            while (!ComponentType.Equals(current = source[index], componentType))
            {
                destination[index++] = current;
            }

            while (index < destination.Length)
            {
                destination[index] = source[++index];
            }

            return new EntityArchetype(destination);
        }

        /// <summary>
        /// Indicates whether the entity archetype contains a specified component type.
        /// </summary>
        /// 
        /// <param name="componentType">
        /// The component type to search for.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the entity archetype contains the component type,
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Contains(ComponentType componentType)
        {
            return componentType != null
                && BitmaskOperations.Test(ComponentBitmask, componentType.ID);
        }

        public bool Equals([NotNullWhen(true)] EntityArchetype? other)
        {
            return Equals(this, other);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(this, obj as EntityArchetype);
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
