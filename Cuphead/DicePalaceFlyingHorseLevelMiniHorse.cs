using System.Collections;
using UnityEngine;

public class DicePalaceFlyingHorseLevelMiniHorse : AbstractProjectile
{
	[SerializeField]
	private BasicProjectile bullet;

	[SerializeField]
	private BasicProjectile pinkBullet;

	[SerializeField]
	private GameObject jockey;

	[SerializeField]
	private SpriteRenderer[] renderers;

	private LevelProperties.DicePalaceFlyingHorse.MiniHorses properties;

	private AbstractPlayerController player;

	private DamageReceiver damageReceiver;

	private Coroutine horseCoroutine;

	private float hp;

	private float threeProximity;

	private bool isPink;

	private bool jockeyDead;

	private Vector3 backgroundLane;

	private Animator jockeyAnimator;

	protected override float DestroyLifetime => 20f;

	protected override void Awake()
	{
		base.Awake();
		jockey.GetComponent<CollisionChild>().OnPlayerCollision += OnCollisionPlayer;
		damageReceiver = jockey.GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		jockeyAnimator = jockey.GetComponent<Animator>();
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp < 0f && !jockeyDead)
		{
			KillJockey();
			jockeyDead = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void Init(Vector3 position, float hp, LevelProperties.DicePalaceFlyingHorse.MiniHorses properties, AbstractPlayerController player, DicePalaceFlyingHorseLevelHorse.MiniHorseType type, bool isPink, float threeProximity, int lane, Vector3 backgroundLane)
	{
		base.transform.position = position;
		this.hp = hp;
		this.properties = properties;
		this.isPink = isPink;
		this.player = player;
		this.threeProximity = threeProximity;
		this.backgroundLane = backgroundLane;
		base.animator.SetInteger("Horse", Random.Range(1, 3));
		switch (type)
		{
		case DicePalaceFlyingHorseLevelHorse.MiniHorseType.One:
			jockeyAnimator.SetInteger("Caddy", Random.Range(1, 4));
			break;
		case DicePalaceFlyingHorseLevelHorse.MiniHorseType.Two:
			jockeyAnimator.SetInteger("Caddy", Random.Range(1, 4));
			horseCoroutine = StartCoroutine(horse_two_cr());
			break;
		case DicePalaceFlyingHorseLevelHorse.MiniHorseType.Three:
			jockeyAnimator.SetInteger("Caddy", 4);
			horseCoroutine = StartCoroutine(horse_three_cr());
			break;
		}
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].sortingOrder = renderers.Length * lane + renderers[i].sortingOrder;
		}
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		float speed = properties.miniSpeedRange.RandomFloat();
		while (base.transform.position.x > -740f)
		{
			base.transform.AddPosition((0f - speed) * (float)CupheadTime.Delta);
			yield return null;
		}
		base.transform.position = backgroundLane;
		base.transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
		SpriteRenderer horseRenderer = GetComponent<SpriteRenderer>();
		horseRenderer.color = ColorUtils.HexToColor("C5C5C5FF");
		horseRenderer.sortingLayerName = "Default";
		horseRenderer.sortingOrder -= 100;
		if (jockey != null)
		{
			SpriteRenderer component = jockey.GetComponent<SpriteRenderer>();
			component.material = horseRenderer.material;
			component.color = horseRenderer.color;
			component.sortingLayerName = "Default";
			component.sortingOrder -= 100;
			jockey.GetComponent<Collider2D>().enabled = false;
		}
		GetComponent<Collider2D>().enabled = false;
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (base.transform.position.x < 740f)
		{
			base.transform.AddPosition(speed * (float)CupheadTime.Delta * 0.5f);
			yield return null;
		}
		Die();
		yield return null;
	}

	private IEnumerator horse_two_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, properties.miniTwoShotDelayRange.RandomFloat());
		if (!jockeyDead)
		{
			ShootBullet();
		}
		yield return null;
	}

	private void ShootBullet()
	{
		if (player == null || player.IsDead)
		{
			player = PlayerManager.GetNext();
		}
		Vector3 vector = player.transform.position - base.transform.position;
		float rotation = MathUtils.DirectionToAngle(vector);
		if (isPink)
		{
			pinkBullet.Create(base.transform.position, rotation, properties.miniTwoBulletSpeed);
		}
		else
		{
			bullet.Create(base.transform.position, rotation, properties.miniTwoBulletSpeed);
		}
	}

	private IEnumerator horse_three_cr()
	{
		if (player == null || player.IsDead)
		{
			player = PlayerManager.GetNext();
		}
		float dist = base.transform.position.x - player.transform.position.x;
		while (dist > threeProximity)
		{
			if (player == null || player.IsDead)
			{
				player = PlayerManager.GetNext();
			}
			dist = base.transform.position.x - player.transform.position.x;
			yield return null;
		}
		if (!jockeyDead)
		{
			jockeyAnimator.SetTrigger("Attack");
			yield return jockeyAnimator.WaitForAnimationToStart(this, "CloakedAttack_End");
		}
		if (!jockeyDead)
		{
			jockey.transform.SetParent(null);
			jockey.transform.GetChild(0).SetParent(base.transform);
		}
		while (jockey.transform.position.y < 360f && !jockeyDead)
		{
			jockey.transform.AddPosition(0f, properties.miniThreeJockeySpeed * (float)CupheadTime.Delta);
			yield return null;
		}
		if (!jockeyDead)
		{
			KillJockey();
		}
		yield return null;
	}

	protected override void Die()
	{
		StopAllCoroutines();
		base.Die();
	}

	private void KillJockey()
	{
		StopCoroutine(horseCoroutine);
		Object.Destroy(jockey);
	}

	public override void OnLevelEnd()
	{
	}
}
