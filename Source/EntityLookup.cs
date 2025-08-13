// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Logos.Entities
{
    public sealed class EntityLookup : ILookup<EntityArchetype, EntityTable>,
        ICollection<EntityGrouping>, ICollection, IReadOnlyCollection<EntityGrouping>
    {
        private const uint MaximumStackAllocSize = 8;

        private static readonly EntityLookup s_empty = new EntityLookup();

        private readonly int[] m_buckets;
        private readonly Entry[] m_entries;

        private EntityLookup()
        {
            m_buckets = Array.Empty<int>();
            m_entries = Array.Empty<Entry>();
        }

        private EntityLookup(int[] buckets, Entry[] entries)
        {
            m_buckets = buckets;
            m_entries = entries;
        }

        public static EntityLookup Empty
        {
            get => s_empty;
        }

        public int Count
        {
            get => m_entries.Length;
        }

        public EntityGrouping this[EntityArchetype key]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(key);

                ref readonly Entry entry = ref FindEntry(key.ComponentBitmask, out _);

                if (Unsafe.IsNullRef(in entry))
                {
                    ThrowForKeyNotFound();
                }

                return entry.Grouping;
            }
        }

        bool ICollection<EntityGrouping>.IsReadOnly
        {
            get => true;
        }

        bool ICollection.IsSynchronized
        {
            get => false;
        }

        object ICollection.SyncRoot
        {
            get => this;
        }

        IEnumerable<EntityTable> ILookup<EntityArchetype, EntityTable>.this[EntityArchetype key]
        {
            get => this[key];
        }

        public EntityLookup Add(EntityGrouping item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (Unsafe.IsNullRef(in FindEntry(item.Key.ComponentBitmask, out uint hashCode)))
            {
                return AddEntry(item, hashCode);
            }

            return this;
        }

        public EntityLookup AddOrUpdate(EntityGrouping item)
        {
            ArgumentNullException.ThrowIfNull(item);

            ref readonly Entry targetEntry = ref FindEntry(item.Key.ComponentBitmask, out uint hashCode);

            if (Unsafe.IsNullRef(in targetEntry))
            {
                return AddEntry(item, hashCode);
            }

            if (targetEntry.Grouping == item)
            {
                return this;
            }

            Entry[] sourceEntries = m_entries;
            int length = sourceEntries.Length;
            Entry[] destinationEntries = new Entry[length];

            for (int i = 0; i < length; i++)
            {
                ref readonly Entry sourceEntry = ref sourceEntries[i];
                ref Entry destinationEntry = ref destinationEntries[i];

                if (Unsafe.AreSame(in sourceEntry, in targetEntry))
                {
                    destinationEntry.Grouping = item;
                    destinationEntry.HashCode = hashCode;
                    destinationEntry.Next = sourceEntry.Next;
                }
                else
                {
                    destinationEntry = sourceEntry;
                }
            }

            return new EntityLookup(m_buckets, destinationEntries);
        }

        public EntityLookup Remove(EntityArchetype key)
        {
            ArgumentNullException.ThrowIfNull(key);

            ref readonly Entry entry = ref FindEntry(key.ComponentBitmask, out _);

            if (Unsafe.IsNullRef(in entry))
            {
                return this;
            }

            return RemoveEntry(entry.Grouping);
        }

        public EntityLookup Remove(EntityGrouping item)
        {
            ArgumentNullException.ThrowIfNull(item);

            ref readonly Entry entry = ref FindEntry(item.Key.ComponentBitmask, out _);

            if (Unsafe.IsNullRef(in entry) || entry.Grouping != item)
            {
                return this;
            }

            return RemoveEntry(item);
        }

        public bool Contains(EntityArchetype key)
        {
            return key != null && !Unsafe.IsNullRef(in FindEntry(key.ComponentBitmask, out _));
        }

        public bool Contains(EntityGrouping item)
        {
            if (item == null)
            {
                return false;
            }

            ref readonly Entry entry = ref FindEntry(item.Key.ComponentBitmask, out _);

            return !Unsafe.IsNullRef(in entry)
                && entry.Grouping == item;
        }

        public bool TryGetValue(EntityArchetype key, [NotNullWhen(true)] out EntityGrouping? value)
        {
            ArgumentNullException.ThrowIfNull(key);

            ref readonly Entry entry = ref FindEntry(key.ComponentBitmask, out _);

            if (Unsafe.IsNullRef(in entry))
            {
                value = null;
                return false;
            }

            value = entry.Grouping;
            return true;
        }

        public bool TryGetInclusiveValue(EntityArchetype key, ComponentType keyElement,
            [NotNullWhen(true)] out EntityGrouping? value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(keyElement);

            ReadOnlySpan<int> bitmask = key.ComponentBitmask;
            int index = keyElement.Index >> 5;
            int bit = 1 << keyElement.Index;

            int[]? rentedArray;
            scoped Span<int> buffer;
            int length;

            if (index < bitmask.Length)
            {
                if ((bit & bitmask[index]) != 0)
                {
                    return TryGetValue(key, out value);
                }

                length = bitmask.Length;
            }
            else
            {
                length = index + 1;
            }

            if (length <= MaximumStackAllocSize)
            {
                rentedArray = null;
                buffer = stackalloc int[length];
            }
            else
            {
                rentedArray = ArrayPool<int>.Shared.Rent(length);
                buffer = new Span<int>(rentedArray, 0, length);
            }

            bitmask.CopyTo(buffer);
            buffer.Slice(bitmask.Length).Clear();
            buffer[index] |= bit;

            ref readonly Entry entry = ref FindEntry(buffer, out _);

            if (rentedArray != null)
            {
                ArrayPool<int>.Shared.Return(rentedArray);
            }

            if (Unsafe.IsNullRef(in entry))
            {
                value = null;
                return false;
            }

            value = entry.Grouping;
            return true;
        }

        public bool TryGetExclusiveValue(EntityArchetype key, ComponentType keyElement,
            [NotNullWhen(true)] out EntityGrouping? value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(keyElement);
            
            ReadOnlySpan<int> bitmask = key.ComponentBitmask;
            int index = keyElement.Index >> 5;
            int bit = 1 << keyElement.Index;

            if (index >= bitmask.Length || (bit & bitmask[index]) == 0)
            {
                return TryGetValue(key, out value);
            }

            int[]? rentedArray;
            scoped Span<int> buffer;

            if (bitmask.Length <= MaximumStackAllocSize)
            {
                rentedArray = null;
                buffer = stackalloc int[bitmask.Length];
            }
            else
            {
                rentedArray = ArrayPool<int>.Shared.Rent(bitmask.Length);
                buffer = new Span<int>(rentedArray, 0, bitmask.Length);
            }

            bitmask.CopyTo(buffer);

            if ((buffer[index] ^= bit) == 0 && buffer.Length == index + 1)
            {
                ReadOnlySpan<ComponentType> keyElements = key.ComponentTypes;

                buffer = (keyElements.Length > 1)
                    ? buffer.Slice(0, keyElements[^2].Index + 32 >> 5)
                    : Span<int>.Empty;
            }

            ref readonly Entry entry = ref FindEntry(buffer, out _);

            if (rentedArray != null)
            {
                ArrayPool<int>.Shared.Return(rentedArray);
            }

            if (Unsafe.IsNullRef(in entry))
            {
                value = null;
                return false;
            }

            value = entry.Grouping;
            return true;
        }

        public void CopyTo(EntityGrouping[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);

            Entry[] entries = m_entries;
            int upperBound = arrayIndex + entries.Length;

            if ((uint)array.Length < (uint)upperBound)
            {
                ThrowForInsufficientArraySpace();
            }

            while (arrayIndex < upperBound)
            {
                array[arrayIndex] = entries[arrayIndex++].Grouping;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        void ICollection<EntityGrouping>.Add(EntityGrouping item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<EntityGrouping>.Remove(EntityGrouping item)
        {
            throw new NotSupportedException();
        }

        void ICollection<EntityGrouping>.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (array.Rank != 1)
            {
                ThrowForInvalidArrayRank();
            }

            if (array.GetLowerBound(0) != 0)
            {
                ThrowForInvalidArrayLowerBound();
            }

            ArgumentOutOfRangeException.ThrowIfNegative(index);

            Entry[] entries = m_entries;
            int upperBound = index + entries.Length;

            if ((uint)array.Length < (uint)upperBound)
            {
                ThrowForInsufficientArraySpace();
            }

            object[]? objects = array as object[];

            if (objects == null)
            {
                ThrowForInvalidArrayType();
            }

            try
            {
                while (index < upperBound)
                {
                    objects[index] = entries[index++].Grouping;
                }
            }
            catch (ArrayTypeMismatchException)
            {
                ThrowForInvalidArrayType();
            }
        }

        IEnumerator<IGrouping<EntityArchetype, EntityTable>> IEnumerable<IGrouping<EntityArchetype, EntityTable>>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<EntityGrouping> IEnumerable<EntityGrouping>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        [DoesNotReturn]
        private static void ThrowForKeyNotFound()
        {
            throw new KeyNotFoundException(
                "The key does not exist in the EntityLookup.");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayRank()
        {
            throw new ArgumentException(
                "The array is multidimensional.", "array");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayLowerBound()
        {
            throw new ArgumentException(
                "The array does not have zero-based indexing.", "array");
        }

        [DoesNotReturn]
        private static void ThrowForInsufficientArraySpace()
        {
            throw new ArgumentException(
                "The number of elements in the EntityLookup is greater than " +
                "the available space from the index to the end of the " +
                "destination array.");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayType()
        {
            throw new ArgumentException(
                "EntityGrouping cannot be cast automatically to the type of " +
                "the destination array.", "array");
        }

        private EntityLookup AddEntry(EntityGrouping item, uint hashCode)
        {
            Entry[] sourceEntries = m_entries;
            int sourceLength = sourceEntries.Length;
            uint destinationLength = (uint)sourceLength + 1;
            int[] destinationBuckets = new int[destinationLength];
            Entry[] destinationEntries = new Entry[destinationLength];

            for (int i = 0; i < sourceLength; i++)
            {
                ref readonly Entry sourceEntry = ref sourceEntries[i];
                ref Entry destinationEntry = ref destinationEntries[i];
                ref int destinationBucket = ref destinationBuckets[sourceEntry.HashCode % destinationLength];

                destinationEntry.Grouping = sourceEntry.Grouping;
                destinationEntry.HashCode = sourceEntry.HashCode;
                destinationEntry.Next = destinationBucket;
                destinationBucket = ~i;
            }

            ref Entry newEntry = ref destinationEntries[sourceLength];
            ref int newBucket = ref destinationBuckets[hashCode % destinationLength];

            newEntry.Grouping = item;
            newEntry.HashCode = hashCode;
            newEntry.Next = newBucket;
            newBucket = ~sourceLength;

            return new EntityLookup(destinationBuckets, destinationEntries);
        }

        private EntityLookup RemoveEntry(EntityGrouping item)
        {
            Entry[] sourceEntries = m_entries;
            int sourceLength = sourceEntries.Length;

            if (sourceLength == 1)
            {
                return s_empty;
            }

            uint destinationLength = (uint)sourceLength - 1;
            int[] newBuckets = new int[destinationLength];
            Entry[] newEntries = new Entry[destinationLength];
            int sourceIndex = 0;
            int destinationIndex = 0;

            do
            {
                ref readonly Entry sourceEntry = ref sourceEntries[sourceIndex++];

                if (sourceEntry.Grouping != item)
                {
                    ref Entry destinationEntry = ref newEntries[destinationIndex];
                    ref int destinationBucket = ref newBuckets[sourceEntry.HashCode % destinationLength];

                    destinationEntry.Grouping = sourceEntry.Grouping;
                    destinationEntry.HashCode = sourceEntry.HashCode;
                    destinationEntry.Next = destinationBucket;
                    destinationBucket = ~destinationIndex++;
                }
            }
            while (sourceIndex < sourceLength);

            return new EntityLookup(newBuckets, newEntries);
        }

        private ref readonly Entry FindEntry(ReadOnlySpan<int> bitmask, out uint hashCode)
        {
            Entry[] entries = m_entries;
            hashCode = (uint)BitmaskOperations.GetHashCode(bitmask);

            if (entries.Length > 0)
            {
                int index = m_buckets[hashCode % (uint)entries.Length];

                while (index < 0)
                {
                    ref readonly Entry entry = ref entries[~index];

                    if (entry.HashCode == hashCode &&
                        entry.Grouping.Key.ComponentBitmask.SequenceEqual(bitmask))
                    {
                        return ref entry;
                    }

                    index = entry.Next;
                }
            }

            return ref Unsafe.NullRef<Entry>();
        }

        private struct Entry
        {
            public EntityGrouping Grouping;
            public uint HashCode;
            public int Next;
        }

        public struct Enumerator : IEnumerator<EntityGrouping>
        {
            private readonly Entry[] m_entries;
            private readonly int m_length;
            private int m_index;

            internal Enumerator(EntityLookup lookup)
            {
                m_entries = lookup.m_entries;
                m_length = m_entries.Length;
                m_index = -1;
            }

            public readonly EntityGrouping Current
            {
                get => m_entries[m_index].Grouping;
            }

            readonly object IEnumerator.Current
            {
                get => m_entries[m_index].Grouping;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                int index = m_index + 1;

                if (index < m_length)
                {
                    m_index = index;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                m_index = -1;
            }
        }
    }
}
