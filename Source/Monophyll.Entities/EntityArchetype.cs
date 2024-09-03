using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monophyll.Entities
{
	public sealed class EntityArchetype : IEquatable<EntityArchetype>, IComparable<EntityArchetype>, IComparable
	{
		private const int TargetChunkByteSize = 16 * 1024;
		private const int MinChunkCapacity = 100;

		private static readonly EntityArchetype s_base = new EntityArchetype();

		private readonly ComponentType[] m_componentTypes;
		private readonly uint[] m_componentBits;
		private readonly int[] m_componentOffsets;
		private readonly int m_chunkByteSize;
		private readonly int m_chunkCapacity;
		private readonly int m_entityByteSize;
		private readonly int m_id;

		private EntityArchetype()
		{
			m_componentTypes = Array.Empty<ComponentType>();
			m_componentBits = Array.Empty<uint>();
			m_componentOffsets = Array.Empty<int>();
			m_entityByteSize = Unsafe.SizeOf<Entity>();
			m_chunkByteSize = TargetChunkByteSize;
			m_chunkCapacity = Math.Max(TargetChunkByteSize / m_entityByteSize, MinChunkCapacity);
			m_id = 0;
		}

		private EntityArchetype(EntityArchetype other, int id)
		{
			m_componentTypes = other.m_componentTypes;
			m_componentBits = other.m_componentBits;
			m_componentOffsets = other.m_componentOffsets;
			m_chunkByteSize = other.m_chunkByteSize;
			m_chunkCapacity = other.m_chunkCapacity;
			m_entityByteSize = other.m_entityByteSize;
			m_id = id;
		}

		private EntityArchetype(ComponentType[] componentTypes, int id)
		{
			m_componentTypes = componentTypes;
			m_componentBits = new uint[componentTypes[^1].Id + 32 >> 5];
			m_entityByteSize = Unsafe.SizeOf<Entity>();
			m_id = id;

			ComponentType? componentType = null;
			int freeIndex = 0;

			for (int i = 0; i < m_componentTypes.Length; i++)
			{
				ComponentType? componentTypeToCompare = m_componentTypes[i];

				if (!ComponentType.Equals(componentType, componentTypeToCompare))
				{
					m_componentTypes[freeIndex++] = componentType = componentTypeToCompare;
					m_componentBits[componentType.Id >> 5] |= 1u << componentType.Id;
					m_entityByteSize += componentType.ByteSize;
				}
			}

			Array.Resize(ref m_componentTypes, freeIndex);

			m_componentOffsets = new int[freeIndex];
			m_chunkCapacity = Math.Max(TargetChunkByteSize / m_entityByteSize, MinChunkCapacity);
			m_chunkByteSize = m_chunkCapacity * m_entityByteSize;
			freeIndex = m_chunkCapacity * Unsafe.SizeOf<Entity>();

			for (int i = 0; i < m_componentTypes.Length; i++)
			{
				m_componentOffsets[i] = freeIndex;
				freeIndex += m_chunkCapacity * m_componentTypes[i].ByteSize;
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
			get => m_chunkByteSize;
		}

		public int ChunkCapacity
		{
			get => m_chunkCapacity;
		}

		public int EntityByteSize
		{
			get => m_entityByteSize;
		}

		public int Id
		{
			get => m_id;
		}

		public static EntityArchetype Create(ComponentType[] componentTypes, int id = 0)
		{
			ArgumentNullException.ThrowIfNull(componentTypes);
			ComponentType[] array = componentTypes.Length == 0 ?
									Array.Empty<ComponentType>() :
									new ComponentType[componentTypes.Length];
			Array.Copy(componentTypes, array, array.Length);
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array, id);
			}

			if (id != 0)
			{
				return new EntityArchetype(s_base, id);
			}

			return s_base;
		}

		public static EntityArchetype Create(IEnumerable<ComponentType> componentTypes, int id = 0)
		{
			ComponentType[] array = componentTypes.ToArray();
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array, id);
			}

			if (id != 0)
			{
				return new EntityArchetype(s_base, id);
			}

			return s_base;
		}

		public static EntityArchetype Create(ReadOnlySpan<ComponentType> componentTypes, int id = 0)
		{
			ComponentType[] array = componentTypes.ToArray();
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array, id);
			}

			if (id != 0)
			{
				return new EntityArchetype(s_base, id);
			}

			return s_base;
		}

		public static int Compare(EntityArchetype? a, EntityArchetype? b)
		{
			if (a == b)
			{
				return 0;
			}

			if (a == null)
			{
				return -1;
			}

			if (b == null)
			{
				return 1;
			}

			return a.m_id.CompareTo(b.m_id);
		}

		public static bool Equals(EntityArchetype? a, EntityArchetype? b)
		{
			return a == b
				|| a != null
				&& b != null
				&& a.m_id == b.m_id
				&& ((ReadOnlySpan<uint>)a.m_componentBits).SequenceEqual(b.m_componentBits);
		}

		public EntityArchetype Add(ComponentType componentType, int id = 0)
		{
			ArgumentNullException.ThrowIfNull(componentType);

			if (Contains(componentType))
			{
				if (m_id != id)
				{
					return new EntityArchetype(this, id);
				}

				return this;
			}

			ComponentType[] destinationComponentTypes = new ComponentType[m_componentTypes.Length + 1];
			int destinationIndex = 0;
			int sourceIndex = 0;

			while (sourceIndex < m_componentTypes.Length)
			{
				ComponentType currentComponentType = m_componentTypes[sourceIndex++];

				if (ComponentType.Compare(componentType, currentComponentType) < 0)
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

			return new EntityArchetype(destinationComponentTypes, id);
		}

		public EntityArchetype Clone(int id = 0)
		{
			if (m_id == id)
			{
				return this;
			}

			return new EntityArchetype(this, id);
		}

		public bool Contains(ComponentType componentType)
		{
			int index;
			return componentType != null
				&& (index = componentType.Id >> 5) < m_componentBits.Length
				&& (m_componentBits[index] & 1u << componentType.Id) != 0;
		}

		public int CompareTo(EntityArchetype? other)
		{
			if (other == null)
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

		public bool Equals([NotNullWhen(true)] EntityArchetype? other)
		{
			return other == this
				|| other != null
				&& m_id == other.m_id
				&& ((ReadOnlySpan<uint>)m_componentBits).SequenceEqual(other.m_componentBits);
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
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

		public EntityArchetype Remove(ComponentType componentType, int id = 0)
		{
			ArgumentNullException.ThrowIfNull(componentType);

			if (!Contains(componentType))
			{
				if (m_id != id)
				{
					return new EntityArchetype(this, id);
				}

				return this;
			}

			if (m_componentTypes.Length == 1)
			{
				if (id != 0)
				{
					return new EntityArchetype(s_base, id);
				}

				return s_base;
			}

			ComponentType[] destinationComponentTypes = new ComponentType[m_componentTypes.Length - 1];
			int destinationIndex = 0;

			for (int i = 0; i < m_componentTypes.Length; i++)
			{
				ComponentType currentComponentType = m_componentTypes[i];

				if (!ComponentType.Equals(componentType, currentComponentType))
				{
					destinationComponentTypes[destinationIndex++] = currentComponentType;
				}
			}

			return new EntityArchetype(destinationComponentTypes, id);
		}

		public override string ToString()
		{
			return $"EntityArchetype {{ ComponentTypes = [{string.Join(", ", (object[])m_componentTypes)}] Id = {m_id} }}";
		}
	}
}
