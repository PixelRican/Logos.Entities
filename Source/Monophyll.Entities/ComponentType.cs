using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
	/// <summary>
	/// Represents component type declarations associated with an ID.
	/// </summary>
	public sealed class ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>, IComparable
	{
		private const BindingFlags FieldBindingFlags = BindingFlags.Instance
													   | BindingFlags.Public
													   | BindingFlags.NonPublic;

		private static int s_nextTypeId = -1;

		private readonly Type m_type;
		private readonly int m_id;
		private readonly int m_size;

		private ComponentType(Type type, int id, int size, bool isManaged)
		{
			m_type = type;
			m_id = id;

			if (size > 1 || type.GetFields(FieldBindingFlags).Length > 0)
			{
				m_size = isManaged ? size | int.MinValue : size;
			}
		}

		/// <summary>
		/// Gets the <see cref="System.Type"/> associated with the
		/// <see cref="ComponentType"/>.
		/// </summary>
		public Type Type
		{
			get => m_type;
		}

		/// <summary>
		/// Gets the numeric ID associated with the
		/// <see cref="ComponentType"/>.
		/// </summary>
		public int Id
		{
			get => m_id;
		}

		/// <summary>
		/// Gets the size of instances of the <see cref="ComponentType"/> in
		/// bytes.
		/// </summary>
		public int Size
		{
			get => m_size & int.MaxValue;
		}

		/// <summary>
		/// Gets a value that indicates whether instances of the
		/// <see cref="ComponentType"/> has no member fields.
		/// </summary>
		public bool IsTag
		{
			get => m_size == 0;
		}

		/// <summary>
		/// Gets a value that indicates whether instances of the
		/// <see cref="ComponentType"/> are neither references nor contain
		/// references as member fields.
		/// </summary>
		public bool IsUnmanaged
		{
			get => m_size > 0;
		}

		/// <summary>
		/// Gets a value that indicates whether instances of the
		/// <see cref="ComponentType"/> are references or contain references
		/// as member fields.
		/// </summary>
		public bool IsManaged
		{
			get => m_size < 0;
		}

		/// <summary>
		/// Gets the <see cref="ComponentTypeCategory"/> associated with the
		/// <see cref="ComponentType"/>.
		/// </summary>
		public ComponentTypeCategory Category
		{
			get
			{
				switch (m_size)
				{
					case < 0:
						return ComponentTypeCategory.Managed;
					case > 0:
						return ComponentTypeCategory.Unmanaged;
					default:
						return ComponentTypeCategory.Tag;
				}
			}
		}

		/// <summary>
		/// Compares two specified <see cref="ComponentType"/> objects and
		/// returns an integer that indicates their relative position in the
		/// sort order.
		/// </summary>
		/// <param name="a">
		/// The first <see cref="ComponentType"/> to compare.
		/// </param>
		/// <param name="b">
		/// The second <see cref="ComponentType"/> to compare.
		/// </param>
		/// <returns>
		/// A 32-bit signed integer that indicates the lexical relationship
		/// between the two comparands.
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

			// Determine what kind of comparison to use.
			int compareFlag = a.m_size ^ b.m_size;

			// Managed component types will always precede unmanaged component types.
			if (compareFlag < 0)
			{
				return a.m_size < 0 ? -1 : 1;
			}

			// Non-tag component types will always precede tag component types.
			if (compareFlag > 0)
			{
				if (a.m_size == 0)
				{
					return 1;
				}

				if (b.m_size == 0)
				{
					return -1;
				}
			}

			// Fall back to comparing IDs.
			return a.m_id.CompareTo(b.m_id);
		}

		/// <summary>
		/// Determines whether two specified <see cref="ComponentType"/>
		/// objects have the same value.
		/// </summary>
		/// <param name="a">
		/// The first <see cref="ComponentType"/> to compare, or
		/// <see langword="null"/>.
		/// </param>
		/// <param name="b">
		/// The second <see cref="ComponentType"/> to compare, or
		/// <see langword="null"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the value of <paramref name="a"/> is the
		/// same as the value of <paramref name="b"/>; otherwise,
		/// <see langword="false"/>. If both <paramref name="a"/> and
		/// <paramref name="b"/> are <see langword="null"/>, the method returns
		/// <see langword="true"/>.
		/// </returns>
		public static bool Equals(ComponentType? a, ComponentType? b)
		{
			return a == b
				|| a != null
				&& b != null
				&& a.m_id == b.m_id
				&& a.m_size == b.m_size
				&& a.m_type == b.m_type;
		}

		/// <summary>
		/// Gets a <see cref="ComponentType"/> instance associated with
		/// <see langword="typeof"/>(<typeparamref name="T"/>).
		/// </summary>
		/// <typeparam name="T">The type of the component.</typeparam>
		/// <returns>
		/// A <see cref="ComponentType"/> instance associated with
		/// <see langword="typeof"/>(<typeparamref name="T"/>).
		/// </returns>
		public static ComponentType TypeOf<T>()
		{
			return ComponentTypeLookup<T>.Value;
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
			return HashCode.Combine(m_type, m_id, m_size);
		}

		public override string ToString()
		{
			return $"ComponentType {{ Type = {m_type.Name} Id = {m_id} }}";
		}

		private static class ComponentTypeLookup<T>
		{
			public static readonly ComponentType Value = new ComponentType(typeof(T),
				Interlocked.Increment(ref s_nextTypeId), Unsafe.SizeOf<T>(),
				RuntimeHelpers.IsReferenceOrContainsReferences<T>());
		}
	}
}
