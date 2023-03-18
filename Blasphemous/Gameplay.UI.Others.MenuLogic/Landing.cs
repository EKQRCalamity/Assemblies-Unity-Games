using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.UI.Others.MenuLogic;

public class Landing : MonoBehaviour
{
	private enum MenuState
	{
		FADEIN,
		PRESS,
		FADEOUT,
		NOTHING
	}

	public bool preInitCore;

	public float waitToPress = 1f;

	public float waitToFadeout = 1f;

	[EventRef]
	public string soundPressKey = "event:/Key Event/RelicCollected";

	private const string ANIMATOR_FADEOUT = "FADEOUT";

	private AsyncOperation preloadMenu;

	private MenuState currentState = MenuState.NOTHING;

	public Animator landingAnimator;

	private float timeWaiting;

	private void Awake()
	{
		if (Application.runInBackground)
		{
			Debug.LogWarning("Run in background was true in landing! Correcting.");
		}
		Application.runInBackground = false;
		currentState = MenuState.FADEIN;
		timeWaiting = 0f;
		Time.maximumDeltaTime = 0.033f;
		Settings instance = Settings.Instance;
		if (instance.AutomaticEventLoading || instance.ImportType != 0)
		{
			Debug.LogError("*** FMODAudioManager, setting must be AutomaticEventLoading=false and ImportType=StreamingAssets");
		}
		else
		{
			try
			{
				foreach (string masterBank in instance.MasterBanks)
				{
					RuntimeManager.LoadBank(masterBank + ".strings", instance.AutomaticSampleLoading);
					RuntimeManager.LoadBank(masterBank, instance.AutomaticSampleLoading);
				}
				foreach (string bank in instance.Banks)
				{
					if (!bank.ToUpper().StartsWith("VOICEOVER_"))
					{
						RuntimeManager.LoadBank(bank, instance.AutomaticSampleLoading);
					}
				}
				RuntimeManager.WaitForAllLoads();
			}
			catch (BankLoadException exception)
			{
				Debug.LogException(exception);
			}
		}
		Cursor.visible = false;
	}

	private void Start()
	{
		string sceneName = "MainMenu_MAIN";
		preloadMenu = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		preloadMenu.allowSceneActivation = false;
		if (!Core.preinit && preInitCore)
		{
			Singleton<Core>.Instance.PreInit();
			Time.timeScale = 1f;
		}
	}

	private void Update()
	{
		switch (currentState)
		{
		case MenuState.FADEIN:
			timeWaiting += Time.deltaTime;
			if (timeWaiting >= waitToPress)
			{
				currentState = MenuState.PRESS;
			}
			break;
		case MenuState.PRESS:
			if (Input.anyKey || ReInput.players.GetPlayer(0).GetAnyButtonDown())
			{
				landingAnimator.SetTrigger("FADEOUT");
				RuntimeManager.PlayOneShot(soundPressKey);
				timeWaiting = 0f;
				currentState = MenuState.FADEOUT;
			}
			break;
		case MenuState.FADEOUT:
			timeWaiting += Time.deltaTime;
			if (timeWaiting > waitToFadeout)
			{
				currentState = MenuState.NOTHING;
				preloadMenu.allowSceneActivation = true;
			}
			break;
		}
	}
}
