using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.GhostKnight.Audio;

public class GhostKnightAudio : EntityAudio
{
	private const string Prefix = "GhostKnight";

	private const string AppearingEventKey = "GhostKnightAppearing";

	private const string DisappearingEventKey = "GhostKnightDisappearing";

	private const string StartAttackEventKey = "GhostKnightStartAttack";

	private const string AttackEventKey = "GhostKnightAttack";

	private const string HitEventKey = "GhostKnightHit";

	private const string DeathEventKey = "GhostKnightDeath";

	private EventInstance _attackEventInstance;

	public void Appear()
	{
		PlayOneShotEvent("GhostKnightAppearing", FxSoundCategory.Motion);
	}

	public void Disappear()
	{
		PlayOneShotEvent("GhostKnightDisappearing", FxSoundCategory.Motion);
	}

	public void Damage()
	{
		PlayOneShotEvent("GhostKnightHit", FxSoundCategory.Damage);
	}

	public void Death()
	{
		PlayOneShotEvent("GhostKnightDeath", FxSoundCategory.Damage);
	}

	public void StartAttack()
	{
		PlayOneShotEvent("GhostKnightStartAttack", FxSoundCategory.Damage);
	}

	public void Attack()
	{
		if (!_attackEventInstance.isValid())
		{
			_attackEventInstance = base.AudioManager.CreateCatalogEvent("GhostKnightAttack");
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

	public void StopAttack()
	{
		if (_attackEventInstance.isValid())
		{
			_attackEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_attackEventInstance.release();
			_attackEventInstance = default(EventInstance);
		}
	}
}
