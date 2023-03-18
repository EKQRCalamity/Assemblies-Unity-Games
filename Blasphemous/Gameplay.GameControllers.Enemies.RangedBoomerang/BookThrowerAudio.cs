using FMOD.Studio;
using Gameplay.GameControllers.Enemies.RangedBoomerang.Audio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.RangedBoomerang;

public class BookThrowerAudio : RangedBoomerangAudio
{
	private const string DeathEventKey = "BookThrowerDeath";

	private const string ThrowEventKey = "BookThrow";

	private const string GrabEventKey = "BookGrab";

	private EventInstance _attackEventInstance;

	public override void PlayThrow()
	{
		PlayEvent(ref _attackEventInstance, "BookThrow");
	}

	public override void StopThrow()
	{
		StopEvent(ref _attackEventInstance);
	}

	public override void PlayGrab()
	{
		if (Owner.SpriteRenderer.isVisible && !Owner.Status.Dead)
		{
			PlayOneShotEvent("BookGrab", FxSoundCategory.Attack);
		}
	}

	public override void PlayDeath()
	{
		PlayOneShotEvent("BookThrowerDeath", FxSoundCategory.Damage);
	}
}
