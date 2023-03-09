using System.Collections;
using UnityEngine;

public class KitchenAudioManagerHelper : MonoBehaviour
{
	private bool exitingLevel;

	private string sceneName;

	private bool started;

	private static KitchenAudioManagerHelper _instance;

	public static KitchenAudioManagerHelper Instance => _instance;

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		sceneName = Scenes.scene_level_kitchen.ToString();
		base.transform.parent = null;
		Object.DontDestroyOnLoad(base.gameObject);
		SceneLoader.instance.ResetBgmVolume();
	}

	private IEnumerator exit_level_cr()
	{
		while (SceneLoader.CurrentlyLoading)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (!exitingLevel && SceneLoader.CurrentlyLoading && SceneLoader.SceneName != sceneName && SceneLoader.SceneName != Scenes.scene_cutscene_dlc_saltbaker_prebattle.ToString())
		{
			exitingLevel = true;
			StartCoroutine(exit_level_cr());
		}
	}
}
