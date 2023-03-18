using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Bishop.Audio;

public class BishopAudio : EntityAudio
{
	private const string AttackEventKey = "BishopAttack";

	private const string FloatingEventKey = "BishopFloating";

	private const string DeathEventKey = "BishopDeath";

	private const string ChasingEventKey = "BishopChasing";

	private const string MoveParameterKey = "Moves";

	private EventInstance _chasingEventInstance;

	private EventInstance _floatingEventInstance;

	private EventInstance _attackEventInstance;

	protected override void OnStart()
	{
		base.OnStart();
		Owner.OnDamaged += OnDamagedEntity;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		bool flag = Owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
		bool flag2 = Owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Chasing");
		if (flag)
		{
			if (Owner.SpriteRenderer.isVisible)
			{
				PlayEvent(ref _floatingEventInstance, "BishopFloating");
				StopEvent(ref _chasingEventInstance);
				UpdateEvent(ref _floatingEventInstance);
			}
		}
		else
		{
			StopEvent(ref _floatingEventInstance);
		}
		if (flag2)
		{
			PlayEvent(ref _chasingEventInstance, "BishopChasing");
			StopEvent(ref _floatingEventInstance);
			UpdateEvent(ref _chasingEventInstance);
		}
		else
		{
			StopEvent(ref _chasingEventInstance);
		}
		if (!Owner.SpriteRenderer.isVisible)
		{
			StopEvent(ref _chasingEventInstance);
			StopEvent(ref _floatingEventInstance);
		}
	}

	public void SetAttackParam(float value)
	{
		SetMoveParam(_attackEventInstance, value);
	}

	private void OnDamagedEntity()
	{
		StopAttack();
	}

	public void PlayAttack()
	{
		PlayEvent(ref _attackEventInstance, "BishopAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void StopAll()
	{
		StopAttack();
		StopEvent(ref _chasingEventInstance);
		StopEvent(ref _floatingEventInstance);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("BishopDeath", FxSoundCategory.Damage);
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
		Owner.OnDamaged -= OnDamagedEntity;
		StopEvent(ref _floatingEventInstance);
		StopEvent(ref _chasingEventInstance);
	}
}
