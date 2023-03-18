using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Menina;

public class IsabelAudio : MeninaAudio
{
	private const string IdleEventKey = "IsabelIdle";

	private const string DeathEventKey = "IsabelDeath";

	private const string AttackEventKey = "IsabelAttack";

	private const string FootstepsEventKey = "IsabelFootsteps";

	public override void PlayDeath()
	{
		PlayOneShotEvent("IsabelDeath", FxSoundCategory.Damage);
	}

	public override void PlayRightLeg()
	{
		PlayOneShotEvent("IsabelFootsteps", FxSoundCategory.Motion);
	}

	public override void PlayIdle()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("IsabelIdle", FxSoundCategory.Motion);
		}
	}

	public override void PlayAttack()
	{
		PlayOneShotEvent("IsabelAttack", FxSoundCategory.Attack);
	}
}
