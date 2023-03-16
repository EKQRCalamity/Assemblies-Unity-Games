using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestartTowerConfirmGUI : AbstractMonoBehaviour
{
	[Serializable]
	public class Button
	{
		public Text text;
	}

	[SerializeField]
	private GameObject mainObject;

	[SerializeField]
	private Button[] mainObjectButtons;

	private List<Button> currentItems;

	private CanvasGroup canvasGroup;

	private AbstractPauseGUI pauseMenu;

	private float _selectionTimer;

	private const float _SELECTION_TIME = 0.15f;

	private int _verticalSelection;

	private CupheadInput.AnyPlayerInput input;

	private int lastIndex;

	public static Color COLOR_SELECTED { get; private set; }

	public static Color COLOR_INACTIVE { get; private set; }

	public bool restartTowerConfirmMenuOpen { get; private set; }

	public bool inputEnabled { get; private set; }

	private int verticalSelection
	{
		get
		{
			return _verticalSelection;
		}
		set
		{
			bool flag = value > _verticalSelection;
			int num = (int)Mathf.Repeat(value, currentItems.Count);
			while (!currentItems[num].text.gameObject.activeSelf)
			{
				num = ((!flag) ? (num - 1) : (num + 1));
				num = (int)Mathf.Repeat(num, currentItems.Count);
			}
			_verticalSelection = num;
			UpdateVerticalSelection();
		}
	}

	public bool justClosed { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		restartTowerConfirmMenuOpen = false;
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		currentItems = new List<Button>(mainObjectButtons);
		COLOR_SELECTED = currentItems[0].text.color;
		COLOR_INACTIVE = currentItems[currentItems.Count - 1].text.color;
	}

	public void Init(bool checkIfDead)
	{
		input = new CupheadInput.AnyPlayerInput(checkIfDead);
	}

	private void Update()
	{
		justClosed = false;
		if (!inputEnabled)
		{
			return;
		}
		if (GetButtonDown(CupheadButton.Pause) || GetButtonDown(CupheadButton.Cancel))
		{
			MenuSelectSound();
			HideMenu();
		}
		else if (GetButtonDown(CupheadButton.Accept))
		{
			MenuSelectSound();
			switch (verticalSelection)
			{
			case 0:
				RestartTower();
				break;
			case 1:
				ToPauseMenu();
				break;
			}
		}
		else if (_selectionTimer >= 0.15f)
		{
			if (GetButton(CupheadButton.MenuUp))
			{
				MenuSelectSound();
				verticalSelection--;
			}
			if (GetButton(CupheadButton.MenuDown))
			{
				MenuSelectSound();
				verticalSelection++;
			}
		}
		else
		{
			_selectionTimer += Time.deltaTime;
		}
	}

	private void UpdateVerticalSelection()
	{
		_selectionTimer = 0f;
		for (int i = 0; i < currentItems.Count; i++)
		{
			Button button = currentItems[i];
			if (i == verticalSelection)
			{
				button.text.color = COLOR_SELECTED;
			}
			else
			{
				button.text.color = COLOR_INACTIVE;
			}
		}
	}

	public void ShowMenu()
	{
		restartTowerConfirmMenuOpen = true;
		verticalSelection = 0;
		canvasGroup.alpha = 1f;
		FrameDelayedCallback(Interactable, 1);
		UpdateVerticalSelection();
	}

	public void HideMenu()
	{
		verticalSelection = 0;
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		inputEnabled = false;
		restartTowerConfirmMenuOpen = false;
		justClosed = true;
	}

	private void Interactable()
	{
		verticalSelection = 0;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		inputEnabled = true;
	}

	protected void MenuSelectSound()
	{
		AudioManager.Play("level_menu_select");
	}

	private void ToPauseMenu()
	{
		restartTowerConfirmMenuOpen = false;
		HideMenu();
	}

	private void RestartTower()
	{
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
		PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, canSwitch: false);
		SceneLoader.ResetTheTowerOfPower();
		Dialoguer.EndDialogue();
	}

	protected bool GetButtonDown(CupheadButton button)
	{
		if (input.GetButtonDown(button))
		{
			AudioManager.Play("level_menu_select");
			return true;
		}
		return false;
	}

	protected bool GetButton(CupheadButton button)
	{
		if (input.GetButton(button))
		{
			return true;
		}
		return false;
	}
}
