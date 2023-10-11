using UnityEngine;

public class CreationCardHook : MonoBehaviour
{
	private SceneSelectHook _sceneSelect;

	private SceneSelectHook sceneSelect => this.CacheComponentSafe(ref _sceneSelect).SetData(SceneSelectHook.SceneFilterType.Creation, "Create");

	public void ShowCreationInterface()
	{
		sceneSelect.ShowSceneSelect(CameraManager.Instance.GetUICanvasTransform());
	}
}
