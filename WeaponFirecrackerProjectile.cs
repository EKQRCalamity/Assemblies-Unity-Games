using System.Collections;
using UnityEngine;

public class WeaponFirecrackerProjectile : BasicProjectile
{
	[SerializeField]
	private WeaponFirecrackerProjectile projectile;

	public float bulletLife;

	public float explosionSize;

	public float explosionDuration;

	public float explosionRadiusSize;

	public float explosionAngle;

	private Transform parent;

	private LevelPlayerController player;

	private float parentScaleX;

	private bool brokeOffFromParent;

	private Vector3 moveVector;

	private float distanceTraveled;

	public Collider2D collider;

	public new Animator animator;

	private bool hitEnemy;

	protected override void Update()
	{
		base.Update();
		if (parent != null && !brokeOffFromParent && parent.transform.localScale.x != (float)player.motor.LookDirection.x)
		{
			base.transform.SetParent(null, worldPositionStays: true);
			brokeOffFromParent = true;
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemy(hit, phase);
		hitEnemy = true;
		animator.Play("Hit");
		collider.enabled = false;
	}

	public void SetupFirecracker(Transform parent, LevelPlayerController player, bool isTypeB)
	{
		base.transform.SetParent(parent, worldPositionStays: true);
		this.parent = parent;
		this.player = player;
		distanceTraveled = 0f;
		if (isTypeB)
		{
			StartCoroutine(bullet_life_B_cr());
		}
		else
		{
			StartCoroutine(bullet_life_cr());
		}
	}

	public void StillBullet()
	{
		move = false;
		StartCoroutine(bullet_slice_life_cr());
	}

	private IEnumerator bullet_life_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, bulletLife);
		move = false;
		collider.enabled = true;
		base.transform.SetScale(explosionSize, explosionSize);
		yield return CupheadTime.WaitForSeconds(this, explosionDuration);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator bullet_life_B_cr()
	{
		float explodeDistance = bulletLife * Speed;
		while (distanceTraveled < explodeDistance)
		{
			yield return null;
		}
		move = false;
		WeaponFirecrackerProjectile slice = Object.Instantiate(projectile);
		Vector3 dir = MathUtils.AngleToDirection(explosionAngle);
		slice.transform.position = base.transform.position + dir * explosionRadiusSize;
		slice.collider.enabled = true;
		slice.collider.transform.SetScale(explosionSize, explosionSize);
		slice.DamageRate = DamageRate;
		slice.StillBullet();
		slice.gameObject.name = "FirecrackerExplosion";
		slice.transform.eulerAngles = new Vector3(0f, 0f, explosionAngle);
		Object.Destroy(base.gameObject);
		yield return null;
	}

	private IEnumerator bullet_slice_life_cr()
	{
		hitEnemy = false;
		animator.Play("Die");
		float t = 0f;
		yield return CupheadTime.WaitForSeconds(this, explosionDuration);
		if (hitEnemy)
		{
			SpriteRenderer sprite = GetComponent<SpriteRenderer>();
			while (sprite.enabled)
			{
				yield return null;
			}
		}
		Object.Destroy(base.gameObject);
		yield return null;
	}

	protected override void Move()
	{
		moveVector = base.transform.right * Speed * CupheadTime.FixedDelta - new Vector3(0f, _accumulativeGravity * CupheadTime.FixedDelta, 0f);
		base.transform.position += moveVector;
		distanceTraveled += moveVector.magnitude;
	}
}
