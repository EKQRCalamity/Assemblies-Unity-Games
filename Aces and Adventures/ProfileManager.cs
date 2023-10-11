using UnityEngine;

public static class ProfileManager
{
	public const string PROFILE_FILENAME = "Profile.bytes";

	private static Profile _Profile;

	public static Profile Profile
	{
		get
		{
			return _Profile ?? (_Profile = _LoadProfile("Default"));
		}
		private set
		{
			_Profile = value ?? new Profile("Default");
		}
	}

	public static string path => Profile.path;

	public static ProfileOptions.ControlOptions controls => Profile.options.controls;

	public static ProfileProgress progress => Profile.progress;

	public static ProfilePrefs prefs => Profile.preferences;

	public static ProfileOptions options => Profile.options;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad()
	{
		ProtoUtil.InitializeSurrogateTrigger = true;
		Profile.options.ApplyChanges();
	}

	static ProfileManager()
	{
		Job.OnApplicationQuit(delegate
		{
			Profile.SaveAll();
		});
	}

	private static Profile _LoadProfile(string profileName)
	{
		return IOUtil.LoadFromBytesBackup<Profile>(IOUtil.Combine(GetProfileFilepath(profileName), "Profile.bytes")) ?? new Profile(profileName).SaveAll();
	}

	public static string GetProfileFilepath(string profileName)
	{
		return IOUtil.Combine(IOUtil.UserProfilesPath, profileName);
	}

	public static void SetProfileByName(string profileName)
	{
		Profile = _LoadProfile(profileName);
	}

	public static void SaveOptionsMenu()
	{
		Profile.SaveOptions(applyChanges: true);
		SaveProfileData.Save();
	}

	public static void Unload()
	{
	}
}
