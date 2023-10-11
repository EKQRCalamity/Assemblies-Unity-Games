using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class GameStepGroupTransitionToAdventureTable : GameStepGroup
{
	protected GameManager manager => GameManager.Instance;

	protected override IEnumerable<GameStep> _GetSteps()
	{
		yield return new GameStepGenericSimple(delegate
		{
			manager.establishCamera.SetDollyTrack(manager.adventureTrack);
		});
		yield return new GameStepTimeline(manager.establishShotToGame, manager.adventureLookAt);
		yield return new GameStepVirtualCamera(manager.adventureCamera, 0f);
		yield return new GameStepGenericSimple(delegate
		{
			Camera.main.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
		});
		yield return new GameStepWaitFrame();
		yield return new GameStepStateChange(manager.adventureState);
		yield return new GameStepUnloadSceneAsync(manager.cosmeticScene);
		yield return new GameStepSetupAdventureRendering();
	}
}
