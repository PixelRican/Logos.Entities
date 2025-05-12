using System;
using System.Collections;
using System.Collections.Generic;

namespace Monophyll.Entities.Utilities
{
	public struct ArrayEnumerator<T> : IEnumerator<T>, IEnumerator
	{
		private readonly T[] m_items;
		private readonly int m_end;
		private int m_index;

		public ArrayEnumerator(T[]? items)
		{
			m_items = items ?? Array.Empty<T>();
			m_end = m_items.Length;
			m_index = -1;
		}

		public static ArrayEnumerator<T> Empty
		{
			get => new ArrayEnumerator<T>(Array.Empty<T>());
		}

		public readonly T Current
		{
			get => m_items[m_index];
		}

		readonly object IEnumerator.Current
		{
			get => m_items[m_index]!;
		}

		public readonly void Dispose()
		{
		}

		public bool MoveNext()
		{
			int index = m_index + 1;

			if (index < m_end)
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
}
