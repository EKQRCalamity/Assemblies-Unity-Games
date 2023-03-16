using System.Collections;
using UnityEngine;

public class SaltbakerLevelPhaseOneProjectile : BasicProjectile
{
	[SerializeField]
	protected SpriteRenderer shadow;

	[SerializeField]
	protected MinMax shadowScaleHeightRange = new MinMax(100f, 500f);

	[SerializeField]
	protected Effect sparkEffect;

	[SerializeField]
	private float sparkSpawnDelay = 0.15f;

	[SerializeField]
	private MinMax sparkAngleShiftRange = new MinMax(60f, 300f);

	[SerializeField]
	private MinMax sparkDistanceRange = new MinMax(0f, 20f);

	private float sparkAngle;

	protected bool createSparks = true;

	protected override void Start()
	{
		base.Start();
		createSparks = true;
		StartCoroutine(spawn_sparkles_cr());
	}

	protected virtual bool SparksFollow()
	{
		return false;
	}

	private IEnumerator spawn_sparkles_cr()
	{
		sparkAngle = Random.Range(0, 360);
		while (createSparks)
		{
			yield return CupheadTime.WaitForSeconds(this, sparkSpawnDelay);
			int count = 1;
			if (sparkSpawnDelay < (float)CupheadTime.Delta)
			{
				count = (int)((float)CupheadTime.Delta / sparkSpawnDelay);
			}
			for (int i = 0; i < count; i++)
			{
				Effect effect = sparkEffect.Create(base.transform.position + (Vector3)MathUtils.AngleToDirection(sparkAngle) * sparkDistanceRange.RandomFloat());
				if (SparksFollow())
				{
					effect.transform.parent = base.transform;
				}
				sparkAngle = (sparkAngle + sparkAngleShiftRange.RandomFloat()) % 360f;
			}
		}
	}

	protected void HandleShadow(float heightOffset, float shadowPosOffset)
	{
		shadow.transform.position = new Vector3(base.transform.position.x, (float)Level.Current.Ground + shadowPosOffset);
		float t = Mathf.InverseLerp(shadowScaleHeightRange.max, shadowScaleHeightRange.min, base.transform.position.y - heightOffset - (float)Level.Current.Ground);
		shadow.transform.eulerAngles = Vector3.zero;
		shadow.transform.localScale = Vector3.Lerp(new Vector3(0.25f, 0.25f), new Vector3(1f, 1f), t);
		shadow.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.25f, 1f, t));
	}
}
