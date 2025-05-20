using Monophyll.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Monophyll.Entities
{
	public class EntityTableGrouping : IGrouping<EntityArchetype, EntityTable>, IList<EntityTable>, IReadOnlyList<EntityTable>, IList
	{
		private readonly object m_lock;
		private readonly EntityArchetype m_key;
		private volatile EntityTable[] m_items;

		public EntityTableGrouping(EntityArchetype key)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			m_lock = new object();
			m_key = key;
			m_items = Array.Empty<EntityTable>();
		}

		public EntityArchetype Key
		{
			get => m_key;
		}

		public int Count
		{
			get => m_items.Length;
		}

		bool ICollection<EntityTable>.IsReadOnly
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

		public EntityTable this[int index]
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
					EntityTable[] items = m_items;

					if (items[index] != value)
					{
						EntityTable[] array = new EntityTable[items.Length];

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
					this[index] = (EntityTable)value!;
				}
				catch (InvalidCastException)
				{
					throw new ArgumentException("value is not of type EntityTable", nameof(value));
				}
			}
		}

		public void Add(EntityTable item)
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
				EntityTable[] items = m_items;
				EntityTable[] array = new EntityTable[items.Length + 1];

				Array.Copy(items, array, items.Length);
				array[items.Length] = item;
				m_items = array;
			}
		}

		int IList.Add(object? value)
		{
			try
			{
				Add((EntityTable)value!);
				return m_items.Length;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException("value is not of type EntityTable", nameof(value));
			}
		}

		public ReadOnlySpan<EntityTable> AsSpan()
		{
			return new ReadOnlySpan<EntityTable>(m_items);
		}

		public void Clear()
		{
			lock (m_lock)
			{
				m_items = Array.Empty<EntityTable>();
			}
		}

		public bool Contains(EntityTable item)
		{
			return Array.IndexOf(m_items, item) != -1;
		}

		bool IList.Contains(object? value)
		{
			return Array.IndexOf(m_items, value) != -1;
		}

		public void CopyTo(EntityTable[] array, int index)
		{
			EntityTable[] items = m_items;
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
				EntityTable[] items = m_items;
				Array.Copy(items, 0, array!, index, items.Length);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("array is not of type EntityTable[].", nameof(array));
			}
		}

		public ArrayEnumerator<EntityTable> GetEnumerator()
		{
			return new ArrayEnumerator<EntityTable>(m_items);
		}

		IEnumerator<EntityTable> IEnumerable<EntityTable>.GetEnumerator()
		{
			return new ArrayEnumerator<EntityTable>(m_items);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ArrayEnumerator<EntityTable>(m_items);
		}

		public int IndexOf(EntityTable item)
		{
			return Array.IndexOf(m_items, item);
		}

		int IList.IndexOf(object? value)
		{
            return Array.IndexOf(m_items, value);
        }

		public void Insert(int index, EntityTable item)
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
				EntityTable[] items = m_items;

				if ((uint)index > (uint)items.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
						"index is less than zero or greater than Count.");
				}

				EntityTable[] array = new EntityTable[items.Length + 1];
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
				Insert(index, (EntityTable)value!);
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException("value is not of type EntityTable", nameof(value));
			}
		}

		public bool Remove(EntityTable item)
		{
			lock (m_lock)
			{
				EntityTable[] items = m_items;
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
			Remove((value as EntityTable)!);
		}

		public void RemoveAt(int index)
		{
			lock (m_lock)
			{
				EntityTable[] items = m_items;

				if ((uint)index >= (uint)items.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index,
						"index is less than zero or greater than or equal to Count.");
				}

				m_items = RemoveAt(items, index);
			}
		}

		private static EntityTable[] RemoveAt(EntityTable[] array, int index)
		{
			if (array.Length == 1)
			{
				return Array.Empty<EntityTable>();
			}

			EntityTable[] newArray = new EntityTable[array.Length - 1];
			Array.Copy(array, newArray, index);

			if (index < newArray.Length)
			{
				Array.Copy(array, index + 1, newArray, index, newArray.Length - index);
			}

			return newArray;
		}
	}
}
