using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityQueryEnumerationTest : ITestCase
	{
		public void Execute()
		{
			EntityTableLookup lookup = new EntityTableLookup();
			EntityTableGrouping grouping = lookup.GetGrouping([
				ComponentType.TypeOf<Tag>(),
				ComponentType.TypeOf<Position2D>(),
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Scale2D>(),
				ComponentType.TypeOf<Matrix3x2>(),
				ComponentType.TypeOf<Position3D>(),
				ComponentType.TypeOf<Rotation3D>(),
				ComponentType.TypeOf<Scale3D>(),
				ComponentType.TypeOf<Matrix4x4>()]);
            EntityQuery query = new EntityQuery(lookup, EntityFilter.Create([
                ComponentType.TypeOf<Position3D>(),
                ComponentType.TypeOf<Rotation3D>(),
                ComponentType.TypeOf<Scale3D>(),
                ComponentType.TypeOf<Matrix4x4>()], [], []));

            for (int i = 0; i < 5; i++)
			{
				grouping.Add(new EntityTable(grouping.Key));
			}

			foreach (ComponentType type in grouping.Key.ComponentTypes)
			{
				grouping = lookup.GetSubgrouping(grouping.Key, type);

				for (int i = 0; i < 5; i++)
				{
					grouping.Add(new EntityTable(grouping.Key));
				}
			}

			int expectedCount = 0;

			foreach (EntityTable table in query)
			{
				expectedCount++;
			}

			Debug.Assert(expectedCount == 25);
		}
	}
}
