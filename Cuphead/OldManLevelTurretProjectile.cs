using System.Collections;
using UnityEngine;

public class OldManLevelTurretProjectile : BasicProjectile
{
	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private float sparkleSpawnDelay;

	[SerializeField]
	private MinMax sparkleAngleShiftRange;

	[SerializeField]
	private MinMax sparkleDistanceRange;

	private float sparkleAngle;

	protected override void Start()
	{
		base.Start();
		rend.flipX = Rand.Bool();
		base.animator.Play("Projectile", 0, Random.Range(0f, 1f));
		StartCoroutine(spawn_sparkles_cr());
	}

	private IEnumerator spawn_sparkles_cr()
	{
		sparkleAngle = Random.Range(0, 360);
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, sparkleSpawnDelay);
			((OldManLevel)Level.Current).CreateFX(base.transform.position + (Vector3)MathUtils.AngleToDirection(sparkleAngle) * sparkleDistanceRange.RandomFloat(), isSparkle: true, base.CanParry);
			sparkleAngle = (sparkleAngle + sparkleAngleShiftRange.RandomFloat()) % 360f;
			yield return null;
		}
	}

	protected override void Move()
	{
		if (Speed == 0f)
		{
		}
		base.transform.position += base.transform.up * Speed * CupheadTime.FixedDelta;
	}
}
