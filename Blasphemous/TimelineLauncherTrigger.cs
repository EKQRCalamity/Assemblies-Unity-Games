using UnityEngine;
using UnityEngine.Playables;

public class TimelineLauncherTrigger : MonoBehaviour
{
	public LayerMask collisionMask;

	public PlayableDirector timelinePlayableDirector;

	public bool repeat;

	private bool alreadyPlayed;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if ((!alreadyPlayed || repeat) && (int)collisionMask == ((int)collisionMask | (1 << collision.gameObject.layer)) && (bool)timelinePlayableDirector)
		{
			timelinePlayableDirector.Play();
			alreadyPlayed = true;
		}
	}
}
