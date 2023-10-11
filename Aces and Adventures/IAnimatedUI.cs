using System.Collections.Generic;
using UnityEngine;

public interface IAnimatedUI
{
	AnimationCurve animationCurve { get; }

	AnimationCurve alphaCurve { get; }

	List<UIAnimation> animations { get; }

	bool animating { get; set; }
}
