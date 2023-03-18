using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Nun.Audio;

public class NunAudio : EntityAudio
{
	private const string WalkEventKey = "NunWalk";

	private const string DeathEventKey = "NunDeath";

	private const string AttackEventKey = "NunAttack";

	private const string TurnEventKey = "NunTurn";

	private const string OilPuddleEventKey = "OilPuddle";

	private const string VanishOilPuddleEventKey = "OilDisappear";

	private const string NunBotafumeiroEventKey = "NunBotafumeiro";

	private EventInstance _walkEventInstance;

	private EventInstance _turnEventInstance;

	private EventInstance _chasingEventInstance;

	private EventInstance _attackEventInstance;

	private EventInstance _floatingEventInstance;

	private EventInstance _botafumeiroEventInstance;

	private const string MoveParameterKey = "Moves";

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Owner.SpriteRenderer.isVisible && !Owner.Status.Dead)
		{
			PlayBotafumeiro();
			UpdateBotafumeiro();
		}
		else
		{
			StopBotafumeiro();
		}
	}

	public void PlayWalk()
	{
		PlayOneShotEvent("NunWalk", FxSoundCategory.Motion);
	}

	public void PlayAttack()
	{
		PlayEvent(ref _attackEventInstance, "NunAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void PlayTurnAround()
	{
		PlayEvent(ref _turnEventInstance, "NunTurn");
	}

	public void StopTurnAround()
	{
		StopEvent(ref _turnEventInstance);
	}

	public void PlayBotafumeiro()
	{
		PlayEvent(ref _botafumeiroEventInstance, "NunBotafumeiro");
	}

	public void StopBotafumeiro()
	{
		StopEvent(ref _botafumeiroEventInstance);
	}

	public void UpdateBotafumeiro()
	{
		UpdateEvent(ref _botafumeiroEventInstance);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("NunDeath", FxSoundCategory.Damage);
		if (_attackEventInstance.isValid())
		{
			_attackEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_attackEventInstance.release();
		}
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

	private void OnDestroy()
	{
		StopBotafumeiro();
	}
}
