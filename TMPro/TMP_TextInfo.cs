using System;
using UnityEngine;

namespace TMPro;

[Serializable]
public class TMP_TextInfo
{
	private static Vector2 k_InfinityVectorPositive = new Vector2(1000000f, 1000000f);

	private static Vector2 k_InfinityVectorNegative = new Vector2(-1000000f, -1000000f);

	public TMP_Text textComponent;

	public int characterCount;

	public int spriteCount;

	public int spaceCount;

	public int wordCount;

	public int linkCount;

	public int lineCount;

	public int pageCount;

	public int materialCount;

	public TMP_CharacterInfo[] characterInfo;

	public TMP_WordInfo[] wordInfo;

	public TMP_LinkInfo[] linkInfo;

	public TMP_LineInfo[] lineInfo;

	public TMP_PageInfo[] pageInfo;

	public TMP_MeshInfo[] meshInfo;

	public TMP_TextInfo()
	{
		characterInfo = new TMP_CharacterInfo[8];
		wordInfo = new TMP_WordInfo[16];
		linkInfo = new TMP_LinkInfo[0];
		lineInfo = new TMP_LineInfo[2];
		pageInfo = new TMP_PageInfo[16];
		meshInfo = new TMP_MeshInfo[1];
	}

	public TMP_TextInfo(TMP_Text textComponent)
	{
		this.textComponent = textComponent;
		characterInfo = new TMP_CharacterInfo[8];
		wordInfo = new TMP_WordInfo[4];
		linkInfo = new TMP_LinkInfo[0];
		lineInfo = new TMP_LineInfo[2];
		pageInfo = new TMP_PageInfo[16];
		meshInfo = new TMP_MeshInfo[1];
		meshInfo[0].mesh = textComponent.mesh;
		materialCount = 1;
	}

	public void Clear()
	{
		characterCount = 0;
		spaceCount = 0;
		wordCount = 0;
		linkCount = 0;
		lineCount = 0;
		pageCount = 0;
		spriteCount = 0;
		for (int i = 0; i < meshInfo.Length; i++)
		{
			meshInfo[i].vertexCount = 0;
		}
	}

	public void ClearMeshInfo(bool updateMesh)
	{
		for (int i = 0; i < meshInfo.Length; i++)
		{
			meshInfo[i].Clear(updateMesh);
		}
	}

	public void ClearAllMeshInfo()
	{
		for (int i = 0; i < meshInfo.Length; i++)
		{
			meshInfo[i].Clear(uploadChanges: true);
		}
	}

	public void ClearUnusedVertices(MaterialReference[] materials)
	{
		for (int i = 0; i < meshInfo.Length; i++)
		{
			int startIndex = 0;
			meshInfo[i].ClearUnusedVertices(startIndex);
		}
	}

	public void ClearLineInfo()
	{
		if (lineInfo == null)
		{
			lineInfo = new TMP_LineInfo[2];
		}
		for (int i = 0; i < lineInfo.Length; i++)
		{
			lineInfo[i].characterCount = 0;
			lineInfo[i].spaceCount = 0;
			lineInfo[i].width = 0f;
			lineInfo[i].ascender = k_InfinityVectorNegative.x;
			lineInfo[i].descender = k_InfinityVectorPositive.x;
			lineInfo[i].lineExtents.min = k_InfinityVectorPositive;
			lineInfo[i].lineExtents.max = k_InfinityVectorNegative;
			lineInfo[i].maxAdvance = 0f;
		}
	}

	public static void Resize<T>(ref T[] array, int size)
	{
		int newSize = ((size <= 1024) ? Mathf.NextPowerOfTwo(size) : (size + 256));
		Array.Resize(ref array, newSize);
	}

	public static void Resize<T>(ref T[] array, int size, bool isBlockAllocated)
	{
		if (size > array.Length)
		{
			if (isBlockAllocated)
			{
				size = ((size <= 1024) ? Mathf.NextPowerOfTwo(size) : (size + 256));
			}
			Array.Resize(ref array, size);
		}
	}
}
