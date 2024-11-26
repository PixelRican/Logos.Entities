using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityQueryEnumerationTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetypeChunkLookup lookup = new EntityArchetypeChunkLookup();
			EntityQuery universalQuery = new EntityQuery(lookup, EntityFilter.Universal);
			EntityQuery tagQuery = new EntityQuery(lookup, EntityFilter.Create([TypeOf<Tag>()], [], []));
			EntityQuery noTagQuery = new EntityQuery(lookup, EntityFilter.Create([], [], [TypeOf<Tag>()]));
			EntityQuery transform2DQuery = new EntityQuery(lookup, EntityFilter.Create(
				[TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()], [], []));
			EntityQuery transform3DQuery = new EntityQuery(lookup, EntityFilter.Create(
				[TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()], [], []));

			InitializeGrouping(lookup.GetOrCreate(
				[TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()]));
			InitializeGrouping(lookup.GetOrCreate(
				[TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>(), TypeOf<Tag>()]));
			InitializeGrouping(lookup.GetOrCreate(
				[TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()]));
			InitializeGrouping(lookup.GetOrCreate(
				[TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>(), TypeOf<Tag>()]));

			AssertQueryCorrectness(universalQuery, 20);
			AssertQueryCorrectness(tagQuery, 10);
			AssertQueryCorrectness(noTagQuery, 10);
			AssertQueryCorrectness(transform2DQuery, 10);
			AssertQueryCorrectness(transform3DQuery, 10);
		}

		private static void InitializeGrouping(EntityArchetypeChunkGrouping grouping)
		{
			EntityArchetypeChunk chunk = new EntityArchetypeChunk(grouping.Key);

			for (int i = 0; i < 5; i++)
			{
				grouping.TryAdd(chunk);
			}
		}

		private static void AssertQueryCorrectness(EntityQuery query, int expectedCount)
		{
			while (query.MoveNext())
			{
				expectedCount--;
			}

			Debug.Assert(expectedCount == 0);
		}
	}
}
