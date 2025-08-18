// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities.Tests
{
    public static class EntityPredicateTestCaseSource
    {
        public static object[][] CreateTestCases
        {
            get => new object[][]
            {
                new object[]
                {
                    Array.Empty<ComponentType>(),
                    Array.Empty<ComponentType>()
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        null!
                    },
                    Array.Empty<ComponentType>()
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        null!
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        null!,
                        ComponentType.TypeOf<Disabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Disabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        null!,
                        ComponentType.TypeOf<Disabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Disabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Disabled>(),
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Scale2D>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Scale2D>(),
                        ComponentType.TypeOf<Disabled>()
                    }
                }
            };
        }

        public static object[][] EqualsTestCases
        {
            get => new object[][]
            {
                new object[]
                {
                    EntityPredicate.Universal,
                    null!
                },
                new object[]
                {
                    EntityPredicate.Universal,
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        Array.Empty<ComponentType>(),
                        Array.Empty<ComponentType>())
                },
                new object[]
                {
                    EntityPredicate.Universal,
                    EntityPredicate.Create(
                        Array.Empty<ComponentType>(),
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        Array.Empty<ComponentType>())
                },
                new object[]
                {
                    EntityPredicate.Universal,
                    EntityPredicate.Create(
                        Array.Empty<ComponentType>(),
                        Array.Empty<ComponentType>(),
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        })
                },
                new object[]
                {
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        })
                },
                new object[]
                {
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        })
                },
                new object[]
                {
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        })
                },
                new object[]
                {
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        }),
                    EntityPredicate.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Disabled>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        })
                }
            };
        }
    }
}
