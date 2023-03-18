using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.FrameworkCore;
using UnityEngine;

namespace Framework.Managers;

public class UpdateManager : GameSystem
{
	public const string EDITOR_BUNDLE_PATH = "/../obj/Updates/StandaloneWindows/";

	public const string GAME_BUNDLE_PATH = "/Updates/";

	private List<AssetBundle> bundles = new List<AssetBundle>();

	public override void Initialize()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		LoadBundles();
	}

	private void LoadBundles()
	{
		try
		{
			string path = ((!Application.isEditor) ? "/Updates/" : (Application.dataPath + "/../obj/Updates/StandaloneWindows/"));
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			FileInfo[] files = directoryInfo.GetFiles();
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				if (fileInfo.Extension == string.Empty)
				{
					AssetBundle item = AssetBundle.LoadFromFile(fileInfo.DirectoryName + "/" + fileInfo.Name);
					bundles.Add(item);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
		}
	}

	private void LoadLevelContent(string scene)
	{
		foreach (AssetBundle bundle in bundles)
		{
			string[] allAssetNames = bundle.GetAllAssetNames();
			foreach (string text in allAssetNames)
			{
				string text2 = text.Split('/').Last().Split('.')
					.First();
				if (text2.Contains(scene.ToLower()))
				{
					GameObject original = bundle.LoadAsset(text2) as GameObject;
					UnityEngine.Object.Instantiate(original);
				}
			}
		}
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		LoadLevelContent(newLevel.LevelName);
	}

	public override void Dispose()
	{
		base.Dispose();
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}
}
