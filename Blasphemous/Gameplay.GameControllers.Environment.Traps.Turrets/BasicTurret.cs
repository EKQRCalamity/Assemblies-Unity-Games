using System;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Sirenix.OdinInspector;
using Tools.Level;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.Turrets;

public class BasicTurret : MonoBehaviour, IProjectileAttack, IActionable
{
	public enum TURRET_DIRECTION
	{
		LEFT,
		RIGHT,
		UP,
		DOWN
	}

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public bool isActivated = true;

	[BoxGroup("Design Settings", true, false, 0)]
	public int ProjectileDamageAmount;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Tooltip("Seconds between each shot")]
	[SuffixLabel("seconds", false)]
	private float fireRate = 0.5f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private TURRET_DIRECTION direction = TURRET_DIRECTION.RIGHT;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private StraightProjectile projectilePrefab;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Vector2 projectileLaunchOffset;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float projectileSpeed;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float destructionDistance;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Tooltip("Initial delay before firing the first bullet")]
	[SuffixLabel("seconds", false)]
	private float startDelay;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Tooltip("If true the first shot comes right after the delay. If not, it will also wait its fireRate before the first shot is fired")]
	private bool shootOnceAfterDelay;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private int bulletPoolSize;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Tooltip("If checked, the turret will fire towards it's local right (x) vector, shown as red in Unity")]
	private bool useTurretRightAsDirection;

	public bool shootUsingAnimation = true;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string shootSound;

	[SerializeField]
	[FoldoutGroup("Debug settings", 0)]
	private bool showGizmos = true;

	[SerializeField]
	[FoldoutGroup("Debug settings", 0)]
	private bool showLine = true;

	[SerializeField]
	[FoldoutGroup("Debug settings", 0)]
	private int projectilesShown = 3;

	public Animator animator;

	private SpriteRenderer _spriteRenderer;

	private float _shootCounter;

	private float _delayCounter;

	public Action<Projectile> onProjectileFired;

	private EventInstance _shootAudioInstance;

	private bool isLocked;

	public bool Locked
	{
		get
		{
			return isLocked;
		}
		set
		{
			isLocked = value;
		}
	}

	private void Start()
	{
		PoolManager.Instance.CreatePool(projectilePrefab.gameObject, 30);
		_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_delayCounter = startDelay;
	}

	private void Update()
	{
		if (!isActivated)
		{
			return;
		}
		if (_delayCounter > 0f)
		{
			_delayCounter -= Time.deltaTime;
			if (_delayCounter < 0f && shootOnceAfterDelay)
			{
				_shootCounter = fireRate;
			}
		}
		else
		{
			CheckShoot();
		}
	}

	private void CheckShoot()
	{
		_shootCounter += Time.deltaTime;
		if (_shootCounter > fireRate)
		{
			_shootCounter = 0f;
			Shoot();
		}
	}

	public void ForceShoot()
	{
		_shootCounter = fireRate + 1f;
	}

	protected virtual void Shoot()
	{
		if (shootUsingAnimation)
		{
			animator.SetTrigger("shoot");
			PlayShoot();
		}
		else
		{
			LaunchProjectile();
		}
	}

	public void SetFireParameters(float _speed, float _fireRate)
	{
		projectileSpeed = _speed;
		fireRate = _fireRate;
	}

	public void LaunchProjectile()
	{
		Vector2 vector = GetDirectionFromEnum(direction);
		if (useTurretRightAsDirection)
		{
			vector = base.transform.right;
		}
		StraightProjectile component = PoolManager.Instance.ReuseObject(projectilePrefab.gameObject, base.transform.position + (Vector3)projectileLaunchOffset, Quaternion.identity).GameObject.GetComponent<StraightProjectile>();
		SetProjectileWeaponDamage(component, ProjectileDamageAmount);
		component.Init(vector, projectileSpeed);
		component.timeToLive = destructionDistance / projectileSpeed;
		component.ResetTTL();
		if (onProjectileFired != null)
		{
			onProjectileFired(component);
		}
	}

	private Vector2 GetDirectionFromEnum(TURRET_DIRECTION direction)
	{
		return direction switch
		{
			TURRET_DIRECTION.LEFT => Vector2.left, 
			TURRET_DIRECTION.RIGHT => Vector2.right, 
			TURRET_DIRECTION.UP => Vector2.up, 
			TURRET_DIRECTION.DOWN => Vector2.down, 
			_ => Vector2.zero, 
		};
	}

	private void OnDrawGizmos()
	{
		if (showGizmos)
		{
			Gizmos.color = Color.red;
			Vector3 vector = base.transform.position + (Vector3)projectileLaunchOffset;
			float num = fireRate * projectileSpeed;
			Gizmos.DrawSphere(vector, 0.25f);
			Vector3 vector2 = GetDirectionFromEnum(direction);
			Vector3 vector3 = vector + vector2 * destructionDistance;
			for (int i = 1; i <= projectilesShown; i++)
			{
				Gizmos.DrawSphere(vector + vector2 * num * i, 0.1f);
			}
			Gizmos.color = Color.yellow;
			if (showLine)
			{
				Gizmos.DrawLine(vector, vector3);
			}
			Gizmos.DrawSphere(vector3, 0.25f);
		}
	}

	public void PlayShoot()
	{
		if (_spriteRenderer.IsVisibleFrom(UnityEngine.Camera.main))
		{
			if (_shootAudioInstance.isValid())
			{
				_shootAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}
			_shootAudioInstance = Core.Audio.CreateEvent(shootSound);
			_shootAudioInstance.start();
		}
	}

	public void SetProjectileWeaponDamage(int damage)
	{
		if (damage > 0)
		{
			ProjectileDamageAmount = damage;
		}
	}

	public void SetProjectileWeaponDamage(Projectile projectile, int damage)
	{
		SetProjectileWeaponDamage(damage);
		if (damage > 0 && !(projectile == null))
		{
			ProjectileWeapon componentInChildren = projectile.GetComponentInChildren<ProjectileWeapon>();
			if ((bool)componentInChildren)
			{
				componentInChildren.SetDamage(damage);
			}
		}
	}

	public void Use()
	{
		isActivated = !isActivated;
	}
}
