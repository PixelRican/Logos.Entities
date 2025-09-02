// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Logos.Entities
{
    /// <summary>
    /// Represents a collection of archetypes each mapped to one or more tables.
    /// </summary>
    public sealed class EntityLookup : ILookup<EntityArchetype, EntityTable>,
        ICollection<EntityGrouping>, ICollection, IReadOnlyCollection<EntityGrouping>
    {
        private const int BranchIndexMask = 0x1F;
        private const int TwigIndexMask = 0x03;
        private const int TwigLevel = 6;
        private const int StackallocIntBufferSizeLimit = 32;

        private static readonly EntityLookup s_empty = new EntityLookup();

        private readonly Node m_root;
        private readonly int m_count;

        private EntityLookup()
        {
        }

        private EntityLookup(Node root, int count)
        {
            m_root = root;
            m_count = count;
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
            get => m_count;
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
            get => m_count == 0;
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

                EntityGrouping? item = FindItem(key.ComponentBitmap);

                if (item == null)
                {
                    ThrowForKeyNotFound();
                }

                return item;
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
            return InsertItem(item, throwOnDuplicateKey: true);
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
            return InsertItem(item, throwOnDuplicateKey: false);
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
            if (m_count == 0)
            {
                return this;
            }

            return s_empty;
        }

        /// <summary>
        /// Determines whether a specified key exists in the <see cref="EntityLookup"/>.
        /// </summary>
        /// <param name="key">
        /// The key to search for in the <see cref="EntityLookup"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="key"/> is in the <see cref="EntityLookup"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public bool Contains(EntityArchetype key)
        {
            ArgumentNullException.ThrowIfNull(key);
            return FindItem(key.ComponentBitmap) != null;
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public bool Contains(EntityGrouping item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return FindItem(item.Key.ComponentBitmap) == item;
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
        /// <paramref name="array"/> is <see langword="null"/>.
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

            int upperBound = arrayIndex + m_count;

            if ((uint)array.Length < (uint)upperBound)
            {
                ThrowForInsufficientArraySpace();
            }

            if (arrayIndex == upperBound)
            {
                return;
            }

            int level = 0;
            int indexStack = 0;
            JaggedNodeArray subtrie = default;
            Array children = m_root.Children;

            // Perform a depth-first trie traversal using iteration.
            while (true)
            {
                // Climb up the trie from the current branch node to reach a twig node.
                while (level < TwigLevel)
                {
                    Node[] branches = subtrie[level++] = (Node[])children;

                    children = branches[0].Children;
                    indexStack <<= 5;
                }

                EntityGrouping[][] twigs = (EntityGrouping[][])children;

                // Climb up the twig nodes to access the leaf nodes and copy their contents to the
                // array.
                for (int twigIndex = 0; twigIndex < twigs.Length; twigIndex++)
                {
                    EntityGrouping[] leaves = twigs[twigIndex];

                    for (int leafIndex = 0; leafIndex < leaves.Length; leafIndex++)
                    {
                        array[arrayIndex++] = leaves[leafIndex];
                    }
                }

                // Return if every leaf node has been visited.
                if (arrayIndex == upperBound)
                {
                    return;
                }

                // Climb down all completely explored branch nodes and move onto the next branch
                // node.
                while (true)
                {
                    int branchIndex = (indexStack & BranchIndexMask) + 1;
                    int subtrieIndex = level - 1;
                    Node[] branches = subtrie[subtrieIndex];

                    if (branchIndex < branches.Length)
                    {
                        children = branches[branchIndex].Children;
                        indexStack++;
                        break;
                    }

                    indexStack >>>= 5;
                    level = subtrieIndex;
                }
            }
        }

        /// <summary>
        /// Creates a copy of the <see cref="EntityLookup"/> with the sequence of values indexed by
        /// the specified key removed from it.
        /// </summary>
        /// <param name="key">
        /// The key of the sequence of values to remove from the copy of the
        /// <see cref="EntityLookup"/>.
        /// </param>
        /// <returns>
        /// A copy of the <see cref="EntityLookup"/> with the sequence of values indexed by
        /// <paramref name="key"/> removed from it, or the <see cref="EntityLookup"/> if
        /// <paramref name="key"/> is not found in it.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public EntityLookup Remove(EntityArchetype key)
        {
            ArgumentNullException.ThrowIfNull(key);
            return RemoveItem(key.ComponentBitmap, comparand: null);
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public EntityLookup Remove(EntityGrouping item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return RemoveItem(item.Key.ComponentBitmap, comparand: item);
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
        /// <param name="item">
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
        public bool TryGetValue(EntityArchetype key, [NotNullWhen(true)] out EntityGrouping? item)
        {
            ArgumentNullException.ThrowIfNull(key);
            return (item = FindItem(key.ComponentBitmap)) != null;
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
        /// <param name="item">
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
        public bool TryGetValueWith(EntityArchetype key, ComponentType componentType, [NotNullWhen(true)] out EntityGrouping? item)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<int> componentBitmap = key.ComponentBitmap;
            int index = componentType.Index >> 5;
            int bit = 1 << componentType.Index;
            int length;
            int[]? rentedArray;
            scoped Span<int> buffer;

            if (index < componentBitmap.Length)
            {
                if ((bit & componentBitmap[index]) != 0)
                {
                    return TryGetValue(key, out item);
                }

                length = componentBitmap.Length;
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

            componentBitmap.CopyTo(buffer);
            buffer.Slice(componentBitmap.Length).Clear();
            buffer[index] |= bit;

            bool result = (item = FindItem(buffer)) != null;

            if (rentedArray != null)
            {
                ArrayPool<int>.Shared.Return(rentedArray);
            }

            return result;
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
        /// <param name="item">
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
        public bool TryGetValueWithout(EntityArchetype key, ComponentType componentType, [NotNullWhen(true)] out EntityGrouping? item)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(componentType);

            ReadOnlySpan<int> componentBitmap = key.ComponentBitmap;
            int index = componentType.Index >> 5;
            int bit = 1 << componentType.Index;

            if (index >= componentBitmap.Length || (bit & componentBitmap[index]) == 0)
            {
                return TryGetValue(key, out item);
            }

            int[]? rentedArray;
            scoped Span<int> buffer;

            if (componentBitmap.Length <= StackallocIntBufferSizeLimit)
            {
                rentedArray = null;
                buffer = stackalloc int[componentBitmap.Length];
            }
            else
            {
                rentedArray = ArrayPool<int>.Shared.Rent(componentBitmap.Length);
                buffer = new Span<int>(rentedArray, 0, componentBitmap.Length);
            }

            componentBitmap.CopyTo(buffer);

            if ((buffer[index] ^= bit) == 0 && buffer.Length == index + 1)
            {
                ReadOnlySpan<ComponentType> componentTypes = key.ComponentTypes;

                buffer = (componentTypes.Length > 1)
                    ? buffer.Slice(0, componentTypes[^2].Index + 32 >> 5)
                    : Span<int>.Empty;
            }

            bool result = (item = FindItem(buffer)) != null;

            if (rentedArray != null)
            {
                ArrayPool<int>.Shared.Return(rentedArray);
            }

            return result;
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

            int upperBound = index + m_count;

            if ((uint)array.Length < (uint)upperBound)
            {
                ThrowForInsufficientArraySpace();
            }

            if (index == upperBound)
            {
                return;
            }

            object[]? objects = array as object[];

            if (objects == null)
            {
                ThrowForInvalidArrayType();
            }

            int level = 0;
            int indexStack = 0;
            JaggedNodeArray subtrie = default;
            Array children = m_root.Children;

            try
            {
                // See the comments in the strongly typed version of this method for more
                // information on how this traversal works.
                while (true)
                {
                    while (level < TwigLevel)
                    {
                        Node[] branches = subtrie[level++] = (Node[])children;

                        children = branches[0].Children;
                        indexStack <<= 5;
                    }

                    EntityGrouping[][] twigs = (EntityGrouping[][])children;

                    for (int twigIndex = 0; twigIndex < twigs.Length; twigIndex++)
                    {
                        EntityGrouping[] leaves = twigs[twigIndex];

                        for (int leafIndex = 0; leafIndex < leaves.Length; leafIndex++)
                        {
                            objects[index++] = leaves[leafIndex];
                        }
                    }

                    if (index == upperBound)
                    {
                        return;
                    }

                    while (true)
                    {
                        int branchIndex = (indexStack & BranchIndexMask) + 1;
                        int subtrieIndex = level - 1;
                        Node[] branches = subtrie[subtrieIndex];

                        if (branchIndex < branches.Length)
                        {
                            children = branches[branchIndex].Children;
                            indexStack++;
                            break;
                        }

                        indexStack >>>= 5;
                        level = subtrieIndex;
                    }
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
        private static void ThrowForDuplicateKey()
        {
            throw new ArgumentException(
                "An element with the same key already exists in the EntityLookup.", "item");
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetChildIndex(int bitmap, int mask)
        {
            return BitOperations.PopCount((uint)(bitmap & mask - 1));
        }

        private EntityGrouping? FindItem(ReadOnlySpan<int> componentBitmap)
        {
            // In Phil Bagwell's hash array mapped trie, hash codes are treated as an encoding for
            // the path of nodes that need to be taken in order to reach a leaf node. The hash code
            // is split into six 5-bit partitions and one 2-bit partition. Each 5-bit partition is
            // used as index in each branch nodes bitmap, which is used to check if an edge exists
            // from one branch node to another. If an edge does exist from one branch to another,
            // its index in the source branch node's array of children can be determined by counting
            // the number of bits to the right of the bitmap index. The 2-bit partition is used
            // similarly as the previous 5-bit partitions, with the main difference being that they
            // are used to index twig nodes, which contain leaf nodes as children.
            int hashCode = BitmapOperations.GetHashCode(componentBitmap);
            int mask = 1 << hashCode;
            Node node = m_root;

            // Climb up the branch nodes until a twig node has been reached.
            for (int level = 0; level < TwigLevel; level++)
            {
                // Return null when a path to the next branch node does not exist.
                if ((node.Bitmap & mask) == 0)
                {
                    return null;
                }

                Node[] branches = (Node[])node.Children;

                node = branches[GetChildIndex(node.Bitmap, mask)];
                mask = 1 << (hashCode >>>= 5);
            }

            // Check if the branch node the loop stopped at has a path to the requested twig node.
            if ((node.Bitmap & mask) != 0)
            {
                EntityGrouping[][] twigs = (EntityGrouping[][])node.Children;
                EntityGrouping[] leaves = twigs[GetChildIndex(node.Bitmap, mask)];

                // Climb up the twig node to the leaf nodes. Perform a linear search to filter out
                // items with the same hash code.
                for (int i = 0; i < leaves.Length; i++)
                {
                    EntityGrouping leaf = leaves[i];

                    if (leaf.Key.ComponentBitmap.SequenceEqual(componentBitmap))
                    {
                        return leaf;
                    }
                }
            }

            return null;
        }

        private EntityLookup InsertItem(EntityGrouping item, bool throwOnDuplicateKey)
        {
            // For time efficiency purposes, this method performs allocations during traversal. This
            // may lead to increased memory pressure on the garbage collector if insertion fails.
            // Callers should not attempt to insert duplicate items if they care about memory
            // efficiency.
            ArgumentNullException.ThrowIfNull(item);

            // See the top comment in the FindItem method for information on how hash codes are used
            // to traverse the trie.
            ReadOnlySpan<int> componentBitmap = item.Key.ComponentBitmap;
            int hashCode = BitmapOperations.GetHashCode(componentBitmap);
            int mask = 1 << hashCode;
            Node root = m_root;
            ref Node current = ref root;

            // Climb up the branch nodes until a twig node has been reached.
            for (int level = 0; level < TwigLevel; level++)
            {
                ReadOnlySpan<Node> sourceBranches = new ReadOnlySpan<Node>((Node[])current.Children);
                int branchIndex = GetChildIndex(current.Bitmap, mask);
                Node[] branches;

                if ((current.Bitmap & mask) == 0)
                {
                    // Create shallow copies of existing branch nodes along with a new branch node.
                    current.Bitmap |= mask;
                    branches = new Node[sourceBranches.Length + 1];

                    Span<Node> destinationBranches = new Span<Node>(branches);

                    sourceBranches.Slice(0, branchIndex).CopyTo(destinationBranches);
                    sourceBranches.Slice(branchIndex).CopyTo(destinationBranches.Slice(branchIndex + 1));
                }
                else
                {
                    // Create shallow copies of existing branch nodes.
                    branches = sourceBranches.ToArray();
                }

                // Update the current branch node and move up to the next branch node.
                current.Children = branches;
                current = ref branches[branchIndex];
                mask = 1 << (hashCode >>>= 5);
            }

            ReadOnlySpan<EntityGrouping[]> sourceTwigs =
                new ReadOnlySpan<EntityGrouping[]>((EntityGrouping[][])current.Children);
            int twigIndex = GetChildIndex(current.Bitmap, mask);
            int count = m_count;
            EntityGrouping[][] twigs;
            EntityGrouping[] leaves;

            // Construct the twig and leaf nodes necessary to accommodate the new item.
            if ((current.Bitmap & mask) == 0)
            {
                // Create shallow copies of existing twig nodes along with a new twig node, which
                // will contain the item as its sole child node.
                current.Bitmap |= mask;
                twigs = new EntityGrouping[sourceTwigs.Length + 1][];

                Span<EntityGrouping[]> destinationTwigs = new Span<EntityGrouping[]>(twigs);

                sourceTwigs.Slice(0, twigIndex).CopyTo(destinationTwigs);
                sourceTwigs.Slice(twigIndex).CopyTo(destinationTwigs.Slice(twigIndex + 1));
                leaves = new EntityGrouping[] { item };
                count++;
            }
            else
            {
                // Create shallow copies of existing twig nodes and insert the item to its array of
                // children.
                ReadOnlySpan<EntityGrouping> sourceLeaves =
                    new ReadOnlySpan<EntityGrouping>(sourceTwigs[twigIndex]);
                int leafIndex = 0;

                while (true)
                {
                    EntityGrouping leaf = sourceLeaves[leafIndex];

                    // Do not throw when the item is already in the trie. Just discard the
                    // previously allocated nodes and have the garbage collection clean them up.
                    if (leaf == item)
                    {
                        return this;
                    }

                    // If a duplicate key has been found, determine whether to throw an exception
                    // or replace the existing leaf node with the item.
                    if (leaf.Key.ComponentBitmap.SequenceEqual(componentBitmap))
                    {
                        if (throwOnDuplicateKey)
                        {
                            ThrowForDuplicateKey();
                        }

                        leaves = sourceLeaves.ToArray();
                        break;
                    }

                    // If a leaf node with an equivalent key could not be found, insert the item as
                    // a new leaf in the twig's array of children.
                    if (++leafIndex == sourceLeaves.Length)
                    {
                        leaves = new EntityGrouping[sourceLeaves.Length + 1];
                        sourceLeaves.CopyTo(new Span<EntityGrouping>(leaves));
                        count++;
                        break;
                    }
                }

                twigs = sourceTwigs.ToArray();
                leaves[leafIndex] = item;
            }

            current.Children = twigs;
            twigs[twigIndex] = leaves;
            return new EntityLookup(root, count);
        }

        private EntityLookup RemoveItem(ReadOnlySpan<int> componentBitmap, EntityGrouping? comparand)
        {
            // See the top comment in the FindItem method for information on how hash codes are used
            // to traverse the trie.
            int hashCode = BitmapOperations.GetHashCode(componentBitmap);
            int mask = 1 << hashCode;
            int indexQueue = 0;
            int pruneMask = 0;
            int pruneLevel = -1;
            Node root = m_root;
            Node node = root;

            // Climb up the branch nodes until a twig node has been reached.
            for (int level = 0; level < TwigLevel; level++)
            {
                // Return self when a path to the next branch node does not exist.
                if ((node.Bitmap & mask) == 0)
                {
                    return this;
                }

                Node[] branches = (Node[])node.Children;
                int branchIndex = GetChildIndex(node.Bitmap, mask);

                // Determine the level at which branch nodes can be safely cut off in order to
                // reduce the amount of allocations needed to construct the final trie.
                if (branches.Length > 1)
                {
                    pruneMask = mask;
                    pruneLevel = level;
                }

                // Enqueue the branch index taken so that it can be quickly retrieved during the
                // construction of the final trie.
                indexQueue |= branchIndex << level * 5;
                node = branches[branchIndex];
                mask = 1 << (hashCode >>>= 5);
            }

            // Return self when a path to the requested twig node does not exist.
            if ((node.Bitmap & mask) == 0)
            {
                return this;
            }

            ReadOnlySpan<EntityGrouping[]> sourceTwigs =
                new ReadOnlySpan<EntityGrouping[]>((EntityGrouping[][])node.Children);
            int twigIndex = GetChildIndex(node.Bitmap, mask);
            ReadOnlySpan<EntityGrouping> sourceLeaves =
                new ReadOnlySpan<EntityGrouping>(sourceTwigs[twigIndex]);
            int leafIndex = 0;

            // Determine which leaf node contains the requested item.
            while (true)
            {
                EntityGrouping leaf = sourceLeaves[leafIndex];

                if (leaf.Key.ComponentBitmap.SequenceEqual(componentBitmap))
                {
                    // Check if the caller is requesting to remove a specific item, or any item with
                    // a specific key. The final trie will only be constructed if either one of the
                    // following conditions are satisfied.
                    if (comparand == null || comparand == leaf)
                    {
                        break;
                    }

                    return this;
                }

                // Return self if the requested item could not be found among the leaf nodes.
                if (++leafIndex == sourceLeaves.Length)
                {
                    return this;
                }
            }

            if (sourceLeaves.Length > 1)
            {
                // Create a shallow copy of the twig nodes and remove the item from the source twig
                // node's array of children.
                EntityGrouping[][]? twigs = sourceTwigs.ToArray();
                EntityGrouping[] leaves = new EntityGrouping[sourceLeaves.Length - 1];
                Span<EntityGrouping> destinationLeaves = new Span<EntityGrouping>(leaves);

                node.Children = twigs;
                twigs[twigIndex] = leaves;
                sourceLeaves.Slice(0, leafIndex).CopyTo(destinationLeaves);
                sourceLeaves.Slice(leafIndex + 1).CopyTo(destinationLeaves.Slice(leafIndex));
                pruneLevel = TwigLevel;
            }
            else if ((node.Bitmap ^= mask) != 0)
            {
                // Create a shallow copy of the twig nodes and prune the twig node containing the
                // item to remove.
                EntityGrouping[][]? twigs = new EntityGrouping[sourceTwigs.Length - 1][];
                Span<EntityGrouping[]> destinationTwigs = new Span<EntityGrouping[]>(twigs);

                node.Children = twigs;
                sourceTwigs.Slice(0, twigIndex).CopyTo(destinationTwigs);
                sourceTwigs.Slice(twigIndex + 1).CopyTo(destinationTwigs.Slice(twigIndex));
                pruneLevel = TwigLevel;
            }
            else if (pruneLevel == -1)
            {
                // Return an empty trie if there was only one item in the trie.
                return s_empty;
            }

            ref Node current = ref root;

            // Create shallow copies of the branch nodes that should be preserved in the final trie.
            while (pruneLevel > 0)
            {
                Node[] branches = new ReadOnlySpan<Node>((Node[])current.Children).ToArray();

                current.Children = branches;
                current = ref branches[indexQueue & BranchIndexMask];
                indexQueue >>>= 5;
                pruneLevel--;
            }

            // Check if the twig node containing the item among its children should be preserved in
            // the final trie.
            if (node.Bitmap == 0)
            {
                // If the twig node needs to be excluded from the final trie, prune the branch node
                // connecting it to the rest of the trie.
                ReadOnlySpan<Node> sourceBranches = new ReadOnlySpan<Node>((Node[])current.Children);
                Node[] branches = new Node[sourceBranches.Length - 1];
                Span<Node> destinationBranches = new Span<Node>(branches);
                int branchIndex = indexQueue & BranchIndexMask;

                sourceBranches.Slice(0, branchIndex).CopyTo(destinationBranches);
                sourceBranches.Slice(branchIndex + 1).CopyTo(destinationBranches.Slice(branchIndex));
                current.Bitmap ^= pruneMask;
                current.Children = branches;
            }
            else
            {
                // If the twig node should be preserved in the final trie, then assign the current
                // node to the updated twig node. This is the only case where assigning the twig
                // node directly to the current node is valid.
                current = node;
            }

            return new EntityLookup(root, m_count - 1);
        }

        /// <summary>
        /// Enumerates the elements of an <see cref="EntityLookup"/>.
        /// </summary>
        public sealed class Enumerator : IEnumerator<EntityGrouping>
        {
            private readonly EntityLookup m_lookup;
            private JaggedNodeArray m_subtrie;
            private EntityGrouping[][]? m_twigs;
            private EntityGrouping[]? m_leaves;
            private EntityGrouping? m_current;
            private int m_count;
            private int m_indexStack;
            private int m_leafIndex;

            internal Enumerator(EntityLookup lookup)
            {
                m_lookup = lookup;
                Reset();
            }

            /// <summary>
            /// Gets the element in the <see cref="EntityLookup"/> at the current position of the
            /// <see cref="Enumerator"/>.
            /// </summary>
            /// <returns>
            /// The element in the <see cref="EntityLookup"/> at the current position of the
            /// <see cref="Enumerator"/>.
            /// </returns>
            public EntityGrouping Current
            {
                get => m_current!;
            }

            object IEnumerator.Current
            {
                get => m_current!;
            }

            /// <inheritdoc cref="IDisposable.Dispose"/>
            public void Dispose()
            {
                m_subtrie = default;
                m_twigs = null;
                m_leaves = null;
                m_current = null;
                m_count = 0;
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
                int count = m_count;

                if (count == 0)
                {
                    return false;
                }

                // Check leaf nodes for the next item.
                EntityGrouping[] leaves = m_leaves!;
                int index = m_leafIndex + 1;

                if (index < leaves.Length)
                {
                    m_current = leaves[index];
                    m_count = count - 1;
                    m_leafIndex = index;
                    return true;
                }

                // If the next item could not be retrieved from the leaf nodes, then try to find one
                // in the leaf nodes from the next twig node.
                EntityGrouping[][] twigs = m_twigs!;
                int indexStack = m_indexStack;

                index = (indexStack & TwigIndexMask) + 1;

                if (index < twigs.Length)
                {
                    leaves = twigs[index];

                    m_leaves = leaves;
                    m_current = leaves[0];
                    m_count = count - 1;
                    m_indexStack = indexStack + 1;
                    m_leafIndex = 0;
                    return true;
                }

                // If the next twig node could not be found, then the branch nodes need to be
                // traversed via a depth-first search in order to find the next item.
                ref JaggedNodeArray subtrie = ref m_subtrie;
                int level = TwigLevel;
                Array children;

                indexStack >>>= 2;

                // Climb down all fully explored branch nodes.
                while (true)
                {
                    int subtrieIndex = level - 1;
                    Node[] branches = subtrie[subtrieIndex];

                    index = (indexStack & BranchIndexMask) + 1;

                    if (index < branches.Length)
                    {
                        children = branches[index].Children;
                        indexStack++;
                        break;
                    }

                    indexStack >>>= 5;
                    level = subtrieIndex;
                }

                // Climb up to the twig node containing the next item in its leaf nodes.
                while (level < TwigLevel)
                {
                    Node[] branches = subtrie[level++] = (Node[])children;

                    children = branches[0].Children;
                    indexStack <<= 5;
                }

                // Retrieve the next item from the first leaf node in the twig node.
                twigs = (EntityGrouping[][])children;
                leaves = twigs[0];

                m_twigs = twigs;
                m_leaves = leaves;
                m_current = leaves[0];
                m_count = count - 1;
                m_indexStack = indexStack << 2;
                m_leafIndex = 0;
                return true;
            }

            /// <summary>
            /// Sets the <see cref="Enumerator"/> to its initial position, which is before the first
            /// element in the <see cref="EntityLookup"/>.
            /// </summary>
            public void Reset()
            {
                EntityLookup lookup = m_lookup;
                int count = lookup.m_count;

                // Return early if the source lookup was empty.
                if (count == 0)
                {
                    return;
                }

                // Set up a stack of branch nodes to traverse once enumeration starts.
                ref JaggedNodeArray subtrie = ref m_subtrie;
                Array children = lookup.m_root.Children;

                for (int level = 0; level < TwigLevel; level++)
                {
                    Node[] branches = subtrie[level] = (Node[])children;

                    children = branches[0].Children;
                }

                // Get the first twig node encountered and retrieve the array of leaf nodes
                // containing the first set of items.
                EntityGrouping[][] twigs = (EntityGrouping[][])children;
                EntityGrouping[] leaves = twigs[0];

                m_twigs = twigs;
                m_leaves = leaves;
                m_count = count;
                m_leafIndex = -1;
            }
        }

        [InlineArray(TwigLevel)]
        private struct JaggedNodeArray
        {
            public Node[] Reference;
        }

        private struct Node
        {
            public int Bitmap;
            public Array Children;
        }
    }
}
