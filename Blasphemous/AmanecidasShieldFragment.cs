using DG.Tweening;
using Gameplay.GameControllers.Effects.Entity;
using UnityEngine;

public class AmanecidasShieldFragment : MonoBehaviour
{
	public enum AMA_SHIELD_FRAGMENT_STATES
	{
		SHIELD,
		BROKEN,
		RISING
	}

	public Transform shieldTransform;

	public float timeToShieldTransform = 0.5f;

	private SpriteRenderer spr;

	public AMA_SHIELD_FRAGMENT_STATES currentState;

	private void Awake()
	{
		shieldTransform = base.transform.parent;
		spr = GetComponentInChildren<SpriteRenderer>();
		currentState = AMA_SHIELD_FRAGMENT_STATES.SHIELD;
		FadeOut();
	}

	public void GoToShieldTransform(float delay)
	{
		base.transform.DOMove(shieldTransform.position, timeToShieldTransform).SetEase(Ease.InCubic).SetDelay(delay)
			.OnComplete(delegate
			{
				OnReachedShield();
			});
		base.transform.DORotate(shieldTransform.rotation.eulerAngles, timeToShieldTransform).SetEase(Ease.InCubic).SetDelay(delay);
	}

	private void OnReachedShield()
	{
		base.transform.SetParent(shieldTransform);
		base.transform.localRotation = Quaternion.identity;
		base.transform.localPosition = Vector3.zero;
		currentState = AMA_SHIELD_FRAGMENT_STATES.SHIELD;
	}

	public void Flash()
	{
		MasterShaderEffects componentInChildren = GetComponentInChildren<MasterShaderEffects>();
		componentInChildren.TriggerColorizeLerpInOut(0.01f, 0.2f);
	}

	public void ColorizeOut(float seconds = 0.2f)
	{
		MasterShaderEffects componentInChildren = GetComponentInChildren<MasterShaderEffects>();
		componentInChildren.TriggerColorizeLerp(1f, 0f, seconds);
	}

	public void RaiseFromGround(Vector2 originPoint, float timeToRaise)
	{
		currentState = AMA_SHIELD_FRAGMENT_STATES.RISING;
		base.transform.SetParent(null);
		float num = Random.Range(0.5f, 2f);
		base.transform.position = originPoint;
		SetAlpha(0f);
		base.transform.DOMoveY(originPoint.y + num, timeToRaise).SetEase(Ease.InOutCubic);
		FadeIn();
	}

	public void BreakFromShield(Vector2 dir)
	{
		currentState = AMA_SHIELD_FRAGMENT_STATES.BROKEN;
		base.transform.SetParent(null);
		base.transform.DOMove((Vector2)base.transform.position + dir, 0.6f).SetEase(Ease.OutCubic);
		FadeOut();
	}

	public void OnChargeInterrupted()
	{
		currentState = AMA_SHIELD_FRAGMENT_STATES.BROKEN;
	}

	public void SetAlpha(float a)
	{
		spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, a);
	}

	public void FadeOut(float seconds = 0.2f)
	{
		spr.DOFade(0f, 0.2f);
	}

	public void BlinkAlpha()
	{
		spr.DOFade(1f, 0.01f).OnComplete(delegate
		{
			spr.DOFade(0f, 0.5f);
		});
	}

	public void FadeIn(float seconds = 0.2f)
	{
		spr.DOFade(1f, seconds);
	}
}
