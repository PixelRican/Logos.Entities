using Monophyll.Entities.Utilities;
using System;
using System.Collections.Generic;
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
            get => new ReadOnlySpan<ComponentType>(m_requiredComponentTypes);
        }

        public ReadOnlySpan<ComponentType> IncludedComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_includedComponentTypes);
        }

        public ReadOnlySpan<ComponentType> ExcludedComponentTypes
        {
            get => new ReadOnlySpan<ComponentType>(m_excludedComponentTypes);
        }

        public ReadOnlySpan<uint> RequiredComponentBits
        {
            get => new ReadOnlySpan<uint>(m_requiredComponentBits);
        }

        public ReadOnlySpan<uint> IncludedComponentBits
        {
            get => new ReadOnlySpan<uint>(m_includedComponentBits);
        }

        public ReadOnlySpan<uint> ExcludedComponentBits
        {
            get => new ReadOnlySpan<uint>(m_excludedComponentBits);
        }

		public static EntityFilter Create(ComponentType[] requiredComponentTypes, ComponentType[] includedComponentTypes, ComponentType[] excludedComponentTypes)
		{
			if (requiredComponentTypes == null)
			{
				throw new ArgumentNullException(nameof(requiredComponentTypes));
			}

			if (includedComponentTypes == null)
			{
				throw new ArgumentNullException(nameof(includedComponentTypes));
			}

			if (excludedComponentTypes == null)
			{
				throw new ArgumentNullException(nameof(excludedComponentTypes));
			}

			ComponentType[] requiredComponentTypeArray = requiredComponentTypes.Length == 0 ?
														 Array.Empty<ComponentType>() :
														 new ComponentType[requiredComponentTypes.Length];
			Array.Copy(requiredComponentTypes, requiredComponentTypeArray, requiredComponentTypeArray.Length);

			ComponentType[] includedComponentTypeArray = includedComponentTypes.Length == 0 ?
														 Array.Empty<ComponentType>() :
														 new ComponentType[includedComponentTypes.Length];
			Array.Copy(includedComponentTypes, includedComponentTypeArray, includedComponentTypeArray.Length);

			ComponentType[] excludedComponentTypeArray = excludedComponentTypes.Length == 0 ?
														 Array.Empty<ComponentType>() :
														 new ComponentType[excludedComponentTypes.Length];
			Array.Copy(excludedComponentTypes, excludedComponentTypeArray, excludedComponentTypeArray.Length);

			uint[] requiredComponentBitArray = ResolveComponentTypes(ref requiredComponentTypeArray);
			uint[] includedComponentBitArray = ResolveComponentTypes(ref includedComponentTypeArray);
			uint[] excludedComponentBitArray = ResolveComponentTypes(ref excludedComponentTypeArray);

			if (requiredComponentTypeArray.Length > 0 ||
				includedComponentTypeArray.Length > 0 ||
				excludedComponentTypeArray.Length > 0)
			{
				return new EntityFilter(requiredComponentTypeArray, requiredComponentBitArray,
										includedComponentTypeArray, includedComponentBitArray,
										excludedComponentTypeArray, excludedComponentBitArray);
			}

			return s_universal;
		}

		public static EntityFilter Create(IEnumerable<ComponentType> requiredComponentTypes, IEnumerable<ComponentType> includedComponentTypes, IEnumerable<ComponentType> excludedComponentTypes)
		{
			ComponentType[] requiredComponentTypeArray = requiredComponentTypes.ToArray();
			ComponentType[] includedComponentTypeArray = includedComponentTypes.ToArray();
			ComponentType[] excludedComponentTypeArray = excludedComponentTypes.ToArray();

			uint[] requiredComponentBitArray = ResolveComponentTypes(ref requiredComponentTypeArray);
			uint[] includedComponentBitArray = ResolveComponentTypes(ref includedComponentTypeArray);
			uint[] excludedComponentBitArray = ResolveComponentTypes(ref excludedComponentTypeArray);

			if (requiredComponentTypeArray.Length > 0 ||
				includedComponentTypeArray.Length > 0 ||
				excludedComponentTypeArray.Length > 0)
			{
				return new EntityFilter(requiredComponentTypeArray, requiredComponentBitArray,
										includedComponentTypeArray, includedComponentBitArray,
										excludedComponentTypeArray, excludedComponentBitArray);
			}

			return s_universal;
		}

		public static EntityFilter Create(ReadOnlySpan<ComponentType> requiredComponentTypes, ReadOnlySpan<ComponentType> includedComponentTypes, ReadOnlySpan<ComponentType> excludedComponentTypes)
		{
			ComponentType[] requiredComponentTypeArray = requiredComponentTypes.ToArray();
			ComponentType[] includedComponentTypeArray = includedComponentTypes.ToArray();
			ComponentType[] excludedComponentTypeArray = excludedComponentTypes.ToArray();

			uint[] requiredComponentBitArray = ResolveComponentTypes(ref requiredComponentTypeArray);
			uint[] includedComponentBitArray = ResolveComponentTypes(ref includedComponentTypeArray);
			uint[] excludedComponentBitArray = ResolveComponentTypes(ref excludedComponentTypeArray);

			if (requiredComponentTypeArray.Length > 0 ||
				includedComponentTypeArray.Length > 0 ||
				excludedComponentTypeArray.Length > 0)
			{
				return new EntityFilter(requiredComponentTypeArray, requiredComponentBitArray,
										includedComponentTypeArray, includedComponentBitArray,
										excludedComponentTypeArray, excludedComponentBitArray);
			}

			return s_universal;
		}

		private static uint[] ResolveComponentTypes(ref ComponentType[] componentTypes)
        {
			Array.Sort(componentTypes);

			if (componentTypes.Length == 0 || componentTypes[^1] == null)
			{
				componentTypes = Array.Empty<ComponentType>();
				return Array.Empty<uint>();
			}

			uint[] componentBits = new uint[componentTypes[^1].Id + 32 >> 5];
			int freeIndex = 0;
			ComponentType? previousComponentType = null;

			foreach (ComponentType currentComponentType in componentTypes)
			{
				if (!ComponentType.Equals(previousComponentType, currentComponentType))
				{
					componentTypes[freeIndex++] = previousComponentType = currentComponentType;
					componentBits[currentComponentType.Id >> 5] |= 1u << currentComponentType.Id;
				}
			}

			Array.Resize(ref componentTypes, freeIndex);
			return componentBits;
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
				&& MemoryExtensions.SequenceEqual<uint>(a.m_requiredComponentBits, b.m_requiredComponentBits)
				&& MemoryExtensions.SequenceEqual<uint>(a.m_includedComponentBits, b.m_includedComponentBits)
				&& MemoryExtensions.SequenceEqual<uint>(a.m_excludedComponentBits, b.m_excludedComponentBits);
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
			return HashCode.Combine(BitSetOperations.GetHashCode(m_requiredComponentBits),
									BitSetOperations.GetHashCode(m_includedComponentBits),
									BitSetOperations.GetHashCode(m_excludedComponentBits));
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
            ReadOnlySpan<uint> componentBits;
			return archetype != null
				&& BitSetOperations.IsSubsetOf(m_requiredComponentBits, componentBits = archetype.ComponentBits)
				&& (m_includedComponentBits.Length == 0 || BitSetOperations.Overlaps(m_includedComponentBits, componentBits))
				&& !BitSetOperations.Overlaps(m_excludedComponentBits, componentBits);
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
				m_requiredComponentTypes = Array.Empty<ComponentType>();
				m_includedComponentTypes = Array.Empty<ComponentType>();
				m_excludedComponentTypes = Array.Empty<ComponentType>();
				m_requiredComponentBits = Array.Empty<uint>();
				m_includedComponentBits = Array.Empty<uint>();
				m_excludedComponentBits = Array.Empty<uint>();
			}

			public Builder(EntityFilter filter)
			{
				if (filter == null)
				{
					throw new ArgumentNullException(nameof(filter));
				}

				m_requiredComponentTypes = filter.m_requiredComponentTypes;
				m_includedComponentTypes = filter.m_includedComponentTypes;
				m_excludedComponentTypes = filter.m_excludedComponentTypes;
				m_requiredComponentBits = filter.m_requiredComponentBits;
				m_includedComponentBits = filter.m_includedComponentBits;
				m_excludedComponentBits = filter.m_excludedComponentBits;
			}

			public ReadOnlySpan<ComponentType> RequiredComponentTypes
			{
				get => new ReadOnlySpan<ComponentType>(m_requiredComponentTypes);
			}

			public ReadOnlySpan<ComponentType> IncludedComponentTypes
			{
				get => new ReadOnlySpan<ComponentType>(m_includedComponentTypes);
			}

			public ReadOnlySpan<ComponentType> ExcludedComponentTypes
			{
				get => new ReadOnlySpan<ComponentType>(m_excludedComponentTypes);
			}

			public ReadOnlySpan<uint> RequiredComponentBits
			{
				get => new ReadOnlySpan<uint>(m_requiredComponentBits);
			}

			public ReadOnlySpan<uint> IncludedComponentBits
			{
				get => new ReadOnlySpan<uint>(m_includedComponentBits);
			}

			public ReadOnlySpan<uint> ExcludedComponentBits
			{
				get => new ReadOnlySpan<uint>(m_excludedComponentBits);
			}

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
				if (componentTypes == null)
				{
					throw new ArgumentNullException(nameof(componentTypes));
				}

				m_requiredComponentTypes = componentTypes.Length == 0 ?
										   Array.Empty<ComponentType>() :
										   new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, m_requiredComponentTypes, m_requiredComponentTypes.Length);
				m_requiredComponentBits = ResolveComponentTypes(ref m_requiredComponentTypes);
                return this;
			}

            public Builder Require(IEnumerable<ComponentType> componentTypes)
            {
				m_requiredComponentTypes = componentTypes.ToArray();
				m_requiredComponentBits = ResolveComponentTypes(ref m_requiredComponentTypes);
                return this;
			}

            public Builder Require(ReadOnlySpan<ComponentType> componentTypes)
			{
				m_requiredComponentTypes = componentTypes.ToArray();
				m_requiredComponentBits = ResolveComponentTypes(ref m_requiredComponentTypes);
				return this;
			}

			public Builder Include(ComponentType[] componentTypes)
			{
				if (componentTypes == null)
				{
					throw new ArgumentNullException(nameof(componentTypes));
				}

				m_includedComponentTypes = componentTypes.Length == 0 ?
										   Array.Empty<ComponentType>() :
										   new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, m_includedComponentTypes, m_includedComponentTypes.Length);
				m_includedComponentBits = ResolveComponentTypes(ref m_includedComponentTypes);
				return this;
			}

			public Builder Include(IEnumerable<ComponentType> componentTypes)
			{
				m_includedComponentTypes = componentTypes.ToArray();
				m_includedComponentBits = ResolveComponentTypes(ref m_includedComponentTypes);
				return this;
			}

			public Builder Include(ReadOnlySpan<ComponentType> componentTypes)
			{
				m_includedComponentTypes = componentTypes.ToArray();
				m_includedComponentBits = ResolveComponentTypes(ref m_includedComponentTypes);
				return this;
			}

			public Builder Exclude(ComponentType[] componentTypes)
			{
				if (componentTypes == null)
				{
					throw new ArgumentNullException(nameof(componentTypes));
				}

				m_excludedComponentTypes = componentTypes.Length == 0 ?
										   Array.Empty<ComponentType>() :
										   new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, m_excludedComponentTypes, m_excludedComponentTypes.Length);
				m_excludedComponentBits = ResolveComponentTypes(ref m_excludedComponentTypes);
				return this;
			}

			public Builder Exclude(IEnumerable<ComponentType> componentTypes)
			{
				m_excludedComponentTypes = componentTypes.ToArray();
				m_excludedComponentBits = ResolveComponentTypes(ref m_excludedComponentTypes);
				return this;
			}

			public Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
			{
				m_excludedComponentTypes = componentTypes.ToArray();
				m_excludedComponentBits = ResolveComponentTypes(ref m_excludedComponentTypes);
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
