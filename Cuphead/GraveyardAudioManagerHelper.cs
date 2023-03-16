using System.Collections;
using UnityEngine;

public class GraveyardAudioManagerHelper : MonoBehaviour
{
	private bool exitingLevel;

	private string sceneName;

	private bool started;

	private static GraveyardAudioManagerHelper _instance;

	public static GraveyardAudioManagerHelper Instance => _instance;

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		sceneName = Scenes.scene_level_graveyard.ToString();
		base.transform.parent = null;
		Object.DontDestroyOnLoad(base.gameObject);
		SceneLoader.instance.ResetBgmVolume();
		AudioManager.PlayBGM();
	}

	private IEnumerator exit_level_cr()
	{
		AudioManager.ChangeBGMPitch(0.7f, 5f);
		while (SceneLoader.CurrentlyLoading)
		{
			yield return null;
		}
		AudioManager.ChangeBGMPitch(1f, 0f);
		yield return new WaitForEndOfFrame();
		SceneLoader.instance.ResetBgmVolume();
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (!exitingLevel && SceneLoader.CurrentlyLoading && SceneLoader.SceneName != sceneName)
		{
			exitingLevel = true;
			StartCoroutine(exit_level_cr());
		}
	}
}
