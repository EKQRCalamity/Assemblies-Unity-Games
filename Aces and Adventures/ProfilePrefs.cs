using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[ProtoContract]
public class ProfilePrefs
{
	[ProtoContract]
	public class FileBrowserData
	{
		private const int MAX_RECENT = 10;

		[ProtoMember(1, OverwriteList = true)]
		private List<string> _recentDirectories;

		[ProtoMember(2)]
		[DefaultValue(ContentRefSortType.Alphabetical)]
		private ContentRefSortType _resultSortType = ContentRefSortType.Alphabetical;

		private List<string> recentDirectories => _recentDirectories ?? (_recentDirectories = new List<string>());

		public ContentRefSortType resultSortType => _resultSortType;

		public PoolKeepItemListHandle<string> GetRecentDirectories()
		{
			for (int num = recentDirectories.Count - 1; num >= 0; num--)
			{
				if (!Directory.Exists(recentDirectories[num]))
				{
					recentDirectories.RemoveAt(num);
				}
			}
			return Pools.UseKeepItemList(recentDirectories);
		}

		public bool AddRecentDirectory(string directory)
		{
			if (recentDirectories.Count > 0 && StringComparer.OrdinalIgnoreCase.Equals(recentDirectories[0], directory))
			{
				return false;
			}
			recentDirectories.Remove(directory);
			recentDirectories.Insert(0, directory);
			for (int num = recentDirectories.Count - 1; num >= 10; num--)
			{
				recentDirectories.RemoveAt(num);
			}
			return true;
		}

		public void SetResultSortType(ContentRefSortType sortType, bool saveOnChange = true)
		{
			if (SetPropertyUtility.SetStruct(ref _resultSortType, sortType) && saveOnChange)
			{
				ProfileManager.Profile.SavePreferences();
			}
		}
	}

	[ProtoContract]
	public class SteamData
	{
		[ProtoMember(1)]
		private Steam.Ugc.Query.QueryType _queryType;

		[ProtoMember(2, OverwriteList = true)]
		private Dictionary<ulong, ulong> _createdAuthorPublishedFileIds;

		[ProtoMember(3, OverwriteList = true)]
		private HashSet<ulong> _likedAuthors;

		[ProtoMember(4, OverwriteList = true)]
		private HashSet<ulong> _dislikedAuthors;

		[ProtoMember(5)]
		private ulong _searchGroup;

		[ProtoMember(6, OverwriteList = true)]
		private HashSet<ulong> _usersThatHaveAcceptedEULA;

		[ProtoMember(7)]
		[DefaultValue(ContentInstallStatusFlags.Installed | ContentInstallStatusFlags.Update | ContentInstallStatusFlags.New)]
		private ContentInstallStatusFlags _statusFlags = ContentInstallStatusFlags.Installed | ContentInstallStatusFlags.Update | ContentInstallStatusFlags.New;

		public Steam.Ugc.Query.QueryType queryType => _queryType;

		public ContentInstallStatusFlags statusFlags => _statusFlags;

		private Dictionary<ulong, ulong> createdAuthorPublishedFileIds => _createdAuthorPublishedFileIds ?? (_createdAuthorPublishedFileIds = new Dictionary<ulong, ulong>());

		public HashSet<ulong> likedAuthors => _likedAuthors ?? (_likedAuthors = new HashSet<ulong>());

		public HashSet<ulong> dislikedAuthors => _dislikedAuthors ?? (_dislikedAuthors = new HashSet<ulong>());

		public Steam.Friends.Group group => new Steam.Friends.Group((CSteamID)_searchGroup);

		private HashSet<ulong> usersThatHaveAcceptedEULA => _usersThatHaveAcceptedEULA ?? (_usersThatHaveAcceptedEULA = new HashSet<ulong>());

		private bool _hasAcceptedEULA => usersThatHaveAcceptedEULA.Contains(Steam.SteamId.m_SteamID);

		public void SetResultSortType(Steam.Ugc.Query.QueryType queryType, bool saveOnChange = true)
		{
			if (SetPropertyUtility.SetStruct(ref _queryType, queryType) && saveOnChange)
			{
				ProfileManager.Profile.SavePreferences();
			}
		}

