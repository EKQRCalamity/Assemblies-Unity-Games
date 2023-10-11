using System.Collections;
using UnityEngine;

public class GameStepLetterbox : GameStep
{
	private static GameStepLetterbox _Active;

	private float _duration;

	private float? _minAspect;

	private float? _maxAspect;

	private float _elapsed;

	private float _startingMinAspect;

	private float _startingMaxAspect;

	public GameStepLetterbox(float duration, float? minAspect = null, float? maxAspect = null)
	{
		_duration = duration;
		_minAspect = minAspect;
		_maxAspect = maxAspect;
	}

	public override void Start()
	{
		_Active?.Cancel();
		_Active = this;
		_startingMinAspect = Letterbox.Instance.minAspect;
		_startingMaxAspect = Letterbox.Instance.maxAspect;
	}

	protected override IEnumerator Update()
	{
		while (true)
		{
			_elapsed += Time.deltaTime;
			float t = MathUtil.CubicSplineInterpolant(Mathf.Clamp01(_elapsed / _duration));
			if (_minAspect.HasValue)
			{
				Letterbox.Instance.minAspect = Mathf.LerpUnclamped(_startingMinAspect, _minAspect.Value, t);
			}
			if (_maxAspect.HasValue)
			{
				Letterbox.Instance.maxAspect = Mathf.LerpUnclamped(_startingMaxAspect, _maxAspect.Value, t);
			}
			if (_elapsed >= _duration)
			{
				break;
			}
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		_Active = ((_Active == this) ? null : _Active);
	}
}
