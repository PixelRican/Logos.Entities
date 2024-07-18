using System;
using System.Collections;
using System.Collections.Generic;

namespace Monophyll.Entities
{
	public sealed class EntityArchetypeChunkGroup : IReadOnlyCollection<EntityArchetypeChunk>, ICollection
	{
		private readonly EntityArchetype m_archetype;
		private EntityArchetypeChunk? m_top;
		private int m_count;
		private int m_version;

		public EntityArchetypeChunkGroup(EntityArchetype archetype)
		{
			ArgumentNullException.ThrowIfNull(archetype);
			m_archetype = archetype;
		}

		public EntityArchetype Archetype
		{
			get => m_archetype;
		}

		public EntityArchetypeChunk? Top
		{
			get => m_top;
		}

		public int Count
		{
			get => m_count;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => this;
		}

		public EntityArchetypeChunk Allocate()
		{
			EntityArchetypeChunk result = m_top == null ? new(m_archetype) : new(m_top);
			m_top = result;
			m_count++;
			m_version++;
			return result;
		}

		public EntityArchetypeChunk? Deallocate()
		{
			if (m_top == null)
			{
				return null;
			}

			EntityArchetypeChunk result = m_top;
			m_top = result.Next;
			m_count--;
			m_version++;
			return result;
		}

		public void CopyTo(EntityArchetypeChunk[] array, int arrayIndex)
		{
			ArgumentNullException.ThrowIfNull(array);
			ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);

			if (array.Length - arrayIndex < m_count)
			{
				throw new ArgumentException("The array does not have enough space to fit the EntityArchetypeChunkGroup's elements.");
			}

			for (EntityArchetypeChunk? chunk = m_top; chunk != null; chunk = chunk.Next)
			{
				array[arrayIndex++] = chunk;
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

			ArgumentOutOfRangeException.ThrowIfNegative(index);

			if (array.Length - index < m_count)
			{
				throw new ArgumentException("The array does not have enough space to fit the EntityArchetypeChunkGroup's elements.");
			}

			if (array is EntityArchetypeChunk[] chunks)
			{
				for (EntityArchetypeChunk? chunk = m_top; chunk != null; chunk = chunk.Next)
				{
					chunks[index++] = chunk;
				}
			}
			else
			{
				if (array is not object[] objects)
				{
					throw new ArgumentException("The array is not of a type which EntityArchetypeChunk derives from.", nameof(array));
				}

				try
				{
					for (EntityArchetypeChunk? chunk = m_top; chunk != null; chunk = chunk.Next)
					{
						objects[index++] = chunk;
					}
				}
				catch (ArrayTypeMismatchException)
				{
					throw new ArgumentException("The array is not of a type which EntityArchetypeChunk derives from.", nameof(array));
				}
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

		public struct Enumerator : IEnumerator<EntityArchetypeChunk>
		{
			private readonly EntityArchetypeChunkGroup m_group;
			private EntityArchetypeChunk? m_current;
			private readonly int m_version;

			internal Enumerator(EntityArchetypeChunkGroup group)
			{
				m_group = group;
				m_current = group.Top;
				m_version = group.m_version;
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
				if (m_version != m_group.m_version)
				{
					throw new InvalidOperationException(
						"The EntityArchetypeChunkGroup was modified after the enumerator was created.");
				}

				return (m_current = m_current?.Next) != null;
			}

			void IEnumerator.Reset()
			{
				if (m_version != m_group.m_version)
				{
					throw new InvalidOperationException(
						"The EntityArchetypeChunkGroup was modified after the enumerator was created.");
				}

				m_current = m_group.m_top;
			}
		}
	}
}
