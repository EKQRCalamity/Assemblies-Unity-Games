using System;
using DG.Tweening;
using FMOD.Studio;
using Framework.Managers;
using UnityEngine;

namespace Tools.Level.Interactables;

[RequireComponent(typeof(Animator))]
public class ExecutionAnimationEvents : MonoBehaviour
{
	private Execution _execution;

	private Animator _animator;

	private EventInstance _executionSound;

	private ParameterInstance _hitParam;

	private void Start()
	{
		_execution = GetComponentInParent<Execution>();
		_animator = GetComponent<Animator>();
		Execution execution = _execution;
		execution.OnNormalTime = (Core.SimpleEvent)Delegate.Combine(execution.OnNormalTime, new Core.SimpleEvent(OnNormalTime));
		Execution execution2 = _execution;
		execution2.OnSlowMotion = (Core.SimpleEvent)Delegate.Combine(execution2.OnSlowMotion, new Core.SimpleEvent(OnSlowMotion));
		CreateExecutionSoundEvent();
	}

	private void SetAnimatorSpeed(float speed, float lapse)
	{
		DOTween.To(() => _animator.speed, delegate(float x)
		{
			_animator.speed = x;
		}, speed, lapse).SetUpdate(isIndependentUpdate: true).SetEase(Ease.Linear);
	}

	private void OnSlowMotion()
	{
	}

	private void OnNormalTime()
	{
	}

	public void DoSlowMotion()
	{
		if (!(_execution == null))
		{
			SetAnimatorSpeed(3f, 0.5f);
			_execution.DoSlowmotion();
		}
	}

	public void StopSlowMotion()
	{
		if (!(_execution == null))
		{
			_execution.StopSlowMotion();
			SetAnimatorSpeed(1f, 0f);
		}
	}

	public void ZoomIn()
	{
		if (!(_execution == null))
		{
			_execution.CameraZoomIn();
		}
	}

	public void ZoomOut()
	{
		if (!(_execution == null))
		{
			_execution.CameraZoomOut();
		}
	}

	public void CamShake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
	}

	private void CreateExecutionSoundEvent()
	{
		try
		{
			_executionSound = Core.Audio.CreateEvent(_execution.ActivationSound);
			_executionSound.getParameter("Hits", out _hitParam);
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
		}
	}

	public void PlayExecutionSound()
	{
		if (_executionSound.isValid())
		{
			_executionSound.start();
		}
	}

	public void StopExecutionSound()
	{
		if (_executionSound.isValid())
		{
			_executionSound.release();
		}
	}

	public void UpdateExecutionSoundEventParam(float param)
	{
		if (_hitParam.isValid())
		{
			_hitParam.setValue(param);
		}
	}

	public void Rumble()
	{
		Core.Logic.Penitent.Rumble.UsePreset("SimpleHit");
	}
}
