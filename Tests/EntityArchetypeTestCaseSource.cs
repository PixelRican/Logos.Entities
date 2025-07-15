// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;

namespace Monophyll.Entities.Tests
{
    public static class EntityArchetypeTestCaseSource
    {
        public static IEnumerable AddTestCases
        {
            get
            {
                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Base,
                    ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                };
            }
        }

        public static IEnumerable CreateTestCases
        {
            get
            {
                yield return new object[]
                {
                    Array.Empty<ComponentType>(),
                    Array.Empty<ComponentType>()
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        null!
                    },
                    Array.Empty<ComponentType>()
                };

                yield return new object[]
                {
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };
            }
        }

        public static IEnumerable EqualsTestCases
        {
            get
            {
                yield return new object[]
                {
                    EntityArchetype.Base,
                    null!
                };

                yield return new object[]
                {
                    EntityArchetype.Base,
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    })
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>(),
                        ComponentType.TypeOf<Name>()
                    }),
                    EntityArchetype.Create([ComponentType.TypeOf<Enabled>()])
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };
            }
        }

        public static IEnumerable RemoveTestCases
        {
            get
            {
                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    }),
                    ComponentType.TypeOf<Name>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>()
                    }),
                    ComponentType.TypeOf<Position2D>()
                };

                yield return new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    }),
                    ComponentType.TypeOf<Enabled>()
                };
            }
        }
    }
}
