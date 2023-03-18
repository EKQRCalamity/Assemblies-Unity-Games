using Framework.FrameworkCore;
using Gameplay.GameControllers.Bosses.Amanecidas;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

public class AmanecidasAnimationEventsController : MonoBehaviour
{
	[SerializeField]
	public AmanecidasBehaviour amanecidasBehaviour;

	[SerializeField]
	private bool listenEvents;

	public void DoActivateCollisions(bool act)
	{
		if (listenEvents)
		{
			amanecidasBehaviour.DoActivateCollisions(act);
		}
	}

	public void AnimationEvent_SummonWeapon()
	{
		if (listenEvents)
		{
			amanecidasBehaviour.SummonWeapon();
		}
	}

	public void AnimationEvent_ShowWeaponIfBowLaudes()
	{
		if (listenEvents && amanecidasBehaviour.Amanecidas.IsLaudes && amanecidasBehaviour.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW)
		{
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
		}
	}

	public void AnimationEvent_HideWeaponIfBowLaudes()
	{
		if (listenEvents && amanecidasBehaviour.Amanecidas.IsLaudes && amanecidasBehaviour.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW)
		{
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
		}
	}

	public void OnTurnAnimationEnded()
	{
		if (listenEvents)
		{
			Amanecidas amanecidas = amanecidasBehaviour.Amanecidas;
			EntityOrientation orientation = ((amanecidas.Status.Orientation == EntityOrientation.Right || 1 == 0) ? EntityOrientation.Left : EntityOrientation.Right);
			amanecidas.SetOrientation(orientation);
		}
	}

	public void AnimationEvent_MeleeAttackStart()
	{
		if (listenEvents)
		{
			amanecidasBehaviour.OnMeleeAttackStarts();
		}
	}

	public void AnimationEvent_MeleeAttackFinished()
	{
		if (listenEvents)
		{
			amanecidasBehaviour.OnMeleeAttackFinished();
		}
	}

	public void AnimationEvent_PlayAnimationOneShot(string eventId, EntityAudio.FxSoundCategory category)
	{
		if (listenEvents)
		{
			amanecidasBehaviour.Amanecidas.Audio.PlayOneShot_AUDIO(eventId, category);
		}
	}

	public void AnimationEvent_PlayAnimationAudio(string eventId)
	{
		if (listenEvents)
		{
			amanecidasBehaviour.Amanecidas.Audio.Play_AUDIO(eventId);
		}
	}

	public void AnimationEvent_StopAnimationAudio(string eventId)
	{
		if (listenEvents)
		{
			amanecidasBehaviour.Amanecidas.Audio.Stop_AUDIO(eventId);
		}
	}
}
