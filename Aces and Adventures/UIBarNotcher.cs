using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBarNotcher : MaskableGraphic
{
	protected const int MAX_NOTCHES = 500;

	[Header("Texture")]
	public Texture2D texture;

	[Header("Range")]
	public float notchEvery;

	public Vector2 range;

	[Header("Notch Attributes")]
	public float[] notchWidthRatios;

	public float[] notchHeightRatios;

	public Color[] notchColors;

	protected List<int> _notchWidthsPowersOfTwo;

	protected List<int> _notchHeightsPowersOfTwo;

	protected List<int> _notchColorsPowersOfTwo;

	public override Texture mainTexture => texture;

	protected override void Awake()
	{
		base.Awake();
		_notchWidthsPowersOfTwo = new List<int>();
		_notchHeightsPowersOfTwo = new List<int>();
		_notchColorsPowersOfTwo = new List<int>();
	}

	protected void _FillPowerOfTwoList(List<int> list, int length)
	{
		list.Clear();
		for (int i = 0; i < length; i++)
		{
			list.Add(Mathf.RoundToInt(Mathf.Pow(2f, i)));
		}
	}

	protected int _GetIndex(List<int> powerOfTwoList, int index)
	{
		for (int num = powerOfTwoList.Count - 1; num >= 0; num--)
		{
			if (index % powerOfTwoList[num] == 0)
			{
				return num;
			}
		}
		return 0;
	}

	public void SetRange(Vector2 newRange)
	{
		if (!(range == newRange))
		{
			range = newRange;
			SetVerticesDirty();
		}
	}

	public void SetMin(float min)
	{
		SetRange(new Vector2(min, range.y));
	}

	public void SetMax(float max)
	{
		SetRange(new Vector2(range.x, max));
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		float num = range.Range() / Mathf.Max(MathUtil.BigEpsilon, notchEvery);
		int num2 = Mathf.Min(500, Mathf.CeilToInt(num) - 1);
		if (num2 > 0)
		{
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			Vector2 min = pixelAdjustedRect.min;
			Vector2 max = pixelAdjustedRect.max;
			float num3 = texture.AspectRatio();
			float num4 = 1f / Mathf.Max(MathUtil.BigEpsilon, num);
			_FillPowerOfTwoList(_notchWidthsPowersOfTwo, notchWidthRatios.Length);
			_FillPowerOfTwoList(_notchHeightsPowersOfTwo, notchHeightRatios.Length);
			_FillPowerOfTwoList(_notchColorsPowersOfTwo, notchColors.Length);
			int num5 = 1;
			for (int i = 1; i <= num2; i++)
			{
				float num6 = notchWidthRatios[_GetIndex(_notchWidthsPowersOfTwo, num5)] * 0.5f * (max.x - min.x);
				float t = notchHeightRatios[_GetIndex(_notchHeightsPowersOfTwo, num5)];
				Color color = notchColors[_GetIndex(_notchColorsPowersOfTwo, num5)].Multiply(this.color);
				float num7 = Mathf.Lerp(min.x, max.x, (float)i * num4);
				float num8 = Mathf.Lerp(min.y, max.y, t);
				float y = (num8 - min.y) / Mathf.Max(MathUtil.BigEpsilon, num6 + num6) * num3;
				vh.AddVert(new Vector3(num7 - num6, min.y), color, new Vector2(0f, 0f));
				vh.AddVert(new Vector3(num7 - num6, num8), color, new Vector2(0f, y));
				vh.AddVert(new Vector3(num7 + num6, num8), color, new Vector2(1f, y));
				vh.AddVert(new Vector3(num7 + num6, min.y), color, new Vector2(1f, 0f));
				int num9 = (i - 1) * 4;
				vh.AddTriangle(num9, num9 + 1, num9 + 2);
				vh.AddTriangle(num9, num9 + 2, num9 + 3);
				num5++;
			}
		}
	}
}
