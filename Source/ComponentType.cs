// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Logos.Entities
{
    /// <summary>
    /// Represents the mapping of a common language runtime (CLR) type to columns of components in a
    /// data source.
    /// </summary>
    public abstract class ComponentType : IComparable<ComponentType>, IComparable
    {
        private const DynamicallyAccessedMemberTypes FieldMembers =
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields;

        private static int s_nextIndex = -1;

        private readonly ComponentTypeCategory m_category;
        private readonly int m_index;

        private ComponentType(ComponentTypeCategory category)
        {
            m_category = category;
            m_index = Interlocked.Increment(ref s_nextIndex);
        }

        /// <summary>
        /// Gets the category of the <see cref="ComponentType"/>.
        /// </summary>
        /// <returns>
        /// The category of the <see cref="ComponentType"/>.
        /// </returns>
        public ComponentTypeCategory Category
        {
            get => m_category;
        }

        /// <summary>
        /// Gets the index of the <see cref="ComponentType"/>.
        /// </summary>
        /// <returns>
        /// The index of the <see cref="ComponentType"/>.
        /// </returns>
        public int Index
        {
            get => m_index;
        }

        /// <summary>
        /// Gets the underlying common language runtime (CLR) type associated with the
        /// <see cref="ComponentType"/>.
        /// </summary>
        /// <returns>
        /// The CLR type associated with the <see cref="ComponentType"/>.
        /// </returns>
        public abstract Type Type { get; }

        /// <summary>
        /// Gets the size of a value of the type associated with the <see cref="ComponentType"/>.
        /// </summary>
        /// <returns>
        /// The size, in bytes, of a value of the type associated with the
        /// <see cref="ComponentType"/>.
        /// </returns>
        public abstract int Size { get; }

        /// <summary>
        /// Gets a unique <see cref="ComponentType"/> associated with type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The associated type.
        /// </typeparam>
        /// <returns>
        /// A unique <see cref="ComponentType"/> associated with type <typeparamref name="T"/>.
        /// </returns>
        public static ComponentType TypeOf<T>()
        {
            return GenericComponentType<T>.Instance;
        }

        /// <inheritdoc cref="IComparable{T}.CompareTo"/>
        public int CompareTo(ComponentType? other)
        {
            if (other == this)
            {
                return 0;
            }

            if (other == null)
            {
                return 1;
            }

            // Component types are first sorted in categorical order, which is defined by the
            // constant definition order in the ComponentTypeCategory enumeration.
            int comparison = ((int)m_category).CompareTo((int)other.m_category);

            // Should two component types share the same categorical order, they will be sorted by
            // their type IDs instead.
            if (comparison == 0)
            {
                comparison = m_index.CompareTo(other.m_index);
            }

            return comparison;
        }

        /// <inheritdoc cref="IComparable.CompareTo"/>
        public int CompareTo(object? obj)
        {
            ComponentType? other = obj as ComponentType;

            if (obj != other)
            {
                ThrowForInvalidComparandType();
            }

            return CompareTo(other);
        }

        /// <summary>
        /// Creates a one-dimensional array of the type associated with the
        /// <see cref="ComponentType"/> and specified length, with zero-based indexing.
        /// </summary>
        /// <param name="length">
        /// The size of the array to create.
        /// </param>
        /// <returns>
        /// A new one-dimensional array of the type associated with the <see cref="ComponentType"/>
        /// with the specified length, using zero-based indexing.
        /// </returns>
        /// <exception cref="OverflowException">
        /// <paramref name="length"/> is negative.
        /// </exception>
        public abstract Array CreateArray(int length);

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"ComponentType {{ Type = {Type} }}";
        }

        [DoesNotReturn]
        private static void ThrowForInvalidComparandType()
        {
            throw new ArgumentException("Object must be of type ComponentType.", "obj");
        }

        private sealed class GenericComponentType<[DynamicallyAccessedMembers(FieldMembers)] T> : ComponentType
        {
            public static readonly GenericComponentType<T> Instance = new GenericComponentType<T>();

            private GenericComponentType()
                : base(GetCategory())
            {
            }

            public override Type Type
            {
                get => typeof(T);
            }

            public override int Size
            {
                get => Unsafe.SizeOf<T>();
            }

            public override Array CreateArray(int length)
            {
                return new T[length];
            }

            private static ComponentTypeCategory GetCategory()
            {
                const BindingFlags InstanceMembers = BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic;

                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    return ComponentTypeCategory.Managed;
                }
                
                if (Unsafe.SizeOf<T>() > 1 || typeof(T).GetFields(InstanceMembers).Length > 0)
                {
                    return ComponentTypeCategory.Unmanaged;
                }

                return ComponentTypeCategory.Tag;
            }
        }
    }
}
