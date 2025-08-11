// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Logos.Entities
{
    /// <summary>
    /// Represents an identifier associated with a set of components.
    /// </summary>
    public readonly struct Entity : IEquatable<Entity>
    {
        private readonly int m_index;
        private readonly int m_version;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> structure that has the specified
        /// index and version.
        /// </summary>
        /// <param name="index">
        /// The index of the <see cref="Entity"/>.
        /// </param>
        /// <param name="version">
        /// The version of the <see cref="Entity"/>.
        /// </param>
        public Entity(int index, int version)
        {
            m_index = index;
            m_version = version;
        }

        /// <summary>
        /// Gets the index of the <see cref="Entity"/>.
        /// </summary>
        /// <returns>
        /// The index of the <see cref="Entity"/>.
        /// </returns>
        public int Index
        {
            get => m_index;
        }

        /// <summary>
        /// Gets the version of the <see cref="Entity"/>.
        /// </summary>
        /// <returns>
        /// The version of the <see cref="Entity"/>.
        /// </returns>
        public int Version
        {
            get => m_version;
        }

        /// <inheritdoc cref="IEquatable{T}.Equals"/>
        public bool Equals(Entity other)
        {
            return m_index == other.m_index
                && m_version == other.m_version;
        }

        /// <inheritdoc cref="object.Equals"/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Entity other && Equals(other);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return HashCode.Combine(m_index, m_version);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"Entity {{ Index = {m_index}, Version = {m_version} }}";
        }

        /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.operator =="/>
        public static bool operator ==(Entity left, Entity right)
        {
            return left.m_index == right.m_index
                && left.m_version == right.m_version;
        }

        /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.operator !="/>
        public static bool operator !=(Entity left, Entity right)
        {
            return left.m_index != right.m_index
                || left.m_version != right.m_version;
        }
    }
}
