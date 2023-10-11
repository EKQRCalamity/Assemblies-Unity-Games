using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedImage : Image, IAnimatedUI
{
	[SerializeField]
	protected AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	protected AnimationCurve _alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private List<UIAnimation> _animations;

	private bool _animating;

	public AnimationCurve animationCurve => _animationCurve;

	public AnimationCurve alphaCurve => _alphaCurve;

	public List<UIAnimation> animations => _animations ?? (_animations = new List<UIAnimation>());

	public bool animating
	{
		get
		{
			return _animating;
		}
		set
		{
			_animating = value;
		}
	}

	private void Update()
	{
		if (_animating)
		{
			SetVerticesDirty();
		}
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		base.OnPopulateMesh(toFill);
		this.PopulateMesh(toFill);
	}
}
