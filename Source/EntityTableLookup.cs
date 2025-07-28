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

namespace Logos.Entities
{
    /// <summary>
    /// Represents a collection of entity archetypes each mapped to an
    /// <see cref="EntityTableGrouping"/>.
    /// </summary>
    public class EntityTableLookup : ILookup<EntityArchetype, EntityTable>, IReadOnlyList<EntityTableGrouping>, ICollection
    {
        private const int DefaultCapacity = 8;

        private readonly object m_lock;
        private volatile Container m_container;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTableLookup"/> class.
        /// </summary>
        public EntityTableLookup()
        {
            m_lock = new object();
            m_container = new Container();
        }

        /// <summary>
        /// Gets the total number of key/value collection pairs the internal data structure can
        /// hold without resizing.
        /// </summary>
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
                        "Index was out of range. Must be non-negative and less than the size of " +
                        "the EntityTableLookup.");
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

        /// <summary>
        /// Copies the elements of the <see cref="EntityTableLookup"/> to an array, starting at a
        /// particular array index.
        /// </summary>
        /// 
        /// <param name="array">
        /// The one-dimensional array that is the destination of the entity table groupings copied
        /// from the <see cref="EntityTableLookup"/>. The array must have zero-based indexing.
        /// </param>
        /// 
        /// <param name="index">
        /// The zero-based index in the array at which copying begins.
        /// </param>
        public void CopyTo(EntityTableGrouping[] array, int index)
        {
            m_container.CopyTo(array, index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            m_container.CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="EntityTableLookup"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An enumerator that can be used to iterate through the <see cref="EntityTableLookup"/>.
        /// </returns>
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

        /// <summary>
        /// Gets or creates an <see cref="EntityTableGrouping"/> whose key contains component types
        /// from the specified array.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The array of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityTableGrouping"/> whose key contains component types from the array.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentTypes"/> is <see langword="null"/>.
        /// </exception>
        public EntityTableGrouping GetGrouping(ComponentType[] componentTypes)
        {
            ArgumentNullException.ThrowIfNull(componentTypes);

            BitmaskBuilder builder = new BitmaskBuilder(stackalloc int[DefaultCapacity]);
            
            foreach (ComponentType componentType in componentTypes)
            {
                if (componentType != null)
                {
                    builder.Set(componentType.Id);
                }
            }

            EntityTableGrouping grouping = GetGrouping(builder.Build(),
                new ReadOnlySpan<ComponentType>(componentTypes));
            builder.Dispose();
            return grouping;
        }

        /// <summary>
        /// Gets or creates an <see cref="EntityTableGrouping"/> whose key contains component types
        /// from the specified sequence.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The sequence of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityTableGrouping"/> whose key contains component types from the
        /// sequence.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentTypes"/> is <see langword="null"/>.
        /// </exception>
        public EntityTableGrouping GetGrouping(IEnumerable<ComponentType> componentTypes)
        {
            ComponentType[] buffer = componentTypes.TryGetNonEnumeratedCount(out int count)
                ? ArrayPool<ComponentType>.Shared.Rent(count)
                : Array.Empty<ComponentType>();
            BitmaskBuilder builder = new BitmaskBuilder(stackalloc int[DefaultCapacity]);
            count = 0;

            foreach (ComponentType componentType in componentTypes)
            {
                if (componentType != null)
                {
                    if (count >= buffer.Length)
                    {
                        ComponentType[] array = ArrayPool<ComponentType>.Shared.Rent(count + 1);
                        Array.Copy(buffer, array, count);
                        ArrayPool<ComponentType>.Shared.Return(buffer, true);
                        buffer = array;
                    }

                    buffer[count++] = componentType;
                    builder.Set(componentType.Id);
                }
            }

            EntityTableGrouping grouping = GetGrouping(builder.Build(),
                new ReadOnlySpan<ComponentType>(buffer, 0, count));
            ArrayPool<ComponentType>.Shared.Return(buffer, clearArray: true);
            builder.Dispose();
            return grouping;
        }

        /// <summary>
        /// Gets or creates an <see cref="EntityTableGrouping"/> whose key contains component types
        /// from the specified span.
        /// </summary>
        /// 
        /// <param name="componentTypes">
        /// The span of component types.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityTableGrouping"/> whose key contains component types from the span.
        /// </returns>
        public EntityTableGrouping GetGrouping(ReadOnlySpan<ComponentType> componentTypes)
        {
            BitmaskBuilder builder = new BitmaskBuilder(stackalloc int[DefaultCapacity]);

            for (int i = 0; i < componentTypes.Length; i++)
            {
                ComponentType componentType = componentTypes[i];

                if (componentType != null)
                {
                    builder.Set(componentType.Id);
                }
            }

            EntityTableGrouping grouping = GetGrouping(builder.Build(), componentTypes);
            builder.Dispose();
            return grouping;
        }

        private EntityTableGrouping GetGrouping(ReadOnlySpan<int> componentBitmask,
            ReadOnlySpan<ComponentType> componentTypes)
        {
            EntityTableGrouping? grouping = m_container.Find(componentBitmask);

            if (grouping == null)
            {
                lock (m_lock)
                {
                    Container container = m_container;
                    grouping = container.Find(componentBitmask);

                    if (grouping == null)
                    {
                        if (container.Isfull)
                        {
                            m_container = container = container.Grow();
                        }

                        grouping = new EntityTableGrouping(EntityArchetype.Create(componentTypes));
                        container.Add(grouping);
                    }
                }
            }

            return grouping;
        }

        /// <summary>
        /// Gets or creates an entity table grouping whose key is equal to the specified entity
        /// archetype.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The entity archetype.
        /// </param>
        /// 
        /// <returns>
        /// An entity table grouping whose key is equal to the entity archetype.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        public EntityTableGrouping GetGrouping(EntityArchetype archetype)
        {
            ArgumentNullException.ThrowIfNull(archetype);

            EntityTableGrouping? grouping = m_container.Find(archetype.ComponentBitmask);

            if (grouping == null)
            {
                lock (m_lock)
                {
                    Container container = m_container;
                    grouping = container.Find(archetype.ComponentBitmask);

                    if (grouping == null)
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

        /// <summary>
        /// Gets or creates an <see cref="EntityTableGrouping"/> whose key is equal to the
        /// specified <see cref="EntityArchetype"/> with the specified <see cref="ComponentType"/>
        /// removed from it.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The <see cref="EntityArchetype"/>.
        /// </param>
        /// 
        /// <param name="componentType">
        /// The <see cref="ComponentType"/> to remove.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityTableGrouping"/> whose key is equal to the
        /// <see cref="EntityArchetype"/> with the <see cref="ComponentType"/> removed from it.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        public EntityTableGrouping GetSubgrouping(EntityArchetype archetype, ComponentType componentType)
        {
            if (componentType == null)
            {
                return GetGrouping(archetype);
            }

            ArgumentNullException.ThrowIfNull(archetype);

            ReadOnlySpan<int> sourceBitmask = archetype.ComponentBitmask;
            int index = componentType.Id >> 5;
            int bit = 1 << componentType.Id;

            if (index >= sourceBitmask.Length || (bit & sourceBitmask[index]) == 0)
            {
                return GetGrouping(archetype);
            }

            int[]? rentedArray;
            scoped Span<int> destinationBitmask;
            
            if (sourceBitmask.Length <= DefaultCapacity)
            {
                rentedArray = null;
                destinationBitmask = stackalloc int[sourceBitmask.Length];
            }
            else
            {
                rentedArray = ArrayPool<int>.Shared.Rent(sourceBitmask.Length);
                destinationBitmask = new Span<int>(rentedArray, 0, sourceBitmask.Length);
            }

            sourceBitmask.CopyTo(destinationBitmask);

            if ((destinationBitmask[index] ^= bit) == 0 && destinationBitmask.Length == index + 1)
            {
                ReadOnlySpan<ComponentType> componentTypes = archetype.ComponentTypes;
                destinationBitmask = componentTypes.Length > 1
                    ? destinationBitmask.Slice(0, componentTypes[^2].Id + 32 >> 5)
                    : Span<int>.Empty;
            }

            EntityTableGrouping? grouping = m_container.Find(destinationBitmask);

            if (grouping == null)
            {
                lock (m_lock)
                {
                    Container container = m_container;

                    if ((grouping = container.Find(destinationBitmask)) == null)
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
                ArrayPool<int>.Shared.Return(rentedArray);
            }

            return grouping;
        }

        /// <summary>
        /// Gets or creates an <see cref="EntityTableGrouping"/> whose key is equal to the
        /// specified <see cref="EntityArchetype"/> with the specified <see cref="ComponentType"/>
        /// added to it.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The <see cref="EntityArchetype"/>.
        /// </param>
        /// 
        /// <param name="componentType">
        /// The <see cref="ComponentType"/> to add.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="EntityTableGrouping"/> whose key is equal to the
        /// <see cref="EntityArchetype"/> with the <see cref="ComponentType"/> added to it.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        public EntityTableGrouping GetSupergrouping(EntityArchetype archetype, ComponentType componentType)
        {
            if (componentType == null)
            {
                return GetGrouping(archetype);
            }

            ArgumentNullException.ThrowIfNull(archetype);

            ReadOnlySpan<int> sourceBitmask = archetype.ComponentBitmask;
            int index = componentType.Id >> 5;
            int bit = 1 << componentType.Id;

            if (index < sourceBitmask.Length && (bit & sourceBitmask[index]) != 0)
            {
                return GetGrouping(archetype);
            }

            int[]? rentedArray;
            scoped Span<int> destinationBitmask;
            int length = Math.Max(index + 1, sourceBitmask.Length);

            if (length <= DefaultCapacity)
            {
                rentedArray = null;
                destinationBitmask = stackalloc int[length];
            }
            else
            {
                rentedArray = ArrayPool<int>.Shared.Rent(length);
                destinationBitmask = new Span<int>(rentedArray, 0, length);
            }

            sourceBitmask.CopyTo(destinationBitmask);
            destinationBitmask.Slice(sourceBitmask.Length).Clear();
            destinationBitmask[index] |= bit;

            EntityTableGrouping? grouping = m_container.Find(destinationBitmask);

            if (grouping == null)
            {
                lock (m_lock)
                {
                    Container container = m_container;
                    grouping = container.Find(destinationBitmask);

                    if (grouping == null)
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
                ArrayPool<int>.Shared.Return(rentedArray);
            }

            return grouping;
        }

        /// <summary>
        /// Gets the <see cref="EntityTableGrouping"/> whose key is equal to the specified
        /// <see cref="EntityArchetype"/>.
        /// </summary>
        /// 
        /// <param name="archetype">
        /// The key of the <see cref="EntityTableGrouping"/> to get.
        /// </param>
        /// 
        /// <param name="grouping">
        /// When this method returns, contains the <see cref="EntityTableGrouping"/> whose key is
        /// equal to the specified <see cref="EntityArchetype"/>, if the
        /// <see cref="EntityArchetype"/> is found; otherwise, <see langword="null"/>. This
        /// parameter is passed uninitialized.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityTableLookup"/> contains an
        /// <see cref="EntityTableGrouping"/> whose key is equal to the
        /// <see cref="EntityArchetype"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="archetype"/> is <see langword="null"/>.
        /// </exception>
        public bool TryGetGrouping(EntityArchetype archetype, [NotNullWhen(true)] out EntityTableGrouping? grouping)
        {
            ArgumentNullException.ThrowIfNull(archetype);
            return (grouping = m_container.Find(archetype.ComponentBitmask)) != null;
        }

        /// <summary>
        /// Enumerates through the elements of the <see cref="EntityTableLookup"/>.
        /// </summary>
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

        private ref struct BitmaskBuilder
        {
            private int[]? m_rentedArray;
            private Span<int> m_span;
            private int m_size;

            public BitmaskBuilder(Span<int> span)
            {
                m_rentedArray = null;
                m_span = span;
                m_size = 0;
            }

            public readonly void Dispose()
            {
                int[]? rentedArray = m_rentedArray;

                if (rentedArray != null)
                {
                    ArrayPool<int>.Shared.Return(rentedArray);
                }
            }

            public void Set(int index)
            {
                int spanIndex = index >> 5;

                if (spanIndex >= m_size)
                {
                    Grow(spanIndex + 1);
                }

                m_span[spanIndex] |= 1 << index;
            }

            private void Grow(int capacity)
            {
                if (capacity > m_span.Length)
                {
                    int[]? rentedArray = m_rentedArray;
                    int[] newArray = ArrayPool<int>.Shared.Rent(capacity);

                    m_span.CopyTo(newArray);

                    if (rentedArray != null)
                    {
                        ArrayPool<int>.Shared.Return(rentedArray);
                    }

                    m_rentedArray = newArray;
                    m_span = new Span<int>(newArray);
                }

                m_span.Slice(m_size, capacity).Clear();
                m_size = capacity;
            }

            public readonly ReadOnlySpan<int> Build()
            {
                return m_span.Slice(0, m_size);
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
                        "Index was out of range. Must be non-negative and less than the length " +
                        "of the array.");
                }

                if (array.Length - index < m_size)
                {
                    throw new ArgumentException(
                        "Index and Count exceeds the length of the array.");
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
                        "Index was out of range. Must be non-negative and less than the length " +
                        "of the array.");
                }

                if (array.Length - index < m_size)
                {
                    throw new ArgumentException(
                        "Index and Count exceeds the length of the array.");
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

            public EntityTableGrouping? Find(ReadOnlySpan<int> bitmask)
            {
                Entry[] entries = m_entries;
                ref Entry entry = ref Unsafe.NullRef<Entry>();
                int hashCode = BitmaskOperations.GetHashCode(bitmask) & int.MaxValue;

                for (int i = Volatile.Read(ref m_buckets[hashCode & m_buckets.Length - 1]); i < 0; i = entry.Next)
                {
                    EntityTableGrouping grouping = (entry = ref entries[~i]).Grouping;

                    if (entry.HashCode == hashCode && grouping.Key.ComponentBitmask.SequenceEqual(bitmask))
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
