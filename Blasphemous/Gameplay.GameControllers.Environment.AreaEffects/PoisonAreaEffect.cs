using System.Linq;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.AreaEffects;

public class PoisonAreaEffect : AreaEffect
{
	[FoldoutGroup("EffectSettings", true, 0)]
	[Tooltip("Damage amount per lapse.")]
	public float DamageAmount;

	[FoldoutGroup("EffectSettings", true, 0)]
	[EventRef]
	public string DamageAudioEffect;

	[FoldoutGroup("EffectSettings", true, 0)]
	public Material DamageEffectMaterial;

	private Hit _hit;

	protected override void OnAwake()
	{
		base.OnAwake();
		_hit = new Hit
		{
			DamageAmount = DamageAmount,
			DamageElement = DamageArea.DamageElement.Toxic,
			AttackingEntity = base.gameObject,
			DamageType = DamageArea.DamageType.Simple,
			Unnavoidable = true
		};
	}

	protected override void OnStayAreaEffect()
	{
		base.OnStayAreaEffect();
		PoisonEntities();
	}

	public void PoisonEntities()
	{
		for (int i = 0; i < Population.Count; i++)
		{
			if (!(Population.ElementAtOrDefault(i) == null))
			{
				PoisonGameObject(Population[i]);
			}
		}
	}

	private void PoisonGameObject(GameObject entityGameObject)
	{
		IDamageable damageable = null;
		Entity componentInParent = entityGameObject.GetComponentInParent<Entity>();
		if (componentInParent != null)
		{
			if (componentInParent.Status.Dead)
			{
				return;
			}
			PoisonEntity(componentInParent);
		}
		else
		{
			damageable = entityGameObject.GetComponentInParent<IDamageable>();
			PoisonDamageable(damageable);
		}
		Core.Audio.PlaySfx(DamageAudioEffect);
	}

	private void PoisonEntity(Entity entity)
	{
		if (!(entity == null) && entity is IDamageable damageable)
		{
			_hit.DamageAmount = DamageAmount;
			_hit.DamageAmount = entity.GetReducedDamage(_hit);
			damageable.Damage(_hit);
			if (entity.Status.Dead)
			{
				RemoveEntityToAreaPopulation(entity.gameObject);
			}
			TriggerVisualEffect(entity);
		}
	}

	private void PoisonDamageable(IDamageable damageable)
	{
		if (damageable != null)
		{
			_hit.DamageAmount = DamageAmount;
			damageable.Damage(_hit);
		}
	}

	private void TriggerVisualEffect(Entity entity)
	{
		MasterShaderEffects componentInChildren = entity.GetComponentInChildren<MasterShaderEffects>();
		if (componentInChildren != null && DamageEffectMaterial != null)
		{
			componentInChildren.DamageEffectBlink(0f, 0.2f, DamageEffectMaterial);
		}
	}
}
