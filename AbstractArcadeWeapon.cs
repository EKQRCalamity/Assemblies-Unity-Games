using System;
using System.Collections;
using UnityEngine;

public abstract class AbstractArcadeWeapon : AbstractPausableComponent
{
	public enum Mode
	{
		Basic,
		Ex
	}

	private delegate AbstractProjectile FireProjectileDelegate();

	[Serializable]
	public class Prefabs
	{
		public AbstractProjectile basic;

		public AbstractProjectile ex;

		public AbstractProjectile Get(Mode mode)
		{
			return mode switch
			{
				Mode.Ex => ex, 
				_ => basic, 
			};
		}
	}

	[Serializable]
	public class MuzzleEffects
	{
		public Effect basic;

		public Effect ex;

		public Effect Get(Mode mode)
		{
			return mode switch
			{
				Mode.Ex => ex, 
				_ => basic, 
			};
		}
	}

	public class FiringSwitches
	{
		public bool basic;

		public bool ex;

		public bool Get(Mode mode)
		{
			return mode switch
			{
				Mode.Ex => ex, 
				_ => basic, 
			};
		}

		public void Set(Mode mode, bool val)
		{
			switch (mode)
			{
			case Mode.Ex:
				ex = val;
				break;
			default:
				basic = val;
				break;
			}
		}
	}

	[Header("Ex")]
	[SerializeField]
	protected AbstractProjectile exPrefab;

	[SerializeField]
	protected Effect exEffectPrefab;

	[Header("Basic")]
	[SerializeField]
	protected AbstractProjectile basicPrefab;

	[SerializeField]
	protected Effect basicEffectPrefab;

	protected FiringSwitches firing;

	protected ArcadePlayerController player;

	protected ArcadePlayerWeaponManager weaponManager;

	protected abstract bool rapidFire { get; }

	protected abstract float rapidFireRate { get; }

	public Weapon id { get; private set; }

	public virtual void Initialize(ArcadePlayerWeaponManager weaponManager, Weapon id)
	{
		this.weaponManager = weaponManager;
		player = weaponManager.GetComponent<ArcadePlayerController>();
		this.id = id;
		firing = new FiringSwitches();
		StartCoroutines();
		player.OnReviveEvent += OnRevive;
	}

	private void OnDealDamage(float damage, DamageReceiver receiver, DamageDealer dealer)
	{
		if (!(player == null) && !player.IsDead && !(player.stats == null) && receiver.enabled)
		{
			player.stats.OnDealDamage(damage, dealer);
		}
	}

	private void OnRevive(Vector3 pos)
	{
		StartCoroutines();
	}

	private void StartCoroutines()
	{
		StopAllCoroutines();
		StartCoroutine(fireWeapon_cr(Mode.Basic));
		StartCoroutine(fireWeapon_cr(Mode.Ex));
	}

	private void OnEnable()
	{
		StartCoroutines();
	}

	public virtual void BeginBasic()
	{
		beginFiring(Mode.Basic);
	}

	public virtual void EndBasic()
	{
		endFiring(Mode.Basic);
	}

	protected virtual AbstractProjectile fireBasic()
	{
		AbstractProjectile abstractProjectile = fireProjectile(Mode.Basic);
		abstractProjectile.OnDealDamageEvent += OnDealDamage;
		return abstractProjectile;
	}

	public virtual void BeginEx()
	{
		beginFiring(Mode.Ex);
	}

	public virtual void EndEx()
	{
		endFiring(Mode.Ex);
	}

	protected virtual AbstractProjectile fireEx()
	{
		return fireProjectile(Mode.Ex);
	}

	protected virtual void beginFiring(Mode mode)
	{
		weaponManager.IsShooting = true;
		firing.Set(mode, val: true);
	}

	protected virtual AbstractProjectile fireProjectile(Mode mode)
	{
		Vector2 position = weaponManager.GetBulletPosition();
		if (mode == Mode.Ex)
		{
			position = weaponManager.ExPosition;
		}
		if (mode == Mode.Basic)
		{
			weaponManager.UpdateAim();
		}
		if (GetProjectile(mode) == null)
		{
			return null;
		}
		if (GetEffect(mode) != null && mode != 0 && mode == Mode.Ex)
		{
			weaponManager.CreateExDust(GetEffect(mode));
		}
		weaponManager.UpdateAim();
		return GetProjectile(mode).Create(position, weaponManager.GetBulletRotation(), weaponManager.GetBulletScale());
	}

	protected virtual void endFiring(Mode mode)
	{
		weaponManager.IsShooting = false;
		firing.Set(mode, val: false);
	}

	private AbstractProjectile GetProjectile(Mode mode)
	{
		if (mode == Mode.Basic || mode != Mode.Ex)
		{
			return basicPrefab;
		}
		return exPrefab;
	}

	private Effect GetEffect(Mode mode)
	{
		if (mode == Mode.Basic || mode != Mode.Ex)
		{
			return basicEffectPrefab;
		}
		return exEffectPrefab;
	}

	private FireProjectileDelegate getFiringMethod(Mode mode)
	{
		return mode switch
		{
			Mode.Ex => fireEx, 
			_ => fireBasic, 
		};
	}

	protected virtual IEnumerator fireWeapon_cr(Mode mode)
	{
		float t = 0f;
		WaitForFixedUpdate waitInstruction = new WaitForFixedUpdate();
		while (true)
		{
			yield return waitInstruction;
			if (player.motor.Dashing)
			{
				continue;
			}
			if (t < rapidFireRate)
			{
				t += CupheadTime.FixedDelta;
			}
			else if (firing.Get(mode) && weaponManager.IsShooting)
			{
				weaponManager.TriggerWeaponFire();
				getFiringMethod(mode)();
				if (mode == Mode.Ex || !rapidFire)
				{
					firing.Set(mode, val: false);
					weaponManager.IsShooting = false;
				}
				t = 0f;
			}
		}
	}
}
