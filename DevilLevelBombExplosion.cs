using System.Collections;
using UnityEngine;

public class DevilLevelBombExplosion : Effect
{
	[SerializeField]
	private float loopTime;

	private DamageDealer damageDealer;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
	}

	private void Start()
	{
		StartCoroutine(timer_cr());
		AudioManager.Play("bat_bomb_explo");
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private IEnumerator timer_cr()
	{
		yield return base.animator.WaitForAnimationToEnd(this, "Intro");
		yield return CupheadTime.WaitForSeconds(this, loopTime);
		base.animator.SetTrigger("Continue");
	}
}
