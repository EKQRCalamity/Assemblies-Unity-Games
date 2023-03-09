using UnityEngine;

namespace UnityStandardAssets.ImageEffects;

internal class Quads
{
	private static Mesh[] meshes;

	private static int currentQuads;

	private static bool HasMeshes()
	{
		if (meshes == null)
		{
			return false;
		}
		Mesh[] array = meshes;
		foreach (Mesh mesh in array)
		{
			if (null == mesh)
			{
				return false;
			}
		}
		return true;
	}

	public static void Cleanup()
	{
		if (meshes == null)
		{
			return;
		}
		for (int i = 0; i < meshes.Length; i++)
		{
			if (null != meshes[i])
			{
				Object.DestroyImmediate(meshes[i]);
				meshes[i] = null;
			}
		}
		meshes = null;
	}

	public static Mesh[] GetMeshes(int totalWidth, int totalHeight)
	{
		if (HasMeshes() && currentQuads == totalWidth * totalHeight)
		{
			return meshes;
		}
		int num = 10833;
		int num2 = (currentQuads = totalWidth * totalHeight);
		int num3 = Mathf.CeilToInt(1f * (float)num2 / (1f * (float)num));
		meshes = new Mesh[num3];
		int num4 = 0;
		int num5 = 0;
		for (num4 = 0; num4 < num2; num4 += num)
		{
			int triCount = Mathf.FloorToInt(Mathf.Clamp(num2 - num4, 0, num));
			meshes[num5] = GetMesh(triCount, num4, totalWidth, totalHeight);
			num5++;
		}
		return meshes;
	}

	private static Mesh GetMesh(int triCount, int triOffset, int totalWidth, int totalHeight)
	{
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSave;
		Vector3[] array = new Vector3[triCount * 4];
		Vector2[] array2 = new Vector2[triCount * 4];
		Vector2[] array3 = new Vector2[triCount * 4];
		int[] array4 = new int[triCount * 6];
		for (int i = 0; i < triCount; i++)
		{
			int num = i * 4;
			int num2 = i * 6;
			int num3 = triOffset + i;
			float num4 = Mathf.Floor(num3 % totalWidth) / (float)totalWidth;
			float num5 = Mathf.Floor(num3 / totalWidth) / (float)totalHeight;
			Vector3 vector = new Vector3(num4 * 2f - 1f, num5 * 2f - 1f, 1f);
			array[num] = vector;
			array[num + 1] = vector;
			array[num + 2] = vector;
			array[num + 3] = vector;
			ref Vector2 reference = ref array2[num];
			reference = new Vector2(0f, 0f);
			ref Vector2 reference2 = ref array2[num + 1];
			reference2 = new Vector2(1f, 0f);
			ref Vector2 reference3 = ref array2[num + 2];
			reference3 = new Vector2(0f, 1f);
			ref Vector2 reference4 = ref array2[num + 3];
			reference4 = new Vector2(1f, 1f);
			ref Vector2 reference5 = ref array3[num];
			reference5 = new Vector2(num4, num5);
			ref Vector2 reference6 = ref array3[num + 1];
			reference6 = new Vector2(num4, num5);
			ref Vector2 reference7 = ref array3[num + 2];
			reference7 = new Vector2(num4, num5);
			ref Vector2 reference8 = ref array3[num + 3];
			reference8 = new Vector2(num4, num5);
			array4[num2] = num;
			array4[num2 + 1] = num + 1;
			array4[num2 + 2] = num + 2;
			array4[num2 + 3] = num + 1;
			array4[num2 + 4] = num + 2;
			array4[num2 + 5] = num + 3;
		}
		mesh.vertices = array;
		mesh.triangles = array4;
		mesh.uv = array2;
		mesh.uv2 = array3;
		return mesh;
	}
}
