using System;
using System.Collections.Generic;
using Framework.FrameworkCore.Attributes;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class BossHealth : MonoBehaviour
{
	public Text text;

	public Text textShadow;

	public Image loss;

	public Image health;

	public float ConsumptionSpeeed;

	private float _damageTimeElapsed;

	public AnimationCurve HealthLossAnimationCurve;

	private CanvasGroup canvasGroup;

	private Entity target;

	private float BarTarget
	{
		get
		{
			Life life = target.Stats.Life;
			float current = life.Current;
			float final = life.Final;
			return current / final;
		}
	}

	private void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		Hide();
	}

	private void Update()
	{
		if (!(target == null))
		{
			CalculateLossBar();
			CalculateHealthBar();
		}
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
			health.fillAmount = Mathf.Lerp(health.fillAmount, BarTarget, _damageTimeElapsed * ConsumptionSpeeed);
		}
	}

	public void Show()
	{
		canvasGroup.alpha = 1f;
		canvasGroup.blocksRaycasts = true;
	}

	public void Hide()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.blocksRaycasts = false;
		UnsubscribeToDamage();
	}

	public void SetName(string name)
	{
		text.text = name;
		textShadow.text = name;
	}

	public void SetTarget(GameObject entity)
	{
		target = entity.GetComponent<Entity>();
		SubscribeToDamage();
	}

	private void SubscribeToDamage()
	{
		if (!(target == null))
		{
			List<EnemyDamageArea> list = new List<EnemyDamageArea>(target.GetComponentsInChildren<EnemyDamageArea>(includeInactive: true));
			list.ForEach(delegate(EnemyDamageArea x)
			{
				x.OnDamaged = (EnemyDamageArea.EnemyDamagedEvent)Delegate.Combine(x.OnDamaged, new EnemyDamageArea.EnemyDamagedEvent(OnDamaged));
			});
		}
	}

	private void UnsubscribeToDamage()
	{
		if (!(target == null))
		{
			List<EnemyDamageArea> list = new List<EnemyDamageArea>(target.GetComponentsInChildren<EnemyDamageArea>(includeInactive: true));
			list.ForEach(delegate(EnemyDamageArea x)
			{
				x.OnDamaged = (EnemyDamageArea.EnemyDamagedEvent)Delegate.Remove(x.OnDamaged, new EnemyDamageArea.EnemyDamagedEvent(OnDamaged));
			});
		}
	}

	private void OnDamaged(GameObject damaged, Hit hit)
	{
		_damageTimeElapsed = 0f;
	}
}
