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
    /// Represents component type declarations associated with a unique ID.
    /// </summary>
    public abstract class ComponentType : IComparable<ComponentType>, IComparable
    {
        private const DynamicallyAccessedMemberTypes FieldMembers = DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields;
        private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static int s_nextId = -1;

        private readonly Type m_runtimeType;
        private readonly int m_id;
        private readonly int m_size;
        private readonly ComponentTypeCategory m_category;

        private ComponentType(Type runtimeType, int id, int size, ComponentTypeCategory category)
        {
            m_runtimeType = runtimeType;
            m_id = id;
            m_size = size;
            m_category = category;
        }

        /// <summary>
        /// Gets the runtime type associated with the <see cref="ComponentType"/>.
        /// </summary>
        public Type RuntimeType
        {
            get => m_runtimeType;
        }

        /// <summary>
        /// Gets the unique ID associated with the <see cref="ComponentType"/>.
        /// </summary>
        public int Id
        {
            get => m_id;
        }

        /// <summary>
        /// Gets the size of a component associated with the <see cref="ComponentType"/>.
        /// </summary>
        public int Size
        {
            get => m_size;
        }

        /// <summary>
        /// Gets the category associated with the <see cref="ComponentType"/>.
        /// </summary>
        public ComponentTypeCategory Category
        {
            get => m_category;
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
            return GenericComponentType<T>.Instance;
        }

        /// <summary>
        /// Creates a one-dimensional array of the <see cref="ComponentType"/> and specified
        /// length, with zero-based indexing.
        /// </summary>
        /// 
        /// <param name="length">
        /// The size of the array to create.
        /// </param>
        /// 
        /// <returns>
        /// A new one-dimensional array of the <see cref="ComponentType"/> with the specified
        /// length, using zero-based indexing.
        /// </returns>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is negative.
        /// </exception>
        public abstract Array CreateArray(int length);

        public int CompareTo(ComponentType? other)
        {
            if (this == other)
            {
                return 0;
            }

            if (other == null)
            {
                return 1;
            }

            // Component types are first sorted in categorical order, which is defined by the
            // constant definition order in the ComponentTypeCategory enumeration.
            ComponentTypeCategory categoryA = m_category;
            ComponentTypeCategory categoryB = other.m_category;

            if (categoryA < categoryB)
            {
                return -1;
            }

            if (categoryA > categoryB)
            {
                return 1;
            }

            // Should two component types share the same categorical order, they will be sorted by
            // their IDs.
            return m_id.CompareTo(other.m_id);
        }

        public int CompareTo(object? obj)
        {
            ComponentType? other = obj as ComponentType;

            if (obj != other)
            {
                throw new ArgumentException(
                    "obj is not the same type as this instance.", nameof(obj));
            }

            return CompareTo(other);
        }

        public override string ToString()
        {
            return $"ComponentType {{ RuntimeType = {m_runtimeType}, Id = {m_id},"
                + $" Size = {m_size}, Category = {m_category} }}";
        }

        private sealed class GenericComponentType<[DynamicallyAccessedMembers(FieldMembers)] T> : ComponentType
        {
            public static readonly GenericComponentType<T> Instance;

            static GenericComponentType()
            {
                Type runtimeType = typeof(T);
                int id = Interlocked.Increment(ref s_nextId);
                int size = Unsafe.SizeOf<T>();
                ComponentTypeCategory category;

                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    category = ComponentTypeCategory.Managed;
                }
                else if (size > 1 || runtimeType.GetFields(InstanceMembers).Length > 0)
                {
                    category = ComponentTypeCategory.Unmanaged;
                }
                else
                {
                    category = ComponentTypeCategory.Tag;
                    size = 0;
                }

                Instance = new GenericComponentType<T>(runtimeType, id, size, category);
            }

            private GenericComponentType(Type runtimeType, int id, int size, ComponentTypeCategory category)
                : base(runtimeType, id, size, category)
            {
            }

            public override Array CreateArray(int length)
            {
                return new T[length];
            }
        }
    }
}
