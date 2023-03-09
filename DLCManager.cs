using System;
using System.Collections;
using Steamworks;
using UnityEngine;
using UnityEngine.U2D;

public class DLCManager
{
	public class AssetBundleLoadWaitInstruction : IEnumerator
	{
		protected bool complete;

		public object Current => null;

		public AssetBundle assetBundle { get; protected set; }

		public bool MoveNext()
		{
			return !complete;
		}

		public void Reset()
		{
		}
	}

	private static bool availabilityPromptTriggered;

	private static readonly AppId_t DLCAppID = new AppId_t(1117850u);

	private static bool dlcAvailable;

	public static bool persistentAssetsLoaded { get; set; }

	public static bool showAvailabilityPrompt { get; private set; }

	public static void RefreshDLC()
	{
		refreshDLC();
	}

	public static void CheckInstallationStatusChanged()
	{
		checkInstallationStatusChanged();
	}

	public static bool DLCEnabled()
	{
		bool enabled = false;
		dlcEnabled(ref enabled);
		return enabled;
	}

	public static string AssetBundlePath()
	{
		string path = null;
		assetBundlePath(ref path);
		return path;
	}

	public static bool UsesAlternateBundleLoadingMechanism()
	{
		bool usesAlternate = false;
		usesAlternateBundleLoadingMechanism(ref usesAlternate);
		return usesAlternate;
	}

	public static AssetBundleLoadWaitInstruction LoadAssetBundle(string path)
	{
		AssetBundleLoadWaitInstruction waitInstruction = null;
		loadAssetBundle(path, ref waitInstruction);
		return waitInstruction;
	}

	public static bool UnloadBundlesImmediately()
	{
		bool unloadImmediately = false;
		unloadBundlesImmediately(ref unloadImmediately);
		return unloadImmediately;
	}

	public static bool CanRedirectToStore()
	{
		bool canRedirect = false;
		canRedirectToStore(ref canRedirect);
		return canRedirect;
	}

	public static void LaunchStore()
	{
		launchStore();
	}

	public static Coroutine[] LoadPersistentAssets()
	{
		if (persistentAssetsLoaded || !DLCEnabled())
		{
			return null;
		}
		persistentAssetsLoaded = true;
		return new Coroutine[1] { AssetLoader<SpriteAtlas>.LoadPersistentAssetsDLC() };
	}

	public static void ResetAvailabilityPrompt()
	{
		availabilityPromptTriggered = true;
		showAvailabilityPrompt = false;
	}

	private static bool steamDLCStatus()
	{
		ulong punBytesDownloaded;
		ulong punBytesTotal;
		if (SteamApps.BIsDlcInstalled(DLCAppID))
		{
			return !SteamApps.GetDlcDownloadProgress(DLCAppID, out punBytesDownloaded, out punBytesTotal);
		}
		return false;
	}

	private static void refreshDLC()
	{
		if (!SteamManager.Initialized)
		{
			dlcAvailable = false;
		}
		else if (!dlcAvailable)
		{
			dlcAvailable = steamDLCStatus();
		}
	}

	private static void checkInstallationStatusChanged()
	{
		if (SteamManager.Initialized && !DLCEnabled() && !availabilityPromptTriggered && steamDLCStatus())
		{
			showAvailabilityPrompt = true;
		}
	}

	private static void dlcEnabled(ref bool enabled)
	{
		enabled = dlcAvailable;
	}

	private static void assetBundlePath(ref string path)
	{
		path = Application.streamingAssetsPath;
	}

	private static void usesAlternateBundleLoadingMechanism(ref bool usesAlternate)
	{
		usesAlternate = false;
	}

	private static void unloadBundlesImmediately(ref bool unloadImmediately)
	{
		unloadImmediately = false;
	}

	private static void loadAssetBundle(string path, ref AssetBundleLoadWaitInstruction waitInstruction)
	{
		throw new NotImplementedException();
	}

	private static void canRedirectToStore(ref bool canRedirect)
	{
		canRedirect = true;
	}

	private static void launchStore()
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.ActivateGameOverlayToStore(DLCAppID, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
		}
	}
}
