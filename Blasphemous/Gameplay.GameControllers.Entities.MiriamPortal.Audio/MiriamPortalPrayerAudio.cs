using FMOD.Studio;
using FMODUnity;
using Gameplay.GameControllers.Entities.Audio;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.MiriamPortal.Audio;

public class MiriamPortalPrayerAudio : EntityAudio
{
	[FoldoutGroup("Audio Events", 0)]
	[SerializeField]
	[EventRef]
	private string AttackFx;

	[FoldoutGroup("Audio Events", 0)]
	[SerializeField]
	[EventRef]
	private string AppearFx;

	[FoldoutGroup("Audio Events", 0)]
	[SerializeField]
	[EventRef]
	private string FollowFx;

	[FoldoutGroup("Audio Events", 0)]
	[SerializeField]
	[EventRef]
	private string VanishFx;

	[FoldoutGroup("Audio Events", 0)]
	[SerializeField]
	[EventRef]
	private string TurnFx;

	private EventInstance followEvent;

	public void PlayAttack()
	{
		if (!AttackFx.IsNullOrWhitespace())
		{
			base.AudioManager.PlaySfx(AttackFx);
		}
	}

	public void PlayAppear()
	{
		if (!AppearFx.IsNullOrWhitespace())
		{
			base.AudioManager.PlaySfx(AppearFx);
		}
	}

	public void PlayFollow()
	{
		if (!FollowFx.IsNullOrWhitespace())
		{
			StopFollow();
			base.AudioManager.PlayEventNoCatalog(ref followEvent, FollowFx);
		}
	}

	public void StopFollow()
	{
		if (followEvent.isValid())
		{
			followEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			followEvent.release();
		}
	}

	public void PlayVanish()
	{
		if (!VanishFx.IsNullOrWhitespace())
		{
			base.AudioManager.PlaySfx(VanishFx);
		}
	}

	public void PlayTurn()
	{
		if (!VanishFx.IsNullOrWhitespace())
		{
			base.AudioManager.PlaySfx(TurnFx);
		}
	}
}
