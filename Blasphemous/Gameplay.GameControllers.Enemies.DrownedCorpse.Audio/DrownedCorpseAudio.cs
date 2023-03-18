using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.DrownedCorpse.Audio;

public class DrownedCorpseAudio : EntityAudio
{
	private const string RunEventKey = "DrownedCorpseRun";

	private const string HiddenEventKey = "DrownedCorpseHidden";

	private const string WakeupEventKey = "DronwedCorpseWakeUp";

	private const string DamageHiddenEventKey = "DrownedCorpseDamageHidden";

	public void PlayRun()
	{
		PlayOneShotEvent("DrownedCorpseRun", FxSoundCategory.Motion);
	}

	public void PlayHidden()
	{
		PlayOneShotEvent("DrownedCorpseHidden", FxSoundCategory.Motion);
	}

	public void PlayWakeup()
	{
		PlayOneShotEvent("DronwedCorpseWakeUp", FxSoundCategory.Motion);
	}

	public void PlayDamageHidden()
	{
		PlayOneShotEvent("DrownedCorpseDamageHidden", FxSoundCategory.Damage);
	}
}
