using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
	public sealed class ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>, IComparable
	{
		private const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static int s_nextSequenceNumber = -1;

		private readonly Type m_type;
		private readonly int m_byteSize;
		private readonly int m_id;

		private ComponentType(Type type, int byteSize, int id)
		{
			if (byteSize == 1 && type.GetFields(FieldBindingFlags).Length == 0)
			{
				byteSize = 0;
			}

			m_type = type;
			m_byteSize = byteSize;
			m_id = id;
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
			return TypeLookup<T>.Value;
		}

		public int CompareTo(ComponentType? other)
		{
			if (other is null)
			{
				return 1;
			}

			return m_id.CompareTo(other.m_id);
		}

		public int CompareTo(object? obj)
		{
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

		public bool Equals(ComponentType? other)
		{
			return ReferenceEquals(this, other)
				|| other is not null
				&& m_id == other.m_id
				&& m_byteSize == other.m_byteSize
				&& m_type == other.m_type;
		}

		public override bool Equals(object? obj)
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

		private static class TypeLookup<T> where T : unmanaged
		{
			public static readonly ComponentType Value =
				new(typeof(T), Unsafe.SizeOf<T>(), Interlocked.Increment(ref s_nextSequenceNumber));
		}
	}
}
