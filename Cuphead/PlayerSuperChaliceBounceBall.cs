using System;
using UnityEngine;

public class PlayerSuperChaliceBounceBall : AbstractProjectile
{
	private const float PADDING_LEFT = 30f;

	private const float PADDING_RIGHT = 30f;

	private const float ANALOG_THRESHOLD = 0.35f;

	private float MAX_DAMAGE = WeaponProperties.LevelSuperChaliceBounce.maxDamage;

	private float MOVE_ACCEL = WeaponProperties.LevelSuperChaliceBounce.horizontalAcceleration;

	private float MOVE_MAX_SPEED = WeaponProperties.LevelSuperChaliceBounce.maxHorizontalSpeed;

	private float BOUNCE_VEL = WeaponProperties.LevelSuperChaliceBounce.bounceVelocity;

	private float BOUNCE_MODIFIER_NO_JUMP = WeaponProperties.LevelSuperChaliceBounce.bounceModifierNoJump;

	private float GRAVITY = WeaponProperties.LevelSuperChaliceBounce.gravity;

	private float ENEMY_REBOUND_MULTIPLIER = WeaponProperties.LevelSuperChaliceBounce.enemyReboundMultiplier;

	private float ENEMY_MULTIHIT_DELAY = WeaponProperties.LevelSuperChaliceBounce.enemyMultihitDelay;

	[SerializeField]
	private Effect smokePuffEffect;

	public Vector2 velocity;

	public LevelPlayerController player;

	private GameObject lastEnemyHit;

	private float lastHitTimer;

	public PlayerSuperChaliceBounce super;

	private float jiggleTime;

	private Vector3 baseScale;

	private float colliderSize;

	private SpriteRenderer rend;

	private float damageCount;

	protected override void OnDieLifetime()
	{
	}

	protected override void OnDieDistance()
	{
	}

	protected override void Start()
	{
		base.Start();
		baseScale = base.transform.localScale;
		colliderSize = GetComponent<CircleCollider2D>().radius;
		rend = GetComponent<SpriteRenderer>();
		velocity.y = 0f;
		damageDealer.SetDamageSource(DamageDealer.DamageSource.Super);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		velocity.y -= GRAVITY * CupheadTime.FixedDelta;
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		HandleInput();
		HandleJiggle();
		CheckEdges();
		CheckCollisionsThenMove();
		if (!super.LAUNCHED_VERSION)
		{
			player.transform.position = base.transform.position;
		}
		lastHitTimer -= CupheadTime.FixedDelta;
		if (super.timer < 1f)
		{
			rend.enabled = Time.frameCount % 2 == 0;
		}
	}

	public override void OnLevelEnd()
	{
		super.CleanUp();
	}

	private void HandleJiggle()
	{
		if (jiggleTime > 0f)
		{
			base.transform.localScale = new Vector3(baseScale.x + Mathf.Sin(jiggleTime * (float)Math.PI * 15f) * jiggleTime * 10f, baseScale.y + Mathf.Cos(jiggleTime * (float)Math.PI * 15f) * jiggleTime * 10f, 1f);
			jiggleTime -= CupheadTime.FixedDelta;
		}
		else
		{
			base.transform.localScale = baseScale;
		}
	}

	private void SetJiggle()
	{
		AudioManager.Play("player_jump");
		AudioManager.Play("circus_trampoline_bounce");
		jiggleTime = 0.2f;
	}

