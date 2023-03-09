using System;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeSceneAssetDatabase : ScriptableObject
{
	[Serializable]
	public class SceneAssetMapping
	{
		public string sceneName;

		public string[] assetNames;
	}

	public string[] INTERNAL_persistentAssetNames;

	public string[] INTERNAL_persistentAssetNamesDLC;

	public SceneAssetMapping[] INTERNAL_sceneAssetMappings;

	private HashSet<string> _persistentAssets;

	private HashSet<string> _persistentAssetsDLC;

	private Dictionary<string, string[]> _sceneAssetMappings;

	public HashSet<string> persistentAssets
	{
		get
		{
			if (_persistentAssets == null)
			{
				_persistentAssets = new HashSet<string>(INTERNAL_persistentAssetNames);
			}
			return _persistentAssets;
		}
	}

	public HashSet<string> persistentAssetsDLC
	{
		get
		{
			if (_persistentAssetsDLC == null)
			{
				_persistentAssetsDLC = new HashSet<string>(INTERNAL_persistentAssetNamesDLC);
			}
			return _persistentAssetsDLC;
		}
	}

	public Dictionary<string, string[]> sceneAssetMappings
	{
		get
		{
			if (_sceneAssetMappings == null)
			{
				_sceneAssetMappings = new Dictionary<string, string[]>();
				SceneAssetMapping[] iNTERNAL_sceneAssetMappings = INTERNAL_sceneAssetMappings;
				foreach (SceneAssetMapping sceneAssetMapping in iNTERNAL_sceneAssetMappings)
				{
					_sceneAssetMappings.Add(sceneAssetMapping.sceneName, sceneAssetMapping.assetNames);
				}
			}
			return _sceneAssetMappings;
		}
	}
}
