// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

namespace Monophyll.Entities.Tests
{
    public readonly record struct Enabled
    {
    }

    public record struct Name
    {
        public string Value;
    }

    public record struct Position2D
    {
        public float X;
        public float Y;
    }

    public record struct Position3D
    {
        public float X;
        public float Y;
        public float Z;
    }

    public record struct Rotation2D
    {
        public float Angle;
    }

    public record struct Rotation3D
    {
        public float Yaw;
        public float Pitch;
        public float Roll;
    }

    public record struct Scale2D
    {
        public float X;
        public float Y;
    }

    public record struct Scale3D
    {
        public float X;
        public float Y;
        public float Z;
    }
}
