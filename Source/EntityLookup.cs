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
    /// <summary>
    /// Represents a collection of archetypes each mapped to one or more tables.
    /// </summary>
    public sealed class EntityLookup : ILookup<EntityArchetype, EntityTable>,
        ICollection<EntityGrouping>, ICollection, IReadOnlyCollection<EntityGrouping>
    {
        private const int StackallocIntBufferSizeLimit = 32;

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

        /// <summary>
        /// Gets an empty <see cref="EntityLookup"/>.
        /// </summary>
        /// <returns>
        /// An empty <see cref="EntityLookup"/>.
        /// </returns>
        public static EntityLookup Empty
        {
            get => s_empty;
        }

        /// <summary>
        /// Gets the number of key/value collection pairs in the <see cref="EntityLookup"/>.
        /// </summary>
        /// <returns>
        /// The number of key/value collection pairs in the <see cref="EntityLookup"/>.
        /// </returns>
        public int Count
        {
            get => m_entries.Length;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="EntityLookup"/> is empty.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityLookup"/> is empty; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsEmpty
        {
            get => m_entries.Length == 0;
        }

        /// <summary>
        /// Gets the sequence of values indexed by the specified key in the
        /// <see cref="EntityLookup"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the desired sequence of values.
        /// </param>
        /// <returns>
        /// The sequence of values indexed by <paramref name="key"/> in the
        /// <see cref="EntityLookup"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// <paramref name="key"/> does not exist in the <see cref="EntityLookup"/>.
        /// </exception>
        public EntityGrouping this[EntityArchetype key]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(key);

                ref readonly Entry entry = ref FindEntry(key.ComponentBitmap, out _);

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
            get
            {
                if (key is not null)
                {
                    ref readonly Entry entry = ref FindEntry(key.ComponentBitmap, out _);

                    if (!Unsafe.IsNullRef(in entry))
                    {
                        return entry.Grouping;
                    }
                }

                return Enumerable.Empty<EntityTable>();
            }
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityLookup"/> with the specified key/value collection
        /// pair added to it.
        /// </summary>
        /// <param name="item">
        /// The key/value collection pair to add to the copy of the <see cref="EntityLookup"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityLookup"/> with <paramref name="item"/> added to it, or
        /// the <see cref="EntityLookup"/> if it contains an element with an equivalent key.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public EntityLookup Add(EntityGrouping item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (Unsafe.IsNullRef(in FindEntry(item.Key.ComponentBitmap, out uint hashCode)))
            {
                return AddEntry(item, hashCode);
            }

            return this;
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityLookup"/> with the specified key/value collection
        /// pair added to it. If the <see cref="EntityLookup"/> contains an element with an
        /// equavalent key, the element is replaced by the pair instead.
        /// </summary>
        /// <param name="item">
        /// The key/value collection pair to add to the copy of the <see cref="EntityLookup"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityLookup"/> with <paramref name="item"/> added to it, or
        /// the <see cref="EntityLookup"/> if it contains <paramref name="item"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public EntityLookup AddOrUpdate(EntityGrouping item)
        {
            ArgumentNullException.ThrowIfNull(item);

            ref readonly Entry targetEntry = ref FindEntry(item.Key.ComponentBitmap, out uint hashCode);

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

        /// <summary>
        /// Creates a copy of the <see cref="EntityLookup"/> with all key/value collection pairs
        /// removed from it.
        /// </summary>
        /// <returns>
        /// A copy of the <see cref="EntityLookup"/> with all key/value collection pairs removed
        /// from it, or the <see cref="EntityLookup"/> if it is empty.
        /// </returns>
        public EntityLookup Clear()
        {
            // This method is redundant considering that the only instance that should be empty is
            // s_empty. This method was only added to mirror the API of the EntityGrouping class.
            if (m_entries.Length == 0)
            {
                return this;
            }

            return s_empty;
        }

        /// <summary>
        /// Determines whether the <see cref="EntityLookup"/> contains a specific key/value
        /// collection pair.
        /// </summary>
        /// <param name="item">
        /// The key/value collection pair to locate in the <see cref="EntityLookup"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> is found in the
        /// <see cref="EntityLookup"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(EntityGrouping item)
        {
            if (item == null)
            {
                return false;
            }

            ref readonly Entry entry = ref FindEntry(item.Key.ComponentBitmap, out _);

            return !Unsafe.IsNullRef(in entry) && entry.Grouping == item;
        }

        /// <summary>
        /// Copies the elements of the <see cref="EntityLookup"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        /// from the <see cref="EntityLookup"/>. The <see cref="Array"/> must have zero-based
        /// indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the <see cref="EntityLookup"/> is greater than the available
        /// space from <paramref name="arrayIndex"/> to the end of <paramref name="array"/>.
        /// </exception>
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

        /// <summary>
        /// Creates a copy of the <see cref="EntityLookup"/> with the specified key/value collection
        /// pair removed from it.
        /// </summary>
        /// <param name="item">
        /// The key/value collection pair to remove from the copy of the <see cref="EntityLookup"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityLookup"/> with <paramref name="item"/> removed from it,
        /// or the <see cref="EntityLookup"/> if <paramref name="item"/> is not found in it.
        /// </returns>
        public EntityLookup Remove(EntityGrouping item)
        {
            ArgumentNullException.ThrowIfNull(item);

            ref readonly Entry targetEntry = ref FindEntry(item.Key.ComponentBitmap, out _);

            if (Unsafe.IsNullRef(in targetEntry) || targetEntry.Grouping != item)
            {
                return this;
            }

            Entry[] sourceEntries = m_entries;
            int sourceLength = sourceEntries.Length;

            if (sourceLength == 1)
            {
                return s_empty;
            }

            uint destinationLength = (uint)sourceLength - 1;
            int[] destinationBuckets = new int[destinationLength];
            Entry[] destinationEntries = new Entry[destinationLength];
            int sourceIndex = 0;
            int destinationIndex = 0;

            do
            {
                ref readonly Entry sourceEntry = ref sourceEntries[sourceIndex++];

                if (!Unsafe.AreSame(in targetEntry, in sourceEntry))
                {
                    ref Entry destinationEntry = ref destinationEntries[destinationIndex];
                    ref int destinationBucket = ref destinationBuckets[sourceEntry.HashCode % destinationLength];

                    destinationEntry.Grouping = sourceEntry.Grouping;
                    destinationEntry.HashCode = sourceEntry.HashCode;
                    destinationEntry.Next = destinationBucket;
                    destinationBucket = ~destinationIndex++;
                }
            }
            while (sourceIndex < sourceLength);

            return new EntityLookup(destinationBuckets, destinationEntries);
        }

        /// <summary>
        /// Returns an <see cref="Enumerator"/> that iterates through the
        /// <see cref="EntityLookup"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Enumerator"/> that can be used to iterate through the
        /// <see cref="EntityLookup"/>.
        /// </returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Gets the sequence of values indexed by the specified key in the
        /// <see cref="EntityLookup"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the desired sequence of values.
        /// </param>
        /// <param name="grouping">
        /// When this method returns, contains the sequence of values indexed by
        /// <paramref name="key"/> in the <see cref="EntityLookup"/>, if <paramref name="key"/> is
        /// found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityLookup"/> contains an element with
        /// <paramref name="key"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public bool TryGetGrouping(EntityArchetype key, [NotNullWhen(true)] out EntityGrouping? grouping)
        {
            ArgumentNullException.ThrowIfNull(key);

            ref readonly Entry entry = ref FindEntry(key.ComponentBitmap, out _);

            if (Unsafe.IsNullRef(in entry))
            {
                grouping = null;
                return false;
            }

            grouping = entry.Grouping;
            return true;
        }

        /// <summary>
        /// Gets the sequence of values indexed by a key in the <see cref="EntityLookup"/> that is
        /// equivalent to the specified key with the specified component type removed from it.
        /// </summary>
        /// <param name="key">
        /// The key whose component types form the superset of component types contained by the key
        /// of the desired sequence of values.
        /// </param>
        /// <param name="componentType">
        /// The component type to exclude from <paramref name="key"/> when searching for the desired
        /// sequence of values.
        /// </param>
        /// <param name="grouping">
        /// When this method returns, contains the sequence of values indexed by a key in the
        /// <see cref="EntityLookup"/> that is equivalent to <paramref name="key"/> with
        /// <paramref name="componentType"/> removed from it, if such a key is found; otherwise,
        /// <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityLookup"/> contains an element whose key
        /// is equivalent to <paramref name="key"/> with <paramref name="componentType"/> removed
        /// from it; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentType"/> is <see langword="null"/>.
        /// </exception>
        public bool TryGetSubgrouping(EntityArchetype key, ComponentType componentType, [NotNullWhen(true)] out EntityGrouping? grouping)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<int> bitmap = key.ComponentBitmap;
            int index = componentType.Index >> 5;
            int bit = 1 << componentType.Index;

            if (index >= bitmap.Length || (bit & bitmap[index]) == 0)
            {
                return TryGetGrouping(key, out grouping);
            }

            int[]? rentedArray;
            scoped Span<int> buffer;

            if (bitmap.Length <= StackallocIntBufferSizeLimit)
            {
                rentedArray = null;
                buffer = stackalloc int[bitmap.Length];
            }
            else
            {
                rentedArray = ArrayPool<int>.Shared.Rent(bitmap.Length);
                buffer = new Span<int>(rentedArray, 0, bitmap.Length);
            }

            bitmap.CopyTo(buffer);

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
                grouping = null;
                return false;
            }

            grouping = entry.Grouping;
            return true;
        }

        /// <summary>
        /// Gets the sequence of values indexed by a key in the <see cref="EntityLookup"/> that is
        /// equivalent to the specified key with the specified component type added to it.
        /// </summary>
        /// <param name="key">
        /// The key whose component types form the subset of component types contained by the key of
        /// the desired sequence of values.
        /// </param>
        /// <param name="componentType">
        /// The component type to include with <paramref name="key"/> when searching for the desired
        /// sequence of values.
        /// </param>
        /// <param name="grouping">
        /// When this method returns, contains the sequence of values indexed by a key in the
        /// <see cref="EntityLookup"/> that is equivalent to <paramref name="key"/> with
        /// <paramref name="componentType"/> added to it, if such a key is found; otherwise,
        /// <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EntityLookup"/> contains an element whose key
        /// is equivalent to <paramref name="key"/> with <paramref name="componentType"/> added to
        /// it; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="componentType"/> is <see langword="null"/>.
        /// </exception>
        public bool TryGetSupergrouping(EntityArchetype key, ComponentType componentType, [NotNullWhen(true)] out EntityGrouping? grouping)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<int> bitmap = key.ComponentBitmap;
            int index = componentType.Index >> 5;
            int bit = 1 << componentType.Index;
            int length;
            int[]? rentedArray;
            scoped Span<int> buffer;

            if (index < bitmap.Length)
            {
                if ((bit & bitmap[index]) != 0)
                {
                    return TryGetGrouping(key, out grouping);
                }

                length = bitmap.Length;
            }
            else
            {
                length = index + 1;
            }

            if (length <= StackallocIntBufferSizeLimit)
            {
                rentedArray = null;
                buffer = stackalloc int[length];
            }
            else
            {
                rentedArray = ArrayPool<int>.Shared.Rent(length);
                buffer = new Span<int>(rentedArray, 0, length);
            }

            bitmap.CopyTo(buffer);
            buffer.Slice(bitmap.Length).Clear();
            buffer[index] |= bit;

            ref readonly Entry entry = ref FindEntry(buffer, out _);

            if (rentedArray != null)
            {
                ArrayPool<int>.Shared.Return(rentedArray);
            }

            if (Unsafe.IsNullRef(in entry))
            {
                grouping = null;
                return false;
            }

            grouping = entry.Grouping;
            return true;
        }

        bool ILookup<EntityArchetype, EntityTable>.Contains(EntityArchetype key)
        {
            return key is not null && !Unsafe.IsNullRef(in FindEntry(key.ComponentBitmap, out _));
        }

        void ICollection<EntityGrouping>.Add(EntityGrouping item)
        {
            throw new NotSupportedException();
        }

        void ICollection<EntityGrouping>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<EntityGrouping>.Remove(EntityGrouping item)
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
            throw new KeyNotFoundException("The key does not exist in the EntityLookup.");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayRank()
        {
            throw new ArgumentException("The array is multidimensional.", "array");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayLowerBound()
        {
            throw new ArgumentException("The array does not have zero-based indexing.", "array");
        }

        [DoesNotReturn]
        private static void ThrowForInsufficientArraySpace()
        {
            throw new ArgumentException(
                "The number of elements in the EntityLookup is greater than the available space " +
                "from the index to the end of the destination array.");
        }

        [DoesNotReturn]
        private static void ThrowForInvalidArrayType()
        {
            throw new ArgumentException(
                "EntityGrouping cannot be cast automatically to the type of the destination array.",
                "array");
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

        private ref readonly Entry FindEntry(ReadOnlySpan<int> bitmap, out uint hashCode)
        {
            Entry[] entries = m_entries;
            hashCode = (uint)BitmapOperations.GetHashCode(bitmap);

            if (entries.Length > 0)
            {
                int index = m_buckets[hashCode % (uint)entries.Length];

                while (index < 0)
                {
                    ref readonly Entry entry = ref entries[~index];

                    if (entry.HashCode == hashCode &&
                        entry.Grouping.Key.ComponentBitmap.SequenceEqual(bitmap))
                    {
                        return ref entry;
                    }

                    index = entry.Next;
                }
            }

            return ref Unsafe.NullRef<Entry>();
        }

        /// <summary>
        /// Enumerates the elements of an <see cref="EntityLookup"/>.
        /// </summary>
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

            /// <summary>
            /// Gets the element in the <see cref="EntityLookup"/> at the current position of the
            /// <see cref="Enumerator"/>.
            /// </summary>
            /// <returns>
            /// The element in the <see cref="EntityLookup"/> at the current position of the
            /// <see cref="Enumerator"/>.
            /// </returns>
            public readonly EntityGrouping Current
            {
                get => m_entries[m_index].Grouping;
            }

            readonly object IEnumerator.Current
            {
                get => m_entries[m_index].Grouping;
            }

            /// <inheritdoc cref="IDisposable.Dispose"/>
            public readonly void Dispose()
            {
            }

            /// <summary>
            /// Advances the <see cref="Enumerator"/> to the next element of the
            /// <see cref="EntityLookup"/>.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the <see cref="Enumerator"/> was successfully advanced to
            /// the next element; <see langword="false"/> if the <see cref="Enumerator"/> has passed
            /// the end of the <see cref="EntityLookup"/>.
            /// </returns>
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

            /// <summary>
            /// Sets the <see cref="Enumerator"/> to its initial position, which is before the first
            /// element in the <see cref="EntityLookup"/>.
            /// </summary>
            public void Reset()
            {
                m_index = -1;
            }
        }

        private struct Entry
        {
            public EntityGrouping Grouping;
            public uint HashCode;
            public int Next;
        }
    }
}
