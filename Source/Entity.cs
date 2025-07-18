// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Logos.Entities
{
    /// <summary>
    /// Represents an ID associated with a set of components.
    /// </summary>
    public readonly struct Entity : IEquatable<Entity>, IComparable<Entity>, IComparable
    {
        private readonly int m_id;
        private readonly int m_version;

        /// <summary>
        /// Initializes an new instance of the <see cref="Entity"/> structure that has the
        /// specified ID and version.
        /// </summary>
        /// 
        /// <param name="id">
        /// The ID.
        /// </param>
        /// 
        /// <param name="version">
        /// The version.
        /// </param>
        public Entity(int id, int version)
        {
            m_id = id;
            m_version = version;
        }

        /// <summary>
        /// Gets the ID of the <see cref="Entity"/>.
        /// </summary>
        public int Id
        {
            get => m_id;
        }

        /// <summary>
        /// Gets the version of the <see cref="Entity"/>.
        /// </summary>
        public int Version
        {
            get => m_version;
        }

        public int CompareTo(Entity other)
        {
            int comparison = m_id.CompareTo(other.Id);

            if (comparison != 0)
            {
                return comparison;
            }

            return m_version.CompareTo(other.m_version);
        }

        public int CompareTo(object? obj)
        {
            if (obj is Entity other)
            {
                return CompareTo(other);
            }

            if (obj != null)
            {
                throw new ArgumentException("obj is not the same type as this instance.");
            }

            return 1;
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
            return $"Entity {{ Id = {m_id}, Version = {m_version} }}";
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
