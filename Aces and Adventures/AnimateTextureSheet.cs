using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AnimateTextureSheet : AnimateComponent
{
	[Header("Texture Sheet")]
	public string textureName = "_MainTex";

	public int rows = 1;

	public int columns = 1;

	[Header("Animation")]
	public AnimationCurve frameSample = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationType animType = AnimationType.LoopAdditive;

	public float framesPerSecond = -1f;

	private float rowHeight;

	private float columnWidth;

	private int numCells;

	private int numCellsMinusOne;

	private bool propertyFound;

	private Material material;

	private Vector2 previousCellPosition;

	public void CalculateInternals()
	{
		rows = Math.Max(1, rows);
		columns = Math.Max(1, columns);
		rowHeight = 1f / (float)rows;
		columnWidth = 1f / (float)columns;
		numCells = rows * columns;
		numCellsMinusOne = numCells - 1;
	}

	private void OnValidate()
	{
		CalculateInternals();
	}

	public override void CacheInitialValues()
	{
		CalculateInternals();
		if (string.IsNullOrEmpty(textureName))
		{
			textureName = "_MainTex";
		}
		material = GetComponent<Renderer>().material;
		propertyFound = material.GetTexture(textureName);
		if (!propertyFound && textureName[0] != '_')
		{
			textureName = "_" + textureName;
			propertyFound = material.GetTexture(textureName);
		}
		if (!propertyFound)
		{
			Debug.LogError("~AnimateTextureSheet: Failed to find [Texture] named [" + textureName + "] in material [" + material.name + "] of GameObject [" + base.gameObject.name + "].");
		}
		else
		{
			material.SetTextureScale(textureName, new Vector2(columnWidth, rowHeight));
		}
	}

	protected override void UniqueUpdate(float t)
	{
		if (propertyFound)
		{
			if (framesPerSecond > 0f)
			{
				loopTime = (float)numCells / framesPerSecond;
			}
			t = GetValue(Vector2.up, frameSample, animType, t, 1f);
			Vector2 cellPosition = GetCellPosition(t);
			if (cellPosition != previousCellPosition)
			{
				material.SetTextureOffset(textureName, cellPosition);
				previousCellPosition = cellPosition;
			}
		}
	}

	private Vector2 GetCellPosition(float t)
	{
		int num = (int)(t * (float)numCellsMinusOne);
		int num2 = num / columns;
		return new Vector2((float)(num - num2 * columns) * columnWidth, 1f - (float)(num2 + 1) * rowHeight);
	}
}
