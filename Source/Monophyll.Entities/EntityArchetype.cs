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
		private readonly ComponentType[] m_componentTypes;
		private readonly uint[] m_componentBits;
		private readonly int m_storedComponentTypeCount;
		private readonly int m_managedComponentTypeCount;
		private readonly int m_entitySize;
		private readonly int m_id;

		private EntityArchetype(int id)
		{
			m_componentTypes = Array.Empty<ComponentType>();
			m_componentBits = Array.Empty<uint>();
			m_entitySize = Unsafe.SizeOf<Entity>();
			m_id = id;
		}

		private EntityArchetype(EntityArchetype other, int id)
		{
			m_componentTypes = other.m_componentTypes;
			m_componentBits = other.m_componentBits;
			m_storedComponentTypeCount = other.m_storedComponentTypeCount;
			m_managedComponentTypeCount = other.m_managedComponentTypeCount;
			m_entitySize = other.m_entitySize;
			m_id = id;
		}

		private EntityArchetype(ComponentType[] componentTypes, int id)
		{
			m_componentTypes = componentTypes;
			m_componentBits = new uint[componentTypes[^1].Id + 32 >> 5];
			m_entitySize = Unsafe.SizeOf<Entity>();
			m_id = id;

			int freeIndex = 0;
			ComponentType? previousComponentType = null;

			foreach (ComponentType currentComponentType in componentTypes)
			{
				if (!ComponentType.Equals(previousComponentType, currentComponentType))
				{
					m_componentTypes[freeIndex++] = previousComponentType = currentComponentType;
					m_componentBits[currentComponentType.Id >> 5] |= 1u << currentComponentType.Id;

					if (!currentComponentType.IsTag)
					{
						m_entitySize += currentComponentType.Size;
						m_storedComponentTypeCount++;

						if (currentComponentType.IsManaged)
						{
							m_managedComponentTypeCount++;
						}
					}
				}
			}

			Array.Resize(ref m_componentTypes, freeIndex);
		}

		public ImmutableArray<ComponentType> ComponentTypes
		{
			get => ImmutableCollectionsMarshal.AsImmutableArray(m_componentTypes);
		}

		public ImmutableArray<uint> ComponentBits
		{
			get => ImmutableCollectionsMarshal.AsImmutableArray(m_componentBits);
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

		public int TagComponentTypeCount
		{
			get => m_componentTypes.Length - m_storedComponentTypeCount;
		}

		public int EntitySize
		{
			get => m_entitySize;
		}

		public int Id
		{
			get => m_id;
		}

		public static EntityArchetype Create(int id)
		{
			return new EntityArchetype(id);
		}

		public static EntityArchetype Create(ComponentType[] componentTypes, int id)
		{
			ArgumentNullException.ThrowIfNull(componentTypes);

			if (componentTypes.Length > 0)
			{
				ComponentType[] array = new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, array, componentTypes.Length);
				Array.Sort(array);

				if (array[^1] != null)
				{
					return new EntityArchetype(array, id);
				}
			}

			return new EntityArchetype(id);
		}

		public static EntityArchetype Create(IEnumerable<ComponentType> componentTypes, int id)
		{
			ComponentType[] array = componentTypes.ToArray();
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array, id);
			}

			return new EntityArchetype(id);
		}

		public static EntityArchetype Create(ReadOnlySpan<ComponentType> componentTypes, int id)
		{
			ComponentType[] array = componentTypes.ToArray();
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array, id);
			}

			return new EntityArchetype(id);
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

			int compareValue = a.m_id.CompareTo(b.m_id);

			if (compareValue != 0)
			{
				return compareValue;
			}

			return ((ReadOnlySpan<uint>)a.m_componentBits).SequenceCompareTo(b.m_componentBits);
		}

		public static bool Equals(EntityArchetype? a, EntityArchetype? b)
		{
			return a == b
				|| a != null
				&& b != null
				&& a.m_id == b.m_id
				&& ((ReadOnlySpan<uint>)a.m_componentBits).SequenceEqual(b.m_componentBits);
		}

		public EntityArchetype Clone(int id)
		{
			return new EntityArchetype(this, id);
		}

		public EntityArchetype CloneWith(ComponentType componentType, int id)
		{
			if (componentType == null || Contains(componentType))
			{
				return new EntityArchetype(this, id);
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

			return new EntityArchetype(destinationComponentTypes, id);
		}

		public EntityArchetype CloneWithout(ComponentType componentType, int id)
		{
			if (!Contains(componentType))
			{
				return new EntityArchetype(this, id);
			}

			if (m_componentTypes.Length == 1)
			{
				return new EntityArchetype(id);
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

			return new EntityArchetype(destinationComponentTypes, id);
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
			if (obj == null)
			{
				return 1;
			}

			if (obj is EntityArchetype other)
			{
				return Compare(this, other);
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