	private void CheckCollisionsThenMove()
	{
		Vector3 inNormal = Vector3.zero;
		float num = 0f;
		GameObject gameObject = null;
		int num2 = 0;
		float num3 = Vector3.Magnitude(velocity * CupheadTime.FixedDelta);
		int num4 = 9;
		float num5 = 3f;
		float num6 = baseScale.x * colliderSize * 0.9f;
		int num7 = 262144;
		int num8 = 1048576;
		int num9 = 524288;
		int layerMask = num7 + num9 + num8;
		int layerMask2 = 1;
		Vector3[] array = new Vector3[num4];
		while (num3 > 0f && (float)num2 < num5)
		{
			gameObject = null;
			bool flag = false;
			Vector3 vector = velocity.normalized;
			float num10 = 180 / (array.Length - 1);
			for (int i = 0; i < array.Length; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = base.transform.position + Quaternion.Euler(0f, 0f, -90f + num10 * (float)i) * vector * num6;
			}
			num = num3;
			for (int j = 0; j < num4; j++)
			{
				if (Physics2D.OverlapPoint(array[j], layerMask) != null && Physics2D.OverlapPoint(base.transform.position, layerMask) == null)
				{
					RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, array[j] - base.transform.position, num6 * 2f, layerMask);
					if (raycastHit2D.collider != null)
					{
						ref Vector3 reference2 = ref array[j];
						reference2 = Vector3.Lerp(raycastHit2D.point, base.transform.position, 0.001f);
					}
				}
			}
			for (int k = 0; k < num4; k++)
			{
				RaycastHit2D raycastHit2D2 = Physics2D.Raycast(array[k], velocity, num, num7);
				Debug.DrawLine(array[k], array[k] + vector * num, Color.red, 1f);
				if (raycastHit2D2.collider != null)
				{
					smokePuffEffect.Create(raycastHit2D2.point);
					if (Vector3.Distance(array[k], raycastHit2D2.point) <= num)
					{
						flag = true;
						inNormal = raycastHit2D2.normal;
						num = Vector3.Distance(array[k], raycastHit2D2.point);
					}
				}
			}
			for (int l = 0; l < num4; l++)
			{
				RaycastHit2D raycastHit2D2 = Physics2D.Raycast(array[l], velocity, num, layerMask2);
				if (raycastHit2D2.collider != null && raycastHit2D2.collider.gameObject.CompareTag("Enemy") && (lastEnemyHit == null || (bool)lastEnemyHit != (bool)raycastHit2D2 || lastHitTimer <= 0f))
				{
					smokePuffEffect.Create(raycastHit2D2.point);
					if (Vector3.Distance(array[l], raycastHit2D2.point) <= num)
					{
						flag = true;
						inNormal = raycastHit2D2.normal;
						num = Vector3.Distance(array[l], raycastHit2D2.point);
						gameObject = raycastHit2D2.collider.gameObject;
					}
				}
			}
			if (velocity.y >= 0f)
			{
				for (int m = 0; m < num4; m++)
				{
					RaycastHit2D raycastHit2D2 = Physics2D.Raycast(array[m], velocity, num, num9);
					if (raycastHit2D2.collider != null)
					{
						smokePuffEffect.Create(raycastHit2D2.point);
						if (Vector3.Distance(array[m], raycastHit2D2.point) <= num)
						{
							flag = true;
							inNormal = raycastHit2D2.normal;
							num = Vector3.Distance(array[m], raycastHit2D2.point);
						}
					}
				}
			}
			for (int n = 0; n < num4; n++)
			{
				RaycastHit2D raycastHit2D2 = Physics2D.Raycast(array[n], velocity, num, num8);
				if (!(raycastHit2D2.collider != null))
				{
					continue;
				}
				LevelPlatform component = raycastHit2D2.collider.gameObject.GetComponent<LevelPlatform>();
				bool flag2 = false;
				if (component != null && (base.transform.position.y < raycastHit2D2.point.y || velocity.y > 0f))
				{
					flag2 = true;
				}
				if (!flag2 && (component == null || !component.canFallThrough || player.input.actions.GetAxis(1) > -0.35f))
				{
					smokePuffEffect.Create(raycastHit2D2.point);
					if (Vector3.Distance(array[n], raycastHit2D2.point) <= num)
					{
						flag = true;
						inNormal = raycastHit2D2.normal;
						num = Vector3.Distance(array[n], raycastHit2D2.point);
					}
				}
			}
			num3 -= num;
			base.transform.position += vector * num;
			if (flag)
			{
				SetJiggle();
				num2++;
				velocity = Vector3.Reflect(velocity, inNormal);
				if (inNormal.y > 0f)
				{
					velocity.y = inNormal.y * BOUNCE_VEL * ((!player.input.actions.GetButton(2)) ? BOUNCE_MODIFIER_NO_JUMP : 1f);
				}
				if (gameObject != null)
				{
					DoCollisionEnemy(gameObject);
					velocity.x *= ENEMY_REBOUND_MULTIPLIER;
				}
			}
		}
	}

	protected void DoCollisionEnemy(GameObject hit)
	{
		lastEnemyHit = hit;
		lastHitTimer = ENEMY_MULTIHIT_DELAY;
		float num = damageDealer.DealDamage(hit);
		if (num > 0f)
		{
			base.animator.Play("Player_Super_Chalice_BounceBall_Flash");
			AudioManager.Play("player_parry_axe");
		}
		damageCount += num;
		if (damageCount >= MAX_DAMAGE)
		{
			super.Interrupt();
		}
	}

	private void HandleInput()
	{
		Trilean trilean = 0;
		Trilean trilean2 = 0;
		float axis = player.input.actions.GetAxis(0);
		if (axis > 0.35f || axis < -0.35f)
		{
			trilean = axis;
		}
		float mOVE_ACCEL = MOVE_ACCEL;
		velocity.x += (float)trilean.Value * mOVE_ACCEL * CupheadTime.FixedDelta;
		velocity.x = Mathf.Clamp(velocity.x, 0f - MOVE_MAX_SPEED, MOVE_MAX_SPEED);
	}

	private void CheckEdges()
	{
		if (LevelPit.Instance != null && base.transform.position.y < LevelPit.Instance.transform.position.y && velocity.y < 0f)
		{
			base.transform.position += Vector3.down * 300f;
			super.Interrupt();
		}
		Vector2 vector = base.transform.position;
		vector.x = Mathf.Clamp(vector.x, (float)Level.Current.Left + 30f, (float)Level.Current.Right - 30f);
		if (vector.x != base.transform.position.x)
		{
			velocity.x = 0f - velocity.x;
		}
		base.transform.position = vector;
	}
}
