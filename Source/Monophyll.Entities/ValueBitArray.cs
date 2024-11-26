using System;
using System.Buffers;

namespace Monophyll.Entities
{
	public ref struct ValueBitArray
	{
		private uint[]? m_rentedArray;
		private Span<uint> m_bits;
		private int m_size;

		public ValueBitArray(Span<uint> span)
		{
			m_rentedArray = null;
			m_bits = span;
			m_size = 0;
		}

		public ValueBitArray(int capacity)
		{
			m_rentedArray = ArrayPool<uint>.Shared.Rent(capacity);
			m_bits = new Span<uint>(m_rentedArray);
			m_size = 0;
		}

		public readonly int Capacity
		{
			get => m_bits.Length;
		}

		public int Length
		{
			readonly get => m_size;
			set
			{
				if ((uint)value > (uint)m_bits.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "");
				}

				m_size = value;
			}
		}

		public readonly Span<uint> RawBits
		{
			get => m_bits;
		}

		public ref uint this[int index]
		{
			get
			{
				if (index > m_size)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index, "Index is less than zero or greater than or equal to Length.");
				}

				return ref m_bits[index];
			}
		}

		public void Dispose()
		{
			uint[]? arrayToReturn = m_rentedArray;

			if (arrayToReturn != null)
			{
				this = default;
				ArrayPool<uint>.Shared.Return(arrayToReturn);
			}
		}

		public void Set(int index)
		{
			int spanIndex = index >> 5;

			if (spanIndex >= m_size)
			{
				Grow(spanIndex + 1);
			}

			m_bits[spanIndex] |= 1u << index;
		}

		private void Grow(int capacity)
		{
			if (capacity > m_bits.Length)
			{
				uint[]? rentedArray = m_rentedArray;
				uint[] array = ArrayPool<uint>.Shared.Rent(capacity);

				m_bits.CopyTo(array);

				if (rentedArray != null)
				{
					ArrayPool<uint>.Shared.Return(rentedArray);
				}

				m_bits = new Span<uint>(m_rentedArray = array);
			}

			m_bits.Slice(m_size, capacity).Clear();
			m_size = capacity;
		}

		public readonly ReadOnlySpan<uint> AsSpan()
		{
			return m_bits.Slice(0, m_size);
		}
	}
}
