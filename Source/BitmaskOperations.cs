// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for more details.

using System;

namespace Monophyll.Entities
{
	public static class BitmaskOperations
	{
		public static bool Contains(ReadOnlySpan<uint> bitmask, int index)
		{
			int bitmaskIndex;
			return (bitmaskIndex = index >> 5) < bitmask.Length
				&& (bitmask[bitmaskIndex] & 1u << index) != 0;
		}

		public static bool Requires(ReadOnlySpan<uint> filter, ReadOnlySpan<uint> bitmask)
		{
			if (filter.Length > bitmask.Length)
			{
				return false;
			}

			for (int i = 0; i < filter.Length; i++)
			{
				if ((filter[i] & ~bitmask[i]) != 0)
				{
					return false;
				}
			}

			return true;
		}

		public static bool Excludes(ReadOnlySpan<uint> filter, ReadOnlySpan<uint> bitmask)
		{
			int length = Math.Min(filter.Length, bitmask.Length);

			for (int i = 0; i < length; i++)
			{
				if ((filter[i] & bitmask[i]) != 0)
				{
					return false;
				}
			}

			return true;
		}

		public static bool Includes(ReadOnlySpan<uint> filter, ReadOnlySpan<uint> bitmask)
		{
			int length = Math.Min(filter.Length, bitmask.Length);

			for (int i = 0; i < length; i++)
			{
				if ((filter[i] & bitmask[i]) != 0)
				{
					return true;
				}
			}

			return filter.Length == 0;
		}

		public static int GetHashCode(ReadOnlySpan<uint> bitmask)
		{
			int result = 0;

			for (int i = bitmask.Length > 8 ? bitmask.Length - 8 : 0; i < bitmask.Length; i++)
			{
				result = (result << 5) + result ^ (int)bitmask[i];
			}

			return result;
		}
	}
}
