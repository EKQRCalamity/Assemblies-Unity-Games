using System.Collections;
using UnityEngine;

public class GameStepUnloadUnusedResources : GameStep
{
	protected override IEnumerator Update()
	{
		AsyncOperation asyncOperation = Resources.UnloadUnusedAssets();
		while (!asyncOperation.isDone)
		{
			yield return null;
		}
	}
}
