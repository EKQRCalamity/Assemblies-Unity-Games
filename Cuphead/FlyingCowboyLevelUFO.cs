using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelUFO : AbstractProjectile
{
	[SerializeField]
	private BasicProjectile projectilePrefab;

	private const float LEFT_OFFSET = 100f;

	private const float INITIAL_LEFT_OFFSET = 100f;

	private LevelProperties.FlyingCowboy.UFOEnemy properties;

	private DamageReceiver damageReceiver;

	private Vector3 startPos;

	private bool isDead;

	private bool movingLeft;

	private float Health;

	protected override float DestroyLifetime => 0f;

	public virtual FlyingCowboyLevelUFO Init(Vector3 pos, LevelProperties.FlyingCowboy.UFOEnemy properties, float health)
	{
		ResetLifetime();
		ResetDistance();
		startPos = pos;
		base.transform.position = pos;
		this.properties = properties;
		Health = health;
		StartCoroutine(move_cr());
		return this;
	}

	protected override void Start()
	{
		base.Start();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		Health -= info.damage;
		if (Health < 0f && !isDead)
		{
			Level.Current.RegisterMinionKilled();
			Respawn();
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

	private IEnumerator move_cr()
	{
		isDead = false;
		float leftEdge = -540f;
		float initialLeftEdge = leftEdge - 100f;
		float rightEdge = -640f + properties.ufoPathLength;
		float initialX = base.transform.position.x;
		float travelDistance2 = Mathf.Abs(initialX - initialLeftEdge);
		float travelTime2 = travelDistance2 / properties.introUFOSpeed;
		float elapsedTime2 = 0f;
		while (elapsedTime2 < travelTime2)
		{
			elapsedTime2 += CupheadTime.FixedDelta;
			Vector3 position = base.transform.position;
			position.x = EaseUtils.Ease(EaseUtils.EaseType.easeOutQuad, initialX, initialLeftEdge, elapsedTime2 / travelTime2);
			base.transform.position = position;
			yield return new WaitForFixedUpdate();
		}
		movingLeft = false;
		StartCoroutine(shoot_cr());
		float currentLeftEdge = initialLeftEdge;
		travelDistance2 = Mathf.Abs(rightEdge - leftEdge);
		travelTime2 = travelDistance2 / properties.topUFOSpeed;
		elapsedTime2 = 0f;
		while (true)
		{
			if (elapsedTime2 < travelTime2)
			{
				elapsedTime2 += CupheadTime.FixedDelta;
				Vector3 position2 = base.transform.position;
				position2.x = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, (!movingLeft) ? currentLeftEdge : rightEdge, (!movingLeft) ? rightEdge : currentLeftEdge, elapsedTime2 / travelTime2);
				base.transform.position = position2;
			}
			else
			{
				currentLeftEdge = leftEdge;
				elapsedTime2 = 0f;
				movingLeft = !movingLeft;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator shoot_cr()
	{
		PatternString shootString = new PatternString(properties.topUFOShootString);
		PatternString parryString = new PatternString(properties.bulletParryString);
		while (true)
		{
			MinMax spreadAngle = new MinMax(0f, properties.spreadAngle);
			yield return CupheadTime.WaitForSeconds(this, shootString.PopFloat());
			for (int i = 0; i < properties.bulletCount; i++)
			{
				float floatAt = spreadAngle.GetFloatAt((float)i / ((float)properties.bulletCount - 1f));
				float num = spreadAngle.max / 2f;
				floatAt -= num;
				float rotation = floatAt + -90f;
				BasicProjectile basicProjectile = projectilePrefab.Create(base.transform.position, rotation, properties.bulletSpeed);
				bool flag = parryString.PopLetter() == 'P';
				basicProjectile.SetParryable(flag);
				if (flag)
				{
					basicProjectile.GetComponent<SpriteRenderer>().color = Color.magenta;
				}
			}
		}
	}

	private void Respawn()
	{
		StopAllCoroutines();
		StartCoroutine(respawn_cr());
	}

	private IEnumerator respawn_cr()
	{
		isDead = true;
		base.transform.position = new Vector3(1000f, 1000f);
		float waitTime = properties.topUFORespawnDelay;
		yield return CupheadTime.WaitForSeconds(this, waitTime);
		Health = properties.UFOHealth;
		base.transform.position = startPos;
		StartCoroutine(move_cr());
		yield return null;
	}

	public void Dead()
	{
		StopAllCoroutines();
		Object.Destroy(base.gameObject);
	}
}
