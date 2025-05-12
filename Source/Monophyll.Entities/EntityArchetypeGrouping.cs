using Monophyll.Entities.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Monophyll.Entities
{
	public class EntityArchetypeGrouping : IGrouping<EntityArchetype, EntityArchetypeChunk>, IList<EntityArchetypeChunk>, IReadOnlyList<EntityArchetypeChunk>, IList
	{
		private readonly object m_lock;
		private readonly EntityArchetype m_key;
		private volatile EntityArchetypeChunk[] m_items;

		public EntityArchetypeGrouping(EntityArchetype key)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			m_lock = new object();
			m_key = key;
			m_items = Array.Empty<EntityArchetypeChunk>();
		}

		public EntityArchetype Key
		{
			get => m_key;
		}

		public int Count
		{
			get => m_items.Length;
		}

		bool ICollection<EntityArchetypeChunk>.IsReadOnly
		{
			get => false;
		}

		bool IList.IsReadOnly
		{
			get => false;
		}

		bool IList.IsFixedSize
		{
			get => false;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => this;
		}

		public EntityArchetypeChunk this[int index]
		{
			get => m_items[index];
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (!EntityArchetype.Equals(m_key, value.Archetype))
				{
					throw new ArgumentException("value.Archetype does not match Key.", nameof(value));
				}

				lock (m_lock)
				{
					EntityArchetypeChunk[] items = m_items;

					if (items[index] != value)
					{
						EntityArchetypeChunk[] array = new EntityArchetypeChunk[items.Length];

						Array.Copy(items, array, items.Length);
						array[index] = value;
						m_items = array;
					}
				}
			}
		}

		object? IList.this[int index]
		{
			get => m_items[index];
			set
			{
				try
				{
					this[index] = (EntityArchetypeChunk)value!;
				}
				catch (InvalidCastException)
				{
					throw new ArgumentException("value is not of type EntityArchetypeChunk", nameof(value));
				}
			}
		}

		public void Add(EntityArchetypeChunk item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (!EntityArchetype.Equals(m_key, item.Archetype))
			{
				throw new ArgumentException("item.Archetype does not match Key.", nameof(item));
			}

			lock (m_lock)
			{
				EntityArchetypeChunk[] items = m_items;
				EntityArchetypeChunk[] array = new EntityArchetypeChunk[items.Length + 1];

				Array.Copy(items, array, items.Length);
				array[items.Length] = item;
				m_items = array;
			}
		}

		int IList.Add(object? value)
		{
			try
			{
				Add((EntityArchetypeChunk)value!);
				return m_items.Length;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException("value is not of type EntityArchetypeChunk", nameof(value));
			}
		}

		public ReadOnlySpan<EntityArchetypeChunk> AsSpan()
		{
			return new ReadOnlySpan<EntityArchetypeChunk>(m_items);
		}

		public void Clear()
		{
			lock (m_lock)
			{
				m_items = Array.Empty<EntityArchetypeChunk>();
			}
		}

		public bool Contains(EntityArchetypeChunk item)
		{
			return Array.IndexOf(m_items, item) != -1;
		}

		bool IList.Contains(object? value)
		{
			return Array.IndexOf(m_items, value as EntityArchetypeChunk) != -1;
		}

		public void CopyTo(EntityArchetypeChunk[] array, int index)
		{
			EntityArchetypeChunk[] items = m_items;
			Array.Copy(items, 0, array, index, items.Length);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if ((array != null) && (array.Rank != 1))
			{
				throw new ArgumentException("Multi-dimensional arrays are not supported.", nameof(array));
			}

			try
			{
				EntityArchetypeChunk[] items = m_items;
				Array.Copy(items, 0, array!, index, items.Length);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("array is not of type EntityArchetypeChunk[].", nameof(array));
			}
		}

		public ArrayEnumerator<EntityArchetypeChunk> GetEnumerator()
		{
			return new ArrayEnumerator<EntityArchetypeChunk>(m_items);
		}

		IEnumerator<EntityArchetypeChunk> IEnumerable<EntityArchetypeChunk>.GetEnumerator()
		{
			return new ArrayEnumerator<EntityArchetypeChunk>(m_items);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ArrayEnumerator<EntityArchetypeChunk>(m_items);
		}

		public int IndexOf(EntityArchetypeChunk item)
		{
			return Array.IndexOf(m_items, item);
		}

		int IList.IndexOf(object? value)
		{
			if (value is EntityArchetypeChunk item)
			{
				return Array.IndexOf(m_items, item);
			}

			return -1;
		}

		public void Insert(int index, EntityArchetypeChunk item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (!EntityArchetype.Equals(m_key, item.Archetype))
			{
				throw new ArgumentException("item.Archetype does not match Key.", nameof(item));
			}

			lock (m_lock)
			{
				EntityArchetypeChunk[] items = m_items;

				if ((uint)index > (uint)items.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
						"index is less than zero or greater than Count.");
				}

				EntityArchetypeChunk[] array = new EntityArchetypeChunk[items.Length + 1];
				Array.Copy(items, array, index);
				array[index] = item;

				if (index < items.Length)
				{
					Array.Copy(items, index, array, index + 1, items.Length - index);
				}

				m_items = array;
			}
		}

		void IList.Insert(int index, object? value)
		{
			try
			{
				Insert(index, (EntityArchetypeChunk)value!);
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException("value is not of type EntityArchetypeChunk", nameof(value));
			}
		}

		public bool Remove(EntityArchetypeChunk item)
		{
			lock (m_lock)
			{
				EntityArchetypeChunk[] items = m_items;
				int index = Array.IndexOf(items, item);

				if (index != -1)
				{
					m_items = RemoveAt(items, index);
					return true;
				}

				return false;
			}
		}

		void IList.Remove(object? value)
		{
			Remove((value as EntityArchetypeChunk)!);
		}

		public void RemoveAt(int index)
		{
			lock (m_lock)
			{
				EntityArchetypeChunk[] items = m_items;

				if ((uint)index >= (uint)items.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
						"index is less than zero or greater than or equal to Count.");
				}

				m_items = RemoveAt(items, index);
			}
		}

		private static EntityArchetypeChunk[] RemoveAt(EntityArchetypeChunk[] array, int index)
		{
			if (array.Length == 1)
			{
				return Array.Empty<EntityArchetypeChunk>();
			}

			EntityArchetypeChunk[] newArray = new EntityArchetypeChunk[array.Length - 1];
			Array.Copy(array, newArray, index);

			if (index < newArray.Length)
			{
				Array.Copy(array, index + 1, newArray, index, newArray.Length - index);
			}

			return newArray;
		}
	}
}
