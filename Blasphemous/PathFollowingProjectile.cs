using FMODUnity;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

public class PathFollowingProjectile : Weapon
{
	private Animator _animator;

	public Enemy AttackingEntity;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	public string hitSound;

	public DamageArea.DamageType damageType;

	public DamageArea.DamageElement damageElement;

	public float force;

	public float damage;

	public bool unavoidable;

	public bool forceGuardslide;

	public LayerMask BlockLayerMask;

	private Hit _hit;

	public bool leaveSparks = true;

	public GameObject sparksPrefab;

	public Vector2 collisionPoint;

	public float collisionRadius;

	public ContactFilter2D filter;

	private RaycastHit2D[] results;

	private float instantiationTimer;

	public float secondsBetweenInstances = 0.5f;

	public bool destroyOnSplineFinish = true;

	private SplineFollowingProjectile _projectile;

	public AttackArea AttackArea { get; private set; }

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_animator = GetComponentInChildren<Animator>();
		_animator.SetBool("SPIN", value: true);
		AttackArea = GetComponentInChildren<AttackArea>();
		_projectile = GetComponent<SplineFollowingProjectile>();
		_projectile.OnSplineCompletedEvent += _projectile_OnSplineCompletedEvent;
		CreateHit();
	}

	private void CreateHit()
	{
		_hit = new Hit
		{
			DamageAmount = damage,
			DamageType = damageType,
			DamageElement = damageElement,
			Force = force,
			HitSoundId = hitSound,
			Unnavoidable = unavoidable,
			forceGuardslide = forceGuardslide
		};
	}

	private void _projectile_OnSplineCompletedEvent(SplineFollowingProjectile obj)
	{
		if (destroyOnSplineFinish)
		{
			Destroy();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		results = new RaycastHit2D[4];
		AttackArea.OnEnter += AttackAreaOnEnter;
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		_animator.SetBool("SPIN", value: true);
	}

	public new void SetHit(Hit hit)
	{
		_hit.AttackingEntity = hit.AttackingEntity;
	}

	public void SetDamage(float damage)
	{
		_hit.DamageAmount = damage;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (leaveSparks)
		{
			CheckCollision();
		}
	}

	private void CheckCollision()
	{
		if (instantiationTimer > 0f)
		{
			instantiationTimer -= Time.deltaTime;
			return;
		}
		Vector2 pos = (Vector2)base.transform.position + collisionPoint;
		CheckCollisionInDirection(pos, Vector2.down);
		CheckCollisionInDirection(pos, Vector2.left);
		CheckCollisionInDirection(pos, Vector2.up);
		CheckCollisionInDirection(pos, Vector2.right);
	}

	private void CheckCollisionInDirection(Vector2 pos, Vector2 dir)
	{
		if (Physics2D.Raycast(pos, dir, filter, results, collisionRadius) > 0)
		{
			Vector2 point = results[0].point;
			instantiationTimer = secondsBetweenInstances;
			GameObject gameObject = Object.Instantiate(sparksPrefab, point, Quaternion.identity);
			gameObject.transform.up = -dir;
		}
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam collider2DParam)
	{
		GameObject gameObject = collider2DParam.Collider2DArg.gameObject;
		Attack(_hit);
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void SetOwner(Enemy enemy)
	{
		WeaponOwner = (AttackingEntity = enemy);
	}

	private void OnDestroy()
	{
		AttackArea.OnEnter -= AttackAreaOnEnter;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(base.transform.position + (Vector3)collisionPoint, base.transform.position + (Vector3)collisionPoint + Vector3.down * collisionRadius);
		Gizmos.DrawLine(base.transform.position + (Vector3)collisionPoint, base.transform.position + (Vector3)collisionPoint + Vector3.left * collisionRadius);
		Gizmos.DrawLine(base.transform.position + (Vector3)collisionPoint, base.transform.position + (Vector3)collisionPoint + Vector3.up * collisionRadius);
		Gizmos.DrawLine(base.transform.position + (Vector3)collisionPoint, base.transform.position + (Vector3)collisionPoint + Vector3.right * collisionRadius);
	}
}
