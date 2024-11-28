using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Monophyll.Entities
{
	public class EntityArchetypeChunkLookup : ILookup<EntityArchetype, EntityArchetypeChunk>, IReadOnlyList<EntityArchetypeChunkGrouping>, ICollection
	{
		private const int DefaultCapacity = 16;
		private const int DefaultChunkSize = 16 * 1024;
		private const int StackAllocUInt32BufferSizeLimit = 8;

		private readonly object m_lock;
		private EntityArchetype[] m_keys;
		private EntityArchetypeChunkGrouping[] m_groupings;
		private readonly int m_targetChunkSize;
		private int m_size;

		public EntityArchetypeChunkLookup() : this(DefaultChunkSize)
		{
		}

		public EntityArchetypeChunkLookup(int targetChunkSize)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(targetChunkSize);

			m_lock = new object();
			m_keys = Array.Empty<EntityArchetype>();
			m_groupings = Array.Empty<EntityArchetypeChunkGrouping>();
			m_targetChunkSize = targetChunkSize;
		}

		public int Capacity
		{
			get => m_keys.Length;
		}

		public int Count
		{
			get => m_size;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => this;
		}

		public EntityArchetypeChunkGrouping this[int index]
		{
			get
			{
				if ((uint)index >= (uint)m_size)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
						"Index is less than zero or greater than or equal to count.");
				}

				return m_groupings[index];
			}
		}

		public EntityArchetypeChunkGrouping this[EntityArchetype key]
		{
			get
			{
				ArgumentNullException.ThrowIfNull(key);

				int index = key.Id;
				EntityArchetypeChunkGrouping grouping;

				if ((uint)index >= (uint)m_size || !EntityArchetype.Equals(key, (grouping = m_groupings[index]).Key))
				{
					throw new KeyNotFoundException($"{key} does not exist within the EntityArchetypeChunkLookup.");
				}

				return grouping;
			}
		}

		IEnumerable<EntityArchetypeChunk> ILookup<EntityArchetype, EntityArchetypeChunk>.this[EntityArchetype key]
		{
			get => this[key];
		}

		public bool Contains(EntityArchetype key)
		{
			int index;
			return key != null
				&& (uint)(index = key.Id) < (uint)m_size
				&& EntityArchetype.Equals(key, m_groupings[index].Key);
		}

		public void CopyTo(EntityArchetypeChunkGrouping[] array, int index)
		{
			Array.Copy(m_groupings, 0, array, index, m_size);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException("The array is multi-dimensional.", nameof(array));
			}

			try
			{
				Array.Copy(m_groupings, 0, array!, index, m_size);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("The array cannot be assigned values of type EntityArchetypeChunkGrouping.", nameof(array));
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<IGrouping<EntityArchetype, EntityArchetypeChunk>> IEnumerable<IGrouping<EntityArchetype, EntityArchetypeChunk>>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<EntityArchetypeChunkGrouping> IEnumerable<EntityArchetypeChunkGrouping>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		public EntityArchetypeChunkGrouping GetOrCreate(EntityArchetype archetype)
		{
			ArgumentNullException.ThrowIfNull(archetype);

			int index = archetype.Id;
			EntityArchetypeChunkGrouping grouping;

			if ((uint)index < (uint)m_size && EntityArchetype.Equals(archetype, (grouping = m_groupings[index]).Key))
			{
				return grouping;
			}

			lock (m_lock)
			{
				index = BinarySearch(archetype.ComponentBits.AsSpan());

				if (index >= 0)
				{
					return m_groupings[m_keys[index].Id];
				}

				return CreateGrouping(archetype.Clone(m_targetChunkSize, m_size), ~index);
			}
		}

		public EntityArchetypeChunkGrouping GetOrCreate(ComponentType[] componentTypes)
		{
			ArgumentNullException.ThrowIfNull(componentTypes);

			ValueBitArray buffer = new ValueBitArray(stackalloc uint[StackAllocUInt32BufferSizeLimit]);

			foreach (ComponentType componentType in componentTypes)
			{
				if (componentType != null)
				{
					buffer.Set(componentType.Id);
				}
			}

			EntityArchetypeChunkGrouping grouping;

			lock (m_lock)
			{
				int index = BinarySearch(buffer.AsSpan());
				grouping = index >= 0 ?
						   m_groupings[m_keys[index].Id] :
						   CreateGrouping(EntityArchetype.Create(componentTypes, m_targetChunkSize, m_size), ~index);
			}

			buffer.Dispose();
			return grouping;
		}

		public EntityArchetypeChunkGrouping GetOrCreate(IEnumerable<ComponentType> componentTypes)
		{
			_ = componentTypes.TryGetNonEnumeratedCount(out int count);
			ArrayPool<ComponentType> pool = ArrayPool<ComponentType>.Shared;
			ComponentType[] array = pool.Rent(count);
			ValueBitArray buffer = new ValueBitArray(stackalloc uint[StackAllocUInt32BufferSizeLimit]);
			count = 0;

			foreach (ComponentType componentType in componentTypes)
			{
				if (componentType != null)
				{
					if (count >= array.Length)
					{
						ComponentType[] newArray = pool.Rent(count + 1);
						Array.Copy(array, newArray, count);
						pool.Return(array, true);
						array = newArray;
					}

					array[count++] = componentType;
					buffer.Set(componentType.Id);
				}
			}

			EntityArchetypeChunkGrouping grouping;

			lock (m_lock)
			{
				int index = BinarySearch(buffer.AsSpan());
				grouping = index >= 0 ?
						   m_groupings[m_keys[index].Id] :
						   CreateGrouping(EntityArchetype.Create(new ReadOnlySpan<ComponentType>(array, 0, count), m_targetChunkSize, m_size), ~index);
			}

			buffer.Dispose();
			pool.Return(array);
			return grouping;
		}

		public EntityArchetypeChunkGrouping GetOrCreate(ReadOnlySpan<ComponentType> componentTypes)
		{
			ValueBitArray buffer = new ValueBitArray(stackalloc uint[StackAllocUInt32BufferSizeLimit]);

			for (int i = 0; i < componentTypes.Length; i++)
			{
				ComponentType componentType = componentTypes[i];

				if (componentType != null)
				{
					buffer.Set(componentType.Id);
				}
			}

			EntityArchetypeChunkGrouping grouping;

			lock (m_lock)
			{
				int index = BinarySearch(buffer.AsSpan());
				grouping = index >= 0 ?
						   m_groupings[m_keys[index].Id] :
						   CreateGrouping(EntityArchetype.Create(componentTypes, m_targetChunkSize, m_size), ~index);
			}

			buffer.Dispose();
			return grouping;
		}

		public EntityArchetypeChunkGrouping GetOrCreateSubset(EntityArchetype archetype, ComponentType componentType)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			ArgumentNullException.ThrowIfNull(componentType);

			ImmutableArray<uint> componentBits = archetype.ComponentBits;
			int index = componentType.Id;
			int bufferIndex = index >> 5;
			uint[]? array = null;
			Span<uint> buffer = componentBits.Length <= StackAllocUInt32BufferSizeLimit ?
								stackalloc uint[componentBits.Length] :
								array = ArrayPool<uint>.Shared.Rent(componentBits.Length);

			componentBits.AsSpan().CopyTo(buffer);

			if (bufferIndex < buffer.Length && (buffer[bufferIndex] &= ~(1u << index)) == 0 && ++bufferIndex == buffer.Length)
			{
				buffer = buffer[..bufferIndex];
			}

			EntityArchetypeChunkGrouping grouping;

			lock (m_lock)
			{
				index = BinarySearch(buffer);
				grouping = index >= 0 ?
						   m_groupings[m_keys[index].Id] :
						   CreateGrouping(archetype.CloneWithout(componentType, m_targetChunkSize, m_size), ~index);
			}

			if (array != null)
			{
				ArrayPool<uint>.Shared.Return(array);
			}

			return grouping;
		}

		public EntityArchetypeChunkGrouping GetOrCreateSuperset(EntityArchetype archetype, ComponentType componentType)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			ArgumentNullException.ThrowIfNull(componentType);

			ImmutableArray<uint> componentBits = archetype.ComponentBits;
			ValueBitArray buffer = componentBits.Length <= StackAllocUInt32BufferSizeLimit ?
								   new ValueBitArray(stackalloc uint[componentBits.Length]) :
								   new ValueBitArray(componentBits.Length);

			componentBits.AsSpan().CopyTo(buffer.RawBits);
			buffer.Set(componentType.Id);

			EntityArchetypeChunkGrouping grouping;

			lock (m_lock)
			{
				int index = BinarySearch(buffer.AsSpan());
				grouping = index >= 0 ?
						   m_groupings[m_keys[index].Id] :
						   CreateGrouping(archetype.CloneWith(componentType, m_targetChunkSize, m_size), ~index);
			}

			buffer.Dispose();
			return grouping;
		}

		private int BinarySearch(ReadOnlySpan<uint> componentBits)
		{
			int low = 0;
			int high = m_size - 1;

			while (low <= high)
			{
				int index = low + ((high - low) >> 1);
				ImmutableArray<uint> archetypeBits = m_keys[index].ComponentBits;

				switch (archetypeBits.Length.CompareTo(componentBits.Length))
				{
					case -1:
						low = index + 1;
						continue;
					case 1:
						high = index - 1;
						continue;
					default:
						for (int i = componentBits.Length - 1; i >= 0; i--)
						{
							uint archetypeWord = archetypeBits[i];
							uint componentWord = componentBits[i];

							if (archetypeWord < componentWord)
							{
								goto case -1;
							}

							if (archetypeWord > componentWord)
							{
								goto case 1;
							}
						}

						return index;
				}
			}

			return ~low;
		}

		private EntityArchetypeChunkGrouping CreateGrouping(EntityArchetype key, int keyIndex)
		{
			if (m_size == m_keys.Length)
			{
				int capacity = m_size + 1;
				int newCapacity = m_keys.Length == 0 ? DefaultCapacity : 2 * m_keys.Length;

				if ((uint)newCapacity > (uint)Array.MaxLength)
				{
					newCapacity = Array.MaxLength;
				}

				if (newCapacity < capacity)
				{
					newCapacity = capacity;
				}

				Array.Resize(ref m_keys, newCapacity);
				Array.Resize(ref m_groupings, newCapacity);
			}

			if (keyIndex < m_size)
			{
				Array.Copy(m_keys, keyIndex, m_keys, keyIndex + 1, m_size - keyIndex);
			}

			m_keys[keyIndex] = key;
			return m_groupings[m_size++] = new EntityArchetypeChunkGrouping(key);
		}

		public bool TryGetValue(EntityArchetype key, [MaybeNullWhen(false)] out EntityArchetypeChunkGrouping value)
		{
			int index;

			if (key != null && (uint)(index = key.Id) < (uint)m_size &&
				EntityArchetype.Equals(key, (value = m_groupings[index]).Key))
			{
				return true;
			}

			value = null;
			return false;
		}

		private ref struct ValueBitArray
		{
			private uint[]? m_rentedArray;
			private Span<uint> m_bits;
			private int m_size;

			public ValueBitArray(Span<uint> span)
			{
				m_rentedArray = null;
				m_bits = span;
				m_size = 0;
			}

			public ValueBitArray(int capacity)
			{
				m_rentedArray = ArrayPool<uint>.Shared.Rent(capacity);
				m_bits = new Span<uint>(m_rentedArray);
				m_size = 0;
			}

			public readonly Span<uint> RawBits
			{
				get => m_bits;
			}

			public void Dispose()
			{
				uint[]? arrayToReturn = m_rentedArray;

				if (arrayToReturn != null)
				{
					this = default;
					ArrayPool<uint>.Shared.Return(arrayToReturn);
				}
			}

			public void Set(int index)
			{
				int spanIndex = index >> 5;

				if (spanIndex >= m_size)
				{
					Grow(spanIndex + 1);
				}

				m_bits[spanIndex] |= 1u << index;
			}

			private void Grow(int capacity)
			{
				if (capacity > m_bits.Length)
				{
					uint[]? rentedArray = m_rentedArray;
					uint[] array = ArrayPool<uint>.Shared.Rent(capacity);

					m_bits.CopyTo(array);

					if (rentedArray != null)
					{
						ArrayPool<uint>.Shared.Return(rentedArray);
					}

					m_bits = new Span<uint>(m_rentedArray = array);
				}

				m_bits.Slice(m_size, capacity).Clear();
				m_size = capacity;
			}

			public readonly ReadOnlySpan<uint> AsSpan()
			{
				return m_bits.Slice(0, m_size);
			}
		}

		public struct Enumerator : IEnumerator<EntityArchetypeChunkGrouping>
		{
			private readonly EntityArchetypeChunkLookup m_lookup;
			private readonly int m_count;
			private int m_index;
			private EntityArchetypeChunkGrouping? m_current;

			internal Enumerator(EntityArchetypeChunkLookup lookup)
			{
				m_lookup = lookup;
				m_count = lookup.m_size;
				m_index = 0;
				m_current = null;
			}

			public readonly EntityArchetypeChunkGrouping Current
			{
				get => m_current!;
			}

			readonly object IEnumerator.Current
			{
				get => m_current!;
			}

			public readonly void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (m_index < m_count)
				{
					m_current = m_lookup.m_groupings[m_index++];
					return true;
				}

				return false;
			}

			void IEnumerator.Reset()
			{
				m_index = 0;
				m_current = null;
			}
		}
	}
}
