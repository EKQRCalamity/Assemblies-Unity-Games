using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Jumper.Audio;

public class JumperAudio : EntityAudio
{
	private const string JumpEventKey = "JumperJump";

	private const string FallEventKey = "JumperFall";

	private const string DeathEventKey = "JumperDeath";

	private EventInstance _jumpEventInstance;

	public void PlayJump()
	{
		PlayEvent(ref _jumpEventInstance, "JumperJump");
	}

	public void StopJump()
	{
		StopEvent(ref _jumpEventInstance);
	}

	public void PlayFall()
	{
		StopJump();
		PlayOneShotEvent("JumperFall", FxSoundCategory.Motion);
	}

	public void PlayDeath()
	{
		StopJump();
		PlayOneShotEvent("JumperDeath", FxSoundCategory.Damage);
	}
}
