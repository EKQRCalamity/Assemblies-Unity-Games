using System;
using UnityEngine;

public class CreationNavigationView : MonoBehaviour
{
	public SceneSelectHook sceneSelect;

	[Range(1f, 10f)]
	public float requestVisibilityDuration = 3f;

	public FloatEvent onRequestVisibility;

	public StringEvent onCurrentSceneNameChange;

	public StringEvent onBackSceneNameChange;

	private CanvasInputFocus _canvasInputFocus;

	private void _OnSceneTransitionRequested(Action actionAfterUnsavedChangesChecked)
	{
		using PoolKeepItemListHandle<IDataRefControl> poolKeepItemListHandle = Pools.UseKeepItemList(DataRefControl.GetActiveControlsWithUnsavedChanges());
		if (poolKeepItemListHandle.Count == 0)
		{
			actionAfterUnsavedChangesChecked();
			return;
		}
		UIUtil.CreatePopup("Unsaved Changes", UIUtil.CreateMessageBox("There are unsaved changes to data in this scene. Would you like to save before exiting?"), null, parent: base.transform, buttons: new string[2] { "Save Changes", "Discard Changes" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (s != "Save Changes")
			{
				actionAfterUnsavedChangesChecked();
			}
			else
			{
				SceneRef.DisableEventSystemUntilSceneTransition();
				DataRefControl.SaveAllActiveControls();
				Job.Process(Job.WaitForDepartmentEmpty(Department.Content)).Immediately().Do(actionAfterUnsavedChangesChecked);
			}
		});
	}

	private void Start()
	{
		onCurrentSceneNameChange.Invoke(SceneRef.ActiveScene.sceneName);
		onBackSceneNameChange.Invoke(SceneRef.RemoveCreationFromSceneName(SceneRef.GetCreationBackSceneRef(removeFromBreadCrumbs: false)));
	}

	private void Update()
	{
		if (InputManager.I[KeyCode.Escape][KState.Clicked] && this.CacheComponentInParent(ref _canvasInputFocus).HasFocusPermissive())
		{
			onRequestVisibility.Invoke(requestVisibilityDuration);
		}
	}

	public void MainMenu()
	{
		_OnSceneTransitionRequested(delegate
		{
			LoadScreenView.Load(SceneRef.MainMenu);
		});
	}

	public void Back()
	{
		_OnSceneTransitionRequested(delegate
		{
			LoadScreenView.Load(SceneRef.GetCreationBackSceneRef(removeFromBreadCrumbs: true));
		});
	}

	public void SceneSelect()
	{
		sceneSelect.ShowSceneSelect(sceneSelect.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), delegate(SceneRef sceneRef)
		{
			_OnSceneTransitionRequested(delegate
			{
				LoadScreenView.Load(sceneRef);
			});
		});
	}
}
