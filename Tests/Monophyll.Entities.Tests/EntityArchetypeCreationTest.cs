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
				[TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()]);

			EntityArchetype transform2DFromSpan = EntityArchetype.Create(
				[TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()]);

			EntityArchetype transform2DFromEnumerable = EntityArchetype.Create((IEnumerable<ComponentType>)
				[TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()]);

			EntityArchetype transform2DWithDuplicates = EntityArchetype.Create(
				[TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>(),
				TypeOf<Rotation2D>(), TypeOf<Matrix3x2>(), TypeOf<Scale2D>(), TypeOf<Position2D>()]);

			EntityArchetype transform2DWithNulls = EntityArchetype.Create(
				[null!, TypeOf<Matrix3x2>(), null!, TypeOf<Position2D>(),
				TypeOf<Rotation2D>(), null!, TypeOf<Scale2D>(), null!]);

			try
			{
				EntityArchetype.Create(null!);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				EntityArchetype.Create((IEnumerable<ComponentType>)null!);
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
			Debug.Assert(archetype.ChunkByteSize == 16380);
			Debug.Assert(archetype.ChunkCapacity == 315);
			Debug.Assert(archetype.EntityByteSize == 52);
			Debug.Assert(archetype.Id == 0);

			Debug.Assert(archetype.Contains(TypeOf<Position2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Rotation2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Scale2D>()));
			Debug.Assert(archetype.Contains(TypeOf<Matrix3x2>()));

			ImmutableArray<ComponentType> componentTypes = archetype.ComponentTypes;
			ImmutableArray<int> componentOffsets = archetype.ComponentOffsets;

			Debug.Assert(componentTypes[0] == TypeOf<Position2D>());
			Debug.Assert(componentTypes[1] == TypeOf<Rotation2D>());
			Debug.Assert(componentTypes[2] == TypeOf<Scale2D>());
			Debug.Assert(componentTypes[3] == TypeOf<Matrix3x2>());

			Debug.Assert(componentOffsets[0] == 2520);
			Debug.Assert(componentOffsets[1] == 5040);
			Debug.Assert(componentOffsets[2] == 6300);
			Debug.Assert(componentOffsets[3] == 8820);
		}
	}
}
