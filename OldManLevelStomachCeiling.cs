using UnityEngine;

public class OldManLevelStomachCeiling : MonoBehaviour
{
	private const float MAX_FRAME_TIME = 1f / 6f;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private Sprite[] sprites;

	[SerializeField]
	private OldManLevelGnomeLeader leader;

	private int currentPosition;

	private int lastPosition;

	private float timeSinceChangeFrame;

	private int offset;

	private void Update()
	{
		currentPosition = (int)(Mathf.Clamp(leader.GetPosition(), 0f, 0.99f) * (float)sprites.Length);
		timeSinceChangeFrame += CupheadTime.Delta;
		if (lastPosition != currentPosition)
		{
			lastPosition = currentPosition;
			timeSinceChangeFrame = 0f;
		}
		offset = (int)(timeSinceChangeFrame / (1f / 6f)) % 2 * (int)(0f - Mathf.Sign(leader.GetPosition() - 0.5f));
		rend.sprite = sprites[currentPosition + offset];
	}
}
