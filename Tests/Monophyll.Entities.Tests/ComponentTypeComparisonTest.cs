using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class ComponentTypeComparisonTest : IUnitTest
	{
		public void Run()
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

			ComponentType previousType = null!;

			foreach (ComponentType currentType in types)
			{
				Debug.Assert(ComponentType.Equals(currentType, currentType));
				Debug.Assert(!ComponentType.Equals(previousType, currentType));
				Debug.Assert(!ComponentType.Equals(currentType, previousType));
				Debug.Assert(ComponentType.Compare(currentType, currentType) == 0);
				Debug.Assert(ComponentType.Compare(previousType, currentType) < 0);
				Debug.Assert(ComponentType.Compare(currentType, previousType) > 0);

				previousType = currentType;
			}
		}
	}
}
