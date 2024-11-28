using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeCreationTest : IUnitTest
	{
		private const int ChunkSize0KiB = 0;
		private const int ChunkSize4KiB = 4096;
		private const int ChunkSize8KiB = 8192;
		private const int ChunkSize16KiB = 16384;
		private const int MinChunkCapacity = 16;

		public void Run()
		{
			ComponentType[] argumentTypes = new ComponentType[10];
			ComponentType[] expectedTypes = new ComponentType[10];

			AssertEntityArchetypeCreateConsistency(argumentTypes, default, 512, 1024, 2048);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(ChunkSize0KiB, 0), default, MinChunkCapacity);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(ChunkSize4KiB, 0), default, 512);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(ChunkSize8KiB, 0), default, 1024);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(ChunkSize16KiB, 0), default, 2048);

			argumentTypes[0] = argumentTypes[4] = expectedTypes[0] = ComponentType.TypeOf<Position2D>();
			argumentTypes[1] = argumentTypes[5] = expectedTypes[1] = ComponentType.TypeOf<Rotation2D>();
			argumentTypes[2] = argumentTypes[6] = expectedTypes[2] = ComponentType.TypeOf<Scale2D>();
			argumentTypes[3] = argumentTypes[7] = expectedTypes[3] = ComponentType.TypeOf<Matrix3x2>();

			AssertEntityArchetypeCreateConsistency(argumentTypes, expectedTypes.AsSpan(0, 4), 78, 157, 315);

			Array.Clear(argumentTypes);
			Array.Clear(expectedTypes);

			argumentTypes[1] = expectedTypes[0] = ComponentType.TypeOf<Position3D>();
			argumentTypes[8] = expectedTypes[1] = ComponentType.TypeOf<Rotation3D>();
			argumentTypes[4] = expectedTypes[2] = ComponentType.TypeOf<Scale3D>();
			argumentTypes[7] = expectedTypes[3] = ComponentType.TypeOf<Matrix4x4>();
			argumentTypes[0] = expectedTypes[4] = ComponentType.TypeOf<Tag>();

			AssertEntityArchetypeCreateConsistency(argumentTypes, expectedTypes.AsSpan(0, 5), 36, 73, 146);

			Array.Clear(argumentTypes);
			Array.Clear(expectedTypes);

			argumentTypes[9] = expectedTypes[0] = ComponentType.TypeOf<object>();
			argumentTypes[8] = expectedTypes[1] = ComponentType.TypeOf<Position2D>();
			argumentTypes[7] = expectedTypes[2] = ComponentType.TypeOf<Rotation2D>();
			argumentTypes[6] = expectedTypes[3] = ComponentType.TypeOf<Scale2D>();
			argumentTypes[5] = expectedTypes[4] = ComponentType.TypeOf<Matrix3x2>();
			argumentTypes[4] = expectedTypes[5] = ComponentType.TypeOf<Tag>();

			AssertEntityArchetypeCreateConsistency(argumentTypes, expectedTypes.AsSpan(0, 6), 68, 136, 273);

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

			AssertEntityArchetypeCreateConsistency(argumentTypes, expectedTypes, 24, 49, 99);
		}

		private static void AssertEntityArchetypeCreateConsistency(ComponentType[] argumentTypes, ReadOnlySpan<ComponentType> expectedTypes, int expectedChunkCapacity4KiB, int expectedChunkCapacity8KiB, int expectedChunkCapacity16KiB)
		{

			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsEnumerable(), ChunkSize0KiB, 0), expectedTypes, MinChunkCapacity);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsEnumerable(), ChunkSize4KiB, 0), expectedTypes, expectedChunkCapacity4KiB);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsEnumerable(), ChunkSize8KiB, 0), expectedTypes, expectedChunkCapacity8KiB);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsEnumerable(), ChunkSize16KiB, 0), expectedTypes, expectedChunkCapacity16KiB);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsSpan(), ChunkSize0KiB, 0), expectedTypes, MinChunkCapacity);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsSpan(), ChunkSize4KiB, 0), expectedTypes, expectedChunkCapacity4KiB);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsSpan(), ChunkSize8KiB, 0), expectedTypes, expectedChunkCapacity8KiB);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes.AsSpan(), ChunkSize16KiB, 0), expectedTypes, expectedChunkCapacity16KiB);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes, ChunkSize0KiB, 0), expectedTypes, MinChunkCapacity);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes, ChunkSize4KiB, 0), expectedTypes, expectedChunkCapacity4KiB);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes, ChunkSize8KiB, 0), expectedTypes, expectedChunkCapacity8KiB);
			AssertEntityArchetypeIntegrity(EntityArchetype.Create(argumentTypes, ChunkSize16KiB, 0), expectedTypes, expectedChunkCapacity16KiB);
		}

		private static void AssertEntityArchetypeIntegrity(EntityArchetype archetype, ReadOnlySpan<ComponentType> expectedTypes, int expectedChunkCapacity)
		{
			Debug.Assert(archetype.ComponentTypes.Length == expectedTypes.Length);
			Debug.Assert(archetype.ChunkCapacity == expectedChunkCapacity);

			int expectedEntitySize = Unsafe.SizeOf<Entity>();
			int expectedTagComponentTypeCount = 0;
			int expectedUnmanagedComponentTypeCount = 0;
			int expectedManagedComponentTypeCount = 0;

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
							expectedTagComponentTypeCount++;
							continue;
						case ComponentTypeCategory.Unmanaged:
							expectedUnmanagedComponentTypeCount++;
							continue;
						case ComponentTypeCategory.Managed:
							expectedManagedComponentTypeCount++;
							continue;
					}
				}
				while (++i < expectedTypes.Length);
			}

			Debug.Assert(archetype.EntitySize == expectedEntitySize);
			Debug.Assert(archetype.ChunkSize == expectedEntitySize * expectedChunkCapacity);
			Debug.Assert(archetype.TagComponentTypeCount == expectedTagComponentTypeCount);
			Debug.Assert(archetype.UnmanagedComponentTypeCount == expectedUnmanagedComponentTypeCount);
			Debug.Assert(archetype.ManagedComponentTypeCount == expectedManagedComponentTypeCount);
			Debug.Assert(archetype.StoredComponentTypeCount == expectedManagedComponentTypeCount + expectedUnmanagedComponentTypeCount);
		}
	}
}
