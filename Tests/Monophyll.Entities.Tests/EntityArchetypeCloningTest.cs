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

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(0));

            argumentTypes[0] = ComponentType.TypeOf<Position2D>();
            argumentTypes[1] = ComponentType.TypeOf<Rotation2D>();
            argumentTypes[2] = ComponentType.TypeOf<Scale2D>();
            argumentTypes[3] = ComponentType.TypeOf<Matrix3x2>();

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(argumentTypes.AsSpan(0, 4), 0));

            argumentTypes[0] = ComponentType.TypeOf<Position3D>();
            argumentTypes[1] = ComponentType.TypeOf<Rotation3D>();
            argumentTypes[2] = ComponentType.TypeOf<Scale3D>();
            argumentTypes[3] = ComponentType.TypeOf<Matrix4x4>();

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(argumentTypes.AsSpan(0, 4), 0));

            argumentTypes[0] = ComponentType.TypeOf<Position3D>();
            argumentTypes[1] = ComponentType.TypeOf<Rotation3D>();
            argumentTypes[2] = ComponentType.TypeOf<Scale3D>();
            argumentTypes[3] = ComponentType.TypeOf<Matrix4x4>();
            argumentTypes[4] = ComponentType.TypeOf<Tag>();
            argumentTypes[5] = ComponentType.TypeOf<object>();

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(argumentTypes.AsSpan(0, 6), 0));

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

            AssertEntityArchetypeCloneConsistency(EntityArchetype.Create(argumentTypes, 0));
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
			AssertEntityArchetypeEquality(archetype, archetype.Clone(0));

			EntityArchetype clone = archetype.CloneWith(type, 0);
			int index = archetype.ComponentTypes.BinarySearch(type);

			if (index < 0)
			{
				index = ~index;

				Debug.Assert(clone.Contains(type));
				Debug.Assert(clone.ComponentTypes[index] == type);

				AssertEntityArchetypeEquality(clone, EntityArchetype.Create(clone.ComponentTypes.AsSpan(), 0));
				AssertEntityArchetypeEquality(archetype, clone.CloneWithout(type, 0));
			}
			else
			{
				AssertEntityArchetypeEquality(archetype, clone);
			}
		}

		private static void AssertEntityArchetypeEquality(EntityArchetype a, EntityArchetype b)
        {
            Debug.Assert(a.ComponentTypes.AsSpan().SequenceEqual(b.ComponentTypes.AsSpan()));
            Debug.Assert(a.ComponentBits.AsSpan().SequenceEqual(b.ComponentBits.AsSpan()));
            Debug.Assert(a.TagComponentTypeCount == b.TagComponentTypeCount);
            Debug.Assert(a.UnmanagedComponentTypeCount == b.UnmanagedComponentTypeCount);
            Debug.Assert(a.ManagedComponentTypeCount == b.ManagedComponentTypeCount);
            Debug.Assert(a.StoredComponentTypeCount == b.StoredComponentTypeCount);
			Debug.Assert(a.EntitySize == b.EntitySize);
			Debug.Assert(a.Id == b.Id);
        }
    }
}
