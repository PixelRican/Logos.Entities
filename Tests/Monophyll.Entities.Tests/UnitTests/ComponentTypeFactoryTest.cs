using System.Diagnostics;
using System.Numerics;

namespace Monophyll.Entities.Test
{
	internal sealed class ComponentTypeFactoryTest : IUnitTest
	{
		public void Run()
		{
			Debug.Assert(ComponentType.TypeOf<Position2D>().Type == typeof(Position2D));
			Debug.Assert(ComponentType.TypeOf<Position2D>().ByteSize == 8);
			Debug.Assert(ComponentType.TypeOf<Position2D>().SequenceNumber == 0);

			Debug.Assert(ComponentType.TypeOf<Rotation2D>().Type == typeof(Rotation2D));
			Debug.Assert(ComponentType.TypeOf<Rotation2D>().ByteSize == 4);
			Debug.Assert(ComponentType.TypeOf<Rotation2D>().SequenceNumber == 1);

			Debug.Assert(ComponentType.TypeOf<Scale2D>().Type == typeof(Scale2D));
			Debug.Assert(ComponentType.TypeOf<Scale2D>().ByteSize == 8);
			Debug.Assert(ComponentType.TypeOf<Scale2D>().SequenceNumber == 2);

			Debug.Assert(ComponentType.TypeOf<Matrix3x2>().Type == typeof(Matrix3x2));
			Debug.Assert(ComponentType.TypeOf<Matrix3x2>().ByteSize == 24);
			Debug.Assert(ComponentType.TypeOf<Matrix3x2>().SequenceNumber == 3);

			Debug.Assert(ComponentType.TypeOf<Position3D>().Type == typeof(Position3D));
			Debug.Assert(ComponentType.TypeOf<Position3D>().ByteSize == 12);
			Debug.Assert(ComponentType.TypeOf<Position3D>().SequenceNumber == 4);

			Debug.Assert(ComponentType.TypeOf<Rotation3D>().Type == typeof(Rotation3D));
			Debug.Assert(ComponentType.TypeOf<Rotation3D>().ByteSize == 16);
			Debug.Assert(ComponentType.TypeOf<Rotation3D>().SequenceNumber == 5);

			Debug.Assert(ComponentType.TypeOf<Scale3D>().Type == typeof(Scale3D));
			Debug.Assert(ComponentType.TypeOf<Scale3D>().ByteSize == 12);
			Debug.Assert(ComponentType.TypeOf<Scale3D>().SequenceNumber == 6);

			Debug.Assert(ComponentType.TypeOf<Matrix4x4>().Type == typeof(Matrix4x4));
			Debug.Assert(ComponentType.TypeOf<Matrix4x4>().ByteSize == 64);
			Debug.Assert(ComponentType.TypeOf<Matrix4x4>().SequenceNumber == 7);

			Debug.Assert(ComponentType.TypeOf<Tag>().Type == typeof(Tag));
			Debug.Assert(ComponentType.TypeOf<Tag>().ByteSize == 0);
			Debug.Assert(ComponentType.TypeOf<Tag>().SequenceNumber == 8);
		}
	}
}
