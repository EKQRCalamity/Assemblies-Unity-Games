using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapDLCUI : AbstractMonoBehaviour
{
	[SerializeField]
	private Text[] menuItems;

	private float _selectionTimer;

	private const float _SELECTION_TIME = 0.15f;

	private int _selection;

	private Color COLOR_SELECTED;

	private Color COLOR_INACTIVE;

	private bool inputEnabled;

	private CupheadInput.AnyPlayerInput input;

	private CanvasGroup canvasGroup;

	private int selection
	{
		get
		{
			return _selection;
		}
		set
		{
			bool flag = value > _selection;
			int num = (int)Mathf.Repeat(value, menuItems.Length);
			while (!menuItems[num].gameObject.activeSelf)
			{
				num = ((!flag) ? (num - 1) : (num + 1));
				num = (int)Mathf.Repeat(num, menuItems.Length);
			}
			_selection = num;
			UpdateSelection();
		}
	}

	public bool visible { get; private set; }

	public void Init(bool checkIfDead)
	{
		input = new CupheadInput.AnyPlayerInput(checkIfDead);
	}

	protected override void Awake()
	{
		base.Awake();
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		COLOR_SELECTED = menuItems[0].color;
		COLOR_INACTIVE = menuItems[menuItems.Length - 1].color;
	}

	private void OnDestroy()
	{
		PauseManager.Unpause();
	}

	private void Update()
	{
		if (!inputEnabled)
		{
			return;
		}
		if (GetButtonDown(CupheadButton.Accept))
		{
			MenuSelectSound();
			switch (selection)
			{
			case 0:
				ExitToTitle();
				break;
			case 1:
				Close();
				break;
			}
		}
		else if (_selectionTimer >= 0.15f)
		{
			if (GetButton(CupheadButton.MenuUp))
			{
				MenuMoveSound();
				selection--;
			}
			if (GetButton(CupheadButton.MenuDown))
			{
				MenuMoveSound();
				selection++;
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
		for (int i = 0; i < menuItems.Length; i++)
		{
			Text text = menuItems[i];
			if (i == selection)
			{
				text.color = COLOR_SELECTED;
			}
			else
			{
				text.color = COLOR_INACTIVE;
			}
		}
	}

	private void UpdateSelection()
	{
		_selectionTimer = 0f;
		for (int i = 0; i < menuItems.Length; i++)
		{
			Text text = menuItems[i];
			if (i == selection)
			{
				text.color = COLOR_SELECTED;
			}
			else
			{
				text.color = COLOR_INACTIVE;
			}
		}
	}

	private void ExitToTitle()
	{
		PlayerManager.ResetPlayers();
		Dialoguer.EndDialogue();
		SceneLoader.LoadScene(Scenes.scene_title, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
	}

	private void Close()
	{
		HideMenu();
	}

	public void ShowMenu()
	{
		visible = true;
		selection = 0;
		UpdateVerticalSelection();
		StartCoroutine(show_cr());
	}

	private IEnumerator show_cr()
	{
		float t = 0f;
		while (t < 0.2f)
		{
			float val = t / 0.2f;
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, val);
			t += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1f;
		yield return null;
		Interactable();
	}

	public void HideMenu()
	{
		StartCoroutine(hide_cr());
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		inputEnabled = false;
	}

	private IEnumerator hide_cr()
	{
		float t = 0f;
		while (t < 0.2f)
		{
			float val = t / 0.2f;
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, val);
			t += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 0f;
		yield return null;
		selection = 0;
		visible = false;
	}

	private void Interactable()
	{
		selection = 0;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		inputEnabled = true;
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

	protected void MenuSelectSound()
	{
		AudioManager.Play("level_menu_select");
	}

	protected void MenuMoveSound()
	{
		AudioManager.Play("level_menu_move");
	}
}
