using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.Audio;

public class BejeweledSaintAudio : EntityAudio
{
	private const string SmashMaceEventKey = "SaintAttack";

	private const string LiftUpMaceEventKey = "SaintAttackRelease";

	private const string AttackSlideEventKey = "SaintAttackSlide";

	private const string HandStompEventKey = "SaintHandStomp";

	private const string ThunderEventKey = "SaintThunder";

	private const string SaintFallEventKey = "SaintFall";

	private const string SaintRiseEventKey = "SaintRise";

	private const string HitEventKey = "SaintHit";

	private const string DeathEventKey = "SaintDeath";

	public void PlaySaintFall()
	{
		PlayOneShotEvent("SaintFall", FxSoundCategory.Motion);
	}

	public void PlaySaintRise()
	{
		PlayOneShotEvent("SaintRise", FxSoundCategory.Motion);
	}

	public void PlayHeadDamage()
	{
		PlayOneShotEvent("SaintHit", FxSoundCategory.Damage);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("SaintDeath", FxSoundCategory.Motion);
	}

	public void PlaySmashMace()
	{
		PlayOneShotEvent("SaintAttack", FxSoundCategory.Damage);
	}

	public void PlayLiftUpMace()
	{
		PlayOneShotEvent("SaintAttackRelease", FxSoundCategory.Damage);
	}

	public void PlaySlideAttack()
	{
		PlayOneShotEvent("SaintAttackSlide", FxSoundCategory.Damage);
	}

	public void PlayHandStomp()
	{
		PlayOneShotEvent("SaintHandStomp", FxSoundCategory.Damage);
	}

	public void PlayThunder()
	{
		PlayOneShotEvent("SaintThunder", FxSoundCategory.Damage);
	}
}
