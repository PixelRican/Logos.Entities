using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Monophyll.Entities
{
	public class EntityArchetypeChunk
	{
		private const int DefaultCapacity = 8;

		private readonly object? m_lock;
		private readonly EntityArchetype m_archetype;
		private readonly Array[] m_components;
		private readonly Entity[] m_entities;
		private int m_size;
		private int m_version;

		public EntityArchetypeChunk(EntityArchetype archetype) : this(archetype, null, DefaultCapacity)
		{
		}

		public EntityArchetypeChunk(EntityArchetype archetype, int capacity) : this(archetype, null, capacity)
		{
		}

		public EntityArchetypeChunk(EntityArchetype archetype, object? modificationLock) : this(archetype, modificationLock, DefaultCapacity)
		{
		}

		public EntityArchetypeChunk(EntityArchetype archetype, object? modificationLock, int capacity)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			ArgumentOutOfRangeException.ThrowIfNegative(capacity);

			m_archetype = archetype;
			m_lock = modificationLock;

			if (capacity < DefaultCapacity)
			{
				capacity = DefaultCapacity;
			}

			if (archetype.StoredComponentTypeCount > 0)
			{
				ImmutableArray<ComponentType> componentTypes = archetype.ComponentTypes;

				m_components = new Array[archetype.StoredComponentTypeCount];

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

		public bool IsModifiable
		{
			get => m_lock == null || Monitor.IsEntered(m_lock);
		}

		public Span<T> GetComponents<T>()
		{
			return new Span<T>((T[])GetComponents(ComponentType.TypeOf<T>()), 0, m_size);
		}

		public ref T GetComponentDataReference<T>()
		{
			return ref MemoryMarshal.GetArrayDataReference((T[])GetComponents(ComponentType.TypeOf<T>()));
		}

		private Array GetComponents(ComponentType componentType)
		{
			int index = m_archetype.ComponentTypes.BinarySearch(componentType);

			if ((uint)index >= (uint)m_components.Length)
			{
				throw new ArgumentException($"The EntityArchetypeChunk does not store components of type {componentType.Type.Name}.");
			}

			return m_components[index];
		}

		public ReadOnlySpan<Entity> GetEntities()
		{
			return new ReadOnlySpan<Entity>(m_entities, 0, m_size);
		}

		public ref readonly Entity GetEntityDataReference()
		{
			return ref MemoryMarshal.GetArrayDataReference(m_entities);
		}

		public void Push(Entity entity)
		{
			int size = m_size;

			if ((uint)size >= (uint)m_entities.Length)
			{
				throw new InvalidOperationException("The EntityArchetypeChunk is full.");
			}

			ThrowIfUnmodifiable();

			// Zero-initializes unmanaged components.
			for (int i = m_archetype.ManagedComponentTypeCount; i < m_archetype.StoredComponentTypeCount; i++)
			{
				Array.Clear(m_components[i], size, 1);
			}

			m_entities[size] = entity;
			m_size = size + 1;
			m_version++;
		}

		public void PushRange(EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			int index = m_size;

			if ((uint)(index + length) > (uint)m_entities.Length)
			{
				throw new InvalidOperationException();
			}

			CopyRange(index, chunk, chunkIndex, length);
			m_size = index + length;
		}

		public Entity Pop()
		{
			int size = m_size - 1;

			if (size < 0)
			{
				throw new InvalidOperationException("The EntityArchetypeChunk is empty.");
			}

			ThrowIfUnmodifiable();

			// Frees references to managed objects.
			for (int i = 0; i < m_archetype.ManagedComponentTypeCount; i++)
			{
				Array.Clear(m_components[i], size, 1);
			}

			Entity entity = m_entities[size];
			m_size = size;
			m_version++;
			return entity;
		}

		public void Set(int index, Entity entity)
		{
			if ((uint)index >= (uint)m_size)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, "");
			}

			for (int i = 0; i < m_components.Length; i++)
			{
				Array.Clear(m_components[i], index, 1);
			}

			m_entities[index] = entity;
			m_version++;
		}

		public void SetRange(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			if ((uint)(index + length) > (uint)m_size)
			{
				throw new ArgumentOutOfRangeException();
			}

			CopyRange(index, chunk, chunkIndex, length);
		}

		private void CopyRange(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			ArgumentNullException.ThrowIfNull(chunk);
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfNegative(chunkIndex);
			ArgumentOutOfRangeException.ThrowIfNegative(length);

			if ((uint)(chunkIndex + length) > (uint)chunk.m_size)
			{
				throw new ArgumentOutOfRangeException();
			}

			ThrowIfUnmodifiable();

			EntityArchetype destinationArchetype = m_archetype;
			EntityArchetype sourceArchetype = chunk.m_archetype;
			ImmutableArray<ComponentType> destinationComponentTypes = destinationArchetype.ComponentTypes;
			ImmutableArray<ComponentType> sourceComponentTypes = sourceArchetype.ComponentTypes;
			Array[] destinationComponents = m_components;
			Array[] sourceComponents = chunk.m_components;
			int destinationIndex = 0;
			int sourceIndex = 0;
			ComponentType? sourceComponentType = null;

			while (destinationIndex < destinationArchetype.StoredComponentTypeCount)
			{
				ComponentType destinationComponentType = destinationComponentTypes[destinationIndex];

			CompareTypes:
				switch (ComponentType.Compare(sourceComponentType, destinationComponentType))
				{
					case -1:
						if (sourceIndex < sourceArchetype.StoredComponentTypeCount &&
							(sourceComponentType == null || ++sourceIndex < sourceArchetype.StoredComponentTypeCount))
						{
							sourceComponentType = sourceComponentTypes[sourceIndex];
							goto CompareTypes;
						}
						goto case 1;
					case 1:
						Array.Clear(destinationComponents[destinationIndex++], index, length);
						continue;
					default:
						Array.Copy(sourceComponents[sourceIndex], chunkIndex, destinationComponents[destinationIndex++], index, length);
						continue;
				}
			}

			Array.Copy(chunk.m_entities, chunkIndex, m_entities, index, length);
			m_version++;
		}

		private void ThrowIfUnmodifiable()
		{
			if (m_lock != null && !Monitor.IsEntered(m_lock))
			{
				throw new InvalidOperationException("The EntityArchetypeChunk cannot be modified by the caller.");
			}
		}
	}
}
