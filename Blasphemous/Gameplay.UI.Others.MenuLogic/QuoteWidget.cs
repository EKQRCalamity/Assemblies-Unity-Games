using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class QuoteWidget : BaseMenuScreen
{
	private CanvasGroup canvasGroup;

	private float fadeInTime;

	private float timeActive;

	private float fadeOutTime;

	private Action onFinish;

	private bool IsClosing;

	public bool IsOpen { get; private set; }

	public void Awake()
	{
		IsOpen = false;
	}

	public void Open(float fadeInTime, float timeActive, float fadeOutTime, Action onFinish)
	{
		this.fadeInTime = fadeInTime;
		this.timeActive = timeActive;
		this.fadeOutTime = fadeOutTime;
		this.onFinish = onFinish;
		Open();
	}

	public override void Open()
	{
		base.Open();
		IsOpen = true;
		base.gameObject.SetActive(value: true);
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, fadeInTime).OnComplete(StartToWait);
	}

	public override void Close()
	{
		if (!IsClosing)
		{
			IsClosing = true;
			DOTween.To(() => canvasGroup.alpha, delegate(float x)
			{
				canvasGroup.alpha = x;
			}, 0f, fadeOutTime).OnComplete(OnClose);
			base.Close();
		}
	}

	protected override void OnClose()
	{
		IsOpen = false;
		base.gameObject.SetActive(value: false);
		IsClosing = false;
		onFinish();
	}

	private void StartToWait()
	{
		StartCoroutine(Wait());
	}

	private IEnumerator Wait()
	{
		yield return new WaitForSeconds(timeActive);
		if (IsOpen)
		{
			Close();
		}
	}
}
