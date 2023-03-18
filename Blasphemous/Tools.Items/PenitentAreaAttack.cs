using System.Collections;
using FMODUnity;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Items;

public class PenitentAreaAttack : ObjectEffect
{
	[SerializeField]
	[BoxGroup("Attack settings", true, false, 0)]
	private float Radius;

	[SerializeField]
	[BoxGroup("Attack settings", true, false, 0)]
	private LayerMask damageMask;

	[SerializeField]
	[BoxGroup("Effects", true, false, 0)]
	private GameObject prefabIntoPlayer;

	[SerializeField]
	[BoxGroup("Effects", true, false, 0)]
	private GameObject prefabIntoEnemies;

	[SerializeField]
	[BoxGroup("Effects", true, false, 0)]
	private Vector2 intoPlayerOffset;

	[SerializeField]
	[BoxGroup("Effects", true, false, 0)]
	private Vector2 intoEnemiesOffset;

	[SerializeField]
	[BoxGroup("Effects", true, false, 0)]
	private float damageDelay;

	[SerializeField]
	[BoxGroup("Damage", true, false, 0)]
	private DamageArea.DamageType DamageType;

	[SerializeField]
	[BoxGroup("Damage", true, false, 0)]
	private DamageArea.DamageElement DamageElement;

	[SerializeField]
	[BoxGroup("Damage", true, false, 0)]
	private float Amount;

	[SerializeField]
	[BoxGroup("Damage", true, false, 0)]
	private float Force;

	[SerializeField]
	[BoxGroup("Damage", true, false, 0)]
	private bool Unnavoidable;

	[BoxGroup("Damage", true, false, 0)]
	public AnimationCurve slowTimeCurve;

	[BoxGroup("Damage", true, false, 0)]
	public float slowTimeDuration;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string HitSound;

	private Hit attackHit;

	protected override void OnStart()
	{
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	protected void OnDisable()
	{
		StopAllCoroutines();
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		if (prefabIntoPlayer != null)
		{
			PoolManager.Instance.CreatePool(prefabIntoPlayer, 1);
		}
		if (prefabIntoEnemies != null)
		{
			PoolManager.Instance.CreatePool(prefabIntoEnemies, 10);
		}
	}

	protected override bool OnApplyEffect()
	{
		CreateHit();
		if (prefabIntoPlayer != null)
		{
			PoolManager.Instance.ReuseObject(prefabIntoPlayer, Core.Logic.Penitent.GetPosition() + (Vector3)intoPlayerOffset, Quaternion.identity);
		}
		StartCoroutine(DamageCoroutine(attackHit));
		return base.OnApplyEffect();
	}

	private float CalculateDamage()
	{
		return Amount * (1f + (Core.Logic.Penitent.Stats.PrayerStrengthMultiplier.Final - 1f) * 0.5f);
	}

	private void CreateHit()
	{
		attackHit = new Hit
		{
			AttackingEntity = Core.Logic.Penitent.gameObject,
			DamageType = DamageType,
			Force = Force,
			DamageAmount = CalculateDamage(),
			HitSoundId = HitSound,
			Unnavoidable = Unnavoidable
		};
	}

	public void ShakeWave()
	{
		Core.Logic.ScreenFreeze.Freeze(0.05f, slowTimeDuration, 0f, slowTimeCurve);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(Core.Logic.Penitent.transform.position + (Vector3)intoPlayerOffset, 1.2f, 0.2f, 1.8f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.35f, Vector3.down * 0.5f, 12, 0.2f, 0.01f, default(Vector3), 0.01f, ignoreTimeScale: true);
	}

	private IEnumerator DamageCoroutine(Hit hit)
	{
		Penitent penitent = Core.Logic.Penitent;
		int max = 16;
		Quaternion rot = Quaternion.Euler(0f, 0f, 360f / (float)max);
		Vector2 v = Vector2.right * Radius;
		Vector3 playerPos = penitent.GetPosition();
		for (int j = 0; j < max; j++)
		{
			Debug.DrawLine(playerPos, playerPos + (Vector3)v, Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f), 5f);
			v = rot * v;
		}
		ShakeWave();
		Collider2D[] hits = new Collider2D[20];
		int overlappedEntities = Physics2D.OverlapCircleNonAlloc(penitent.GetPosition() + (Vector3)intoPlayerOffset, Radius, hits, damageMask);
		float currentDamageDelay = damageDelay;
		for (int i = 0; i < overlappedEntities; i++)
		{
			if (!hits[i] || hits[i].CompareTag("Penitent"))
			{
				continue;
			}
			EnemyDamageArea dmgArea = null;
			Enemy enemy = hits[i].GetComponentInParent<Enemy>();
			if ((bool)enemy)
			{
				dmgArea = enemy.GetComponentInChildren<EnemyDamageArea>();
				if (enemy.SpriteRenderer != null && !enemy.SpriteRenderer.IsVisibleFrom(Camera.main))
				{
					continue;
				}
			}
			if (dmgArea != null)
			{
				if ((bool)prefabIntoEnemies)
				{
					PoolManager.Instance.ReuseObject(prefabIntoEnemies, hits[i].bounds.center + (Vector3)intoEnemiesOffset, Quaternion.identity);
				}
				yield return new WaitForSeconds(currentDamageDelay);
				currentDamageDelay *= 0.8f;
				if ((bool)dmgArea && (bool)dmgArea.OwnerEntity)
				{
					((IDamageable)dmgArea.OwnerEntity).Damage(hit);
				}
				continue;
			}
			IDamageable damageable = hits[i].GetComponentInChildren<IDamageable>();
			if (damageable != null && !(damageable is ProjectileReaction))
			{
				if ((bool)prefabIntoEnemies)
				{
					PoolManager.Instance.ReuseObject(prefabIntoEnemies, hits[i].gameObject.transform.position + (Vector3)intoEnemiesOffset, Quaternion.identity);
				}
				yield return new WaitForSeconds(currentDamageDelay);
				currentDamageDelay *= 0.8f;
				damageable?.Damage(hit);
			}
		}
	}
}
