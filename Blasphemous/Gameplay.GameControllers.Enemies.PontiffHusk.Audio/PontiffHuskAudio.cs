using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.Audio;

public class PontiffHuskAudio : EntityAudio
{
	private const string AttackRangedEventKey = "PontiffHuskRangedAttack";

	private const string AttackMeleEventKey = "PontiffHuskMeleAttack";

	private const string AppearEventKey = "PontiffHuskVanishIn";

	private const string DeathEventKey = "PontiffHuskRangedDeath";

	private const string CutDeathEventKey = "PontiffHuskMeleDeath";

	private const string DissapearEventKey = "PontiffHuskVanishOut";

	private EventInstance _attackEventInstance;

	private EventInstance _chargingEventInstance;

	private EventInstance _shootEventInstance;

	private EventInstance _floatingEventInstance;

	public void Appear()
	{
		PlayOneShotEvent("PontiffHuskVanishIn", FxSoundCategory.Motion);
	}

	public void ChargeAttack()
	{
		if (Owner.Status.IsVisibleOnCamera && !_chargingEventInstance.isValid())
		{
			_chargingEventInstance = base.AudioManager.CreateCatalogEvent("PontiffHuskMeleAttack");
			_chargingEventInstance.start();
			SetPanning(_chargingEventInstance);
		}
	}

	public void Dissapear()
	{
		if (Owner.Status.IsVisibleOnCamera)
		{
			PlayOneShotEvent("PontiffHuskVanishOut", FxSoundCategory.Motion);
		}
	}

	public void Death(bool cut)
	{
		StopShoot();
		if (cut)
		{
			PlayOneShotEvent("PontiffHuskMeleDeath", FxSoundCategory.Motion);
		}
		else
		{
			PlayOneShotEvent("PontiffHuskRangedDeath", FxSoundCategory.Motion);
		}
	}

	public void PlayShoot()
	{
		if (Owner.Status.IsVisibleOnCamera)
		{
			StopShoot();
			PlayEvent(ref _shootEventInstance, "PontiffHuskRangedAttack");
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
			_attackEventInstance = base.AudioManager.CreateCatalogEvent("PontiffHuskMeleAttack");
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
