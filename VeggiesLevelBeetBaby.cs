using UnityEngine;

public class VeggiesLevelBeetBaby : AbstractCollidableObject
{
	public enum Type
	{
		Regular,
		Fat,
		Pink
	}

	public enum State
	{
		Go,
		Dead
	}

	private const int BULLET_COUNT = 3;

	private const int BULLET_COUNT_FAT = 5;

	[SerializeField]
	private VeggiesLevelBeetBabyBullet bulletPrefab;

	private Type type;

	private float speed;

	private float childSpeed;

	private float range;

	private State state;

	private DamageDealer damageDealer;

	public VeggiesLevelBeetBaby Create(Type type, float speed, float childSpeed, float range, Vector2 pos, float rot)
	{
		VeggiesLevelBeetBaby veggiesLevelBeetBaby = InstantiatePrefab<VeggiesLevelBeetBaby>();
		veggiesLevelBeetBaby.Init(type, speed, childSpeed, range, pos, rot);
		return veggiesLevelBeetBaby;
	}

	private void Init(Type type, float speed, float childSpeed, float range, Vector2 pos, float rot)
	{
		this.type = type;
		this.speed = speed;
		this.childSpeed = childSpeed;
		this.range = range;
		base.transform.position = pos;
		base.transform.SetEulerAngles(0f, 0f, rot);
		base.animator.Play(type.ToString());
		damageDealer = new DamageDealer(1f, 0.2f, damagesPlayer: true, damagesEnemy: false, damagesOther: false);
		damageDealer.SetDirection(DamageDealer.Direction.Neutral, base.transform);
	}

	private void Update()
	{
		if (state != State.Dead)
		{
			base.transform.position += base.transform.right * speed * CupheadTime.Delta;
			if (base.transform.position.y > 360f)
			{
				Die();
			}
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		damageDealer.DealDamage(hit);
	}

	private void Die()
	{
		state = State.Dead;
		base.animator.SetTrigger("Explode");
		int num = ((type != Type.Fat) ? 3 : 5);
		for (int i = 0; i < num; i++)
		{
			float t = (float)i / (float)(num - 1);
			float rot = Mathf.Lerp(0f, range, t) - 90f - range / 2f;
			VeggiesLevelBeetBabyBullet veggiesLevelBeetBabyBullet = bulletPrefab.Create(childSpeed, base.transform.position, rot);
			if (type == Type.Pink)
			{
				veggiesLevelBeetBabyBullet.SetParryable(parryable: true);
			}
		}
	}

	private void OnDeathAnimComplete()
	{
		Object.Destroy(base.gameObject);
	}
}
