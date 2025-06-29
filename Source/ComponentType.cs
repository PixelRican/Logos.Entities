// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
    /// <summary>
    /// Represents component type declarations associated with a unique ID.
    /// </summary>
    public sealed class ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>, IComparable
    {
        private const BindingFlags Constraints = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static int s_nextIdentifier = -1;

        private readonly Type m_type;
        private readonly int m_identifier;
        private readonly int m_size;

        private ComponentType(Type type, int identifier, int size, bool isManaged)
        {
            m_type = type;
            m_identifier = identifier;

            if (size > 1 || type.GetFields(Constraints).Length > 0)
            {
                m_size = isManaged ? size | int.MinValue : size;
            }
        }

        /// <summary>
        /// Gets the runtime type associated with the <see cref="ComponentType"/>.
        /// </summary>
        public Type Type
        {
            get => m_type;
        }

        /// <summary>
        /// Gets the unique ID associated with the <see cref="ComponentType"/>.
        /// </summary>
        public int Identifier
        {
            get => m_identifier;
        }

        /// <summary>
        /// Gets the size of a component associated with the <see cref="ComponentType"/>.
        /// </summary>
        public int Size
        {
            get => m_size & int.MaxValue;
        }

        /// <summary>
        /// Gets the category associated with the <see cref="ComponentType"/>.
        /// </summary>
        public ComponentTypeCategory Category
        {
            get
            {
                int size = m_size;

                if (size < 0)
                {
                    return ComponentTypeCategory.Managed;
                }

                if (size > 0)
                {
                    return ComponentTypeCategory.Unmanaged;
                }

                return ComponentTypeCategory.Tag;
            }
        }

        /// <summary>
        /// Compares two specified <see cref="ComponentType"/> objects and returns an integer that
        /// indicates their relative position in the sort order.
        /// </summary>
        /// 
        /// <param name="a">
        /// The first object to compare, or <see langword="null"/>.
        /// </param>
        /// 
        /// <param name="b">
        /// The second object to compare, or <see langword="null"/>.
        /// </param>
        /// 
        /// <returns>
        /// A 32-bit signed integer that indicates the lexical relationship between the two
        /// comparands.
        /// </returns>
        public static int Compare(ComponentType? a, ComponentType? b)
        {
            if (a == b)
            {
                return 0;
            }

            if (a == null)
            {
                return -1;
            }

            if (b == null)
            {
                return 1;
            }

            int comparison = ((int)a.Category).CompareTo((int)b.Category);

            if (comparison != 0)
            {
                return comparison;
            }

            return a.m_identifier.CompareTo(b.m_identifier);
        }

        /// <summary>
        /// Determines whether two specified <see cref="ComponentType"/> objects have the same
        /// value.
        /// </summary>
        /// 
        /// <param name="a">
        /// The first object to compare, or <see langword="null"/>.
        /// </param>
        /// 
        /// <param name="b">
        /// The second object to compare, or <see langword="null"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the value of <paramref name="a"/> is the same as the value of
        /// <paramref name="b"/>; otherwise, <see langword="false"/>. If both <paramref name="a"/>
        /// and <paramref name="b"/> are <see langword="null"/>, the method returns
        /// <see langword="true"/>.
        /// </returns>
        public static bool Equals(ComponentType? a, ComponentType? b)
        {
            return a == b
                || a != null
                && b != null
                && a.m_identifier == b.m_identifier
                && a.m_size == b.m_size
                && a.m_type == b.m_type;
        }

        /// <summary>
        /// Gets a <see cref="ComponentType"/> that represents components of type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// The type of the component.
        /// </typeparam>
        /// 
        /// <returns>
        /// A <see cref="ComponentType"/> that represents components of type
        /// <typeparamref name="T"/>.
        /// </returns>
        public static ComponentType TypeOf<T>()
        {
            return TypeLookup<T>.Value;
        }

        public int CompareTo(ComponentType? other)
        {
            return Compare(this, other);
        }

        public int CompareTo(object? obj)
        {
            ComponentType? other = obj as ComponentType;

            if (obj != other)
            {
                throw new ArgumentException("obj is not the same type as this instance.");
            }

            return Compare(this, other);
        }

        public bool Equals([NotNullWhen(true)] ComponentType? other)
        {
            return Equals(this, other);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(this, obj as ComponentType);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_type, m_identifier, m_size);
        }

        public override string ToString()
        {
            return $"ComponentType {{ Type = {m_type.Name}, Identifier = {m_identifier} }}";
        }

        private static class TypeLookup<T>
        {
            public static readonly ComponentType Value = new ComponentType(typeof(T),
                Interlocked.Increment(ref s_nextIdentifier), Unsafe.SizeOf<T>(),
                RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }
}
