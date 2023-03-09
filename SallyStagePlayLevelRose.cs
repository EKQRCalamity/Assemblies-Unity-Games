using System.Collections;
using UnityEngine;

public class SallyStagePlayLevelRose : AbstractProjectile
{
	[SerializeField]
	private GameObject normalRose;

	[SerializeField]
	private GameObject pinkRose;

	private LevelProperties.SallyStagePlay.Roses properties;

	private float speed;

	public SallyStagePlayLevelRose Create(Vector2 pos, LevelProperties.SallyStagePlay.Roses properties)
	{
		SallyStagePlayLevelRose sallyStagePlayLevelRose = base.Create(pos) as SallyStagePlayLevelRose;
		sallyStagePlayLevelRose.properties = properties;
		return sallyStagePlayLevelRose;
	}

	protected override void Start()
	{
		base.Start();
		base.transform.SetScale(Rand.Bool() ? 1 : (-1));
		StartCoroutine(move_cr());
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

	private IEnumerator move_cr()
	{
		float speed = properties.fallSpeed.min;
		while (base.transform.position.y > (float)(Level.Current.Ground + 10))
		{
			base.transform.position += Vector3.down * speed * CupheadTime.Delta;
			if (speed < properties.fallSpeed.max)
			{
				speed += properties.fallAcceleration;
			}
			yield return null;
		}
		base.animator.SetTrigger("Land");
		GetComponent<BoxCollider2D>().enabled = false;
		base.animator.SetBool("IsA", Rand.Bool());
		yield return CupheadTime.WaitForSeconds(this, properties.groundDuration);
		StartCoroutine(despawn_cr());
	}

	private IEnumerator despawn_cr()
	{
		SpriteRenderer s = GetComponentInChildren<SpriteRenderer>(includeInactive: false);
		float t = 0f;
		float time = 2f;
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			s.color = new Color(s.color.r, s.color.b, s.color.g, 1f - t / time);
			yield return null;
		}
		GetComponent<Collider2D>().enabled = false;
		Die();
	}

	protected override void Die()
	{
		StopAllCoroutines();
		GetComponentInChildren<SpriteRenderer>(includeInactive: false).enabled = false;
		base.Die();
	}

	public override void SetParryable(bool parryable)
	{
		base.SetParryable(parryable);
		pinkRose.SetActive(value: false);
		normalRose.SetActive(value: false);
		if (parryable)
		{
			pinkRose.SetActive(value: true);
		}
		else
		{
			normalRose.SetActive(value: true);
		}
	}
}
