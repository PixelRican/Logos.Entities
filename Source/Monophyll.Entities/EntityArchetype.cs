using Monophyll.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
	/// <summary>
	/// Represents a data model that describes how an entity's components are
	/// laid out in memory.
	/// </summary>
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
			ComponentType? previous = null;

			foreach (ComponentType current in componentTypes)
			{
				if (!ComponentType.Equals(previous, current))
				{
					m_componentTypes[freeIndex++] = previous = current;
					m_componentBits[current.Id >> 5] |= 1u << current.Id;
					m_entitySize += current.Size;

					switch (current.Category)
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

		/// <summary>
		/// Gets an <see cref="EntityArchetype"/> instance that models entities
		/// without any components.
		/// </summary>
		public static EntityArchetype Base
		{
			get => s_base;
		}

		public ReadOnlySpan<ComponentType> ComponentTypes
		{
			get => m_componentTypes;
		}

		public ReadOnlySpan<uint> ComponentBits
		{
			get => m_componentBits;
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
			ArgumentNullException.ThrowIfNull(componentTypes);

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

			if (array.Length > 0)
            {
                Array.Sort(array);

				if (array[^1] != null)
                {
                    return new EntityArchetype(array);
                }
			}

			return s_base;
		}

		public static EntityArchetype Create(ReadOnlySpan<ComponentType> componentTypes)
		{
			if (componentTypes.Length > 0)
			{
                ComponentType[] array = componentTypes.ToArray();
                Array.Sort(array);

				if (array[^1] != null)
				{
                    return new EntityArchetype(array);
                }
			}

			return s_base;
		}

		public static bool Equals(EntityArchetype? a, EntityArchetype? b)
		{
			return a == b
				|| a != null
				&& b != null
				&& a.m_componentBits.AsSpan().SequenceEqual(b.m_componentBits);
		}

		public EntityArchetype Add(ComponentType componentType)
		{
			if (componentType == null || BitSetOperations.Contains(m_componentBits, componentType.Id))
			{
				return this;
			}

			ComponentType[] source = m_componentTypes;
            ComponentType[] destination = new ComponentType[source.Length + 1];
            int index = 0;
			ComponentType current;

			while (index < source.Length && ComponentType.Compare(current = source[index], componentType) < 0)
			{
				destination[index++] = current;
			}

			destination[index] = componentType;

			while (index < source.Length)
			{
				current = source[index];
				destination[++index] = current;
			}

			return new EntityArchetype(destination);
		}

		public EntityArchetype Remove(ComponentType componentType)
		{
			if (componentType == null || !BitSetOperations.Contains(m_componentBits, componentType.Id))
			{
				return this;
            }

            ComponentType[] source = m_componentTypes;

            if (source.Length == 1)
			{
				return s_base;
			}

            ComponentType[] destination = new ComponentType[source.Length - 1];
			int index = 0;
			ComponentType current;

            while (!ComponentType.Equals(current = source[index], componentType))
            {
				destination[index++] = current;
            }

            while (index < destination.Length)
            {
				destination[index] = source[++index];
			}

            return new EntityArchetype(destination);
		}

		public bool Contains(ComponentType componentType)
		{
			return componentType != null
				&& BitSetOperations.Contains(m_componentBits, componentType.Id);
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
			return BitSetOperations.GetHashCode(m_componentBits);
		}

		public override string ToString()
		{
			return $"EntityArchetype {{ ComponentTypes = [{string.Join(", ", (object[])m_componentTypes)}] }}";
		}
	}
}
