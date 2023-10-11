using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRefHook : MonoBehaviour
{
	public SceneRef sceneRef;

	public void LoadScene()
	{
		LoadScene(sceneRef);
	}

	public void LoadSceneAdditive()
	{
		LoadSceneAdditive(sceneRef);
	}

	public void LoadScene(SceneRef scene)
	{
		if ((bool)scene)
		{
			LoadScreenView.Load(scene);
		}
	}

	public void LoadSceneAdditive(SceneRef scene)
	{
		if ((bool)scene)
		{
			LoadScreenView.Load(scene, LoadSceneMode.Additive);
		}
	}
}
