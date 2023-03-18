using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.ChainedAngel.Audio;

public class ChainedAngelAudio : EntityAudio
{
	public const string FlyEventKey = "AngelFly";

	public const string AttackEventKey = "CageAngelAttack";

	public const string DeathEventKey = "CageAngelDeath";

	public void PlayFlap()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("AngelFly", FxSoundCategory.Motion);
		}
	}

	public void PlayAttack()
	{
		PlayOneShotEvent("CageAngelAttack", FxSoundCategory.Motion);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("CageAngelDeath", FxSoundCategory.Damage);
	}
}
