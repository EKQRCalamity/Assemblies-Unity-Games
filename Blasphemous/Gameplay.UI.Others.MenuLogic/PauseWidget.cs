using DG.Tweening;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class PauseWidget : BasicUIBlockingWidget
{
	public enum ChildWidgets
	{
		MAP,
		OPTIONS
	}

	public enum MapModes
	{
		SHOW,
		TELEPORT
	}

	public NewMapMenuWidget MapWidget;

	public OptionsWidget OptionsWidget;

	private bool isFadingChild;

	private bool isActive;

	private float alphaDurantionChild = 0.2f;

	private CanvasGroup canvasMap;

	private CanvasGroup canvasOptions;

	public ChildWidgets CurrentWidget { get; private set; }

	public ChildWidgets InitialWidget { get; set; }

	public MapModes InitialMapMode { get; set; }

	protected override void OnWidgetInitialize()
	{
		InitialWidget = ChildWidgets.MAP;
		InitialMapMode = MapModes.SHOW;
	}

	public void ReadOptionConfigurations()
	{
		OptionsWidget.Initialize();
	}

	protected override void OnWidgetShow()
	{
		canvasMap = MapWidget.GetComponent<CanvasGroup>();
		canvasOptions = OptionsWidget.GetComponent<CanvasGroup>();
		SetChildActive(canvasMap, InitialWidget == ChildWidgets.MAP);
		SetChildActive(canvasOptions, InitialWidget == ChildWidgets.OPTIONS);
		CurrentWidget = InitialWidget;
		MapWidget.Initialize();
		OptionsWidget.Initialize();
		if (InitialWidget == ChildWidgets.MAP)
		{
			MapWidget.OnShow(InitialMapMode);
		}
		else
		{
			OptionsWidget.OnShow(optionsIsInitial: true);
		}
	}

	public override bool GoBack()
	{
		if (isFadingChild)
		{
			return true;
		}
		if (CurrentWidget == ChildWidgets.MAP)
		{
			if (!MapWidget.GoBack())
			{
				InitialWidget = ChildWidgets.MAP;
				return false;
			}
		}
		else if (!OptionsWidget.GoBack())
		{
			if (InitialWidget != 0)
			{
				InitialWidget = ChildWidgets.MAP;
				return false;
			}
			CurrentWidget = ChildWidgets.MAP;
			SwitchElements();
		}
		return true;
	}

	public void CenterView()
	{
		if (CurrentWidget == ChildWidgets.MAP)
		{
			MapWidget.CenterView();
		}
	}

	public void UITabLeft()
	{
		if (CurrentWidget == ChildWidgets.MAP)
		{
			MapWidget.UITabLeft();
		}
		else
		{
			OptionsWidget.SelectPreviousTutorial();
		}
	}

	public void UITabRight()
	{
		if (CurrentWidget == ChildWidgets.MAP)
		{
			MapWidget.UITabRight();
		}
		else
		{
			OptionsWidget.SelectNextTutorial();
		}
	}

	public void SubmitPressed()
	{
		if (CurrentWidget == ChildWidgets.MAP)
		{
			MapWidget.SubmitPressed();
		}
	}

	public bool ChangeToOptions()
	{
		if (CurrentWidget != 0)
		{
			return false;
		}
		if (MapWidget.CurrentMapMode == MapModes.TELEPORT)
		{
			return false;
		}
		CurrentWidget = ChildWidgets.OPTIONS;
		SwitchElements();
		return true;
	}

	public OptionsWidget.SCALING_STRATEGY GetScalingStrategy()
	{
		return OptionsWidget.GetScalingStrategy();
	}

	private void SetChildActive(CanvasGroup group, bool value)
	{
		group.alpha = ((!value) ? 0f : 1f);
		group.gameObject.SetActive(value);
	}

	private void SwitchElements()
	{
		isFadingChild = true;
		canvasOptions.gameObject.SetActive(value: true);
		canvasMap.gameObject.SetActive(value: true);
		CanvasGroup canvasTo1 = canvasOptions;
		CanvasGroup canvasTo0 = canvasMap;
		if (CurrentWidget == ChildWidgets.MAP)
		{
			canvasTo1 = canvasMap;
			canvasTo0 = canvasOptions;
			MapWidget.OnShow(InitialMapMode);
		}
		else
		{
			OptionsWidget.OnShow(optionsIsInitial: false);
		}
		DOTween.To(() => canvasTo0.alpha, delegate(float x)
		{
			canvasTo0.alpha = x;
		}, 0f, alphaDurantionChild).OnComplete(EndFading);
		DOTween.To(() => canvasTo1.alpha, delegate(float x)
		{
			canvasTo1.alpha = x;
		}, 1f, alphaDurantionChild);
	}

	private void EndFading()
	{
		isFadingChild = false;
		if (CurrentWidget == ChildWidgets.MAP)
		{
			canvasOptions.gameObject.SetActive(value: false);
		}
		else
		{
			canvasMap.gameObject.SetActive(value: false);
		}
	}

	public override bool IsActive()
	{
		return isActive;
	}

	public override void Activate()
	{
		base.Activate();
		base.gameObject.transform.localPosition = Vector3.zero;
		isActive = true;
	}

	public override void Deactivate()
	{
		base.gameObject.transform.localPosition = Vector3.up * 1000f;
		isActive = false;
	}
}
