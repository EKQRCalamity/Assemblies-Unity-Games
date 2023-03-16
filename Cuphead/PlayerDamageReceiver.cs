using UnityEngine;

public class PlayerDamageReceiver : DamageReceiver
{
	public enum State
	{
		Vulnerable,
		Invulnerable,
		Other
	}

	private const float TIME_HIT = 2f;

	private const float TIME_REVIVED = 3f;

	private AbstractPlayerController player;

	private float timer;

	public State state { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		if (type != Type.Player)
		{
		}
		type = Type.Player;
		player = GetComponent<AbstractPlayerController>();
		player.OnReviveEvent += OnRevive;
	}

	private void Update()
	{
		if (state == State.Invulnerable && timer > 0f)
		{
			timer -= CupheadTime.Delta;
			if (timer <= 0f)
			{
				Vulnerable();
			}
		}
	}

	private void HandleChaliceShmupSuper(DamageDealer.DamageInfo info)
	{
		if (player.stats.State == PlayerStatsManager.PlayerState.Super && player.stats.isChalice && player.stats.Loadout.super == Super.level_super_ghost)
		{
			base.TakeDamageBruteForce(info);
		}
	}

	public override void TakeDamage(DamageDealer.DamageInfo info)
	{
		if (player.stats.SuperInvincible)
		{
			return;
		}
		if (info.damage > 0f)
		{
			HandleChaliceShmupSuper(info);
			if (!base.enabled)
			{
				return;
			}
			if (info.damageSource == DamageDealer.DamageSource.Pit)
			{
				if (player.damageReceiver.state != 0)
				{
					return;
				}
			}
			else if (!player.CanTakeDamage)
			{
				return;
			}
			if (!(timer > 0f))
			{
				float num = 1f;
				Invulnerable(2f * num);
				base.TakeDamage(info);
				if (player.stats.ChaliceShieldOn)
				{
					player.stats.SetChaliceShield(chaliceShield: false);
				}
			}
		}
		else if (info.stoneTime > 0f)
		{
			base.TakeDamage(info);
		}
	}

	public void OnRevive(Vector3 pos)
	{
		Invulnerable(3f);
	}

	public void Invulnerable(float time)
	{
		state = State.Invulnerable;
		timer = time;
	}

	public void Vulnerable()
	{
		state = State.Vulnerable;
		timer = 0f;
	}

	public void OnDeath()
	{
		state = State.Other;
	}

	public void OnWin()
	{
		state = State.Other;
	}
}
