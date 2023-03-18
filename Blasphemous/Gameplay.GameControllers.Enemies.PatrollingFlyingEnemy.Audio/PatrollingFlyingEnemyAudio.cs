using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy.Audio;

public class PatrollingFlyingEnemyAudio : EntityAudio
{
	[EventRef]
	public string DeathEventKey;

	[EventRef]
	public string FlyEventKey;

	private EventInstance _attackEventInstance;

	public void PlayFlap()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			Core.Audio.PlaySfx(FlyEventKey);
		}
	}

	public void PlayDeath()
	{
		Core.Audio.PlaySfx(DeathEventKey);
	}
}
