using System;
using System.Numerics;

namespace Monophyll.Entities
{
	public readonly struct Entity : IEqualityOperators<Entity, Entity, bool>, IEquatable<Entity>, IComparable<Entity>, IComparable
	{
		private readonly int m_id;
		private readonly int m_version;

		public Entity()
		{
			m_id = 0;
			m_version = 0;
		}

		public Entity(int id, int version)
		{
			m_id = id;
			m_version = version;
		}

		public int Id
		{
			get => m_id;
		}

		public int Version
		{
			get => m_version;
		}

		public int CompareTo(Entity other)
		{
			return m_id.CompareTo(other.m_id);
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

			return m_id.CompareTo(other.m_id);
		}

		public bool Equals(Entity other)
		{
			return m_id == other.m_id
				&& m_version == other.m_version;
		}

		public override bool Equals(object? obj)
		{
			return obj is Entity other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(m_id, m_version);
		}

		public override string ToString()
		{
			return $"Entity {{ Id = {m_id} Version = {m_version} }}";
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
