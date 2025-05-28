using System;
using System.Collections;
using System.Collections.Generic;

namespace Monophyll.Utilities
{
	public struct ArrayEnumerator<T> : IEnumerator<T>
	{
		private readonly T[] m_array;
		private readonly int m_end;
		private int m_index;

		public ArrayEnumerator()
		{
			m_array = Array.Empty<T>();
			m_end = 0;
			m_index = -1;
		}

		public ArrayEnumerator(T[] array)
		{
			ArgumentNullException.ThrowIfNull(array);

			m_array = array;
			m_end = array.Length;
			m_index = -1;
		}

		public static ArrayEnumerator<T> Empty
		{
			get => new ArrayEnumerator<T>();
		}

		public readonly T Current
		{
			get => m_array[m_index];
		}

		readonly object IEnumerator.Current
		{
			get => m_array[m_index]!;
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
