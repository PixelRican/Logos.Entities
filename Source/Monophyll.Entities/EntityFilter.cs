using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace Monophyll.Entities
{
    public sealed class EntityFilter : IEquatable<EntityFilter>
    {
        private static readonly EntityFilter s_universal = new();

        private readonly ComponentType[] m_requiredComponentTypes;
        private readonly ComponentType[] m_includedComponentTypes;
        private readonly ComponentType[] m_excludedComponentTypes;
        private readonly uint[] m_requiredComponentBits;
        private readonly uint[] m_includedComponentBits;
        private readonly uint[] m_excludedComponentBits;

        public EntityFilter()
        {
            m_requiredComponentTypes = [];
            m_includedComponentTypes = [];
            m_excludedComponentTypes = [];
            m_requiredComponentBits = [];
            m_includedComponentBits = [];
            m_excludedComponentBits = [];
        }

        public EntityFilter(EntityFilter other)
        {
            ArgumentNullException.ThrowIfNull(other, nameof(other));
            m_requiredComponentTypes = other.m_requiredComponentTypes;
            m_includedComponentTypes = other.m_includedComponentTypes;
            m_excludedComponentTypes = other.m_excludedComponentTypes;
            m_requiredComponentBits = other.m_requiredComponentBits;
            m_includedComponentBits = other.m_includedComponentBits;
            m_excludedComponentBits = other.m_excludedComponentBits;
        }

        public EntityFilter(params ComponentType[] requiredComponentTypes)
        {
            ArgumentNullException.ThrowIfNull(requiredComponentTypes, nameof(requiredComponentTypes));
			ComponentType[] args = requiredComponentTypes.Length == 0 ? [] : new ComponentType[requiredComponentTypes.Length];
			Array.Copy(requiredComponentTypes, args, args.Length);
            Array.Sort(args);
            Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);
            m_includedComponentTypes = [];
            m_excludedComponentTypes = [];
            m_includedComponentBits = [];
            m_excludedComponentBits = [];
        }

        public EntityFilter(IEnumerable<ComponentType> requiredComponentTypes)
        {
			ComponentType[] args = requiredComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);
			m_includedComponentTypes = [];
			m_excludedComponentTypes = [];
			m_includedComponentBits = [];
			m_excludedComponentBits = [];
		}

        public EntityFilter(ReadOnlySpan<ComponentType> requiredComponentTypes)
		{
			ComponentType[] args = requiredComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);
			m_includedComponentTypes = [];
			m_excludedComponentTypes = [];
			m_includedComponentBits = [];
			m_excludedComponentBits = [];
		}

		public EntityFilter(Span<ComponentType> requiredComponentTypes)
		{
			ComponentType[] args = requiredComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);
			m_includedComponentTypes = [];
			m_excludedComponentTypes = [];
			m_includedComponentBits = [];
			m_excludedComponentBits = [];
		}

		public EntityFilter(ComponentType[] requiredComponentTypes, ComponentType[] includedComponentTypes, ComponentType[] excludedComponentTypes)
        {
            ArgumentNullException.ThrowIfNull(requiredComponentTypes, nameof(requiredComponentTypes));
			ArgumentNullException.ThrowIfNull(includedComponentTypes, nameof(includedComponentTypes));
			ArgumentNullException.ThrowIfNull(excludedComponentTypes, nameof(excludedComponentTypes));

			ComponentType[] args = requiredComponentTypes.Length == 0 ? [] : new ComponentType[requiredComponentTypes.Length];
			Array.Copy(requiredComponentTypes, args, args.Length);
			Array.Sort(args);
			Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);

			args = includedComponentTypes.Length == 0 ? [] : new ComponentType[includedComponentTypes.Length];
			Array.Copy(includedComponentTypes, args, args.Length);
			Array.Sort(args);
			Initialize(args, out m_includedComponentTypes, out m_includedComponentBits);

			args = excludedComponentTypes.Length == 0 ? [] : new ComponentType[excludedComponentTypes.Length];
			Array.Copy(excludedComponentTypes, args, args.Length);
			Array.Sort(args);
			Initialize(args, out m_excludedComponentTypes, out m_excludedComponentBits);
		}

		public EntityFilter(IEnumerable<ComponentType> requiredComponentTypes, IEnumerable<ComponentType> includedComponentTypes, IEnumerable<ComponentType> excludedComponentTypes)
		{
			ArgumentNullException.ThrowIfNull(requiredComponentTypes, nameof(requiredComponentTypes));
			ArgumentNullException.ThrowIfNull(includedComponentTypes, nameof(includedComponentTypes));
			ArgumentNullException.ThrowIfNull(excludedComponentTypes, nameof(excludedComponentTypes));

			ComponentType[] args = requiredComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);

			args = includedComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_includedComponentTypes, out m_includedComponentBits);

			args = excludedComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_excludedComponentTypes, out m_excludedComponentBits);
		}

		public EntityFilter(ReadOnlySpan<ComponentType> requiredComponentTypes, ReadOnlySpan<ComponentType> includedComponentTypes, ReadOnlySpan<ComponentType> excludedComponentTypes)
		{
			ComponentType[] args = requiredComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);

			args = includedComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_includedComponentTypes, out m_includedComponentBits);

			args = excludedComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_excludedComponentTypes, out m_excludedComponentBits);
		}

		public EntityFilter(Span<ComponentType> requiredComponentTypes, Span<ComponentType> includedComponentTypes, Span<ComponentType> excludedComponentTypes)
		{
			ComponentType[] args = requiredComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);

			args = includedComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_includedComponentTypes, out m_includedComponentBits);

			args = excludedComponentTypes.ToArray();
			Array.Sort(args);
			Initialize(args, out m_excludedComponentTypes, out m_excludedComponentBits);
		}

        private EntityFilter(Builder builder)
        {
            m_requiredComponentTypes = ImmutableCollectionsMarshal.AsArray(builder.RequiredComponentTypes)!;
			m_includedComponentTypes = ImmutableCollectionsMarshal.AsArray(builder.IncludedComponentTypes)!;
			m_excludedComponentTypes = ImmutableCollectionsMarshal.AsArray(builder.ExcludedComponentTypes)!;
            m_requiredComponentBits = ImmutableCollectionsMarshal.AsArray(builder.RequiredComponentBits)!;
            m_includedComponentBits = ImmutableCollectionsMarshal.AsArray(builder.IncludedComponentBits)!;
            m_excludedComponentBits = ImmutableCollectionsMarshal.AsArray(builder.ExcludedComponentBits)!;
		}

		public static EntityFilter Universal
        {
            get => s_universal;
        }

        public ImmutableArray<ComponentType> RequiredComponentTypes
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_requiredComponentTypes);
        }

        public ImmutableArray<ComponentType> IncludedComponentTypes
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_includedComponentTypes);
        }

        public ImmutableArray<ComponentType> ExcludedComponentTypes
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_excludedComponentTypes);
        }

        public ImmutableArray<uint> RequiredComponentBits
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_requiredComponentBits);
        }

        public ImmutableArray<uint> IncludedComponentBits
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_includedComponentBits);
        }

        public ImmutableArray<uint> ExcludedComponentBits
        {
            get => ImmutableCollectionsMarshal.AsImmutableArray(m_excludedComponentBits);
        }

        private static void Initialize(ComponentType[] args, out ComponentType[] componentTypes, out uint[] componentBits)
        {
			ComponentType? componentTypeToCompare;

			if (args.Length == 0 || (componentTypeToCompare = args[^1]) == null)
			{
				componentTypes = [];
				componentBits = [];
				return;
			}

			componentTypes = args;
			componentBits = new uint[componentTypeToCompare.Id + 32 >> 5];
			componentTypeToCompare = null;

			int freeIndex = 0;

			for (int i = 0; i < componentTypes.Length; i++)
			{
				ComponentType currentComponentType = componentTypes[i];

				if (currentComponentType != componentTypeToCompare)
				{
					componentTypes[freeIndex++] = componentTypeToCompare = currentComponentType;
					componentBits[currentComponentType.Id >> 5] |= 1u << currentComponentType.Id;
				}
			}

			Array.Resize(ref componentTypes, freeIndex);
		}

		public static Builder CreateBuilder()
		{
			return new Builder();
		}

		public static Builder Require(params ComponentType[] componentTypes)
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

		public static Builder Require(Span<ComponentType> componentTypes)
		{
			return new Builder().Require(componentTypes);
		}

		public static Builder Include(params ComponentType[] componentTypes)
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

		public static Builder Include(Span<ComponentType> componentTypes)
		{
			return new Builder().Include(componentTypes);
		}

		public static Builder Exclude(params ComponentType[] componentTypes)
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

		public static Builder Exclude(Span<ComponentType> componentTypes)
		{
			return new Builder().Exclude(componentTypes);
		}

		public bool Equals(EntityFilter? other)
        {
            return other == this
                || other != null
                && ((ReadOnlySpan<uint>)m_requiredComponentBits).SequenceEqual(other.m_requiredComponentBits)
                && ((ReadOnlySpan<uint>)m_includedComponentBits).SequenceEqual(other.m_includedComponentBits)
                && ((ReadOnlySpan<uint>)m_excludedComponentBits).SequenceEqual(other.m_excludedComponentBits);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as EntityFilter);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = default;

            for (int i = 0; i < m_requiredComponentBits.Length; i++)
            {
                hashCode.Add(m_requiredComponentBits[i]);
            }

            for (int i = 0; i < m_includedComponentBits.Length; i++)
            {
                hashCode.Add(m_includedComponentBits[i]);
            }

            for (int i = 0; i < m_excludedComponentBits.Length; i++)
            {
                hashCode.Add(m_excludedComponentBits[i]);
            }

            return hashCode.ToHashCode();
        }

		public bool Requires(ComponentType componentType)
		{
			int index;
			return componentType != null
				&& (index = componentType.Id >> 5) < m_requiredComponentBits.Length
				&& (m_requiredComponentBits[index] & 1u << componentType.Id) != 0;
		}

		public bool Includes(ComponentType componentType)
		{
			int index;
			return componentType != null
				&& (index = componentType.Id >> 5) < m_includedComponentBits.Length
				&& (m_includedComponentBits[index] & 1u << componentType.Id) != 0;
		}

		public bool Excludes(ComponentType componentType)
		{
			int index;
			return componentType != null
				&& (index = componentType.Id >> 5) < m_excludedComponentBits.Length
				&& (m_excludedComponentBits[index] & 1u << componentType.Id) != 0;
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

            if (m_includedComponentBits.Length > 0)
            {
                int length = Math.Min(m_includedComponentBits.Length, componentBits.Length);
                int i = 0;

                while (i < length && (m_includedComponentBits[i] & componentBits[i]) == 0)
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

            internal Builder()
            {
                m_requiredComponentTypes = [];
                m_includedComponentTypes = [];
                m_excludedComponentTypes = [];
                m_requiredComponentBits = [];
                m_includedComponentBits = [];
                m_excludedComponentBits = [];
            }

			internal Builder(EntityFilter filter)
			{
				m_requiredComponentTypes = filter.m_requiredComponentTypes;
				m_includedComponentTypes = filter.m_includedComponentTypes;
				m_excludedComponentTypes = filter.m_excludedComponentTypes;
				m_requiredComponentBits = filter.m_requiredComponentBits;
				m_includedComponentBits = filter.m_includedComponentBits;
				m_excludedComponentBits = filter.m_excludedComponentBits;
			}

			public ImmutableArray<ComponentType> RequiredComponentTypes
			{
				get => ImmutableCollectionsMarshal.AsImmutableArray(m_requiredComponentTypes);
			}

			public ImmutableArray<ComponentType> IncludedComponentTypes
			{
				get => ImmutableCollectionsMarshal.AsImmutableArray(m_includedComponentTypes);
			}

			public ImmutableArray<ComponentType> ExcludedComponentTypes
			{
				get => ImmutableCollectionsMarshal.AsImmutableArray(m_excludedComponentTypes);
			}

			public ImmutableArray<uint> RequiredComponentBits
			{
				get => ImmutableCollectionsMarshal.AsImmutableArray(m_requiredComponentBits);
			}

			public ImmutableArray<uint> IncludedComponentBits
			{
				get => ImmutableCollectionsMarshal.AsImmutableArray(m_includedComponentBits);
			}

			public ImmutableArray<uint> ExcludedComponentBits
			{
				get => ImmutableCollectionsMarshal.AsImmutableArray(m_excludedComponentBits);
			}

			public void Reset()
			{
				m_requiredComponentTypes = [];
				m_includedComponentTypes = [];
				m_excludedComponentTypes = [];
				m_requiredComponentBits = [];
				m_includedComponentBits = [];
				m_excludedComponentBits = [];
			}

            public Builder Require(params ComponentType[] componentTypes)
            {
				ArgumentNullException.ThrowIfNull(componentTypes, nameof(componentTypes));
				ComponentType[] args = componentTypes.Length == 0 ? [] : new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, args, args.Length);
				Array.Sort(args);
				Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);
                return this;
			}

            public Builder Require(IEnumerable<ComponentType> componentTypes)
            {
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);
                return this;
			}

            public Builder Require(ReadOnlySpan<ComponentType> componentTypes)
            {
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);
				return this;
			}

			public Builder Require(Span<ComponentType> componentTypes)
			{
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_requiredComponentTypes, out m_requiredComponentBits);
				return this;
			}

			public Builder Include(params ComponentType[] componentTypes)
			{
				ArgumentNullException.ThrowIfNull(componentTypes, nameof(componentTypes));
				ComponentType[] args = componentTypes.Length == 0 ? [] : new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, args, args.Length);
				Array.Sort(args);
				Initialize(args, out m_includedComponentTypes, out m_includedComponentBits);
				return this;
			}

			public Builder Include(IEnumerable<ComponentType> componentTypes)
			{
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_includedComponentTypes, out m_includedComponentBits);
				return this;
			}

			public Builder Include(ReadOnlySpan<ComponentType> componentTypes)
			{
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_includedComponentTypes, out m_includedComponentBits);
				return this;
			}

			public Builder Include(Span<ComponentType> componentTypes)
			{
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_includedComponentTypes, out m_includedComponentBits);
				return this;
			}

			public Builder Exclude(params ComponentType[] componentTypes)
			{
				ArgumentNullException.ThrowIfNull(componentTypes, nameof(componentTypes));
				ComponentType[] args = componentTypes.Length == 0 ? [] : new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, args, args.Length);
				Array.Sort(args);
				Initialize(args, out m_excludedComponentTypes, out m_excludedComponentBits);
				return this;
			}

			public Builder Exclude(IEnumerable<ComponentType> componentTypes)
			{
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_excludedComponentTypes, out m_excludedComponentBits);
				return this;
			}

			public Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
			{
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_excludedComponentTypes, out m_excludedComponentBits);
				return this;
			}

			public Builder Exclude(Span<ComponentType> componentTypes)
			{
				ComponentType[] args = componentTypes.ToArray();
				Array.Sort(args);
				Initialize(args, out m_excludedComponentTypes, out m_excludedComponentBits);
				return this;
			}

			public EntityFilter Build()
			{
				if (m_requiredComponentTypes.Length == 0 &&
					m_includedComponentTypes.Length == 0 &&
					m_excludedComponentTypes.Length == 0)
				{
					return s_universal;
				}

				return new EntityFilter(this);
			}
		}
    }
}
