using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua.Audio;

public class PerpetuaAudio : EntityAudio
{
	public const string ThunderSwordEventKey = "PerpetuaThunderSword";

	public const string AppearEventKey = "PerpetuaAppear";

	public const string SlideAttackEventKey = "PerpetuaSlideAttack";

	public const string DisappearEventKey = "PerpetuaDisappear";

	public const string FlyEventKey = "PerpetuaFly";

	public void PlayFly()
	{
		PlayOneShotEvent("PerpetuaFly", FxSoundCategory.Motion);
	}

	public void PlayDisappear()
	{
		PlayOneShotEvent("PerpetuaDisappear", FxSoundCategory.Motion);
	}

	public void PlayAppear()
	{
		PlayOneShotEvent("PerpetuaAppear", FxSoundCategory.Motion);
	}

	public void PlaySlideAttack()
	{
		PlayOneShotEvent("PerpetuaSlideAttack", FxSoundCategory.Motion);
	}

	public void PlayThunderSword()
	{
		PlayOneShotEvent("PerpetuaThunderSword", FxSoundCategory.Motion);
	}
}
