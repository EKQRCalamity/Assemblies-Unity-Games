using System.Collections;
using UnityEngine;

public class RumRunnersLevelBouncingBeetle : AbstractProjectile
{
	private const float DESTROY_RANGE = 100f;

	private static int LastSortingIndex;

	[SerializeField]
	private Effect wallPoofEffect;

	[SerializeField]
	private Transform visualTransform;

	[SerializeField]
	private float squashAmount;

	[SerializeField]
	private float squashAmountPerpendicular;

	private bool isMoving;

	private float initialSpeed;

	private float targetSpeed;

	private float currentSpeed;

	private float slowdownDuration;

	private float hp;

	private float offset;

	private Vector3 velocity;

	private Vector3 initialScale;

	private Coroutine squashCoroutine;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private Effect explosionPrefab;

	[SerializeField]
	private SpriteDeathPartsDLC shrapnelPrefab;

	public bool leaveScreen { get; set; }

	protected override float DestroyLifetime => 0f;

	protected override void Start()
	{
		base.Start();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		initialScale = visualTransform.localScale;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	public virtual RumRunnersLevelBouncingBeetle Init(Vector2 pos, Vector3 velocity, float initialSpeed, float timeToSlowdown, float targetSpeed, float hp)
	{
		ResetLifetime();
		ResetDistance();
		SetParryable(parryable: false);
		base.transform.position = pos;
		this.velocity = velocity;
		this.initialSpeed = (currentSpeed = initialSpeed);
		this.targetSpeed = targetSpeed;
		currentSpeed = targetSpeed;
		slowdownDuration = timeToSlowdown;
		isMoving = true;
		this.hp = hp;
		offset = GetComponent<Collider2D>().bounds.size.x / 2f;
		Move();
		leaveScreen = false;
		LastSortingIndex--;
		if (LastSortingIndex < 10)
		{
			LastSortingIndex = 15;
		}
		visualTransform.GetComponent<SpriteRenderer>().sortingOrder = LastSortingIndex;
		return this;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp <= 0f)
		{
			Level.Current.RegisterMinionKilled();
			Die();
		}
	}

	protected override void Die()
	{
		SFX_RUMRUN_CaterpillarBall_DeathExplosion();
		explosionPrefab.Create(base.transform.position);
		for (int i = 0; i < Random.Range(3, 5); i++)
		{
			float f = Random.Range(0f, 360f);
			Vector3 vector = new Vector3(Mathf.Cos(f) * 50f, Mathf.Sin(f) * 50f);
			SpriteDeathParts spriteDeathParts = shrapnelPrefab.CreatePart(base.transform.position + vector);
			spriteDeathParts.animator.Update(0f);
			spriteDeathParts.animator.Play(0, 0, Random.Range(0f, 1f));
		}
		for (int j = 0; j < Random.Range(3, 5); j++)
		{
			float f2 = Random.Range(0f, 360f);
			Vector3 vector2 = new Vector3(Mathf.Cos(f2) * 50f, Mathf.Sin(f2) * 50f);
			SpriteDeathParts spriteDeathParts2 = shrapnelPrefab.CreatePart(base.transform.position + vector2);
			spriteDeathParts2.animator.Update(0f);
			spriteDeathParts2.animator.Play(0, 0, Random.Range(0f, 1f));
			spriteDeathParts2.transform.SetScale(0.75f, 0.75f);
			SpriteRenderer component = spriteDeathParts2.GetComponent<SpriteRenderer>();
			component.sortingLayerName = "Background";
			component.sortingOrder = 95;
			component.color = new Color(0.7f, 0.7f, 0.7f, 1f);
		}
		base.Die();
		this.Recycle();
	}

