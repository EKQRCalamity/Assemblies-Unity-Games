using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WheelCarrier.Audio;

public class WheelCarrierAudio : EntityAudio
{
	private const string StepLeftEventKey = "RocieroFootStepLeft";

	private const string StepRightEventKey = "RocieroFootStepRight";

	private const string DeathEventKey = "RocieroDeath";

	private const string AttackEventKey = "RocieroAttack";

	private const string MoveParameterKey = "Moves";

	private EventInstance _attackEventInstance;

	private EventInstance _chasingEventInstance;

	private EventInstance _walkEventInstance;

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	public void PlayStepLeft()
	{
		PlayOneShotEvent("RocieroFootStepLeft", FxSoundCategory.Motion);
	}

	public void PlayStepRight()
	{
		PlayOneShotEvent("RocieroFootStepRight", FxSoundCategory.Motion);
	}

	public void PlayAttack()
	{
		PlayEvent(ref _attackEventInstance, "RocieroAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void SetAttackMoveParam(float value)
	{
		SetMoveParam(_attackEventInstance, value);
	}

	public void PlayDeath()
	{
		StopAttack();
		PlayOneShotEvent("RocieroDeath", FxSoundCategory.Damage);
	}

	public void SetMoveParam(EventInstance eventInstance, float value)
	{
		try
		{
			eventInstance.getParameter("Moves", out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}
}
