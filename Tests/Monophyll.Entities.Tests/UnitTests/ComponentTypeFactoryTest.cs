using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class ComponentTypeFactoryTest : IUnitTest
	{
		public void Run()
		{
			Debug.Assert(TypeOf<Position2D>().Type == typeof(Position2D));
			Debug.Assert(TypeOf<Position2D>().ByteSize == 8);
			Debug.Assert(TypeOf<Position2D>().Id == 0);

			Debug.Assert(TypeOf<Rotation2D>().Type == typeof(Rotation2D));
			Debug.Assert(TypeOf<Rotation2D>().ByteSize == 4);
			Debug.Assert(TypeOf<Rotation2D>().Id == 1);

			Debug.Assert(TypeOf<Scale2D>().Type == typeof(Scale2D));
			Debug.Assert(TypeOf<Scale2D>().ByteSize == 8);
			Debug.Assert(TypeOf<Scale2D>().Id == 2);

			Debug.Assert(TypeOf<Matrix3x2>().Type == typeof(Matrix3x2));
			Debug.Assert(TypeOf<Matrix3x2>().ByteSize == 24);
			Debug.Assert(TypeOf<Matrix3x2>().Id == 3);

			Debug.Assert(TypeOf<Position3D>().Type == typeof(Position3D));
			Debug.Assert(TypeOf<Position3D>().ByteSize == 12);
			Debug.Assert(TypeOf<Position3D>().Id == 4);

			Debug.Assert(TypeOf<Rotation3D>().Type == typeof(Rotation3D));
			Debug.Assert(TypeOf<Rotation3D>().ByteSize == 16);
			Debug.Assert(TypeOf<Rotation3D>().Id == 5);

			Debug.Assert(TypeOf<Scale3D>().Type == typeof(Scale3D));
			Debug.Assert(TypeOf<Scale3D>().ByteSize == 12);
			Debug.Assert(TypeOf<Scale3D>().Id == 6);

			Debug.Assert(TypeOf<Matrix4x4>().Type == typeof(Matrix4x4));
			Debug.Assert(TypeOf<Matrix4x4>().ByteSize == 64);
			Debug.Assert(TypeOf<Matrix4x4>().Id == 7);

			Debug.Assert(TypeOf<Tag>().Type == typeof(Tag));
			Debug.Assert(TypeOf<Tag>().ByteSize == 0);
			Debug.Assert(TypeOf<Tag>().Id == 8);
		}
	}
}
