using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneSelectHook : MonoBehaviour
{
	public enum SceneFilterType
	{
		None,
		Directory,
		ExactDirectory,
		List,
		Creation
	}

	public string title;

	public bool excludeActiveScenes = true;

	public SceneFilterType filter;

	[SerializeField]
	[HideInInspectorIf("_hideDirectoyRef", false)]
	private SceneRef _directoryRef;

	[SerializeField]
	[HideInInspectorIf("_hideSceneList", false)]
	private List<SceneRef> _sceneList;

	private IEnumerable<SceneRef> _FilteredScenes()
	{
		return ((filter >= SceneFilterType.List || (bool)_directoryRef) ? filter : SceneFilterType.None) switch
		{
			SceneFilterType.None => SceneRef.GetAllScenesInBuild(includeSubScenes: false), 
			SceneFilterType.Directory => SceneRef.GetAllScenesInDirectory(_directoryRef.directory), 
			SceneFilterType.ExactDirectory => SceneRef.GetAllScenesInDirectory(_directoryRef.directory, includeSubScenes: true, includeSubDirectories: false), 
			SceneFilterType.List => _sceneList, 
			SceneFilterType.Creation => SceneRef.CreationSceneRefs, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private IEnumerable<SceneRef> _Scenes()
	{
		foreach (SceneRef item in _FilteredScenes())
		{
			if (!item.hideInSceneSelect && (!excludeActiveScenes || !item.isActive))
			{
				yield return item;
			}
		}
	}

	private Func<SceneRef, string> _SceneRefToString()
	{
		if (filter == SceneFilterType.Creation)
		{
			return SceneRef.RemoveCreationFromSceneName;
		}
		return null;
	}

	public void ShowSceneSelect(Transform parent, Vector2 center, Vector2 pivot, Action<SceneRef> onSceneSelected = null)
	{
		UIUtil.CreateSceneSelectPopup(_Scenes(), parent, title, center, pivot, onSceneSelected, _SceneRefToString());
	}

	public void ShowSceneSelect(Transform parent)
	{
		IEnumerable<SceneRef> scenes = _Scenes();
		string popupTitle = title;
		Func<SceneRef, string> toString = _SceneRefToString();
		UIUtil.CreateSceneSelectPopup(scenes, parent, popupTitle, null, null, null, toString);
	}

	public void ShowSceneSelect()
	{
		ShowSceneSelect(base.transform);
	}

	public SceneSelectHook SetData(SceneFilterType filter, string title = null)
	{
		this.filter = filter;
		this.title = title;
		return this;
	}
}
