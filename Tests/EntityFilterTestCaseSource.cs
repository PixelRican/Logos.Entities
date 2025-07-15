// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;
using System.Collections;

namespace Monophyll.Entities.Tests
{
    public static class EntityFilterTestCaseSource
    {
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
                        ComponentType.TypeOf<Scale2D>()
                    },
                    new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Scale2D>(),
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
                    EntityFilter.Universal,
                    null!
                };

                yield return new object[]
                {
                    EntityFilter.Universal,
                    EntityFilter.Create(
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Name>()
                        },
                        Array.Empty<ComponentType>(),
                        Array.Empty<ComponentType>())
                };

                yield return new object[]
                {
                    EntityFilter.Universal,
                    EntityFilter.Create(
                        Array.Empty<ComponentType>(),
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Position2D>()
                        },
                        Array.Empty<ComponentType>())
                };

                yield return new object[]
                {
                    EntityFilter.Universal,
                    EntityFilter.Create(
                        Array.Empty<ComponentType>(),
                        Array.Empty<ComponentType>(),
                        new ComponentType[]
                        {
                            ComponentType.TypeOf<Enabled>()
                        })
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };

                yield return new object[]
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
                };
            }
        }
    }
}
