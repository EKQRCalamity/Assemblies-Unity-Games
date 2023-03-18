using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.GoldenCorpse.Audio;

public class GoldenCorpseAudio : EntityAudio
{
	private const string FootStepEventKey = "GoldenCorpseWalk";

	private const string DeathEventKey = "GoldenCorpseWakeUp";

	private const string HitEventKey = "GoldenCorpseHit";

	public void PlayFootStep()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("GoldenCorpseWalk", FxSoundCategory.Motion);
		}
	}

	public void PlayAwake()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("GoldenCorpseWakeUp", FxSoundCategory.Motion);
		}
	}

	public void PlayHit()
	{
		PlayOneShotEvent("GoldenCorpseHit", FxSoundCategory.Damage);
	}
}
