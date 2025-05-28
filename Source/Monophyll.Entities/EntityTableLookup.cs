using Monophyll.Utilities;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
	public class EntityTableLookup : ILookup<EntityArchetype, EntityTable>, IReadOnlyList<EntityTableGrouping>, ICollection
	{
		private const int DefaultCapacity = 8;

		private readonly object m_lock;
		private volatile Container m_container;

		public EntityTableLookup()
		{
			m_lock = new object();
			m_container = new Container();
		}

		public int Capacity
		{
			get => m_container.Capacity;
		}

		public int Count
		{
			get => m_container.Count;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => this;
		}

		public EntityTableGrouping this[int index]
		{
			get
			{
				Container container = m_container;

                if ((uint)index >= (uint)container.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Index was out of range. Must be non-negative and less than the size of the EntityTableLookup.");
                }

				return container[index];
            }
		}

		IEnumerable<EntityTable> ILookup<EntityArchetype, EntityTable>.this[EntityArchetype key]
		{
			get
			{
				ArgumentNullException.ThrowIfNull(key);
				return m_container.FindGrouping(key.ComponentBits) ?? Enumerable.Empty<EntityTable>();
			}
		}

		public bool Contains(EntityArchetype key)
        {
            ArgumentNullException.ThrowIfNull(key);
            return m_container.FindGrouping(key.ComponentBits) != null;
		}

		public void CopyTo(EntityTableGrouping[] array, int index)
		{
			m_container.CopyTo(array, index);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			m_container.CopyTo(array, index);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<IGrouping<EntityArchetype, EntityTable>> IEnumerable<IGrouping<EntityArchetype, EntityTable>>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<EntityTableGrouping> IEnumerable<EntityTableGrouping>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		public EntityTableGrouping GetGrouping(ComponentType[] componentTypes)
		{
			ArgumentNullException.ThrowIfNull(componentTypes);

			ValueBitArray buffer = new ValueBitArray(stackalloc uint[DefaultCapacity]);

			try
			{
				foreach (ComponentType componentType in componentTypes)
				{
					if (componentType != null)
					{
						buffer.Set(componentType.Id);
					}
				}

				return GetGrouping(componentTypes, buffer.AsSpan());
			}
			finally
			{
				buffer.Dispose();
			}
		}

		public EntityTableGrouping GetGrouping(IEnumerable<ComponentType> componentTypes)
		{
			ComponentType[] array = componentTypes.TryGetNonEnumeratedCount(out int count)
				? ArrayPool<ComponentType>.Shared.Rent(count)
				: Array.Empty<ComponentType>();
			ValueBitArray buffer = new ValueBitArray(stackalloc uint[DefaultCapacity]);
			count = 0;

			try
			{
				foreach (ComponentType componentType in componentTypes)
				{
					if (componentType != null)
					{
						if (count >= array.Length)
						{
							ComponentType[] newArray = ArrayPool<ComponentType>.Shared.Rent(count + 1);
							Array.Copy(array, newArray, count);
							ArrayPool<ComponentType>.Shared.Return(array, true);
							array = newArray;
						}

						array[count++] = componentType;
						buffer.Set(componentType.Id);
					}
				}

				return GetGrouping(new ReadOnlySpan<ComponentType>(array, 0, count), buffer.AsSpan());
			}
			finally
			{
				ArrayPool<ComponentType>.Shared.Return(array, true);
				buffer.Dispose();
			}
		}

		public EntityTableGrouping GetGrouping(ReadOnlySpan<ComponentType> componentTypes)
		{
			ValueBitArray buffer = new ValueBitArray(stackalloc uint[DefaultCapacity]);

			try
			{
				for (int i = 0; i < componentTypes.Length; i++)
				{
					ComponentType componentType = componentTypes[i];

					if (componentType != null)
					{
						buffer.Set(componentType.Id);
					}
				}

				return GetGrouping(componentTypes, buffer.AsSpan());
			}
			finally
			{
				buffer.Dispose();
			}
		}

		private EntityTableGrouping GetGrouping(ReadOnlySpan<ComponentType> componentTypes, ReadOnlySpan<uint> componentBits)
		{
			EntityTableGrouping? grouping = m_container.FindGrouping(componentBits);

			if (grouping == null)
			{
				lock (m_lock)
				{
					Container container = m_container;

					if ((grouping = container.FindGrouping(componentBits)) == null)
					{
						if (container.Isfull)
						{
							m_container = container = container.Grow();
						}

						grouping = new EntityTableGrouping(EntityArchetype.Create(componentTypes));
						container.AddGrouping(grouping);
					}
				}
			}

			return grouping;
		}

		public EntityTableGrouping GetGrouping(EntityArchetype archetype)
		{
			ArgumentNullException.ThrowIfNull(archetype);

			EntityTableGrouping? grouping = m_container.FindGrouping(archetype.ComponentBits);

			if (grouping == null)
			{
				lock (m_lock)
				{
					Container container = m_container;

					if ((grouping = container.FindGrouping(archetype.ComponentBits)) == null)
					{
						if (container.Isfull)
						{
							m_container = container = container.Grow();
						}

						grouping = new EntityTableGrouping(archetype);
						container.AddGrouping(grouping);
					}
				}
			}

			return grouping;
		}

		public EntityTableGrouping GetSubgrouping(EntityArchetype archetype, ComponentType componentType)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			ArgumentNullException.ThrowIfNull(componentType);

			ReadOnlySpan<uint> componentBits = archetype.ComponentBits;
			int index = componentType.Id >> 5;
			uint bit = 1u << componentType.Id;

			if (index >= componentBits.Length || (bit & componentBits[index]) == 0)
			{
				return GetGrouping(archetype);
			}

			uint[]? rentedArray;
			scoped Span<uint> key;
			
			if (componentBits.Length > DefaultCapacity)
			{
				rentedArray = ArrayPool<uint>.Shared.Rent(componentBits.Length);
				key = new Span<uint>(rentedArray, 0, componentBits.Length);
            }
			else
			{
				rentedArray = null;
				key = stackalloc uint[componentBits.Length];
            }

			try
			{
				componentBits.CopyTo(key);

				if ((key[index] ^= bit) == 0)
				{
					ReadOnlySpan<ComponentType> componentTypes = archetype.ComponentTypes;

                    key = key.Slice(0, componentTypes.Length > 1 ? componentTypes[^2].Id >> 5 : 0);
				}

				EntityTableGrouping? grouping = m_container.FindGrouping(key);

				if (grouping == null)
				{
					lock (m_lock)
					{
						Container container = m_container;

						if ((grouping = container.FindGrouping(key)) == null)
						{
							if (container.Isfull)
							{
								m_container = container = container.Grow();
							}

							grouping = new EntityTableGrouping(archetype.Remove(componentType));
							container.AddGrouping(grouping);
						}
					}
				}

				return grouping;
			}
			finally
			{
				if (rentedArray != null)
				{
					ArrayPool<uint>.Shared.Return(rentedArray);
				}
			}
		}

		public EntityTableGrouping GetSupergrouping(EntityArchetype archetype, ComponentType componentType)
        {
            ArgumentNullException.ThrowIfNull(archetype);
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<uint> componentBits = archetype.ComponentBits;
			int index = componentType.Id >> 5;
			uint bit = 1u << componentType.Id;

			if (index < componentBits.Length && (componentBits[index] & bit) != 0)
			{
				return GetGrouping(archetype);
			}

			uint[]? rentedArray;
			scoped Span<uint> key;
            int length = Math.Max(index + 1, componentBits.Length);

            if (length > DefaultCapacity)
            {
                rentedArray = ArrayPool<uint>.Shared.Rent(length);
                key = new Span<uint>(rentedArray, 0, length);
            }
            else
            {
                rentedArray = null;
                key = stackalloc uint[length];
            }

            try
			{
				componentBits.CopyTo(key);
				key.Slice(componentBits.Length).Clear();
				key[index] |= bit;

				EntityTableGrouping? grouping = m_container.FindGrouping(key);

				if (grouping == null)
				{
					lock (m_lock)
					{
						Container container = m_container;

						if ((grouping = container.FindGrouping(key)) == null)
						{
							if (container.Isfull)
							{
								m_container = container = container.Grow();
							}

							grouping = new EntityTableGrouping(archetype.Add(componentType));
							container.AddGrouping(grouping);
						}
					}
				}

				return grouping;
			}
			finally
			{
				if (rentedArray != null)
				{
					ArrayPool<uint>.Shared.Return(rentedArray);
				}
			}
		}

		public bool TryGetGrouping(EntityArchetype key, [NotNullWhen(true)] out EntityTableGrouping? grouping)
		{
			ArgumentNullException.ThrowIfNull(key);
			return (grouping = m_container.FindGrouping(key.ComponentBits)) != null;
		}

		public struct Enumerator : IEnumerator<EntityTableGrouping>
		{
			private readonly Container m_container;
			private readonly int m_count;
			private int m_index;

			internal Enumerator(EntityTableLookup lookup)
			{
				m_container = lookup.m_container;
				m_count = m_container.Count;
				m_index = -1;
			}

			public readonly EntityTableGrouping Current
			{
				get => m_container[m_index];
			}

			readonly object IEnumerator.Current
            {
                get => m_container[m_index];
            }

			public readonly void Dispose()
			{
			}

			public bool MoveNext()
			{
				int index = m_index + 1;

				if (index < m_count)
				{
					m_index = index;
					return true;
				}

				return false;
			}

			void IEnumerator.Reset()
			{
				m_index = -1;
			}
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

					m_bits = m_rentedArray = array;
				}

				m_bits.Slice(m_size, capacity).Clear();
				m_size = capacity;
			}

			public readonly ReadOnlySpan<uint> AsSpan()
			{
				return m_bits.Slice(0, m_size);
			}
		}

		private sealed class Container
		{
			private readonly int[] m_buckets;
			private readonly Entry[] m_entries;
			private int m_count;

			public Container()
			{
				m_buckets = new int[DefaultCapacity];
				m_entries = new Entry[DefaultCapacity];
			}

			private Container(int capacity, int count)
			{
				m_buckets = new int[capacity];
				m_entries = new Entry[capacity];
				m_count = count;
			}

			public EntityTableGrouping this[int index]
			{
				get => m_entries[index].Grouping;
			}

			public int Capacity
			{
				get => m_entries.Length;
			}

			public int Count
			{
				get => m_count;
			}

			public bool Isfull
			{
				get => m_count == m_entries.Length;
			}

			public void AddGrouping(EntityTableGrouping grouping)
			{
				int hashCode = BitSetOperations.GetHashCode(grouping.Key.ComponentBits) & int.MaxValue;
				ref int bucket = ref m_buckets[hashCode & (m_buckets.Length - 1)];
				ref Entry entry = ref m_entries[m_count];

				entry.Grouping = grouping;
				entry.HashCode = hashCode;
				entry.Next = bucket;

				Volatile.Write(ref bucket, ~m_count++);
			}

			public void CopyTo(EntityTableGrouping[] array, int index)
			{
				ArgumentNullException.ThrowIfNull(array);

                if ((uint)index >= (uint)array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Index was out of range. Must be non-negative and less than the length of the array.");
                }

                if (array.Length - index < m_count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
						"Count exceeds the length of the array.");
				}

				int count = m_count;

				for (int i = 0; i < count; i++)
				{
					array[index++] = m_entries[i].Grouping;
				}
			}

			public void CopyTo(Array array, int index)
            {
                ArgumentNullException.ThrowIfNull(array);

                if (array is EntityTableGrouping[] groupings)
                {
                    CopyTo(groupings, index);
					return;
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentException(
						"Multi-dimensional arrays are not supported.", nameof(array));
                }

				if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException(
						"Arrays with non-zero lower bounds are not supported.", nameof(array));
                }

				if ((uint)index > (uint)array.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Index was out of range. Must be non-negative and less than the length of the array.");
                }

				if (array.Length - index < m_count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Count exceeds the length of the array.");
                }

				if (array is not object[] objects)
                {
                    throw new ArgumentException(
                        "Array is not of type EntityTableGrouping[].", nameof(array));
				}

                try
                {
                    for (int i = 0, count = m_count; i < count; i++)
                    {
                        objects[index++] = m_entries[i].Grouping;
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException(
                        "Array is not of type EntityTableGrouping[].", nameof(array));
                }
            }

			public EntityTableGrouping? FindGrouping(ReadOnlySpan<uint> key)
			{
				ref Entry entry = ref Unsafe.NullRef<Entry>();
				int hashCode = BitSetOperations.GetHashCode(key) & int.MaxValue;

				for (int i = Volatile.Read(ref m_buckets[hashCode & (m_buckets.Length - 1)]); i < 0; i = entry.Next)
				{
					EntityTableGrouping grouping = (entry = ref m_entries[~i]).Grouping;

					if (entry.HashCode == hashCode && grouping.Key.ComponentBits.SequenceEqual(key))
					{
						return grouping;
					}
				}

				return null;
			}

			public Container Grow()
			{
				Container newTable = new Container(m_entries.Length * 2, m_count);

				for (int i = 0; i < m_count; i++)
				{
					ref Entry oldEntry = ref m_entries[i];
					ref Entry newEntry = ref newTable.m_entries[i];
					ref int newBucket = ref newTable.m_buckets[oldEntry.HashCode & (newTable.m_buckets.Length - 1)];

					newEntry.Grouping = oldEntry.Grouping;
					newEntry.HashCode = oldEntry.HashCode;
					newEntry.Next = newBucket;
					newBucket = ~i;
				}

				return newTable;
			}

			private struct Entry
			{
				public EntityTableGrouping Grouping;
				public int HashCode;
				public int Next;
			}
		}
	}
}
