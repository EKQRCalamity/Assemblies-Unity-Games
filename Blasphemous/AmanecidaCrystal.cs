using Gameplay.GameControllers.Bosses.Amanecidas;
using Gameplay.GameControllers.Effects.Entity;
using UnityEngine;

public class AmanecidaCrystal : MonoBehaviour
{
	private AmanecidasBehaviour ama;

	private SimpleDamageableObject damageable;

	private MasterShaderEffects fx;

	private void Awake()
	{
		damageable = GetComponent<SimpleDamageableObject>();
		damageable.OnDamageableDestroyed += Damageable_OnDamageableDestroyed;
		fx = GetComponentInChildren<MasterShaderEffects>();
	}

	private void Damageable_OnDamageableDestroyed()
	{
		Explode();
	}

	private void OnEnable()
	{
		ama = AmanecidasFightSpawner.Instance.currentBoss.GetComponent<AmanecidasBehaviour>();
	}

	private void Update()
	{
		UpdateColorByTTL();
	}

	private void UpdateColorByTTL()
	{
		float colorizeStrength = 1f - damageable.currentTtl / damageable.durationToUse;
		fx.SetColorizeStrength(colorizeStrength);
	}

	private void Explode()
	{
		if (ama != null)
		{
			ama.OnCrystalExplode(this);
		}
	}

	public void MultiplyCurrentTimeToLive(float multiplier, float maxTtl)
	{
		damageable.currentTtl *= multiplier;
		if (damageable.currentTtl > maxTtl)
		{
			damageable.currentTtl = maxTtl;
		}
	}
}
