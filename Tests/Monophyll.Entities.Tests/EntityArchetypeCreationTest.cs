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
            ComponentType[] arguments = new ComponentType[10];
            ComponentType[] controls = new ComponentType[10];

            AssertCreateConsistency(Array.Empty<ComponentType>(), Span<ComponentType>.Empty);

            arguments[0] = arguments[4] = controls[0] = ComponentType.TypeOf<Position2D>();
			arguments[1] = arguments[5] = controls[1] = ComponentType.TypeOf<Rotation2D>();
			arguments[2] = arguments[6] = controls[2] = ComponentType.TypeOf<Scale2D>();
			arguments[3] = arguments[7] = controls[3] = ComponentType.TypeOf<Matrix3x2>();

			AssertCreateConsistency(arguments, controls.AsSpan(0, 4));

			arguments[1] = controls[0] = ComponentType.TypeOf<Position3D>();
			arguments[8] = controls[1] = ComponentType.TypeOf<Rotation3D>();
			arguments[4] = controls[2] = ComponentType.TypeOf<Scale3D>();
			arguments[7] = controls[3] = ComponentType.TypeOf<Matrix4x4>();
			arguments[0] = controls[4] = ComponentType.TypeOf<Tag>();

			AssertCreateConsistency(arguments, controls.AsSpan(0, 5));

			arguments[9] = controls[0] = ComponentType.TypeOf<object>();
			arguments[8] = controls[1] = ComponentType.TypeOf<Position2D>();
			arguments[7] = controls[2] = ComponentType.TypeOf<Rotation2D>();
			arguments[6] = controls[3] = ComponentType.TypeOf<Scale2D>();
			arguments[5] = controls[4] = ComponentType.TypeOf<Matrix3x2>();
			arguments[4] = controls[5] = ComponentType.TypeOf<Tag>();

			AssertCreateConsistency(arguments, controls.AsSpan(0, 6));

			arguments[0] = controls[0] = ComponentType.TypeOf<object>();
			arguments[2] = controls[1] = ComponentType.TypeOf<Position2D>();
			arguments[4] = controls[2] = ComponentType.TypeOf<Rotation2D>();
			arguments[6] = controls[3] = ComponentType.TypeOf<Scale2D>();
			arguments[8] = controls[4] = ComponentType.TypeOf<Matrix3x2>();
			arguments[1] = controls[5] = ComponentType.TypeOf<Position3D>();
			arguments[3] = controls[6] = ComponentType.TypeOf<Rotation3D>();
			arguments[5] = controls[7] = ComponentType.TypeOf<Scale3D>();
			arguments[7] = controls[8] = ComponentType.TypeOf<Matrix4x4>();
			arguments[9] = controls[9] = ComponentType.TypeOf<Tag>();

			AssertCreateConsistency(arguments, controls);
		}

		private static void AssertCreateConsistency(ComponentType[] arguments, Span<ComponentType> controls)
        {
            AssertCreateCorrectness(EntityArchetype.Create(arguments), controls);
            AssertCreateCorrectness(EntityArchetype.Create(arguments.AsSpan()), controls);
            AssertCreateCorrectness(EntityArchetype.Create(arguments.AsEnumerable()), controls);

            Array.Clear(arguments);
			controls.Clear();
        }

		private static void AssertCreateCorrectness(EntityArchetype archetype, ReadOnlySpan<ComponentType> controls)
		{
			Debug.Assert(archetype.ComponentTypes.Length == controls.Length);

			int expectedEntitySize = Unsafe.SizeOf<Entity>();
			int expectedTagPartitionLength = 0;
			int expectedUnmanagedPartitionLength = 0;
			int expectedManagedPartitionLength = 0;

			if (controls.Length > 0)
			{
				Debug.Assert(archetype.ComponentBits.Length == (controls[^1].Id >> 5) + 1);

				int i = 0;

				do
				{
					ComponentType type = controls[i];

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
				while (++i < controls.Length);
			}

			Debug.Assert(archetype.ManagedPartitionLength == expectedManagedPartitionLength);
			Debug.Assert(archetype.UnmanagedPartitionLength == expectedUnmanagedPartitionLength);
			Debug.Assert(archetype.TagPartitionLength == expectedTagPartitionLength);
			Debug.Assert(archetype.EntitySize == expectedEntitySize);
		}
	}
}
