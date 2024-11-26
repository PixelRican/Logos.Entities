using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
	public sealed class ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>, IComparable
	{
		private const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static int s_nextTypeId = -1;

		private readonly Type m_systemType;
		private readonly int m_id;
		private readonly int m_size;

		private ComponentType(Type systemType, int id, int size, bool isManaged)
		{
			m_systemType = systemType;
			m_id = id;

			if (size > 1 || systemType.GetFields(FieldBindingFlags).Length > 0)
			{
				m_size = isManaged ? size | int.MinValue : size;
			}
		}

		public Type SystemType
		{
			get => m_systemType;
		}

		public int Id
		{
			get => m_id;
		}

		public int Size
		{
			get => m_size & int.MaxValue;
		}

		public bool IsEmpty
		{
			get => m_size == 0;
		}

		public bool IsUnmanaged
		{
			get => m_size > 0;
		}

		public bool IsManaged
		{
			get => m_size < 0;
		}

		public ComponentTypeCode TypeCode
		{
			get
			{
				switch (m_size)
				{
					case < 0:
						return ComponentTypeCode.Managed;
					case > 0:
						return ComponentTypeCode.Unmanaged;
					default:
						return ComponentTypeCode.Empty;
				}
			}
		}

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

			// Determines which kind of comparison to use.
			int comparisonFlag = a.m_size ^ b.m_size;

			// Managed component types will always precede unmanaged component types.
			if (comparisonFlag < 0)
			{
				return a.m_size < 0 ? -1 : 1;
			}

			// Non-tag component types will always precede tag component types.
			if (comparisonFlag > 0)
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

			// Fall back to comparing Ids.
			return a.m_id.CompareTo(b.m_id);
		}

		public static bool Equals(ComponentType? a, ComponentType? b)
		{
			return a == b
				|| a != null
				&& b != null
				&& a.m_id == b.m_id
				&& a.m_size == b.m_size
				&& a.m_systemType == b.m_systemType;
		}

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
			if (obj == this)
			{
				return 0;
			}

			if (obj == null)
			{
				return 1;
			}

			if (obj is ComponentType other)
			{
				return m_id.CompareTo(other.m_id);
			}

			throw new ArgumentException("obj is not the same type as this instance.");
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
			return HashCode.Combine(m_systemType, m_id, m_size);
		}

		public override string ToString()
		{
			return $"ComponentType {{ Type = {m_systemType.FullName} Id = {m_id} }}";
		}

		private static class ComponentTypeLookup<T>
		{
			public static readonly ComponentType Value = new ComponentType(typeof(T), Interlocked.Increment(ref s_nextTypeId), Unsafe.SizeOf<T>(), RuntimeHelpers.IsReferenceOrContainsReferences<T>());
		}
	}
}
