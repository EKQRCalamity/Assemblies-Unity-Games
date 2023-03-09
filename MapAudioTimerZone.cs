using UnityEngine;

public class MapAudioTimerZone : AbstractCollidableObject
{
	[SerializeField]
	private string audioKey;

	[SerializeField]
	private Rangef audioDelayRange;

	private int playerCount;

	private float elapsedTime;

	private float waitTime;

	private void Start()
	{
		waitTime = Random.Range(audioDelayRange.minimum, audioDelayRange.maximum);
	}

	private void Update()
	{
		if (playerCount > 0)
		{
			elapsedTime += CupheadTime.Delta;
			if (elapsedTime > waitTime)
			{
				AudioManager.Play(audioKey);
				elapsedTime = 0f;
				waitTime = Random.Range(audioDelayRange.minimum, audioDelayRange.maximum);
			}
		}
	}

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		base.OnCollision(hit, phase);
		if (hit.CompareTag("Player_Map"))
		{
			switch (phase)
			{
			case CollisionPhase.Enter:
				playerCount++;
				break;
			case CollisionPhase.Exit:
				playerCount--;
				break;
			}
		}
	}
}
