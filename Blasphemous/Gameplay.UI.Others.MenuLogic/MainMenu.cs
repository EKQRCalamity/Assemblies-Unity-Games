using System.Collections;
using System.Diagnostics;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.UI.Others.MenuLogic;

public class MainMenu : MonoBehaviour
{
	private enum MenuState
	{
		IDLE,
		DISCLAIMER,
		DISCLAIMER_TEXT,
		FADEOUT
	}

	private bool axisAvailable;

	public static MainMenu instance;

	public GameObject importMenu;

	public GameObject successMenu;

	public string initialScene;

	public float disclaimerWait = 5f;

	public float fadingWait = 0.5f;

	private const string ANIMATOR_STATE = "STATE";

	private MenuState currentState;

	private Animator animator;

	private KeepFocus keepFocus;

	private float timeWaiting;

	private bool isContinue;

	private void Awake()
	{
		instance = this;
		animator = GetComponent<Animator>();
		keepFocus = GetComponent<KeepFocus>();
		SetState(MenuState.IDLE);
	}

	private void Update()
	{
		switch (currentState)
		{
		case MenuState.IDLE:
			break;
		case MenuState.DISCLAIMER:
			timeWaiting += Time.deltaTime;
			if (timeWaiting > disclaimerWait)
			{
				SetState(MenuState.DISCLAIMER_TEXT);
			}
			break;
		case MenuState.DISCLAIMER_TEXT:
		{
			Player player = ReInput.players.GetPlayer(0);
			if (player.GetButtonDown(8) || player.GetButtonDown(5) || player.GetButtonDown(7) || player.GetButtonDown(6) || Input.anyKey)
			{
				timeWaiting = 0f;
				SetState(MenuState.FADEOUT);
			}
			break;
		}
		case MenuState.FADEOUT:
			timeWaiting += Time.deltaTime;
			if (timeWaiting > fadingWait)
			{
				InternalPlay();
			}
			break;
		}
	}

	private void Start()
	{
		SetState(MenuState.IDLE);
		Core.Logic.CameraManager.ProCamera2D.gameObject.SetActive(value: false);
	}

	public void Continue()
	{
		if (currentState == MenuState.IDLE)
		{
			keepFocus.enabled = false;
			EventSystem.current.SetSelectedGameObject(null);
			Log.Trace("Continue pressed, starting the game...");
			isContinue = Core.Persistence.ExistSlot(PersistentManager.GetAutomaticSlot());
			SetState(MenuState.DISCLAIMER);
		}
	}

	public void Play()
	{
		if (currentState == MenuState.IDLE)
		{
			keepFocus.enabled = false;
			isContinue = false;
			EventSystem.current.SetSelectedGameObject(null);
			Log.Trace("Play pressed, starting the game...");
			SetState(MenuState.DISCLAIMER);
		}
	}

	public void OpenImportMenu()
	{
		if (currentState == MenuState.IDLE)
		{
			importMenu.SetActive(value: true);
			base.gameObject.SetActive(value: false);
			PlayerPrefs.SetInt("SOULS_IMPORTED", 1);
		}
	}

	public void CloseImportMenu()
	{
		if (currentState == MenuState.IDLE)
		{
			importMenu.SetActive(value: false);
			base.gameObject.SetActive(value: true);
		}
	}

	public void ExitSucessMenu()
	{
		if (currentState == MenuState.IDLE)
		{
			successMenu.SetActive(value: false);
			base.gameObject.SetActive(value: true);
		}
	}

	public void ChangeLanguage()
	{
		if (currentState == MenuState.IDLE)
		{
			Core.Localization.SetNextLanguage();
		}
	}

	public void ExitGame()
	{
		if (currentState == MenuState.IDLE && !FadeWidget.instance.Fading)
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	private IEnumerator LoadFirstScene()
	{
		FadeWidget.instance.Fade(toBlack: true);
		yield return new WaitForSeconds(0.3f);
		Core.Logic.ResetAllData();
		if (!isContinue)
		{
		}
	}

	private void OnImportFinished()
	{
		importMenu.SetActive(value: false);
		successMenu.SetActive(value: true);
	}

	private void InternalPlay()
	{
		StartCoroutine(LoadFirstScene());
	}

	private void SetState(MenuState state)
	{
		currentState = state;
		animator.SetInteger("STATE", (int)currentState);
	}
}
