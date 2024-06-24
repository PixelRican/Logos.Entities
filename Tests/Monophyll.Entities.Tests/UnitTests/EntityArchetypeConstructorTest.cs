using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Monophyll.Entities.Test
{
	internal sealed class EntityArchetypeConstructorTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetype transform2DFromArray = new(
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Matrix3x2>(),
				ComponentType.TypeOf<Scale2D>(),
				ComponentType.TypeOf<Position2D>()
			);
			EntityArchetype transform2DFromSpan = new([
				ComponentType.TypeOf<Rotation2D>(),
				ComponentType.TypeOf<Matrix3x2>(),
				ComponentType.TypeOf<Scale2D>(),
				ComponentType.TypeOf<Position2D>()
			]);
			EntityArchetype transform2DFromIterator = new(CreateTransform2DIterator());

			Debug.Assert(transform2DFromArray.Contains(ComponentType.TypeOf<Position2D>()));
			Debug.Assert(transform2DFromArray.Contains(ComponentType.TypeOf<Rotation2D>()));
			Debug.Assert(transform2DFromArray.Contains(ComponentType.TypeOf<Scale2D>()));
			Debug.Assert(transform2DFromArray.Contains(ComponentType.TypeOf<Matrix3x2>()));

			Debug.Assert(transform2DFromArray.IndexOf(ComponentType.TypeOf<Position2D>()) != -1);
			Debug.Assert(transform2DFromArray.IndexOf(ComponentType.TypeOf<Rotation2D>()) != -1);
			Debug.Assert(transform2DFromArray.IndexOf(ComponentType.TypeOf<Scale2D>()) != -1);
			Debug.Assert(transform2DFromArray.IndexOf(ComponentType.TypeOf<Matrix3x2>()) != -1);

			Debug.Assert(transform2DFromIterator.Contains(ComponentType.TypeOf<Position2D>()));
			Debug.Assert(transform2DFromIterator.Contains(ComponentType.TypeOf<Rotation2D>()));
			Debug.Assert(transform2DFromIterator.Contains(ComponentType.TypeOf<Scale2D>()));
			Debug.Assert(transform2DFromIterator.Contains(ComponentType.TypeOf<Matrix3x2>()));

			Debug.Assert(transform2DFromIterator.IndexOf(ComponentType.TypeOf<Position2D>()) != -1);
			Debug.Assert(transform2DFromIterator.IndexOf(ComponentType.TypeOf<Rotation2D>()) != -1);
			Debug.Assert(transform2DFromIterator.IndexOf(ComponentType.TypeOf<Scale2D>()) != -1);
			Debug.Assert(transform2DFromIterator.IndexOf(ComponentType.TypeOf<Matrix3x2>()) != -1);

			Debug.Assert(transform2DFromSpan.Contains(ComponentType.TypeOf<Position2D>()));
			Debug.Assert(transform2DFromSpan.Contains(ComponentType.TypeOf<Rotation2D>()));
			Debug.Assert(transform2DFromSpan.Contains(ComponentType.TypeOf<Scale2D>()));
			Debug.Assert(transform2DFromSpan.Contains(ComponentType.TypeOf<Matrix3x2>()));

			Debug.Assert(transform2DFromSpan.IndexOf(ComponentType.TypeOf<Position2D>()) != -1);
			Debug.Assert(transform2DFromSpan.IndexOf(ComponentType.TypeOf<Rotation2D>()) != -1);
			Debug.Assert(transform2DFromSpan.IndexOf(ComponentType.TypeOf<Scale2D>()) != -1);
			Debug.Assert(transform2DFromSpan.IndexOf(ComponentType.TypeOf<Matrix3x2>()) != -1);

			Debug.Assert(transform2DFromArray.ComponentTypes.SequenceEqual(transform2DFromIterator.ComponentTypes));
			Debug.Assert(transform2DFromArray.ComponentBits.SequenceEqual(transform2DFromIterator.ComponentBits));
			Debug.Assert(transform2DFromArray.ComponentLookup.SequenceEqual(transform2DFromIterator.ComponentLookup));

			Debug.Assert(transform2DFromArray.ComponentTypes.SequenceEqual(transform2DFromSpan.ComponentTypes));
			Debug.Assert(transform2DFromArray.ComponentBits.SequenceEqual(transform2DFromSpan.ComponentBits));
			Debug.Assert(transform2DFromArray.ComponentLookup.SequenceEqual(transform2DFromSpan.ComponentLookup));
		}

		private static IEnumerable<ComponentType> CreateTransform2DIterator()
		{
			yield return ComponentType.TypeOf<Position2D>();
			yield return ComponentType.TypeOf<Rotation2D>();
			yield return ComponentType.TypeOf<Scale2D>();
			yield return ComponentType.TypeOf<Matrix3x2>();
		}
	}
}
