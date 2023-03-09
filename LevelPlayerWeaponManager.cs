using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class LevelPlayerWeaponManager : AbstractLevelPlayerComponent
{
	public enum Pose
	{
		Forward,
		Forward_R,
		Up,
		Up_D,
		Up_D_R,
		Down,
		Down_D,
		Duck,
		Jump,
		Ex
	}

	public delegate void OnWeaponChangeHandler(Weapon weapon);

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ProjectilePosition
	{
		public static Vector2 Get(Pose pose, Pose direction, bool isChalice)
		{
			return pose switch
			{
				Pose.Jump => direction switch
				{
					Pose.Forward => (!isChalice) ? new Vector2(78f, 64f) : new Vector2(85f, 70f), 
					Pose.Up => (!isChalice) ? new Vector2(0f, 158f) : new Vector2(22f, 162f), 
					Pose.Up_D => (!isChalice) ? new Vector2(71f, 107f) : new Vector2(66f, 117f), 
					Pose.Down => (!isChalice) ? new Vector2(0f, -11f) : new Vector2(22f, 2f), 
					Pose.Down_D => (!isChalice) ? new Vector2(71f, 20f) : new Vector2(66f, 31f), 
					_ => (!isChalice) ? new Vector2(0f, 0f) : new Vector2(0f, 0f), 
				}, 
				Pose.Forward => (!isChalice) ? new Vector2(78f, 64f) : new Vector2(100f, 63f), 
				Pose.Forward_R => (!isChalice) ? new Vector2(70f, 46f) : new Vector2(62f, 51f), 
				Pose.Up => (!isChalice) ? new Vector2(27f, 158f) : new Vector2(32f, 162f), 
				Pose.Up_D => (!isChalice) ? new Vector2(71f, 107f) : new Vector2(78f, 112f), 
				Pose.Up_D_R => (!isChalice) ? new Vector2(73f, 107f) : new Vector2(66f, 108f), 
				Pose.Down => (!isChalice) ? new Vector2(28f, -11f) : new Vector2(32f, -6f), 
				Pose.Down_D => (!isChalice) ? new Vector2(71f, 20f) : new Vector2(78f, 17f), 
				Pose.Duck => (!isChalice) ? new Vector2(102f, 24f) : new Vector2(103f, 33f), 
				_ => (!isChalice) ? new Vector2(0f, 54f) : new Vector2(0f, 54f), 
			};
		}
	}

	public class WeaponState
	{
		public enum State
		{
			Ready,
			Firing,
			Fired,
			Ended
		}

		public State state;

		public bool firing;

		public bool holding;
	}

	public class ExState
	{
		public bool airAble = true;

		public bool firing;

		public bool Able => airAble && !firing;
	}

	[Serializable]
	public class WeaponPrefabs
	{
		[SerializeField]
		private WeaponPeashot peashot;

		[SerializeField]
		private WeaponSpread spread;

		[SerializeField]
		private WeaponArc arc;

		[SerializeField]
		private WeaponHoming homing;

		[SerializeField]
		private WeaponExploder exploder;

		[SerializeField]
		private WeaponCharge charge;

		[SerializeField]
		private WeaponBoomerang boomerang;

		[SerializeField]
		private WeaponBouncer bouncer;

		[SerializeField]
		private WeaponWideShot wideShot;

		[SerializeField]
		private WeaponUpshot upShot;

		[SerializeField]
		private WeaponCrackshot crackshot;

		private Transform root;

		private LevelPlayerWeaponManager weaponManager;

		private Dictionary<Weapon, AbstractLevelWeapon> weapons;

		public void Init(LevelPlayerWeaponManager weaponManager, Transform root)
		{
			this.weaponManager = weaponManager;
			this.root = root;
			weapons = new Dictionary<Weapon, AbstractLevelWeapon>();
			Weapon[] values = EnumUtils.GetValues<Weapon>();
			for (int i = 0; i < values.Length; i++)
			{
				Weapon id = values[i];
				if (id.ToString().ToLower().Contains("level"))
				{
					InitWeapon(id);
				}
			}
		}

		public AbstractLevelWeapon GetWeapon(Weapon weapon)
		{
			return weapons[weapon];
		}

		private void InitWeapon(Weapon id)
		{
			AbstractLevelWeapon abstractLevelWeapon = null;
			switch (id)
			{
			default:
				return;
			case Weapon.level_weapon_peashot:
				abstractLevelWeapon = peashot;
				break;
			case Weapon.level_weapon_spreadshot:
				abstractLevelWeapon = spread;
				break;
			case Weapon.level_weapon_arc:
				abstractLevelWeapon = arc;
				break;
			case Weapon.level_weapon_homing:
				abstractLevelWeapon = homing;
				break;
			case Weapon.level_weapon_exploder:
				abstractLevelWeapon = exploder;
				break;
			case Weapon.level_weapon_charge:
				abstractLevelWeapon = charge;
				break;
			case Weapon.level_weapon_boomerang:
				abstractLevelWeapon = boomerang;
				break;
			case Weapon.level_weapon_bouncer:
				abstractLevelWeapon = bouncer;
				break;
			case Weapon.level_weapon_wide_shot:
				abstractLevelWeapon = wideShot;
				break;
			case Weapon.level_weapon_upshot:
				abstractLevelWeapon = upShot;
				break;
			case Weapon.level_weapon_crackshot:
				abstractLevelWeapon = crackshot;
				break;
			}
			if (!(abstractLevelWeapon == null))
			{
				AbstractLevelWeapon abstractLevelWeapon2 = UnityEngine.Object.Instantiate(abstractLevelWeapon);
				abstractLevelWeapon2.transform.parent = root.transform;
				abstractLevelWeapon2.Initialize(weaponManager, id);
				abstractLevelWeapon2.name = abstractLevelWeapon2.name.Replace("(Clone)", string.Empty);
				weapons[id] = abstractLevelWeapon2;
			}
		}

		public void OnDestroy()
		{
			peashot = null;
			spread = null;
			arc = null;
			homing = null;
			exploder = null;
			charge = null;
			boomerang = null;
			bouncer = null;
			wideShot = null;
		}
	}

	[Serializable]
	public class SuperPrefabs
	{
		[SerializeField]
		private AbstractPlayerSuper beam;

		[SerializeField]
		private AbstractPlayerSuper ghost;

		[SerializeField]
		private AbstractPlayerSuper invincible;

		[SerializeField]
		private AbstractPlayerSuper chaliceIII;

		[SerializeField]
		private AbstractPlayerSuper chaliceVertBeam;

		[SerializeField]
		private AbstractPlayerSuper chaliceShield;

		public void Init(LevelPlayerController player)
		{
		}

		public AbstractPlayerSuper GetPrefab(Super super)
		{
			return super switch
			{
				Super.level_super_ghost => ghost, 
				Super.level_super_invincible => invincible, 
				Super.level_super_chalice_iii => chaliceIII, 
				Super.level_super_chalice_vert_beam => chaliceVertBeam, 
				Super.level_super_chalice_shield => chaliceShield, 
				_ => beam, 
			};
		}

		public void OnDestroy()
		{
			beam = null;
			ghost = null;
			invincible = null;
		}
	}

	[SerializeField]
	private WeaponPrefabs weaponPrefabs;

	[SerializeField]
	private SuperPrefabs superPrefabs;

	[Space(10f)]
	[SerializeField]
	private Effect exDustEffect;

	[SerializeField]
	private Effect exChargeEffect;

	[SerializeField]
	private Transform exRoot;

	private Weapon currentWeapon = Weapon.None;

	private Pose currentPose;

	private WeaponState basic;

	private ExState ex;

	private Transform weaponsRoot;

	private Transform aim;

	public bool allowInput = true;

	private bool allowSuper = true;

	public bool IsShooting { get; set; }

	public bool FreezePosition { get; set; }

	public Vector2 ExPosition => exRoot.position;

	public AbstractLevelWeapon CurrentWeapon => weaponPrefabs.GetWeapon(currentWeapon);

	public AbstractPlayerSuper activeSuper { get; private set; }

	public event OnWeaponChangeHandler OnWeaponChangeEvent;

	public event Action OnBasicStart;

	public event Action OnExStart;

	public event Action OnSuperStart;

	public event Action OnExFire;

	public event Action OnWeaponFire;

	public event Action OnExEnd;

	public event Action OnSuperEnd;

	public event Action OnSuperInterrupt;

	protected override void OnAwake()
	{
		base.OnAwake();
		base.basePlayer.damageReceiver.OnDamageTaken += OnDamageTaken;
		Level.Current.OnLevelEndEvent += OnLevelEnd;
		base.player.motor.OnDashStartEvent += OnDash;
		basic = new WeaponState();
		ex = new ExState();
		weaponsRoot = new GameObject("Weapons").transform;
		weaponsRoot.parent = base.transform;
		weaponsRoot.localPosition = Vector3.zero;
		weaponsRoot.localEulerAngles = Vector3.zero;
		weaponsRoot.localScale = Vector3.one;
		aim = new GameObject("Aim").transform;
		aim.SetParent(base.transform);
		aim.ResetLocalTransforms();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Level.Current != null)
		{
			Level.Current.OnLevelEndEvent -= OnLevelEnd;
		}
		if (base.player != null && base.player.motor != null)
		{
			base.player.motor.OnDashStartEvent -= OnDash;
		}
		weaponPrefabs.OnDestroy();
		superPrefabs.OnDestroy();
		exDustEffect = null;
		exChargeEffect = null;
		WORKAROUND_NullifyFields();
	}

	private void FixedUpdate()
	{
		if (base.player.levelStarted && allowInput)
		{
			HandleWeaponSwitch();
			HandleWeaponFiring();
			if (base.player.motor.Grounded)
			{
				ex.airAble = true;
			}
		}
	}

	private void OnEnable()
	{
		EnableInput();
	}

	public void ForceStopWeaponFiring()
	{
		EndBasic();
	}

	public override void OnLevelEnd()
	{
		EndBasic();
		base.OnLevelEnd();
	}

	private void OnDash()
	{
		EndBasic();
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (ex.firing && !base.player.stats.SuperInvincible)
		{
			ex.firing = false;
		}
	}

	public void AbortEX()
	{
		ex.firing = false;
	}

	public void ParrySuccess()
	{
	}

	public void LevelInit(PlayerId id)
	{
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(base.player.id);
		if (playerLoadout.charm == Charm.charm_curse && base.player.stats.CurseCharmLevel > -1)
		{
			int[] availableWeaponIDs = WeaponProperties.CharmCurse.availableWeaponIDs;
			currentWeapon = (Weapon)availableWeaponIDs[UnityEngine.Random.Range(0, availableWeaponIDs.Length)];
		}
		else
		{
			currentWeapon = playerLoadout.primaryWeapon;
		}
		weaponPrefabs.Init(this, weaponsRoot);
		superPrefabs.Init(base.player);
	}

	public void OnDeath()
	{
		EndBasic();
	}

	public void EnableInput()
	{
		allowInput = true;
	}

	public void DisableInput()
	{
		allowInput = false;
		IsShooting = false;
	}

	public void EnableSuper(bool value)
	{
		allowSuper = value;
	}

	private void _WeaponFireEx()
	{
		FireEx();
	}

	private void _WeaponEndEx()
	{
		EndEx();
	}

	private void StartBasic()
	{
		UpdateAim();
		if (!Level.IsChessBoss)
		{
			weaponPrefabs.GetWeapon(currentWeapon).BeginBasic();
		}
		if (this.OnBasicStart != null)
		{
			this.OnBasicStart();
		}
	}

	private void EndBasic()
	{
		if (currentWeapon != Weapon.None)
		{
			weaponPrefabs.GetWeapon(currentWeapon).EndBasic();
			basic.firing = false;
		}
	}

	public void TriggerWeaponFire()
	{
		this.OnWeaponFire();
	}

	public void InterruptSuper()
	{
		if (this.OnSuperInterrupt != null)
		{
			this.OnSuperInterrupt();
		}
	}

	private void StartEx()
	{
		EndBasic();
		UpdateAim();
		ex.firing = true;
		ex.airAble = false;
		base.player.stats.OnEx();
		exChargeEffect.Create(base.player.center);
		if (this.OnExStart != null)
		{
			this.OnExStart();
		}
	}

	private void FireEx()
	{
		weaponPrefabs.GetWeapon(currentWeapon).BeginEx();
		if (this.OnExFire != null)
		{
			this.OnExFire();
		}
	}

	private void EndEx()
	{
		ex.firing = false;
		if (this.OnExEnd != null)
		{
			this.OnExEnd();
		}
	}

	public void CreateExDust(Effect starsEffect)
	{
		Transform transform = new GameObject("ExRootTemp").transform;
		transform.ResetLocalTransforms();
		transform.position = exRoot.position;
		Vector2 vector = transform.position;
		if (starsEffect != null)
		{
			Transform transform2 = starsEffect.Create(vector).transform;
			transform2.SetParent(transform);
			transform2.ResetLocalTransforms();
			transform2.SetParent(null);
			transform2.SetEulerAngles(0f, 0f, GetBulletRotation());
			transform2.localScale = GetBulletScale();
			transform2.AddPositionForward2D(-100f);
		}
		if (exDustEffect != null)
		{
			Transform transform3 = exDustEffect.Create(vector).transform;
			transform3.SetParent(transform);
			transform3.ResetLocalTransforms();
			transform3.SetParent(null);
			transform3.SetEulerAngles(0f, 0f, GetBulletRotation());
			transform3.localScale = GetBulletScale();
			transform3.AddPositionForward2D(-15f);
		}
		UnityEngine.Object.Destroy(transform.gameObject);
	}

	private void StartSuper()
	{
		EndBasic();
		UpdateAim();
		base.player.stats.OnSuper();
		Super super = PlayerData.Data.Loadouts.GetPlayerLoadout(base.player.id).super;
		if (base.player.stats.isChalice)
		{
			switch (super)
			{
			case Super.level_super_beam:
				super = Super.level_super_chalice_vert_beam;
				break;
			case Super.level_super_ghost:
				super = Super.level_super_chalice_iii;
				break;
			case Super.level_super_invincible:
				super = Super.level_super_chalice_shield;
				break;
			}
		}
		AbstractPlayerSuper abstractPlayerSuper = superPrefabs.GetPrefab(super).Create(base.player);
		abstractPlayerSuper.OnEndedEvent += EndSuperFromSuper;
		activeSuper = abstractPlayerSuper;
		if (this.OnSuperStart != null)
		{
			this.OnSuperStart();
		}
	}

	private void EndSuper()
	{
		if (this.OnSuperEnd != null)
		{
			this.OnSuperEnd();
		}
	}

	public void EndSuperFromSuper()
	{
		EndSuper();
	}

	private void HandleWeaponFiring()
	{
		if (base.player.motor.Dashing || base.player.motor.IsHit)
		{
			return;
		}
		if (base.player.input.actions.GetButtonDown(4) || base.player.motor.HasBufferedInput(LevelPlayerMotor.BufferedInput.Super) || (base.player.stats.Loadout.charm == Charm.charm_EX && base.player.input.actions.GetButton(3) && !ex.firing))
		{
			base.player.motor.ClearBufferedInput();
			Super super = PlayerData.Data.Loadouts.GetPlayerLoadout(base.player.id).super;
			if (base.player.stats.SuperMeter >= base.player.stats.SuperMeterMax && super != Super.None && !base.player.stats.ChaliceShieldOn && allowSuper && base.player.stats.Loadout.charm != Charm.charm_EX)
			{
				StartSuper();
				return;
			}
			if (base.player.stats.CanUseEx && ex.Able)
			{
				StartEx();
				return;
			}
		}
		if (ex.firing || base.player.stats.Loadout.charm == Charm.charm_EX)
		{
			return;
		}
		if (basic.firing != base.player.input.actions.GetButton(3))
		{
			if (base.player.input.actions.GetButton(3))
			{
				if (PlayerData.Data.Loadouts.GetPlayerLoadout(base.player.id).charm == Charm.charm_curse && base.player.stats.CurseCharmLevel > -1)
				{
					int[] availableWeaponIDs = WeaponProperties.CharmCurse.availableWeaponIDs;
					int num;
					for (num = (int)currentWeapon; num == (int)currentWeapon; num = availableWeaponIDs[UnityEngine.Random.Range(0, availableWeaponIDs.Length)])
					{
					}
					SwitchWeapon((Weapon)num);
				}
				else
				{
					StartBasic();
				}
			}
			else
			{
				EndBasic();
			}
		}
		basic.firing = base.player.input.actions.GetButton(3);
	}

	public void ResetEx()
	{
		ex.firing = false;
	}

	private void HandleWeaponSwitch()
	{
		if (!base.player.input.actions.GetButtonDown(5))
		{
			return;
		}
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(base.player.id);
		if ((playerLoadout.charm != Charm.charm_curse || base.player.stats.CurseCharmLevel <= -1) && playerLoadout.secondaryWeapon != Weapon.None)
		{
			if (currentWeapon == playerLoadout.primaryWeapon)
			{
				SwitchWeapon(playerLoadout.secondaryWeapon);
			}
			else
			{
				SwitchWeapon(playerLoadout.primaryWeapon);
			}
		}
	}

	private void SwitchWeapon(Weapon weapon)
	{
		if (weapon != Weapon.None)
		{
			weaponPrefabs.GetWeapon(currentWeapon).EndBasic();
			weaponPrefabs.GetWeapon(currentWeapon).EndEx();
			currentWeapon = weapon;
			if (this.OnWeaponChangeEvent != null)
			{
				this.OnWeaponChangeEvent(weapon);
			}
			if (base.player.input.actions.GetButton(3))
			{
				StartBasic();
			}
		}
	}

	private Pose GetCurrentPose()
	{
		if (ex.firing)
		{
			return Pose.Ex;
		}
		if (base.player.motor.Ducking)
		{
			return Pose.Duck;
		}
		if (!base.player.motor.Grounded)
		{
			return Pose.Jump;
		}
		if (base.player.motor.Locked)
		{
			if ((int)base.player.motor.LookDirection.y > 0)
			{
				if ((int)base.player.motor.LookDirection.x != 0)
				{
					return Pose.Up_D;
				}
				return Pose.Up;
			}
			if ((int)base.player.motor.LookDirection.y < 0)
			{
				if ((int)base.player.motor.LookDirection.x != 0)
				{
					return Pose.Down_D;
				}
				return Pose.Down;
			}
		}
		else
		{
			if ((int)base.player.motor.LookDirection.x != 0)
			{
				if ((int)base.player.motor.LookDirection.y > 0)
				{
					return Pose.Up_D_R;
				}
				return Pose.Forward_R;
			}
			if ((int)base.player.motor.LookDirection.y < 0)
			{
				return Pose.Duck;
			}
			if ((int)base.player.motor.LookDirection.y > 0)
			{
				return Pose.Up;
			}
		}
		return Pose.Forward;
	}

	public Pose GetDirectionPose()
	{
		if (base.player.motor.Dashing)
		{
			return Pose.Forward;
		}
		if ((int)base.player.motor.LookDirection.y > 0)
		{
			if ((int)base.player.motor.LookDirection.x != 0)
			{
				return Pose.Up_D;
			}
			return Pose.Up;
		}
		if ((int)base.player.motor.LookDirection.y < 0)
		{
			if ((int)base.player.motor.LookDirection.x != 0)
			{
				return Pose.Down_D;
			}
			return Pose.Down;
		}
		return Pose.Forward;
	}

	public void UpdateAim()
	{
		float num = 0f;
		Pose directionPose = GetDirectionPose();
		num = ((base.transform.localScale.x > 0f) ? (directionPose switch
		{
			Pose.Up => 90f, 
			Pose.Down => -90f, 
			Pose.Up_D => 45f, 
			Pose.Down_D => -45f, 
			_ => 0f, 
		}) : (directionPose switch
		{
			Pose.Up => 90f, 
			Pose.Down => -90f, 
			Pose.Up_D => 135f, 
			Pose.Down_D => -135f, 
			_ => 180f, 
		}));
		num *= base.player.motor.GravityReversalMultiplier;
		aim.SetEulerAngles(0f, 0f, num);
	}

	public Vector2 GetBulletPosition()
	{
		Vector2 vector = base.transform.position;
		Vector2 vector2 = ProjectilePosition.Get(GetCurrentPose(), GetDirectionPose(), base.player.stats.isChalice);
		return new Vector2(vector.x + vector2.x * (float)base.player.motor.TrueLookDirection.x, vector.y + vector2.y * base.player.motor.GravityReversalMultiplier);
	}

	public float GetBulletRotation()
	{
		Pose pose = GetCurrentPose();
		if (pose == Pose.Duck)
		{
			if (base.transform.localScale.x < 0f)
			{
				return 180f;
			}
			return 0f;
		}
		return aim.eulerAngles.z;
	}

	public Vector3 GetBulletScale()
	{
		return new Vector3(1f, base.player.motor.TrueLookDirection.x, 1f);
	}

	private void WORKAROUND_NullifyFields()
	{
		activeSuper = null;
		weaponPrefabs = null;
		superPrefabs = null;
		exDustEffect = null;
		exChargeEffect = null;
		exRoot = null;
		this.OnWeaponChangeEvent = null;
		this.OnBasicStart = null;
		this.OnExStart = null;
		this.OnSuperStart = null;
		this.OnExFire = null;
		this.OnWeaponFire = null;
		this.OnExEnd = null;
		this.OnSuperEnd = null;
		this.OnSuperInterrupt = null;
		basic = null;
		ex = null;
		weaponsRoot = null;
		aim = null;
	}
}
