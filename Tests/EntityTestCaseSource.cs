// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System.Collections;

namespace Monophyll.Entities.Tests
{
    public static class EntityTestCaseSource
    {
        public static IEnumerable CompareEqualsTestCases
        {
            get
            {
                yield return new object[]
                {
                    new Entity(0, 0),
                    new Entity(0, 1)
                };

                yield return new object[]
                {
                    new Entity(0, 0),
                    new Entity(1, 0)
                };

                yield return new object[]
                {
                    new Entity(0, 0),
                    new Entity(1, 1)
                };

                yield return new object[]
                {
                    new Entity(0, 0),
                    new Entity(1, -1)
                };

                yield return new object[]
                {
                    new Entity(-1, 0),
                    new Entity(0, 0)
                };

                yield return new object[]
                {
                    new Entity(0, -1),
                    new Entity(0, 0)
                };

                yield return new object[]
                {
                    new Entity(-1, -1),
                    new Entity(0, 0)
                };

                yield return new object[]
                {
                    new Entity(-1, 1),
                    new Entity(0, 0)
                };
            }
        }
    }
}
