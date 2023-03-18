using System;
using FMODUnity;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Rewired;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class IntroDemakeWidget : BaseMenuScreen
{
	[SerializeField]
	[EventRef]
	private string soundOnAccept = "event:/SFX/UI/ChangeTab";

	private const string BLOCKER_NAME = "UIBLOCKING_CONFIRMATION";

	private CanvasGroup canvasGroup;

	private Action onAccept;

	private Player rewiredPlayer;

	private Animator animator;

	private bool isOpen;

	private bool isClosing;

	private bool accepted;

	public void Open(Action onAccept)
	{
		this.onAccept = onAccept;
		Open();
	}

	public override void Open()
	{
		base.Open();
		if (rewiredPlayer == null)
		{
			rewiredPlayer = ReInput.players.GetPlayer(0);
		}
		if (canvasGroup == null)
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
		accepted = false;
		isClosing = false;
		Core.Input.SetBlocker("UIBLOCKING_CONFIRMATION", blocking: true);
		base.gameObject.SetActive(value: true);
		FadeWidget.instance.StartEasyFade(Color.black, new Color(0f, 0f, 0f, 0f), 0.2f, toBlack: false);
		canvasGroup.alpha = 1f;
		OnOpen();
	}

	protected override void OnOpen()
	{
		isOpen = true;
		animator.SetTrigger("INTRO");
	}

	public override void Close()
	{
		isOpen = false;
		base.Close();
		OnClose();
	}

	protected override void OnClose()
	{
		Core.Input.SetBlocker("UIBLOCKING_CONFIRMATION", blocking: false);
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (rewiredPlayer != null && isOpen && !isClosing && rewiredPlayer.GetButtonDown(50))
		{
			accepted = true;
			isClosing = true;
			onAccept();
			Core.Audio.PlaySfx(soundOnAccept);
			Core.Audio.StopNamedSound("DEMAKE_INTRO");
			animator.SetTrigger("PRESS_START");
		}
	}
}
