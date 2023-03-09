using System.Collections;
using UnityEngine;

public class RumRunnersLevelMoth : AbstractCollidableObject
{
	private const float RIGHT_X = 540f;

	private const float LEFT_X = -540f;

	[SerializeField]
	private GameObject sparkWarning;

	[SerializeField]
	private BasicProjectile regularProjectile;

	private RumRunnersLevelSpider parent;

	private LevelProperties.RumRunners.Moth properties;

	private DamageReceiver damageReceiver;

	private float hp;

	private bool goingLeft;

	private bool dead;

	private void Start()
	{
		sparkWarning.SetActive(value: false);
		if ((bool)GetComponent<DamageReceiver>())
		{
			damageReceiver = GetComponent<DamageReceiver>();
			damageReceiver.OnDamageTaken += OnDamageTaken;
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp <= 0f)
		{
			Die();
		}
	}

	public void Init(Vector3 pos, LevelProperties.RumRunners.Moth properties, RumRunnersLevelSpider parent)
	{
		base.transform.position = pos;
		this.properties = properties;
		hp = properties.hp;
		StartAttack();
		this.parent = parent;
		this.parent.OnDeathEvent += Die;
	}

	private void StartAttack()
	{
		StartCoroutine(move_cr());
		StartCoroutine(shoot_cr());
		StartCoroutine(life_timer_cr());
	}

	private IEnumerator move_cr()
	{
		goingLeft = Rand.Bool();
		float dist = ((!goingLeft) ? Mathf.Abs(540f - base.transform.position.x) : Mathf.Abs(-540f - base.transform.position.x));
		float time = dist / properties.mothSpeed;
		float t = 0f;
		float start = base.transform.position.x;
		float end = ((!goingLeft) ? 540f : (-540f));
		YieldInstruction wait = new WaitForFixedUpdate();
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			base.transform.SetPosition(Mathf.Lerp(start, end, t / time));
			yield return wait;
		}
		dist = Mathf.Abs(-1080f);
		time = dist / properties.mothSpeed;
		while (!dead)
		{
			t = 0f;
			goingLeft = !goingLeft;
			start = base.transform.position.x;
			end = ((!goingLeft) ? 540f : (-540f));
			while (t < time)
			{
				t += CupheadTime.FixedDelta;
				base.transform.SetPosition(Mathf.Lerp(start, end, t / time));
				yield return wait;
			}
			yield return wait;
		}
		yield return null;
	}

	private IEnumerator shoot_cr()
	{
		while (!dead)
		{
			sparkWarning.SetActive(value: false);
			yield return CupheadTime.WaitForSeconds(this, properties.mothShootDelay);
			sparkWarning.SetActive(value: true);
			regularProjectile.Create(base.transform.position, -90f, properties.mothBulletSpeed);
			yield return null;
		}
	}

	private IEnumerator life_timer_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, properties.mothLifetime);
		Die();
	}

	private void Die()
	{
		dead = true;
		StopAllCoroutines();
		Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		parent.OnDeathEvent -= Die;
		base.OnDestroy();
	}
}
