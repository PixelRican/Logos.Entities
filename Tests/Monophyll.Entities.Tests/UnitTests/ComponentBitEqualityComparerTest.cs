using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Test
{
	internal class ComponentBitEqualityComparerTest : IUnitTest
	{
		public void Run()
		{
			Dictionary<ReadOnlyMemory<uint>, EntityArchetype> dictionary = CreateDictionary();
			uint[] buffer = new uint[1];

			Debug.Assert(dictionary.Count == 3);
			Debug.Assert(dictionary.ContainsKey(ReadOnlyMemory<uint>.Empty));

			buffer[0] = (1u << ComponentType.TypeOf<Position2D>().SequenceNumber) |
						(1u << ComponentType.TypeOf<Rotation2D>().SequenceNumber) |
						(1u << ComponentType.TypeOf<Scale2D>().SequenceNumber) |
						(1u << ComponentType.TypeOf<Matrix3x2>().SequenceNumber);

			Debug.Assert(dictionary.ContainsKey(buffer));

			buffer[0] = (1u << ComponentType.TypeOf<Position3D>().SequenceNumber) |
						(1u << ComponentType.TypeOf<Rotation3D>().SequenceNumber) |
						(1u << ComponentType.TypeOf<Scale3D>().SequenceNumber) |
						(1u << ComponentType.TypeOf<Matrix4x4>().SequenceNumber);

			Debug.Assert(dictionary.ContainsKey(buffer));

			buffer[0] = 1u << ComponentType.TypeOf<Tag>().SequenceNumber;

			Debug.Assert(!dictionary.ContainsKey(buffer));
		}

		private static Dictionary<ReadOnlyMemory<uint>, EntityArchetype> CreateDictionary()
		{
			Dictionary<ReadOnlyMemory<uint>, EntityArchetype> dictionary = new(3, ComponentBitEqualityComparer.Instance);
			EntityArchetype archetype = EntityArchetype.Base;

			dictionary.Add(archetype.ComponentBits.AsMemory(), archetype);

			archetype = new([
				ComponentType.TypeOf<Position2D>(),
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Scale2D>(),
				ComponentType.TypeOf<Matrix3x2>()
			]);

			dictionary.Add(archetype.ComponentBits.AsMemory(), archetype);

			archetype = new([
				ComponentType.TypeOf<Position3D>(),
				ComponentType.TypeOf<Rotation3D>(),
				ComponentType.TypeOf<Scale3D>(),
				ComponentType.TypeOf<Matrix4x4>()
			]);

			dictionary.Add(archetype.ComponentBits.AsMemory(), archetype);
			return dictionary;
		}
	}
}
