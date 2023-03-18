using FMODUnity;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Audio;

public class DemakeEnemyAudio : MonoBehaviour
{
	[SerializeField]
	[EventRef]
	private string attackEvent;

	[SerializeField]
	[EventRef]
	private string jumpEvent;

	[SerializeField]
	[EventRef]
	private string deadEvent;

	[SerializeField]
	[EventRef]
	private string warningEvent;

	[SerializeField]
	[EventRef]
	private string gruntEvent;

	[SerializeField]
	[EventRef]
	private string effectsEvent;

	[SerializeField]
	[EventRef]
	private string stepEvent;

	[SerializeField]
	private SpriteRenderer ownerSprite;

	[SerializeField]
	private bool muteAudioOutsideScreen;

	private void Start()
	{
		MuteDefaultAudio();
	}

	public void PlayAttackDemake()
	{
		PlaySfx(attackEvent);
	}

	public void PLayJumpDemake()
	{
		PlaySfx(jumpEvent);
	}

	public void PlayWarningDemake()
	{
		PlaySfx(warningEvent);
	}

	public void PlayDeadDemake()
	{
		PlaySfx(deadEvent);
	}

	public void PlayGruntDemake()
	{
		PlaySfx(gruntEvent);
	}

	public void PlayEffectsDemake()
	{
		PlaySfx(effectsEvent);
	}

	public void PlayLeftLeg()
	{
		PlaySfx(stepEvent);
	}

	private void MuteDefaultAudio(bool mute = true)
	{
		Entity componentInParent = GetComponentInParent<Entity>();
		if (!(componentInParent == null))
		{
			EntityAudio componentInChildren = GetComponentInParent<Entity>().GetComponentInChildren<EntityAudio>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Mute = mute;
			}
		}
	}

	private void PlaySfx(string eventId)
	{
		if (!string.IsNullOrEmpty(eventId) && (!muteAudioOutsideScreen || !(ownerSprite != null) || ownerSprite.isVisible))
		{
			Core.Audio.PlaySfx(eventId);
		}
	}
}
