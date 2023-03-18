using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.ViciousDasher.Audio;

public class ViciousDasherAudio : EntityAudio
{
	private const string AttackEventKey = "ViciousDasherAttack";

	private const string DashEventKey = "ViciousDasherDash";

	private const string DeathEventKey = "ViciousDasherDeath";

	public void PlayAttack()
	{
		PlayOneShotEvent("ViciousDasherAttack", FxSoundCategory.Attack);
	}

	public void PlayDash()
	{
		PlayOneShotEvent("ViciousDasherDash", FxSoundCategory.Motion);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("ViciousDasherDeath", FxSoundCategory.Damage);
	}
}
