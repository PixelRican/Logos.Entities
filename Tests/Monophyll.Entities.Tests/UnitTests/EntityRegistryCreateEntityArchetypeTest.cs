using System.Diagnostics;
using System.Numerics;
using static Monophyll.Entities.ComponentType;

namespace Monophyll.Entities.Tests
{
	internal sealed class EntityRegistryCreateEntityArchetypeTest : IUnitTest
	{
		public void Run()
		{
			EntityRegistry registry = new();

			Debug.Assert(registry.CreateEntityArchetype([]) == EntityArchetype.Base);

			EntityArchetype transform2DArchetype = registry.CreateEntityArchetype([
				TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()
			]);
			EntityArchetype transform3DArchetype = registry.CreateEntityArchetype([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()
			]);
			EntityArchetype taggedTransform2DArchetype = registry.CreateEntityArchetype([
				TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>(), TypeOf<Tag>()
			]);
			EntityArchetype taggedTransform3DArchetype = registry.CreateEntityArchetype([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>(), TypeOf<Tag>()
			]);

			Debug.Assert(transform2DArchetype.Id == 1);
			Debug.Assert(transform2DArchetype.Contains(TypeOf<Position2D>()));
			Debug.Assert(transform2DArchetype.Contains(TypeOf<Rotation2D>()));
			Debug.Assert(transform2DArchetype.Contains(TypeOf<Scale2D>()));
			Debug.Assert(transform2DArchetype.Contains(TypeOf<Matrix3x2>()));
			Debug.Assert(transform2DArchetype == registry.CreateEntityArchetype([
				TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>()
			]));
			Debug.Assert(transform2DArchetype == registry.CreateEntityArchetype(new EntityArchetype(transform2DArchetype)));

			Debug.Assert(transform3DArchetype.Id == 2);
			Debug.Assert(transform3DArchetype.Contains(TypeOf<Position3D>()));
			Debug.Assert(transform3DArchetype.Contains(TypeOf<Rotation3D>()));
			Debug.Assert(transform3DArchetype.Contains(TypeOf<Scale3D>()));
			Debug.Assert(transform3DArchetype.Contains(TypeOf<Matrix4x4>()));
			Debug.Assert(transform3DArchetype == registry.CreateEntityArchetype([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>()
			]));
			Debug.Assert(transform3DArchetype == registry.CreateEntityArchetype(new EntityArchetype(transform3DArchetype)));

			Debug.Assert(taggedTransform2DArchetype.Id == 3);
			Debug.Assert(taggedTransform2DArchetype.Contains(TypeOf<Position2D>()));
			Debug.Assert(taggedTransform2DArchetype.Contains(TypeOf<Rotation2D>()));
			Debug.Assert(taggedTransform2DArchetype.Contains(TypeOf<Scale2D>()));
			Debug.Assert(taggedTransform2DArchetype.Contains(TypeOf<Matrix3x2>()));
			Debug.Assert(taggedTransform2DArchetype.Contains(TypeOf<Tag>()));
			Debug.Assert(taggedTransform2DArchetype == registry.CreateEntityArchetype([
				TypeOf<Position2D>(), TypeOf<Rotation2D>(), TypeOf<Scale2D>(), TypeOf<Matrix3x2>(), TypeOf<Tag>()
			]));
			Debug.Assert(taggedTransform2DArchetype == registry.CreateEntityArchetype(new EntityArchetype(taggedTransform2DArchetype)));

			Debug.Assert(taggedTransform3DArchetype.Id == 4);
			Debug.Assert(taggedTransform3DArchetype.Contains(TypeOf<Position3D>()));
			Debug.Assert(taggedTransform3DArchetype.Contains(TypeOf<Rotation3D>()));
			Debug.Assert(taggedTransform3DArchetype.Contains(TypeOf<Scale3D>()));
			Debug.Assert(taggedTransform3DArchetype.Contains(TypeOf<Matrix4x4>()));
			Debug.Assert(taggedTransform3DArchetype.Contains(TypeOf<Tag>()));
			Debug.Assert(taggedTransform3DArchetype == registry.CreateEntityArchetype([
				TypeOf<Position3D>(), TypeOf<Rotation3D>(), TypeOf<Scale3D>(), TypeOf<Matrix4x4>(), TypeOf<Tag>()
			]));
			Debug.Assert(taggedTransform3DArchetype == registry.CreateEntityArchetype(new EntityArchetype(taggedTransform3DArchetype)));
		}
	}
}
