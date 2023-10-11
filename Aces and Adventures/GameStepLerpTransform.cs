using System.Collections;
using UnityEngine;

public class GameStepLerpTransform : GameStep
{
	private Transform _transformToLerp;

	private TransformData _lerpTo;

	private float _duration;

	private TransformData _lerpFrom;

	private float _elapsedTime;

	public GameStepLerpTransform(Transform transformToLerp, TransformData lerpTo, float duration)
	{
		_transformToLerp = transformToLerp;
		_lerpTo = lerpTo;
		_duration = Mathf.Max(0.001f, duration);
	}

	public override void Start()
	{
		_lerpFrom = new TransformData(_transformToLerp);
	}

	protected override IEnumerator Update()
	{
		while (true)
		{
			_elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(_elapsedTime / _duration);
			_lerpFrom.Lerp(_lerpTo, MathUtil.CubicSplineInterpolant(num)).SetValues(_transformToLerp);
			if (num >= 1f)
			{
				break;
			}
			yield return null;
		}
	}
}
