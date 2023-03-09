using System.Collections;
using UnityEngine;

public class PlatformHandlingTitleScreenOverride
{
	public static readonly string XboxOneForceOriginalTitleScreenKey = "XboxOne_ForceOriginalTitleScreen";

	public static readonly string UWPForceOriginalTitleScreenKey = "UWP_ForceOriginalTitleScreen";

	private StartScreen.InitialLoadData startScreenLoadData;

	public PlatformHandlingTitleScreenOverride(StartScreen.InitialLoadData startScreenLoadData)
	{
		this.startScreenLoadData = startScreenLoadData;
	}

	public IEnumerator GetTitleScreenOverrideStatus_cr(MonoBehaviour parent)
	{
		yield return null;
		yield return null;
		startScreenLoadData.forceOriginalTitleScreen = SettingsData.Data.forceOriginalTitleScreen;
	}
}
