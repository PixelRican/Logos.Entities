namespace Monophyll.Entities.Tests
{
    public record struct Name
    {
        public string Value;

        public Name(string value)
        {
            Value = value;
        }
    }

    public record struct Position2D
    {
        public float X;
        public float Y;

        public Position2D(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public record struct Position3D
    {
        public float X;
        public float Y;
        public float Z;

        public Position3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public record struct Rotation2D
    {
        public float Angle;

        public Rotation2D(float angle)
        {
            Angle = angle;
        }
    }

    public record struct Rotation3D
    {
        public float Yaw;
        public float Pitch;
        public float Roll;

        public Rotation3D(float yaw, float pitch, float roll)
        {
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
        }
    }

    public record struct Scale2D
    {
        public float X;
        public float Y;

        public Scale2D(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public record struct Scale3D
    {
        public float X;
        public float Y;
        public float Z;

        public Scale3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public readonly record struct Tag
    {
    }
}
