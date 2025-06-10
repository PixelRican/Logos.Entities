// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

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
                return m_container.Find(key.ComponentBitmask) ?? Enumerable.Empty<EntityTable>();
            }
        }

        public bool Contains(EntityArchetype key)
        {
            ArgumentNullException.ThrowIfNull(key);
            return m_container.Find(key.ComponentBitmask) != null;
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
            
            foreach (ComponentType componentType in componentTypes)
            {
                if (componentType != null)
                {
                    buffer.Set(componentType.ID);
                }
            }

            EntityTableGrouping grouping = GetGrouping(buffer.AsSpan(), componentTypes);
            buffer.Dispose();
            return grouping;
        }

        public EntityTableGrouping GetGrouping(IEnumerable<ComponentType> componentTypes)
        {
            ComponentType[] array = componentTypes.TryGetNonEnumeratedCount(out int count)
                ? ArrayPool<ComponentType>.Shared.Rent(count)
                : Array.Empty<ComponentType>();
            ValueBitArray buffer = new ValueBitArray(stackalloc uint[DefaultCapacity]);
            count = 0;

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
                    buffer.Set(componentType.ID);
                }
            }

            EntityTableGrouping grouping = GetGrouping(buffer.AsSpan(),
                new ReadOnlySpan<ComponentType>(array, 0, count));
            ArrayPool<ComponentType>.Shared.Return(array, clearArray: true);
            buffer.Dispose();
            return grouping;
        }

        public EntityTableGrouping GetGrouping(ReadOnlySpan<ComponentType> componentTypes)
        {
            ValueBitArray buffer = new ValueBitArray(stackalloc uint[DefaultCapacity]);

            for (int i = 0; i < componentTypes.Length; i++)
            {
                ComponentType componentType = componentTypes[i];

                if (componentType != null)
                {
                    buffer.Set(componentType.ID);
                }
            }

            EntityTableGrouping grouping = GetGrouping(buffer.AsSpan(), componentTypes);
            buffer.Dispose();
            return grouping;
        }

        private EntityTableGrouping GetGrouping(ReadOnlySpan<uint> key, ReadOnlySpan<ComponentType> value)
        {
            EntityTableGrouping? grouping = m_container.Find(key);

            if (grouping == null)
            {
                lock (m_lock)
                {
                    Container container = m_container;
                    grouping = container.Find(key);

                    if (grouping == null)
                    {
                        if (container.Isfull)
                        {
                            m_container = container = container.Grow();
                        }

                        grouping = new EntityTableGrouping(EntityArchetype.Create(value));
                        container.Add(grouping);
                    }
                }
            }

            return grouping;
        }

        public EntityTableGrouping GetGrouping(EntityArchetype archetype)
        {
            ArgumentNullException.ThrowIfNull(archetype);

            EntityTableGrouping? grouping = m_container.Find(archetype.ComponentBitmask);

            if (grouping == null)
            {
                lock (m_lock)
                {
                    Container container = m_container;

                    if ((grouping = container.Find(archetype.ComponentBitmask)) == null)
                    {
                        if (container.Isfull)
                        {
                            m_container = container = container.Grow();
                        }

                        grouping = new EntityTableGrouping(archetype);
                        container.Add(grouping);
                    }
                }
            }

            return grouping;
        }

        public EntityTableGrouping GetSubgrouping(EntityArchetype archetype, ComponentType componentType)
        {
            ArgumentNullException.ThrowIfNull(archetype);
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<uint> componentBitmask = archetype.ComponentBitmask;
            int index = componentType.ID >> 5;
            uint bit = 1u << componentType.ID;

            if (index >= componentBitmask.Length || (bit & componentBitmask[index]) == 0)
            {
                return GetGrouping(archetype);
            }

            uint[]? rentedArray;
            scoped Span<uint> key;
            
            if (componentBitmask.Length <= DefaultCapacity)
            {
                rentedArray = null;
                key = stackalloc uint[componentBitmask.Length];
            }
            else
            {
                rentedArray = ArrayPool<uint>.Shared.Rent(componentBitmask.Length);
                key = new Span<uint>(rentedArray, 0, componentBitmask.Length);
            }

            componentBitmask.CopyTo(key);

            if ((key[index] ^= bit) == 0)
            {
                ReadOnlySpan<ComponentType> componentTypes = archetype.ComponentTypes;
                key = key.Slice(0, componentTypes.Length > 1 ? componentTypes[^2].ID + 32 >> 5 : 0);
            }

            EntityTableGrouping? grouping = m_container.Find(key);

            if (grouping == null)
            {
                lock (m_lock)
                {
                    Container container = m_container;

                    if ((grouping = container.Find(key)) == null)
                    {
                        if (container.Isfull)
                        {
                            m_container = container = container.Grow();
                        }

                        grouping = new EntityTableGrouping(archetype.Remove(componentType));
                        container.Add(grouping);
                    }
                }
            }

            if (rentedArray != null)
            {
                ArrayPool<uint>.Shared.Return(rentedArray);
            }

            return grouping;
        }

        public EntityTableGrouping GetSupergrouping(EntityArchetype archetype, ComponentType componentType)
        {
            ArgumentNullException.ThrowIfNull(archetype);
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<uint> componentBitmask = archetype.ComponentBitmask;
            int index = componentType.ID >> 5;
            uint bit = 1u << componentType.ID;

            if (index < componentBitmask.Length && (componentBitmask[index] & bit) != 0)
            {
                return GetGrouping(archetype);
            }

            uint[]? rentedArray;
            scoped Span<uint> key;
            int length = Math.Max(index + 1, componentBitmask.Length);

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

            componentBitmask.CopyTo(key);
            key.Slice(componentBitmask.Length).Clear();
            key[index] |= bit;

            EntityTableGrouping? grouping = m_container.Find(key);

            if (grouping == null)
            {
                lock (m_lock)
                {
                    Container container = m_container;

                    if ((grouping = container.Find(key)) == null)
                    {
                        if (container.Isfull)
                        {
                            m_container = container = container.Grow();
                        }

                        grouping = new EntityTableGrouping(archetype.Add(componentType));
                        container.Add(grouping);
                    }
                }
            }

            if (rentedArray != null)
            {
                ArrayPool<uint>.Shared.Return(rentedArray);
            }

            return grouping;
        }

        public bool TryGetGrouping(EntityArchetype key, [NotNullWhen(true)] out EntityTableGrouping? grouping)
        {
            ArgumentNullException.ThrowIfNull(key);
            return (grouping = m_container.Find(key.ComponentBitmask)) != null;
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

            public readonly void Dispose()
            {
                uint[]? arrayToReturn = m_rentedArray;

                if (arrayToReturn != null)
                {
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
                    uint[] newArray = ArrayPool<uint>.Shared.Rent(capacity);

                    m_bits.CopyTo(newArray);

                    if (rentedArray != null)
                    {
                        ArrayPool<uint>.Shared.Return(rentedArray);
                    }

                    m_bits = m_rentedArray = newArray;
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
            private int m_size;

            public Container()
            {
                m_buckets = new int[DefaultCapacity];
                m_entries = new Entry[DefaultCapacity];
            }

            private Container(int capacity, int size)
            {
                m_buckets = new int[capacity];
                m_entries = new Entry[capacity];
                m_size = size;
            }

            public int Capacity
            {
                get => m_entries.Length;
            }

            public int Count
            {
                get => m_size;
            }

            public bool Isfull
            {
                get => m_size == m_entries.Length;
            }

            public EntityTableGrouping this[int index]
            {
                get => m_entries[index].Grouping;
            }

            public void Add(EntityTableGrouping grouping)
            {
                int size = m_size;
                int hashCode = BitmaskOperations.GetHashCode(grouping.Key.ComponentBitmask) & int.MaxValue;
                ref int bucket = ref m_buckets[hashCode & m_buckets.Length - 1];
                ref Entry entry = ref m_entries[size];

                entry.Grouping = grouping;
                entry.HashCode = hashCode;
                entry.Next = bucket;
                m_size = size + 1;

                Volatile.Write(ref bucket, ~size);
            }

            public void CopyTo(EntityTableGrouping[] array, int index)
            {
                ArgumentNullException.ThrowIfNull(array);

                if ((uint)index >= (uint)array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Index was out of range. Must be non-negative and less than the length of the array.");
                }

                if (array.Length - index < m_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Count exceeds the length of the array.");
                }

                int size = m_size;
                Entry[] entries = m_entries;

                for (int i = 0; i < size; i++)
                {
                    array[index++] = entries[i].Grouping;
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

                if (array.Length - index < m_size)
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
                    int size = m_size;
                    Entry[] entries = m_entries;

                    for (int i = 0; i < size; i++)
                    {
                        objects[index++] = entries[i].Grouping;
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException(
                        "Array is not of type EntityTableGrouping[].", nameof(array));
                }
            }

            public EntityTableGrouping? Find(ReadOnlySpan<uint> key)
            {
                Entry[] entries = m_entries;
                ref Entry entry = ref Unsafe.NullRef<Entry>();
                int hashCode = BitmaskOperations.GetHashCode(key) & int.MaxValue;

                for (int i = Volatile.Read(ref m_buckets[hashCode & m_buckets.Length - 1]); i < 0; i = entry.Next)
                {
                    EntityTableGrouping grouping = (entry = ref entries[~i]).Grouping;

                    if (entry.HashCode == hashCode && grouping.Key.ComponentBitmask.SequenceEqual(key))
                    {
                        return grouping;
                    }
                }

                return null;
            }

            public Container Grow()
            {
                int size = m_size;
                Entry[] oldEntries = m_entries;
                Container container = new Container(oldEntries.Length * 2, size);
                int[] newBuckets = container.m_buckets;
                Entry[] newEntries = container.m_entries;

                for (int i = 0; i < size; i++)
                {
                    ref Entry oldEntry = ref oldEntries[i];
                    ref Entry newEntry = ref newEntries[i];
                    ref int newBucket = ref newBuckets[oldEntry.HashCode & newBuckets.Length - 1];

                    newEntry.Grouping = oldEntry.Grouping;
                    newEntry.HashCode = oldEntry.HashCode;
                    newEntry.Next = newBucket;
                    newBucket = ~i;
                }

                return container;
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
