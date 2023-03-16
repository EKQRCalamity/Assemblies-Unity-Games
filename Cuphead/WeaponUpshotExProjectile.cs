using UnityEngine;

public class WeaponUpshotExProjectile : AbstractProjectile
{
	private float timeUntilUnfreeze;

	private float totalDamage;

	private float angle;

	private float time;

	private float radius;

	private Vector3 startPos;

	public float rotateDir;

	private Vector3 startScale;

	private Vector3 endScale;

	private Vector2[] trailPositions;

	private int currentPositionIndex;

	[SerializeField]
	private SpriteRenderer trail1;

	[SerializeField]
	private SpriteRenderer trail2;

	private const int trailFrameDelay = 3;

	[SerializeField]
	private Effect hitFXPrefab;

	protected override float DestroyLifetime => 4f;

	protected override bool DestroyedAfterLeavingScreen => false;

	protected override void OnDieDistance()
	{
	}

	protected override void Start()
	{
		base.Start();
		damageDealer.SetDamage(WeaponProperties.LevelWeaponUpshot.Ex.damage);
		damageDealer.SetRate(WeaponProperties.LevelWeaponUpshot.Ex.damageRate);
		damageDealer.isDLCWeapon = true;
		angle = MathUtils.DirectionToAngle(base.transform.right);
		base.transform.position += base.transform.right * 120f;
		base.transform.localScale = new Vector3((!(base.transform.eulerAngles.z > 90f) || !(base.transform.eulerAngles.z < 270f)) ? 1 : (-1), 1f);
		endScale = base.transform.localScale;
		base.transform.localScale *= 0.5f;
		startScale = base.transform.localScale;
		base.transform.eulerAngles = Vector3.zero;
		startPos = base.transform.position;
		base.animator.Play("EX", 0, Random.Range(0f, 1f));
		trailPositions = new Vector2[6];
		for (int i = 0; i < trailPositions.Length; i++)
		{
			ref Vector2 reference = ref trailPositions[i];
			reference = base.transform.position;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.dead)
		{
			if (timeUntilUnfreeze > 0f)
			{
				timeUntilUnfreeze -= CupheadTime.FixedDelta;
				return;
			}
			time += CupheadTime.FixedDelta;
			angle += Mathf.Lerp(WeaponProperties.LevelWeaponUpshot.Ex.minRotationSpeed, WeaponProperties.LevelWeaponUpshot.Ex.maxRotationSpeed, time / WeaponProperties.LevelWeaponUpshot.Ex.rotationRampTime) * CupheadTime.FixedDelta * rotateDir;
			radius += Mathf.Lerp(WeaponProperties.LevelWeaponUpshot.Ex.minRadiusSpeed, WeaponProperties.LevelWeaponUpshot.Ex.maxRadiusSpeed, time / WeaponProperties.LevelWeaponUpshot.Ex.radiusRampTime) * CupheadTime.FixedDelta;
			base.transform.position = startPos + (Vector3)MathUtils.AngleToDirection(angle) * radius;
			float num = Mathf.Round(time * 24f) / 24f;
			base.transform.localScale = Vector3.Lerp(startScale, endScale, num * 5f);
			num *= 0.2f;
			trail1.color = new Color(1f, 1f, 1f, 0.5f - num);
			trail2.color = new Color(1f, 1f, 1f, 0.25f - num);
			UpdateTrails();
		}
	}

	private void UpdateTrails()
	{
		int num = currentPositionIndex - 2;
		if (num < 0)
		{
			num += trailPositions.Length;
		}
		int num2 = currentPositionIndex - 5;
		if (num2 < 0)
		{
			num2 += trailPositions.Length;
		}
		trail1.transform.position = trailPositions[num];
		trail2.transform.position = trailPositions[num2];
		currentPositionIndex = (currentPositionIndex + 1) % trailPositions.Length;
		ref Vector2 reference = ref trailPositions[currentPositionIndex];
		reference = base.transform.position;
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemy(hit, phase);
		float num = damageDealer.DealDamage(hit);
		totalDamage += num;
		if (totalDamage > WeaponProperties.LevelWeaponUpshot.Ex.maxDamage)
		{
			Die();
		}
		if (num > 0f)
		{
			hitFXPrefab.Create(Vector3.Lerp(base.transform.position, hit.transform.position, 0.5f) + (Vector3)Random.insideUnitCircle * 20f);
			AudioManager.Play("player_ex_impact_hit");
			emitAudioFromObject.Add("player_ex_impact_hit");
			timeUntilUnfreeze = WeaponProperties.LevelWeaponUpshot.Ex.freezeTime;
		}
	}

	protected override void Die()
	{
		base.Die();
		GetComponent<Collider2D>().enabled = false;
		base.animator.Play("Die");
	}
}
