using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Bosses.PontiffGiant.Audio;

public class PontiffGiantAudio : EntityAudio
{
	private const string PontiffGiant_DEATH = "PontiffGiantDeath";

	private const string PontiffGiant_PURPLE_SPELL = "PontiffGiantPurpleSpell";

	private const string PontiffGiant_GREEN_SPELL = "PontiffGiantGreenSpell";

	private const string PontiffGiant_BLUE_SPELL = "PontiffGiantBlueSpell";

	private const string PontiffGiant_OPEN_MASK = "PontiffGiantMaskOpen";

	private const string PontiffGiant_CLOSE_MASK = "PontiffGiantMaskClose";

	public void PlayPurpleSpell_AUDIO()
	{
		PlayOneShotEvent("PontiffGiantPurpleSpell", FxSoundCategory.Attack);
	}

	public void PlayGreenSpell_AUDIO()
	{
		PlayOneShotEvent("PontiffGiantGreenSpell", FxSoundCategory.Attack);
	}

	public void PlayBlueSpell_AUDIO()
	{
		PlayOneShotEvent("PontiffGiantBlueSpell", FxSoundCategory.Attack);
	}

	public void PlayDeath_AUDIO()
	{
		PlayOneShotEvent("PontiffGiantDeath", FxSoundCategory.Damage);
	}

	public void PlayOpenMask()
	{
		PlayOneShotEvent("PontiffGiantMaskOpen", FxSoundCategory.Motion);
	}

	public void PlayCloseMask()
	{
		PlayOneShotEvent("PontiffGiantMaskClose", FxSoundCategory.Motion);
	}
}
