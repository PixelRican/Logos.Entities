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
        /// Determines whether the bit at the specified index is set in the source bitmask.
        /// </summary>
        /// <param name="source">
        /// The source bitmask.
        /// </param>
        /// <param name="index">
        /// The index of the bit to test in <paramref name="source"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the bit at <paramref name="index"/> is set in
        /// <paramref name="source"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        public static bool Test(ReadOnlySpan<int> source, int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            int spanIndex = index >> 5;

            return spanIndex < source.Length
                && (source[spanIndex] & 1 << index) != 0;
        }

        /// <summary>
        /// Determines whether the target bitmask is a bitwise superset of the source bitmask.
        /// </summary>
        /// <param name="source">
        /// The source bitmask.
        /// </param>
        /// <param name="target">
        /// The target bitmask.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="target"/> is a bitwise superset of
        /// <paramref name="source"/>; otherwise, <see langword="false"/>.
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
        /// <param name="source">
        /// The source bitmask.
        /// </param>
        /// <param name="target">
        /// The target bitmask.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a bitwise intersection exists between
        /// <paramref name="target"/> and <paramref name="source"/>, or if <paramref name="source"/>
        /// is empty; otherwise, <see langword="false"/>.
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
        /// Determines whether the bitwise intersection between the target bitmask and the source
        /// bitmask is empty.
        /// </summary>
        /// <param name="source">
        /// The source bitmask.
        /// </param>
        /// <param name="target">
        /// The target bitmask.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the bitwise intersection between <paramref name="target"/> and
        /// <paramref name="source"/> is empty; otherwise, <see langword="false"/>.
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
        /// <param name="obj">
        /// The bitmask for which a hash code is to be returned.
        /// </param>
        /// <returns>
        /// A hash code for <paramref name="obj"/>.
        /// </returns>
        public static int GetHashCode(ReadOnlySpan<int> obj)
        {
            HashCode hashCode = default;

            for (int i = obj.Length > 8 ? obj.Length - 8 : 0; i < obj.Length; i++)
            {
                hashCode.Add(obj[i]);
            }

            return hashCode.ToHashCode();
        }
    }
}
