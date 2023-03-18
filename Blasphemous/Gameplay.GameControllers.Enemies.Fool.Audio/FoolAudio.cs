using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Fool.Audio;

public class FoolAudio : EntityAudio
{
	private const string FootStepEventKey = "FoolFootStep";

	private const string DeathEventKey = "FoolDeath";

	public void PlayFootStep()
	{
		if (base.AudioManager != null)
		{
			PlayOneShotEvent("FoolFootStep", FxSoundCategory.Motion);
		}
	}

	public void PlayDeath()
	{
		if (base.AudioManager != null)
		{
			PlayOneShotEvent("FoolDeath", FxSoundCategory.Motion);
		}
	}
}
