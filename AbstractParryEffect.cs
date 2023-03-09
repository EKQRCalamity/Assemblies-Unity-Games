using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractParryEffect : Effect
{
	public const string TAG = "Parry";

	private const float PAUSE_TIME = 0.185f;

	private const float COLLIDER_LIFETIME = 0.2f;

	private const float CHALICE_COLLIDER_LIFETIME = 0.3f;

	private const float CHALICE_PLANE_COLLIDER_LIFETIME = 0.4f;

	private const float SPRITE_LIFETIME = 1f;

	[SerializeField]
	private GameObject sprites;

	[SerializeField]
	private Effect spark;

	[SerializeField]
	private ParryAttackSpark parryAttack;

	protected AbstractPlayerController player;

	protected bool didHitSomething;

	protected bool cancel;

	private List<AbstractProjectile> projectiles;

	private List<Effect> sparks;

	private List<ParrySwitch> switches;

	private List<AbstractLevelEntity> entities;

	private RaycastHit2D[] contactsBuffer = new RaycastHit2D[10];

	protected abstract bool IsHit { get; }

	public AbstractParryEffect Create(AbstractPlayerController player)
	{
		AbstractParryEffect abstractParryEffect = base.Create(player.center, player.transform.localScale) as AbstractParryEffect;
		abstractParryEffect.SetPlayer(player);
		return abstractParryEffect;
	}

	public override void Initialize(Vector3 position, Vector3 scale, bool randomR)
	{
		base.Initialize(position, scale, randomR);
		base.animator.enabled = false;
		sprites.SetActive(value: false);
		projectiles = new List<AbstractProjectile>();
		sparks = new List<Effect>();
		switches = new List<ParrySwitch>();
		entities = new List<AbstractLevelEntity>();
		base.tag = "Parry";
	}

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		if (cancel)
		{
			return;
		}
		base.OnCollision(hit, phase);
		if (player.IsDead || phase != 0)
		{
			return;
		}
		AbstractProjectile component = hit.GetComponent<AbstractProjectile>();
		if (component == null)
		{
			CollisionChild component2 = hit.GetComponent<CollisionChild>();
			if (component2 != null && component2.ForwardParry(out var collisionParent))
			{
				component = collisionParent.GetComponent<AbstractProjectile>();
			}
		}
		if (component != null && component.CanParry)
		{
			projectiles.Add(component);
			if (!player.stats.NextParryActivatesHealerCharm())
			{
				sparks.Add(spark.Create(component.transform.position));
			}
			if (!didHitSomething)
			{
				StartCoroutine(hit_cr());
			}
		}
		ParrySwitch component3 = hit.GetComponent<ParrySwitch>();
		if (component3 != null && component3.enabled && component3.IsParryable)
		{
			switches.Add(component3);
			if (!didHitSomething)
			{
				StartCoroutine(hit_cr());
			}
		}
		AbstractLevelEntity component4 = hit.GetComponent<AbstractLevelEntity>();
		if (component4 != null && component4.enabled && component4.canParry)
		{
			entities.Add(component4);
			if (!didHitSomething)
			{
				StartCoroutine(hit_cr());
			}
		}
		if ((player.stats.Loadout.charm != Charm.charm_parry_attack && !player.stats.CurseWhetsone) || didHitSomething || Level.IsChessBoss)
		{
			return;
		}
		IParryAttack component5 = player.GetComponent<IParryAttack>();
		if (component5 == null || component5.AttackParryUsed)
		{
			return;
		}
		DamageReceiver damageReceiver = hit.GetComponent<DamageReceiver>();
		if (damageReceiver == null)
		{
			DamageReceiverChild component6 = hit.GetComponent<DamageReceiverChild>();
			if (component6 != null)
			{
				damageReceiver = component6.Receiver;
			}
		}
		if (damageReceiver != null && damageReceiver.type == DamageReceiver.Type.Enemy)
		{
			component5.HasHitEnemy = true;
			DamageDealer damageDealer = new DamageDealer(WeaponProperties.CharmParryAttack.damage, 0f, damagesPlayer: false, damagesEnemy: true, damagesOther: false);
			damageDealer.DealDamage(hit);
			ShowParryAttackEffect(hit);
			StartCoroutine(hit_cr(hitEnemy: true));
		}
	}

	private void ShowParryAttackEffect(GameObject hit)
	{
		int num = Physics2D.RaycastNonAlloc(hit.transform.position, base.transform.position - hit.transform.position, contactsBuffer, (base.transform.position - hit.transform.position).magnitude);
		if (num == 0)
		{
			return;
		}
		Vector3 position = contactsBuffer[0].point;
		for (int i = 1; i < num; i++)
		{
			if (contactsBuffer[i].collider.tag == "Parry")
			{
				position = contactsBuffer[i].point;
			}
		}
		ParryAttackSpark parryAttackSpark = parryAttack.Create(position) as ParryAttackSpark;
		parryAttackSpark.IsCuphead = player.id == PlayerId.PlayerOne;
		sparks.Add(parryAttackSpark);
		parryAttackSpark.Play();
	}

	protected virtual void SetPlayer(AbstractPlayerController player)
	{
		this.player = player;
		base.transform.SetParent(player.transform);
		StartCoroutine(lifetime_cr());
	}

	protected virtual void OnHitCancel()
	{
		if (!(this == null))
		{
			Cancel();
			AudioManager.Stop("player_parry");
		}
	}

	protected virtual void Cancel()
	{
		foreach (Effect spark in sparks)
		{
			Object.Destroy(spark.gameObject);
		}
		cancel = true;
		CancelSwitch();
		StopAllCoroutines();
		Object.Destroy(base.gameObject);
	}

	protected virtual void CancelSwitch()
	{
	}

	protected virtual void OnPaused()
	{
	}

	protected virtual void OnUnpaused()
	{
	}

	protected virtual void OnSuccess()
	{
	}

	protected virtual void OnEnd()
	{
	}

	private IEnumerator lifetime_cr()
	{
		if (player != null && (player.stats.Loadout.charm != Charm.charm_parry_plus || Level.IsChessBoss))
		{
			if (player.stats.isChalice)
			{
				yield return CupheadTime.WaitForSeconds(this, (Level.Current.playerMode != PlayerMode.Plane) ? 0.3f : 0.4f);
			}
			else
			{
				yield return CupheadTime.WaitForSeconds(this, 0.2f);
			}
			GetComponent<Collider2D>().enabled = false;
			CancelSwitch();
			yield return CupheadTime.WaitForSeconds(this, 1f);
		}
		yield return null;
	}

	private IEnumerator hit_cr(bool hitEnemy = false)
	{
		if (player.IsDead || !player.gameObject.activeInHierarchy || !base.gameObject.activeInHierarchy)
		{
			yield break;
		}
		bool hit = false;
		didHitSomething = true;
		IParryAttack parryController = player.GetComponent<IParryAttack>();
		if (parryController != null)
		{
			parryController.AttackParryUsed = true;
		}
		base.animator.enabled = true;
		sprites.SetActive(value: true);
		if (!hitEnemy)
		{
			foreach (ParrySwitch @switch in switches)
			{
				@switch.OnParryPrePause(player);
			}
			foreach (AbstractLevelEntity entity in entities)
			{
				entity.OnParry(player);
			}
			foreach (AbstractProjectile projectile in projectiles)
			{
				projectile.OnParry(player);
				player.stats.OnParry(projectile.ParryMeterMultiplier, projectile.CountParryTowardsScore);
			}
		}
		if (player.IsDead || !player.gameObject.activeInHierarchy || !base.gameObject.activeInHierarchy)
		{
			yield break;
		}
		if (Level.Current == null || !Level.IsChessBoss || !Level.Current.Ending)
		{
			PauseManager.Pause();
		}
		AudioManager.Play("player_parry");
		OnPaused();
		float pauseTime = ((!hitEnemy) ? 0.185f : 0.13875f);
		float t = 0f;
		while (t < pauseTime)
		{
			hit = IsHit;
			if (hit)
			{
				t = pauseTime;
			}
			t += Time.fixedDeltaTime;
			for (int i = 0; i < 2; i++)
			{
				PlayerId playerId = ((i != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
				if (player != null && player.id == playerId)
				{
					if (pauseTime - t < 0.134f)
					{
						player.BufferInputs();
					}
					continue;
				}
				AbstractPlayerController abstractPlayerController = PlayerManager.GetPlayer(playerId);
				if (abstractPlayerController != null)
				{
					abstractPlayerController.BufferInputs();
				}
			}
			yield return new WaitForFixedUpdate();
		}
		while (LevelNewPlayerGUI.Current != null && LevelNewPlayerGUI.Current.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		if (hit)
		{
			yield break;
		}
		OnSuccess();
		if (Level.Current == null || !Level.IsChessBoss || !Level.Current.Ending)
		{
			PauseManager.Unpause();
		}
		OnUnpaused();
		OnEnd();
		base.transform.parent = null;
		GetComponent<Collider2D>().enabled = false;
		if (hitEnemy)
		{
			yield break;
		}
		foreach (ParrySwitch switch2 in switches)
		{
			switch2.OnParryPostPause(player);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		spark = null;
		parryAttack = null;
	}
}
