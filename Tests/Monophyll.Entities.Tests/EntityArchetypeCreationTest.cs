using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeCreationTest : ITestCase
	{
		public void Execute()
		{
			ComponentType[] argumentTypes = new ComponentType[10];
			ComponentType[] expectedTypes = new ComponentType[10];

			AssertEntityArchetypeCreateConsistency(argumentTypes, default);

			argumentTypes[0] = argumentTypes[4] = expectedTypes[0] = ComponentType.TypeOf<Position2D>();
			argumentTypes[1] = argumentTypes[5] = expectedTypes[1] = ComponentType.TypeOf<Rotation2D>();
			argumentTypes[2] = argumentTypes[6] = expectedTypes[2] = ComponentType.TypeOf<Scale2D>();
			argumentTypes[3] = argumentTypes[7] = expectedTypes[3] = ComponentType.TypeOf<Matrix3x2>();

			AssertEntityArchetypeCreateConsistency(argumentTypes, expectedTypes.AsSpan(0, 4));

			Array.Clear(argumentTypes);
			Array.Clear(expectedTypes);

			argumentTypes[1] = expectedTypes[0] = ComponentType.TypeOf<Position3D>();
			argumentTypes[8] = expectedTypes[1] = ComponentType.TypeOf<Rotation3D>();
			argumentTypes[4] = expectedTypes[2] = ComponentType.TypeOf<Scale3D>();
			argumentTypes[7] = expectedTypes[3] = ComponentType.TypeOf<Matrix4x4>();
			argumentTypes[0] = expectedTypes[4] = ComponentType.TypeOf<Tag>();

			AssertEntityArchetypeCreateConsistency(argumentTypes, expectedTypes.AsSpan(0, 5));

			Array.Clear(argumentTypes);
			Array.Clear(expectedTypes);

			argumentTypes[9] = expectedTypes[0] = ComponentType.TypeOf<object>();
			argumentTypes[8] = expectedTypes[1] = ComponentType.TypeOf<Position2D>();
			argumentTypes[7] = expectedTypes[2] = ComponentType.TypeOf<Rotation2D>();
			argumentTypes[6] = expectedTypes[3] = ComponentType.TypeOf<Scale2D>();
			argumentTypes[5] = expectedTypes[4] = ComponentType.TypeOf<Matrix3x2>();
			argumentTypes[4] = expectedTypes[5] = ComponentType.TypeOf<Tag>();

			AssertEntityArchetypeCreateConsistency(argumentTypes, expectedTypes.AsSpan(0, 6));

			Array.Clear(argumentTypes);
			Array.Clear(expectedTypes);

			argumentTypes[0] = expectedTypes[0] = ComponentType.TypeOf<object>();
			argumentTypes[2] = expectedTypes[1] = ComponentType.TypeOf<Position2D>();
			argumentTypes[4] = expectedTypes[2] = ComponentType.TypeOf<Rotation2D>();
			argumentTypes[6] = expectedTypes[3] = ComponentType.TypeOf<Scale2D>();
			argumentTypes[8] = expectedTypes[4] = ComponentType.TypeOf<Matrix3x2>();
			argumentTypes[1] = expectedTypes[5] = ComponentType.TypeOf<Position3D>();
			argumentTypes[3] = expectedTypes[6] = ComponentType.TypeOf<Rotation3D>();
			argumentTypes[5] = expectedTypes[7] = ComponentType.TypeOf<Scale3D>();
			argumentTypes[7] = expectedTypes[8] = ComponentType.TypeOf<Matrix4x4>();
			argumentTypes[9] = expectedTypes[9] = ComponentType.TypeOf<Tag>();

			AssertEntityArchetypeCreateConsistency(argumentTypes, expectedTypes);
		}

		private static void AssertEntityArchetypeCreateConsistency(ComponentType[] argumentTypes, ReadOnlySpan<ComponentType> expectedTypes)
		{
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsEnumerable()), expectedTypes);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsSpan()), expectedTypes);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes), expectedTypes);
		}

		private static void AssertEntityArchetypeIntegrity(EntityArchetype archetype, ReadOnlySpan<ComponentType> expectedTypes)
		{
			Debug.Assert(archetype.ComponentTypes.Length == expectedTypes.Length);

			int expectedEntitySize = Unsafe.SizeOf<Entity>();
			int expectedTagPartitionLength = 0;
			int expectedUnmanagedPartitionLength = 0;
			int expectedManagedPartitionLength = 0;

			if (expectedTypes.Length > 0)
			{
				Debug.Assert(archetype.ComponentBits.Length == (expectedTypes[^1].Id >> 5) + 1);

				int i = 0;

				do
				{
					ComponentType type = expectedTypes[i];

					Debug.Assert(type == archetype.ComponentTypes[i]);
					Debug.Assert(archetype.Contains(type));
					expectedEntitySize += type.Size;

					switch (type.Category)
					{
						case ComponentTypeCategory.Tag:
							expectedTagPartitionLength++;
							continue;
						case ComponentTypeCategory.Unmanaged:
							expectedUnmanagedPartitionLength++;
							continue;
						case ComponentTypeCategory.Managed:
							expectedManagedPartitionLength++;
							continue;
					}
				}
				while (++i < expectedTypes.Length);
			}

			Debug.Assert(archetype.ManagedPartitionLength == expectedManagedPartitionLength);
			Debug.Assert(archetype.UnmanagedPartitionLength == expectedUnmanagedPartitionLength);
			Debug.Assert(archetype.TagPartitionLength == expectedTagPartitionLength);
			Debug.Assert(archetype.EntitySize == expectedEntitySize);
		}
	}
}
