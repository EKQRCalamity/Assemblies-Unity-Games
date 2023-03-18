using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.ShieldMaiden.Audio;

public class ShieldMaidenAudio : EntityAudio
{
	private const string FootstepEventKey = "ShieldMaidenFootstep";

	private const string DeathEventKey = "ShieldMaidenDeath";

	private const string AttackEventKey = "ShieldMaidenAttack";

	private const string HitShieldEventKey = "ShieldMaidenHitShield";

	private EventInstance _attackEventInstance;

	protected override void OnStart()
	{
		base.OnStart();
	}

	private void Shield_OnShieldHitDetected()
	{
		PlayHitShield();
	}

	public void PlayHitShield()
	{
		PlayOneShotEvent("ShieldMaidenHitShield", FxSoundCategory.Damage);
	}

	public void PlayAttack()
	{
		PlayEvent(ref _attackEventInstance, "ShieldMaidenAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void PlayDeath()
	{
		StopAttack();
		PlayOneShotEvent("ShieldMaidenDeath", FxSoundCategory.Damage);
	}

	public void PlayFootstep()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("ShieldMaidenFootstep", FxSoundCategory.Motion);
		}
	}
}
