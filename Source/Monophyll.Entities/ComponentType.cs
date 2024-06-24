using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
	public sealed class ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>, IComparable
	{
		private const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private const int IsReferenceOrContainsReferencesFlag = 1 << 32;
		private const int RemoveFlagsBitMask = 0x7FFFFFFF;

		private static int s_nextSequenceNumber = -1;

		private readonly Type m_type;
		private readonly int m_byteSize;
		private readonly int m_sequenceNumber;

		private ComponentType(Type type, int byteSize, int sequenceNumber, bool isReferenceOrContainsReferences)
		{
			if (byteSize == 1 && type.GetFields(FieldBindingFlags).Length == 0)
			{
				byteSize = 0;
			}
			else if (isReferenceOrContainsReferences)
			{
				byteSize |= IsReferenceOrContainsReferencesFlag;
			}

			m_type = type;
			m_byteSize = byteSize;
			m_sequenceNumber = sequenceNumber;
		}

		public Type Type
		{
			get => m_type;
		}

		public int ByteSize
		{
			get => m_byteSize & RemoveFlagsBitMask;
		}

		public int SequenceNumber
		{
			get => m_sequenceNumber;
		}

		public bool IsReferenceOrContainsReferences
		{
			get => m_byteSize < 0;
		}

		public static ComponentType TypeOf<T>()
		{
			return RuntimeComponentType<T>.Instance;
		}

		public int CompareTo(ComponentType? other)
		{
			if (other is null)
			{
				return 1;
			}

			return m_sequenceNumber.CompareTo(other.m_sequenceNumber);
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

			return m_sequenceNumber.CompareTo(other.m_sequenceNumber);
		}

		public bool Equals(ComponentType? other)
		{
			return ReferenceEquals(this, other)
				|| other is not null
				&& m_sequenceNumber == other.m_sequenceNumber
				&& m_byteSize == other.m_byteSize
				&& m_type == other.m_type;
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as ComponentType);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(m_type, m_byteSize, m_sequenceNumber);
		}

		public override string ToString()
		{
			return $"ComponentType {{ Type = {m_type.Name} SequenceNumber = {m_sequenceNumber} }}";
		}

		private static class RuntimeComponentType<T>
		{
			public static readonly ComponentType Instance =
				new(typeof(T), Unsafe.SizeOf<T>(), Interlocked.Increment(ref s_nextSequenceNumber),
					RuntimeHelpers.IsReferenceOrContainsReferences<T>());
		}
	}
}
