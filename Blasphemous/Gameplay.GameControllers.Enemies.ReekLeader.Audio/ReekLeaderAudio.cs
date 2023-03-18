using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.ReekLeader.Audio;

public class ReekLeaderAudio : EntityAudio
{
	private const string IdleEventKey = "LeperLeaderIdle";

	private const string CallEventKey = "LeperLeaderCall";

	private const string DeathEventKey = "LeperLeaderDeath";

	private EventInstance _idleEventInstance;

	private EventInstance _callEventInstance;

	public void PlayIdle()
	{
		if (Owner.SpriteRenderer.isVisible && !_idleEventInstance.isValid())
		{
			_idleEventInstance = base.AudioManager.CreateCatalogEvent("LeperLeaderIdle");
			_idleEventInstance.start();
		}
	}

	public void StopIdle()
	{
		if (_idleEventInstance.isValid())
		{
			_idleEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_idleEventInstance.release();
			_idleEventInstance = default(EventInstance);
		}
	}

	public void PlayCall()
	{
		if (Owner.SpriteRenderer.isVisible && !_callEventInstance.isValid())
		{
			_callEventInstance = base.AudioManager.CreateCatalogEvent("LeperLeaderCall");
			_callEventInstance.start();
		}
	}

	public void StopCall()
	{
		if (_callEventInstance.isValid())
		{
			_callEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_callEventInstance.release();
			_callEventInstance = default(EventInstance);
		}
	}

	public void PlayDeath()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("LeperLeaderDeath", FxSoundCategory.Damage);
		}
	}
}
