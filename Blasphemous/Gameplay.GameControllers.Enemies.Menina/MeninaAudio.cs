using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Menina;

public class MeninaAudio : EntityAudio
{
	private const string IdleEventKey = "MeninaIdle";

	private const string DeathEventKey = "MeninaDeath";

	private const string AttackEventKey = "MeninaAttack";

	private const string LeftLegEventKey = "MeninaFootStepLeft";

	private const string RightLegEventKey = "MeninaFootStepRight";

	protected EventInstance _attackSoundInstance;

	private const string MoveParameterKey = "Moves";

	public virtual void PlayDeath()
	{
		PlayOneShotEvent("MeninaDeath", FxSoundCategory.Damage);
	}

	public virtual void PlayAttack()
	{
		PlayEvent(ref _attackSoundInstance, "MeninaAttack");
	}

	public virtual void StopAttack()
	{
		StopEvent(ref _attackSoundInstance);
	}

	public virtual void PlayLeftLeg()
	{
		PlayOneShotEvent("MeninaFootStepLeft", FxSoundCategory.Motion);
	}

	public virtual void PlayRightLeg()
	{
		PlayOneShotEvent("MeninaFootStepRight", FxSoundCategory.Motion);
	}

	public virtual void PlayIdle()
	{
		if (Owner.SpriteRenderer.isVisible && !Owner.Status.Dead)
		{
			PlayOneShotEvent("MeninaIdle", FxSoundCategory.Motion);
		}
	}

	public void SetAttackMoveParam(float value)
	{
		SetMoveParam(_attackSoundInstance, value);
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
