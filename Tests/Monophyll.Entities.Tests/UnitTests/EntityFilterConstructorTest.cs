using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityFilterConstructorTest : IUnitTest
	{
		public void Run()
		{
			EntityFilter transform3DFilterFromArray = new((ComponentType[]) [
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()
			], (ComponentType[]) [
				TypeOf<Tag>()
			], (ComponentType[]) [
				TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()
			]);
			EntityFilter transform3DFilterFromSpan = new([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()
			], [
				TypeOf<Tag>()
			], [
				TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()
			]);
			EntityFilter transform3DFilterFromEnumerable = new((IEnumerable<ComponentType>)[
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()
			], (IEnumerable<ComponentType>)[
				TypeOf<Tag>()
			], (IEnumerable<ComponentType>)[
				TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()
			]);
			EntityFilter transform3DFilterFromBuilder =
				EntityFilter.Require([TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()])
				.Include([TypeOf<Tag>()])
				.Exclude([TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()])
				.Build();

			Debug.Assert(transform3DFilterFromArray.Requires(TypeOf<Position3D>()));
			Debug.Assert(transform3DFilterFromArray.Requires(TypeOf<Rotation3D>()));
			Debug.Assert(transform3DFilterFromArray.Requires(TypeOf<Scale3D>()));
			Debug.Assert(transform3DFilterFromArray.Requires(TypeOf<Matrix4x4>()));
			Debug.Assert(transform3DFilterFromArray.Includes(TypeOf<Tag>()));
			Debug.Assert(transform3DFilterFromArray.Excludes(TypeOf<Position2D>()));
			Debug.Assert(transform3DFilterFromArray.Excludes(TypeOf<Rotation2D>()));
			Debug.Assert(transform3DFilterFromArray.Excludes(TypeOf<Scale2D>()));
			Debug.Assert(transform3DFilterFromArray.Excludes(TypeOf<Matrix3x2>()));

			Debug.Assert(transform3DFilterFromSpan.Requires(TypeOf<Position3D>()));
			Debug.Assert(transform3DFilterFromSpan.Requires(TypeOf<Rotation3D>()));
			Debug.Assert(transform3DFilterFromSpan.Requires(TypeOf<Scale3D>()));
			Debug.Assert(transform3DFilterFromSpan.Requires(TypeOf<Matrix4x4>()));
			Debug.Assert(transform3DFilterFromSpan.Includes(TypeOf<Tag>()));
			Debug.Assert(transform3DFilterFromSpan.Excludes(TypeOf<Position2D>()));
			Debug.Assert(transform3DFilterFromSpan.Excludes(TypeOf<Rotation2D>()));
			Debug.Assert(transform3DFilterFromSpan.Excludes(TypeOf<Scale2D>()));
			Debug.Assert(transform3DFilterFromSpan.Excludes(TypeOf<Matrix3x2>()));

			Debug.Assert(transform3DFilterFromEnumerable.Requires(TypeOf<Position3D>()));
			Debug.Assert(transform3DFilterFromEnumerable.Requires(TypeOf<Rotation3D>()));
			Debug.Assert(transform3DFilterFromEnumerable.Requires(TypeOf<Scale3D>()));
			Debug.Assert(transform3DFilterFromEnumerable.Requires(TypeOf<Matrix4x4>()));
			Debug.Assert(transform3DFilterFromEnumerable.Includes(TypeOf<Tag>()));
			Debug.Assert(transform3DFilterFromEnumerable.Excludes(TypeOf<Position2D>()));
			Debug.Assert(transform3DFilterFromEnumerable.Excludes(TypeOf<Rotation2D>()));
			Debug.Assert(transform3DFilterFromEnumerable.Excludes(TypeOf<Scale2D>()));
			Debug.Assert(transform3DFilterFromEnumerable.Excludes(TypeOf<Matrix3x2>()));

			Debug.Assert(transform3DFilterFromBuilder.Requires(TypeOf<Position3D>()));
			Debug.Assert(transform3DFilterFromBuilder.Requires(TypeOf<Rotation3D>()));
			Debug.Assert(transform3DFilterFromBuilder.Requires(TypeOf<Scale3D>()));
			Debug.Assert(transform3DFilterFromBuilder.Requires(TypeOf<Matrix4x4>()));
			Debug.Assert(transform3DFilterFromBuilder.Includes(TypeOf<Tag>()));
			Debug.Assert(transform3DFilterFromBuilder.Excludes(TypeOf<Position2D>()));
			Debug.Assert(transform3DFilterFromBuilder.Excludes(TypeOf<Rotation2D>()));
			Debug.Assert(transform3DFilterFromBuilder.Excludes(TypeOf<Scale2D>()));
			Debug.Assert(transform3DFilterFromBuilder.Excludes(TypeOf<Matrix3x2>()));

			Debug.Assert(transform3DFilterFromArray.RequiredComponentTypes.SequenceEqual(transform3DFilterFromSpan.RequiredComponentTypes));
			Debug.Assert(transform3DFilterFromArray.IncludedComponentTypes.SequenceEqual(transform3DFilterFromSpan.IncludedComponentTypes));
			Debug.Assert(transform3DFilterFromArray.ExcludedComponentTypes.SequenceEqual(transform3DFilterFromSpan.ExcludedComponentTypes));
			Debug.Assert(transform3DFilterFromArray.RequiredComponentBits.SequenceEqual(transform3DFilterFromSpan.RequiredComponentBits));
			Debug.Assert(transform3DFilterFromArray.IncludedComponentBits.SequenceEqual(transform3DFilterFromSpan.IncludedComponentBits));
			Debug.Assert(transform3DFilterFromArray.ExcludedComponentBits.SequenceEqual(transform3DFilterFromSpan.ExcludedComponentBits));

			Debug.Assert(transform3DFilterFromSpan.RequiredComponentTypes.SequenceEqual(transform3DFilterFromEnumerable.RequiredComponentTypes));
			Debug.Assert(transform3DFilterFromSpan.IncludedComponentTypes.SequenceEqual(transform3DFilterFromEnumerable.IncludedComponentTypes));
			Debug.Assert(transform3DFilterFromSpan.ExcludedComponentTypes.SequenceEqual(transform3DFilterFromEnumerable.ExcludedComponentTypes));
			Debug.Assert(transform3DFilterFromSpan.RequiredComponentBits.SequenceEqual(transform3DFilterFromEnumerable.RequiredComponentBits));
			Debug.Assert(transform3DFilterFromSpan.IncludedComponentBits.SequenceEqual(transform3DFilterFromEnumerable.IncludedComponentBits));
			Debug.Assert(transform3DFilterFromSpan.ExcludedComponentBits.SequenceEqual(transform3DFilterFromEnumerable.ExcludedComponentBits));

			Debug.Assert(transform3DFilterFromEnumerable.RequiredComponentTypes.SequenceEqual(transform3DFilterFromBuilder.RequiredComponentTypes));
			Debug.Assert(transform3DFilterFromEnumerable.IncludedComponentTypes.SequenceEqual(transform3DFilterFromBuilder.IncludedComponentTypes));
			Debug.Assert(transform3DFilterFromEnumerable.ExcludedComponentTypes.SequenceEqual(transform3DFilterFromBuilder.ExcludedComponentTypes));
			Debug.Assert(transform3DFilterFromEnumerable.RequiredComponentBits.SequenceEqual(transform3DFilterFromBuilder.RequiredComponentBits));
			Debug.Assert(transform3DFilterFromEnumerable.IncludedComponentBits.SequenceEqual(transform3DFilterFromBuilder.IncludedComponentBits));
			Debug.Assert(transform3DFilterFromEnumerable.ExcludedComponentBits.SequenceEqual(transform3DFilterFromBuilder.ExcludedComponentBits));
		}
	}
}
