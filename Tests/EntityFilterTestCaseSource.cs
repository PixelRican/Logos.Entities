// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities.Tests
{
    public static class EntityFilterTestCaseSource
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
                        ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        null!
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        null!,
                        ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        null!,
                        ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Enabled>()
                    }
                },
                new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Scale2D>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Scale2D>(),
                        ComponentType.TypeOf<Enabled>()
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
                    EntityFilter.Universal,
                    null!
                },
                new object[]
                {
                    EntityFilter.Universal,
                    EntityFilter.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        Array.Empty<ComponentType>(),
                        Array.Empty<ComponentType>())
                },
                new object[]
                {
                    EntityFilter.Universal,
                    EntityFilter.Create(
                        Array.Empty<ComponentType>(),
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        Array.Empty<ComponentType>())
                },
                new object[]
                {
                    EntityFilter.Universal,
                    EntityFilter.Create(
                        Array.Empty<ComponentType>(),
                        Array.Empty<ComponentType>(),
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Enabled>()
                        })
                },
                new object[]
                {
                    EntityFilter.Create(
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
                            ComponentType.TypeOf<Enabled>()
                        }),
                    EntityFilter.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Enabled>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        })
                },
                new object[]
                {
                    EntityFilter.Create(
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
                            ComponentType.TypeOf<Enabled>()
                        }),
                    EntityFilter.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Enabled>()
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
                    EntityFilter.Create(
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
                            ComponentType.TypeOf<Enabled>()
                        }),
                    EntityFilter.Create(
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
                            ComponentType.TypeOf<Enabled>()
                        })
                },
                new object[]
                {
                    EntityFilter.Create(
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
                            ComponentType.TypeOf<Enabled>()
                        }),
                    EntityFilter.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Enabled>()
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
