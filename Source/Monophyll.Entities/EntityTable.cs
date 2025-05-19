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

		public EntityTable(EntityArchetype archetype) : this(archetype, null, MinimumCapacity)
		{
		}

		public EntityTable(EntityArchetype archetype, int capacity) : this(archetype, null, capacity)
		{
		}

		public EntityTable(EntityArchetype archetype, object? writeLock) : this(archetype, writeLock, MinimumCapacity)
		{
		}

		public EntityTable(EntityArchetype archetype, object? writeLock, int capacity)
		{
			if (archetype == null)
			{
				throw new ArgumentNullException(nameof(archetype));
			}

			if (capacity < MinimumCapacity)
			{
				if (capacity < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "capacity is negative.");
				}

				capacity = MinimumCapacity;
			}

			ReadOnlySpan<ComponentType> componentTypes = archetype.ComponentTypes.Slice(0,
				archetype.ManagedPartitionLength + archetype.UnmanagedPartitionLength);
			m_archetype = archetype;
			m_writeLock = writeLock;

			if (componentTypes.Length > 0)
			{
				m_components = new Array[componentTypes.Length];

				for (int i = 0; i < m_components.Length; i++)
				{
					m_components[i] = Array.CreateInstance(componentTypes[i].Type, capacity);
				}
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
			T[]? components = (T[]?)GetComponents(ComponentType.TypeOf<T>());

			if (components == null)
			{
				throw new ArgumentException($"The EntityTable does not store components of type {typeof(T).Name}.");
			}

			return new Span<T>(components, 0, m_size);
		}

		public bool TryGetComponents<T>(out Span<T> result)
		{
			T[]? components = (T[]?)GetComponents(ComponentType.TypeOf<T>());

			if (components != null)
			{
				result = new Span<T>(components, 0, m_size);
				return true;
			}

			result = Span<T>.Empty;
			return false;
		}

		public ref T GetComponentDataReference<T>()
		{
			T[]? components = (T[]?)GetComponents(ComponentType.TypeOf<T>());

			if (components == null)
			{
				throw new ArgumentException($"The EntityTable does not store components of type {typeof(T).Name}.");
			}

			return ref MemoryMarshal.GetArrayDataReference(components);
		}

		private Array? GetComponents(ComponentType componentType)
		{
			int index = m_archetype.ComponentTypes.BinarySearch(componentType);

			if ((uint)index < (uint)m_components.Length)
			{
				return m_components[index];
			}

			return null;
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
			int size = m_size;

			if ((uint)size >= (uint)m_entities.Length)
			{
				throw new InvalidOperationException("The EntityTable is full.");
			}

			if (IsReadOnly)
			{
				throw new InvalidOperationException("The EntityTable is read-only.");
			}

			// Zero-initialize unmanaged components.
			for (int i = m_archetype.ManagedPartitionLength; i < m_components.Length; i++)
			{
				Array.Clear(m_components[i], size, 1);
			}

			m_entities[size] = entity;
			m_size = size + 1;
			m_version++;
		}

		public void AddRange(EntityTable table, int tableIndex, int length)
		{
			int index = m_size;

			if ((uint)(index + length) > (uint)m_entities.Length)
			{
				throw new InvalidOperationException();
			}

			CopyRange(index, table, tableIndex, length);
			m_size = index + length;
		}

		public bool Remove(Entity entity)
		{
			int index = Array.IndexOf(m_entities, entity, 0, m_size);

			if (index == -1)
			{
				return false;
			}

			RemoveAt(index);
			return true;
		}

		public void RemoveAt(int index)
		{
			int size = m_size;

			if ((uint)index >= (uint)size)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, "");
			}

			if (IsReadOnly)
			{
				throw new InvalidOperationException("The EntityTable is read-only.");
			}

			if (index < --size)
			{
				for (int i = 0; i < m_components.Length; i++)
				{
					Array array = m_components[i];
					Array.Copy(array, size, array, index, 1);

					if (i < m_archetype.ManagedPartitionLength)
					{
						Array.Clear(array, size, 1);
					}
				}

				m_entities[index] = m_entities[size];
			}
			else
			{
				// Frees references to managed objects.
				for (int i = 0; i < m_archetype.ManagedPartitionLength; i++)
				{
					Array.Clear(m_components[i], size, 1);
				}
			}

			m_size = size;
			m_version++;
		}

		public void Set(int index, Entity entity)
		{
			if ((uint)index >= (uint)m_size)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, "");
			}

			if (IsReadOnly)
			{
				throw new InvalidOperationException("The EntityTable is read-only.");
			}

			foreach (Array array in m_components)
			{
				Array.Clear(array, index, 1);
			}

			m_entities[index] = entity;
			m_version++;
		}

		public void SetRange(int index, EntityTable table, int tableIndex, int length)
		{
			if ((uint)(index + length) > (uint)m_size)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, "");
			}

			CopyRange(index, table, tableIndex, length);
		}

		private void CopyRange(int index, EntityTable table, int tableIndex, int length)
		{
			ArgumentNullException.ThrowIfNull(table);
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfNegative(tableIndex);
			ArgumentOutOfRangeException.ThrowIfNegative(length);

			if ((uint)(tableIndex + length) > (uint)table.m_size)
			{
				throw new ArgumentOutOfRangeException();
			}

			if (IsReadOnly)
			{
				throw new InvalidOperationException("The EntityTable is read-only.");
			}

			ReadOnlySpan<ComponentType> destinationComponentTypes = m_archetype.ComponentTypes;
			ReadOnlySpan<ComponentType> sourceComponentTypes = table.m_archetype.ComponentTypes;
			Array[] destinationComponents = m_components;
			Array[] sourceComponents = table.m_components;
			int destinationIndex = 0;
			int sourceIndex = 0;
			ComponentType? sourceComponentType = null;

			while (destinationIndex < destinationComponents.Length)
			{
				ComponentType destinationComponentType = destinationComponentTypes[destinationIndex];

			CompareTypes:
				switch (ComponentType.Compare(sourceComponentType, destinationComponentType))
				{
					case -1:
						if (sourceIndex < sourceComponents.Length
							&& (sourceComponentType == null || ++sourceIndex < sourceComponents.Length))
						{
							sourceComponentType = sourceComponentTypes[sourceIndex];
							goto CompareTypes;
						}
						goto case 1;
					case 1:
						Array.Clear(destinationComponents[destinationIndex++], index, length);
						continue;
					default:
						Array.Copy(sourceComponents[sourceIndex], tableIndex, destinationComponents[destinationIndex++], index, length);
						continue;
				}
			}

			Array.Copy(table.m_entities, tableIndex, m_entities, index, length);
			m_version++;
		}
	}
}
