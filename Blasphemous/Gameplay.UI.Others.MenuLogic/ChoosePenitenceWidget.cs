using System;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class ChoosePenitenceWidget : BaseMenuScreen
{
	[SerializeField]
	private Text penitenceTitle;

	[SerializeField]
	private Text penitenceInfoText;

	[SerializeField]
	private CustomScrollView penitenceScroll;

	[SerializeField]
	private List<Button> buttons;

	private const string BLOCKER_NAME = "UIBLOCKING_PENITENCE";

	private CanvasGroup canvasGroup;

	private Action onChoosingPenitence;

	private Action onContinueWithoutChoosingPenitence;

	public void Open(Action onChoosingPenitence, Action onContinueWithoutChoosingPenitence)
	{
		this.onChoosingPenitence = onChoosingPenitence;
		this.onContinueWithoutChoosingPenitence = onContinueWithoutChoosingPenitence;
		Open();
	}

	public override void Open()
	{
		base.Open();
		Core.Input.SetBlocker("UIBLOCKING_PENITENCE", blocking: true);
		base.gameObject.SetActive(value: true);
		canvasGroup = GetComponent<CanvasGroup>();
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, 1f);
	}

	public override void Close()
	{
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 0f, 1f).OnComplete(OnClose);
		base.Close();
	}

	protected override void OnClose()
	{
		Core.Input.SetBlocker("UIBLOCKING_PENITENCE", blocking: false);
		base.gameObject.SetActive(value: false);
		ResetButtonsNavigation();
	}

	public void Option_SelectPE01()
	{
		penitenceTitle.text = ScriptLocalization.UI_Penitences.PE01_NAME;
		penitenceInfoText.text = ScriptLocalization.UI_Penitences.PE01_INFO;
		penitenceScroll.NewContentSetted();
	}

	public void Option_SelectPE02()
	{
		penitenceTitle.text = ScriptLocalization.UI_Penitences.PE02_NAME;
		penitenceInfoText.text = ScriptLocalization.UI_Penitences.PE02_INFO;
		penitenceScroll.NewContentSetted();
	}

	public void Option_SelectPE03()
	{
		penitenceTitle.text = ScriptLocalization.UI_Penitences.PE03_NAME;
		penitenceInfoText.text = ScriptLocalization.UI_Penitences.PE03_INFO;
		penitenceScroll.NewContentSetted();
	}

	public void Option_SelectNoPenitence()
	{
		penitenceTitle.text = ScriptLocalization.UI_Penitences.NO_PENITENCE;
		penitenceInfoText.text = ScriptLocalization.UI_Penitences.NO_PENITENCE_INFO;
		penitenceScroll.NewContentSetted();
	}

	public void Option_ActivatePE01()
	{
		SetButtonsNavigationMode(buttons, Navigation.Mode.None);
		UIController.instance.ShowConfirmationWidget(ScriptLocalization.UI_Penitences.CHOOSE_PENITENCE_CONFIRMATION, ActivatePenitencePE01AndClose, ResetButtonsNavigation);
	}

	public void Option_ActivatePE02()
	{
		SetButtonsNavigationMode(buttons, Navigation.Mode.None);
		UIController.instance.ShowConfirmationWidget(ScriptLocalization.UI_Penitences.CHOOSE_PENITENCE_CONFIRMATION, ActivatePenitencePE02AndClose, ResetButtonsNavigation);
	}

	public void Option_ActivatePE03()
	{
		SetButtonsNavigationMode(buttons, Navigation.Mode.None);
		UIController.instance.ShowConfirmationWidget(ScriptLocalization.UI_Penitences.CHOOSE_PENITENCE_CONFIRMATION, ActivatePenitencePE03AndClose, ResetButtonsNavigation);
	}

	public void Option_ContinueWithNoPenitence()
	{
		SetButtonsNavigationMode(buttons, Navigation.Mode.None);
		UIController.instance.ShowConfirmationWidget(ScriptLocalization.UI_Penitences.CHOOSE_NO_PENITENCE_CONFIRMATION, ContinueWithNoPenitenceAndClose, ResetButtonsNavigation);
	}

	private void ActivatePenitencePE01AndClose()
	{
		Core.PenitenceManager.ActivatePE01();
		onChoosingPenitence();
		Close();
	}

	private void ActivatePenitencePE02AndClose()
	{
		Core.PenitenceManager.ActivatePE02();
		onChoosingPenitence();
		Close();
	}

	private void ActivatePenitencePE03AndClose()
	{
		Core.PenitenceManager.ActivatePE03();
		onChoosingPenitence();
		Close();
	}

	private void ContinueWithNoPenitenceAndClose()
	{
		onContinueWithoutChoosingPenitence();
		Close();
	}

	private void ResetButtonsNavigation()
	{
		SetButtonsNavigationMode(buttons, Navigation.Mode.Explicit);
	}

	private void SetButtonsNavigationMode(List<Button> buttons, Navigation.Mode mode)
	{
		foreach (Button button in buttons)
		{
			Navigation navigation = button.navigation;
			navigation.mode = mode;
			button.navigation = navigation;
		}
	}
}
