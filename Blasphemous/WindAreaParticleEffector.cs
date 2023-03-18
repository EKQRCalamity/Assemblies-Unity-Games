using System;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay.GameControllers.Environment.AreaEffects;
using Sirenix.OdinInspector;
using UnityEngine;

public class WindAreaParticleEffector : MonoBehaviour
{
	public List<ParticleSystem> psList;

	public bool changeEmissionRate = true;

	[ShowIf("changeEmissionRate", true)]
	public float maxRate = 10f;

	[ShowIf("changeEmissionRate", true)]
	public float minRate = 10f;

	[ShowIf("changeEmissionRate", true)]
	public float maxForce = 15f;

	public bool stopIfWindDisabled;

	public bool modifyXMultiplierRelatively;

	[ShowIf("modifyXMultiplierRelatively", true)]
	public float xMultiplierFactor = 0.2f;

	[ShowIf("stopIfWindDisabled", true)]
	public bool resetMultiplierSmoothly;

	[ShowIf("resetMultiplierSmoothly", true)]
	public float lerpTime = 0.5f;

	private WindAreaEffect _windZone;

	private Dictionary<ParticleSystem, float> startingXMultipliers = new Dictionary<ParticleSystem, float>();

	private bool resetPending;

	private void Awake()
	{
		_windZone = GetComponent<WindAreaEffect>();
	}

	private void Start()
	{
		foreach (ParticleSystem ps in psList)
		{
			startingXMultipliers.Add(ps, ps.velocityOverLifetime.xMultiplier);
		}
	}

	private void Update()
	{
		if (stopIfWindDisabled && _windZone.IsDisabled)
		{
			if (resetPending)
			{
				Reset();
			}
			return;
		}
		Vector3 windForce = _windZone.GetWindForce();
		foreach (ParticleSystem ps in psList)
		{
			UpdatePS(ps, windForce.x);
		}
		resetPending = true;
	}

	private void Reset()
	{
		resetPending = false;
		foreach (ParticleSystem ps in psList)
		{
			float num = Math.Sign(startingXMultipliers[ps]);
			UpdatePS(ps, num * 1f / xMultiplierFactor, resetting: true);
		}
	}

	private void UpdatePS(ParticleSystem ps, float v, bool resetting = false)
	{
		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;
		if (modifyXMultiplierRelatively)
		{
			float num = ((!(v > 0f)) ? (0f - Math.Abs(v * startingXMultipliers[ps])) : Math.Abs(v * startingXMultipliers[ps]));
			if (resetting && resetMultiplierSmoothly)
			{
				DOTween.To(() => velocityOverLifetime.xMultiplier, delegate(float x)
				{
					velocityOverLifetime.xMultiplier = x;
				}, num * xMultiplierFactor, lerpTime);
			}
			else
			{
				velocityOverLifetime.xMultiplier = num * xMultiplierFactor;
			}
		}
		else
		{
			velocityOverLifetime.xMultiplier = v;
		}
		if (changeEmissionRate)
		{
			float num2 = Mathf.Abs(v);
			ParticleSystem.EmissionModule emission = ps.emission;
			emission.rateOverTime = Mathf.Lerp(minRate, maxRate, num2 / maxForce);
		}
	}
}
