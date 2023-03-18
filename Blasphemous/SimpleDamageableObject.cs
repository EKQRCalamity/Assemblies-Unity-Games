using System;
using System.Collections.Generic;
using Framework.Managers;
using Framework.Pooling;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

public class SimpleDamageableObject : PoolObject, IDamageable
{
	[FoldoutGroup("General", 0)]
	public float hpMax = 30f;

	[FoldoutGroup("Effects", 0)]
	public bool sparkOnImpact;

	[FoldoutGroup("Effects", 0)]
	public bool bleedOnImpact;

	[FoldoutGroup("Effects", 0)]
	public bool screenshakeOnDeath;

	[FoldoutGroup("Effects", 0)]
	public bool shockwaveOnDeath;

	[FoldoutGroup("Effects", 0)]
	public bool screenfreezeOnDeath;

	[FoldoutGroup("Destruction", 0)]
	public List<GameObject> instantiateOnDestroy;

	[FoldoutGroup("Destruction", 0)]
	public Vector2 instantiationOffset;

	[FoldoutGroup("Destruction", 0)]
	public bool shouldIgnoreFirstHit;

	[FoldoutGroup("Destruction", 0)]
	public bool hasLimitedDuration;

	[FoldoutGroup("Destruction", 0)]
	[ShowIf("hasLimitedDuration", true)]
	public bool usesRandomDurationRange;

	[FoldoutGroup("Destruction", 0)]
	[HideIf("usesRandomDurationRange", true)]
	public float duration;

	[FoldoutGroup("Destruction", 0)]
	[ShowIf("usesRandomDurationRange", true)]
	[MinMaxSlider(0f, 10f, true)]
	public Vector2 durationRange;

	private float currentHp;

	private bool firstHitIgnored;

	[HideInInspector]
	public float durationToUse;

	[HideInInspector]
	public float currentTtl;

	public event Action OnDamageableDestroyed;

	public bool BleedOnImpact()
	{
		return bleedOnImpact;
	}

	private void Awake()
	{
		foreach (GameObject item in instantiateOnDestroy)
		{
			PoolManager.Instance.CreatePool(item, 1);
		}
	}

	private void Update()
	{
		if (hasLimitedDuration)
		{
			currentTtl -= Time.deltaTime;
			if (currentTtl < 0f)
			{
				DoDestroy();
			}
		}
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		currentHp = hpMax;
		if (usesRandomDurationRange)
		{
			durationToUse = UnityEngine.Random.Range(durationRange.x, durationRange.y);
		}
		else
		{
			durationToUse = duration;
		}
		currentTtl = durationToUse;
		firstHitIgnored = false;
	}

	private void EffectsOnDeath()
	{
		if (screenfreezeOnDeath)
		{
			Core.Logic.ScreenFreeze.Freeze(0.15f, 0.1f);
		}
		if (shockwaveOnDeath)
		{
			Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.3f, 0.1f, 0.4f);
		}
		if (screenshakeOnDeath)
		{
			Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 1f, 12, 0.2f, 0.01f, default(Vector3), 0.01f, ignoreTimeScale: true);
		}
	}

	public void SetFlip(bool flip)
	{
		SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.flipX = flip;
		}
	}

	public void Damage(Hit hit)
	{
		if (!firstHitIgnored && shouldIgnoreFirstHit)
		{
			firstHitIgnored = true;
			return;
		}
		currentHp -= hit.DamageAmount;
		if (currentHp <= 0f)
		{
			DoDestroy();
		}
	}

	protected virtual void DoDestroy()
	{
		foreach (GameObject item in instantiateOnDestroy)
		{
			PoolManager.Instance.ReuseObject(item, (Vector2)base.transform.position + instantiationOffset, Quaternion.identity);
		}
		EffectsOnDeath();
		if (this.OnDamageableDestroyed != null)
		{
			this.OnDamageableDestroyed();
		}
		Destroy();
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public bool SparkOnImpact()
	{
		return sparkOnImpact;
	}
}
