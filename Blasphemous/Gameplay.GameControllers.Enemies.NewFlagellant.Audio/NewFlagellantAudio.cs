using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.NewFlagellant.Audio;

public class NewFlagellantAudio : EntityAudio
{
	private const string AttackEventKey = "FlagellantAttack";

	private const string DashEventKey = "FlagellantDash";

	private const string DeathEventKey = "FlagellantDeath";

	public const string FootStepEventKey = "FlagellantFootStep";

	public const string RunnigEventKey = "FlagellantRunning";

	public const string LandingEventKey = "FlagellantLanding";

	public const string BasicAttackEventKey = "FlagellantBasicAttack";

	public const string BasicAttackHitEventKey = "FlagellantAttackHit";

	public const string SelfLashEventKey = "FlagellantSelfLash";

	public const string BloodDecalEventKey = "FlagellantBloodDecal";

	private EventInstance _basicAttackEventInstance;

	public void PlayFootStep()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("FlagellantFootStep", FxSoundCategory.Motion);
		}
	}

	public void PlayRunning()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("FlagellantRunning", FxSoundCategory.Motion);
		}
	}

	public void PlayLandingSound()
	{
		PlayOneShotEvent("FlagellantLanding", FxSoundCategory.Motion);
	}

	public void PlayBasicAttack()
	{
		PlayOneShotEvent("FlagellantBasicAttack", FxSoundCategory.Attack);
	}

	public void PlayAttack()
	{
		PlayOneShotEvent("FlagellantAttack", FxSoundCategory.Attack);
	}

	public void PlayAttackHit()
	{
		PlayOneShotEvent("FlagellantAttackHit", FxSoundCategory.Attack);
	}

	public void PlaySelfLash()
	{
		if (base.AudioManager != null)
		{
			base.AudioManager.PlaySfxOnCatalog("FlagellantSelfLash");
		}
	}

	public void PlayBloodDecal()
	{
		if (base.AudioManager != null)
		{
			base.AudioManager.PlaySfxOnCatalog("FlagellantBloodDecal");
		}
	}

	public void PlayDeath()
	{
		if (base.AudioManager != null)
		{
			base.AudioManager.PlaySfxOnCatalog("FlagellantDeath");
		}
	}

	public void PlaySlashHit()
	{
		PlayAttackHit();
	}
}
