using System;
using System.Collections;
using UnityEngine;

public static class ProjectileViewUtil
{
	private static IEnumerator _GenerateSprite(Texture2D texture, Mesh mesh)
	{
		yield return null;
		Texture2D t = GraphicsUtil.RenderMeshIdentity(mesh, texture, 128);
		yield return null;
		yield return Sprite.Create(t, new Rect(0f, 0f, t.width, t.height), new Vector2(0f, 0f), 100f, 0u, SpriteMeshType.FullRect);
	}

	public static void GetCutoutMesh(ImageRef equipmentImageRef, Action<Mesh> onMesh)
	{
		equipmentImageRef.GetGeneratedFromContent(GraphicsUtil.TextureToEquipmentCutout, onMesh);
	}

	public static void GetSprite(ImageRef equipmentImageRef, Action<Sprite> onSprite)
	{
		GetCutoutMesh(equipmentImageRef, delegate(Mesh mesh)
		{
			equipmentImageRef.GetGeneratedFromContent(new GeneratorChain<Texture2D, Mesh>(mesh, _GenerateSprite).Chain, onSprite);
		});
	}
}
