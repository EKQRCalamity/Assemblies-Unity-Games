using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.Audio;

public class MeltedLadyAudio : EntityAudio
{
	private const string AttackEventKey = "MeltedLadyAttack";

	private const string AppearingEventKey = "MeltedLadyAppearing";

	private const string DisappearingEventKey = "MeltedLadyDisappearing";

	private const string DeathEventKey = "MeltedLadyDeath";

	private const string MeltedLadyDamage = "MeltedLadyHit";

	private EventInstance _attackEventInstance;

	private const string MoveParameterKey = "Moves";

	protected override void OnStart()
	{
		base.OnStart();
		Owner.OnDeath += OnDeath;
	}

	private void OnDeath()
	{
		StopAttack();
	}

	public void PlayAttack()
	{
		StopAttack();
		PlayEvent(ref _attackEventInstance, "MeltedLadyAttack");
	}

	public void Hurt()
	{
		PlayOneShotEvent("MeltedLadyHit", FxSoundCategory.Damage);
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void Appearing()
	{
		PlayOneShotEvent("MeltedLadyAppearing", FxSoundCategory.Motion);
	}

	public void Disappearing()
	{
		PlayOneShotEvent("MeltedLadyDisappearing", FxSoundCategory.Motion);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("MeltedLadyDeath", FxSoundCategory.Damage);
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
}