	private void Move()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float elapsedTime = 0f;
		while (true)
		{
			yield return wait;
			if (isMoving)
			{
				if (elapsedTime <= slowdownDuration)
				{
					elapsedTime += CupheadTime.FixedDelta;
					currentSpeed = Mathf.Lerp(initialSpeed, targetSpeed, elapsedTime / slowdownDuration);
				}
				base.transform.position += velocity * currentSpeed * CupheadTime.FixedDelta;
				CheckBounds();
			}
		}
	}

	private void CheckBounds()
	{
		bool flag = false;
		Vector3 vector = Vector3.zero;
		Vector3 one = Vector3.one;
		float z = 0f;
		if (base.transform.position.y > CupheadLevelCamera.Current.Bounds.yMax - offset && velocity.y > 0f)
		{
			flag = true;
			velocity.y = 0f - Mathf.Abs(velocity.y);
			vector = Vector2.up;
			one.x = ((!(velocity.x > 0f)) ? (-1f) : 1f);
			z = 180f;
		}
		if (base.transform.position.y < (float)Level.Current.Ground + offset && velocity.y < 0f)
		{
			flag = true;
			velocity.y = Mathf.Abs(velocity.y);
			vector = Vector2.down;
			one.x = ((!(velocity.x < 0f)) ? (-1f) : 1f);
			one.y = 1f;
			z = 0f;
		}
		if (!leaveScreen)
		{
			if (base.transform.position.x > CupheadLevelCamera.Current.Bounds.xMax - offset && velocity.x > 0f)
			{
				flag = true;
				velocity.x = 0f - Mathf.Abs(velocity.x);
				vector = Vector2.right;
				z = 90f;
				one.x = ((!(velocity.y < 0f)) ? (-1f) : 1f);
			}
			if (base.transform.position.x < CupheadLevelCamera.Current.Bounds.xMin + offset && velocity.x < 0f)
			{
				flag = true;
				velocity.x = Mathf.Abs(velocity.x);
				vector = Vector2.left;
				one.x = ((!(velocity.y > 0f)) ? (-1f) : 1f);
				z = 270f;
			}
		}
		else if (base.transform.position.x < CupheadLevelCamera.Current.Bounds.xMin - 100f || base.transform.position.x > CupheadLevelCamera.Current.Bounds.xMax + 100f)
		{
			base.Die();
		}
		if (flag)
		{
			Effect effect = wallPoofEffect.Create(base.transform.position + vector * offset);
			effect.transform.rotation = Quaternion.Euler(0f, 0f, z);
			Vector3 localScale = effect.transform.localScale;
			localScale.x *= one.x;
			effect.transform.localScale = localScale;
			SFX_RUMRUN_CaterpillarBall_Bounce();
			if (squashCoroutine != null)
			{
				StopCoroutine(squashCoroutine);
			}
			squashCoroutine = StartCoroutine(squash_cr(vector));
		}
	}

	private IEnumerator squash_cr(Vector2 normal)
	{
		Vector3 scale = initialScale;
		Vector3 visualOffset;
		if (normal.x != 0f)
		{
			scale.x *= squashAmount;
			scale.y *= squashAmountPerpendicular;
			visualOffset = new Vector3(offset * (1f - squashAmount) * Mathf.Sign(normal.x), 0f);
		}
		else
		{
			scale.y *= squashAmount;
			scale.x *= squashAmountPerpendicular;
			visualOffset = new Vector3(0f, offset * (1f - squashAmount) * Mathf.Sign(normal.y));
			SFX_RUMRUN_CaterpillarBall_Bounce();
		}
		visualTransform.localScale = scale;
		visualTransform.localPosition = visualOffset;
		for (float elapsedTime = 0f; elapsedTime < 1f / 24f; elapsedTime += (float)CupheadTime.Delta)
		{
			yield return null;
		}
		visualTransform.localScale = initialScale;
		visualTransform.localPosition = Vector3.zero;
		squashCoroutine = null;
	}

	private void SFX_RUMRUN_CaterpillarBall_Bounce()
	{
		AudioManager.Play("sfx_dlc_rumrun_caterpillarball_bounce");
		emitAudioFromObject.Add("sfx_dlc_rumrun_caterpillarball_bounce");
	}

	private void SFX_RUMRUN_CaterpillarBall_DeathExplosion()
	{
		AudioManager.Play("sfx_dlc_rumrun_caterpillarball_deathexplosion");
		emitAudioFromObject.Add("sfx_dlc_rumrun_caterpillarball_deathexplosion");
	}
}
