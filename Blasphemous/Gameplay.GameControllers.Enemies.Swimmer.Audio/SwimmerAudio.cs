using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Swimmer.Audio;

public class SwimmerAudio : EntityAudio
{
	private const string JumpEventKey = "UndergroundSwimmerJump";

	private const string DeathEventKey = "UndergroundSwimmerDeath";

	private const string LandingEventKey = "UndergroundSwimmerLanding";

	private const string SwimEventKey = "UndergroundSwimmerSwim";

	private EventInstance _swimAudioInstance;

	public Swimmer Swimmer { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Swimmer = (Swimmer)Owner;
		Swimmer.OnDeath += OnDeath;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Swimmer.Behaviour.IsSwimming)
		{
			PlaySwim();
		}
		else
		{
			StopSwim();
		}
	}

	private void PlaySwim()
	{
		PlayEvent(ref _swimAudioInstance, "UndergroundSwimmerSwim");
		UpdateEvent(ref _swimAudioInstance);
	}

	private void StopSwim()
	{
		StopEvent(ref _swimAudioInstance);
	}

	public void PlayJump()
	{
		if ((bool)Swimmer && Swimmer.Behaviour.IsTriggerAttack)
		{
			PlayOneShotEvent("UndergroundSwimmerJump", FxSoundCategory.Motion);
		}
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("UndergroundSwimmerDeath", FxSoundCategory.Damage);
	}

	public void PlayLanding()
	{
		PlayOneShotEvent("UndergroundSwimmerLanding", FxSoundCategory.Motion);
	}

	private void OnDeath()
	{
		StopSwim();
		Swimmer.OnDeath -= OnDeath;
	}

	private void OnDestroy()
	{
		StopSwim();
	}
}
