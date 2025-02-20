using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Monophyll.Entities
{
	public class EntityArchetypeLookup : ILookup<EntityArchetype, EntityArchetypeChunk>, IReadOnlyList<EntityArchetypeGrouping>, ICollection
	{
		private const int DefaultCapacity = 8;

		private readonly object m_lock;
		private volatile Table m_table;

		public EntityArchetypeLookup()
		{
			m_lock = new object();
			m_table = new Table();
		}

		public int Capacity
		{
			get => m_table.Capacity;
		}

		public int Count
		{
			get => m_table.Count;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => this;
		}

		public EntityArchetypeGrouping this[int index]
		{
			get => m_table[index];
		}

		IEnumerable<EntityArchetypeChunk> ILookup<EntityArchetype, EntityArchetypeChunk>.this[EntityArchetype key]
		{
			get => m_table.FindGrouping(key) ?? Enumerable.Empty<EntityArchetypeChunk>();
		}

		public bool Contains(EntityArchetype key)
		{
			return m_table.FindGrouping(key) != null;
		}

		public void CopyTo(EntityArchetypeGrouping[] array, int index)
		{
			m_table.CopyTo(array, index);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			m_table.CopyTo(array, index);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<IGrouping<EntityArchetype, EntityArchetypeChunk>> IEnumerable<IGrouping<EntityArchetype, EntityArchetypeChunk>>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<EntityArchetypeGrouping> IEnumerable<EntityArchetypeGrouping>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		public EntityArchetypeGrouping GetGrouping(ComponentType[] componentTypes)
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

				return GetGrouping(new ReadOnlySpan<ComponentType>(componentTypes), buffer.AsSpan());
			}
			finally
			{
				buffer.Dispose();
			}
		}

		public EntityArchetypeGrouping GetGrouping(IEnumerable<ComponentType> componentTypes)
		{
			ComponentType[] array = componentTypes.TryGetNonEnumeratedCount(out int count) ?
									ArrayPool<ComponentType>.Shared.Rent(count) :
									Array.Empty<ComponentType>();
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

		public EntityArchetypeGrouping GetGrouping(ReadOnlySpan<ComponentType> componentTypes)
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

		private EntityArchetypeGrouping GetGrouping(ReadOnlySpan<ComponentType> componentTypes, ReadOnlySpan<uint> componentBits)
		{
			EntityArchetypeGrouping? grouping = m_table.FindGrouping(componentBits);

			if (grouping == null)
			{
				lock (m_lock)
				{
					Table table = m_table;

					if ((grouping = table.FindGrouping(componentBits)) == null)
					{
						if (table.Isfull)
						{
							m_table = table = table.Grow();
						}

						grouping = new EntityArchetypeGrouping(EntityArchetype.Create(componentTypes, table.Count));
						table.AddGrouping(grouping);
					}
				}
			}

			return grouping;
		}

		public EntityArchetypeGrouping GetGrouping(EntityArchetype archetype)
		{
			EntityArchetypeGrouping? grouping = m_table.FindGrouping(archetype);

			if (grouping == null)
			{
				lock (m_lock)
				{
					Table table = m_table;

					if ((grouping = table.FindGrouping(archetype)) == null)
					{
						if (table.Isfull)
						{
							m_table = table = table.Grow();
						}

						grouping = new EntityArchetypeGrouping(archetype.Clone(table.Count));
						table.AddGrouping(grouping);
					}
				}
			}

			return grouping;
		}

		public EntityArchetypeGrouping GetSubgrouping(EntityArchetype archetype, ComponentType componentType)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			ArgumentNullException.ThrowIfNull(componentType);

			ImmutableArray<uint> componentBits = archetype.ComponentBits;
			int index = componentType.Id >> 5;
			uint bit = 1u << componentType.Id;

			if (index >= componentBits.Length || (bit & componentBits[index]) == 0)
			{
				return GetGrouping(archetype);
			}

			uint[]? rentedArray = null;
			Span<uint> key = componentBits.Length > DefaultCapacity ?
							 new Span<uint>(rentedArray = ArrayPool<uint>.Shared.Rent(componentBits.Length), 0, componentBits.Length) :
							 stackalloc uint[componentBits.Length];

			try
			{
				componentBits.CopyTo(key);

				if ((key[index] ^= bit) == 0)
				{
					key = key.Slice(0, archetype.ComponentTypes.Length > 1 ? archetype.ComponentTypes[^2].Id >> 5 : 0);
				}

				EntityArchetypeGrouping? grouping = m_table.FindGrouping(key);

				if (grouping == null)
				{
					lock (m_lock)
					{
						Table table = m_table;

						if ((grouping = table.FindGrouping(key)) == null)
						{
							if (table.Isfull)
							{
								m_table = table = table.Grow();
							}

							grouping = new EntityArchetypeGrouping(archetype.CloneWithout(componentType, table.Count));
							table.AddGrouping(grouping);
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

		public EntityArchetypeGrouping GetSupergrouping(EntityArchetype archetype, ComponentType componentType)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			ArgumentNullException.ThrowIfNull(componentType);

			ImmutableArray<uint> componentBits = archetype.ComponentBits;
			int index = componentType.Id >> 5;
			uint bit = 1u << componentType.Id;

			if (index < componentBits.Length && (componentBits[index] & bit) != 0)
			{
				return GetGrouping(archetype);
			}

			uint[]? rentedArray = null;
			int length = Math.Max(index + 1, componentBits.Length);
			Span<uint> key = length > DefaultCapacity ?
							 new Span<uint>(rentedArray = ArrayPool<uint>.Shared.Rent(length), 0, length) :
							 stackalloc uint[length];

			try
			{
				componentBits.CopyTo(key);
				key.Slice(componentBits.Length).Clear();
				key[index] |= bit;

				EntityArchetypeGrouping? grouping = m_table.FindGrouping(key);

				if (grouping == null)
				{
					lock (m_lock)
					{
						Table table = m_table;

						if ((grouping = table.FindGrouping(key)) == null)
						{
							if (table.Isfull)
							{
								m_table = table = table.Grow();
							}

							grouping = new EntityArchetypeGrouping(archetype.CloneWith(componentType, table.Count));
							table.AddGrouping(grouping);
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

		public bool TryGetGrouping(EntityArchetype key, out EntityArchetypeGrouping? grouping)
		{
			return (grouping = m_table.FindGrouping(key)) != null;
		}

		public struct Enumerator : IEnumerator<EntityArchetypeGrouping>
		{
			private readonly Table m_table;
			private readonly int m_count;
			private int m_index;
			private EntityArchetypeGrouping? m_current;

			internal Enumerator(EntityArchetypeLookup lookup)
			{
				m_table = lookup.m_table;
				m_count = m_table.Count;
				m_index = 0;
				m_current = null;
			}

			public readonly EntityArchetypeGrouping Current
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
				Table table = m_table;

				if ((uint)m_index < (uint)table.Count)
				{
					m_current = table[m_index++];
					return true;
				}

				m_current = null;
				return false;
			}

			void IEnumerator.Reset()
			{
				m_current = null;
				m_index = 0;
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

		private sealed class Table
		{
			private readonly int[] m_buckets;
			private readonly Entry[] m_entries;
			private int m_count;

			public Table()
			{
				m_buckets = new int[DefaultCapacity];
				m_entries = new Entry[DefaultCapacity];
			}

			private Table(int capacity, int count)
			{
				m_buckets = new int[capacity];
				m_entries = new Entry[capacity];
				m_count = count;
			}

			public EntityArchetypeGrouping this[int index]
			{
				get
				{
					if ((uint)index >= (uint)m_count)
					{
						throw new ArgumentOutOfRangeException(nameof(index), index, "");
					}

					return m_entries[index].Grouping;
				}
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

			private static int GetHashCode(ReadOnlySpan<uint> key)
			{
				int result = 0;

				for (int i = key.Length > 8 ? key.Length - 8 : 0; i < key.Length; i++)
				{
					result = ((result << 5) + result) ^ (int)key[i];
				}

				return result & int.MaxValue;
			}

			public void AddGrouping(EntityArchetypeGrouping grouping)
			{
				int hashCode = GetHashCode(grouping.Key.ComponentBits.AsSpan());
				ref int bucket = ref m_buckets[hashCode & (m_buckets.Length - 1)];
				ref Entry entry = ref m_entries[m_count];

				entry.Grouping = grouping;
				entry.HashCode = hashCode;
				entry.Next = bucket;

				Volatile.Write(ref bucket, ~m_count++);
			}

			public void CopyTo(EntityArchetypeGrouping[] array, int index)
			{
				ArgumentNullException.ThrowIfNull(array);

				if ((uint)index > (uint)array.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index, "");
				}

				if (array.Length - index < m_count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index, "");
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

				if (array.Rank != 1)
				{
					throw new ArgumentException();
				}

				if (array.GetLowerBound(0) != 0)
				{
					throw new ArgumentException();
				}

				if ((uint)index > (uint)array.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index, "");
				}

				if (array.Length - index < m_count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index, "");
				}

				if (array is EntityArchetypeGrouping[] groupings)
				{
					CopyTo(groupings, index);
				}
				else if (array is object[] objects)
				{
					try
					{
						int count = m_count;

						for (int i = 0; i < count; i++)
						{
							objects[index++] = m_entries[i].Grouping;
						}
					}
					catch (ArrayTypeMismatchException)
					{
						throw new ArgumentException();
					}
				}
				else
				{
					throw new ArgumentException();
				}
			}

			public EntityArchetypeGrouping? FindGrouping(EntityArchetype archetype)
			{
				ArgumentNullException.ThrowIfNull(archetype);

				EntityArchetypeGrouping grouping;

				if ((uint)archetype.Id < (uint)m_count
					&& EntityArchetype.Equals((grouping = m_entries[archetype.Id].Grouping).Key, archetype))
				{
					return grouping;
				}

				return FindGrouping(archetype.ComponentBits.AsSpan());
			}

			public EntityArchetypeGrouping? FindGrouping(ReadOnlySpan<uint> key)
			{
				ref Entry entry = ref Unsafe.NullRef<Entry>();
				int hashCode = GetHashCode(key);

				for (int i = Volatile.Read(ref m_buckets[hashCode & (m_buckets.Length - 1)]); i < 0; i = entry.Next)
				{
					EntityArchetypeGrouping grouping = (entry = ref m_entries[~i]).Grouping;

					if (entry.HashCode == hashCode && grouping.Key.ComponentBits.AsSpan().SequenceEqual(key))
					{
						return grouping;
					}
				}

				return null;
			}

			public Table Grow()
			{
				Table newTable = new Table(m_entries.Length * 2, m_count);

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
				public EntityArchetypeGrouping Grouping;
				public int HashCode;
				public int Next;
			}
		}
	}
}
