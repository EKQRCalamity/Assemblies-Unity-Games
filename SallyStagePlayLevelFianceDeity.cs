using System.Collections;
using UnityEngine;

public class SallyStagePlayLevelFianceDeity : LevelProperties.SallyStagePlay.Entity
{
	[SerializeField]
	private SallyStagePlayLevelCherubProjectile cherubProjectile;

	[SerializeField]
	private Transform husbandRoot;

	private bool isDead;

	private float health;

	private DamageReceiver damageReceiver;

	private void Start()
	{
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		GetComponent<Collider2D>().enabled = false;
	}

	public void Attack()
	{
		StartCoroutine(attack_cr());
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (SallyStagePlayLevelAngel.extraHP > 0f)
		{
			SallyStagePlayLevelAngel.extraHP -= info.damage;
		}
		else
		{
			base.properties.DealDamage(info.damage);
		}
	}

	private IEnumerator attack_cr()
	{
		LevelProperties.SallyStagePlay.Husband p = base.properties.CurrentState.husband;
		while (!isDead)
		{
			yield return CupheadTime.WaitForSeconds(this, p.shotDelayRange.RandomFloat());
			GetComponent<Animator>().SetBool("OnAttack", value: true);
			yield return GetComponent<Animator>().WaitForAnimationToEnd(this, "Puppet_Attack_Start");
			cherubProjectile.Create(husbandRoot.position, 0f, p.shotSpeed);
			GetComponent<Animator>().SetBool("OnAttack", value: false);
			yield return null;
		}
	}

	public void Dead()
	{
		isDead = true;
		StopAllCoroutines();
		damageReceiver.enabled = false;
		GetComponent<Animator>().SetTrigger("OnDeath");
		StartCoroutine(move_cr());
	}

	public IEnumerator move_cr()
	{
		float t = 0f;
		float time = 3f;
		Vector3 endPos = new Vector3(-1140f, base.transform.position.y);
		Vector2 start = base.transform.position;
		GetComponent<LevelBossDeathExploder>().StartExplosion();
		yield return CupheadTime.WaitForSeconds(this, 0.8f);
		while (t < time)
		{
			float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t / time);
			base.transform.position = Vector2.Lerp(start, endPos, val);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		GetComponent<LevelBossDeathExploder>().StopExplosions();
		GetComponent<Collider2D>().enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		cherubProjectile = null;
	}
}
