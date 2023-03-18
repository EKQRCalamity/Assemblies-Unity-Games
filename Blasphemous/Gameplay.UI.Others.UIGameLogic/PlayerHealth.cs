using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Damage;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class PlayerHealth : MonoBehaviour
{
	[SerializeField]
	private Image health;

	[SerializeField]
	private AnimationCurve HealthLossAnimationCurve;

	[SerializeField]
	private Image loss;

	[SerializeField]
	private RectTransform backgroundFillTransform;

	[SerializeField]
	[Range(0f, 10f)]
	private float speed;

	[SerializeField]
	private float backgroundStartSize = 60f;

	[SerializeField]
	private float endFillSize = 10f;

	[SerializeField]
	private RectTransform backgroundMid;

	private RectTransform healthTransform;

	private RectTransform lossTransform;

	private float _damageTimeElapsed;

	private float lastBarWidth = -1f;

	private float BarTarget
	{
		get
		{
			Penitent penitent = Core.Logic.Penitent;
			if ((bool)penitent)
			{
				float current = penitent.Stats.Life.Current;
				float final = penitent.Stats.Life.Final;
				return current / final;
			}
			return 0f;
		}
	}

	private void Awake()
	{
		SpawnManager.OnPlayerSpawn += OnPenitentReady;
		lossTransform = loss.GetComponent<RectTransform>();
		healthTransform = health.GetComponent<RectTransform>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPenitentReady;
	}

	private void OnPenitentReady(Penitent penitent)
	{
		base.enabled = true;
		PenitentDamageArea damageArea = penitent.DamageArea;
		damageArea.OnDamaged = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(damageArea.OnDamaged, new PenitentDamageArea.PlayerDamagedEvent(OnDamaged));
	}

	private void Update()
	{
		CalculateHealthBarSize();
		CalculateLossBar();
		CalculateHealthBar();
	}

	private void OnDamaged(Penitent damaged, Hit hit)
	{
		_damageTimeElapsed = 0f;
	}

	private void CalculateLossBar()
	{
		if (!Mathf.Approximately(loss.fillAmount, BarTarget))
		{
			_damageTimeElapsed += Time.deltaTime;
			loss.fillAmount = Mathf.Lerp(loss.fillAmount, BarTarget, HealthLossAnimationCurve.Evaluate(_damageTimeElapsed));
		}
	}

	private void CalculateHealthBar()
	{
		if (!Mathf.Approximately(health.fillAmount, BarTarget))
		{
			_damageTimeElapsed += Time.deltaTime;
			health.fillAmount = Mathf.Lerp(health.fillAmount, BarTarget, _damageTimeElapsed * speed);
		}
	}

	private void CalculateHealthBarSize()
	{
		Penitent penitent = Core.Logic.Penitent;
		if (!(penitent == null))
		{
			float final = penitent.Stats.Life.Final;
			if (final != lastBarWidth)
			{
				lastBarWidth = final;
				float num = final - backgroundStartSize - endFillSize;
				num = ((!(num > 0f)) ? 0f : num);
				backgroundMid.sizeDelta = new Vector2(num, backgroundMid.sizeDelta.y);
				lossTransform.sizeDelta = new Vector2(final, lossTransform.sizeDelta.y);
				healthTransform.sizeDelta = new Vector2(final, healthTransform.sizeDelta.y);
				backgroundFillTransform.sizeDelta = new Vector2(final, healthTransform.sizeDelta.y);
			}
		}
	}
}
