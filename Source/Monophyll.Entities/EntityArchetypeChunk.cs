using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monophyll.Entities
{
	public class EntityArchetypeChunk : IList<Entity>, IReadOnlyList<Entity>, IList
	{
		private readonly EntityArchetype m_archetype;
		private readonly byte[] m_data;
		private int m_count;
		private int m_version;

		public EntityArchetypeChunk() : this(EntityArchetype.Base)
		{
		}

		public EntityArchetypeChunk(EntityArchetype archetype)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			m_archetype = archetype;
			m_data = GC.AllocateUninitializedArray<byte>(archetype.ChunkByteSize);
		}

		public EntityArchetype Archetype
		{
			get => m_archetype;
		}

		public int Count
		{
			get => m_count;
		}

		public int Version
		{
			get => m_version;
		}

		public bool IsEmpty
		{
			get => m_count == 0;
		}

		public bool IsFull
		{
			get => m_count == m_archetype.ChunkCapacity;
		}

		bool ICollection<Entity>.IsReadOnly
		{
			get => false;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => this;
		}

		bool IList.IsFixedSize
		{
			get => true;
		}

		bool IList.IsReadOnly
		{
			get => false;
		}

		public Entity this[int index]
		{
			get
			{
				if ((uint)index >= (uint)m_count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index, string.Empty);
				}

				return Unsafe.As<byte, Entity>(ref MemoryMarshal.GetReference(new Span<byte>(
					m_data, index * Unsafe.SizeOf<Entity>(), Unsafe.SizeOf<Entity>())));
			}
			set => SetEntity(index, value);
		}

		object? IList.this[int index]
		{
			get => this[index];
			set
			{
				if (value is not Entity entity)
				{
					throw new ArgumentException("Value is not of type Entity.", nameof(value));
				}

				SetEntity(index, entity);
			}
		}

		public void Add(Entity entity)
		{
			InsertEntity(m_count, entity);
		}

		int IList.Add(object? value)
		{
			if (value is not Entity entity)
			{
				throw new ArgumentException("Value is not of type Entity.");
			}

			int count = m_count;

			if ((uint)count < (uint)m_archetype.ChunkCapacity)
			{
				InsertEntity(count, entity);
				return count;
			}

			return -1;
		}

		public void AddRange(EntityArchetypeChunk chunk)
		{
			ArgumentNullException.ThrowIfNull(chunk);
			InsertEntities(m_count, chunk, 0, chunk.m_count);
		}

		public void AddRange(EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			InsertEntities(m_count, chunk, chunkIndex, length);
		}

		public void Clear()
		{
			ClearEntities();
		}

		public bool Contains(Entity entity)
		{
			return GetEntities().Contains(entity);
		}

		bool IList.Contains(object? value)
		{
			return value is Entity entity && GetEntities().Contains(entity);
		}

		public void CopyTo(Entity[] array)
		{
			ArgumentNullException.ThrowIfNull(array);
			GetEntities().CopyTo(array);
		}

		public void CopyTo(Entity[] array, int arrayIndex)
		{
			ArgumentNullException.ThrowIfNull(array);
			GetEntities().CopyTo(array.AsSpan(arrayIndex));
		}

		public void CopyTo(int index, Entity[] array, int arrayIndex, int length)
		{
			ArgumentNullException.ThrowIfNull(array);
			GetEntities()[..index].CopyTo(array.AsSpan(arrayIndex, length));
		}

		void ICollection.CopyTo(Array array, int index)
		{
			ArgumentNullException.ThrowIfNull(array);

			if (array.Rank != 1)
			{
				throw new ArgumentException("Multi-dimensional arrays are not supported.", nameof(array));
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
			int count = m_count;
			int index;

			if (componentType.ByteSize == 0 || (index = m_archetype.ComponentTypes.BinarySearch(componentType)) < 0)
			{
				throw new ArgumentException(
					$"The EntityArchetypeChunk does not store components of type {componentType.Type.Name}.");
			}

			ref T reference = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(new Span<byte>(
				m_data, m_archetype.ComponentOffsets[index], count * Unsafe.SizeOf<T>())));
			return MemoryMarshal.CreateSpan(ref reference, count);
		}

		public bool TryGetComponents<T>(out Span<T> components) where T : unmanaged
		{
			ComponentType componentType = ComponentType.TypeOf<T>();
			int count = m_count;
			int index;

			if (componentType.ByteSize == 0 || (index = m_archetype.ComponentTypes.BinarySearch(componentType)) < 0)
			{
				components = default;
				return false;
			}

			ref T reference = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(new Span<byte>(
				m_data, m_archetype.ComponentOffsets[index], count * componentType.ByteSize)));
			components = MemoryMarshal.CreateSpan(ref reference, count);
			return true;
		}

		public ReadOnlySpan<Entity> GetEntities()
		{
			int count = m_count;
			ref Entity reference = ref Unsafe.As<byte, Entity>(ref MemoryMarshal.GetReference(
				new Span<byte>(m_data, 0, count * Unsafe.SizeOf<Entity>())));
			return MemoryMarshal.CreateReadOnlySpan(ref reference, count);
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

		public int IndexOf(Entity entity)
		{
			return GetEntities().IndexOf(entity);
		}

		int IList.IndexOf(object? value)
		{
			if (value is Entity entity)
			{
				return GetEntities().IndexOf(entity);
			}

			return -1;
		}

		public void Insert(int index, Entity entity)
		{
			InsertEntity(index, entity);
		}

		void IList.Insert(int index, object? value)
		{
			if (value is not Entity entity)
			{
				throw new ArgumentException("Value is not of type Entity.", nameof(value));
			}

			InsertEntity(index, entity);
		}

		public void InsertRange(int index, EntityArchetypeChunk chunk)
		{
			ArgumentNullException.ThrowIfNull(chunk);
			InsertEntities(index, chunk, 0, chunk.m_count);
		}

		public void InsertRange(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			InsertEntities(index, chunk, chunkIndex, length);
		}

		public bool Remove(Entity entity)
		{
			int index = GetEntities().IndexOf(entity);

			if (index >= 0)
			{
				RemoveEntity(index);
				return true;
			}

			return false;
		}

		void IList.Remove(object? value)
		{
			int index;

			if (value is Entity entity && (index = GetEntities().IndexOf(entity)) >= 0)
			{
				RemoveEntity(index);
			}
		}

		public void RemoveAt(int index)
		{
			RemoveEntity(index);
		}

		public void SetRange(int index, EntityArchetypeChunk chunk)
		{
			ArgumentNullException.ThrowIfNull(chunk);
			SetEntities(index, chunk, 0, chunk.m_count);
		}

		public void SetRange(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			SetEntities(index, chunk, chunkIndex, length);
		}

		protected virtual void ClearEntities()
		{
			m_count = 0;
			m_version++;
		}

		protected virtual void InsertEntity(int index, Entity entity)
		{
			int count = m_count;
			int capacity = m_archetype.ChunkCapacity;

			if ((uint)index > (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Index is less than zero or greater than Count.");
			}

			if ((uint)count >= (uint)capacity)
			{
				throw new InvalidOperationException("The EntityArchetypeChunk is full.");
			}

			ImmutableArray<ComponentType> componentTypes = m_archetype.ComponentTypes;
			ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
			Span<byte> data = m_data;
			int byteSize = Unsafe.SizeOf<Entity>();

			if (index == count)
			{
				Unsafe.As<byte, Entity>(ref MemoryMarshal.GetReference(data.Slice(index * byteSize, byteSize))) = entity;

				for (int i = 0; i < componentTypes.Length; i++)
				{
					byteSize = componentTypes[i].ByteSize;
					data.Slice(componentOffsets[i] + index * byteSize, byteSize).Clear();
				}
			}
			else
			{
				int copyLength = count - index;
				int byteLength = copyLength * byteSize;
				int byteIndex = index * byteSize;

				data.Slice(byteIndex, byteLength).CopyTo(data.Slice(byteIndex + byteSize, byteLength));
				Unsafe.As<byte, Entity>(ref MemoryMarshal.GetReference(data.Slice(byteIndex, byteSize))) = entity;

				for (int i = 0; i < componentTypes.Length; i++)
				{
					byteSize = componentTypes[i].ByteSize;
					byteLength = copyLength * byteSize;
					byteIndex = componentOffsets[i] + index * byteSize;

					data.Slice(byteIndex, byteLength).CopyTo(data.Slice(byteIndex + byteSize, byteLength));
					data.Slice(byteIndex, byteSize).Clear();
				}
			}

			m_count = count + 1;
			m_version++;
		}

		protected virtual void InsertEntities(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			ArgumentNullException.ThrowIfNull(chunk);
			ArgumentOutOfRangeException.ThrowIfNegative(length);

			int count = m_count;
			int chunkCount = chunk.m_count;

			if ((uint)index > (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Index is less than zero or greater than Count.");
			}

			if ((uint)chunkIndex >= (uint)chunkCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Chunk index is less than zero or greater than or equal to the other EntityArchetypeChunk's Count.");
			}

			if (m_archetype.ChunkCapacity - count < length)
			{
				throw new ArgumentOutOfRangeException(nameof(length), length,
					"Length exceeds the bounds of the EntityArchetypeChunk.");
			}

			if (chunkCount - chunkIndex < length)
			{
				throw new ArgumentOutOfRangeException(nameof(length), length,
					"Length exceeds the bounds of the other EntityArchetypeChunk.");
			}

			if (chunkCount == 0)
			{
				return;
			}

			if (index == count)
			{
				CopyFromEntityArchetypeChunk(index, chunk, chunkIndex, length);
			}
			else
			{
				EntityArchetype otherArchetype = chunk.m_archetype;
				ImmutableArray<ComponentType> otherComponentTypes = otherArchetype.ComponentTypes;
				ImmutableArray<ComponentType> componentTypes = m_archetype.ComponentTypes;
				ImmutableArray<int> otherComponentOffsets = otherArchetype.ComponentOffsets;
				ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
				ReadOnlySpan<byte> otherData = chunk.m_data;
				Span<byte> data = m_data;
				int byteSize = Unsafe.SizeOf<Entity>();
				int byteLength = length * byteSize;
				int otherTypeIndex = 0;
				int typeIndex = 0;
				int byteIndex = index * byteSize;
				Span<byte> slice = data.Slice(byteIndex, byteLength);

				slice.CopyTo(data.Slice(byteIndex + byteLength, byteLength));
				otherData.Slice(chunkIndex * byteSize, byteLength).CopyTo(slice);

				while (otherTypeIndex < otherComponentTypes.Length && typeIndex < componentTypes.Length)
				{
					ComponentType otherComponentType = otherComponentTypes[otherTypeIndex];
					ComponentType componentType = componentTypes[typeIndex];

					switch (ComponentType.Compare(otherComponentType, componentType))
					{
						case -1:
							otherTypeIndex++;
							continue;
						case 1:
							byteSize = componentType.ByteSize;
							byteLength = length * byteSize;
							byteIndex = componentOffsets[typeIndex++] + index * byteSize;
							slice = data.Slice(byteIndex, byteLength);
							slice.CopyTo(data.Slice(byteIndex + byteLength, byteLength));
							slice.Clear();
							continue;
						default:
							Debug.Assert(otherComponentType == componentType);
							byteSize = componentType.ByteSize;
							byteLength = length * byteSize;
							byteIndex = componentOffsets[typeIndex++] + index * byteSize;
							slice = data.Slice(byteIndex, byteLength);
							slice.CopyTo(data.Slice(byteIndex + byteLength, byteLength));
							otherData.Slice(otherComponentOffsets[otherTypeIndex++] + chunkIndex * byteSize, byteLength)
								.CopyTo(slice);
							continue;
					}
				}

				while (typeIndex < componentTypes.Length)
				{
					byteSize = componentTypes[typeIndex].ByteSize;
					byteLength = length * byteSize;
					byteIndex = componentOffsets[typeIndex++] + index * byteSize;
					slice = data.Slice(byteIndex, byteLength);
					slice.CopyTo(data.Slice(byteIndex + byteLength, byteLength));
					slice.Clear();
				}
			}

			m_count = count + length;
			m_version++;
		}

		protected virtual void RemoveEntity(int index)
		{
			int count = m_count - 1;

			if ((uint)index > (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Index is less than zero or greater than or equal to Count.");
			}

			if (index < count)
			{
				ImmutableArray<ComponentType> componentTypes = m_archetype.ComponentTypes;
				ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
				Span<byte> data = m_data;
				int byteSize = Unsafe.SizeOf<Entity>();
				int copyLength = count - index;
				int byteLength = copyLength * byteSize;
				int byteIndex = index * byteSize;

				data.Slice(byteIndex + byteSize, byteLength).CopyTo(data.Slice(byteIndex, byteLength));

				for (int i = 0; i < componentTypes.Length; i++)
				{
					byteSize = componentTypes[i].ByteSize;
					byteLength = copyLength * byteSize;
					byteIndex = componentOffsets[i] + index * byteSize;

					data.Slice(byteIndex + byteSize, byteLength).CopyTo(data.Slice(byteIndex, byteLength));
				}
			}

			m_count = count;
			m_version++;
		}

		protected virtual void SetEntity(int index, Entity entity)
		{
			if ((uint)index >= (uint)m_count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Index is less than zero or greater than or equal to Count.");
			}

			Unsafe.As<byte, Entity>(ref MemoryMarshal.GetReference(new Span<byte>(
				m_data, index * Unsafe.SizeOf<Entity>(), Unsafe.SizeOf<Entity>()))) = entity;
			m_version++;
		}

		protected virtual void SetEntities(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			ArgumentNullException.ThrowIfNull(chunk);
			ArgumentOutOfRangeException.ThrowIfNegative(length);

			int count = m_count;
			int chunkCount = chunk.m_count;

			if ((uint)index > (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Index is less than zero or greater than Count.");
			}

			if ((uint)chunkIndex >= (uint)chunkCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Chunk index is less than zero or greater than or equal to the other EntityArchetypeChunk's Count.");
			}

			if (count - index < length)
			{
				throw new ArgumentOutOfRangeException(nameof(length), length,
					"Length exceeds the bounds of the EntityArchetypeChunk.");
			}

			if (chunkCount - chunkIndex < length)
			{
				throw new ArgumentOutOfRangeException(nameof(length), length,
					"Length exceeds the bounds of the other EntityArchetypeChunk.");
			}

			if (chunkCount > 0)
			{
				CopyFromEntityArchetypeChunk(index, chunk, chunkIndex, length);
				m_version++;
			}
		}

		private void CopyFromEntityArchetypeChunk(int index, EntityArchetypeChunk chunk, int chunkIndex, int length)
		{
			EntityArchetype otherArchetype = chunk.m_archetype;
			ImmutableArray<ComponentType> otherComponentTypes = otherArchetype.ComponentTypes;
			ImmutableArray<ComponentType> componentTypes = m_archetype.ComponentTypes;
			ImmutableArray<int> otherComponentOffsets = otherArchetype.ComponentOffsets;
			ImmutableArray<int> componentOffsets = m_archetype.ComponentOffsets;
			ReadOnlySpan<byte> otherData = chunk.m_data;
			Span<byte> data = m_data;
			int byteSize = Unsafe.SizeOf<Entity>();
			int byteLength = length * byteSize;
			int otherTypeIndex = 0;
			int typeIndex = 0;

			otherData.Slice(chunkIndex * byteSize, byteLength).CopyTo(data.Slice(index * byteSize, byteLength));

			while (otherTypeIndex < otherComponentTypes.Length && typeIndex < componentTypes.Length)
			{
				ComponentType otherComponentType = otherComponentTypes[otherTypeIndex];
				ComponentType componentType = componentTypes[typeIndex];

				switch (ComponentType.Compare(otherComponentType, componentType))
				{
					case -1:
						otherTypeIndex++;
						continue;
					case 1:
						byteSize = componentType.ByteSize;
						data.Slice(componentOffsets[typeIndex++] + index * byteSize, length * byteSize).Clear();
						continue;
					default:
						Debug.Assert(otherComponentType == componentType);
						byteSize = componentType.ByteSize;
						byteLength = length * byteSize;
						otherData.Slice(otherComponentOffsets[otherTypeIndex++] + chunkIndex * byteSize, byteLength)
							.CopyTo(data.Slice(componentOffsets[typeIndex++] + index * byteSize, byteLength));
						continue;
				}
			}

			while (typeIndex < componentTypes.Length)
			{
				byteSize = componentTypes[typeIndex].ByteSize;
				data.Slice(componentOffsets[typeIndex++] + index * byteSize, length * byteSize).Clear();
			}
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
				get
				{
					if (m_index == 0 || m_index == m_chunk.m_count + 1)
					{
						throw new InvalidOperationException(
							"Enumeration has not yet started or has already been completed.");
					}

					return m_current;
				}
			}

			public readonly void Dispose()
			{
			}

			public bool MoveNext()
			{
				EntityArchetypeChunk localChunk = m_chunk;

				if (m_index < localChunk.m_count && m_version == localChunk.m_version)
				{
					m_current = Unsafe.As<byte, Entity>(ref MemoryMarshal.GetReference(new Span<byte>(
						localChunk.m_data, m_index++ * Unsafe.SizeOf<Entity>(), Unsafe.SizeOf<Entity>())));
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
