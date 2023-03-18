using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.RangedBoomerang.Audio;

public class RangedBoomerangAudio : EntityAudio
{
	private const string WalkRightFootEventKey = "RangedBoomerangFootStepRight";

	private const string WalkLeftFootEventKey = "RangedBoomerangFootStepLeft";

	private const string DeathEventKey = "RangedBoomerangDeath";

	private const string ThrowEventKey = "RangedBoomerangThrow";

	private const string GrabEventKey = "RangedBoomerangGrab";

	private EventInstance _attackEventInstance;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Owner.SpriteRenderer.isVisible || Owner.Status.Dead)
		{
			StopAll();
		}
	}

	public void PlayLeftFootWalk()
	{
		if (Owner.SpriteRenderer.isVisible && !Owner.Status.Dead)
		{
			PlayOneShotEvent("RangedBoomerangFootStepLeft", FxSoundCategory.Motion);
		}
	}

	public void PlayRightFootWalk()
	{
		if (Owner.SpriteRenderer.isVisible && !Owner.Status.Dead)
		{
			PlayOneShotEvent("RangedBoomerangFootStepRight", FxSoundCategory.Motion);
		}
	}

	public virtual void PlayThrow()
	{
		PlayEvent(ref _attackEventInstance, "RangedBoomerangThrow");
	}

	public virtual void StopThrow()
	{
		StopEvent(ref _attackEventInstance);
	}

	public virtual void PlayGrab()
	{
		if (Owner.SpriteRenderer.isVisible && !Owner.Status.Dead)
		{
			PlayOneShotEvent("RangedBoomerangGrab", FxSoundCategory.Attack);
		}
	}

	public virtual void PlayDeath()
	{
		PlayOneShotEvent("RangedBoomerangDeath", FxSoundCategory.Damage);
	}

	public void StopAll()
	{
	}
}
