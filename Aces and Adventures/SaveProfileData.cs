using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ProtoContract]
[UIField]
public class SaveProfileData
{
	public const int MAX_PROFILE_COUNT = 50;

	private static SaveProfileData _Data;

	[ProtoMember(1)]
	[DefaultValue("Default")]
	private string _activeProfileName = "Default";

	[ProtoMember(2)]
	private List<SaveProfile> _saveProfiles;

	private SaveProfile _activeProfile;

	public static SaveProfileData Data => _Data ?? (_Data = IOUtil.LoadFromBytesBackupSafe<SaveProfileData>(IOUtil.SaveProfileDataFilepath));

	public static string ActiveProfileName => Data.activeProfileName;

	public static string ContentFolderSuffix => _GetContentFolderSuffix(ActiveProfileName);

	public string activeProfileName
	{
		get
		{
			return _activeProfileName;
		}
		private set
		{
			if (SetPropertyUtility.SetObject(ref _activeProfileName, value))
			{
				_activeProfile = null;
			}
		}
	}

	[UIField(fixedSize = true, dynamicInitMethod = "_InitSaveProfiles", collapse = UICollapseType.Open, filter = 50)]
	[UIFieldCollectionItem]
	public List<SaveProfile> saveProfiles
	{
		get
		{
			if (_saveProfiles.IsNullOrEmpty())
			{
				List<SaveProfile> obj = new List<SaveProfile>
				{
					new SaveProfile()
				};
				List<SaveProfile> result = obj;
				_saveProfiles = obj;
				return result;
			}
			return _saveProfiles;
		}
		private set
		{
			_saveProfiles = value;
		}
	}

	public SaveProfile activeProfile
	{
		get
		{
			SaveProfile saveProfile = _activeProfile;
			if (saveProfile == null)
			{
				SaveProfile obj = this[activeProfileName] ?? this["Default"] ?? new SaveProfile();
				SaveProfile saveProfile2 = obj;
				_activeProfile = obj;
				saveProfile = saveProfile2;
			}
			return saveProfile;
		}
	}

	public SaveProfile this[string profileName] => saveProfiles.FirstOrDefault((SaveProfile p) => StringComparer.OrdinalIgnoreCase.Equals(p.name, profileName));

	private bool _hideCreateNewProfile => saveProfiles.Count >= 50;

	public static void Save()
	{
		IOUtil.WriteToFileBackup(Data, IOUtil.SaveProfileDataFilepath);
	}

	public static void DeleteProfile(string profileName)
	{
		if (StringComparer.OrdinalIgnoreCase.Equals(profileName, ActiveProfileName) || StringComparer.OrdinalIgnoreCase.Equals(profileName, "Default"))
		{
			return;
		}
		if (Data.saveProfiles.Remove(Data[profileName]))
		{
			if (Directory.Exists(ProfileManager.GetProfileFilepath(profileName)))
			{
				Directory.Delete(ProfileManager.GetProfileFilepath(profileName), recursive: true);
			}
			if (Directory.Exists(GetContentFolderPath(profileName)))
			{
				Directory.Delete(GetContentFolderPath(profileName), recursive: true);
			}
		}
		Save();
	}

	public static void SwitchToProfile(string profileName)
	{
		Data.activeProfile.lastPlayed = DateTime.UtcNow.ToFileTimeUtc();
		Data.activeProfileName = profileName;
		Save();
		LoadScreenView.Load(SceneRef.Launch);
	}

	private static string _GetContentFolderSuffix(string profileName)
	{
		if (!StringComparer.OrdinalIgnoreCase.Equals(profileName, "Default"))
		{
			return " " + profileName;
		}
		return "";
	}

	public static string GetContentFolderPath(string profileName)
	{
		return IOUtil.Combine(IOUtil.UserSaveDir, "_C" + _GetContentFolderSuffix(profileName));
	}

	private void _DoCreateNewProfile()
	{
		Transform parent = UIGeneratorType.GetActiveUIForType<ProfileOptions>().transform;
		GameObject inputField = UIUtil.CreateInputField("New Profile Name", null, "", 20, InputField.ContentType.Alphanumeric, null, null, readOnly: false, "Enter Profile Name...");
		inputField.GetComponentInChildren<LayoutElement>().preferredWidth = 800f;
		UIUtil.CreatePopup("Create New Profile", inputField, null, parent: parent, buttons: new string[2] { "Cancel", "Create New Profile" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (!(s == "Cancel"))
			{
				string name = inputField.GetComponentInChildren<TMP_InputField>().text;
				if (!name.HasVisibleCharacter())
				{
					GameObject mainContent2 = UIUtil.CreateMessageBox("Profile name must contain at least 1 alphanumeric character.", TextAlignmentOptions.Left, 32, 600, 300, 24f);
					Transform parent3 = parent;
					UIUtil.CreatePopup("Invalid Name", mainContent2, null, null, null, null, null, null, true, true, null, null, null, parent3, null, null);
				}
				else if (this[name] != null)
				{
					GameObject mainContent3 = UIUtil.CreateMessageBox("Profile named \"" + name + "\" already exists. Please choose another name.", TextAlignmentOptions.Left, 32, 600, 300, 24f);
					Transform parent3 = parent;
					UIUtil.CreatePopup("Profile With Name Already Exists", mainContent3, null, null, null, null, null, null, true, true, null, null, null, parent3, null, null);
				}
				else
				{
					saveProfiles.Add(new SaveProfile(name));
					new Profile(name).SetOptions(ProfileManager.options).SaveAll();
					Save();
					string switchProfile = "Switch To Newly Created Profile";
					UIUtil.CreatePopup(switchProfile, UIUtil.CreateMessageBox("Would you like to <b>" + switchProfile + "</b>? This will effectively Save & Exit the current profile.", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: parent, buttons: new string[2] { "Cancel", switchProfile }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string b)
					{
						if (b == switchProfile)
						{
							SwitchToProfile(name);
						}
						else
						{
							UIGeneratorType.ValidateAllOfType<ProfileOptions>();
						}
					});
				}
			}
		});
		inputField.GetComponentInChildren<TMP_InputField>().FocusAndMoveToEnd();
	}

	private void _InitSaveProfiles(UIFieldAttribute uiField)
	{
		uiField.maxCount = saveProfiles.Count;
	}

	[UIField(dynamicInitMethod = "_InitCreateNewProfile")]
	[UIHideIf("_hideCreateNewProfile")]
	private void _CreateNewProfile()
	{
		activeProfile.DoPasswordLogic(activeProfile.logOutPassword, _DoCreateNewProfile);
	}

	private void OnValidateUI()
	{
		saveProfiles.StableSort((SaveProfile a, SaveProfile b) => b.lastPlayed.CompareTo(a.lastPlayed));
	}
}
