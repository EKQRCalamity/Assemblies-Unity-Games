using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.DamageObject;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class DamageObject : MonoBehaviour, IDamageable
{
	[SerializeField]
	[Tooltip("The life base of the entity.")]
	protected float ObjectLife;

	[SerializeField]
	[Tooltip("The damage amount inflicted to the victim.")]
	protected float DamageAmount;

	[SerializeField]
	[EventRef]
	protected string HitSoundFx;

	[SerializeField]
	[EventRef]
	protected string DamageSoundFx;

	[SerializeField]
	[EventRef]
	protected string DestroySoundFx;

	protected SpriteRenderer SpriteRenderer;

	protected Animator Animator;

	protected bool IsDestroyed;

	protected Hit ObjectHit;

	[Tooltip("The layer of the potential victims.")]
	public LayerMask AffectedEntities;

	private void Awake()
	{
		ObjectHit = new Hit
		{
			AttackingEntity = base.gameObject,
			DamageType = DamageArea.DamageType.Normal,
			DamageAmount = DamageAmount,
			HitSoundId = HitSoundFx,
			Unnavoidable = true
		};
		SpriteRenderer = GetComponent<SpriteRenderer>();
		Animator = GetComponent<Animator>();
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((AffectedEntities.value & (1 << other.gameObject.layer)) > 0)
		{
			IDamageable componentInChildren = other.transform.root.GetComponentInChildren<IDamageable>();
			if (componentInChildren != null)
			{
				Attack(componentInChildren);
			}
		}
	}

	public virtual void Attack(IDamageable damageable)
	{
		damageable.Damage(ObjectHit);
	}

	public virtual void SetDestroyAnimation()
	{
		Animator.SetTrigger("DESTROY");
	}

	protected virtual void TakeDamage(Hit hit)
	{
		if (!IsDestroyed)
		{
			ObjectLife -= hit.DamageAmount;
			Core.Logic.ScreenFreeze.Freeze(0.1f, 0.1f);
			Core.Audio.PlaySfx(DamageSoundFx);
			if (ObjectLife <= 0f)
			{
				IsDestroyed = true;
				SetDestroyAnimation();
				GetComponent<Collider2D>().enabled = false;
				Core.Audio.PlaySfx(DestroySoundFx);
			}
		}
	}

	public virtual void DisableDamageObject()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public virtual void Damage(Hit hit)
	{
		TakeDamage(hit);
	}

	public virtual Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return false;
	}
}
