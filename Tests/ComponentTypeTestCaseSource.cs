// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System.Collections;
using System.Runtime.CompilerServices;

namespace Monophyll.Entities.Tests
{
    public static class ComponentTypeTestCaseSource
    {
        static ComponentTypeTestCaseSource()
        {
            // Initialize component types in a predefined order for test consistency.
            ComponentType.TypeOf<Enabled>();
            ComponentType.TypeOf<Name>();
            ComponentType.TypeOf<Position2D>();
            ComponentType.TypeOf<Position3D>();
            ComponentType.TypeOf<Rotation2D>();
            ComponentType.TypeOf<Rotation3D>();
            ComponentType.TypeOf<Scale2D>();
            ComponentType.TypeOf<Scale3D>();
        }

        public static IEnumerable CompareEqualsTestCases
        {
            get
            {
                yield return new object[]
                {
                    null!,
                    ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    null!,
                    ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    null!,
                    ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(),
                    ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(),
                    ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Rotation2D>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(),
                    ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position3D>(),
                    ComponentType.TypeOf<Rotation3D>()
                };
            }
        }

        public static IEnumerable TypeOfTestCases
        {
            get
            {
                yield return new object[]
                {
                    ComponentType.TypeOf<Enabled>(),
                    typeof(Enabled),
                    0,
                    Unsafe.SizeOf<Enabled>() - 1,
                    ComponentTypeCategory.Tag
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Name>(),
                    typeof(Name),
                    1,
                    Unsafe.SizeOf<Name>(),
                    ComponentTypeCategory.Managed
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position2D>(),
                    typeof(Position2D),
                    2,
                    Unsafe.SizeOf<Position2D>(),
                    ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Position3D>(),
                    typeof(Position3D),
                    3,
                    Unsafe.SizeOf<Position3D>(),
                    ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Rotation2D>(),
                    typeof(Rotation2D),
                    4,
                    Unsafe.SizeOf<Rotation2D>(),
                    ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Rotation3D>(),
                    typeof(Rotation3D),
                    5,
                    Unsafe.SizeOf<Rotation3D>(),
                    ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Scale2D>(),
                    typeof(Scale2D),
                    6,
                    Unsafe.SizeOf<Scale2D>(),
                    ComponentTypeCategory.Unmanaged
                };

                yield return new object[]
                {
                    ComponentType.TypeOf<Scale3D>(),
                    typeof(Scale3D),
                    7,
                    Unsafe.SizeOf<Scale3D>(),
                    ComponentTypeCategory.Unmanaged
                };
            }
        }
    }
}
