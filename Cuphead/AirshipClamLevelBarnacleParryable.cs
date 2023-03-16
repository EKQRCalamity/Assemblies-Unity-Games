using System.Collections;
using UnityEngine;

public class AirshipClamLevelBarnacleParryable : ParrySwitch
{
	public bool parried;

	private bool onPlayerCollisionDeath;

	private int direction;

	private Vector3 velocity;

	private LevelProperties.AirshipClam properties;

	private CircleCollider2D circleCollider;

	private DamageDealer damageDealer;

	[SerializeField]
	private Sprite parraybleBarnacleSprite;

	protected override void Awake()
	{
		parried = false;
		damageDealer = DamageDealer.NewEnemy();
		base.Awake();
	}

	private void Update()
	{
		damageDealer.Update();
	}

	public void InitBarnacle(int dir, LevelProperties.AirshipClam properties)
	{
		onPlayerCollisionDeath = true;
		this.properties = properties;
		GetComponent<SpriteRenderer>().sprite = parraybleBarnacleSprite;
		direction = dir;
		circleCollider = GetComponent<CircleCollider2D>();
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		velocity = new Vector3(properties.CurrentState.barnacles.initialArcMovementX * (float)direction, properties.CurrentState.barnacles.initialArcMovementY, 0f);
		while (true)
		{
			base.transform.position += velocity * CupheadTime.Delta;
			if (!parried)
			{
				velocity.y += properties.CurrentState.barnacles.initialGravity;
			}
			else
			{
				velocity.y += properties.CurrentState.barnacles.parryGravity;
			}
			yield return null;
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (onPlayerCollisionDeath)
		{
			damageDealer.DealDamage(hit);
			base.OnCollisionPlayer(hit, phase);
			Object.Destroy(base.gameObject);
		}
	}

	protected override void OnCollisionWalls(GameObject hit, CollisionPhase phase)
	{
		velocity.x = 0f;
		base.OnCollisionWalls(hit, phase);
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionGround(hit, phase);
		Object.Destroy(base.gameObject);
	}

	public override void OnParryPostPause(AbstractPlayerController player)
	{
		direction = ((base.transform.position.x > player.center.x) ? 1 : (-1));
		parried = true;
		velocity.y = properties.CurrentState.barnacles.parryArcMovementY;
		velocity.x = properties.CurrentState.barnacles.parryArcMovementX * (float)direction;
		base.OnParryPostPause(player);
		circleCollider.enabled = true;
		StartCoroutine(damageTypes_cr());
	}

	private IEnumerator damageTypes_cr()
	{
		damageDealer.SetDamageFlags(damagesPlayer: false, damagesEnemy: false, damagesOther: false);
		onPlayerCollisionDeath = false;
		yield return CupheadTime.WaitForSeconds(this, 2f);
		damageDealer.SetDamageFlags(damagesPlayer: true, damagesEnemy: false, damagesOther: false);
		onPlayerCollisionDeath = true;
	}

	protected override void OnDestroy()
	{
		StopAllCoroutines();
		base.OnDestroy();
	}
}
