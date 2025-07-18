// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities
{
    /// <summary>
    /// Provides utility methods for bitmask operations.
    /// </summary>
    public static class BitmaskOperations
    {
        /// <summary>
        /// Determines whether the specified bit index is set within the source bitmask.
        /// </summary>
        /// 
        /// <param name="source">
        /// The source bitmask.
        /// </param>
        /// 
        /// <param name="index">
        /// The bit index to test within <paramref name="source"/>.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the bit index is set within the source bitmask; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool Test(ReadOnlySpan<int> source, int index)
        {
            int sourceIndex;
            return (sourceIndex = index >> 5) < source.Length
                && (source[sourceIndex] & 1 << index) != 0;
        }

        /// <summary>
        /// Determines whether the target bitmask is a bitwise superset of the source bitmask.
        /// </summary>
        /// 
        /// <param name="source">
        /// The source bitmask.
        /// </param>
        /// 
        /// <param name="target">
        /// The target bitmask.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target bitmask is a superset of the source bitmask;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool Requires(ReadOnlySpan<int> source, ReadOnlySpan<int> target)
        {
            if (source.Length > target.Length)
            {
                return false;
            }

            for (int i = 0; i < source.Length; i++)
            {
                if ((source[i] & ~target[i]) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether there exists a bitwise intersection between the target bitmask and
        /// the source bitmask. If not, the method returns a value indicating whether the source
        /// bitmask is empty.
        /// </summary>
        /// 
        /// <param name="source">
        /// The source bitmask.
        /// </param>
        /// 
        /// <param name="target">
        /// The target bitmask.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target bitmask intersects with the source bitmask or if
        /// the source bitmask is empty; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool Includes(ReadOnlySpan<int> source, ReadOnlySpan<int> target)
        {
            int length = Math.Min(source.Length, target.Length);

            for (int i = 0; i < length; i++)
            {
                if ((source[i] & target[i]) != 0)
                {
                    return true;
                }
            }

            return source.IsEmpty;
        }

        /// <summary>
        /// Determines whether there does not exist a bitwise intersection between the target
        /// bitmask and the source bitmask.
        /// </summary>
        /// 
        /// <param name="source">
        /// The source bitmask.
        /// </param>
        /// 
        /// <param name="target">
        /// The target bitmask.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target bitmask does not intersect with the source
        /// bitmask; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool Excludes(ReadOnlySpan<int> source, ReadOnlySpan<int> target)
        {
            int length = Math.Min(source.Length, target.Length);

            for (int i = 0; i < length; i++)
            {
                if ((source[i] & target[i]) != 0)
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
        /// <param name="obj">
        /// The bitmask for which a hash code is to be returned.
        /// </param>
        /// 
        /// <returns>
        /// A hash code for the bitmask.
        /// </returns>
        public static int GetHashCode(ReadOnlySpan<int> obj)
        {
            int result = 0;

            for (int i = obj.Length > 8 ? obj.Length - 8 : 0; i < obj.Length; i++)
            {
                result = (result << 5) + result ^ obj[i];
            }

            return result;
        }
    }
}
