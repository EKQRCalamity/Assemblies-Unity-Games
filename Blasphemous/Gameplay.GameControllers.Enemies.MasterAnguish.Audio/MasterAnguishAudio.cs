using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.MasterAnguish.Audio;

public class MasterAnguishAudio : EntityAudio
{
	private const string MergeEventKey = "MasterAnguishMerge";

	private const string DivideEventKey = "MasterAnguishDivide";

	private const string PreDeath = "MasterAnguishPreDeath";

	private const string Death = "MasterAnguishDeath";

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Owner.SpriteRenderer.isVisible || Owner.Status.Dead)
		{
			StopAll();
		}
	}

	public void PlayMerge()
	{
		PlayOneShotEvent("MasterAnguishMerge", FxSoundCategory.Motion);
	}

	public void PlayDivide()
	{
		PlayOneShotEvent("MasterAnguishDivide", FxSoundCategory.Motion);
	}

	public void PlayPreDeath()
	{
		PlayOneShotEvent("MasterAnguishPreDeath", FxSoundCategory.Motion);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("MasterAnguishDeath", FxSoundCategory.Motion);
	}

	public void StopAll()
	{
	}
}
