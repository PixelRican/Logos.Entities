using System.Numerics;

namespace Monophyll.Entities.Test
{
	internal record struct Color(byte R, byte G, byte B, byte A)
	{
		public byte R = R;
		public byte G = G;
		public byte B = B;
		public byte A = A;
	}

	internal record struct Position2D(Vector2 Value)
	{
		public Vector2 Value = Value;
	}

	internal record struct Position3D(Vector3 Value)
	{
		public Vector3 Value = Value;
	}

	internal record struct Rotation2D(float Value)
	{
		public float Value = Value;
	}

	internal record struct Rotation3D(Quaternion Value)
	{
		public Quaternion Value = Value;
	}

	internal record struct Scale2D(Vector2 Value)
	{
		public Vector2 Value = Value;
	}

	internal record struct Scale3D(Vector3 Value)
	{
		public Vector3 Value = Value;
	}

	internal readonly record struct Tag
	{
	}
}
