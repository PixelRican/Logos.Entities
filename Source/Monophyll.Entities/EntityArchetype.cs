using Monophyll.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
	public sealed class EntityArchetype : IEquatable<EntityArchetype>
	{
		private static readonly EntityArchetype s_base = new EntityArchetype();

		private readonly ComponentType[] m_componentTypes;
		private readonly uint[] m_componentBits;
		private readonly int m_managedPartitionLength;
		private readonly int m_unmanagedPartitionLength;
		private readonly int m_tagPartitionLength;
		private readonly int m_entitySize;

		private EntityArchetype()
		{
			m_componentTypes = Array.Empty<ComponentType>();
			m_componentBits = Array.Empty<uint>();
			m_entitySize = Unsafe.SizeOf<Entity>();
		}

		private EntityArchetype(ComponentType[] componentTypes)
		{
			m_componentTypes = componentTypes;
			m_componentBits = new uint[componentTypes[^1].Id + 32 >> 5];
			m_entitySize = Unsafe.SizeOf<Entity>();

			int freeIndex = 0;
			ComponentType? previousComponentType = null;

			foreach (ComponentType currentComponentType in componentTypes)
			{
				if (!ComponentType.Equals(previousComponentType, currentComponentType))
				{
					m_componentTypes[freeIndex++] = previousComponentType = currentComponentType;
					m_componentBits[currentComponentType.Id >> 5] |= 1u << currentComponentType.Id;
					m_entitySize += currentComponentType.Size;

					switch (currentComponentType.Category)
					{
						case ComponentTypeCategory.Managed:
							m_managedPartitionLength++;
							continue;
						case ComponentTypeCategory.Unmanaged:
							m_unmanagedPartitionLength++;
							continue;
						default:
							m_tagPartitionLength++;
							continue;
					}
				}
			}

			Array.Resize(ref m_componentTypes, freeIndex);
		}

		public static EntityArchetype Base
		{
			get => s_base;
		}

		public ReadOnlySpan<ComponentType> ComponentTypes
		{
			get => new ReadOnlySpan<ComponentType>(m_componentTypes);
		}

		public ReadOnlySpan<uint> ComponentBits
		{
			get => new ReadOnlySpan<uint>(m_componentBits);
		}

		public int ManagedPartitionLength
		{
			get => m_managedPartitionLength;
		}

		public int UnmanagedPartitionLength
		{
			get => m_unmanagedPartitionLength;
		}

		public int TagPartitionLength
		{
			get => m_tagPartitionLength;
		}

		public int EntitySize
		{
			get => m_entitySize;
		}

		public static EntityArchetype Create(ComponentType[] componentTypes)
		{
			if (componentTypes == null)
			{
				throw new ArgumentNullException(nameof(componentTypes));
			}

			if (componentTypes.Length > 0)
			{
				ComponentType[] array = new ComponentType[componentTypes.Length];
				Array.Copy(componentTypes, array, componentTypes.Length);
				Array.Sort(array);

				if (array[^1] != null)
				{
					return new EntityArchetype(array);
				}
			}

			return s_base;
		}

		public static EntityArchetype Create(IEnumerable<ComponentType> componentTypes)
		{
			ComponentType[] array = componentTypes.ToArray();
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array);
			}

			return s_base;
		}

		public static EntityArchetype Create(ReadOnlySpan<ComponentType> componentTypes)
		{
			ComponentType[] array = componentTypes.ToArray();
			Array.Sort(array);

			if (array.Length > 0 && array[^1] != null)
			{
				return new EntityArchetype(array);
			}

			return s_base;
		}

		public static bool Equals(EntityArchetype? a, EntityArchetype? b)
		{
			return a == b
				|| a != null
				&& b != null
				&& MemoryExtensions.SequenceEqual<uint>(a.m_componentBits, b.m_componentBits);
		}

		public EntityArchetype Add(ComponentType componentType)
		{
			if (componentType == null || Contains(componentType))
			{
				return this;
			}

			ComponentType[] destinationComponentTypes = new ComponentType[m_componentTypes.Length + 1];
			int destinationIndex = 1;
			int insertIndex = 0;

			foreach (ComponentType sourceComponentType in m_componentTypes)
			{
				if (ComponentType.Compare(sourceComponentType, componentType) >= 0)
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
			return new EntityArchetype(destinationComponentTypes);
		}

		public EntityArchetype Remove(ComponentType componentType)
		{
			if (!Contains(componentType))
			{
				return this;
			}

			if (m_componentTypes.Length == 1)
			{
				return s_base;
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

			return new EntityArchetype(destinationComponentTypes);
		}

		public bool Contains(ComponentType componentType)
		{
			int index;
			return componentType != null
				&& (index = componentType.Id >> 5) < m_componentBits.Length
				&& (m_componentBits[index] & 1u << componentType.Id) != 0;
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
			return BitSetOperations.GetHashCode(new ReadOnlySpan<uint>(m_componentBits));
		}

		public override string ToString()
		{
			return $"EntityArchetype {{ ComponentTypes = [{string.Join(", ", (object[])m_componentTypes)}] }}";
		}
	}
}