		public void SetStatusFlags(ContentInstallStatusFlags statusFlags, bool saveOnChange = true)
		{
			if (SetPropertyUtility.SetStruct(ref _statusFlags, statusFlags) && saveOnChange)
			{
				ProfileManager.Profile.SavePreferences();
			}
		}

		public PublishedFileId_t? GetAuthorPublishedFileId()
		{
			if (!createdAuthorPublishedFileIds.ContainsKey(Steam.SteamId.m_SteamID))
			{
				return null;
			}
			return (PublishedFileId_t)createdAuthorPublishedFileIds[Steam.SteamId.m_SteamID];
		}

		public void SetAuthorPublishedFileId(PublishedFileId_t publishedFileId)
		{
			if (createdAuthorPublishedFileIds.SetDifferent(Steam.SteamId.m_SteamID, publishedFileId.m_PublishedFileId))
			{
				ProfileManager.Profile.SavePreferences();
			}
		}

		public void SetAuthorLike(CSteamID authorId, bool like)
		{
			if ((like ? likedAuthors : dislikedAuthors).Add(authorId.m_SteamID))
			{
				(like ? dislikedAuthors : likedAuthors).Remove(authorId.m_SteamID);
				ProfileManager.Profile.SavePreferences();
			}
		}

		public void SetSearchGroup(Steam.Friends.Group group)
		{
			if (SetPropertyUtility.SetStruct(ref _searchGroup, group ? group.id.m_SteamID : 0))
			{
				ProfileManager.Profile.SavePreferences();
			}
		}

		private void _AcceptEULA()
		{
			if (usersThatHaveAcceptedEULA.Add(Steam.SteamId.m_SteamID))
			{
				ProfileManager.Profile.SavePreferences();
			}
		}

		public async Task<bool> ShowEULAIfNeeded(Transform parent)
		{
			if (!ProfileManager.options.game.ugc.enabled)
			{
				UIUtil.CreatePopup("User Generated Content Is Disabled", UIUtil.CreateMessageBox("<b>User Generated Content</b> is currently disabled due to option settings. You can find the option to enable <b>User Generated Content</b> under the <b>Game</b> tab of the <b>Options Menu</b>.", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, null, null, null, null, null, true, true, null, null, null, parent, null, null);
				return false;
			}
			if (_hasAcceptedEULA)
			{
				return true;
			}
			await new AwaitCoroutine<object>(Job.WaitTillDestroyed(UIUtil.CreateLegalAgreementPopup("Terms of Use", Resources.Load<TextAsset>("UI/Content/EULA").text, _AcceptEULA, parent, 32)));
			return _hasAcceptedEULA;
		}
	}

	[ProtoMember(1)]
	private FileBrowserData _browser;

	[ProtoMember(2)]
	private SteamData _steam;

	[ProtoMember(3)]
	private DataRef<GameData> _selectedGame;

	[ProtoMember(4)]
	private DataRef<CharacterData> _selectedCharacter;

	[ProtoMember(5)]
	private string _localeCode;

	public FileBrowserData browser => _browser ?? (_browser = new FileBrowserData());

	public SteamData steam => _steam ?? (_steam = new SteamData());

	public DataRef<GameData> selectedGame
	{
		get
		{
			if (!_selectedGame)
			{
				return ContentRef.Defaults.data.startingGame;
			}
			return _selectedGame;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _selectedGame, value))
			{
				ProfilePrefs.OnSelectedGameChange?.Invoke(value);
			}
		}
	}

	public DataRef<CharacterData> selectedCharacter
	{
		get
		{
			if (!_selectedCharacter)
			{
				return ContentRef.Defaults.data.startingCharacter;
			}
			return _selectedCharacter;
		}
		set
		{
			_selectedCharacter = value;
		}
	}

	public Locale localeOverride
	{
		get
		{
			if (!_localeCode.HasVisibleCharacter())
			{
				return null;
			}
			return LocalizationSettings.AvailableLocales.GetLocale(_localeCode);
		}
		set
		{
			_localeCode = value.Identifier.Code;
		}
	}

	public Locale locale => localeOverride ?? LocalizationSettings.SelectedLocale;

	private bool _selectedGameSpecified => _selectedGame.ShouldSerialize();

	private bool _selectedCharacterSpecified => _selectedCharacter.ShouldSerialize();

	public static event Action<DataRef<GameData>> OnSelectedGameChange;
}
