using System.Collections;
using UnityEngine;

public class FlowerLevelSeedBullet : AbstractProjectile
{
	[SerializeField]
	private Effect puffPrefab;

	[SerializeField]
	private Transform root;

	private bool isDead;

	private bool launched;

	private float speed;

	private float minSpeed;

	private float maxSpeed;

	private float timePassed;

	private float accelerationTime;

	private FlowerLevelFlower parent;

	private AbstractPlayerController player;

	public void OnBulletSeedStart(FlowerLevelFlower parent, AbstractPlayerController player, float a, float min, float max)
	{
		base.transform.LookAt2D(player.transform.position);
		base.transform.Rotate(Vector3.forward, 180f);
		minSpeed = min;
		maxSpeed = max;
		accelerationTime = a;
		this.player = player;
		this.parent = parent;
		parent.OnDeathEvent += Die;
	}

	protected override void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		base.Update();
	}

	public void LaunchBullet()
	{
		StartCoroutine(launch_bullet_cr());
	}

	private IEnumerator launch_bullet_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		base.transform.LookAt2D(player.transform.position);
		base.transform.Rotate(Vector3.forward, 180f);
		while (true)
		{
			if (timePassed < accelerationTime)
			{
				timePassed += CupheadTime.FixedDelta;
			}
			if (!isDead)
			{
				speed = minSpeed + (maxSpeed - minSpeed) * timePassed;
			}
			if (speed > 0f && !launched)
			{
				base.animator.SetTrigger("Launch");
				launched = true;
				StartCoroutine(spawn_effect_cr());
			}
			base.transform.position -= base.transform.right * (speed * CupheadTime.FixedDelta);
			if (base.transform.position.x < (float)(Level.Current.Left - 100))
			{
				Object.Destroy(base.gameObject);
			}
			if (base.transform.position.y > (float)(Level.Current.Ceiling + 100))
			{
				Object.Destroy(base.gameObject);
			}
			yield return wait;
		}
	}

	private IEnumerator spawn_effect_cr()
	{
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
		Effect puff = Object.Instantiate(puffPrefab);
		puff.transform.position = root.transform.position;
		puff.transform.LookAt2D(player.transform.position);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		FlowerLevelMiniFlowerSpawn component = hit.GetComponent<FlowerLevelMiniFlowerSpawn>();
		if (component != null)
		{
			component.FriendlyFireDamage();
			Die();
			base.OnCollisionEnemy(hit, phase);
		}
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		DeathAudio();
		base.OnCollisionGround(hit, phase);
	}

	protected override void Die()
	{
		isDead = true;
		speed = 0f;
		GetComponent<Collider2D>().enabled = false;
		StopAllCoroutines();
		base.Die();
	}

	private void DeathAudio()
	{
		AudioManager.Play("flower_bullet_seed_poof");
	}

	protected override void OnDestroy()
	{
		parent.OnDeathEvent -= Die;
		base.OnDestroy();
		puffPrefab = null;
	}
}
