// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Monophyll.Entities.Tests
{
    public static class EntityArchetypeTestCaseSource
    {
        public static object[][] AddTestCases
        {
            get => new object[][]
            {
                new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Enabled>()
                },
                new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Name>()
                },
                new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Position2D>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Name>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Name>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Position2D>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                }
            };
        }

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
                        ComponentType.TypeOf<Position2D>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
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
                    EntityArchetype.Base,
                    null!
                },
                new object[]
                {
                    EntityArchetype.Base,
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>()])
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Position2D>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Rotation2D>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Rotation2D>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Position3D>(),
                        ComponentType.TypeOf<Name>()
                    })
                }
            };
        }

        public static object[][] RemoveTestCases
        {
            get => new object[][]
            {
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Name>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Position2D>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Name>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    }),
                    ComponentType.TypeOf<Name>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>()
                    }),
                    ComponentType.TypeOf<Position2D>()
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                }
            };
        }
    }
}
