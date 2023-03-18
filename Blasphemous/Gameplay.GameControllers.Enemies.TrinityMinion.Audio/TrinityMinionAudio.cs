using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.TrinityMinion.Audio;

public class TrinityMinionAudio : EntityAudio
{
	private const string DeathEventKey = "TrinityMinionDeath";

	private const string FlyEventKey = "TrinityMinionFly";

	private EventInstance _attackEventInstance;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Owner.SpriteRenderer.isVisible || Owner.Status.Dead)
		{
			StopAll();
		}
	}

	public void PlayFlap()
	{
		if (Owner.SpriteRenderer.isVisible && !Owner.Status.Dead)
		{
			PlayOneShotEvent("TrinityMinionFly", FxSoundCategory.Motion);
		}
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("TrinityMinionDeath", FxSoundCategory.Damage);
	}

	public void StopAll()
	{
	}
}
