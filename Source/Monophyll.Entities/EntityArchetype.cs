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
		private const int TargetChunkByteSize = 16 * 1024;

		private static readonly EntityArchetype s_base = new();

		private readonly ComponentType[] m_componentTypes;
		private readonly uint[] m_componentBits;
		private readonly int[] m_componentOffsets;
		private readonly int m_entityByteSize;
		private readonly int m_id;

		public EntityArchetype()
		{
			m_componentTypes = [];
			m_componentBits = [];
			m_componentOffsets = [];
			m_entityByteSize = Unsafe.SizeOf<Entity>();
		}

		public EntityArchetype(EntityArchetype other)
		{
			ArgumentNullException.ThrowIfNull(other);
			m_componentTypes = other.m_componentTypes;
			m_componentBits = other.m_componentBits;
			m_componentOffsets = other.m_componentOffsets;
			m_entityByteSize = other.m_entityByteSize;
			m_id = other.m_id;
		}

		public EntityArchetype(params ComponentType[] componentTypes)
			: this(CloneComponentTypeArray(componentTypes), true)
		{
		}

		public EntityArchetype(IEnumerable<ComponentType> componentTypes)
			: this(componentTypes.ToArray(), true)
		{
		}

		public EntityArchetype(ReadOnlySpan<ComponentType> componentTypes)
			: this(componentTypes.ToArray(), true)
		{
		}

		public EntityArchetype(Span<ComponentType> componentTypes)
			: this(componentTypes.ToArray(), true)
		{
		}

		private EntityArchetype(ComponentType[] componentTypes, bool sortArray)
		{
			ComponentType? componentTypeToCompare;
			m_entityByteSize = Unsafe.SizeOf<Entity>();

			if (sortArray)
			{
				Array.Sort(componentTypes);
			}

			if (componentTypes.Length == 0 || (componentTypeToCompare = componentTypes[^1]) == null)
			{
				m_componentTypes = [];
				m_componentBits = [];
				m_componentOffsets = [];
				return;
			}

			m_componentTypes = componentTypes;
			m_componentBits = new uint[componentTypeToCompare.Id + 32 >> 5];
			m_componentOffsets = new int[componentTypeToCompare.Id + 1];
			componentTypeToCompare = null;

			int freeIndex = 0;

			for (int i = 0; i < m_componentTypes.Length; i++)
			{
				ComponentType currentComponentType = m_componentTypes[i];

				if (currentComponentType != componentTypeToCompare)
				{
					m_componentTypes[freeIndex++] = componentTypeToCompare = currentComponentType;
					m_componentBits[currentComponentType.Id >> 5] |= 1u << currentComponentType.Id;
					m_entityByteSize += currentComponentType.ByteSize;
				}
			}

			Array.Resize(ref m_componentTypes, freeIndex);
			int chunkCapacity = Math.Max(TargetChunkByteSize / m_entityByteSize, MinChunkCapacity);
			freeIndex = chunkCapacity * Unsafe.SizeOf<Entity>();

			for (int i = 0; i < m_componentTypes.Length; i++)
			{
				ComponentType componentType = m_componentTypes[i];
				m_componentOffsets[componentType.Id] = freeIndex;
				freeIndex += chunkCapacity * componentType.ByteSize;
			}
		}

		public static EntityArchetype Base
		{
			get => s_base;
		}

		public ImmutableArray<ComponentType> ComponentTypes
		{
			get => ImmutableCollectionsMarshal.AsImmutableArray(m_componentTypes);
		}

		public ImmutableArray<uint> ComponentBits
		{
			get => ImmutableCollectionsMarshal.AsImmutableArray(m_componentBits);
		}

		public ImmutableArray<int> ComponentOffsets
		{
			get => ImmutableCollectionsMarshal.AsImmutableArray(m_componentOffsets);
		}

		public int ChunkByteSize
		{
			get => Math.Max(TargetChunkByteSize / m_entityByteSize, MinChunkCapacity) * m_entityByteSize;
		}

		public int ChunkCapacity
		{
			get => Math.Max(TargetChunkByteSize / m_entityByteSize, MinChunkCapacity);
		}

		public int EntityByteSize
		{
			get => m_entityByteSize;
		}

		public int Id
		{
			get => m_id;
			init => m_id = value;
		}

		private static ComponentType[] CloneComponentTypeArray(ComponentType[] array)
		{
			ArgumentNullException.ThrowIfNull(array);

			if (array.Length == 0)
			{
				return [];
			}

			ComponentType[] clone = new ComponentType[array.Length];
			Array.Copy(array, clone, clone.Length);
			return clone;
		}

		public EntityArchetype Add(ComponentType componentType, int id = 0)
		{
			ArgumentNullException.ThrowIfNull(componentType);

			if (componentType.Id < m_componentOffsets.Length &&
				m_componentOffsets[componentType.Id] != 0)
			{
				if (m_id != id)
				{
					return new EntityArchetype(this) { Id = id };
				}

				return this;
			}

			ComponentType[] destinationComponentTypes = new ComponentType[m_componentTypes.Length + 1];
			int destinationIndex = 0;
			int sourceIndex = 0;

			while (sourceIndex < m_componentTypes.Length)
			{
				ComponentType currentComponentType = m_componentTypes[sourceIndex++];

				if (componentType.Id < currentComponentType.Id)
				{
					break;
				}

				destinationComponentTypes[destinationIndex++] = currentComponentType;
			}

			destinationComponentTypes[destinationIndex++] = componentType;

			while (sourceIndex < m_componentTypes.Length)
			{
				destinationComponentTypes[destinationIndex++] = m_componentTypes[sourceIndex++];
			}

			return new EntityArchetype(destinationComponentTypes, false) { Id = id };
		}

		public EntityArchetype Remove(ComponentType componentType, int id = 0)
		{
			ArgumentNullException.ThrowIfNull(componentType);

			if (componentType.Id >= m_componentOffsets.Length ||
				m_componentOffsets[componentType.Id] == 0)
			{
				if (m_id != id)
				{
					return new EntityArchetype(this) { Id = id };
				}

				return this;
			}

			if (m_componentTypes.Length == 1)
			{
				if (id == 0)
				{
					return s_base;
				}

				return new EntityArchetype() { Id = id };
			}

			ComponentType[] destinationComponentTypes = new ComponentType[m_componentTypes.Length - 1];
			int destinationIndex = 0;

			for (int i = 0; i < m_componentTypes.Length; i++)
			{
				ComponentType currentComponentType = m_componentTypes[i];

				if (currentComponentType != componentType)
				{
					destinationComponentTypes[destinationIndex++] = currentComponentType;
				}
			}

			return new EntityArchetype(destinationComponentTypes, false) { Id = id };
		}

		public bool Contains(ComponentType componentType)
		{
			return componentType != null
				&& componentType.Id < m_componentOffsets.Length
				&& m_componentOffsets[componentType.Id] != 0;
		}

		public int CompareTo(EntityArchetype? other)
		{
			if (other is null)
			{
				return 1;
			}

			return m_id.CompareTo(other.m_id);
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

			return m_id.CompareTo(other.m_id);
		}

		public bool Equals(EntityArchetype? other)
		{
			return other == this
				|| other != null
				&& m_id == other.m_id
				&& ((ReadOnlySpan<uint>)m_componentBits).SequenceEqual(other.m_componentBits);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as EntityArchetype);
		}

		public override int GetHashCode()
		{
			HashCode hashCode = default;
			hashCode.Add(m_id);

			for (int i = 0; i < m_componentBits.Length; i++)
			{
				hashCode.Add(m_componentBits[i]);
			}

			return hashCode.ToHashCode();
		}

		public override string ToString()
		{
			StringBuilder builder = new($"EntityArchetype {{ Id = {m_id} ComponentTypes = [");

			if (m_componentTypes.Length > 0)
			{
				builder.Append(m_componentTypes[0]);

				for (int i = 1; i < m_componentTypes.Length; i++)
				{
					builder.Append($", {m_componentTypes[i]}");
				}
			}

			builder.Append("] }");
			return builder.ToString();
		}
	}
}
