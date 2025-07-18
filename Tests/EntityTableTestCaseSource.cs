namespace Logos.Entities.Tests
{
    public static class EntityTableTestCaseSource
    {
        public static object[][] ConstructorTestCases
        {
            get => new object[][]
            {
                new object[]
                {
                    EntityArchetype.Base
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Enabled>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Enabled>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Enabled>()
                    })
                },
                new object[]
                {
                    EntityArchetype.Create(new ComponentType[]
                    {
                        ComponentType.TypeOf<Name>(),
                        ComponentType.TypeOf<Position2D>(),
                        ComponentType.TypeOf<Rotation2D>(),
                        ComponentType.TypeOf<Scale2D>(),
                        ComponentType.TypeOf<Enabled>()
                    })
                }
            };
        }
    }
}
