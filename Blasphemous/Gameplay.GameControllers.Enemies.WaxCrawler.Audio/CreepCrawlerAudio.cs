using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.WaxCrawler.Audio;

public abstract class CreepCrawlerAudio : EntityAudio
{
	protected bool Spawned;

	protected void OnEnable()
	{
		if (!Spawned)
		{
			Spawned = true;
		}
	}

	public abstract void Appear();

	public abstract void Disappear();

	public abstract void Hurt();

	public abstract void Death();

	public abstract void Walk();
}
