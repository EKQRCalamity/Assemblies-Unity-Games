using System.Collections;
using UnityEngine;

public class ShmupTutorialExitSign : AbstractLevelInteractiveEntity
{
	private bool activated;

	protected override void Activate()
	{
		if (!activated)
		{
			base.Activate();
			StartCoroutine(go_cr());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CupheadTime.SetLayerSpeed(CupheadTime.Layer.Player, 1f);
	}

	private IEnumerator go_cr()
	{
		activated = true;
		PlayerData.SaveCurrentFile();
		CupheadTime.SetLayerSpeed(CupheadTime.Layer.Player, 0f);
		PlanePlayerController[] array = Object.FindObjectsOfType<PlanePlayerController>();
		foreach (PlanePlayerController planePlayerController in array)
		{
			planePlayerController.PauseAll();
		}
		PlaneSuperBomb[] array2 = Object.FindObjectsOfType<PlaneSuperBomb>();
		foreach (PlaneSuperBomb planeSuperBomb in array2)
		{
			planeSuperBomb.Pause();
		}
		PlaneSuperChalice[] array3 = Object.FindObjectsOfType<PlaneSuperChalice>();
		foreach (PlaneSuperChalice planeSuperChalice in array3)
		{
			planeSuperChalice.Pause();
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		SceneLoader.LoadScene(Scenes.scene_map_world_1, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
	}
}
