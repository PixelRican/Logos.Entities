using System;
using System.Diagnostics.CodeAnalysis;

namespace Monophyll.Entities
{
	public readonly struct Entity : IEquatable<Entity>, IComparable<Entity>, IComparable
	{
		private readonly int m_id;
		private readonly int m_version;

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
			if (m_id != other.m_id)
			{
				return m_id < other.m_id ? -1 : 1;
			}

			return m_version.CompareTo(other.m_version);
		}

		public int CompareTo(object? obj)
		{
			if (obj is not Entity other)
			{
				if (obj != null)
				{
					throw new ArgumentException("obj is not the same type as this instance.");
				}

				return 1;
			}

			return CompareTo(other);
		}

		public bool Equals(Entity other)
		{
			return m_id == other.m_id
				&& m_version == other.m_version;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
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
			return left.m_id == right.m_id
				&& left.m_version == right.m_version;
		}

		public static bool operator !=(Entity left, Entity right)
		{
			return left.m_id != right.m_id
				|| left.m_version != right.m_version;
		}
	}
}
