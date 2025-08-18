// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities
{
    /// <summary>
    /// Provides utility methods for bitmap operations.
    /// </summary>
    public static class BitmapOperations
    {
        /// <summary>
        /// Returns a hash code for the specified bitmap.
        /// </summary>
        /// <param name="bitmap">
        /// The bitmap for which a hash code is to be returned.
        /// </param>
        /// <returns>
        /// A hash code for <paramref name="bitmap"/>.
        /// </returns>
        public static int GetHashCode(ReadOnlySpan<int> bitmap)
        {
            HashCode hashCode = default;

            for (int i = (bitmap.Length > 8) ? bitmap.Length - 8 : 0; i < bitmap.Length; i++)
            {
                hashCode.Add(bitmap[i]);
            }

            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Determines whether the bit at the specified index is set in the specified bitmap.
        /// </summary>
        /// <param name="bitmap">
        /// The source bitmap.
        /// </param>
        /// <param name="index">
        /// The index of the bit to test in <paramref name="bitmap"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the bit at <paramref name="index"/> is set in
        /// <paramref name="bitmap"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        public static bool Test(ReadOnlySpan<int> bitmap, int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            int bitmapIndex = index >> 5;

            // Consider bits beyond the length of the bitmap as all zeroes.
            return bitmapIndex < bitmap.Length && (bitmap[bitmapIndex] & 1 << index) != 0;
        }
    }
}
