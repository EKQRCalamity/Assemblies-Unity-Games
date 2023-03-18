using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.ChasingHead.Audio;

public class ChasingHeadAudio : EntityAudio
{
	public string FloatingEventKey;

	public string ExplodeEventKey;

	private EventInstance _explodingHeadFloating;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Chasing"))
		{
			if (Owner.SpriteRenderer.isVisible)
			{
				PlayFloating();
				UpdateFloating();
			}
			else
			{
				StopFloating();
			}
		}
		if (Owner.Status.Dead)
		{
			StopFloating();
		}
	}

	public void PlayFloating()
	{
		PlayEvent(ref _explodingHeadFloating, FloatingEventKey);
	}

	public void StopFloating()
	{
		StopEvent(ref _explodingHeadFloating);
	}

	public void UpdateFloating()
	{
		UpdateEvent(ref _explodingHeadFloating);
	}

	public void PlayExplode()
	{
		PlayOneShotEvent(ExplodeEventKey, FxSoundCategory.Motion);
	}

	private void OnDestroy()
	{
		StopFloating();
	}
}
