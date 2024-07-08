using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityFilterMatchTest : IUnitTest
	{
		public void Run()
		{
			EntityFilter filter =
				EntityFilter.Require([TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()])
				.Include([TypeOf<Tag>()])
				.Exclude([TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()])
				.Build();

			Debug.Assert(filter.Matches(new EntityArchetype([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>(), TypeOf<Tag>()
			])));
			Debug.Assert(!filter.Matches(new EntityArchetype([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Tag>()
			])));
			Debug.Assert(!filter.Matches(new EntityArchetype([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>(), TypeOf<Matrix3x2>(), TypeOf<Tag>()
			])));
			Debug.Assert(!filter.Matches(new EntityArchetype([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Tag>()
			])));
			Debug.Assert(!filter.Matches(new EntityArchetype([
				TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>(), TypeOf<Tag>()
			])));
			Debug.Assert(!filter.Matches(EntityArchetype.Base));
		}
	}
}
