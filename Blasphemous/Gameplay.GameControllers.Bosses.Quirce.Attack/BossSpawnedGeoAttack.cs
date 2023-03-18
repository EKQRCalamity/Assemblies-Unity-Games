using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossSpawnedGeoAttack : Weapon, IDirectAttack
{
	[Serializable]
	public struct AmanecidaRockSprites
	{
		public Sprite topSprite;

		public Sprite bodySprite;

		public GameObject dustVFX;
	}

	public List<AmanecidaRockSprites> nightSprites;

	public List<AmanecidaRockSprites> dawnSprites;

	public List<AmanecidaRockSprites> daySprites;

	public SpriteRenderer topSpriteRenderer;

	public SpriteRenderer bodySpriteRenderer;

	[FoldoutGroup("Collision settings", 0)]
	public bool snapToGround;

	[FoldoutGroup("Collision settings", 0)]
	public LayerMask groundMask;

	[FoldoutGroup("Collision settings", 0)]
	public float RangeGroundDetection = 2f;

	public List<AmanecidaSpike> spikes;

	public float duration = 2f;

	private RaycastHit2D[] _bottomHits;

	protected override void OnAwake()
	{
		base.OnAwake();
		_bottomHits = new RaycastHit2D[1];
		SetupSprite(AmanecidasFightSpawner.Instance.amanecidaFight);
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	private List<AmanecidaRockSprites> GetListFromFightNumber(int fn)
	{
		if (fn == 0)
		{
			return nightSprites;
		}
		if (fn < 2)
		{
			return dawnSprites;
		}
		return daySprites;
	}

	public void SetupSprite(int fightNumber)
	{
		List<AmanecidaRockSprites> listFromFightNumber = GetListFromFightNumber(fightNumber);
		if (listFromFightNumber.Count <= 0)
		{
			return;
		}
		int index = UnityEngine.Random.Range(0, listFromFightNumber.Count);
		AmanecidaRockSprites amanecidaRockSprites = listFromFightNumber[index];
		GameObject dustVFX = amanecidaRockSprites.dustVFX;
		PoolManager.Instance.CreatePool(dustVFX, 1);
		foreach (AmanecidaSpike spike in spikes)
		{
			spike.dustPrefab = dustVFX;
		}
		topSpriteRenderer.sprite = amanecidaRockSprites.topSprite;
		bodySpriteRenderer.sprite = amanecidaRockSprites.bodySprite;
	}

	public override void Attack(Hit weapondHit)
	{
	}

	public void SpawnGeo(float delay = 0f, float heightPercentage = 1f)
	{
		foreach (AmanecidaSpike spike in spikes)
		{
			spike.Show(0.5f, delay, heightPercentage);
		}
		DelayedHide(delay + duration);
	}

	private void DelayedHide(float delay)
	{
		StartCoroutine(HideAll(delay));
	}

	private IEnumerator HideAll(float delay)
	{
		yield return new WaitForSeconds(delay);
		foreach (AmanecidaSpike spike in spikes)
		{
			if (spike != null)
			{
				spike.Hide();
			}
		}
		yield return new WaitForSeconds(delay);
		Recycle();
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	private void SnapToGround()
	{
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		Collider2D[] array = componentsInChildren;
		foreach (Collider2D collider2D in array)
		{
			collider2D.enabled = false;
		}
		Vector2 vector = base.transform.position;
		if (Physics2D.LinecastNonAlloc(vector, vector + Vector2.down * RangeGroundDetection, _bottomHits, groundMask) > 0)
		{
			base.transform.position += Vector3.down * _bottomHits[0].distance;
		}
		Collider2D[] array2 = componentsInChildren;
		foreach (Collider2D collider2D2 in array2)
		{
			collider2D2.enabled = true;
		}
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		if (snapToGround)
		{
			SnapToGround();
		}
	}

	public void Recycle()
	{
		Destroy();
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void CreateHit()
	{
	}

	public void SetDamage(int damage)
	{
	}
}
