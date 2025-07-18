// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

namespace Monophyll.Entities.Tests
{
    public static class EntityTestCaseSource
    {
        public static object[][] CompareEqualsTestCases
        {
            get => new object[][]
            {
                new object[]
                {
                    new Entity(0, 0),
                    new Entity(0, 1)
                },
                new object[]
                {
                    new Entity(0, 0),
                    new Entity(1, 0)
                },
                new object[]
                {
                    new Entity(0, 0),
                    new Entity(1, 1)
                },
                new object[]
                {
                    new Entity(0, 0),
                    new Entity(1, -1)
                },
                new object[]
                {
                    new Entity(-1, 0),
                    new Entity(0, 0)
                },
                new object[]
                {
                    new Entity(0, -1),
                    new Entity(0, 0)
                },
                new object[]
                {
                    new Entity(-1, -1),
                    new Entity(0, 0)
                },
                new object[]
                {
                    new Entity(-1, 1),
                    new Entity(0, 0)
                }
            };
        }
    }
}
