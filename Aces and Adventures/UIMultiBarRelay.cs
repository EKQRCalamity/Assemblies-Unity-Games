using System;
using UnityEngine;

public class UIMultiBarRelay : MonoBehaviour
{
	[Range(2f, 10f)]
	public int numberOfBars = 2;

	[SerializeField]
	[HideInInspector]
	private float _fillRatio = -1f;

	public FloatEvent[] OnFillRatioChanges;

	public BoolEvent[] OnHasFillChanges;

	public float fillRatio
	{
		get
		{
			return _fillRatio;
		}
		set
		{
			value = Math.Max(0f, value);
			if (_fillRatio == value)
			{
				return;
			}
			float num = -1f / (float)numberOfBars;
			for (int i = 0; i < numberOfBars; i++)
			{
				float num2 = num * (float)i;
				float num3 = ((_fillRatio >= 0f) ? Mathf.Clamp01((_fillRatio + num2) * (float)numberOfBars) : (-1f));
				float num4 = Mathf.Clamp01((value + num2) * (float)numberOfBars);
				if (num3 != num4)
				{
					OnFillRatioChanges[i].Invoke(num4);
					if (Math.Sign(num3) != Math.Sign(num4))
					{
						OnHasFillChanges[i].Invoke(num4 > 0f);
					}
				}
			}
			_fillRatio = value;
		}
	}

	private void _SetDirty()
	{
		_fillRatio = -1f;
	}

	private void OnEnable()
	{
		_SetDirty();
	}
}
