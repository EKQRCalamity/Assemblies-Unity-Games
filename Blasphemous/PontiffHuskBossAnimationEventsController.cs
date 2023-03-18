using Gameplay.GameControllers.Bosses.PontiffHusk;
using UnityEngine;

public class PontiffHuskBossAnimationEventsController : MonoBehaviour
{
	[SerializeField]
	public PontiffHuskBossBehaviour pontiffHuskBossBehaviour;

	[SerializeField]
	private bool listenEvents;

	public void Animation_DoActivateCollisions(bool act)
	{
		if (listenEvents)
		{
			pontiffHuskBossBehaviour.DoActivateCollisions(act);
		}
	}

	public void Animation_PlayOneShot(string eventId)
	{
		if (listenEvents)
		{
			pontiffHuskBossBehaviour.PontiffHuskBoss.Audio.PlayOneShot_AUDIO(eventId);
		}
	}

	public void Animation_PlayAudio(string eventId)
	{
		if (listenEvents)
		{
			pontiffHuskBossBehaviour.PontiffHuskBoss.Audio.Play_AUDIO(eventId);
		}
	}

	public void Animation_StopAudio(string eventId)
	{
		if (listenEvents)
		{
			pontiffHuskBossBehaviour.PontiffHuskBoss.Audio.Stop_AUDIO(eventId);
		}
	}

	public void Animation_FlipOrientation()
	{
		if (listenEvents)
		{
			pontiffHuskBossBehaviour.FlipOrientation();
		}
	}

	public void Animation_DoActivateGuard(bool act)
	{
		if (listenEvents)
		{
			pontiffHuskBossBehaviour.DoActivateGuard(act);
		}
	}
}
