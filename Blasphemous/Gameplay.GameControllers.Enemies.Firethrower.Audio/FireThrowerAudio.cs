using System;
using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Firethrower.Audio;

public class FireThrowerAudio : EntityAudio
{
	private const string AttackEventKey = "FireThrowerAttack";

	private const string WalkEventKey = "FireThrowerFootsteps";

	private const string DeathEventKey = "FireThrowerDeath";

	private EventInstance _attackEventInstance;

	private const string MoveParameterKey = "Moves";

	protected override void OnStart()
	{
		base.OnStart();
		Owner.OnDeath += OnDeath;
	}

	public void PlayFireThrowing()
	{
		StopFireThrowing();
		PlayEvent(ref _attackEventInstance, "FireThrowerAttack");
	}

	public void StopFireThrowing()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void PlayWalk()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			Core.Audio.PlayOneShotFromCatalog("FireThrowerFootsteps", Owner.transform.position);
		}
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("FireThrowerDeath", FxSoundCategory.Damage);
	}

	public void SetAttackParam(float value)
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

	private void OnDeath()
	{
		Owner.OnDeath -= OnDeath;
		StopFireThrowing();
	}
}
