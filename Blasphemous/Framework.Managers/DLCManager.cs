using System.Collections.Generic;
using System.IO;
using Framework.DLCs;
using Framework.FrameworkCore;
using UnityEngine;

namespace Framework.Managers;

public class DLCManager : GameSystem
{
	public const string DLC_SUBFOLDER = "DLC";

	private IDLCHelper helper;

	private Dictionary<string, DLC> allDLCs = new Dictionary<string, DLC>();

	private static List<string> cacheList = new List<string>();

	private static List<string> cachedDLCs = new List<string>();

	public static List<string> GetDLCBaseDirectories()
	{
		if (cacheList.Count == 0)
		{
			cacheList.Add(Path.Combine(Application.streamingAssetsPath, "DLC"));
			cacheList.Add(Path.Combine(Application.dataPath, "DLC"));
			cacheList.Add(PersistentManager.GetPathAppSettings("DLC"));
		}
		return cacheList;
	}

	public override void Initialize()
	{
		helper = new TestDLCHelper();
		DLC[] array = Resources.LoadAll<DLC>("DLC/");
		allDLCs.Clear();
		DLC[] array2 = array;
		foreach (DLC dLC in array2)
		{
			dLC.CheckDownloaded();
			allDLCs[dLC.id] = dLC;
		}
		foreach (string dLCBaseDirectory in GetDLCBaseDirectories())
		{
			Log.Debug("DLCManager", "Base DLC path: " + dLCBaseDirectory);
			if (!Directory.Exists(dLCBaseDirectory))
			{
				Directory.CreateDirectory(dLCBaseDirectory);
			}
		}
		Log.Debug("DLCManager", allDLCs.Count + " DLCs loaded succesfully.");
		DLC[] array3 = array;
		foreach (DLC dLC2 in array3)
		{
			Log.Debug(string.Format("DLC: {0} is {1}.", dLC2.name, (!dLC2.IsDownloaded) ? "not present" : "present"));
		}
	}

	public bool IsFirstDLCInstalled()
	{
		return true;
	}

	public bool IsSecondDLCInstalled()
	{
		return false;
	}

	public bool IsThirdDLCInstalled()
	{
		return true;
	}

	public bool IsDLCDownloaded(string Id, bool recheck = false)
	{
		bool result = false;
		if (allDLCs.ContainsKey(Id))
		{
			if (recheck)
			{
				allDLCs[Id].CheckDownloaded();
			}
			result = allDLCs[Id].IsDownloaded;
		}
		return result;
	}

	public static List<string> GetAllDLCsIDFromEditor()
	{
		if (cachedDLCs.Count == 0)
		{
			DLC[] array = Resources.LoadAll<DLC>("DLC/");
			DLC[] array2 = array;
			foreach (DLC dLC in array2)
			{
				cachedDLCs.Add(dLC.id);
			}
		}
		return cachedDLCs;
	}
}
