using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.BellGhost.Audio;

public class BellGhostAudio : EntityAudio
{
	private const string HurtEventKey = "BellGhostHurt";

	private const string AttackEventKey = "BellGhostAttack";

	private const string AppearEventKey = "BellGhostAppearing";

	private const string BrokenBellEventKey = "BellGhostBrokenBell";

	private const string DeathEventKey = "BellGhostDeath";

	private const string DissapearEventKey = "BellGhostDissapearing";

	private const string FloatingEventKey = "BellGhostFloating";

	private const string ChargeEventKey = "BellGhostCharge";

	private const string ShootEventKey = "GhostShoot";

	private const string VariantHurtKey = "GhostHurt";

	private EventInstance _attackEventInstance;

	private EventInstance _chargingEventInstance;

	private EventInstance _shootEventInstance;

	private EventInstance _floatingEventInstance;

	public void Hurt()
	{
		PlayOneShotEvent("BellGhostHurt", FxSoundCategory.Motion);
	}

	public void HurtVariant()
	{
		StopShoot();
		PlayOneShotEvent("GhostHurt", FxSoundCategory.Motion);
	}

	public void BrokenBell()
	{
		PlayOneShotEvent("BellGhostBrokenBell", FxSoundCategory.Motion);
	}

	public void Death()
	{
		StopShoot();
		PlayOneShotEvent("BellGhostDeath", FxSoundCategory.Motion);
	}

	public void PlayShoot()
	{
		if (Owner.Status.IsVisibleOnCamera)
		{
			PlayEvent(ref _shootEventInstance, "GhostShoot");
		}
	}

	public void StopShoot()
	{
		StopEvent(ref _shootEventInstance);
	}

	public void PlayAttack()
	{
		if (base.AudioManager != null && !_attackEventInstance.isValid())
		{
			_attackEventInstance = base.AudioManager.CreateCatalogEvent("BellGhostAttack");
			_attackEventInstance.start();
		}
	}

	public void UpdateAttackPanning()
	{
		if (_attackEventInstance.isValid())
		{
			SetPanning(_attackEventInstance);
		}
	}

	public void StopAttack(bool allowFade = true)
	{
		if (_attackEventInstance.isValid())
		{
			_attackEventInstance.stop((!allowFade) ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_attackEventInstance.release();
			_attackEventInstance = default(EventInstance);
		}
	}

	public void Appear()
	{
		PlayOneShotEvent("BellGhostAppearing", FxSoundCategory.Motion);
	}

	public void Dissapear()
	{
		if (Owner.Status.IsVisibleOnCamera)
		{
			PlayOneShotEvent("BellGhostDissapearing", FxSoundCategory.Motion);
		}
	}

	public void PlayFloating()
	{
		if (Owner.Status.IsVisibleOnCamera && !_floatingEventInstance.isValid())
		{
			_floatingEventInstance = base.AudioManager.CreateCatalogEvent("BellGhostFloating");
			_floatingEventInstance.start();
		}
	}

	public void UpdateFloatingPanning()
	{
		if (Owner.Status.IsVisibleOnCamera && _floatingEventInstance.isValid())
		{
			SetPanning(_floatingEventInstance);
		}
	}

	public void StopFloating()
	{
		if (_floatingEventInstance.isValid())
		{
			_floatingEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_floatingEventInstance.release();
			_floatingEventInstance = default(EventInstance);
		}
	}

	public void ChargeAttack()
	{
		if (Owner.Status.IsVisibleOnCamera && !_chargingEventInstance.isValid())
		{
			_chargingEventInstance = base.AudioManager.CreateCatalogEvent("BellGhostCharge");
			_chargingEventInstance.start();
			SetPanning(_chargingEventInstance);
		}
	}

	public void StopChargeAttack()
	{
		if (_chargingEventInstance.isValid())
		{
			_chargingEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_chargingEventInstance.release();
			_chargingEventInstance = default(EventInstance);
		}
	}

	private void OnDestroy()
	{
		StopFloating();
		StopChargeAttack();
	}
}
