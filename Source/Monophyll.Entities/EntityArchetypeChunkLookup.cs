using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities
{
	public class EntityArchetypeChunkLookup : ILookup<EntityArchetype, EntityArchetypeChunk>, IReadOnlyCollection<EntityArchetypeChunkGrouping>, ICollection
	{
		private const int DefaultCapacity = 16;
		private const int StackAllocUInt32BufferSizeLimit = 8;

		private readonly object m_syncLock;
		private EntityArchetype[] m_keys;
		private EntityArchetypeChunkGrouping[] m_groupings;
		private int m_size;

		public EntityArchetypeChunkLookup()
		{
			m_syncLock = new object();
			m_keys = [];
			m_groupings = [];
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

		public void CopyTo(EntityArchetypeChunkGrouping[] array, int arrayIndex)
		{
			Array.Copy(m_groupings, 0, array, arrayIndex, m_size);
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

			lock (m_syncLock)
			{
				index = BinarySearch(archetype.ComponentBits.AsSpan());

				if (index >= 0)
				{
					return m_groupings[m_keys[index].Id];
				}

				return CreateGrouping(archetype.Clone(m_size), ~index);
			}
		}

		public EntityArchetypeChunkGrouping GetOrCreate(params ComponentType[] componentTypes)
		{
			ArgumentNullException.ThrowIfNull(componentTypes);

			using ValueBitSet componentBits = new ValueBitSet(stackalloc uint[StackAllocUInt32BufferSizeLimit]);

			foreach (ComponentType componentType in componentTypes)
			{
				if (componentType != null)
				{
					componentBits.Set(componentType.Id);
				}
			}

			lock (m_syncLock)
			{
				int index = BinarySearch(componentBits.AsSpan());

				if (index >= 0)
				{
					return m_groupings[m_keys[index].Id];
				}

				return CreateGrouping(EntityArchetype.Create(componentTypes, m_size), ~index);
			}
		}

		public EntityArchetypeChunkGrouping GetOrCreate(IEnumerable<ComponentType> componentTypes)
		{
			_ = componentTypes.TryGetNonEnumeratedCount(out int count);
			ArrayPool<ComponentType> pool = ArrayPool<ComponentType>.Shared;
			ComponentType[] array = pool.Rent(count);
			using ValueBitSet componentBits = new ValueBitSet(stackalloc uint[StackAllocUInt32BufferSizeLimit]);

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
					componentBits.Set(componentType.Id);
				}
			}

			EntityArchetypeChunkGrouping grouping;

			lock (m_syncLock)
			{
				int index = BinarySearch(componentBits.AsSpan());
				grouping = index >= 0 ? m_groupings[m_keys[index].Id] : CreateGrouping(
					EntityArchetype.Create(new ReadOnlySpan<ComponentType>(array, 0, count), m_size), ~index);
			}

			pool.Return(array);
			return grouping;
		}

		public EntityArchetypeChunkGrouping GetOrCreate(ReadOnlySpan<ComponentType> componentTypes)
		{
			using ValueBitSet componentBits = new ValueBitSet(stackalloc uint[StackAllocUInt32BufferSizeLimit]);

			for (int i = 0; i < componentTypes.Length; i++)
			{
				ComponentType componentType = componentTypes[i];

				if (componentType != null)
				{
					componentBits.Set(componentType.Id);
				}
			}

			lock (m_syncLock)
			{
				int index = BinarySearch(componentBits.AsSpan());

				if (index >= 0)
				{
					return m_groupings[m_keys[index].Id];
				}

				return CreateGrouping(EntityArchetype.Create(componentTypes, m_size), ~index);
			}
		}

		public EntityArchetypeChunkGrouping GetOrCreateSubset(EntityArchetype archetype, ComponentType componentType)
		{
			throw new NotImplementedException();
		}

		public EntityArchetypeChunkGrouping GetOrCreateSuperset(EntityArchetype archetype, ComponentType componentType)
		{
			throw new NotImplementedException();
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

		private ref struct ValueBitSet
		{
			private Span<uint> m_span;
			private int m_length;
			private uint[]? m_array;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ValueBitSet(Span<uint> span)
			{
				m_span = span;
				m_length = 0;
				m_array = null;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ValueBitSet(uint[] array)
			{
				m_span = m_array = array;
				m_length = 0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				if (m_array != null)
				{
					ArrayPool<uint>.Shared.Return(m_array, true);
					m_span = default;
					m_length = 0;
					m_array = null;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Set(int index)
			{
				int spanIndex = index >> 5;

				if (spanIndex >= m_length)
				{
					Grow(spanIndex + 1);
				}

				m_span[spanIndex] |= 1u << index;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			private void Grow(int length)
			{
				Debug.Assert(length > m_length);

				if (length > m_span.Length)
				{
					ArrayPool<uint> pool = ArrayPool<uint>.Shared;
					uint[] newArray = pool.Rent(length);
					Span<uint> newSpan = newArray;

					m_span.CopyTo(newSpan);

					if (m_array != null)
					{
						pool.Return(m_array);
					}

					m_span = newSpan;
					m_array = newArray;
				}

				m_span[m_length..length].Clear();
				m_length = length;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public readonly ReadOnlySpan<uint> AsSpan()
			{
				return m_span[..m_length];
			}
		}
	}
}
