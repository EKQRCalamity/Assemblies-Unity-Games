using UnityEngine;

public class WeaponWideShotExProjectile : AbstractProjectile
{
	public float mainDuration;

	private float mainTimer;

	public Vector3 origin;

	[SerializeField]
	private Effect hitsparkPrefab;

	protected override void Start()
	{
		base.Start();
		base.transform.position += base.transform.right * 100f;
		damageDealer.isDLCWeapon = true;
	}

	protected override void Update()
	{
		base.Update();
		if (mainTimer < mainDuration)
		{
			mainTimer += CupheadTime.Delta;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void OnDealDamage(float damage, DamageReceiver receiver, DamageDealer damageDealer)
	{
		base.OnDealDamage(damage, receiver, damageDealer);
		Collider2D componentInChildren = receiver.GetComponentInChildren<Collider2D>();
		Vector3 b = receiver.transform.position;
		if (componentInChildren != null)
		{
			b = componentInChildren.transform.position + new Vector3(componentInChildren.offset.x * receiver.transform.lossyScale.x, componentInChildren.offset.y * receiver.transform.lossyScale.y);
		}
		Vector3 vector = MathUtils.AngleToDirection(base.transform.eulerAngles.z);
		Vector3 a = origin + vector * Vector3.Distance(origin, b);
		hitsparkPrefab.Create(Vector3.Lerp(a, b, 0.5f) + (Vector3)MathUtils.RandomPointInUnitCircle() * 30f);
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemy(hit, phase);
		damageDealer.DealDamage(hit);
		damageDealer.OnDealDamage += OnDealDamage;
	}
}
