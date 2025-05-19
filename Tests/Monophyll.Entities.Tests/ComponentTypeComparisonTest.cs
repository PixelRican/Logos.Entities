using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class ComponentTypeComparisonTest : ITestCase
	{
		public void Execute()
		{
			ComponentType[] types =
			[
				ComponentType.TypeOf<object>(),
				ComponentType.TypeOf<Position2D>(),
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Scale2D>(),
				ComponentType.TypeOf<Matrix3x2>(),
				ComponentType.TypeOf<Position3D>(),
				ComponentType.TypeOf<Rotation3D>(),
				ComponentType.TypeOf<Scale3D>(),
				ComponentType.TypeOf<Matrix4x4>(),
				ComponentType.TypeOf<Tag>()
			];

			ComponentType previous = null!;

			foreach (ComponentType current in types)
			{
				Debug.Assert(ComponentType.Equals(current, current));
				Debug.Assert(!ComponentType.Equals(previous, current));
				Debug.Assert(!ComponentType.Equals(current, previous));
				Debug.Assert(ComponentType.Compare(current, current) == 0);
				Debug.Assert(ComponentType.Compare(previous, current) < 0);
				Debug.Assert(ComponentType.Compare(current, previous) > 0);

				previous = current;
			}
		}
	}
}
