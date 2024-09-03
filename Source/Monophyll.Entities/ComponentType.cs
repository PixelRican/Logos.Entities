using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
	public abstract class ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>, IComparable
	{
		private static int s_nextComponentTypeId = -1;

		private readonly Type m_type;
		private readonly int m_byteSize;
		private readonly int m_id;

		private ComponentType(Type type, int byteSize)
		{
			if (byteSize == 1 &&
				type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0)
			{
				byteSize = 0;
			}

			m_type = type;
			m_byteSize = byteSize;
			m_id = Interlocked.Increment(ref s_nextComponentTypeId);
		}

		public Type Type
		{
			get => m_type;
		}

		public int ByteSize
		{
			get => m_byteSize;
		}

		public int Id
		{
			get => m_id;
		}

		public static ComponentType TypeOf<T>() where T : unmanaged
		{
			return RuntimeComponentType<T>.Instance;
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

			return a.m_id.CompareTo(b.m_id);
		}

		public static bool Equals(ComponentType? a, ComponentType? b)
		{
			return a == b
				|| a != null
				&& b != null
				&& a.m_id == b.m_id
				&& a.m_byteSize == b.m_byteSize
				&& a.m_type == b.m_type;
		}

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

			return m_id.CompareTo(other.m_id);
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

			if (obj is not ComponentType other)
			{
				throw new ArgumentException("obj is not the same type as this instance.");
			}

			return m_id.CompareTo(other.m_id);
		}

		public bool Equals([NotNullWhen(true)] ComponentType? other)
		{
			return other == this
				|| other != null
				&& m_id == other.m_id
				&& m_byteSize == other.m_byteSize
				&& m_type == other.m_type;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			return Equals(obj as ComponentType);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(m_type, m_byteSize, m_id);
		}

		public override string ToString()
		{
			return $"ComponentType {{ Type = {m_type.Name} ByteSize = {m_byteSize} Id = {m_id} }}";
		}

		private sealed class RuntimeComponentType<T> : ComponentType where T : unmanaged
		{
			public static readonly RuntimeComponentType<T> Instance = new RuntimeComponentType<T>();

			private RuntimeComponentType() : base(typeof(T), Unsafe.SizeOf<T>())
			{
			}
		}
	}
}
