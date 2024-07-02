using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeConstructorTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetype transform2DFromArray = new(
				TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()
			);
			EntityArchetype transform2DFromSpan = new([
				TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()
			]);
			EntityArchetype transform2DFromEnumerable = new((IEnumerable<ComponentType>)[
				TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()
			]);

			Debug.Assert(transform2DFromArray.Contains(TypeOf<Position2D>()));
			Debug.Assert(transform2DFromArray.Contains(TypeOf<Rotation2D>()));
			Debug.Assert(transform2DFromArray.Contains(TypeOf<Scale2D>()));
			Debug.Assert(transform2DFromArray.Contains(TypeOf<Matrix3x2>()));

			Debug.Assert(transform2DFromEnumerable.Contains(TypeOf<Position2D>()));
			Debug.Assert(transform2DFromEnumerable.Contains(TypeOf<Rotation2D>()));
			Debug.Assert(transform2DFromEnumerable.Contains(TypeOf<Scale2D>()));
			Debug.Assert(transform2DFromEnumerable.Contains(TypeOf<Matrix3x2>()));

			Debug.Assert(transform2DFromSpan.Contains(TypeOf<Position2D>()));
			Debug.Assert(transform2DFromSpan.Contains(TypeOf<Rotation2D>()));
			Debug.Assert(transform2DFromSpan.Contains(TypeOf<Scale2D>()));
			Debug.Assert(transform2DFromSpan.Contains(TypeOf<Matrix3x2>()));

			Debug.Assert(transform2DFromArray.ComponentTypes.SequenceEqual(transform2DFromSpan.ComponentTypes));
			Debug.Assert(transform2DFromArray.ComponentBits.SequenceEqual(transform2DFromSpan.ComponentBits));
			Debug.Assert(transform2DFromArray.ComponentOffsets.SequenceEqual(transform2DFromSpan.ComponentOffsets));

			Debug.Assert(transform2DFromSpan.ComponentTypes.SequenceEqual(transform2DFromEnumerable.ComponentTypes));
			Debug.Assert(transform2DFromSpan.ComponentBits.SequenceEqual(transform2DFromEnumerable.ComponentBits));
			Debug.Assert(transform2DFromSpan.ComponentOffsets.SequenceEqual(transform2DFromEnumerable.ComponentOffsets));

			Debug.Assert(transform2DFromEnumerable.ComponentTypes.SequenceEqual(transform2DFromArray.ComponentTypes));
			Debug.Assert(transform2DFromEnumerable.ComponentBits.SequenceEqual(transform2DFromArray.ComponentBits));
			Debug.Assert(transform2DFromEnumerable.ComponentOffsets.SequenceEqual(transform2DFromArray.ComponentOffsets));
		}
	}
}
