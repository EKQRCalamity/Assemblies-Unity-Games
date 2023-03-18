using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CrossCrawler.Audio;

public class CrossCrawlerAudio : EntityAudio
{
	private const string WalkEventKey = "CrossCrawlerWalk";

	private const string DeathEventKey = "CrossCrawlerDeath";

	private const string AttackEventKey = "CrossCrawlerAttack";

	private const string TurnEventKey = "CrossCrawlerTurn";

	private EventInstance _walkEventInstance;

	private EventInstance _turnEventInstance;

	private EventInstance _chasingEventInstance;

	private EventInstance _attackEventInstance;

	private const string MoveParameterKey = "Moves";

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Owner.SpriteRenderer.isVisible || Owner.Status.Dead)
		{
			StopAll();
		}
	}

	public void PlayWalk()
	{
		PlayEvent(ref _walkEventInstance, "CrossCrawlerWalk");
	}

	public void StopWalk()
	{
		StopEvent(ref _walkEventInstance);
	}

	public void PlayAttack()
	{
		PlayEvent(ref _attackEventInstance, "CrossCrawlerAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void PlayTurnAround()
	{
		StopAll();
		PlayEvent(ref _turnEventInstance, "CrossCrawlerTurn");
	}

	public void StopTurnAround()
	{
		StopEvent(ref _turnEventInstance);
	}

	public void StopAll()
	{
		StopAttack();
		StopWalk();
		StopTurnAround();
	}

	public void PlayDeath()
	{
		StopAll();
		PlayOneShotEvent("CrossCrawlerDeath", FxSoundCategory.Damage);
	}

	public void SetAttackMoveParam(float value)
	{
		SetMoveParam(_attackEventInstance, value);
	}

	public void SetTurnMoveParam(float value)
	{
		SetMoveParam(_turnEventInstance, value);
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

	private void OnDestroy()
	{
		StopWalk();
	}
}
