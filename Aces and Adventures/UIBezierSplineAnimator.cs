using System;
using UnityEngine;

[RequireComponent(typeof(UIBezierSpline))]
public class UIBezierSplineAnimator : MonoBehaviour
{
	[Serializable]
	public class AnimationData
	{
		public bool enabled;

		public AnimationType animationType = AnimationType.LoopAdditive;

		public float speed = 1f;

		public float curveMagnitudeMultiplier = 1f;

		public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public float Sample(float time)
		{
			return animationType.Evaluate(curve, time * speed) * curveMagnitudeMultiplier;
		}

		public static implicit operator bool(AnimationData animationData)
		{
			return animationData?.enabled ?? false;
		}
	}

	public bool useScaledTime;

	[Header("UV")]
	public AnimationData uShift;

	public AnimationData vShift;

	private UIBezierSpline _spline;

	public UIBezierSpline spline => this.CacheComponent(ref _spline);

	private void Update()
	{
		float time = GameUtil.GetTime(useScaledTime);
		if ((bool)uShift)
		{
			spline.uShift = uShift.Sample(time);
		}
		if ((bool)vShift)
		{
			spline.vShift = vShift.Sample(time);
		}
	}
}
