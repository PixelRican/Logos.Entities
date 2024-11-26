using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeCreationTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetype transform2DFromArray = EntityArchetype.Create((ComponentType[])
				[TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()], 0, 0);

			EntityArchetype transform2DFromSpan = EntityArchetype.Create(
				[TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()], 0, 0);

			EntityArchetype transform2DFromEnumerable = EntityArchetype.Create((IEnumerable<ComponentType>)
				[TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()], 0, 0);

			EntityArchetype transform2DWithDuplicates = EntityArchetype.Create(
				[TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>(),
				TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()], 0, 0);

			EntityArchetype transform2DWithNulls = EntityArchetype.Create(
				[null!, TypeOf<Matrix3x2>(), null!, TypeOf<Position2D>(),
				TypeOf<Rotation2D>(), null!, TypeOf<Scale2D>(), null!], 0, 0);

			try
			{
				EntityArchetype.Create(null!, 0, 0);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				EntityArchetype.Create((IEnumerable<ComponentType>)null!, 0, 0);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}

			AssertArchetypeCorrectness(transform2DFromArray);
			AssertArchetypeCorrectness(transform2DFromSpan);
			AssertArchetypeCorrectness(transform2DFromEnumerable);
			AssertArchetypeCorrectness(transform2DWithDuplicates);
			AssertArchetypeCorrectness(transform2DWithNulls);
		}

		private static void AssertArchetypeCorrectness(EntityArchetype archetype)
		{
			Debug.Assert(archetype.Contains(TypeOf<Position2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Rotation2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Scale2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Matrix3x2>()));

			ImmutableArray<ComponentType> componentTypes = archetype.ComponentTypes;

			Debug.Assert(componentTypes[0] == TypeOf<Position2D>());
			Debug.Assert(componentTypes[1] == TypeOf<Rotation2D>());
			Debug.Assert(componentTypes[2] == TypeOf<Scale2D>());
			Debug.Assert(componentTypes[3] == TypeOf<Matrix3x2>());
		}
	}
}
