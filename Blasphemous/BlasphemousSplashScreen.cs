using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class BlasphemousSplashScreen : MonoBehaviour
{
	public string postSplashScene = "Landing";

	private void Awake()
	{
		VideoPlayer component = GetComponent<VideoPlayer>();
		component.loopPointReached += delegate
		{
			SceneManager.LoadScene(postSplashScene, LoadSceneMode.Single);
		};
		component.prepareCompleted += delegate(VideoPlayer source)
		{
			source.Play();
		};
		component.Prepare();
	}
}
