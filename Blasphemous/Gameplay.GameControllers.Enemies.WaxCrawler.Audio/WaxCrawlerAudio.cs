using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.WaxCrawler.Audio;

public class WaxCrawlerAudio : CreepCrawlerAudio
{
	private const string EntityPrefix = "WaxCrawler";

	private const string AppearingEventKey = "WaxCrawlerAppearing";

	private const string WalkEventKey = "WaxCrawlerWalk";

	private const string DisapearingEventKey = "WaxCrawlerDisappearing";

	private const string HurtEventKey = "WaxCrawlerHurt";

	private const string DeathEventKey = "WaxCrawlerDeath";

	public override void Appear()
	{
		if (Owner.Status.IsVisibleOnCamera)
		{
			PlayOneShotEvent("WaxCrawlerAppearing", FxSoundCategory.Motion);
		}
	}

	public override void Disappear()
	{
		if (Spawned)
		{
			Spawned = false;
		}
		else if (Owner.Status.IsVisibleOnCamera)
		{
			PlayOneShotEvent("WaxCrawlerDisappearing", FxSoundCategory.Motion);
		}
	}

	public override void Hurt()
	{
		PlayOneShotEvent("WaxCrawlerHurt", FxSoundCategory.Motion);
	}

	public override void Death()
	{
		PlayOneShotEvent("WaxCrawlerDeath", FxSoundCategory.Motion);
	}

	public override void Walk()
	{
		PlayOneShotEvent("WaxCrawlerWalk", FxSoundCategory.Motion);
	}
}
