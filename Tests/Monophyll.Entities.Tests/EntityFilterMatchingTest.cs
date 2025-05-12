using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityFilterMatchingTest : ITestCase
	{
		public void Execute()
		{
			EntityArchetype transform2D = EntityArchetype.Create([
				ComponentType.TypeOf<Position2D>(),
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Scale2D>(),
				ComponentType.TypeOf<Matrix3x2>()]);
			EntityArchetype transform3D = EntityArchetype.Create([
				ComponentType.TypeOf<Position3D>(),
				ComponentType.TypeOf<Rotation3D>(),
				ComponentType.TypeOf<Scale3D>(),
				ComponentType.TypeOf<Matrix4x4>()]);
			EntityArchetype taggedTransform2D = transform2D.Add(ComponentType.TypeOf<Tag>());
			EntityArchetype taggedTransform3D = transform3D.Add(ComponentType.TypeOf<Tag>());
			EntityFilter filter = EntityFilter.Universal;

			Debug.Assert(filter.Matches(transform2D));
			Debug.Assert(filter.Matches(transform3D));
			Debug.Assert(filter.Matches(taggedTransform2D));
			Debug.Assert(filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(taggedTransform2D.ComponentTypes.Slice(4, 1), default, default);

			Debug.Assert(!filter.Matches(transform2D));
			Debug.Assert(!filter.Matches(transform3D));
			Debug.Assert(filter.Matches(taggedTransform2D));
			Debug.Assert(filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(default, taggedTransform2D.ComponentTypes.Slice(4, 1), default);

			Debug.Assert(!filter.Matches(transform2D));
			Debug.Assert(!filter.Matches(transform3D));
			Debug.Assert(filter.Matches(taggedTransform2D));
			Debug.Assert(filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(default, default, taggedTransform2D.ComponentTypes.Slice(4, 1));

			Debug.Assert(filter.Matches(transform2D));
			Debug.Assert(filter.Matches(transform3D));
			Debug.Assert(!filter.Matches(taggedTransform2D));
			Debug.Assert(!filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(transform2D.ComponentTypes.Slice(0, 4), default, default);

			Debug.Assert(filter.Matches(transform2D));
			Debug.Assert(!filter.Matches(transform3D));
			Debug.Assert(filter.Matches(taggedTransform2D));
			Debug.Assert(!filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(transform3D.ComponentTypes.Slice(0, 4), default, default);

			Debug.Assert(!filter.Matches(transform2D));
			Debug.Assert(filter.Matches(transform3D));
			Debug.Assert(!filter.Matches(taggedTransform2D));
			Debug.Assert(filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(taggedTransform2D.ComponentTypes.Slice(0, 4), default, taggedTransform2D.ComponentTypes.Slice(4, 1));

			Debug.Assert(filter.Matches(transform2D));
			Debug.Assert(!filter.Matches(transform3D));
			Debug.Assert(!filter.Matches(taggedTransform2D));
			Debug.Assert(!filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(taggedTransform3D.ComponentTypes.Slice(0, 4), default, taggedTransform3D.ComponentTypes.Slice(4, 1));

			Debug.Assert(!filter.Matches(transform2D));
			Debug.Assert(filter.Matches(transform3D));
			Debug.Assert(!filter.Matches(taggedTransform2D));
			Debug.Assert(!filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(default, taggedTransform2D.ComponentTypes.Slice(3, 2), default);

			Debug.Assert(filter.Matches(transform2D));
			Debug.Assert(!filter.Matches(transform3D));
			Debug.Assert(filter.Matches(taggedTransform2D));
			Debug.Assert(filter.Matches(taggedTransform3D));

			filter = EntityFilter.Create(default, taggedTransform3D.ComponentTypes.Slice(3, 2), default);

			Debug.Assert(!filter.Matches(transform2D));
			Debug.Assert(filter.Matches(transform3D));
			Debug.Assert(filter.Matches(taggedTransform2D));
			Debug.Assert(filter.Matches(taggedTransform3D));
		}
	}
}
