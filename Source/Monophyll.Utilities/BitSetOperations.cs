using System;

namespace Monophyll.Utilities
{
	public static class BitSetOperations
	{
		public static int GetHashCode(ReadOnlySpan<uint> obj)
		{
			int result = 0;

			for (int i = obj.Length > 8 ? obj.Length - 8 : 0; i < obj.Length; i++)
			{
				result = (result << 5) + result ^ (int)obj[i];
			}

			return result;
		}

		public static bool IsSubsetOf(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
		{
			int i = a.Length;

			while (i > b.Length)
			{
				if (a[--i] != 0)
				{
					return false;
				}
			}

			while (--i >= 0)
			{
				if ((a[i] & ~b[i]) != 0)
				{
					return false;
				}
			}

			return true;
		}

		public static bool Overlaps(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
		{
			for (int i = Math.Min(a.Length, b.Length) - 1; i >= 0; i--)
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
