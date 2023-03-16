public static class PlatformHelper
{
	public static bool IsConsole => false;

	public static bool PreloadSettingsData => false;

	public static bool ShowAchievements => false;

	public static bool ShowDLCMenuItem => true;

	public static bool GarbageCollectOnPause => false;

	public static bool ForceAdditionalHeapMemory => false;

	public static bool ManuallyRefreshDLCAvailability => true;

	public static bool CanSwitchUserFromPause => false;
}
