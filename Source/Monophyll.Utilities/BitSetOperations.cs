using System;

namespace Monophyll.Utilities
{
	public static class BitSetOperations
	{
		public static bool Contains(ReadOnlySpan<uint> span, int index)
		{
			int spanIndex;
			return index >= 0
				&& (spanIndex = index >> 5) < span.Length
				&& (span[spanIndex] & 1u << index) != 0;
        }

		public static int GetHashCode(ReadOnlySpan<uint> span)
		{
			int result = 0;

			for (int i = span.Length > 8 ? span.Length - 8 : 0; i < span.Length; i++)
			{
				result = (result << 5) + result ^ (int)span[i];
			}

			return result;
		}

		public static bool IsSubsetOf(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
		{
			for (int i = 0, length = Math.Min(a.Length, b.Length); i < length; i++)
			{
                if ((a[i] & ~b[i]) != 0)
                {
                    return false;
                }
            }

			for (int i = b.Length; i < a.Length; i++)
			{
                if (a[i] != 0)
                {
                    return false;
                }
            }

			return true;
		}

		public static bool Overlaps(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
		{
			for (int i = 0, length = Math.Min(a.Length, b.Length); i < length; i++)
			{
				if ((a[i] & b[i]) != 0)
				{
					return true;
				}
			}

			return false;
		}
	}
}
