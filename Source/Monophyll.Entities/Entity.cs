using System;

namespace Monophyll.Entities
{
	public readonly struct Entity : IEquatable<Entity>, IComparable<Entity>, IComparable
	{
		private readonly int m_sequenceNumber;
		private readonly int m_version;

		public Entity()
		{
			m_sequenceNumber = 0;
			m_version = 0;
		}

		public Entity(int sequenceNumber, int version)
		{
			m_sequenceNumber = sequenceNumber;
			m_version = version;
		}

		public int SequenceNumber
		{
			get => m_sequenceNumber;
		}

		public int Version
		{
			get => m_version;
		}

		public int CompareTo(Entity other)
		{
			return m_sequenceNumber.CompareTo(other.m_sequenceNumber);
		}

		public int CompareTo(object? obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (obj is not Entity other)
			{
				throw new ArgumentException("obj is not the same type as this instance.");
			}

			return m_sequenceNumber.CompareTo(other.m_sequenceNumber);
		}

		public bool Equals(Entity other)
		{
			return m_sequenceNumber == other.m_sequenceNumber
				&& m_version == other.m_version;
		}

		public override bool Equals(object? obj)
		{
			return obj is Entity other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(m_sequenceNumber, m_version);
		}

		public override string ToString()
		{
			return $"Entity {{ SequenceNumber = {m_sequenceNumber} Version = {m_version} }}";
		}

		public static bool operator ==(Entity left, Entity right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Entity left, Entity right)
		{
			return !left.Equals(right);
		}
	}
}
