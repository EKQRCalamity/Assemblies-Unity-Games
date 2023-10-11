using System;
using UnityEngine;
using UnityEngine.UI;

public class UIArcBarNotcher : UIBarNotcher
{
	[Range(0f, 1080f)]
	[Header("Arc Degree Range")]
	public float startDegree;

	[Range(1f, 1080f)]
	public float endDegree = 360f;

	[Range(0f, 1f)]
	public float radiusAtEnd = 1f;

	public bool clockwise;

	[Range(0.01f, 1f)]
	[Header("Arc Width")]
	public float width = 0.333f;

	[Range(0.01f, 1f)]
	public float widthAtExtrema = 1f;

	[Range(0.01f, 1f)]
	public float widthAtCenter = 1f;

	[Range(0.1f, 12f)]
	public float widthPower = 1f;

	public bool adjustWidthsForRectAspect;

	[Range(0f, 0.5f)]
	[Header("Padding")]
	public float innerRadiusPadding;

	[Range(0f, 0.5f)]
	public float outerRadiusPadding;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (startDegree >= endDegree)
		{
			return;
		}
		float num = range.Range() / Mathf.Max(MathUtil.BigEpsilon, notchEvery);
		int num2 = Mathf.Min(500, Mathf.CeilToInt(num) - 1);
		if (num2 > 0)
		{
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			Vector2 center = pixelAdjustedRect.center;
			Vector2 vector = pixelAdjustedRect.max - center;
			Vector2 vector2 = (adjustWidthsForRectAspect ? vector.Multiply(vector.normalized.Pow(0.5f)) : vector);
			float num3 = (clockwise ? endDegree : startDegree);
			float num4 = (clockwise ? startDegree : endDegree) - num3;
			float a = width * widthAtExtrema;
			float b = width * widthAtCenter;
			float num5 = 1f / Mathf.Max(MathUtil.BigEpsilon, num);
			float f = 1f * num5 * (num4 * (MathF.PI / 180f));
			Vector2 vector3 = new Vector2(Mathf.Cos(num3 * (MathF.PI / 180f)), Mathf.Sin(num3 * (MathF.PI / 180f)));
			float cos = Mathf.Cos(f);
			float sin = Mathf.Sin(f);
			float num6 = num4 * (MathF.PI / 180f) * pixelAdjustedRect.size.AbsMax() * 0.125f;
			_FillPowerOfTwoList(_notchWidthsPowersOfTwo, notchWidthRatios.Length);
			_FillPowerOfTwoList(_notchHeightsPowersOfTwo, notchHeightRatios.Length);
			_FillPowerOfTwoList(_notchColorsPowersOfTwo, notchColors.Length);
			int num7 = 1;
			for (int i = 1; i <= num2; i++)
			{
				vector3 = vector3.Rotate(cos, sin);
				float num8 = (float)i * num5;
				float num9 = Mathf.Lerp(1f, radiusAtEnd, num8);
				Vector2 vector4 = vector3.Multiply(vector * num9);
				Vector2 vector5 = vector3.Multiply(vector2 * num9 * (1f - Mathf.Lerp(a, b, Mathf.Pow(1f - Mathf.Abs(MathUtil.Remap(num8, new Vector2(0f, 1f), new Vector2(-1f, 1f))), widthPower))));
				Vector2 b2 = vector4;
				vector4 = Vector2.Lerp(vector4, vector5, outerRadiusPadding);
				vector5 = Vector2.Lerp(vector5, b2, innerRadiusPadding);
				Vector2 vector6 = vector3.PerpCCW();
				Vector2 vector7 = notchWidthRatios[_GetIndex(_notchWidthsPowersOfTwo, num7)] * num6 * vector6;
				float t = notchHeightRatios[_GetIndex(_notchHeightsPowersOfTwo, num7)];
				vector4 = Vector2.Lerp(vector5, vector4, t);
				Color color = notchColors[_GetIndex(_notchColorsPowersOfTwo, num7)].Multiply(this.color);
				vh.AddVert(vector5 - vector7, color, new Vector2(0f, 0f));
				vh.AddVert(vector4 - vector7, color, new Vector2(0f, 1f));
				vh.AddVert(vector4 + vector7, color, new Vector2(1f, 1f));
				vh.AddVert(vector5 + vector7, color, new Vector2(1f, 0f));
				int num10 = (i - 1) * 4;
				vh.AddTriangle(num10, num10 + 1, num10 + 2);
				vh.AddTriangle(num10, num10 + 2, num10 + 3);
				num7++;
			}
		}
	}
}
