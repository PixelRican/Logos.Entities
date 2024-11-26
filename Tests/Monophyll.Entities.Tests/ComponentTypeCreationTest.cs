using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class ComponentTypeCreationTest : IUnitTest
	{
		public void Run()
		{
			Debug.Assert(TypeOf<Position2D>().SystemType == typeof(Position2D));
			Debug.Assert(TypeOf<Position2D>().Size == 8);
			Debug.Assert(TypeOf<Position2D>().Id == 0);

			Debug.Assert(TypeOf<Rotation2D>().SystemType == typeof(Rotation2D));
			Debug.Assert(TypeOf<Rotation2D>().Size == 4);
			Debug.Assert(TypeOf<Rotation2D>().Id == 1);

			Debug.Assert(TypeOf<Scale2D>().SystemType == typeof(Scale2D));
			Debug.Assert(TypeOf<Scale2D>().Size == 8);
			Debug.Assert(TypeOf<Scale2D>().Id == 2);

			Debug.Assert(TypeOf<Matrix3x2>().SystemType == typeof(Matrix3x2));
			Debug.Assert(TypeOf<Matrix3x2>().Size == 24);
			Debug.Assert(TypeOf<Matrix3x2>().Id == 3);

			Debug.Assert(TypeOf<Position3D>().SystemType == typeof(Position3D));
			Debug.Assert(TypeOf<Position3D>().Size == 12);
			Debug.Assert(TypeOf<Position3D>().Id == 4);

			Debug.Assert(TypeOf<Rotation3D>().SystemType == typeof(Rotation3D));
			Debug.Assert(TypeOf<Rotation3D>().Size == 16);
			Debug.Assert(TypeOf<Rotation3D>().Id == 5);

			Debug.Assert(TypeOf<Scale3D>().SystemType == typeof(Scale3D));
			Debug.Assert(TypeOf<Scale3D>().Size == 12);
			Debug.Assert(TypeOf<Scale3D>().Id == 6);

			Debug.Assert(TypeOf<Matrix4x4>().SystemType == typeof(Matrix4x4));
			Debug.Assert(TypeOf<Matrix4x4>().Size == 64);
			Debug.Assert(TypeOf<Matrix4x4>().Id == 7);

			Debug.Assert(TypeOf<Tag>().SystemType == typeof(Tag));
			Debug.Assert(TypeOf<Tag>().Size == 0);
			Debug.Assert(TypeOf<Tag>().Id == 8);
		}
	}
}
