using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

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

		public static EntityFilter Create(ComponentType[] requiredComponentTypes, ComponentType[] includedComponentTypes, ComponentType[] excludedComponentTypes)
		{
			ArgumentNullException.ThrowIfNull(requiredComponentTypes);
			ArgumentNullException.ThrowIfNull(includedComponentTypes);
			ArgumentNullException.ThrowIfNull(excludedComponentTypes);

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

			Initialize(ref requiredComponentTypeArray, out uint[] requiredComponentBitArray);
			Initialize(ref includedComponentTypeArray, out uint[] includedComponentBitArray);
			Initialize(ref excludedComponentTypeArray, out uint[] excludedComponentBitArray);

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

			Initialize(ref requiredComponentTypeArray, out uint[] requiredComponentBitArray);
			Initialize(ref includedComponentTypeArray, out uint[] includedComponentBitArray);
			Initialize(ref excludedComponentTypeArray, out uint[] excludedComponentBitArray);

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

			Initialize(ref requiredComponentTypeArray, out uint[] requiredComponentBitArray);
			Initialize(ref includedComponentTypeArray, out uint[] includedComponentBitArray);
			Initialize(ref excludedComponentTypeArray, out uint[] excludedComponentBitArray);

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

		private static void Initialize(ref ComponentType[] componentTypes, out uint[] componentBits)
        {
			Array.Sort(componentTypes);
			ComponentType? componentType;

			if (componentTypes.Length > 0 && (componentType = componentTypes[^1]) != null)
			{
				int freeIndex = 0;
				componentBits = new uint[componentType.Id + 32 >> 5];
				componentType = null;

				for (int i = 0; i < componentTypes.Length; i++)
				{
					ComponentType componentTypeToCompare = componentTypes[i];

					if (componentTypeToCompare != componentType)
					{
						componentTypes[freeIndex++] = componentType = componentTypeToCompare;
						componentBits[componentTypeToCompare.Id >> 5] |= 1u << componentTypeToCompare.Id;
					}
				}

				Array.Resize(ref componentTypes, freeIndex);
			}
			else
			{
				componentTypes = Array.Empty<ComponentType>();
				componentBits = Array.Empty<uint>();
			}
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
				m_requiredComponentTypes = Array.Empty<ComponentType>();
				m_includedComponentTypes = Array.Empty<ComponentType>();
				m_excludedComponentTypes = Array.Empty<ComponentType>();
				m_requiredComponentBits = Array.Empty<uint>();
				m_includedComponentBits = Array.Empty<uint>();
				m_excludedComponentBits = Array.Empty<uint>();
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
				m_requiredComponentTypes = Array.Empty<ComponentType>();
				m_includedComponentTypes = Array.Empty<ComponentType>();
				m_excludedComponentTypes = Array.Empty<ComponentType>();
				m_requiredComponentBits = Array.Empty<uint>();
				m_includedComponentBits = Array.Empty<uint>();
				m_excludedComponentBits = Array.Empty<uint>();
			}

            public Builder Require(params ComponentType[] componentTypes)
            {
				ArgumentNullException.ThrowIfNull(componentTypes);
				m_requiredComponentTypes = componentTypes.Length == 0 ?
										   Array.Empty<ComponentType>() :
										   new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, m_requiredComponentTypes, m_requiredComponentTypes.Length);
				Initialize(ref m_requiredComponentTypes, out m_requiredComponentBits);
                return this;
			}

            public Builder Require(IEnumerable<ComponentType> componentTypes)
            {
				m_requiredComponentTypes = componentTypes.ToArray();
				Initialize(ref m_requiredComponentTypes, out m_requiredComponentBits);
                return this;
			}

            public Builder Require(ReadOnlySpan<ComponentType> componentTypes)
			{
				m_requiredComponentTypes = componentTypes.ToArray();
				Initialize(ref m_requiredComponentTypes, out m_requiredComponentBits);
				return this;
			}

			public Builder Include(params ComponentType[] componentTypes)
			{
				ArgumentNullException.ThrowIfNull(componentTypes);
				m_includedComponentTypes = componentTypes.Length == 0 ?
										   Array.Empty<ComponentType>() :
										   new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, m_includedComponentTypes, m_includedComponentTypes.Length);
				Initialize(ref m_includedComponentTypes, out m_includedComponentBits);
				return this;
			}

			public Builder Include(IEnumerable<ComponentType> componentTypes)
			{
				m_includedComponentTypes = componentTypes.ToArray();
				Initialize(ref m_includedComponentTypes, out m_includedComponentBits);
				return this;
			}

			public Builder Include(ReadOnlySpan<ComponentType> componentTypes)
			{
				m_includedComponentTypes = componentTypes.ToArray();
				Initialize(ref m_includedComponentTypes, out m_includedComponentBits);
				return this;
			}

			public Builder Exclude(params ComponentType[] componentTypes)
			{
				ArgumentNullException.ThrowIfNull(componentTypes);
				m_excludedComponentTypes = componentTypes.Length == 0 ?
										   Array.Empty<ComponentType>() :
										   new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, m_excludedComponentTypes, m_excludedComponentTypes.Length);
				Initialize(ref m_excludedComponentTypes, out m_excludedComponentBits);
				return this;
			}

			public Builder Exclude(IEnumerable<ComponentType> componentTypes)
			{
				m_excludedComponentTypes = componentTypes.ToArray();
				Initialize(ref m_excludedComponentTypes, out m_excludedComponentBits);
				return this;
			}

			public Builder Exclude(ReadOnlySpan<ComponentType> componentTypes)
			{
				m_excludedComponentTypes = componentTypes.ToArray();
				Initialize(ref m_excludedComponentTypes, out m_excludedComponentBits);
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
