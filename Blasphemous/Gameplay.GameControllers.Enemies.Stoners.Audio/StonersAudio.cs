using System.Collections.Generic;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Stoners.Audio;

public class StonersAudio : EntityAudio
{
	public const string RaiseEventKey = "StonersRaise";

	public const string ThrowRockEventKey = "StonersThrowRock";

	public const string PassRockEventKey = "StonersPassRock";

	public const string RockHit = "StonersRockHit";

	public const string DamageEventKey = "StonersDamage";

	public const string DeathEventKey = "StonersDeath";

	protected override void OnWake()
	{
		base.OnWake();
		EventInstances = new List<EventInstance>();
	}

	public void Raise()
	{
		PlayOneShotEvent("StonersRaise", FxSoundCategory.Motion);
	}

	public void ThrowRock()
	{
		PlayOneShotEvent("StonersThrowRock", FxSoundCategory.Attack);
	}

	public void PassRock()
	{
		PlayOneShotEvent("StonersPassRock", FxSoundCategory.Attack);
	}

	public void Damage()
	{
		PlayOneShotEvent("StonersDamage", FxSoundCategory.Damage);
	}

	public void Death()
	{
		PlayOneShotEvent("StonersDeath", FxSoundCategory.Damage);
	}
}
