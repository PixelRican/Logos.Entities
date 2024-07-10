using System;
using System.Collections;
using System.Collections.Generic;

namespace Monophyll.Entities
{
	public sealed class ComponentBitEqualityComparer : IEqualityComparer<ReadOnlyMemory<uint>>, IEqualityComparer
	{
		private static readonly ComponentBitEqualityComparer s_instance = new();

		private ComponentBitEqualityComparer()
		{
		}

		public static ComponentBitEqualityComparer Instance
		{
			get => s_instance;
		}

		public bool Equals(ReadOnlyMemory<uint> x, ReadOnlyMemory<uint> y)
		{
			return x.Span.SequenceEqual(y.Span);
		}

		public new bool Equals(object? x, object? y)
		{
			if (x == y)
			{
				return true;
			}

			if (x == null || y == null)
			{
				return false;
			}

			if (x is ReadOnlyMemory<uint> a && y is ReadOnlyMemory<uint> b)
			{
				return a.Span.SequenceEqual(b.Span);
			}

			return x.Equals(y);
		}

		public int GetHashCode(ReadOnlyMemory<uint> obj)
		{
			ReadOnlySpan<uint> objSpan = obj.Span;
			HashCode hashCode = default;

			for (int i = 0; i < objSpan.Length; i++)
			{
				hashCode.Add(objSpan[i]);
			}

			return hashCode.ToHashCode();
		}

		public int GetHashCode(object obj)
		{
			ArgumentNullException.ThrowIfNull(obj);

			if (obj is ReadOnlyMemory<uint> memory)
			{
				return GetHashCode(memory);
			}

			return obj.GetHashCode();
		}
	}
}
