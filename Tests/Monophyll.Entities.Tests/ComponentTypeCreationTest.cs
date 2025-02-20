using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities.Tests
{
	internal sealed class ComponentTypeCreationTest : ITestCase
	{
		public void Execute()
		{
			AssertComponentTypeIntegrity<Tag>(0);
			AssertComponentTypeIntegrity<Position2D>(1);
			AssertComponentTypeIntegrity<Rotation2D>(2);
			AssertComponentTypeIntegrity<Scale2D>(3);
			AssertComponentTypeIntegrity<Matrix3x2>(4);
			AssertComponentTypeIntegrity<Position3D>(5);
			AssertComponentTypeIntegrity<Rotation3D>(6);
			AssertComponentTypeIntegrity<Scale3D>(7);
			AssertComponentTypeIntegrity<Matrix4x4>(8);
			AssertComponentTypeIntegrity<object>(9);
		}

		private static void AssertComponentTypeIntegrity<T>(int expectedId)
		{
			ComponentType type = ComponentType.TypeOf<T>();

			Debug.Assert(type == ComponentType.TypeOf<T>());
			Debug.Assert(type.Type == typeof(T));
			Debug.Assert(type.Id == expectedId);

			switch (type.Category)
			{
				case ComponentTypeCategory.Tag:
					Debug.Assert(type.IsTag);
					Debug.Assert(!type.IsUnmanaged);
					Debug.Assert(!type.IsManaged);
					Debug.Assert(type.Size == Unsafe.SizeOf<T>() - 1);
					Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<T>());
					break;
				case ComponentTypeCategory.Unmanaged:
					Debug.Assert(!type.IsTag);
					Debug.Assert(type.IsUnmanaged);
					Debug.Assert(!type.IsManaged);
					Debug.Assert(type.Size == Unsafe.SizeOf<T>());
					Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<T>());
					break;
				case ComponentTypeCategory.Managed:
					Debug.Assert(!type.IsTag);
					Debug.Assert(!type.IsUnmanaged);
					Debug.Assert(type.IsManaged);
					Debug.Assert(type.Size == Unsafe.SizeOf<T>());
					Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<T>());
					break;
				default:
					Debug.Fail("Invalid CompenentTypeCategory detected.");
					break;
			}
		}
	}
}
