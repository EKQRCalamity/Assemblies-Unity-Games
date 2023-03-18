using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WalkingTomb.Audio;

public class WalkingTombAudio : EntityAudio
{
	public const string WalkStepEventKey = "FernandoFootsteps";

	public const string DeathEventKey = "FernandoDeath";

	public const string GuardEventKey = "FernandoBackHit";

	private const string AttackEventKey = "FernandoAttack";

	private const string MoveParameterKey = "Moves";

	private EventInstance _attackEventInstance;

	protected override void OnStart()
	{
		base.OnStart();
		Owner.OnDeath += OnDeath;
	}

	public void Walk()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("FernandoFootsteps", FxSoundCategory.Motion);
		}
	}

	public void Death()
	{
		PlayOneShotEvent("FernandoDeath", FxSoundCategory.Damage);
	}

	public void PlayGuardHit()
	{
		PlayOneShotEvent("FernandoBackHit", FxSoundCategory.Damage);
	}

	public void PlayAttack()
	{
		StopAttack();
		PlayEvent(ref _attackEventInstance, "FernandoAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void SetAttackMoveParam(float value)
	{
		SetMoveParam(_attackEventInstance, value);
	}

	private void SetMoveParam(EventInstance eventInstance, float value)
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

	private void OnDeath()
	{
		Owner.OnDeath -= OnDeath;
		StopAttack();
	}
}
