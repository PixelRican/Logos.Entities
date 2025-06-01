using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Monophyll.Entities
{
	public class EntityTable
	{
		private const int MinimumCapacity = 8;

		private readonly object? m_writeLock;
		private readonly EntityArchetype m_archetype;
		private readonly Array[] m_components;
		private readonly Entity[] m_entities;
		private int m_size;
		private int m_version;

		public EntityTable(EntityArchetype archetype)
			: this(archetype, null, MinimumCapacity)
		{
		}

		public EntityTable(EntityArchetype archetype, int capacity)
			: this(archetype, null, capacity)
		{
		}

		public EntityTable(EntityArchetype archetype, object? writeLock)
			: this(archetype, writeLock, MinimumCapacity)
		{
		}

		public EntityTable(EntityArchetype archetype, object? writeLock, int capacity)
		{
            ArgumentNullException.ThrowIfNull(archetype);

            if (capacity < MinimumCapacity)
			{
				ArgumentOutOfRangeException.ThrowIfNegative(capacity);
				capacity = MinimumCapacity;
			}

			ReadOnlySpan<ComponentType> componentTypes = archetype.ComponentTypes.Slice(0,
				archetype.ManagedPartitionLength + archetype.UnmanagedPartitionLength);
			m_archetype = archetype;
			m_writeLock = writeLock;

			if (componentTypes.Length > 0)
			{
				Array[] components = new Array[componentTypes.Length];

				for (int i = 0; i < components.Length; i++)
				{
					components[i] = Array.CreateInstance(componentTypes[i].Type, capacity);
				}

				m_components = components;
			}
			else
			{
				m_components = Array.Empty<Array>();
			}

			m_entities = new Entity[capacity];
		}

		public EntityArchetype Archetype
		{
			get => m_archetype;
		}

		public int Capacity
		{
			get => m_entities.Length;
		}

		public int Count
		{
			get => m_size;
		}

		public int Version
		{
			get => m_version;
		}

		public bool IsEmpty
		{
			get => m_size == 0;
		}

		public bool IsFull
		{
			get => m_size == m_entities.Length;
		}

		public bool IsReadOnly
		{
			get => m_writeLock != null && !Monitor.IsEntered(m_writeLock);
		}

		public Span<T> GetComponents<T>()
		{
			return new Span<T>(
				(T[])FindComponents(ComponentType.TypeOf<T>(), throwIfNotFound: true)!, 0, m_size);
        }

        public ref T GetComponentDataReference<T>()
        {
            return ref MemoryMarshal.GetArrayDataReference(
				(T[])FindComponents(ComponentType.TypeOf<T>(), throwIfNotFound: true)!);
        }

        public bool TryGetComponents<T>(out Span<T> result)
		{
			Array? components = FindComponents(ComponentType.TypeOf<T>(), throwIfNotFound: false);

			if (components == null)
            {
                result = Span<T>.Empty;
                return false;
            }

            result = new Span<T>((T[])components, 0, m_size);
            return true;
        }

		private Array? FindComponents(ComponentType componentType, bool throwIfNotFound)
		{
			int index = m_archetype.ComponentTypes.BinarySearch(componentType);
			Array[] components = m_components;

			if ((uint)index >= (uint)components.Length)
			{
				if (throwIfNotFound)
				{
                    throw new ArgumentException(
                        $"The EntityTable does not store components of type {componentType.Type.Name}.");
                }

                return null;
            }

            return components[index];
        }

		public ReadOnlySpan<Entity> GetEntities()
		{
			return new ReadOnlySpan<Entity>(m_entities, 0, m_size);
		}

		public ref readonly Entity GetEntityDataReference()
		{
			return ref MemoryMarshal.GetArrayDataReference(m_entities);
		}

		public void Add(Entity entity)
        {
			ThrowIfReadOnly();

			int size = m_size;
            Entity[] entities = m_entities;

            if ((uint)size >= (uint)entities.Length)
            {
                throw new InvalidOperationException("The EntityTable is full.");
            }

            Array[] components = m_components;

            // Zero-initialize unmanaged components.
            for (int i = m_archetype.ManagedPartitionLength; i < components.Length; i++)
			{
				Array.Clear(components[i], size, 1);
			}

			entities[size] = entity;
			m_size = size + 1;
			m_version++;
		}

		public void AddRange(EntityTable table, int tableIndex, int count)
		{
			ThrowIfReadOnly();
			ArgumentNullException.ThrowIfNull(table);

			if ((uint)tableIndex >= (uint)table.m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(tableIndex), tableIndex,
                    "Table index was out of range. Must be non-negative and less than the size of the table.");
            }

            ArgumentOutOfRangeException.ThrowIfNegative(count);
			
			if (table.m_size - count < tableIndex)
            {
                throw new ArgumentException("Count exceeds the size of the table.", nameof(count));
            }

            Entity[] entities = m_entities;
			int size = m_size;

			if (entities.Length - count < size)
			{
				throw new ArgumentException("Count exceeds the capacity of the EntityTable.", nameof(count));
			}

			if (count == 0)
			{
				return;
			}

            ReadOnlySpan<ComponentType> sourceComponentTypes = table.m_archetype.ComponentTypes;
            ReadOnlySpan<ComponentType> destinationComponentTypes = m_archetype.ComponentTypes;
            Array[] sourceComponents = table.m_components;
            Array[] destinationComponents = m_components;
            ComponentType sourceComponentType = null!;

            for (int sourceIndex = -1, destinationIndex = 0;
				destinationIndex < destinationComponents.Length; destinationIndex++)
            {
                ComponentType destinationComponentType = destinationComponentTypes[destinationIndex];
				ComponentTypeCategory category = destinationComponentType.Category;

				if (category == ComponentTypeCategory.Tag)
				{
					break;
                }

            Compare:
                switch (ComponentType.Compare(sourceComponentType, destinationComponentType))
                {
                    case -1:
                        int nextSourceIndex = sourceIndex + 1;

                        if (nextSourceIndex < sourceComponents.Length)
                        {
                            sourceIndex = nextSourceIndex;
                            sourceComponentType = sourceComponentTypes[sourceIndex];
                            goto Compare;
                        }

                        goto default;
                    case 0:
                        Array.Copy(sourceComponents[sourceIndex], tableIndex,
                            destinationComponents[destinationIndex], size, count);
                        continue;
                    default:
                        if (category == ComponentTypeCategory.Unmanaged)
                        {
                            Array.Clear(destinationComponents[destinationIndex], size, count);
                        }

                        continue;
                }
            }

            Array.Copy(table.m_entities, tableIndex, entities, size, count);
            m_size = size + count;
            m_version++;
		}

		public void Clear()
        {
			ThrowIfReadOnly();

			int size = m_size;
            int managedPartitionLength = m_archetype.ManagedPartitionLength;
            Array[] components = m_components;

            // Frees references to managed objects.
            for (int i = 0; i < managedPartitionLength; i++)
            {
                Array.Clear(components[i], 0, size);
            }

            m_size = 0;
			m_version++;
		}

		public bool Remove(Entity entity)
		{
			int index;

			if (IsReadOnly || (index = Array.IndexOf(m_entities, entity, 0, m_size)) == -1)
			{
				return false;
			}

			RemoveAt(index);
			return true;
		}

		public void RemoveAt(int index)
		{
			ThrowIfReadOnly();
			
			int size = m_size;

			if ((uint)index >= (uint)size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    "Index was out of range. Must be non-negative and less than the size of the EntityTable.");
            }

			Array[] components = m_components;

            if (index < --size)
			{
				for (int i = 0; i < components.Length; i++)
				{
					Array array = components[i];
					Array.Copy(array, size, array, index, 1);
				}

				m_entities[index] = m_entities[size];
			}

            int managedPartitionLength = m_archetype.ManagedPartitionLength;

            // Frees references to managed objects.
            for (int i = 0; i < managedPartitionLength; i++)
            {
                Array.Clear(components[i], size, 1);
            }

            m_size = size;
			m_version++;
		}

		public void RemoveRange(int index, int count)
		{
			ThrowIfReadOnly();
			ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            int size = m_size - count;

			if (size < index)
            {
                throw new ArgumentException("Count exceeds the size of the EntityTable.", nameof(count));
            }

			if (count == 0)
			{
				return;
			}

            Array[] components = m_components;

            if (index < size)
            {
				int copyIndex = index + count;
				int copyLength = size - index;

                for (int i = 0; i < components.Length; i++)
                {
                    Array array = components[i];
                    Array.Copy(array, copyIndex, array, index, copyLength);
                }

                m_entities[index] = m_entities[size];
            }

            int managedPartitionLength = m_archetype.ManagedPartitionLength;

            // Frees references to managed objects.
            for (int i = 0; i < managedPartitionLength; i++)
            {
                Array.Clear(components[i], size, count);
            }

            m_size = size;
            m_version++;
        }

		private void ThrowIfReadOnly()
		{
            if (IsReadOnly)
            {
                throw new InvalidOperationException("The EntityTable is read-only.");
            }
        }
	}
}
