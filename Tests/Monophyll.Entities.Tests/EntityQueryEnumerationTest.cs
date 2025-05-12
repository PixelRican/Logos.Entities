using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityQueryEnumerationTest : ITestCase
	{
		public void Execute()
		{
			EntityArchetypeLookup lookup = new EntityArchetypeLookup();
			EntityArchetypeGrouping grouping = lookup.GetGrouping([
				ComponentType.TypeOf<Tag>(),
				ComponentType.TypeOf<Position2D>(),
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Scale2D>(),
				ComponentType.TypeOf<Matrix3x2>(),
				ComponentType.TypeOf<Position3D>(),
				ComponentType.TypeOf<Rotation3D>(),
				ComponentType.TypeOf<Scale3D>(),
				ComponentType.TypeOf<Matrix4x4>()]);

			for (int i = 0; i < 5; i++)
			{
				grouping.Add(new EntityArchetypeChunk(grouping.Key));
			}

			foreach (ComponentType type in grouping.Key.ComponentTypes)
			{
				grouping = lookup.GetSubgrouping(grouping.Key, type);

				for (int i = 0; i < 5; i++)
				{
					grouping.Add(new EntityArchetypeChunk(grouping.Key));
				}
			}

			EntityQuery.Enumerator enumerator = new EntityQuery(lookup, EntityFilter.Create([
				ComponentType.TypeOf<Position3D>(),
				ComponentType.TypeOf<Rotation3D>(),
				ComponentType.TypeOf<Scale3D>(),
				ComponentType.TypeOf<Matrix4x4>()], [], [])).GetEnumerator();
			int expectedCount = 25;

			while (enumerator.MoveNext())
			{
				expectedCount--;
			}

			Debug.Assert(expectedCount == 0);
		}
	}
}
