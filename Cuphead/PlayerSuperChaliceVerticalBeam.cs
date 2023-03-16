using System.Collections.Generic;
using UnityEngine;

public class PlayerSuperChaliceVerticalBeam : AbstractPlayerSuper
{
	private const float STRAW_Y_OFFSET = -200f;

	[Header("Effects")]
	[SerializeField]
	private Effect hitPrefab;

	[SerializeField]
	private GameObject StrawFX;

	private bool updateStraw;

	private List<DamageReceiver> damageReceivers;

	protected override void Awake()
	{
		base.Awake();
		damageReceivers = new List<DamageReceiver>();
	}

	protected override void Update()
	{
		base.Update();
		if (updateStraw)
		{
			UpdateStraw();
		}
		if (player == null)
		{
			Interrupt();
		}
		else
		{
			player.transform.position = base.transform.position;
		}
	}

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		DamageReceiver component = hit.GetComponent<DamageReceiver>();
		if (component != null && !damageReceivers.Contains(component))
		{
			damageReceivers.Add(component);
		}
	}

	private void OnDealDamage(float damage, DamageReceiver receiver, DamageDealer dealer)
	{
		Collider2D componentInChildren = receiver.GetComponentInChildren<Collider2D>();
		Vector2 zero = Vector2.zero;
		Vector2 vector = Vector2.zero;
		if (componentInChildren.GetType() == typeof(BoxCollider2D))
		{
			zero = (componentInChildren as BoxCollider2D).size;
		}
		else
		{
			if (componentInChildren.GetType() != typeof(CircleCollider2D))
			{
				return;
			}
			zero = Vector2.one * (componentInChildren as CircleCollider2D).radius;
		}
		vector = new Vector2(componentInChildren.transform.position.x + Random.Range((0f - zero.x) / 2f, zero.x / 2f), componentInChildren.transform.position.y + Random.Range((0f - zero.y) / 2f, zero.y / 2f));
		vector += componentInChildren.offset;
		hitPrefab.Create(vector);
	}

	protected override void Fire()
	{
		AudioManager.Play("player_super_chalice_superbeam");
		base.Fire();
		damageDealer = new DamageDealer(WeaponProperties.LevelSuperChaliceVertBeam.damage, WeaponProperties.LevelSuperChaliceVertBeam.damageRate, DamageDealer.DamageSource.Super, damagesPlayer: false, damagesEnemy: true, damagesOther: true);
		damageDealer.OnDealDamage += OnDealDamage;
		damageDealer.DamageMultiplier *= PlayerManager.DamageMultiplier;
		damageDealer.PlayerId = player.id;
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Super);
		meterScoreTracker.Add(damageDealer);
	}

	protected override void StartSuper()
	{
		if (!(player == null))
		{
			player.weaponManager.OnSuperStart -= player.motor.StartSuper;
			player.weaponManager.OnSuperEnd -= player.motor.OnSuperEnd;
			base.StartSuper();
			string text = ((!player.motor.Grounded) ? "_Air" : string.Empty);
			base.animator.Play("Vert_Beam_Loop" + text);
			AudioManager.Play("player_super_chalice_superbeam_start");
		}
	}

	private void AnimationDone()
	{
		if (player != null)
		{
			player.motor.CheckForPostSuperHop();
		}
		EndSuper();
		Object.Destroy(base.gameObject);
	}

	private void UpdateStraw()
	{
		Camera main = Camera.main;
		Transform transform = main.transform;
		StrawFX.transform.SetPosition(null, transform.position.y + -200f * base.transform.localScale.y);
	}

	private void Ani_LockStraw()
	{
		updateStraw = true;
	}

	private void Ani_UnLockStraw()
	{
		updateStraw = false;
		AudioManager.Play("player_super_chalice_superbeam_end");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		hitPrefab = null;
		damageReceivers.Clear();
		damageReceivers = null;
	}
}
