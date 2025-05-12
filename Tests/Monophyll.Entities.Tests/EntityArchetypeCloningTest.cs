using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Monophyll.Entities.Tests
{
    internal sealed class EntityArchetypeCloningTest : ITestCase
    {
        public void Execute()
        {
            ComponentType[] argumentTypes = new ComponentType[10];

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Base);

            argumentTypes[0] = ComponentType.TypeOf<Position2D>();
            argumentTypes[1] = ComponentType.TypeOf<Rotation2D>();
            argumentTypes[2] = ComponentType.TypeOf<Scale2D>();
            argumentTypes[3] = ComponentType.TypeOf<Matrix3x2>();

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(argumentTypes.AsSpan(0, 4)));

            argumentTypes[0] = ComponentType.TypeOf<Position3D>();
            argumentTypes[1] = ComponentType.TypeOf<Rotation3D>();
            argumentTypes[2] = ComponentType.TypeOf<Scale3D>();
            argumentTypes[3] = ComponentType.TypeOf<Matrix4x4>();

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(argumentTypes.AsSpan(0, 4)));

            argumentTypes[0] = ComponentType.TypeOf<Position3D>();
            argumentTypes[1] = ComponentType.TypeOf<Rotation3D>();
            argumentTypes[2] = ComponentType.TypeOf<Scale3D>();
            argumentTypes[3] = ComponentType.TypeOf<Matrix4x4>();
            argumentTypes[4] = ComponentType.TypeOf<Tag>();
            argumentTypes[5] = ComponentType.TypeOf<object>();

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(argumentTypes.AsSpan(0, 6)));

            argumentTypes[0] = ComponentType.TypeOf<Position2D>();
            argumentTypes[1] = ComponentType.TypeOf<Rotation2D>();
            argumentTypes[2] = ComponentType.TypeOf<Scale2D>();
            argumentTypes[3] = ComponentType.TypeOf<Matrix3x2>();
            argumentTypes[4] = ComponentType.TypeOf<Position3D>();
            argumentTypes[5] = ComponentType.TypeOf<Rotation3D>();
            argumentTypes[6] = ComponentType.TypeOf<Scale3D>();
            argumentTypes[7] = ComponentType.TypeOf<Matrix4x4>();
            argumentTypes[8] = ComponentType.TypeOf<Tag>();
            argumentTypes[9] = ComponentType.TypeOf<object>();

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(argumentTypes));
        }

        private static void AssertEntityArchetypeCloneConsistency(EntityArchetype archetype)
        {
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<object>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Position2D>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Rotation2D>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Scale2D>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Matrix3x2>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Position3D>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Rotation3D>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Scale3D>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Matrix4x4>());
			AssertEntityArchetypeCloneIntegrity(archetype, ComponentType.TypeOf<Tag>());
        }

		private static void AssertEntityArchetypeCloneIntegrity(EntityArchetype archetype, ComponentType type)
		{
			EntityArchetype clone = archetype.Add(type);
			int index = archetype.ComponentTypes.BinarySearch(type);

			if (index < 0)
			{
				index = ~index;

				Debug.Assert(clone.Contains(type));
				Debug.Assert(clone.ComponentTypes[index] == type);

				AssertEntityArchetypeEquality(clone, EntityArchetype.Create(clone.ComponentTypes));
				AssertEntityArchetypeEquality(archetype, clone.Remove(type));
			}
			else
			{
				AssertEntityArchetypeEquality(archetype, clone);
			}
		}

		private static void AssertEntityArchetypeEquality(EntityArchetype a, EntityArchetype b)
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
