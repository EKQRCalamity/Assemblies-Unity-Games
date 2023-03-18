using System;
using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using Framework.Penitences;
using I2.Loc;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class AbandonPenitenceWidget : BaseMenuScreen
{
	[SerializeField]
	private Text penitenceTitle;

	[SerializeField]
	private Text penitenceInfoText;

	[SerializeField]
	private GameObject PE01Medal;

	[SerializeField]
	private GameObject PE02Medal;

	[SerializeField]
	private GameObject PE03Medal;

	[SerializeField]
	[EventRef]
	private string soundOnAbandoning = "event:/SFX/UI/ChangeTab";

	[SerializeField]
	[EventRef]
	private string soundOnExiting = "event:/SFX/UI/ChangeTab";

	private const string BLOCKER_NAME = "UIBLOCKING_PENITENCE";

	private CanvasGroup canvasGroup;

	private Action onAbandoningPenitence;

	private Action onContinueWithoutAbandoningPenitence;

	private Player rewiredPlayer;

	private bool isOpen;

	private bool isConfirmationPopupOpen;

	public void Open(Action onAbandoningPenitence, Action onContinueWithoutAbandoningPenitence)
	{
		this.onAbandoningPenitence = onAbandoningPenitence;
		this.onContinueWithoutAbandoningPenitence = onContinueWithoutAbandoningPenitence;
		Open();
	}

	public override void Open()
	{
		base.Open();
		Core.Input.SetBlocker("UIBLOCKING_PENITENCE", blocking: true);
		base.gameObject.SetActive(value: true);
		rewiredPlayer = ReInput.players.GetPlayer(0);
		UpdatePenitenceTextsAndDisplayedMedal();
		canvasGroup = GetComponent<CanvasGroup>();
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, 1f).OnComplete(OnOpen);
	}

	protected override void OnOpen()
	{
		isOpen = true;
	}

	public override void Close()
	{
		isOpen = false;
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
	}

	private void Update()
	{
		if (rewiredPlayer == null || !isOpen || isConfirmationPopupOpen)
		{
			return;
		}
		if (rewiredPlayer.GetButtonDown(51))
		{
			if (onContinueWithoutAbandoningPenitence != null)
			{
				onContinueWithoutAbandoningPenitence();
			}
			if (soundOnExiting != string.Empty)
			{
				Core.Audio.PlayOneShot(soundOnExiting);
			}
			Close();
		}
		else if (rewiredPlayer.GetButtonDown(50))
		{
			ChooseToAbandonPenitence();
		}
	}

	private void UpdatePenitenceTextsAndDisplayedMedal()
	{
		IPenitence currentPenitence = Core.PenitenceManager.GetCurrentPenitence();
		if (currentPenitence is PenitencePE01)
		{
			penitenceTitle.text = ScriptLocalization.UI_Penitences.PE01_NAME;
			penitenceInfoText.text = ScriptLocalization.UI_Penitences.PE01_INFO;
			PE01Medal.SetActive(value: true);
			PE02Medal.SetActive(value: false);
			PE03Medal.SetActive(value: false);
		}
		else if (currentPenitence is PenitencePE02)
		{
			penitenceTitle.text = ScriptLocalization.UI_Penitences.PE02_NAME;
			penitenceInfoText.text = ScriptLocalization.UI_Penitences.PE02_INFO;
			PE01Medal.SetActive(value: false);
			PE02Medal.SetActive(value: true);
			PE03Medal.SetActive(value: false);
		}
		else if (currentPenitence is PenitencePE03)
		{
			penitenceTitle.text = ScriptLocalization.UI_Penitences.PE03_NAME;
			penitenceInfoText.text = ScriptLocalization.UI_Penitences.PE03_INFO;
			PE01Medal.SetActive(value: false);
			PE02Medal.SetActive(value: false);
			PE03Medal.SetActive(value: true);
		}
		else
		{
			Debug.LogError("AbandonPenitenceWidget::UpdatePenitenceTexts: Current Penitence is not one of the first three!");
		}
	}

	private void ChooseToAbandonPenitence()
	{
		if (soundOnAbandoning != string.Empty)
		{
			Core.Audio.PlayOneShot(soundOnAbandoning);
		}
		UIController.instance.ShowConfirmationWidget(ScriptLocalization.UI_Penitences.CHOOSE_PENITENCE_ABANDON, AbandonPenitence, ContinueAfterClosingConfirmationPopup);
		isConfirmationPopupOpen = true;
	}

	private void AbandonPenitence()
	{
		ContinueAfterClosingConfirmationPopup();
		Core.PenitenceManager.MarkCurrentPenitenceAsAbandoned();
		if (onAbandoningPenitence != null)
		{
			onAbandoningPenitence();
		}
		Close();
	}

	private void ContinueAfterClosingConfirmationPopup()
	{
		isConfirmationPopupOpen = false;
	}
}
