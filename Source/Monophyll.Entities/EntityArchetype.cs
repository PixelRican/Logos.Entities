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
		private const int DefaultChunkCapacity = 16;

		private readonly ComponentType[] m_componentTypes;
		private readonly uint[] m_componentBits;
		private readonly int m_chunkCapacity;
		private readonly int m_chunkSize;
		private readonly int m_entitySize;
		private readonly int m_storedComponentTypeCount;
		private readonly int m_managedComponentTypeCount;
		private readonly int m_id;

		private EntityArchetype(int targetChunkSize, int id)
		{
			m_componentTypes = Array.Empty<ComponentType>();
			m_componentBits = Array.Empty<uint>();
			m_entitySize = Unsafe.SizeOf<Entity>();
			m_chunkCapacity = Math.Max(targetChunkSize / m_entitySize, DefaultChunkCapacity);
			m_chunkSize = m_chunkCapacity * m_entitySize;
			m_id = id;
		}

		private EntityArchetype(EntityArchetype other, int targetChunkSize, int id)
		{
			m_componentTypes = other.m_componentTypes;
			m_componentBits = other.m_componentBits;
			m_entitySize = other.m_entitySize;
			m_storedComponentTypeCount = other.m_storedComponentTypeCount;
			m_managedComponentTypeCount = other.m_managedComponentTypeCount;
			m_chunkCapacity = Math.Max(targetChunkSize / m_entitySize, DefaultChunkCapacity);
			m_chunkSize = m_chunkCapacity * m_entitySize;
			m_id = id;
		}

		private EntityArchetype(ComponentType[] componentTypes, int targetChunkSize, int id)
		{
			m_componentTypes = componentTypes;
			m_componentBits = new uint[componentTypes[^1].Id + 32 >> 5];
			m_entitySize = Unsafe.SizeOf<Entity>();
			m_id = id;

			int freeIndex = 0;
			ComponentType? componentTypeToCompare = null;

			foreach (ComponentType componentType in componentTypes)
			{
				if (!ComponentType.Equals(componentTypeToCompare, componentType))
				{
					m_componentTypes[freeIndex++] = componentTypeToCompare = componentType;
					m_componentBits[componentType.Id >> 5] |= 1u << componentType.Id;

					if (!componentType.IsEmpty)
					{
						m_entitySize += componentType.Size;
						m_storedComponentTypeCount++;

						if (componentType.IsManaged)
						{
							m_managedComponentTypeCount++;
						}
					}
				}
			}

			Array.Resize(ref m_componentTypes, freeIndex);
			m_chunkCapacity = Math.Max(targetChunkSize / m_entitySize, DefaultChunkCapacity);
			m_chunkSize = m_chunkCapacity * m_entitySize;
		}

		public ImmutableArray<ComponentType> ComponentTypes
		{
			get => ImmutableCollectionsMarshal.AsImmutableArray(m_componentTypes);
		}

		public ImmutableArray<uint> ComponentBits
		{
			get => ImmutableCollectionsMarshal.AsImmutableArray(m_componentBits);
		}

		public int ChunkCapacity
		{
			get => m_chunkCapacity;
		}

		public int ChunkSize
		{
			get => m_chunkSize;
		}

		public int EntitySize
		{
			get => m_entitySize;
		}

		public int StoredComponentTypeCount
		{
			get => m_storedComponentTypeCount;
		}

		public int ManagedComponentTypeCount
		{
			get => m_managedComponentTypeCount;
		}

		public int UnmanagedComponentTypeCount
		{
			get => m_storedComponentTypeCount - m_managedComponentTypeCount;
		}

		public int TaggedComponentTypeCount
		{
			get => m_componentTypes.Length - m_storedComponentTypeCount;
		}

		public int Id
		{
			get => m_id;
		}

		public static EntityArchetype Create(int targetChunkSize, int id)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(targetChunkSize);

			return new EntityArchetype(targetChunkSize, id);
		}

		public static EntityArchetype Create(ComponentType[] componentTypes, int targetChunkSize, int id)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(targetChunkSize);
			ArgumentNullException.ThrowIfNull(componentTypes);

			if (componentTypes.Length > 0)
			{
				ComponentType[] array = new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, array, componentTypes.Length);
				Array.Sort(array);

				if (array[^1] != null)
				{
					return new EntityArchetype(array, targetChunkSize, id);
				}
			}

			return new EntityArchetype(targetChunkSize, id);
		}

		public static EntityArchetype Create(IEnumerable<ComponentType> componentTypes, int targetChunkSize, int id)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(targetChunkSize);

			ComponentType[] array = componentTypes.ToArray();
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array, targetChunkSize, id);
			}

			return new EntityArchetype(targetChunkSize, id);
		}

		public static EntityArchetype Create(ReadOnlySpan<ComponentType> componentTypes, int targetChunkSize, int id)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(targetChunkSize);

			ComponentType[] array = componentTypes.ToArray();
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array, targetChunkSize, id);
			}

			return new EntityArchetype(targetChunkSize, id);
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
				&& a.m_chunkCapacity == b.m_chunkCapacity
				&& ((ReadOnlySpan<uint>)a.m_componentBits).SequenceEqual(b.m_componentBits);
		}

		public EntityArchetype Clone(int targetChunkSize, int id)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(targetChunkSize);

			return new EntityArchetype(this, targetChunkSize, id);
		}

		public EntityArchetype CloneWith(ComponentType componentType, int targetChunkSize, int id)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(targetChunkSize);
			ArgumentNullException.ThrowIfNull(componentType);

			if (Contains(componentType))
			{
				return new EntityArchetype(this, targetChunkSize, id);
			}

			ComponentType[] destinationComponentTypes = new ComponentType[m_componentTypes.Length + 1];
			int destinationIndex = 1;
			int insertIndex = 0;

			foreach (ComponentType sourceComponentType in m_componentTypes)
			{
				if (ComponentType.Compare(componentType, sourceComponentType) < 0)
				{
					destinationComponentTypes[destinationIndex++] = sourceComponentType;
				}
				else
				{
					destinationComponentTypes[insertIndex] = sourceComponentType;
					insertIndex = destinationIndex++;
				}
			}

			destinationComponentTypes[insertIndex] = componentType;

			return new EntityArchetype(destinationComponentTypes, targetChunkSize, id);
		}

		public EntityArchetype CloneWithout(ComponentType componentType, int targetChunkSize, int id)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(targetChunkSize);
			ArgumentNullException.ThrowIfNull(componentType);

			if (!Contains(componentType))
			{
				return new EntityArchetype(this, targetChunkSize, id);
			}

			if (m_componentTypes.Length == 1)
			{
				return new EntityArchetype(targetChunkSize, id);
			}

			ComponentType[] destinationComponentTypes = new ComponentType[m_componentTypes.Length - 1];
			int destinationIndex = 0;

			foreach (ComponentType sourceComponentType in m_componentTypes)
			{
				if (!ComponentType.Equals(componentType, sourceComponentType))
				{
					destinationComponentTypes[destinationIndex++] = sourceComponentType;
				}
			}

			return new EntityArchetype(destinationComponentTypes, targetChunkSize, id);
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
			return Compare(this, other);
		}

		public int CompareTo(object? obj)
		{
			if (obj == this)
			{
				return 0;
			}

			if (obj == null)
			{
				return 1;
			}

			if (obj is EntityArchetype other)
			{
				return m_id.CompareTo(other.m_id);
			}

			throw new ArgumentException("obj is not the same type as this instance.");
		}

		public bool Equals([NotNullWhen(true)] EntityArchetype? other)
		{
			return Equals(this, other);
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			return Equals(this, obj as EntityArchetype);
		}

		public override int GetHashCode()
		{
			int result = m_id;

			for (int i = m_componentBits.Length > 8 ? m_componentBits.Length - 8 : 0; i < m_componentBits.Length; i++)
			{
				result = ((result << 5) + result) ^ (int)m_componentBits[i];
			}

			return result;
		}

		public override string ToString()
		{
			return $"EntityArchetype {{ ComponentTypes = [{string.Join(", ", (object[])m_componentTypes)}] Id = {m_id} }}";
		}
	}
}
