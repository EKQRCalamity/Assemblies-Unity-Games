using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class GameStepTimelineReverse : GameStep
{
	private PlayableDirector _playableDirector;

	private double _originalSpeed;

	private TransformData? _cameraLookAt;

	private float _cameraLookAtDuration;

	public GameStepTimelineReverse(PlayableDirector playableDirector, Transform cameraLookAt = null, float cameraLookAtDuration = 1f)
	{
		_playableDirector = playableDirector;
		_cameraLookAt = cameraLookAt;
		_cameraLookAtDuration = cameraLookAtDuration;
	}

	protected override void OnFirstEnabled()
	{
		_playableDirector.time = _playableDirector.duration;
	}

	protected override void OnEnable()
	{
		_playableDirector.Play();
		_originalSpeed = _playableDirector.playableGraph.GetRootPlayable(0).GetSpeed();
		_playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0.0);
		_playableDirector.Evaluate();
	}

	public override void Start()
	{
		if (_cameraLookAt.HasValue)
		{
			GameStepStack.Active.ParallelProcess(new GameStepLerpTransform(base.manager.cameraLookAt, _cameraLookAt.Value, (float)_playableDirector.duration * _cameraLookAtDuration));
		}
	}

	protected override IEnumerator Update()
	{
		while (true)
		{
			_playableDirector.time -= Time.deltaTime;
			_playableDirector.time = Math.Max(0.0, _playableDirector.time);
			if (_playableDirector.time == 0.0)
			{
				break;
			}
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		_playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_originalSpeed);
		_playableDirector.Pause();
	}
}
