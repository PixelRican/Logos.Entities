// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Logos.Entities
{
    /// <summary>
    /// Represents component type declarations associated with a unique ID.
    /// </summary>
    public sealed class ComponentType : IComparable<ComponentType>, IComparable
    {
        private const BindingFlags Constraints =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static int s_nextId = -1;

        private readonly Type m_runtimeType;
        private readonly int m_id;
        private readonly int m_size;
        private readonly ComponentTypeCategory m_category;

        private ComponentType(Type runtimeType, int size, bool isManaged)
        {
            m_runtimeType = runtimeType;
            m_id = Interlocked.Increment(ref s_nextId);

            if (isManaged)
            {
                m_size = size;
                m_category = ComponentTypeCategory.Managed;
            }
            else if (size > 1 || runtimeType.GetFields(Constraints).Length > 0)
            {
                m_size = size;
                m_category = ComponentTypeCategory.Unmanaged;
            }
            else
            {
                m_category = ComponentTypeCategory.Tag;
            }
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
            return GenericTypeLookup<T>.Value;
        }

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

        private static class GenericTypeLookup<T>
        {
            public static readonly ComponentType Value = new ComponentType(typeof(T),
                Unsafe.SizeOf<T>(), RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }
}
