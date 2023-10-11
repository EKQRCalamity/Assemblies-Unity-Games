using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LaunchManager : MonoBehaviour
{
	public static bool InGame { get; private set; }

	private IEnumerator _Load()
	{
		yield return null;
		if (Steam.Enabled)
		{
			Debug.Log("STEAM ENABLED");
		}
		yield return null;
		Ids<ATarget>.TriggerStaticConstructor = true;
		DataRef<AdventureData>.LoadAll();
		DataRef<TutorialData>.LoadAll();
		DataRef<GameData>.LoadAll();
		DataRef<CharacterData>.LoadAll();
		DataRef<AbilityData>.LoadAll();
		DataRef<AbilityDeckData>.Warmup();
		DataRef<ProjectileMediaData>.Warmup();
		DataRef<MessageData>.LoadAll();
		ReflectionUtil.PreJitAll(typeof(Poker));
		ReflectionUtil.PreJitAll<ATarget>(jitSubClasses: true);
		ReflectionUtil.PreJitAll<GameStep>(jitSubClasses: true);
		ReflectionUtil.PreJitAll<GameStepGroup>(jitSubClasses: true);
		ProfileManager.progress.UnlockDefaultDecks();
		ProfileManager.progress.UnlockDefaultGames();
		yield return null;
		if (Input.GetKey(KeyCode.F8))
		{
			Debug.Log("ENTERING SAFE MODE");
			ProfileManager.options.video.SetSafeModeOptions();
			yield return null;
		}
		else if (Input.GetKey(KeyCode.F7))
		{
			Debug.Log("ENTERING PERFORMANCE MODE");
			ProfileManager.options.video.SetPerformanceOptions();
			yield return null;
		}
		AsyncOperation load = SceneManager.LoadSceneAsync(1);
		while (!load.isDone)
		{
			yield return null;
		}
	}

	private void Awake()
	{
		InGame = true;
	}

	private void Start()
	{
		StartCoroutine(_Load());
	}
}
