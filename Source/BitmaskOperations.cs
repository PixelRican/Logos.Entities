// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities
{
    /// <summary>
    /// Provides utility methods for bitmask operations.
    /// </summary>
    public static class BitmaskOperations
    {
        /// <summary>
        /// Indicates whether a specified bit index is set within a bitmask.
        /// </summary>
        /// 
        /// <param name="bitmask">
        /// The source bitmask.
        /// </param>
        /// 
        /// <param name="index">
        /// The bit index to test.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if set, <see langword="false"/> otherwise.
        /// </returns>
        public static bool Test(ReadOnlySpan<uint> bitmask, int index)
        {
            int bitmaskIndex;
            return (bitmaskIndex = index >> 5) < bitmask.Length
                && (bitmask[bitmaskIndex] & 1u << index) != 0;
        }

        /// <summary>
        /// Determines whether a bitmask is a subset of a filter using bitwise comparisons.
        /// </summary>
        /// 
        /// <param name="filter">
        /// The bitmask to query.
        /// </param>
        /// 
        /// <param name="bitmask">
        /// The bitmask to test.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the bitmask is a subset of the filter,
        /// <see langword="false"/> otherwise.
        /// </returns>
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

        /// <summary>
        /// Determines whether a bitmask intersects with a filter using bitwise comparisons. If
        /// not, the method returns a value indicating whether a filter is empty.
        /// </summary>
        /// 
        /// <param name="filter">
        /// The bitmask to query.
        /// </param>
        /// 
        /// <param name="bitmask">
        /// The bitmask to test.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the bitmask intersects with the filter or if the filter is
        /// empty, <see langword="false"/> otherwise.
        /// </returns>
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

            return filter.IsEmpty;
        }

        /// <summary>
        /// Determines whether a bitmask is disjoint from a filter using bitwise comparisons.
        /// </summary>
        /// 
        /// <param name="filter">
        /// The bitmask to query.
        /// </param>
        /// 
        /// <param name="bitmask">
        /// The bitmask to test.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the bitmask is disjoint from the filter,
        /// <see langword="false"/> otherwise.
        /// </returns>
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

        /// <summary>
        /// Returns a hash code for the specified bitmask.
        /// </summary>
        /// 
        /// <param name="bitmask">
        /// The bitmask for which a hash code is to be returned.
        /// </param>
        /// 
        /// <returns>
        /// A hash code for the specified bitmask.
        /// </returns>
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
