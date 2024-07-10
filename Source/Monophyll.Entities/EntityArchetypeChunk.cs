using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monophyll.Entities
{
	public class EntityArchetypeChunk : IReadOnlyList<Entity>, ICollection
	{
		private readonly EntityArchetypeChunk? m_next;
		private readonly EntityArchetype m_archetype;
		private readonly byte[] m_data;
		private int m_count;
		private int m_version;

		public EntityArchetypeChunk(EntityArchetype archetype)
		{
			m_archetype = archetype ?? throw new ArgumentNullException(nameof(archetype));
			m_data = new byte[archetype.ChunkByteSize];
		}

		public EntityArchetypeChunk(EntityArchetypeChunk chunk)
		{
			m_next = chunk ?? throw new ArgumentNullException(nameof(chunk));
			m_archetype = chunk.m_archetype;
			m_data = new byte[chunk.m_data.Length];
		}

		public EntityArchetypeChunk? Next
		{
			get => m_next;
		}

		public EntityArchetype Archetype
		{
			get => m_archetype;
		}

		public int ByteSize
		{
			get => m_data.Length;
		}

		public int Count
		{
			get => m_count;
		}

		public int Version
		{
			get => m_version;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => this;
		}

		public Entity this[int index]
		{
			get
			{
				if ((uint)index >= (uint)m_count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
						"Index is less than zero or index is greater than or equal to Count.");
				}

				return Unsafe.Add(ref Unsafe.As<byte, Entity>(ref m_data[0]), index);
			}
			set
			{
				if ((uint)index >= (uint)m_count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
						"Index is less than zero or index is greater than or equal to Count.");
				}

				Unsafe.Add(ref Unsafe.As<byte, Entity>(ref m_data[0]), index) = value;
				m_version++;
			}
		}

		public void CopyTo(Entity[] array, int arrayIndex)
		{
			ArgumentNullException.ThrowIfNull(array, nameof(array));
			GetEntities().CopyTo(array.AsSpan(arrayIndex));
		}

		void ICollection.CopyTo(Array array, int index)
		{
            ArgumentNullException.ThrowIfNull(array, nameof(array));

            if (array.Rank != 1)
			{
				throw new ArgumentException("Multi-dimensional arrays are not supported.");
			}

			try
            {
				GetEntities().CopyTo(((Entity[])array).AsSpan(index));
            }
			catch (InvalidCastException)
			{
				throw new ArgumentException("Array is not of type Entity[].", nameof(array));
			}
		}

		public Span<T> GetComponents<T>() where T : unmanaged
		{
			ComponentType componentType = ComponentType.TypeOf<T>();
			ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
			int offset;

			if (componentType.ByteSize == 0 ||
				componentType.Id >= componentOffsets.Length ||
				(offset = componentOffsets[componentType.Id]) == 0)
			{
				throw new ArgumentException(
					$"The EntityArchetypeChunk does not store components of type {typeof(T).Name}.");
			}

			return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, T>(ref m_data[offset]), m_count);
		}

		public ReadOnlySpan<Entity> GetEntities()
		{
			return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, Entity>(ref m_data[0]), m_count);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		public void Push(Entity item)
		{
			if (m_count >= m_archetype.ChunkCapacity)
			{
				throw new InvalidOperationException("The EntityArchetypeChunk is full.");
			}

			ImmutableArray<ComponentType> componentTypes = m_archetype.ComponentTypes;
			ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
			ref byte data = ref m_data[0];

			Unsafe.Add(ref Unsafe.As<byte, Entity>(ref data), m_count) = item;

			for (int i = 0; i < componentTypes.Length; i++)
			{
				ComponentType componentType = componentTypes[i];
				int offset = componentOffsets[componentType.Id] + componentType.ByteSize * m_count;
				Unsafe.InitBlock(ref Unsafe.Add(ref data, offset), 0, (uint)componentType.ByteSize);
			}

			m_count++;
			m_version++;
		}

		public bool TryPush(Entity item)
		{
			if (m_count >= m_archetype.ChunkCapacity)
			{
				return false;
			}

			ImmutableArray<ComponentType> componentTypes = m_archetype.ComponentTypes;
			ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
			ref byte data = ref m_data[0];

			Unsafe.Add(ref Unsafe.As<byte, Entity>(ref data), m_count) = item;

			for (int i = 0; i < componentTypes.Length; i++)
			{
				ComponentType componentType = componentTypes[i];
				int offset = componentOffsets[componentType.Id] + componentType.ByteSize * m_count;
				Unsafe.InitBlock(ref Unsafe.Add(ref data, offset), 0, (uint)componentType.ByteSize);
			}

			m_count++;
			m_version++;
			return true;
		}

		public void PushRange(EntityArchetypeChunk chunk, int chunkIndex, int count)
		{
			ArgumentNullException.ThrowIfNull(chunk, nameof(chunk));

			if (chunkIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(chunkIndex), chunkIndex,
					"Chunk index must be a non-negative number.");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count,
					"Count must be a non-negative number.");
			}

			if (chunk.m_count - chunkIndex < count)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count,
					"Range exceeds the bounds of the other EntityArchetypeChunk.");
			}

			if (m_archetype.ChunkCapacity - m_count < count)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count,
					"Range exceeds the capacity of the EntityArchetypeChunk.");
			}

			ImmutableArray<ComponentType> componentTypes = m_archetype.ComponentTypes;
			ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
			ImmutableArray<int> otherComponentOffsets = chunk.m_archetype.ComponentOffsets;
			ref byte data = ref m_data[0];
			ref byte otherData = ref chunk.m_data[0];

			Unsafe.CopyBlock(ref Unsafe.Add(ref data, m_count * Unsafe.SizeOf<Entity>()),
				ref Unsafe.Add(ref otherData, chunkIndex * Unsafe.SizeOf<Entity>()),
				(uint)(count * Unsafe.SizeOf<Entity>()));

			for (int i = 0; i < componentTypes.Length; i++)
			{
				ComponentType componentType = componentTypes[i];
				int offset = componentOffsets[componentType.Id] + m_count * componentType.ByteSize;
				int otherOffset;

				if (componentType.Id < otherComponentOffsets.Length &&
					(otherOffset = otherComponentOffsets[componentType.Id]) != 0)
				{
					Unsafe.CopyBlock(ref Unsafe.Add(ref data, offset),
						ref Unsafe.Add(ref otherData, otherOffset + chunkIndex * componentType.ByteSize),
						(uint)(count * componentType.ByteSize));
				}
				else
				{
					Unsafe.InitBlock(ref Unsafe.Add(ref data, offset), 0, (uint)(count * componentType.ByteSize));
				}
			}

			m_count += count;
			m_version++;
		}

		public Entity Pop()
		{
			if (m_count <= 0)
			{
				throw new InvalidOperationException("The EntityArchetypeChunk is empty.");
			}

			Entity result = Unsafe.Add(ref Unsafe.As<byte, Entity>(ref m_data[0]), --m_count);
			m_version++;
			return result;
		}

		public bool TryPop(out Entity result)
		{
			if (m_count <= 0)
			{
				result = default;
				return false;
			}

			result = Unsafe.Add(ref Unsafe.As<byte, Entity>(ref m_data[0]), --m_count);
			m_version++;
			return true;
		}

		public void PopRange(EntityArchetypeChunk chunk, int chunkIndex, int count)
		{
			ArgumentNullException.ThrowIfNull(chunk, nameof(chunk));

			if (chunkIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(chunkIndex), chunkIndex,
					"Chunk index must be a non-negative number.");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count,
					"Count must be a non-negative number.");
			}

			if (chunk.m_count - chunkIndex < count)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count,
					"Range exceeds the bounds of the other EntityArchetypeChunk.");
			}

			if (m_count < count)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count,
					"Range exceeds the capacity of the EntityArchetypeChunk.");
			}

			ImmutableArray<ComponentType> otherComponentTypes = chunk.m_archetype.ComponentTypes;
			ImmutableArray<int> otherComponentOffsets = chunk.m_archetype.ComponentOffsets;
			ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
			ref byte otherData = ref chunk.m_data[0];
			ref byte data = ref m_data[0];
			int copyIndex = m_count - count;

			Unsafe.CopyBlock(ref Unsafe.Add(ref otherData, chunkIndex * Unsafe.SizeOf<Entity>()),
				ref Unsafe.Add(ref data, copyIndex * Unsafe.SizeOf<Entity>()),
				(uint)(count * Unsafe.SizeOf<Entity>()));

			for (int i = 0; i < otherComponentTypes.Length; i++)
			{
				ComponentType componentType = otherComponentTypes[i];
				int otherOffset = otherComponentOffsets[componentType.Id] + chunkIndex * componentType.ByteSize;
				int offset;

				if (componentType.Id < componentOffsets.Length &&
					(offset = componentOffsets[componentType.Id]) != 0)
				{
					Unsafe.CopyBlock(ref Unsafe.Add(ref otherData, otherOffset),
						ref Unsafe.Add(ref data, offset + copyIndex * componentType.ByteSize),
						(uint)(count * componentType.ByteSize));
				}
				else
				{
					Unsafe.InitBlock(ref Unsafe.Add(ref otherData, otherOffset), 0, (uint)(count * componentType.ByteSize));
				}
			}

			m_count = copyIndex;
			m_version++;
			chunk.m_version++;
		}

		public struct Enumerator : IEnumerator<Entity>
		{
			private readonly EntityArchetypeChunk m_chunk;
			private readonly int m_version;
			private int m_index;
			private Entity m_current;

			internal Enumerator(EntityArchetypeChunk chunk)
			{
				m_chunk = chunk;
				m_version = chunk.m_version;
				m_index = 0;
				m_current = default;
			}

			public readonly Entity Current
			{
				get => m_current;
			}

			readonly object IEnumerator.Current
			{
				get => m_current;
			}

			public readonly void Dispose()
			{
			}

			public bool MoveNext()
			{
				EntityArchetypeChunk localChunk = m_chunk;

				if (m_index < localChunk.m_count && m_version == localChunk.m_version)
				{
					m_current = Unsafe.Add(ref Unsafe.As<byte, Entity>(ref localChunk.m_data[0]), m_index++);
					return true;
				}

				return MoveNextRare();
			}

			private bool MoveNextRare()
			{
				if (m_version != m_chunk.m_version)
				{
					throw new InvalidOperationException(
						"The EntityArchetypeChunk was modified after the enumerator was created.");
				}

				m_index = m_chunk.m_count + 1;
				m_current = default;
				return false;
			}

			void IEnumerator.Reset()
			{
				if (m_version != m_chunk.m_version)
				{
					throw new InvalidOperationException(
						"The EntityArchetypeChunk was modified after the enumerator was created.");
				}

				m_index = 0;
				m_current = default;
			}
		}
	}
}
