using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public struct SimpleGradient
{
	public static readonly SimpleGradient Default = new SimpleGradient(Color.white);

	public static readonly SimpleGradient Fade = new SimpleGradient(Color.white, null, Color.white.SetAlpha(0f));

	[ProtoMember(1, IsRequired = true)]
	[UIField]
	private Color32 _startColor;

	[ProtoMember(2, IsRequired = true)]
	[UIField]
	private Color32? _midColor;

	[ProtoMember(3, IsRequired = true)]
	[UIField]
	private Color32? _endColor;

	private Gradient _gradient;

	public Gradient gradient
	{
		get
		{
			if (_gradient == null)
			{
				List<GradientColorKey> list = new List<GradientColorKey>();
				List<GradientAlphaKey> list2 = new List<GradientAlphaKey>();
				list.Add(new GradientColorKey(_startColor, 0f));
				list2.Add(new GradientAlphaKey(_startColor.Alpha(), 0f));
				if (_midColor.HasValue)
				{
					list.Add(new GradientColorKey(_midColor.Value, 0.5f));
					list2.Add(new GradientAlphaKey(_midColor.Value.Alpha(), 0.5f));
				}
				if (_endColor.HasValue)
				{
					list.Add(new GradientColorKey(_endColor.Value, 1f));
					list2.Add(new GradientAlphaKey(_endColor.Value.Alpha(), 1f));
				}
				_gradient = new Gradient
				{
					colorKeys = list.ToArray(),
					alphaKeys = list2.ToArray(),
					mode = GradientMode.Blend
				};
			}
			return _gradient;
		}
	}

	public SimpleGradient(Color32 startColor, Color32? midColor = null, Color32? endColor = null)
	{
		_startColor = startColor;
		_midColor = midColor;
		_endColor = endColor;
		_gradient = null;
	}

	public static implicit operator Gradient(SimpleGradient simpleGradient)
	{
		return simpleGradient.gradient;
	}

	public static bool operator ==(SimpleGradient a, SimpleGradient b)
	{
		if (a._startColor.EqualTo(b._startColor) && a._midColor.EqualTo(b._midColor))
		{
			return a._endColor.EqualTo(b._endColor);
		}
		return false;
	}

	public static bool operator !=(SimpleGradient a, SimpleGradient b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj is SimpleGradient)
		{
			return (SimpleGradient)obj == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private void OnValueChanged()
	{
		_gradient = null;
	}
}
