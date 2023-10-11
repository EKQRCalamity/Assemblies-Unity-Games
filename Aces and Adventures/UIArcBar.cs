using System;
using UnityEngine;
using UnityEngine.UI;

public class UIArcBar : MaskableGraphic
{
	[Header("Image")]
	public Sprite sprite;

	[Range(0f, 1080f)]
	[Header("Range")]
	public float startDegree;

	[Range(1f, 1080f)]
	public float endDegree = 360f;

	[Range(0f, 1f)]
	public float radiusAtEnd = 1f;

	[Range(0.01f, 1f)]
	[Header("Width")]
	public float width = 0.333f;

	[Range(0.01f, 1f)]
	public float widthAtExtrema = 1f;

	[Range(0.01f, 1f)]
	public float widthAtCenter = 1f;

	[Range(0.1f, 12f)]
	public float widthPower = 1f;

	public bool adjustWidthsForRectAspect;

	[Range(3f, 360f)]
	[Header("Resolution")]
	public int subdivisions = 36;

	[Range(0f, 120f)]
	public int subdivisionEveryXDegrees = 10;

	[Range(0f, 1f)]
	[Header("Fill")]
	public float fillAmount = 1f;

	public bool clockwiseFill;

	public override Texture mainTexture
	{
		get
		{
			if (!(sprite != null))
			{
				if (!(material != null) || !(material.mainTexture != null))
				{
					return Graphic.s_WhiteTexture;
				}
				return material.mainTexture;
			}
			return sprite.texture;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (startDegree >= endDegree)
		{
			return;
		}
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		Vector2 center = pixelAdjustedRect.center;
		Vector2 vector = pixelAdjustedRect.max - center;
		Vector2 vector2 = (adjustWidthsForRectAspect ? vector.Multiply(vector.normalized.Pow(0.5f)) : vector);
		float num = (clockwiseFill ? Mathf.Lerp(startDegree, endDegree, 1f - fillAmount) : startDegree);
		float num2 = (clockwiseFill ? endDegree : Mathf.Lerp(startDegree, endDegree, fillAmount)) - num;
		if (!(num2 <= 0f))
		{
			int num3 = ((subdivisionEveryXDegrees > 0) ? Mathf.CeilToInt(num2 / (float)subdivisionEveryXDegrees) : subdivisions);
			float a = width * widthAtExtrema;
			float b = width * widthAtCenter;
			float num4 = 1f / (float)((subdivisionEveryXDegrees > 0) ? Mathf.CeilToInt((endDegree - num) / (float)subdivisionEveryXDegrees) : subdivisions) * fillAmount;
			int num5 = num3 + 1;
			float f = 1f / (float)num3 * (num2 * (MathF.PI / 180f));
			Vector2 vector3 = new Vector2(Mathf.Cos(num * (MathF.PI / 180f)), Mathf.Sin(num * (MathF.PI / 180f)));
			float cos = Mathf.Cos(f);
			float sin = Mathf.Sin(f);
			for (int i = 0; i < num5; i++)
			{
				float num6 = (float)i * num4 / fillAmount.InsureNonZero();
				float num7 = Mathf.Lerp(1f, radiusAtEnd, num6);
				Vector2 vector4 = vector3.Multiply(vector * num7);
				Vector2 vector5 = vector3.Multiply(vector2 * num7 * (1f - Mathf.Lerp(a, b, Mathf.Pow(1f - Mathf.Abs(MathUtil.Remap(num6, new Vector2(0f, 1f), new Vector2(-1f, 1f))), widthPower))));
				vh.AddVert(center + vector5, color, new Vector2(num6, 0f));
				vh.AddVert(center + vector4, color, new Vector2(num6, 1f));
				vector3 = vector3.Rotate(cos, sin);
			}
			for (int j = 0; j < num3; j++)
			{
				int num8 = j + j;
				vh.AddTriangle(num8, num8 + 1, num8 + 2);
				vh.AddTriangle(num8 + 1, num8 + 2, num8 + 3);
			}
		}
	}

	public void SetFillAmount(float fill)
	{
		if (fillAmount != fill)
		{
			fillAmount = fill;
			SetVerticesDirty();
		}
	}
}
