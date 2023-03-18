using System;
using System.Collections;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

public class FadeWidget : UIWidget
{
	private Color ON_COLOR_BASE = new Color(0f, 0f, 0f, 1f);

	private Color ON_COLOR = new Color(0f, 0f, 0f, 1f);

	private Color OFF_COLOR = new Color(0f, 0f, 0f, 0f);

	public static FadeWidget instance;

	private Tween currentTween;

	private Image black;

	public bool FadingIn { get; private set; }

	public bool FadingOut { get; private set; }

	public bool IsActive => black.IsActive() && black.color.a > 0f;

	public bool Fading => currentTween != null;

	public static event Core.SimpleEvent OnFadeShowStart;

	public static event Core.SimpleEvent OnFadeShowEnd;

	public static event Core.SimpleEvent OnFadeHidedStart;

	public static event Core.SimpleEvent OnFadeHidedEnd;

	public void Awake()
	{
		black = GetComponent<Image>();
		instance = this;
		black.color = ON_COLOR;
	}

	public void Fade(bool toBlack, float duration = 1.5f, float delay = 0f, Core.SimpleEvent callback = null)
	{
		if (currentTween != null)
		{
			currentTween.Kill();
			currentTween = null;
		}
		if (toBlack)
		{
			Core.Input.SetBlocker("FADE", blocking: true);
		}
		FadingIn = !toBlack;
		FadingOut = toBlack;
		if (toBlack && FadeWidget.OnFadeShowStart != null)
		{
			FadeWidget.OnFadeShowStart();
		}
		if (!toBlack && FadeWidget.OnFadeHidedStart != null)
		{
			FadeWidget.OnFadeHidedStart();
		}
		Color target = ((!toBlack) ? OFF_COLOR : ON_COLOR);
		black.color = (toBlack ? OFF_COLOR : ON_COLOR);
		StartCoroutine(FadeAfterDelay(toBlack, duration, delay, target, callback));
	}

	public IEnumerator FadeCoroutine(bool toBlack, float duration = 1.5f, float delay = 0f)
	{
		if (currentTween != null)
		{
			currentTween.Kill();
			currentTween = null;
		}
		if (toBlack)
		{
			Core.Input.SetBlocker("FADE", blocking: true);
		}
		FadingIn = !toBlack;
		FadingOut = toBlack;
		if (toBlack && FadeWidget.OnFadeShowStart != null)
		{
			FadeWidget.OnFadeShowStart();
		}
		if (!toBlack && FadeWidget.OnFadeHidedStart != null)
		{
			FadeWidget.OnFadeHidedStart();
		}
		Color target = ((!toBlack) ? OFF_COLOR : ON_COLOR);
		black.color = (toBlack ? OFF_COLOR : ON_COLOR);
		yield return FadeAfterDelay(toBlack, duration, delay, target, null);
	}

	public void ClearFade()
	{
		black.color = OFF_COLOR;
	}

	private IEnumerator FadeAfterDelay(bool toBlack, float duration, float delay, Color target, Core.SimpleEvent callback)
	{
		yield return new WaitForSeconds(delay);
		currentTween = black.DOColor(target, duration).OnComplete(delegate
		{
			currentTween = null;
			FadingIn = false;
			FadingOut = false;
			if (!toBlack)
			{
				Core.Input.SetBlocker("FADE", blocking: false);
			}
			if (toBlack && FadeWidget.OnFadeShowEnd != null)
			{
				FadeWidget.OnFadeShowEnd();
			}
			if (!toBlack && FadeWidget.OnFadeHidedEnd != null)
			{
				FadeWidget.OnFadeHidedEnd();
			}
			ON_COLOR = ON_COLOR_BASE;
			black.color = target;
			if (callback != null)
			{
				callback();
			}
		});
		yield return currentTween;
		Log.Trace("Fade", "Called fade function. Active: " + toBlack);
	}

	public IEnumerator FadeCoroutine(Color originColor, Color targetColor, float seconds, bool toBlack, Action<bool> callback)
	{
		for (float counter = 0f; counter < seconds; counter += Time.deltaTime)
		{
			black.color = Color.Lerp(originColor, targetColor, counter / seconds);
			yield return null;
		}
		black.color = targetColor;
		callback?.Invoke(toBlack);
	}

	public void StartEasyFade(Color originColor, Color targetColor, float seconds, bool toBlack)
	{
		if (toBlack)
		{
			Core.Input.SetBlocker("FADE", blocking: true);
		}
		FadingIn = !toBlack;
		FadingOut = toBlack;
		if (toBlack && FadeWidget.OnFadeShowStart != null)
		{
			FadeWidget.OnFadeShowStart();
		}
		if (!toBlack && FadeWidget.OnFadeHidedStart != null)
		{
			FadeWidget.OnFadeHidedStart();
		}
		if (seconds > 0f)
		{
			StartCoroutine(FadeCoroutine(originColor, targetColor, seconds, toBlack, OnEasyFadeComplete));
			return;
		}
		black.color = targetColor;
		OnEasyFadeComplete(toBlack);
	}

	private void OnEasyFadeComplete(bool toBlack)
	{
		if (!toBlack)
		{
			Core.Input.SetBlocker("FADE", blocking: false);
		}
		FadingIn = false;
		FadingOut = false;
		if (toBlack && FadeWidget.OnFadeShowEnd != null)
		{
			FadeWidget.OnFadeShowEnd();
		}
		if (!toBlack && FadeWidget.OnFadeHidedEnd != null)
		{
			FadeWidget.OnFadeHidedEnd();
		}
	}

	public void FadeInOut(float duration, float delay, Core.SimpleEvent outEnd = null, Core.SimpleEvent inEnd = null)
	{
		Core.Input.SetBlocker("FADE", blocking: true);
		if (currentTween != null)
		{
			currentTween.Kill();
			currentTween = null;
		}
		Tweener t = black.DOColor(ON_COLOR, duration).OnComplete(delegate
		{
			if (outEnd != null)
			{
				outEnd();
			}
			ON_COLOR = ON_COLOR_BASE;
		});
		Tweener t2 = black.DOColor(OFF_COLOR, duration).OnComplete(delegate
		{
			if (inEnd != null)
			{
				inEnd();
			}
			if (!Core.Events.GetFlag("CHERUB_RESPAWN"))
			{
				Core.Input.SetBlocker("FADE", blocking: false);
			}
		});
		Sequence sequence = DOTween.Sequence();
		sequence.SetDelay(delay);
		sequence.Append(t);
		sequence.Append(t2);
		sequence.Play();
	}

	public void SetOffColor(Color color)
	{
		OFF_COLOR = color;
	}

	public void SetOnColor(Color color)
	{
		ON_COLOR = color;
		ON_COLOR_BASE = color;
	}

	public void ResetToBlack()
	{
		ON_COLOR = Color.black;
		ON_COLOR_BASE = Color.black;
	}
}
