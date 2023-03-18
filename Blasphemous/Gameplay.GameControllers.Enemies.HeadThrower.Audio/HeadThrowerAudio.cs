using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.HeadThrower.Audio;

public class HeadThrowerAudio : EntityAudio
{
	private const string DeathEventKey = "HeadThrowerDeath";

	public void PlayDeath()
	{
		if (Owner != null)
		{
			PlayOneShotEvent("HeadThrowerDeath", FxSoundCategory.Damage);
		}
	}
}
