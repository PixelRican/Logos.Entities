using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityArchetypeChunkCreationTest : IUnitTest
	{
		public void Run()
		{
			EntityArchetype archetype = EntityArchetype.Create(
				[TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()]);
			EntityArchetypeChunk chunk = new EntityArchetypeChunk(archetype);

			try
			{
				_ = new EntityArchetypeChunk(null!);
				Debug.Fail(string.Empty);
			}
			catch (ArgumentNullException)
			{
			}

			AssertComponentOffsetCorrectness<Position3D>();
			AssertComponentOffsetCorrectness<Rotation3D>();
			AssertComponentOffsetCorrectness<Scale3D>();
			AssertComponentOffsetCorrectness<Matrix4x4>();

			void AssertComponentOffsetCorrectness<TComponent>() where TComponent : unmanaged
			{
				Debug.Assert(archetype.Contains(TypeOf<TComponent>()));

				ref byte entityReference = ref Unsafe.As<Entity, byte>(ref MemoryMarshal.GetReference(chunk.GetEntities()));
				ref byte componentReference = ref Unsafe.As<TComponent, byte>(ref MemoryMarshal.GetReference(chunk.GetComponents<TComponent>()));
				nint expectedOffset = archetype.ComponentOffsets[archetype.ComponentTypes.BinarySearch(TypeOf<TComponent>())];

				Debug.Assert(Unsafe.ByteOffset(ref entityReference, ref componentReference) == expectedOffset);
			}
		}
	}
}
