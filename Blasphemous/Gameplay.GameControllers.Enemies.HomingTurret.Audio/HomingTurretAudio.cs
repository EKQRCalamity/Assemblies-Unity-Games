using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.HomingTurret.Audio;

public class HomingTurretAudio : EntityAudio
{
	private const string WakeupEventKey = "HomingTurretWakeUp";

	private const string IdleEventKey = "HomingTurretIdle";

	private const string ShotProjectileEventKey = "HomingTurretShot";

	private const string DeathEventKey = "HomingTurretDeath";

	private const string ChargeProjectileKey = "ProjectileCharge";

	private EventInstance _idleEventInstance;

	private SpriteRenderer OwnerSprite;

	[SerializeField]
	private float maxTimeInvisibleBeforeMute = 2f;

	private float _currentInvisibleTime;

	private bool _isPlayingIdle;

	private bool _onMute;

	protected override void OnStart()
	{
		base.OnStart();
		OwnerSprite = Owner.SpriteRenderer;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		StopIdleSoundWhenInvisible();
	}

	public void PlayWakeUp()
	{
		PlayOneShotEvent("HomingTurretWakeUp", FxSoundCategory.Motion);
	}

	public void PlayIdle()
	{
		StopIdle();
		_isPlayingIdle = true;
		_onMute = false;
		PlayEvent(ref _idleEventInstance, "HomingTurretIdle");
	}

	public void StopIdle()
	{
		_isPlayingIdle = false;
		StopEvent(ref _idleEventInstance);
	}

	public void PlayChargeProjectile()
	{
		if (OwnerSprite.isVisible)
		{
			PlayOneShotEvent("ProjectileCharge", FxSoundCategory.Attack);
		}
	}

	public void PlayShotProjectile()
	{
		PlayOneShotEvent("HomingTurretShot", FxSoundCategory.Attack);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("HomingTurretDeath", FxSoundCategory.Motion);
	}

	private void StopIdleSoundWhenInvisible()
	{
		if (!Owner.Status.Dead)
		{
			if (!OwnerSprite.isVisible && _isPlayingIdle)
			{
				_onMute = true;
				StopIdle();
			}
			else if (OwnerSprite.isVisible && _onMute)
			{
				PlayIdle();
			}
		}
	}

	private void OnDisable()
	{
		StopIdle();
	}
}
