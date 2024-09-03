using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityFilterCreationTest : IUnitTest
	{
		public void Run()
		{
			EntityFilter transform3DFilterFromArray = EntityFilter.Create(
				(ComponentType[])[TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()],
				(ComponentType[])[TypeOf<Tag>()],
				(ComponentType[])[TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()]);

			EntityFilter transform3DFilterFromSpan = EntityFilter.Create(
				[TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()],
				[TypeOf<Tag>()],
				[TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()]);

			EntityFilter transform3DFilterFromEnumerable = EntityFilter.Create((IEnumerable<ComponentType>)
				[TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()],
				[TypeOf<Tag>()],
				[TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()]);

			EntityFilter transform3DFilterFromBuilder = EntityFilter.CreateBuilder()
				.Require([TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()])
				.Include([TypeOf<Tag>()])
				.Exclude([TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()])
				.Build();

			AssertFilterCorrectness(transform3DFilterFromArray);
			AssertFilterCorrectness(transform3DFilterFromSpan);
			AssertFilterCorrectness(transform3DFilterFromEnumerable);
			AssertFilterCorrectness(transform3DFilterFromBuilder);
		}

		private static void AssertFilterCorrectness(EntityFilter filter)
		{
			Debug.Assert(filter.Requires(TypeOf<Position3D>()));
			Debug.Assert(filter.Requires(TypeOf<Rotation3D>()));
			Debug.Assert(filter.Requires(TypeOf<Scale3D>()));
			Debug.Assert(filter.Requires(TypeOf<Matrix4x4>()));
			Debug.Assert(filter.Includes(TypeOf<Tag>()));
			Debug.Assert(filter.Excludes(TypeOf<Position2D>()));
			Debug.Assert(filter.Excludes(TypeOf<Rotation2D>()));
			Debug.Assert(filter.Excludes(TypeOf<Scale2D>()));
			Debug.Assert(filter.Excludes(TypeOf<Matrix3x2>()));
		}
	}
}
