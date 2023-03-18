using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossAreaSummonAttack : EnemyAttack, ISpawnerAttack
{
	public AnimationCurve curve;

	public bool useDifferentRandomAreas;

	[HideIf("useDifferentRandomAreas", true)]
	public GameObject areaPrefab;

	[ShowIf("useDifferentRandomAreas", true)]
	public List<GameObject> areaPrefabs;

	[ShowIf("useDifferentRandomAreas", true)]
	public float yDisplacement;

	public int totalAreas;

	public float distanceBetweenAreas;

	public float seconds;

	public float offset = 2f;

	public int poolSize = 3;

	public LayerMask collisionMask;

	public bool checkCollisions = true;

	private float damageMultiplier = 1f;

	public List<GameObject> instantiations;

	private int lastRandomAreaIndex;

	public int SpawnedAreaAttackDamage;

	protected override void OnStart()
	{
		base.OnStart();
		if (useDifferentRandomAreas)
		{
			foreach (GameObject areaPrefab in areaPrefabs)
			{
				PoolManager.Instance.CreatePool(areaPrefab, poolSize);
			}
			return;
		}
		PoolManager.Instance.CreatePool(this.areaPrefab, poolSize);
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
	}

	public void SummonAreas(Vector3 direction)
	{
		StartCoroutine(SummonAreasCoroutine(base.transform.position, direction, EntityOrientation.Right));
	}

	public void SummonAreas(Vector3 position, Vector3 direction)
	{
		StartCoroutine(SummonAreasCoroutine(position, direction, EntityOrientation.Right));
	}

	public void SummonAreas(Vector3 position, Vector3 direction, EntityOrientation orientation)
	{
		StartCoroutine(SummonAreasCoroutine(position, direction, orientation));
	}

	public void SetDamageStrength(float str)
	{
		damageMultiplier = str;
	}

	public GameObject SummonAreaOnPoint(Vector3 point, float angle = 0f, float damageStrength = 1f, Action callbackOnLoopFinish = null)
	{
		return InstantiateArea(areaPrefab, point, angle, damageStrength, callbackOnLoopFinish);
	}

	public GameObject SummonAreaOnPoint(int areaIndex, Vector3 point, float angle = 0f, float damageStrength = 1f, Action callbackOnLoopFinish = null)
	{
		return InstantiateArea(areaPrefabs[areaIndex], point, angle, damageStrength, callbackOnLoopFinish);
	}

	public void ClearAll()
	{
		if (instantiations != null)
		{
			for (int i = 0; i < instantiations.Count; i++)
			{
				instantiations[i].SetActive(value: false);
			}
		}
	}

	private IEnumerator SummonAreasCoroutine(Vector3 origin, Vector3 dir, EntityOrientation orientation)
	{
		float counter = 0f;
		int areasSummoned = 0;
		Vector3 lastPoint = origin + dir * offset;
		bool cancelled = false;
		int currentTotalAreas = totalAreas;
		float currentDistanceBetweenAreas = distanceBetweenAreas;
		for (lastRandomAreaIndex = -1; counter < seconds; counter += Time.deltaTime)
		{
			float normalizedValue = curve.Evaluate(counter / seconds);
			if ((float)areasSummoned / (float)currentTotalAreas <= normalizedValue)
			{
				if (checkCollisions || cancelled)
				{
					RaycastHit2D[] array = new RaycastHit2D[1];
					if (Physics2D.LinecastNonAlloc(origin, lastPoint, array, collisionMask) > 0)
					{
						Debug.DrawLine(array[0].point, array[0].point + Vector2.up * 0.25f, Color.red, 1f);
						cancelled = true;
					}
				}
				if (!cancelled)
				{
					GameObject gameObject;
					if (useDifferentRandomAreas)
					{
						int num = UnityEngine.Random.Range(0, areaPrefabs.Count);
						if (areaPrefabs.Count > 1)
						{
							while (lastRandomAreaIndex == num)
							{
								num = UnityEngine.Random.Range(0, areaPrefabs.Count);
							}
							lastRandomAreaIndex = num;
						}
						GameObject toInstantiate = areaPrefabs[num];
						gameObject = InstantiateArea(toInstantiate, lastPoint, 0f, damageMultiplier);
						bool flag = areasSummoned % 2 == 0;
						float num2 = ((!flag) ? (0f - yDisplacement) : yDisplacement);
						num2 += 0.1f;
						BossSpawnedAreaAttack component = gameObject.GetComponent<BossSpawnedAreaAttack>();
						component.transform.position += Vector3.up * num2;
						component.GetComponentInChildren<SpriteRenderer>().sortingOrder = ((!flag) ? 2 : 0);
					}
					else
					{
						gameObject = InstantiateArea(areaPrefab, lastPoint, 0f, damageMultiplier);
					}
					Entity component2 = gameObject.GetComponent<Entity>();
					if (component2 != null)
					{
						component2.SetOrientation(orientation);
					}
					areasSummoned++;
				}
				lastPoint += dir * currentDistanceBetweenAreas;
			}
			yield return null;
		}
	}

	private GameObject InstantiateArea(GameObject toInstantiate, Vector3 point, float angle = 0f, float damageStrength = 1f, Action callbackOnLoopFinish = null)
	{
		Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
		GameObject gameObject = PoolManager.Instance.ReuseObject(toInstantiate, point, rotation).GameObject;
		BossSpawnedAreaAttack component = gameObject.GetComponent<BossSpawnedAreaAttack>();
		if (component != null)
		{
			component.SetOwner(base.EntityOwner);
			component.SetDamageStrength(damageStrength);
			if (SpawnedAreaAttackDamage > 0)
			{
				component.SetDamage(SpawnedAreaAttackDamage);
			}
			component.SetCallbackOnLoopFinish(callbackOnLoopFinish);
		}
		if (instantiations == null)
		{
			instantiations = new List<GameObject>();
		}
		if (!instantiations.Contains(gameObject))
		{
			instantiations.Add(gameObject);
		}
		return gameObject;
	}

	public void SetSpawnsDamage(int damage)
	{
		SpawnedAreaAttackDamage = damage;
		if (useDifferentRandomAreas)
		{
			foreach (GameObject areaPrefab in areaPrefabs)
			{
				areaPrefab.GetComponent<IDirectAttack>()?.SetDamage(damage);
			}
		}
		else
		{
			this.areaPrefab.GetComponent<IDirectAttack>()?.SetDamage(damage);
		}
		CreateSpawnsHits();
	}

	public void CreateSpawnsHits()
	{
		if (useDifferentRandomAreas)
		{
			foreach (GameObject areaPrefab in areaPrefabs)
			{
				areaPrefab.GetComponent<IDirectAttack>()?.CreateHit();
			}
			return;
		}
		this.areaPrefab.GetComponent<IDirectAttack>()?.CreateHit();
	}
}
