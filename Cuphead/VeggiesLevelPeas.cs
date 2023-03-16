using System.Collections;
using UnityEngine;

public class VeggiesLevelPeas : LevelProperties.Veggies.Entity
{
	public enum State
	{
		Start,
		Complete
	}

	public delegate void OnDamageTakenHandler(float damage);

	[SerializeField]
	private VeggiesLevelOnionTearProjectile projectilePrefab;

	private new LevelProperties.Veggies.Peas properties;

	private float hp;

	public State state { get; private set; }

	public event OnDamageTakenHandler OnDamageTakenEvent;

	private void Start()
	{
		GetComponent<Collider2D>().enabled = false;
	}

	public override void LevelInitWithGroup(AbstractLevelPropertyGroup propertyGroup)
	{
		base.LevelInitWithGroup(propertyGroup);
		properties = propertyGroup as LevelProperties.Veggies.Peas;
		hp = properties.hp;
		GetComponent<DamageReceiver>().OnDamageTaken += OnDamageTaken;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (this.OnDamageTakenEvent != null)
		{
			this.OnDamageTakenEvent(info.damage);
		}
		hp -= info.damage;
		if (hp <= 0f)
		{
			Die();
		}
	}

	private void OnInAnimComplete()
	{
		GetComponent<Collider2D>().enabled = true;
		StartCoroutine(peas_cr());
	}

	private void OnDeathAnimComplete()
	{
		state = State.Complete;
		Object.Destroy(base.gameObject);
	}

	private void Die()
	{
		StopAllCoroutines();
		StartCoroutine(die_cr());
	}

	private IEnumerator peas_cr()
	{
		yield return null;
	}

	private IEnumerator die_cr()
	{
		GetComponent<Collider2D>().enabled = false;
		base.animator.SetTrigger("Idle");
		yield return StartCoroutine(dieFlash_cr());
		base.animator.SetTrigger("Dead");
	}
}
