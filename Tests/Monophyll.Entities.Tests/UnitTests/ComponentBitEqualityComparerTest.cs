using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class ComponentBitEqualityComparerTest : IUnitTest
	{
		public void Run()
		{
			Dictionary<ReadOnlyMemory<uint>, EntityArchetype> dictionary = CreateDictionary();
			uint[] buffer = new uint[1];

			Debug.Assert(dictionary.Count == 3);
			Debug.Assert(dictionary.ContainsKey(ReadOnlyMemory<uint>.Empty));

			buffer[0] = (1u << TypeOf<Position2D>().Id) |
						(1u << TypeOf<Rotation2D>().Id) |
						(1u << TypeOf<Scale2D>().Id) |
						(1u << TypeOf<Matrix3x2>().Id);

			Debug.Assert(dictionary.ContainsKey(buffer));

			buffer[0] = (1u << TypeOf<Position3D>().Id) |
						(1u << TypeOf<Rotation3D>().Id) |
						(1u << TypeOf<Scale3D>().Id) |
						(1u << TypeOf<Matrix4x4>().Id);

			Debug.Assert(dictionary.ContainsKey(buffer));

			buffer[0] = 1u << TypeOf<Tag>().Id;

			Debug.Assert(!dictionary.ContainsKey(buffer));
		}

		private static Dictionary<ReadOnlyMemory<uint>, EntityArchetype> CreateDictionary()
		{
			Dictionary<ReadOnlyMemory<uint>, EntityArchetype> dictionary = new(3, ComponentBitEqualityComparer.Instance);
			EntityArchetype archetype = EntityArchetype.Base;
			dictionary.Add(archetype.ComponentBits.AsMemory(), archetype);

			archetype = new([TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()]);
			dictionary.Add(archetype.ComponentBits.AsMemory(), archetype);

			archetype = new([TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()]);
			dictionary.Add(archetype.ComponentBits.AsMemory(), archetype);
			return dictionary;
		}
	}
}
