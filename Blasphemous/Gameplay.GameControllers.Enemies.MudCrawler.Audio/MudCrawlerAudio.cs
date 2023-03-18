using Gameplay.GameControllers.Enemies.WaxCrawler.Audio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.MudCrawler.Audio;

public class MudCrawlerAudio : CreepCrawlerAudio
{
	private const string EntityPrefix = "MudCrawler";

	private const string AppearingEventKey = "MudCrawlerAppearing";

	private const string WalkEventKey = "MudCrawlerWalk";

	private const string DisapearingEventKey = "MudCrawlerDisappearing";

	private const string DeathEventKey = "MudCrawlerDeath";

	public override void Appear()
	{
		if (Owner.Status.IsVisibleOnCamera)
		{
			PlayOneShotEvent("MudCrawlerAppearing", FxSoundCategory.Motion);
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
			PlayOneShotEvent("MudCrawlerDisappearing", FxSoundCategory.Motion);
		}
	}

	public override void Hurt()
	{
	}

	public override void Death()
	{
		PlayOneShotEvent("MudCrawlerDeath", FxSoundCategory.Motion);
	}

	public override void Walk()
	{
		PlayOneShotEvent("MudCrawlerWalk", FxSoundCategory.Motion);
	}
}
