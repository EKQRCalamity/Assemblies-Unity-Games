using UnityEngine;

namespace UnityStandardAssets.ImageEffects;

internal class Triangles
{
	private static Mesh[] meshes;

	private static int currentTris;

	private static bool HasMeshes()
	{
		if (meshes == null)
		{
			return false;
		}
		for (int i = 0; i < meshes.Length; i++)
		{
			if (null == meshes[i])
			{
				return false;
			}
		}
		return true;
	}

	private static void Cleanup()
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

	private static Mesh[] GetMeshes(int totalWidth, int totalHeight)
	{
		if (HasMeshes() && currentTris == totalWidth * totalHeight)
		{
			return meshes;
		}
		int num = 21666;
		int num2 = (currentTris = totalWidth * totalHeight);
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
		Vector3[] array = new Vector3[triCount * 3];
		Vector2[] array2 = new Vector2[triCount * 3];
		Vector2[] array3 = new Vector2[triCount * 3];
		int[] array4 = new int[triCount * 3];
		for (int i = 0; i < triCount; i++)
		{
			int num = i * 3;
			int num2 = triOffset + i;
			float num3 = Mathf.Floor(num2 % totalWidth) / (float)totalWidth;
			float num4 = Mathf.Floor(num2 / totalWidth) / (float)totalHeight;
			Vector3 vector = new Vector3(num3 * 2f - 1f, num4 * 2f - 1f, 1f);
			array[num] = vector;
			array[num + 1] = vector;
			array[num + 2] = vector;
			ref Vector2 reference = ref array2[num];
			reference = new Vector2(0f, 0f);
			ref Vector2 reference2 = ref array2[num + 1];
			reference2 = new Vector2(1f, 0f);
			ref Vector2 reference3 = ref array2[num + 2];
			reference3 = new Vector2(0f, 1f);
			ref Vector2 reference4 = ref array3[num];
			reference4 = new Vector2(num3, num4);
			ref Vector2 reference5 = ref array3[num + 1];
			reference5 = new Vector2(num3, num4);
			ref Vector2 reference6 = ref array3[num + 2];
			reference6 = new Vector2(num3, num4);
			array4[num] = num;
			array4[num + 1] = num + 1;
			array4[num + 2] = num + 2;
		}
		mesh.vertices = array;
		mesh.triangles = array4;
		mesh.uv = array2;
		mesh.uv2 = array3;
		return mesh;
	}
}
