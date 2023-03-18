using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Menina;

public class LionheadAudio : MeninaAudio
{
	private const string AttackEventKey = "LeonAttack";

	private const string DeathEventKey = "LeonDeath";

	public override void PlayDeath()
	{
		StopAttack();
		PlayOneShotEvent("LeonDeath", FxSoundCategory.Damage);
	}

	public override void PlayAttack()
	{
		PlayEvent(ref _attackSoundInstance, "LeonAttack");
	}

	public override void StopAttack()
	{
		StopEvent(ref _attackSoundInstance);
	}

	public override void PlayRightLeg()
	{
		base.PlayRightLeg();
	}

	public override void PlayLeftLeg()
	{
		base.PlayLeftLeg();
	}

	public override void PlayIdle()
	{
		base.PlayIdle();
	}
}
