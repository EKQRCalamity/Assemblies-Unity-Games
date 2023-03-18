using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MasterAnguish.Audio;

public class ElderBrotherAudio : EntityAudio
{
	private const string JumpKey = "ElderBrotherJump";

	private const string LandingKey = "ElderBrotherLanding";

	private const string DeathKey = "ElderBrotherDeath";

	private const string AttackEventKey = "ElderBrotherAttack";

	private const string MoveParameterKey = "Moves";

	private EventInstance _attackEventInstance;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Owner.Status.Dead)
		{
			StopAll();
		}
	}

	public void PlayDeath()
	{
		StopAll();
		PlayOneShotEvent("ElderBrotherDeath", FxSoundCategory.Motion);
	}

	public void PlayJump()
	{
		StopAll();
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("ElderBrotherJump", FxSoundCategory.Motion);
		}
	}

	public void PlayDummyJump()
	{
		StopAll();
		PlayOneShotEvent("ElderBrotherJump", FxSoundCategory.Motion);
	}

	public void PlayJumpLanding()
	{
		StopAll();
		PlayOneShotEvent("ElderBrotherLanding", FxSoundCategory.Motion);
	}

	public void PlayAttack()
	{
		StopAttack();
		PlayEvent(ref _attackEventInstance, "ElderBrotherAttack", checkSpriteRendererVisible: false);
	}

	public void PlayAttackMove2()
	{
		SetAttackMoveParam(1f);
	}

	public void PlayAttackMove3()
	{
		SetAttackMoveParam(2f);
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	private void SetAttackMoveParam(float value)
	{
		SetMoveParam(_attackEventInstance, value);
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

	public void StopAll()
	{
		StopAttack();
	}
}
