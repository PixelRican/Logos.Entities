using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace Monophyll.Entities
{
    public sealed class EntityFilter : IEquatable<EntityFilter>
    {
        private static readonly EntityFilter s_universal = new();

        private readonly ComponentType[] m_requiredComponentTypes;
        private readonly ComponentType[] m_optionalComponentTypes;
        private readonly ComponentType[] m_excludedComponentTypes;
        private readonly uint[] m_requiredComponentBits;
        private readonly uint[] m_optionalComponentBits;
        private readonly uint[] m_excludedComponentBits;

        public EntityFilter()
        {
            m_requiredComponentTypes = [];
            m_optionalComponentTypes = [];
            m_excludedComponentTypes = [];
            m_requiredComponentBits = [];
            m_optionalComponentBits = [];
            m_excludedComponentBits = [];
        }

        public static EntityFilter Universal
        {
            get => s_universal;
        }

        public ImmutableArray<ComponentType> RequiredComponentTypes
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_requiredComponentTypes);
        }

        public ImmutableArray<ComponentType> OptionalComponentTypes
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_optionalComponentTypes);
        }

        public ImmutableArray<ComponentType> ExcludedComponentTypes
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_excludedComponentTypes);
        }

        public ImmutableArray<uint> RequiredComponentBits
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_requiredComponentBits);
        }

        public ImmutableArray<uint> OptionalComponentBits
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_optionalComponentBits);
        }

        public ImmutableArray<uint> ExcludedComponentBits
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_excludedComponentBits);
        }

        public bool Equals(EntityFilter? other)
        {
            return other == this
                || other != null
                && ((ReadOnlySpan<uint>)m_requiredComponentBits).SequenceEqual(other.m_requiredComponentBits)
                && ((ReadOnlySpan<uint>)m_optionalComponentBits).SequenceEqual(other.m_optionalComponentBits)
                && ((ReadOnlySpan<uint>)m_excludedComponentBits).SequenceEqual(other.m_excludedComponentBits);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as EntityFilter);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = default;
            int requiredHashCode;
            int optionalHashCode;

            for (int i = 0; i < m_requiredComponentBits.Length; i++)
            {
                hashCode.Add(m_requiredComponentBits[i]);
            }

            requiredHashCode = hashCode.ToHashCode();
            hashCode = default;

            for (int i = 0; i < m_optionalComponentBits.Length; i++)
            {
                hashCode.Add(m_optionalComponentBits[i]);
            }

            optionalHashCode = hashCode.ToHashCode();
            hashCode = default;

            for (int i = 0; i < m_excludedComponentBits.Length; i++)
            {
                hashCode.Add(m_excludedComponentBits[i]);
            }

            return HashCode.Combine(requiredHashCode, optionalHashCode, hashCode.ToHashCode());
        }

        public bool Matches(EntityArchetype archetype)
        {
            if (archetype == null)
            {
                return false;
            }

            ImmutableArray<uint> componentBits = archetype.ComponentBits;

            if (m_requiredComponentBits.Length > componentBits.Length)
            {
                return false;
            }

            for (int i = 0; i < m_requiredComponentBits.Length; i++)
            {
                if ((m_requiredComponentBits[i] & ~componentBits[i]) != 0)
                {
                    return false;
                }
            }

            if (m_optionalComponentBits.Length > 0)
            {
                int length = Math.Min(m_optionalComponentBits.Length, componentBits.Length);
                int i = 0;

                while (i < length && (m_optionalComponentBits[i] & componentBits[i]) == 0)
                {
                    i++;
                }

                if (i == length)
                {
                    return false;
                }
            }

            for (int i = Math.Min(m_excludedComponentBits.Length, componentBits.Length) - 1; i >= 0; i--)
            {
                if ((m_excludedComponentBits[i] & componentBits[i]) != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
