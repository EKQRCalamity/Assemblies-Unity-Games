using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class GameStepTimeline : GameStep
{
	private PlayableDirector _playableDirector;

	private TransformData? _cameraLookAt;

	private float _cameraLookAtDuration;

	private GameStepStack _lookAtStepStack;

	public GameStepTimeline(PlayableDirector playableDirector, Transform cameraLookAt = null, float cameraLookAtDuration = 1f, GameStepStack lookAtStepStack = null)
	{
		_playableDirector = playableDirector;
		_cameraLookAt = cameraLookAt;
		_cameraLookAtDuration = cameraLookAtDuration;
		_lookAtStepStack = lookAtStepStack;
	}

	protected override void OnFirstEnabled()
	{
		_playableDirector.time = 0.0;
	}

	protected override void OnEnable()
	{
		_playableDirector.Play();
	}

	public override void Start()
	{
		if (_cameraLookAt.HasValue)
		{
			(_lookAtStepStack ?? GameStepStack.Active).ParallelProcess(new GameStepLerpTransform(base.manager.cameraLookAt, _cameraLookAt.Value, (float)_playableDirector.duration * _cameraLookAtDuration));
		}
	}

	protected override IEnumerator Update()
	{
		while (_playableDirector.state == PlayState.Playing && _playableDirector.time < _playableDirector.duration)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		_playableDirector.Pause();
	}
}
