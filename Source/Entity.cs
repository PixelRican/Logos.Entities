// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Monophyll.Entities
{
    /// <summary>
    /// Represents an ID associated with a set of components.
    /// </summary>
    public readonly struct Entity : IEquatable<Entity>, IComparable<Entity>, IComparable
    {
        private readonly int m_identifier;
        private readonly int m_version;

        /// <summary>
        /// Initializes an new instance of the <see cref="Entity"/> structure that has the
        /// specified ID and version.
        /// </summary>
        /// 
        /// <param name="identifier">
        /// The ID.
        /// </param>
        /// 
        /// <param name="version">
        /// The version.
        /// </param>
        public Entity(int identifier, int version)
        {
            m_identifier = identifier;
            m_version = version;
        }

        /// <summary>
        /// Gets the ID of the <see cref="Entity"/>.
        /// </summary>
        public int Identifier
        {
            get => m_identifier;
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
            int comparison = m_identifier.CompareTo(other.Identifier);

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
            return m_identifier == other.m_identifier
                && m_version == other.m_version;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Entity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_identifier, m_version);
        }

        public override string ToString()
        {
            return $"Entity {{ Identifier = {m_identifier}, Version = {m_version} }}";
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
