using System;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class ProjectileReaction : MonoBehaviour, IDamageable
{
	public Projectile owner;

	[FormerlySerializedAs("IsMaterial")]
	public bool IsPhysical;

	public bool DestroyedByNormalHits;

	public AnimationCurve slowTimeCurve;

	public float slowTimeDuration;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string hitSound;

	public bool DestructorHit;

	public event Action<ProjectileReaction> OnProjectileHit;

	public void Damage(Hit hit)
	{
		if (IsPhysical)
		{
			TakeDamage(hit);
		}
		else if (hit.DestroysProjectiles)
		{
			TakeDamage(hit);
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	private void TakeDamage(Hit hit, Action callback = null)
	{
		string eventKey = (string.IsNullOrEmpty(hitSound) ? hit.HitSoundId : hitSound);
		DestructorHit = hit.DestroysProjectiles;
		Core.Audio.EventOneShotPanned(eventKey, base.transform.position);
		if (Math.Abs(slowTimeDuration) > Mathf.Epsilon)
		{
			Core.Logic.ScreenFreeze.Freeze(0.1f, slowTimeDuration, 0f, slowTimeCurve);
		}
		if (this.OnProjectileHit != null)
		{
			this.OnProjectileHit(this);
		}
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return IsPhysical;
	}
}
