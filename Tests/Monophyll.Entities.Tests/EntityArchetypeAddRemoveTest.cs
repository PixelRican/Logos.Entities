using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
    internal sealed class EntityArchetypeAddRemoveTest : ITestCase
    {
        public void Execute()
        {
            ComponentType[] arguments = new ComponentType[10];

            AssertAddRemoveConsistency(EntityArchetype.Base);

            arguments[0] = ComponentType.TypeOf<Position2D>();
            arguments[1] = ComponentType.TypeOf<Rotation2D>();
            arguments[2] = ComponentType.TypeOf<Scale2D>();
            arguments[3] = ComponentType.TypeOf<Matrix3x2>();

            AssertAddRemoveConsistency(EntityArchetype.Create(arguments.AsSpan(0, 4)));

            arguments[0] = ComponentType.TypeOf<Position3D>();
            arguments[1] = ComponentType.TypeOf<Rotation3D>();
            arguments[2] = ComponentType.TypeOf<Scale3D>();
            arguments[3] = ComponentType.TypeOf<Matrix4x4>();

            AssertAddRemoveConsistency(EntityArchetype.Create(arguments.AsSpan(0, 4)));

            arguments[0] = ComponentType.TypeOf<Position3D>();
            arguments[1] = ComponentType.TypeOf<Rotation3D>();
            arguments[2] = ComponentType.TypeOf<Scale3D>();
            arguments[3] = ComponentType.TypeOf<Matrix4x4>();
            arguments[4] = ComponentType.TypeOf<Tag>();
            arguments[5] = ComponentType.TypeOf<object>();

            AssertAddRemoveConsistency(EntityArchetype.Create(arguments.AsSpan(0, 6)));

            arguments[0] = ComponentType.TypeOf<Position2D>();
            arguments[1] = ComponentType.TypeOf<Rotation2D>();
            arguments[2] = ComponentType.TypeOf<Scale2D>();
            arguments[3] = ComponentType.TypeOf<Matrix3x2>();
            arguments[4] = ComponentType.TypeOf<Position3D>();
            arguments[5] = ComponentType.TypeOf<Rotation3D>();
            arguments[6] = ComponentType.TypeOf<Scale3D>();
            arguments[7] = ComponentType.TypeOf<Matrix4x4>();
            arguments[8] = ComponentType.TypeOf<Tag>();
            arguments[9] = ComponentType.TypeOf<object>();

            AssertAddRemoveConsistency(EntityArchetype.Create(arguments));
        }

        private static void AssertAddRemoveConsistency(EntityArchetype archetype)
        {
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<object>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Position2D>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Rotation2D>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Scale2D>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Matrix3x2>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Position3D>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Rotation3D>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Scale3D>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Matrix4x4>());
			AssertAddRemoveCorrectness(archetype, ComponentType.TypeOf<Tag>());
        }

		private static void AssertAddRemoveCorrectness(EntityArchetype archetype, ComponentType type)
		{
			EntityArchetype clone = archetype.Add(type);
			int index = archetype.ComponentTypes.BinarySearch(type);

			if (index < 0)
			{
				Debug.Assert(clone.Contains(type));
				Debug.Assert(clone.ComponentTypes[~index] == type);

				AssertEquality(clone, EntityArchetype.Create(clone.ComponentTypes));
				AssertEquality(archetype, clone.Remove(type));
			}
			else
			{
				AssertEquality(archetype, clone);
			}
		}

		private static void AssertEquality(EntityArchetype a, EntityArchetype b)
        {
            Debug.Assert(a.ComponentTypes.SequenceEqual(b.ComponentTypes));
            Debug.Assert(a.ComponentBits.SequenceEqual(b.ComponentBits));
			Debug.Assert(a.ManagedPartitionLength == b.ManagedPartitionLength);
			Debug.Assert(a.UnmanagedPartitionLength == b.UnmanagedPartitionLength);
			Debug.Assert(a.TagPartitionLength == b.TagPartitionLength);
			Debug.Assert(a.EntitySize == b.EntitySize);
        }
    }
}
