using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.JarThrower.Audio;

public class JarThrowerAudio : EntityAudio
{
	private const string JarExplodingEventKey = "BotijoExplode";

	private const string ThrowJarEventKey = "BotijeraThrowBotijo";

	private const string JumpEventKey = "BotijeraJump";

	private const string LandingEventKey = "BotijeraLanding";

	private const string RunEventKey = "BotijeraRun";

	private const string FootstepEventKey = "BotijeraFootsteps";

	private const string DeathEventKey = "BotijeraDeath";

	public void PlayJarExploding()
	{
		PlayOneShotEvent("BotijoExplode", FxSoundCategory.Attack);
	}

	public void PlayThrowJar()
	{
		PlayOneShotEvent("BotijeraThrowBotijo", FxSoundCategory.Attack);
	}

	public void PlayJump()
	{
		PlayOneShotEvent("BotijeraJump", FxSoundCategory.Motion);
	}

	public void PlayLanding()
	{
		PlayOneShotEvent("BotijeraLanding", FxSoundCategory.Motion);
	}

	public void PlayRun()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BotijeraRun", FxSoundCategory.Motion);
		}
	}

	public void PlayWalk()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("BotijeraFootsteps", FxSoundCategory.Motion);
		}
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("BotijeraDeath", FxSoundCategory.Damage);
	}
}
