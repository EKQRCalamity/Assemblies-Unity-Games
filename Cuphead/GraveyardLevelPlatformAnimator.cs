using UnityEngine;

public class GraveyardLevelPlatformAnimator : AbstractMonoBehaviour
{
	[SerializeField]
	private SpriteRenderer strikeSpark;

	[SerializeField]
	private SpriteRenderer[] trailSparks;

	private float[] xPositionBuffer = new float[8];

	private void Start()
	{
		for (int i = 0; i < trailSparks.Length; i++)
		{
			trailSparks[i].transform.parent = null;
		}
		for (int j = 0; j < xPositionBuffer.Length; j++)
		{
			xPositionBuffer[j] = base.transform.position.x;
		}
	}

	private void LateUpdate()
	{
		strikeSpark.transform.position = new Vector3(strikeSpark.transform.position.x, Level.Current.Ground);
		for (int num = xPositionBuffer.Length - 1; num > 0; num--)
		{
			xPositionBuffer[num] = xPositionBuffer[num - 1];
		}
		xPositionBuffer[0] = base.transform.position.x;
		for (int i = 0; i < 3; i++)
		{
			trailSparks[i].transform.position = new Vector3(xPositionBuffer[i * 2 + 1], Level.Current.Ground);
			trailSparks[i].transform.localScale = new Vector3(Mathf.Sign(xPositionBuffer[1] - xPositionBuffer[0]), 1f);
		}
	}
}
