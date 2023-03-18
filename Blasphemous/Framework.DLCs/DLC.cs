using System;
using System.IO;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.DLCs;

public class DLC : ScriptableObject
{
	[OnValueChanged("OnIdChanged", false)]
	[InfoBox("Common ID for all platforms", InfoMessageType.Info, null)]
	public string id = string.Empty;

	public string sortDescription = string.Empty;

	[InfoBox("ID of the DLC on various platforms", InfoMessageType.Info, null)]
	public uint steam_appid;

	public string gog_appid = string.Empty;

	public string epic_appid = string.Empty;

	[InfoBox("Assetbundle to download, load & check", InfoMessageType.Info, null)]
	public string assetBundle;

	[NonSerialized]
	public AssetBundle loadedBundle;

	public bool IsDownloaded { get; private set; }

	public string FullPath { get; private set; }

	public void OnIdChanged(string value)
	{
		id = value.Replace(' ', '_').ToUpper();
	}

	public string GetFileName()
	{
		return assetBundle;
	}

	public void CheckDownloaded()
	{
		string fileName = GetFileName();
		foreach (string dLCBaseDirectory in DLCManager.GetDLCBaseDirectories())
		{
			FullPath = Path.Combine(dLCBaseDirectory, fileName);
			IsDownloaded = File.Exists(FullPath);
			if (IsDownloaded)
			{
				break;
			}
		}
	}
}
