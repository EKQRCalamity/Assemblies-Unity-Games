using ProtoBuf;

[ProtoContract]
public class Profile
{
	private const string PREFERENCES = "Preferences.bytes";

	private const string OPTIONS = "Options.dat";

	private const string PROGRESS = "Progress.bytes";

	private const bool DEBUG = true;

	[ProtoMember(1)]
	private string _name;

	private ProfilePrefs _preferences;

	private ProfileOptions _options;

	private ProfileProgress _progress;

	public string name
	{
		get
		{
			if (!_name.HasVisibleCharacter())
			{
				return _name = "Default";
			}
			return _name;
		}
	}

	public ProfilePrefs preferences => _preferences ?? (_preferences = new ProfilePrefs());

	public ProfileOptions options => _options ?? (_options = new ProfileOptions());

	public ProfileProgress progress => _progress ?? (_progress = new ProfileProgress());

	public string path => ProfileManager.GetProfileFilepath(name);

	private string _profilePath => IOUtil.Combine(path, "Profile.bytes");

	private string _preferencesPath => IOUtil.Combine(path, "Preferences.bytes");

	private string _optionsPath => IOUtil.Combine(path, "Options.dat");

	private string _progressPath => IOUtil.Combine(path, "Progress.bytes");

	private Profile()
	{
	}

	public Profile(string name)
	{
		_name = name;
	}

	public Profile SaveAll()
	{
		IOUtil.WriteToFileBackup(this, _profilePath);
		return this;
	}

	public void SavePreferences()
	{
		IOUtil.WriteToFileBackup(preferences, _preferencesPath);
	}

	public void SaveOptions(bool applyChanges)
	{
		IOUtil.WriteToFileBackup(options, _optionsPath);
		if (applyChanges)
		{
			options.ApplyChanges();
		}
	}

	public void SaveProgress()
	{
		IOUtil.WriteToFileBackup(progress, _progressPath);
	}

	public Profile SetOptions(ProfileOptions newOptions)
	{
		_options = newOptions;
		return this;
	}

	[ProtoAfterSerialization]
	private void _ProtoAfterSerialization()
	{
		SavePreferences();
		SaveOptions(applyChanges: false);
		SaveProgress();
	}

	[ProtoAfterDeserialization]
	private void _ProtoAfterDeserialization()
	{
		_preferences = IOUtil.LoadFromBytesBackupSafe<ProfilePrefs>(_preferencesPath);
		_options = IOUtil.LoadFromBytesBackupSafe<ProfileOptions>(_optionsPath);
		_progress = IOUtil.LoadFromBytesBackupSafe<ProfileProgress>(_progressPath);
	}
}
