using System.Collections.Generic;
using FMODUnity;
using Framework.Managers;
using Framework.Pooling;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossInstantProjectileAttack : EnemyAttack, IDirectAttack, ISpawnerAttack
{
	public LayerMask collisionMask;

	public LayerMask damageMask;

	private Vector3 _targetPoint;

	private Coroutine _currentCoroutine;

	public Vector2 shootOrigin;

	[FoldoutGroup("Instantiations", 0)]
	public GameObject instantiateOnBegin;

	[FoldoutGroup("Instantiations", 0)]
	public GameObject instantiateOnEnd;

	[FoldoutGroup("Instantiations", 0)]
	public GameObject instantiateOnHit;

	[FoldoutGroup("Instantiations", 0)]
	public List<DashAttackInstantiations> objectsToInstantiate;

	public float areaWidth = 3f;

	public float maxRange = 10f;

	[FoldoutGroup("Damage", 0)]
	public bool dealsDamage = true;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public bool unavoidable;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public bool canBeParried;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public int damage;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public AnimationCurve slowTimeCurve;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public float slowTimeDuration;

	[EventRef]
	public string shotSound;

	[FoldoutGroup("Instantiations", 0)]
	public TileableBeamLauncher beamLauncherToUse;

	[FoldoutGroup("Instantiations", 0)]
	public int beamLauncherPoolSize = 1;

	[FoldoutGroup("Instantiations", 0)]
	public int itemsPoolSize = 1;

	[FoldoutGroup("Instantiations", 0)]
	public int onHitVfxPoolSize = 1;

	private List<DashAttackInstantiations> alreadyInstantiated;

	private Hit _hit;

	private Vector2 _lastAttackOrigin;

	private Vector2 _lastAttackDir;

	private bool _wasBlocked;

	private Vector2 _blockPoint;

	private float _hitStrength = 1f;

	public AttackArea AttackArea { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		alreadyInstantiated = new List<DashAttackInstantiations>();
		CreateHit();
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (beamLauncherToUse != null)
		{
			PoolManager.Instance.CreatePool(beamLauncherToUse.gameObject, beamLauncherPoolSize);
		}
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			if (item.prefabToInstantiate.GetComponent<PoolObject>() != null)
			{
				PoolManager.Instance.CreatePool(item.prefabToInstantiate, itemsPoolSize);
			}
		}
		if (instantiateOnHit != null)
		{
			PoolManager.Instance.CreatePool(instantiateOnHit, onHitVfxPoolSize);
		}
	}

	public void CreateHit()
	{
		if (dealsDamage)
		{
			_hit = new Hit
			{
				AttackingEntity = base.EntityOwner.gameObject,
				DamageType = DamageType,
				Force = Force,
				DamageAmount = (float)damage * _hitStrength,
				DamageElement = DamageElement,
				HitSoundId = HitSound,
				Unnavoidable = unavoidable,
				Unparriable = !canBeParried,
				Unblockable = !canBeParried,
				forceGuardslide = canBeParried
			};
		}
	}

	public void SetDamage(int dmg)
	{
		damage = dmg;
		CreateHit();
	}

	public void SetDamageStrength(float strength)
	{
		_hitStrength = strength;
		CreateHit();
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
	}

	public List<GameObject> Shoot(Vector2 origin, Vector2 dir)
	{
		List<GameObject> list = new List<GameObject>();
		alreadyInstantiated.Clear();
		if (shotSound != string.Empty)
		{
			Core.Audio.PlaySfx(shotSound);
		}
		RaycastHit2D[] array = new RaycastHit2D[1];
		if (Physics2D.LinecastNonAlloc(origin, origin + dir * maxRange, array, collisionMask) > 0)
		{
			Debug.DrawLine(array[0].point, array[0].point + Vector2.up * 0.25f, Color.red, 1f);
			_targetPoint = array[0].point;
		}
		else
		{
			Debug.DrawLine(origin, origin + dir * 10f, Color.red, 1f);
			_targetPoint = origin + dir * maxRange;
		}
		Debug.DrawLine(origin, _targetPoint, Color.green, 1f);
		if (dealsDamage)
		{
			_lastAttackDir = (Vector2)_targetPoint - origin;
			_lastAttackOrigin = origin;
			_wasBlocked = false;
			DamageArea(origin, _targetPoint, areaWidth);
			return DrawAfterDamageArea(origin, dir);
		}
		return DrawCompleteBeam(origin, dir);
	}

	private List<GameObject> DrawAfterDamageArea(Vector2 origin, Vector2 dir)
	{
		List<GameObject> list = new List<GameObject>();
		if (_wasBlocked)
		{
			Vector2 vector = _blockPoint - origin;
			Vector2 vector2 = -_lastAttackDir;
			vector2.y *= -1f;
			RaycastHit2D[] array = new RaycastHit2D[1];
			if (Physics2D.LinecastNonAlloc(_blockPoint, _blockPoint + vector2 * maxRange, array, collisionMask) > 0)
			{
				Debug.DrawLine(array[0].point, array[0].point + Vector2.up * 0.25f, Color.red, 1f);
				_targetPoint = array[0].point;
			}
			else
			{
				Debug.DrawLine(origin, origin + vector2 * 10f, Color.red, 1f);
				_targetPoint = origin + vector2 * maxRange;
			}
			if (beamLauncherToUse != null)
			{
				TileableBeamLauncher component = PoolManager.Instance.ReuseObject(beamLauncherToUse.gameObject, Vector3.zero, Quaternion.identity).GameObject.GetComponent<TileableBeamLauncher>();
				component.transform.position = origin;
				component.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * 57.29578f);
				component.TriggerBeamBodyAnim();
				component.maxRange = vector.magnitude;
				Debug.DrawRay(_blockPoint, vector2, Color.magenta, 10f);
				TileableBeamLauncher component2 = PoolManager.Instance.ReuseObject(beamLauncherToUse.gameObject, Vector3.zero, Quaternion.identity).GameObject.GetComponent<TileableBeamLauncher>();
				component2.maxRange = array[0].distance;
				component2.transform.position = _blockPoint;
				component2.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(vector2.y, vector2.x) * 57.29578f);
				component2.TriggerBeamBodyAnim();
			}
			{
				foreach (DashAttackInstantiations item in objectsToInstantiate)
				{
					Vector2 p = Vector2.Lerp(_blockPoint, _targetPoint, item.dashMoment);
					list.Add(InstantiateObject(item, p, vector2));
				}
				return list;
			}
		}
		return DrawCompleteBeam(origin, dir);
	}

	private List<GameObject> DrawCompleteBeam(Vector2 origin, Vector2 dir)
	{
		List<GameObject> list = new List<GameObject>();
		Vector2 dir2 = (Vector2)_targetPoint - origin;
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			Vector2 p = Vector2.Lerp(origin, _targetPoint, item.dashMoment);
			list.Add(InstantiateObject(item, p, dir2));
		}
		if (beamLauncherToUse != null)
		{
			TileableBeamLauncher component = PoolManager.Instance.ReuseObject(beamLauncherToUse.gameObject, Vector3.zero, Quaternion.identity).GameObject.GetComponent<TileableBeamLauncher>();
			component.transform.position = origin;
			component.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * 57.29578f);
			component.TriggerBeamBodyAnim();
			component.maxRange = dir2.magnitude;
		}
		return list;
	}

	private void DamageArea(Vector2 origin, Vector2 end, float width)
	{
		RaycastHit2D[] array = new RaycastHit2D[20];
		Vector2 vector = end - origin;
		Vector2 normalized = new Vector2(0f - vector.y, vector.x).normalized;
		Vector2 vector2 = origin + vector.normalized * 0.1f;
		DrawDebugCross(vector2, Color.cyan, 2f);
		Vector2 size = new Vector2(width, 0.1f);
		Vector2 vector3 = origin + normalized * width / 2f;
		Vector2 vector4 = origin - normalized * width / 2f;
		Debug.DrawLine(vector3, vector3 + vector, Color.green, 1f);
		Debug.DrawLine(vector4, vector4 + vector, Color.green, 1f);
		float angle = Mathf.Atan2(vector.y, vector.x);
		int num = Physics2D.BoxCastNonAlloc(vector2, size, angle, vector, array, vector.magnitude, damageMask);
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			if ((bool)instantiateOnHit)
			{
				float num2 = Vector2.Distance(array[i].point, origin);
				Vector2 vector5 = origin + vector.normalized * (0.5f + num2);
				GameObject gameObject = PoolManager.Instance.ReuseObject(instantiateOnHit, vector5, Quaternion.identity).GameObject;
				gameObject.GetComponentInChildren<SpriteRenderer>().flipX = vector.x < 0f;
			}
			if (array[i].collider.CompareTag("Penitent"))
			{
				if (canBeParried)
				{
					_hit.OnGuardCallback = OnPenitentGuardAttack;
					_blockPoint = array[i].point;
				}
				Core.Logic.Penitent.Damage(_hit);
			}
			else
			{
				EnemyDamageArea component = array[i].collider.GetComponent<EnemyDamageArea>();
				if ((bool)component)
				{
					((IDamageable)component.OwnerEntity).Damage(_hit);
				}
				else
				{
					IDamageable damageable = array[i].collider.GetComponentInChildren<IDamageable>() ?? array[i].collider.GetComponentInParent<IDamageable>();
					if (damageable != null)
					{
						damageable.Damage(_hit);
						if (Mathf.Abs(slowTimeDuration) > Mathf.Epsilon)
						{
							Core.Logic.ScreenFreeze.Freeze(0.1f, slowTimeDuration, 0f, slowTimeCurve);
						}
					}
				}
			}
			DrawDebugCross(array[i].point, Color.magenta, 2f);
		}
	}

	private void OnPenitentGuardAttack(Hit h)
	{
		Vector2 vector = ((Core.Logic.Penitent.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		_wasBlocked = true;
		Core.Logic.ScreenFreeze.Freeze(0.15f, 0.2f);
	}

	private static void DrawDebugCross(Vector2 point, Color c, float seconds)
	{
		float num = 0.6f;
		Debug.DrawLine(point - Vector2.up * num, point + Vector2.up * num, c, seconds);
		Debug.DrawLine(point - Vector2.right * num, point + Vector2.right * num, c, seconds);
	}

	private GameObject InstantiateObject(DashAttackInstantiations objectConfig, Vector2 p, Vector2 dir)
	{
		alreadyInstantiated.Add(objectConfig);
		GameObject gameObject = ((!objectConfig.prefabToInstantiate.GetComponent<PoolObject>()) ? Object.Instantiate(objectConfig.prefabToInstantiate, p, Quaternion.identity) : PoolManager.Instance.ReuseObject(objectConfig.prefabToInstantiate, p, Quaternion.identity).GameObject);
		if (!objectConfig.keepRotation)
		{
			gameObject.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * 57.29578f);
		}
		return gameObject;
	}

	public void SetSpawnsDamage(int damage)
	{
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			item.prefabToInstantiate.GetComponent<IDirectAttack>()?.SetDamage(damage);
		}
		CreateSpawnsHits();
	}

	public void CreateSpawnsHits()
	{
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			item.prefabToInstantiate.GetComponent<IDirectAttack>()?.CreateHit();
		}
	}
}
