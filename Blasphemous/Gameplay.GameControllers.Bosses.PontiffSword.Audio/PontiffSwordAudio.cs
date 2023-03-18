using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Bosses.PontiffSword.Audio;

public class PontiffSwordAudio : EntityAudio
{
	private const string PontiffSword_APPEAR = "PontiffSwordAppear";

	private const string PontiffSword_VANISH = "PontiffSwordVanish";

	private const string PontiffSword_PLUNGE = "PontiffSwordPlunge";

	private const string PontiffSword_SLASH = "PontiffSwordSlash";

	public void PlayAppear_AUDIO()
	{
		PlayOneShotEvent("PontiffSwordAppear", FxSoundCategory.Motion);
	}

	public void PlayVanish_AUDIO()
	{
		PlayOneShotEvent("PontiffSwordVanish", FxSoundCategory.Motion);
	}

	public void PlayPlunge_AUDIO()
	{
		PlayOneShotEvent("PontiffSwordPlunge", FxSoundCategory.Attack);
	}

	public void PlaySlash_AUDIO()
	{
		PlayOneShotEvent("PontiffSwordSlash", FxSoundCategory.Attack);
	}
}
