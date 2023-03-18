using DG.Tweening;
using Framework.Managers;
using Gameplay.UI.Widgets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.UI.Others.MenuLogic;

[RequireComponent(typeof(CanvasGroup))]
public class BasicUIBlockingWidget : MonoBehaviour
{
	private const string BlockerName = "UIBLOCKING";

	private bool PauseGame = true;

	private float alphaDurantion = 0.4f;

	public bool IsFading { get; private set; }

	public void InitializeWidget()
	{
		IsFading = false;
		GetCanvas().alpha = 0f;
		Deactivate();
		OnWidgetInitialize();
	}

	public void FadeShow(bool checkInput, bool pauseGame = true, bool checkMainFade = true)
	{
		if (!IsActive() && !IsFading && (!pauseGame || !(SceneManager.GetActiveScene().name == "MainMenu")) && (!checkMainFade || !FadeWidget.instance.Fading) && (!checkInput || !Core.Input.InputBlocked || Core.Input.HasBlocker("PLAYER_LOGIC")))
		{
			IsFading = true;
			PauseGame = pauseGame;
			CanvasGroup canvas = GetCanvas();
			canvas.alpha = 0f;
			Activate();
			if (PauseGame)
			{
				Core.Input.SetBlocker("UIBLOCKING", blocking: true);
				Core.Logic.SetState(LogicStates.Unresponsive);
				Core.Logic.PauseGame();
				DOTween.defaultTimeScaleIndependent = true;
			}
			DOTween.To(() => canvas.alpha, delegate(float x)
			{
				canvas.alpha = x;
			}, 1f, alphaDurantion).OnComplete(EndFadeShow);
			OnWidgetShow();
		}
	}

	public void FadeHide()
	{
		if (IsActive() && !IsFading)
		{
			IsFading = true;
			CanvasGroup canvas = GetCanvas();
			canvas.alpha = 1f;
			if (PauseGame)
			{
				Core.Logic.ResumeGame();
				Core.Logic.SetState(LogicStates.Playing);
			}
			DOTween.defaultTimeScaleIndependent = true;
			DOTween.To(() => canvas.alpha, delegate(float x)
			{
				canvas.alpha = x;
			}, 0f, alphaDurantion).OnComplete(EndFadeHide);
		}
	}

	public void Hide()
	{
		if (IsActive() && !IsFading)
		{
			CanvasGroup canvas = GetCanvas();
			canvas.alpha = 0f;
			if (PauseGame)
			{
				Core.Logic.ResumeGame();
				Core.Logic.SetState(LogicStates.Playing);
			}
			EndFadeHide();
		}
	}

	protected virtual void OnWidgetInitialize()
	{
	}

	protected virtual void OnWidgetShow()
	{
	}

	protected virtual void OnWidgetHide()
	{
	}

	public virtual bool GoBack()
	{
		return false;
	}

	public virtual bool AutomaticBack()
	{
		return true;
	}

	public virtual bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	public virtual void Activate()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public virtual void Deactivate()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void EndFadeHide()
	{
		OnWidgetHide();
		IsFading = false;
		Deactivate();
		if (PauseGame)
		{
			Core.Input.SetBlocker("UIBLOCKING", blocking: false);
		}
	}

	private void EndFadeShow()
	{
		IsFading = false;
	}

	private CanvasGroup GetCanvas()
	{
		return GetComponent<CanvasGroup>();
	}
}
