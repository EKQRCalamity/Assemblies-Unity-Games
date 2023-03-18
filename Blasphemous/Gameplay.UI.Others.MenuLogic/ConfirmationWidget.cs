using System;
using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using I2.Loc;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class ConfirmationWidget : BaseMenuScreen
{
	[SerializeField]
	private Text infoText;

	[SerializeField]
	private Text acceptText;

	[SerializeField]
	private Text backText;

	[SerializeField]
	[EventRef]
	private string soundOnAccept = "event:/SFX/UI/ChangeTab";

	[SerializeField]
	[EventRef]
	private string soundOnBack = "event:/SFX/UI/ChangeTab";

	private const string BLOCKER_NAME = "UIBLOCKING_CONFIRMATION";

	private CanvasGroup canvasGroup;

	private Action onAccept;

	private Action onBack;

	private Player rewiredPlayer;

	private bool isOpen;

	private bool isClosing;

	private bool accepted;

	public void Open(string infoMessage, Action onAccept, Action onBack)
	{
		if (!isClosing)
		{
			string lABEL_BUTTON_ACCEPT = ScriptLocalization.UI_Map.LABEL_BUTTON_ACCEPT;
			string lABEL_BUTTON_BACK = ScriptLocalization.UI_Map.LABEL_BUTTON_BACK;
			Open(infoMessage, lABEL_BUTTON_ACCEPT, lABEL_BUTTON_BACK, onAccept, onBack);
		}
	}

	public void Open(string infoMessage, string acceptMessage, string backMessage, Action onAccept, Action onBack)
	{
		if (!isClosing)
		{
			this.onAccept = onAccept;
			this.onBack = onBack;
			infoText.text = infoMessage;
			acceptText.text = acceptMessage;
			backText.text = backMessage;
			Open();
		}
	}

	public override void Open()
	{
		if (!isClosing)
		{
			base.Open();
			accepted = false;
			rewiredPlayer = ReInput.players.GetPlayer(0);
			Core.Input.SetBlocker("UIBLOCKING_CONFIRMATION", blocking: true);
			base.gameObject.SetActive(value: true);
			canvasGroup = GetComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
			DOTween.To(() => canvasGroup.alpha, delegate(float x)
			{
				canvasGroup.alpha = x;
			}, 1f, 1f).OnComplete(OnOpen);
		}
	}

	protected override void OnOpen()
	{
		isOpen = true;
	}

	public override void Close()
	{
		isOpen = false;
		isClosing = true;
		if (accepted && onAccept != null)
		{
			onAccept();
		}
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 0f, 1f).OnComplete(OnClose);
		base.Close();
	}

	protected override void OnClose()
	{
		isClosing = false;
		Core.Input.SetBlocker("UIBLOCKING_CONFIRMATION", blocking: false);
		if (!accepted && onBack != null)
		{
			onBack();
		}
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (rewiredPlayer == null || !isOpen)
		{
			return;
		}
		if (rewiredPlayer.GetButtonDown(51))
		{
			accepted = false;
			if (soundOnAccept != string.Empty)
			{
				Core.Audio.PlayOneShot(soundOnAccept);
			}
			Close();
		}
		else if (rewiredPlayer.GetButtonDown(50))
		{
			accepted = true;
			if (soundOnBack != string.Empty)
			{
				Core.Audio.PlayOneShot(soundOnBack);
			}
			Close();
		}
	}
}
