using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Monophyll.Entities
{
	public class EntityArchetypeChunkGrouping : IGrouping<EntityArchetype, EntityArchetypeChunk>, IProducerConsumerCollection<EntityArchetypeChunk>, IReadOnlyCollection<EntityArchetypeChunk>
	{
		private readonly EntityArchetype m_key;
		private volatile Node? m_head;

		public EntityArchetypeChunkGrouping(EntityArchetype key)
		{
			ArgumentNullException.ThrowIfNull(key);
			m_key = key;
		}

		public EntityArchetype Key
		{
			get => m_key;
		}

		public int Count
		{
			get
			{
				int count = 0;

				for (Node? current = m_head; current != null; current = current.Next)
				{
					count++;
				}

				return count;
			}
		}

		public bool IsEmpty
		{
			get => m_head == null;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => this;
		}

		public void Clear()
		{
			m_head = null;
		}

		public void CopyTo(EntityArchetypeChunk[] array, int index)
		{
			ArgumentNullException.ThrowIfNull(array);

			if ((uint)index >= (uint)array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Array index is less than zero or greater than or equal to array length.");
			}

			Node? head = m_head;
			int count = 0;

			for (Node? current = head; current != null; current = current.Next)
			{
				count++;
			}

			if (array.Length - index < count)
			{
				throw new ArgumentException(
					"The array does not have enough space to fit the items within the EntityArchetypeChunkGrouping.", nameof(array));
			}

			for (Node? current = head; current != null; current = current.Next)
			{
				array[index++] = current.Chunk;
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			ArgumentNullException.ThrowIfNull(array);

			if (array.Rank != 1)
			{
				throw new ArgumentException("Multi-dimensional arrays are not supported.", nameof(array));
			}

			if (array.GetLowerBound(0) != 0)
			{
				throw new ArgumentException("Arrays with non-zero lower bounds are not supported.", nameof(array));
			}

			if ((uint)index >= (uint)array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index,
					"Index is less than zero or greater than or equal to array length.");
			}

			Node? head = m_head;
			int count = 0;

			for (Node? current = head; current != null; current = current.Next)
			{
				count++;
			}

			if (array.Length - index < count)
			{
				throw new ArgumentException(
					"The array does not have enough space to fit the items within the EntityArchetypeChunkGrouping.", nameof(array));
			}

			if (array is object[] objects)
			{
				try
				{
					for (Node? current = head; current != null; current = current.Next)
					{
						objects[index++] = current.Chunk;
					}
				}
				catch (ArrayTypeMismatchException)
				{
					throw new ArgumentException("The array is not of type EntityArchetypeChunk[] or its invariants.");
				}
			}
			else
			{
				throw new ArgumentException("The array is not of type EntityArchetypeChunk[] or its invariants.");
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<EntityArchetypeChunk> IEnumerable<EntityArchetypeChunk>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		public bool TryAdd(EntityArchetypeChunk item)
		{
			if (item == null || !EntityArchetype.Equals(m_key, item.Archetype))
			{
				return false;
			}

			_ = new Node(this, item);
			return true;
		}

		public bool TryPeek([MaybeNullWhen(false)] out EntityArchetypeChunk item)
		{
			Node? head = m_head;

			if (head == null)
			{
				item = null;
				return false;
			}

			item = head.Chunk;
			return true;
		}

		public bool TryTake([MaybeNullWhen(false)] out EntityArchetypeChunk item)
		{
			Node? head = m_head;
			Node? node;

			do
			{
				if (head == null)
				{
					item = null;
					return false;
				}

				node = head;
				head = Interlocked.CompareExchange(ref m_head, head.Next, head);
			}
			while (head != node);

			item = node.Chunk;
			return true;
		}

		public EntityArchetypeChunk[] ToArray()
		{
			Node? head = m_head;
			int count = 0;

			for (Node? current = head; current != null; current = current.Next)
			{
				count++;
			}

			if (count == 0)
			{
				return Array.Empty<EntityArchetypeChunk>();
			}

			EntityArchetypeChunk[] array = new EntityArchetypeChunk[count];
			count = 0;

			for (Node? current = head; current != null; current = current.Next)
			{
				array[count++] = current.Chunk;
			}

			return array;
		}

		public struct Enumerator : IEnumerator<EntityArchetypeChunk>
		{
			private Node? m_node;
			private EntityArchetypeChunk? m_current;

			internal Enumerator(EntityArchetypeChunkGrouping grouping)
			{
				m_node = grouping.m_head;
				m_current = null;
			}

			public readonly EntityArchetypeChunk Current
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
				Node? localNode = m_node;

				if (localNode != null)
				{
					m_current = localNode.Chunk;
					m_node = localNode.Next!;
					return true;
				}

				m_current = null;
				return false;
			}

			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
		}

		private sealed class Node
		{
			public readonly EntityArchetypeChunk Chunk;
			public readonly Node? Next;

			public Node(EntityArchetypeChunkGrouping grouping, EntityArchetypeChunk chunk)
			{
				Node? head = grouping.m_head;
				Chunk = chunk;

				do
				{
					Next = head;
					head = Interlocked.CompareExchange(ref grouping.m_head, this, head);
				}
				while (Next != head);
			}
		}
	}
}
