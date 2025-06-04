using Monophyll.Entities.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Monophyll.Entities
{
    public sealed class EntityFilter : IEquatable<EntityFilter>
    {
        private static readonly EntityFilter s_universal = new EntityFilter();

        private readonly ComponentType[] m_requiredComponentTypes;
        private readonly ComponentType[] m_includedComponentTypes;
        private readonly ComponentType[] m_excludedComponentTypes;
        private readonly uint[] m_requiredComponentBits;
        private readonly uint[] m_includedComponentBits;
        private readonly uint[] m_excludedComponentBits;

        private EntityFilter()
        {
            m_requiredComponentTypes = Array.Empty<ComponentType>();
            m_includedComponentTypes = Array.Empty<ComponentType>();
            m_excludedComponentTypes = Array.Empty<ComponentType>();
            m_requiredComponentBits = Array.Empty<uint>();
            m_includedComponentBits = Array.Empty<uint>();
            m_excludedComponentBits = Array.Empty<uint>();
        }

        private EntityFilter(ComponentType[] requiredComponentTypes, uint[] requiredComponentBits)
        {
            m_requiredComponentTypes = requiredComponentTypes;
            m_includedComponentTypes = Array.Empty<ComponentType>();
            m_excludedComponentTypes = Array.Empty<ComponentType>();
            m_requiredComponentBits = requiredComponentBits;
            m_includedComponentBits = Array.Empty<uint>();
            m_excludedComponentBits = Array.Empty<uint>();
        }

        private EntityFilter(ComponentType[] requiredComponentTypes, uint[] requiredComponentBits,
                             ComponentType[] includedComponentTypes, uint[] includedComponentBits,
                             ComponentType[] excludedComponentTypes, uint[] excludedComponentBits)
        {
            m_requiredComponentTypes = requiredComponentTypes;
            m_includedComponentTypes = includedComponentTypes;
            m_excludedComponentTypes = excludedComponentTypes;
            m_requiredComponentBits = requiredComponentBits;
            m_includedComponentBits = includedComponentBits;
            m_excludedComponentBits = excludedComponentBits;
        }

		public static EntityFilter Universal
        {
            get => s_universal;
        }

        public ReadOnlySpan<ComponentType> RequiredComponentTypes
        {
            get => m_requiredComponentTypes;
        }

        public ReadOnlySpan<ComponentType> IncludedComponentTypes
        {
            get => m_includedComponentTypes;
        }

        public ReadOnlySpan<ComponentType> ExcludedComponentTypes
        {
            get => m_excludedComponentTypes;
        }

        public ReadOnlySpan<uint> RequiredComponentBits
        {
            get => m_requiredComponentBits;
        }

        public ReadOnlySpan<uint> IncludedComponentBits
        {
            get => m_includedComponentBits;
        }

        public ReadOnlySpan<uint> ExcludedComponentBits
        {
            get => m_excludedComponentBits;
        }

        public static EntityFilter Create(ComponentType[] requiredComponentTypes)
        {
            ArgumentNullException.ThrowIfNull(requiredComponentTypes);

            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBits))
            {
                return new EntityFilter(requiredTypes, requiredBits);
            }

            return s_universal;
        }

        public static EntityFilter Create(ComponentType[] requiredComponentTypes,
										  ComponentType[] includedComponentTypes,
										  ComponentType[] excludedComponentTypes)
		{
            ArgumentNullException.ThrowIfNull(requiredComponentTypes);
            ArgumentNullException.ThrowIfNull(includedComponentTypes);
            ArgumentNullException.ThrowIfNull(excludedComponentTypes);

            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBits) |
                TryBuild(includedComponentTypes, out ComponentType[] includedTypes, out uint[] includedBits) |
                TryBuild(excludedComponentTypes, out ComponentType[] excludedTypes, out uint[] excludedBits))
            {
                return new EntityFilter(requiredTypes, requiredBits,
                                        includedTypes, includedBits,
                                        excludedTypes, excludedBits);
            }

            return s_universal;
		}

        public static EntityFilter Create(IEnumerable<ComponentType> requiredComponentTypes)
        {
            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBits))
            {
                return new EntityFilter(requiredTypes, requiredBits);
            }

            return s_universal;
        }

        public static EntityFilter Create(IEnumerable<ComponentType> requiredComponentTypes,
										  IEnumerable<ComponentType> includedComponentTypes,
										  IEnumerable<ComponentType> excludedComponentTypes)
        {
            ArgumentNullException.ThrowIfNull(requiredComponentTypes);
            ArgumentNullException.ThrowIfNull(includedComponentTypes);
            ArgumentNullException.ThrowIfNull(excludedComponentTypes);

            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBits) |
                TryBuild(includedComponentTypes, out ComponentType[] includedTypes, out uint[] includedBits) |
                TryBuild(excludedComponentTypes, out ComponentType[] excludedTypes, out uint[] excludedBits))
            {
                return new EntityFilter(requiredTypes, requiredBits,
                                        includedTypes, includedBits,
                                        excludedTypes, excludedBits);
            }

            return s_universal;
        }

        public static EntityFilter Create(ReadOnlySpan<ComponentType> requiredComponentTypes)
        {
            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBits))
            {
                return new EntityFilter(requiredTypes, requiredBits);
            }

            return s_universal;
        }

        public static EntityFilter Create(ReadOnlySpan<ComponentType> requiredComponentTypes,
										  ReadOnlySpan<ComponentType> includedComponentTypes,
										  ReadOnlySpan<ComponentType> excludedComponentTypes)
        {
            if (TryBuild(requiredComponentTypes, out ComponentType[] requiredTypes, out uint[] requiredBits) |
                TryBuild(includedComponentTypes, out ComponentType[] includedTypes, out uint[] includedBits) |
                TryBuild(excludedComponentTypes, out ComponentType[] excludedTypes, out uint[] excludedBits))
            {
                return new EntityFilter(requiredTypes, requiredBits,
                                        includedTypes, includedBits,
                                        excludedTypes, excludedBits);
            }

            return s_universal;
        }

        private static bool TryBuild(ComponentType[] arguments,
            out ComponentType[] componentTypes, out uint[] componentBits)
        {
            if (arguments.Length > 0)
            {
                componentTypes = new ComponentType[arguments.Length];
                Array.Copy(arguments, componentTypes, arguments.Length);
                return TryFinalizeBuild(ref componentTypes, out componentBits);
            }

            componentTypes = Array.Empty<ComponentType>();
            componentBits = Array.Empty<uint>();
            return false;
        }

        private static bool TryBuild(IEnumerable<ComponentType> arguments,
            out ComponentType[] componentTypes, out uint[] componentBits)
        {
            componentTypes = arguments.ToArray();

            if (componentTypes.Length > 0)
            {
                return TryFinalizeBuild(ref componentTypes, out componentBits);
            }

            componentBits = Array.Empty<uint>();
            return false;
        }

        private static bool TryBuild(ReadOnlySpan<ComponentType> arguments,
            out ComponentType[] componentTypes, out uint[] componentBits)
        {
            componentTypes = arguments.ToArray();

            if (componentTypes.Length > 0)
            {
                return TryFinalizeBuild(ref componentTypes, out componentBits);
            }

            componentBits = Array.Empty<uint>();
            return false;
        }

        private static bool TryFinalizeBuild(ref ComponentType[] componentTypes, out uint[] componentBits)
        {
            Array.Sort(componentTypes);

            if (componentTypes[^1] == null)
            {
                componentTypes = Array.Empty<ComponentType>();
                componentBits = Array.Empty<uint>();
                return false;
            }

            componentBits = new uint[componentTypes[^1].Id + 32 >> 5];

            int freeIndex = 0;
            ComponentType? previous = null;

            foreach (ComponentType current in componentTypes)
            {
                if (!ComponentType.Equals(previous, current))
                {
                    componentTypes[freeIndex++] = previous = current;
                    componentBits[current.Id >> 5] |= 1u << current.Id;
                }
            }

            Array.Resize(ref componentTypes, freeIndex);
            return true;
        }

        public static Builder Require(ComponentType[] componentTypes)
		{
			return new Builder().Require(componentTypes);
		}

		public static Builder Require(IEnumerable<ComponentType> componentTypes)
		{
			return new Builder().Require(componentTypes);
		}

		public static Builder Require(ReadOnlySpan<ComponentType> componentTypes)
		{
			return new Builder().Require(componentTypes);
		}

		public static Builder Include(ComponentType[] componentTypes)
		{
			return new Builder().Include(componentTypes);
		}

		public static Builder Include(IEnumerable<ComponentType> componentTypes)
		{
			return new Builder().Include(componentTypes);
		}

		public static Builder Include(ReadOnlySpan<ComponentType> componentTypes)
		{
			return new Builder().Include(componentTypes);
		}

		public static Builder Exclude(ComponentType[] componentTypes)
		{
			return new Builder().Exclude(componentTypes);
		}

		public static Builder Exclude(IEnumerable<ComponentType> componentTypes)
		{
			return new Builder().Exclude(componentTypes);
		}

		public static Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
		{
			return new Builder().Exclude(componentTypes);
		}

		public static bool Equals(EntityFilter? a, EntityFilter? b)
		{
			return a == b
				|| a != null
				&& b != null
				&& a.RequiredComponentBits.SequenceEqual(b.RequiredComponentBits)
                && a.IncludedComponentBits.SequenceEqual(b.IncludedComponentBits)
                && a.ExcludedComponentBits.SequenceEqual(b.ExcludedComponentBits);
        }

        public bool Equals(EntityFilter? other)
        {
			return Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(this, obj as EntityFilter);
        }

        public override int GetHashCode()
        {
			return HashCode.Combine(BitSetOperations.GetHashCode(RequiredComponentBits),
									BitSetOperations.GetHashCode(IncludedComponentBits),
									BitSetOperations.GetHashCode(ExcludedComponentBits));
        }

		public bool Requires(ComponentType componentType)
		{
            return componentType != null
                && BitSetOperations.Contains(RequiredComponentBits, componentType.Id);
		}

		public bool Includes(ComponentType componentType)
        {
            return componentType != null
                && BitSetOperations.Contains(IncludedComponentBits, componentType.Id);
        }

		public bool Excludes(ComponentType componentType)
        {
            return componentType != null
                && BitSetOperations.Contains(ExcludedComponentBits, componentType.Id);
        }

        public bool Matches(EntityArchetype archetype)
        {
            ReadOnlySpan<uint> componentBits;
			return archetype != null
				&& BitSetOperations.IsSubsetOf(RequiredComponentBits, componentBits = archetype.ComponentBits)
				&& (m_includedComponentBits.Length == 0 || BitSetOperations.Overlaps(IncludedComponentBits, componentBits))
				&& !BitSetOperations.Overlaps(ExcludedComponentBits, componentBits);
        }

        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        public sealed class Builder
        {
            private ComponentType[] m_requiredComponentTypes;
            private ComponentType[] m_includedComponentTypes;
            private ComponentType[] m_excludedComponentTypes;
            private uint[] m_requiredComponentBits;
            private uint[] m_includedComponentBits;
            private uint[] m_excludedComponentBits;

            public Builder()
			{
                Reset();
			}

            public Builder(EntityFilter filter)
            {
                ArgumentNullException.ThrowIfNull(filter);

                m_requiredComponentTypes = filter.m_requiredComponentTypes;
                m_includedComponentTypes = filter.m_includedComponentTypes;
                m_excludedComponentTypes = filter.m_excludedComponentTypes;
                m_requiredComponentBits = filter.m_requiredComponentBits;
                m_includedComponentBits = filter.m_includedComponentBits;
                m_excludedComponentBits = filter.m_excludedComponentBits;
            }

            [MemberNotNull(nameof(m_requiredComponentTypes), nameof(m_requiredComponentBits),
                           nameof(m_includedComponentTypes), nameof(m_includedComponentBits),
                           nameof(m_excludedComponentTypes), nameof(m_excludedComponentBits))]
            public void Reset()
            {
                m_requiredComponentTypes = Array.Empty<ComponentType>();
                m_includedComponentTypes = Array.Empty<ComponentType>();
                m_excludedComponentTypes = Array.Empty<ComponentType>();
                m_requiredComponentBits = Array.Empty<uint>();
                m_includedComponentBits = Array.Empty<uint>();
                m_excludedComponentBits = Array.Empty<uint>();
            }

            public Builder Require(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBits);
                return this;
			}

            public Builder Require(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBits);
                return this;
            }

            public Builder Require(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_requiredComponentTypes, out m_requiredComponentBits);
                return this;
            }

			public Builder Include(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBits);
                return this;
            }

			public Builder Include(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBits);
                return this;
            }

			public Builder Include(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_includedComponentTypes, out m_includedComponentBits);
                return this;
            }

			public Builder Exclude(ComponentType[] componentTypes)
            {
                ArgumentNullException.ThrowIfNull(componentTypes);
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBits);
                return this;
            }

			public Builder Exclude(IEnumerable<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBits);
                return this;
            }

			public Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
            {
                TryBuild(componentTypes, out m_excludedComponentTypes, out m_excludedComponentBits);
                return this;
            }

			public EntityFilter Build()
			{
				if (m_requiredComponentTypes.Length > 0 ||
                    m_includedComponentTypes.Length > 0 ||
                    m_excludedComponentTypes.Length > 0)
				{
					return new EntityFilter(m_requiredComponentTypes, m_requiredComponentBits,
                                            m_includedComponentTypes, m_includedComponentBits,
                                            m_excludedComponentTypes, m_excludedComponentBits);
				}

				return s_universal;
			}
		}
    }
}
