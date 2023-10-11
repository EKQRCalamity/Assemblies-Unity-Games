using System;
using UnityEngine;
using UnityEngine.UI;

public class SmartLayoutElement : LayoutElement
{
	[SerializeField]
	protected float _maxPreferredWidth;

	[SerializeField]
	protected float _maxPreferredHeight;

	private float _aspectRatioMultiplier = 1f;

	public override float preferredWidth
	{
		get
		{
			_aspectRatioMultiplier = 1f;
			float num = base.preferredWidth;
			if (maxPreferredWidth > 0f && num > maxPreferredWidth)
			{
				_aspectRatioMultiplier = maxPreferredWidth / num;
			}
			if (maxPreferredHeight > 0f && base.preferredHeight > maxPreferredHeight)
			{
				_aspectRatioMultiplier = Math.Min(_aspectRatioMultiplier, maxPreferredHeight / base.preferredHeight);
			}
			num *= _aspectRatioMultiplier;
			if (!(maxPreferredWidth <= 0f))
			{
				return Mathf.Min(num, maxPreferredWidth);
			}
			return num;
		}
		set
		{
			base.preferredWidth = value;
		}
	}

	public override float preferredHeight
	{
		get
		{
			float num = base.preferredHeight * _aspectRatioMultiplier;
			if (!(maxPreferredHeight <= 0f))
			{
				return Mathf.Min(num, maxPreferredHeight);
			}
			return num;
		}
		set
		{
			base.preferredHeight = value;
		}
	}

	public float maxPreferredWidth
	{
		get
		{
			return _maxPreferredWidth;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxPreferredWidth, value))
			{
				SetDirty();
			}
		}
	}

	public float maxPreferredHeight
	{
		get
		{
			return _maxPreferredHeight;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxPreferredHeight, value))
			{
				SetDirty();
			}
		}
	}

	public Vector2 preferredDimensions
	{
		get
		{
			return new Vector2(base.preferredWidth, base.preferredHeight);
		}
		set
		{
			base.preferredWidth = value.x;
			base.preferredHeight = value.y;
		}
	}
}
