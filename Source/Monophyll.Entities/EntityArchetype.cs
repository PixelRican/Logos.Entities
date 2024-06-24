using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Monophyll.Entities
{
	public sealed class EntityArchetype : IEquatable<EntityArchetype>, IComparable<EntityArchetype>, IComparable
	{
		private const int MinChunkCapacity = 100;
		private const int TargetChunkByteSize = 16000;

		private static readonly EntityArchetype s_base = new();

		private readonly ImmutableArray<ComponentType> m_componentTypes;
		private readonly ImmutableArray<uint> m_componentBits;
		private readonly ImmutableArray<int> m_componentLookup;
		private readonly int m_entityByteSize;
		private readonly int m_sequenceNumber;

		public EntityArchetype()
		{
			m_componentTypes = [];
			m_componentBits = [];
			m_componentLookup = [];
			m_entityByteSize = Unsafe.SizeOf<Entity>();
		}

		public EntityArchetype(EntityArchetype other)
		{
			ArgumentNullException.ThrowIfNull(other, nameof(other));
			m_componentTypes = other.m_componentTypes;
			m_componentBits = other.m_componentBits;
			m_componentLookup = other.m_componentLookup;
			m_entityByteSize = other.m_entityByteSize;
			m_sequenceNumber = other.m_sequenceNumber;
		}

		public EntityArchetype(params ComponentType[] componentTypes)
		{
			ArgumentNullException.ThrowIfNull(componentTypes, nameof(componentTypes));
			ComponentType[] componentTypeBuilder = componentTypes.Length == 0 ? [] : new ComponentType[componentTypes.Length];
			Array.Copy(componentTypes, componentTypeBuilder, componentTypeBuilder.Length);
			Array.Sort(componentTypeBuilder);
			Initialize(componentTypeBuilder, out m_componentTypes, out m_componentBits, out m_componentLookup, out m_entityByteSize);
		}

		public EntityArchetype(IEnumerable<ComponentType> componentTypes)
		{
			ComponentType[] componentTypeBuilder = componentTypes.ToArray();
			Array.Sort(componentTypeBuilder);
			Initialize(componentTypeBuilder, out m_componentTypes, out m_componentBits, out m_componentLookup, out m_entityByteSize);
		}

		public EntityArchetype(ReadOnlySpan<ComponentType> componentTypes)
		{
			ComponentType[] componentTypeBuilder = componentTypes.ToArray();
			Array.Sort(componentTypeBuilder);
			Initialize(componentTypeBuilder, out m_componentTypes, out m_componentBits, out m_componentLookup, out m_entityByteSize);
		}

		public EntityArchetype(Span<ComponentType> componentTypes)
		{
			ComponentType[] componentTypeBuilder = componentTypes.ToArray();
			Array.Sort(componentTypeBuilder);
			Initialize(componentTypeBuilder, out m_componentTypes, out m_componentBits, out m_componentLookup, out m_entityByteSize);
		}

		public static EntityArchetype Base
		{
			get => s_base;
		}

		public ImmutableArray<ComponentType> ComponentTypes
		{
			get => m_componentTypes;
		}

		public ImmutableArray<uint> ComponentBits
		{
			get => m_componentBits;
		}

		public ImmutableArray<int> ComponentLookup
		{
			get => m_componentLookup;
		}

		public int ChunkCapacity
		{
			get => Math.Max(TargetChunkByteSize / m_entityByteSize, MinChunkCapacity);
		}

		public int EntityByteSize
		{
			get => m_entityByteSize;
		}

		public int SequenceNumber
		{
			get => m_sequenceNumber;
			init => m_sequenceNumber = value;
		}

		private static void Initialize(ComponentType[] componentTypeBuilder, out ImmutableArray<ComponentType> componentTypes,
			out ImmutableArray<uint> componentBits, out ImmutableArray<int> componentLookup, out int entityByteSize)
		{
			ComponentType? componentTypeToCompare;
			entityByteSize = Unsafe.SizeOf<Entity>();

			if (componentTypeBuilder.Length == 0 || (componentTypeToCompare = componentTypeBuilder[^1]) == null)
			{
				componentTypes = [];
				componentBits = [];
				componentLookup = [];
				return;
			}

			uint[] componentBitBuilder = new uint[componentTypeToCompare.SequenceNumber + 32 >> 5];
			int[] componentLookupBuilder = new int[componentTypeToCompare.SequenceNumber + 1];
			int freeIndex = 0;
			componentTypeToCompare = null;

			for (int i = 0; i < componentTypeBuilder.Length; i++)
			{
				ComponentType currentComponentType = componentTypeBuilder[i];

				if (currentComponentType != componentTypeToCompare)
				{
					int componentTypeSequenceNumber = currentComponentType.SequenceNumber;
					componentTypeBuilder[freeIndex] = componentTypeToCompare = currentComponentType;
					componentBitBuilder[componentTypeSequenceNumber >> 5] |= 1u << componentTypeSequenceNumber;
					componentLookupBuilder[componentTypeSequenceNumber] = ~freeIndex++;
					entityByteSize += currentComponentType.ByteSize;
				}
			}

			Array.Resize(ref componentTypeBuilder, freeIndex);
			componentTypes = ImmutableCollectionsMarshal.AsImmutableArray(componentTypeBuilder);
			componentBits = ImmutableCollectionsMarshal.AsImmutableArray(componentBitBuilder);
			componentLookup = ImmutableCollectionsMarshal.AsImmutableArray(componentLookupBuilder);
		}

		public bool Contains(ComponentType componentType)
		{
			return componentType != null
				&& m_componentTypes.Length > 0
				&& m_componentTypes[^1].SequenceNumber >= componentType.SequenceNumber
				&& (m_componentBits[componentType.SequenceNumber >> 5] & (1u << componentType.SequenceNumber)) != 0u;
		}

		public int IndexOf(ComponentType componentType)
		{
			if (componentType == null || componentType.SequenceNumber >= m_componentLookup.Length)
			{
				return -1;
			}

			return ~m_componentLookup[componentType.SequenceNumber];
		}

		public int CompareTo(EntityArchetype? other)
		{
			if (other is null)
			{
				return 1;
			}

			return m_sequenceNumber.CompareTo(other.m_sequenceNumber);
		}

		public int CompareTo(object? obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (obj is not EntityArchetype other)
			{
				throw new ArgumentException("obj is not the same type as this instance.");
			}

			return m_sequenceNumber.CompareTo(other.m_sequenceNumber);
		}

		public bool Equals(EntityArchetype? other)
		{
			return ReferenceEquals(this, other)
				|| other is not null
				&& m_sequenceNumber == other.m_sequenceNumber
				&& m_componentBits.SequenceEqual(other.m_componentBits);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as EntityArchetype);
		}

		public override int GetHashCode()
		{
			ImmutableArray<uint> componentBits = m_componentBits;
			HashCode hashCode = new();

			for (int i = 0; i < componentBits.Length; i++)
			{
				hashCode.Add(componentBits[i]);
			}

			return HashCode.Combine(m_sequenceNumber, hashCode.ToHashCode());
		}

		public override string ToString()
		{
			StringBuilder builder = new("EntityArchetype { ComponentTypes = [");

			if (m_componentTypes.Length > 0)
			{
				builder.Append(m_componentTypes[0]);

				for (int i = 1; i < m_componentTypes.Length; i++)
				{
					builder.Append($", {m_componentTypes[i]}");
				}
			}

			builder.Append($"] SequenceNumber = {m_sequenceNumber} }}");
			return builder.ToString();
		}
	}
}
