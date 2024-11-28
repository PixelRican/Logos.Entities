using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Monophyll.Entities
{
	public class EntityArchetypeChunk
	{
		private readonly EntityArchetype m_archetype;
		private readonly Array[] m_components;
		private readonly Entity[] m_entities;
		private int m_size;
		private int m_version;

		public EntityArchetypeChunk(EntityArchetype archetype)
		{
			ArgumentNullException.ThrowIfNull(archetype);

			m_archetype = archetype;

			if (archetype.StoredComponentTypeCount > 0)
			{
				ImmutableArray<ComponentType> componentTypes = archetype.ComponentTypes;

				m_components = new Array[archetype.StoredComponentTypeCount];

				for (int i = 0; i < archetype.StoredComponentTypeCount; i++)
				{
					m_components[i] = Array.CreateInstance(componentTypes[i].Type, archetype.ChunkCapacity);
				}
			}
			else
			{
				m_components = Array.Empty<Array>();
			}

			m_entities = new Entity[archetype.ChunkCapacity];
		}

		public EntityArchetype Archetype
		{
			get => m_archetype;
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
			get => m_size == m_archetype.ChunkCapacity;
		}

		public Span<T> GetComponents<T>()
		{
			return new Span<T>((T[])GetComponents(ComponentType.TypeOf<T>()), 0, m_size);
		}

		public ref T GetComponentReference<T>()
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

		public void Push(Entity entity)
		{
			int size = m_size;

			if ((uint)size >= (uint)m_archetype.ChunkCapacity)
			{
				throw new InvalidOperationException("The EntityArchetypeChunk is full.");
			}

			// Zero-initializes unmanaged components.
			for (int i = m_archetype.ManagedComponentTypeCount; i < m_archetype.StoredComponentTypeCount; i++)
			{
				Array.Clear(m_components[i], size, 1);
			}

			m_entities[size] = entity;
			m_size = size + 1;
			m_version++;
		}

		public Entity Pop()
		{
			int size = m_size - 1;

			if (size < 0)
			{
				throw new InvalidOperationException("The EntityArchetypeChunk is empty.");
			}

			// Frees references to GC collectable objects.
			for (int i = 0; i < m_archetype.ManagedComponentTypeCount; i++)
			{
				Array.Clear(m_components[i], size, 1);
			}

			Entity entity = m_entities[size];
			m_size = size;
			m_version++;
			return entity;
		}

		public void SetRange(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			ArgumentNullException.ThrowIfNull(chunk);
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfNegative(chunkIndex);
			ArgumentOutOfRangeException.ThrowIfNegative(length);

			if ((uint)(index + length) > (uint)m_size)
			{
				throw new ArgumentOutOfRangeException(null, "");
			}

			if ((uint)(chunkIndex + length) > (uint)chunk.m_size)
			{
				throw new ArgumentOutOfRangeException(null, "");
			}

			CopyRange(index, chunk, chunkIndex, length);
		}

		private void CopyRange(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
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
				ComponentType destinationComponentType = destinationComponentTypes[destinationIndex++];

			CompareTypes:
				switch (ComponentType.Compare(sourceComponentType, destinationComponentType))
				{
					case -1:
						if (sourceIndex < sourceArchetype.StoredComponentTypeCount)
						{
							sourceComponentType = sourceComponentTypes[sourceIndex++];
							goto CompareTypes;
						}
						goto case 1;
					case 1:
						Array.Clear(destinationComponents[destinationIndex], index, length);
						continue;
					default:
						Array.Copy(sourceComponents[sourceIndex], chunkIndex, destinationComponents[destinationIndex], index, length);
						continue;
				}
			}

			Array.Copy(chunk.m_entities, chunkIndex, m_entities, index, length);
			m_version++;
		}
	}
}
