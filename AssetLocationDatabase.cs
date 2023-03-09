using System.Collections.Generic;
using UnityEngine;

public class AssetLocationDatabase : ScriptableObject
{
	public enum AssetType
	{
		Base,
		DLC
	}

	[SerializeField]
	private string[] dlcAssetNames;

	private HashSet<string> _dlcAssets;

	public HashSet<string> dlcAssets
	{
		get
		{
			if (_dlcAssets == null)
			{
				_dlcAssets = new HashSet<string>(dlcAssetNames);
			}
			return _dlcAssets;
		}
	}

	public void SetDLCAssets(string[] dlcAssets)
	{
		dlcAssetNames = dlcAssets;
	}
}
