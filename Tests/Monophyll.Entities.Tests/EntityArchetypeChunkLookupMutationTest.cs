using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkLookupMutationTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetypeChunkLookup lookup = new();
			int count = 0;

			AssertLookupSuccess();
			AssertLookupSuccess(TypeOf<Position2D>());
			AssertLookupSuccess(TypeOf<Rotation2D>());
			AssertLookupSuccess(TypeOf<Scale2D>());
			AssertLookupSuccess(TypeOf<Matrix3x2>());
			AssertLookupSuccess(TypeOf<Position3D>());
			AssertLookupSuccess(TypeOf<Rotation3D>());
			AssertLookupSuccess(TypeOf<Scale3D>());
			AssertLookupSuccess(TypeOf<Matrix4x4>());
			AssertLookupSuccess(TypeOf<Tag>());
			AssertLookupSuccess(TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Tag>());
			AssertLookupSuccess(TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Tag>());
			AssertLookupSuccess(TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>());
			AssertLookupSuccess(TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>());

			count = 0;

			foreach (EntityArchetypeChunkGrouping grouping in lookup)
			{
				Debug.Assert(grouping.Key.Id == count++);
			}

			void AssertLookupSuccess(params ComponentType[] componentTypes)
			{
				EntityArchetypeChunkGrouping grouping = lookup.GetOrCreate(componentTypes);
				EntityArchetype key = grouping.Key;

				Debug.Assert(grouping == lookup[key]);
				Debug.Assert(grouping == lookup.GetOrCreate(key.Clone(key.ChunkSize, key.Id)));
				Debug.Assert(grouping == lookup.GetOrCreate(componentTypes));
				Debug.Assert(grouping == lookup.GetOrCreate((IEnumerable<ComponentType>)componentTypes));
				Debug.Assert(grouping == lookup.GetOrCreate((ReadOnlySpan<ComponentType>)componentTypes));
				Debug.Assert(lookup.Count == ++count);
			}
		}
	}
}
