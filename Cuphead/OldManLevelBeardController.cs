using UnityEngine;

public class OldManLevelBeardController : MonoBehaviour
{
	private const float RUFFLE_FRAME_TIME_NORMALIZED = 1f / 14f;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private Sprite[] sprites;

	[SerializeField]
	private Animator[] ruffles;

	[SerializeField]
	private int[] ruffleStartFrames;

	private bool[] ruffleCued = new bool[10];

	private float frameTimer;

	private int frameNum;

	public void CueRuffle(int which)
	{
		if (ruffles[which] != null && ruffles[which].GetCurrentAnimatorStateInfo(0).IsName("None"))
		{
			ruffleCued[which] = true;
		}
	}

	private void FixedUpdate()
	{
		frameTimer += CupheadTime.FixedDelta;
		if (frameTimer > 1f / 24f)
		{
			frameTimer -= 1f / 24f;
			Step();
		}
	}

	private void Step()
	{
		frameNum = (frameNum + 1) % 6;
		rend.sprite = sprites[frameNum / 2];
	}

	private void LateUpdate()
	{
		for (int i = 0; i < ruffleCued.Length; i++)
		{
			if (ruffleCued[i])
			{
				int num = ruffleStartFrames[i] - frameNum;
				if (num == 0 || num == 1)
				{
					float num2 = frameTimer * 24f;
					float normalizedTime = ((float)num + num2) * (1f / 14f);
					ruffles[i].Play("Ruffle", 0, normalizedTime);
					ruffles[i].Update(0f);
					ruffleCued[i] = false;
				}
			}
		}
	}
}
