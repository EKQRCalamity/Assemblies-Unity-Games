using Gameplay.GameControllers.Bosses.Isidora;
using UnityEngine;

public class IsidoraAnimationEventsController : MonoBehaviour
{
	[SerializeField]
	public IsidoraBehaviour isidoraBehaviour;

	[SerializeField]
	private bool listenEvents;

	public void Animation_OnMeleeAttackStarts()
	{
		if (listenEvents)
		{
			isidoraBehaviour.OnMeleeAttackStarts();
		}
	}

	public void Animation_OnMeleeAttackFinished()
	{
		if (listenEvents)
		{
			isidoraBehaviour.OnMeleeAttackFinished();
		}
	}

	public void DoActivateCollisions(bool act)
	{
		if (listenEvents)
		{
			isidoraBehaviour.DoActivateCollisions(act);
		}
	}

	public void Animation_SetWeapon(IsidoraBehaviour.ISIDORA_WEAPONS weaponToUse)
	{
		if (listenEvents)
		{
			isidoraBehaviour.SetWeapon(weaponToUse);
		}
	}

	public void Animation_CheckFlagAndResetSpeed()
	{
		if (listenEvents)
		{
			isidoraBehaviour.Isidora.AnimatorInyector.CheckFlagAnimationSpeed();
		}
	}

	public void Animation_PlayOneShot(string eventId)
	{
		if (listenEvents)
		{
			isidoraBehaviour.Isidora.Audio.PlayOneShot_AUDIO(eventId);
		}
	}

	public void Animation_PlayAudio(string eventId)
	{
		if (listenEvents)
		{
			isidoraBehaviour.Isidora.Audio.Play_AUDIO(eventId);
		}
	}

	public void Animation_PlayRisingScytheSlashAudio()
	{
		if (listenEvents)
		{
			isidoraBehaviour.Isidora.Audio.PlayRisingScytheSlashAudio();
		}
	}

	public void Animation_PlayRisingScytheAnticipationLoopAudio()
	{
		if (listenEvents)
		{
			isidoraBehaviour.Isidora.Audio.PlayRisingScytheAnticipationLoopAudio();
		}
	}

	public void Animation_StopAudio(string eventId)
	{
		if (listenEvents)
		{
			isidoraBehaviour.Isidora.Audio.Stop_AUDIO(eventId);
		}
	}

	public void Animation_StopAllAudios()
	{
		if (listenEvents)
		{
			isidoraBehaviour.Isidora.Audio.StopAll();
		}
	}

	public void Animation_FlipCollider()
	{
		if (listenEvents)
		{
			isidoraBehaviour.FlipCurrentWeaponCollider();
		}
	}

	public void Animation_SpawnOrb()
	{
		if (listenEvents)
		{
			isidoraBehaviour.SpawnOrb();
		}
	}
}
