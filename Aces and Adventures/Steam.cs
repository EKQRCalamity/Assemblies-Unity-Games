using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using Steamworks;
using UnityEngine;

public static class Steam
{
	public struct CallResultData<T>
	{
		public T result;

		public bool failure;

		public CallResultData(T result, bool failure)
		{
			this.result = result;
			this.failure = failure;
		}

		public static implicit operator T(CallResultData<T> resultData)
		{
			return resultData.result;
		}
	}

	public class CallbackEvent<T> where T : struct
	{
		private Callback<T> _callback;

		private event Action<T> _onCallBack;

		public CallbackEvent()
		{
			_callback = Callback<T>.Create(delegate(T result)
			{
				if (this._onCallBack != null)
				{
					this._onCallBack(result);
				}
			});
		}

		public async Task<T> WaitAsync(Func<T, bool> validEvent = null, Action process = null)
		{
			AwaitValueSet<T> output = new AwaitValueSet<T>();
			if (process != null)
			{
				new AwaitCondition(delegate
				{
					if (output.GetAwaiter().IsCompleted)
					{
						return true;
					}
					process();
					return false;
				});
			}
			Action<T> onEvent = null;
			onEvent = delegate(T result)
			{
				if (validEvent == null || validEvent(result))
				{
					output.SetValue(result);
					_onCallBack -= onEvent;
				}
			};
			_onCallBack += onEvent;
			return await output;
		}
	}

	public static class Ugc
	{
		public struct InstallInfo : IEquatable<InstallInfo>
		{
			public PublishedFileId_t publishedFileId;

			public string filepath;

			public ulong sizeInBytes;

			public uint lastUpdated;

			public bool isInstalled;

			public static implicit operator bool(InstallInfo installInfo)
			{
				if (installInfo.isInstalled && installInfo.publishedFileId.IsValid() && installInfo.sizeInBytes != 0)
				{
					return Directory.Exists(installInfo.filepath);
				}
				return false;
			}

			public static implicit operator InstallInfo(PublishedFileId_t publishedFileId)
			{
				InstallInfo result = default(InstallInfo);
				result.publishedFileId = publishedFileId;
				return result;
			}

			public void DeleteInstallFolder()
			{
				if ((bool)this)
				{
					Directory.Delete(filepath, recursive: true);
				}
			}

			public bool Equals(InstallInfo other)
			{
				return publishedFileId == other.publishedFileId;
			}

			public override bool Equals(object obj)
			{
				if (obj is InstallInfo)
				{
					return Equals((InstallInfo)obj);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return publishedFileId.m_PublishedFileId.GetHashCode();
			}
		}

		public struct UploadInfo
		{
			public UGCUpdateHandle_t updateHandle;

			public EItemUpdateStatus status;

			public ulong bytesProcessed;

			public ulong bytesTotal;

			public float progress => (float)bytesProcessed / (float)Math.Max(1uL, bytesTotal);

			public UploadInfo(UGCUpdateHandle_t updateHandle)
			{
				this.updateHandle = updateHandle;
				status = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
				bytesProcessed = 0uL;
				bytesTotal = 0uL;
			}

			public UploadInfo Update()
			{
				status = SteamUGC.GetItemUpdateProgress(updateHandle, out bytesProcessed, out bytesTotal);
				return this;
			}

			public override string ToString()
			{
				return $"Status: {status}\n{bytesProcessed} / {bytesTotal} bytes uploaded";
			}
		}

		public struct DownloadInfo
		{
			public PublishedFileId_t publishedFileId;

			public bool isDownloading;

			public ulong bytesDownloaded;

			public ulong bytesTotal;

			public float progress => (float)bytesDownloaded / (float)Math.Max(1uL, bytesTotal);

			public DownloadInfo(PublishedFileId_t publishedFileId)
			{
				this.publishedFileId = publishedFileId;
				isDownloading = false;
				bytesDownloaded = 0uL;
				bytesTotal = 0uL;
			}

			public DownloadInfo Update()
			{
				isDownloading = SteamUGC.GetItemDownloadInfo(publishedFileId, out bytesDownloaded, out bytesTotal);
				return this;
			}

			public override string ToString()
			{
				return $"Is Downloading: {isDownloading}\n{bytesDownloaded} / {bytesTotal} bytes downloaded";
			}
		}

		public enum UserItemVoteResult : byte
		{
			Failure,
			None,
			DownVote,
			UpVote
		}

		public enum ItemType : byte
		{
			Standard,
			Author
		}

		public interface IQuery : IDisposable
		{
			IQueryType type { get; }

			int id { get; }

			Task PageAllAsync(Action<IEnumerable<Query.Result>> onPageResult);
		}

		public enum IQueryType
		{
			All,
			Specific,
			User,
			Users
		}

		public struct QuerySettings
		{
			private PoolKeepItemListHandle<KeyValuePair<string, string>> _keyValueTags;

			private PoolKeepItemListHandle<string> _tags;

			private PoolKeepItemListHandle<string> _excludedTags;

			private bool _matchAnyTag;

			private bool _returnMetaData;

			private bool _returnChildren;

			private bool _returnKeyValueTags;

			private bool _returnAdditionalPreviews;

			public uint startIndex;

			public bool returnMetaData => _returnMetaData;

			public bool returnChildren => _returnChildren;

			public bool returnKeyValueTags => _returnKeyValueTags;

			public bool returnAdditionalPreviews => _returnAdditionalPreviews;

			public QuerySettings(IEnumerable<KeyValuePair<string, string>> keyValueTags, IEnumerable<string> tags, bool matchAnyTag, bool returnMetaData, bool returnChildren, bool returnKeyValueTags, bool returnAdditionalPreviews)
			{
				_keyValueTags = ((keyValueTags != null) ? Pools.UseKeepItemList(keyValueTags.Distinct()) : null);
				if (tags != null)
				{
					_tags = Pools.UseKeepItemList<string>();
					_excludedTags = Pools.UseKeepItemList<string>();
					foreach (string item in tags.Where((string s) => !s.IsNullOrEmpty()).Distinct())
					{
						if (item[0] != '!')
						{
							_tags.Add(item);
						}
						else if (item.Length > 1)
						{
							_excludedTags.Add(item.Substring(1));
						}
					}
				}
				else
				{
					_tags = null;
					_excludedTags = null;
				}
				_matchAnyTag = matchAnyTag;
				_returnMetaData = returnMetaData;
				_returnChildren = returnChildren;
				_returnKeyValueTags = returnKeyValueTags;
				_returnAdditionalPreviews = returnAdditionalPreviews;
				startIndex = 0u;
			}

			public void SetSettings(UGCQueryHandle_t queryHandle)
			{
				SteamUGC.SetMatchAnyTag(queryHandle, _matchAnyTag);
				if ((bool)_keyValueTags)
				{
					foreach (KeyValuePair<string, string> item in _keyValueTags.value)
					{
						SteamUGC.AddRequiredKeyValueTag(queryHandle, item.Key, item.Value);
					}
				}
				if ((bool)_tags)
				{
					foreach (string item2 in _tags.value)
					{
						SteamUGC.AddRequiredTag(queryHandle, item2);
					}
				}
				if ((bool)_excludedTags)
				{
					foreach (string item3 in _excludedTags.value)
					{
						SteamUGC.AddExcludedTag(queryHandle, item3);
					}
				}
				SteamUGC.SetReturnMetadata(queryHandle, _returnMetaData);
				SteamUGC.SetReturnChildren(queryHandle, _returnChildren);
				SteamUGC.SetReturnKeyValueTags(queryHandle, _returnKeyValueTags);
				SteamUGC.SetReturnAdditionalPreviews(queryHandle, _returnAdditionalPreviews);
			}

			public PoolKeepItemListHandle<Query.Result> FilterResults(PoolKeepItemListHandle<Query.Result> results)
			{
				for (int num = results.Count - 1; num >= 0; num--)
				{
					Query.Result result = results[num];
					if (_returnKeyValueTags && (bool)_keyValueTags)
					{
						foreach (KeyValuePair<string, string> item in _keyValueTags.value)
						{
							if (result.keyValueTags.ContainsKey(item.Key) && result.keyValueTags[item.Key].Equals(item.Value, StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
							goto IL_015a;
						}
					}
					if ((bool)_excludedTags)
					{
						foreach (string item2 in _excludedTags.value)
						{
							if (!result.tags.Contains(item2, StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
							goto IL_015a;
						}
					}
					if (!_tags)
					{
						continue;
					}
					foreach (string item3 in _tags.value)
					{
						if (result.tags.Contains(item3, StringComparison.OrdinalIgnoreCase))
						{
							if (_matchAnyTag)
							{
								break;
							}
							continue;
						}
						if (_matchAnyTag)
						{
							continue;
						}
						goto IL_015a;
					}
					continue;
					IL_015a:
					results.RemoveAt(num);
				}
				return results;
			}

			public void Repool()
			{
				Pools.Repool(ref _keyValueTags);
				Pools.Repool(ref _tags);
				Pools.Repool(ref _excludedTags);
			}
		}

		public class Query : IQuery, IDisposable
		{
			public struct Result : IEquatable<Result>, IComparable<Result>
			{
				public struct AdditionalPreview
				{
					public readonly string name;

					public readonly string url;

					public AdditionalPreview(UGCQueryHandle_t queryHandle, uint index, uint previewIndex)
					{
						if (!SteamUGC.GetQueryUGCAdditionalPreview(queryHandle, index, previewIndex, out url, 2048u, out name, 128u, out var _) && DEBUG.LogWarning())
						{
							Log.Warning("Failed to Retrieve additional preview.", appendToUserLog: false);
						}
						name = Path.GetFileNameWithoutExtension(name);
					}

					public override string ToString()
					{
						return $"[{name}: {url}]";
					}
				}

				public SteamUGCDetails_t details;

				public byte[] metaData;

				public PublishedFileId_t[] children;

				public string previewImageUrl;

				public Dictionary<string, string> keyValueTags;

				public AdditionalPreview[] additionalPreviews;

				public uint index;

				public CSteamID ownerId => (CSteamID)details.m_ulSteamIDOwner;

				public string name => details.m_rgchTitle;

				public string description => details.m_rgchDescription;

				public string tags => details.m_rgchTags;

				public uint timeCreated => details.m_rtimeCreated;

				public uint timeUpdated => details.m_rtimeUpdated;

				public ContentVisibility visibility
				{
					get
					{
						if (keyValueTags == null || !keyValueTags.ContainsKey("V"))
						{
							return EnumUtil<ContentVisibility>.Min;
						}
						return (ContentVisibility)int.Parse(keyValueTags["V"]);
					}
				}

				public bool belongsToGroup
				{
					get
					{
						if (keyValueTags != null)
						{
							return keyValueTags.ContainsKey("G");
						}
						return false;
					}
				}

				public MatureContentFlags mature
				{
					get
					{
						if (keyValueTags == null || !keyValueTags.ContainsKey("M"))
						{
							return EnumUtil<MatureContentFlags>.NoFlags;
						}
						return (MatureContentFlags)int.Parse(keyValueTags["M"]);
					}
				}

				public int starRating => Mathf.FloorToInt((details.m_flScore - 0.01f) * 5f) + 1;

				public bool downVoted
				{
					get
					{
						if (dislikes >= 5)
						{
							return starRating <= 1;
						}
						return false;
					}
				}

				public bool pendingReview
				{
					get
					{
						if (totalVotes < 10 || likes < dislikes)
						{
							return DateTime.UtcNow.Subtract(MathUtil.FromUnixEpoch(details.m_rtimeUpdated)).TotalHours < 24.0;
						}
						return false;
					}
				}

				public uint likes => details.m_unVotesUp;

				public uint dislikes => details.m_unVotesDown;

				public uint totalVotes => likes + dislikes;

				public bool hasBeenVotedOn => totalVotes != 0;

				public int fileSize => details.m_nFileSize;

				public bool hasContent => fileSize > 0;

				public ulong id
				{
					get
					{
						return details.m_nPublishedFileId.m_PublishedFileId;
					}
					set
					{
						details.m_nPublishedFileId.m_PublishedFileId = value;
					}
				}

				public bool hasMetaData
				{
					get
					{
						if (!metaData.IsNullOrEmpty() && keyValueTags != null)
						{
							return keyValueTags.ContainsKey("Data");
						}
						return false;
					}
				}

				public ushort version => GetMetaData().Version();

				public bool versionIsValid => 12210 >= version;

				public ulong audioPreviewPublishedFileId
				{
					get
					{
						if (!hasMetaData)
						{
							return 0uL;
						}
						return GetMetaData().GetQuickDependencyPublishedFileId("IA");
					}
				}

				public async Task<string> GetCreatorNameAsync()
				{
					return await Friends.GetPersonaNameAsync((CSteamID)details.m_ulSteamIDOwner);
				}

				public IEnumerable<Friends.Group> Groups()
				{
					if (belongsToGroup)
					{
						string[] array = keyValueTags["G"].Split(SPLIT_TAGS);
						foreach (string s in array)
						{
							yield return new Friends.Group((CSteamID)ulong.Parse(s));
						}
					}
				}

				public bool Visible(IQueryType queryType)
				{
					if ((bool)this && hasContent && ProfileManager.options.game.ugc.Visible(mature, downVoted, pendingReview) && visibility.Visible((CSteamID)details.m_ulSteamIDOwner, queryType) && (!belongsToGroup || Groups().Any(Friends.CachedGroups.Contains)))
					{
						return versionIsValid;
					}
					return false;
				}

				public bool IsNotDownVoted()
				{
					if ((bool)this)
					{
						return !downVoted;
					}
					return false;
				}

				public string GetAdditionalPreviewURL(string previewName)
				{
					if (additionalPreviews.IsNullOrEmpty())
					{
						return null;
					}
					for (int i = 0; i < additionalPreviews.Length; i++)
					{
						if (additionalPreviews[i].url.HasVisibleCharacter() && StringComparer.OrdinalIgnoreCase.Equals(previewName, additionalPreviews[i].name))
						{
							return additionalPreviews[i].url;
						}
					}
					return null;
				}

				public string GetTrailerVideoId()
				{
					return GetAdditionalPreviewURL("");
				}

				public ContentRef.MetaData GetMetaData()
				{
					if (!hasMetaData)
					{
						return null;
					}
					return ProtoUtil.FromBytes<ContentRef.MetaData>(metaData);
				}

				public static implicit operator bool(Result result)
				{
					if (result.details.m_eResult.Success())
					{
						return result.details.m_nPublishedFileId != PublishedFileId_t.Invalid;
					}
					return false;
				}

				public static implicit operator PublishedFileId_t(Result result)
				{
					return result.details.m_nPublishedFileId;
				}

				public static bool operator ==(Result a, Result b)
				{
					return a.details.m_nPublishedFileId == b.details.m_nPublishedFileId;
				}

				public static bool operator !=(Result a, Result b)
				{
					return !(a == b);
				}

				public bool Equals(Result other)
				{
					return this == other;
				}

				public int CompareTo(Result other)
				{
					return index.CompareTo(other.index);
				}

				public override bool Equals(object obj)
				{
					if (obj is Result)
					{
						return this == (Result)obj;
					}
					return false;
				}

				public override int GetHashCode()
				{
					return details.m_nPublishedFileId.GetHashCode();
				}
			}

			[ProtoContract]
			public enum QueryType : byte
			{
				Trending,
				Recent,
				LikedCreators,
				CreatedByFriends,
				FriendFavorites,
				GroupMembersOnly
			}

			public const int PAGE_SIZE = 50;

			private EUGCQuery _queryType;

			private EUGCMatchingUGCType _matchingType;

			private QuerySettings _settings;

			private PoolKeepItemStackHandle<string> _previousQueryCursors;

			private SteamUGCQueryCompleted_t _result;

			private bool _isDisposed;

			private int _id;

			public int maxPageNumber => _result.m_unTotalMatchingResults.GetMaxPageNumber(50);

			public uint totalNumberOfResults => _result.m_unTotalMatchingResults;

			public int id => _id;

			public IQueryType type => IQueryType.All;

			static Query()
			{
				Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new Query(), delegate(Query q)
				{
					q.Dispose();
				}, delegate(Query q)
				{
					q._OnUnpool();
				});
			}

			public static Query Create(EUGCQuery queryType = EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType matchingType = EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems, IEnumerable<KeyValuePair<string, string>> keyValueTags = null, IEnumerable<string> tags = null, bool matchAnyTag = true, bool returnMetaData = false, bool returnChildren = false, bool returnKeyValueTags = false, bool returnAdditionalPreviews = false)
			{
				return Pools.Unpool<Query>()._SetData(queryType, matchingType, keyValueTags, tags, matchAnyTag, returnMetaData, returnChildren, returnKeyValueTags, returnAdditionalPreviews);
			}

			private Query()
			{
			}

			private void _OnUnpool()
			{
				_isDisposed = false;
				_id = Interlocked.Increment(ref QueryId);
			}

			private Query _SetData(EUGCQuery queryType, EUGCMatchingUGCType matchingType, IEnumerable<KeyValuePair<string, string>> keyValueTags, IEnumerable<string> tags, bool matchAnyTag, bool returnMetaData, bool returnChildren, bool returnKeyValueTags, bool returnAdditionalPreviews)
			{
				_queryType = queryType;
				_matchingType = matchingType;
				_settings = new QuerySettings(keyValueTags, tags, matchAnyTag, returnMetaData, returnChildren, returnKeyValueTags, returnAdditionalPreviews);
				_previousQueryCursors = Pools.UseKeepItemStack<string>();
				return this;
			}

			private async Task<PoolKeepItemListHandle<Result>> _GetPageByCursorAsync(string cursor)
			{
				SteamEx.ReleaseHandle(ref _result);
				if (DEBUG.LogText())
				{
					Log.Text($"SteamUGC.CreateQueryAllUGCRequest({_queryType}, {_matchingType}, {AppId}, {AppId}, {cursor})");
				}
				UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryAllUGCRequest(_queryType, _matchingType, AppId, AppId, cursor);
				if (queryHandle == UGCQueryHandle_t.Invalid)
				{
					if (DEBUG.LogWarning())
					{
						Log.Warning("SteamUGC.CreateQueryAllUGCRequest Failed To Create Query Handle");
					}
					SteamUGC.ReleaseQueryUGCRequest(queryHandle);
					return null;
				}
				_settings.SetSettings(queryHandle);
				if (DEBUG.LogText())
				{
					UGCQueryHandle_t uGCQueryHandle_t = queryHandle;
					Log.Text("SteamUGC.SendQueryUGCRequest(" + uGCQueryHandle_t.ToString() + "):All");
				}
				CallResultData<SteamUGCQueryCompleted_t> callResultData = await SteamUGC.SendQueryUGCRequest(queryHandle).ResultAsync<SteamUGCQueryCompleted_t>();
				if (callResultData.result.m_eResult.Failure() || _isDisposed)
				{
					if (DEBUG.LogWarning())
					{
						Log.Warning("SteamUGC.SendQueryUGCRequest Failed with Result: " + callResultData.result.m_eResult);
					}
					SteamUGC.ReleaseQueryUGCRequest(queryHandle);
					return null;
				}
				if (DEBUG.LogText())
				{
					Log.Text("SteamUGC.SendQueryUGCRequest Retrieved " + callResultData.result.m_unNumResultsReturned + " of " + callResultData.result.m_unTotalMatchingResults + " Results");
				}
				return (_result = callResultData).GetResults(ref _settings);
			}

			public async Task<PoolKeepItemListHandle<Result>> NextPageAsync()
			{
				if (!_result.m_rgchNextCursor.IsNullOrEmpty() && _result.m_rgchNextCursor != "*")
				{
					_previousQueryCursors.Push(_result.m_rgchNextCursor);
				}
				return await _GetPageByCursorAsync(_result.m_rgchNextCursor);
			}

			public async Task<PoolKeepItemListHandle<Result>> PreviousPageAsync()
			{
				return await _GetPageByCursorAsync((_previousQueryCursors.Count > 0) ? _previousQueryCursors.Pop() : null);
			}

			public async Task PageAllAsync(Action<IEnumerable<Result>> onPageResult)
			{
				int id = _id;
				uint pageNumber = 0u;
				do
				{
					PoolKeepItemListHandle<Result> poolKeepItemListHandle = await NextPageAsync();
					if (_id == id)
					{
						if ((bool)poolKeepItemListHandle)
						{
							uint num = pageNumber + 1;
							pageNumber = num;
							onPageResult(poolKeepItemListHandle.AsEnumerable());
						}
						continue;
					}
					break;
				}
				while ((bool)this && (pageNumber == 0 || pageNumber < maxPageNumber));
			}

			public void Dispose()
			{
				if (!_isDisposed)
				{
					_isDisposed = true;
					SteamEx.ReleaseHandle(ref _result);
					_settings.Repool();
					Pools.Repool(ref _previousQueryCursors);
					_result = default(SteamUGCQueryCompleted_t);
				}
			}

			public static implicit operator bool(Query query)
			{
				if (query != null)
				{
					return !query._isDisposed;
				}
				return false;
			}
		}

		public class QuerySpecific : IQuery, IDisposable
		{
			private PoolKeepItemListHandle<PublishedFileId_t> _fileIds;

			private QuerySettings _settings;

			private int _pageNumber;

			private SteamUGCQueryCompleted_t _result;

			private bool _isDisposed;

			private int _id;

			public int maxPage => _fileIds.value.GetMaxPageNumber(50);

			public int count => _fileIds.value.Count;

			public int id => _id;

			public IQueryType type => IQueryType.Specific;

			static QuerySpecific()
			{
				Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new QuerySpecific(), delegate(QuerySpecific q)
				{
					q.Dispose();
				}, delegate(QuerySpecific q)
				{
					q._OnUnpool();
				});
			}

			public static QuerySpecific Create(IEnumerable<PublishedFileId_t> fileIds, IEnumerable<KeyValuePair<string, string>> keyValueTags = null, IEnumerable<string> tags = null, bool matchAnyTag = true, bool returnMetaData = false, bool returnChildren = false, bool returnKeyValueTags = false, bool returnAdditionalPreviews = false)
			{
				return Pools.Unpool<QuerySpecific>()._SetData(fileIds, new QuerySettings(keyValueTags, tags, matchAnyTag, returnMetaData, returnChildren, returnKeyValueTags, returnAdditionalPreviews));
			}

			public static QuerySpecific Create(PublishedFileId_t fileId, IEnumerable<KeyValuePair<string, string>> keyValueTags = null, IEnumerable<string> tags = null, bool matchAnyTag = true, bool returnMetaData = false, bool returnChildren = false, bool returnKeyValueTags = false, bool returnAdditionalPreviews = false)
			{
				return Create(new PublishedFileId_t[1] { fileId }, keyValueTags, tags, matchAnyTag, returnMetaData, returnChildren, returnKeyValueTags, returnAdditionalPreviews);
			}

			private QuerySpecific()
			{
			}

			private void _OnUnpool()
			{
				_isDisposed = false;
				_id = Interlocked.Increment(ref QueryId);
			}

			private QuerySpecific _SetData(IEnumerable<PublishedFileId_t> fileIds, QuerySettings settings)
			{
				_fileIds = Pools.UseKeepItemList(fileIds);
				_settings = settings;
				return this;
			}

			public async Task<PoolKeepItemListHandle<Query.Result>> GetPageAsync(int pageNumber)
			{
				SteamEx.ReleaseHandle(ref _result);
				_pageNumber = Mathf.Clamp(pageNumber, 1, maxPage);
				PublishedFileId_t[] array = _fileIds.value.GetPagedResults(_pageNumber, 50).ToArray();
				UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(array, (uint)array.Length);
				_settings.SetSettings(queryHandle);
				if (DEBUG.LogText())
				{
					string[] obj = new string[9] { "SteamUGC.SendQueryUGCRequest(", null, null, null, null, null, null, null, null };
					UGCQueryHandle_t uGCQueryHandle_t = queryHandle;
					obj[1] = uGCQueryHandle_t.ToString();
					obj[2] = "):Specific(Page ";
					obj[3] = pageNumber.ToString();
					obj[4] = " of ";
					obj[5] = maxPage.ToString();
					obj[6] = ", Total Count: ";
					obj[7] = _fileIds.value.Count.ToString();
					obj[8] = ")";
					Log.Text(string.Concat(obj), appendToUserLog: false);
				}
				CallResultData<SteamUGCQueryCompleted_t> callResultData = await SteamUGC.SendQueryUGCRequest(queryHandle).ResultAsync<SteamUGCQueryCompleted_t>();
				if (callResultData.result.m_eResult.Failure() || _isDisposed)
				{
					if (DEBUG.LogWarning())
					{
						Log.Warning("SteamUGC.SendQueryUGCRequest Failed with Result: " + callResultData.result.m_eResult);
					}
					SteamUGC.ReleaseQueryUGCRequest(queryHandle);
					return null;
				}
				return _settings.FilterResults((_result = callResultData).GetResults(ref _settings));
			}

			public async Task<PoolKeepItemListHandle<Query.Result>> NextPageAsync()
			{
				return await GetPageAsync(++_pageNumber);
			}

			public async Task<PoolKeepItemListHandle<Query.Result>> PreviousPageAsync()
			{
				return await GetPageAsync(--_pageNumber);
			}

			public async Task PageAllAsync(Action<IEnumerable<Query.Result>> onPageResult)
			{
				int id = _id;
				int page = 1;
				do
				{
					PoolKeepItemListHandle<Query.Result> poolKeepItemListHandle = await GetPageAsync(page);
					if (_id != id)
					{
						break;
					}
					if ((bool)poolKeepItemListHandle)
					{
						int num = page + 1;
						page = num;
						if (num > 0)
						{
							onPageResult(poolKeepItemListHandle.AsEnumerable());
						}
					}
				}
				while ((bool)this && page <= maxPage);
			}

			private async Task _GetPageAsync(int pageNumber, Action<IEnumerable<Query.Result>> onPageResult)
			{
				int page = pageNumber;
				do
				{
					PoolKeepItemListHandle<Query.Result> poolKeepItemListHandle = await GetPageAsync(page);
					if ((bool)poolKeepItemListHandle)
					{
						int num = page + 1;
						page = num;
						if (num > 0)
						{
							onPageResult(poolKeepItemListHandle.AsEnumerable());
						}
					}
				}
				while ((bool)this && page == pageNumber);
			}

			public Task PageAllParallelAsync(Action<IEnumerable<Query.Result>> onPageResult)
			{
				return Task.WhenAll(from p in Enumerable.Range(1, maxPage)
					select _GetPageAsync(p, onPageResult));
			}

			public void Dispose()
			{
				if (!_isDisposed)
				{
					_isDisposed = true;
					SteamEx.ReleaseHandle(ref _result);
					_settings.Repool();
					Pools.Repool(ref _fileIds);
					_result = default(SteamUGCQueryCompleted_t);
				}
			}

			public static implicit operator bool(QuerySpecific query)
			{
				if (query != null)
				{
					return !query._isDisposed;
				}
				return false;
			}
		}

		public class QueryUser : IQuery, IDisposable
		{
			private CSteamID _userId;

			private EUserUGCList _list;

			private EUGCMatchingUGCType _type;

			private EUserUGCListSortOrder _sort;

			private QuerySettings _settings;

			private int _pageNumber;

			private SteamUGCQueryCompleted_t _result;

			private bool _isDisposed;

			private int _id;

			public int maxPage => _result.m_unTotalMatchingResults.GetMaxPageNumber(50);

			public int id => _id;

			public IQueryType type => IQueryType.User;

			static QueryUser()
			{
				Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new QueryUser(), delegate(QueryUser q)
				{
					q.Dispose();
				}, delegate(QueryUser q)
				{
					q._OnUnpool();
				});
			}

			public static QueryUser Create(CSteamID userId, EUserUGCList list = EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType type = EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems, EUserUGCListSortOrder sort = EUserUGCListSortOrder.k_EUserUGCListSortOrder_VoteScoreDesc, IEnumerable<KeyValuePair<string, string>> keyValueTags = null, IEnumerable<string> tags = null, bool matchAnyTag = true, bool returnMetaData = false, bool returnChildren = false, bool returnKeyValueTags = false, bool returnAdditionalPreviews = false)
			{
				return Pools.Unpool<QueryUser>()._SetData(userId, list, type, sort, new QuerySettings(keyValueTags, tags, matchAnyTag, returnMetaData, returnChildren, returnKeyValueTags, returnAdditionalPreviews));
			}

			private QueryUser()
			{
			}

			private void _OnUnpool()
			{
				_isDisposed = false;
				_id = Interlocked.Increment(ref QueryId);
			}

			private QueryUser _SetData(CSteamID userId, EUserUGCList list, EUGCMatchingUGCType type, EUserUGCListSortOrder sort, QuerySettings settings)
			{
				_userId = userId;
				_list = list;
				_type = type;
				_sort = sort;
				_settings = settings;
				return this;
			}

			public async Task<PoolKeepItemListHandle<Query.Result>> GetPageAsync(int pageNumber)
			{
				SteamEx.ReleaseHandle(ref _result);
				_pageNumber = Mathf.Clamp(pageNumber, 1, maxPage);
				UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUserUGCRequest(_userId.GetAccountID(), _list, _type, _sort, AppId, AppId, (uint)_pageNumber);
				_settings.SetSettings(queryHandle);
				if (DEBUG.LogText())
				{
					UGCQueryHandle_t uGCQueryHandle_t = queryHandle;
					Log.Text("SteamUGC.SendQueryUGCRequest(" + uGCQueryHandle_t.ToString() + "):User");
				}
				CallResultData<SteamUGCQueryCompleted_t> callResultData = await SteamUGC.SendQueryUGCRequest(queryHandle).ResultAsync<SteamUGCQueryCompleted_t>();
				if (callResultData.result.m_eResult.Failure() || _isDisposed)
				{
					if (DEBUG.LogWarning())
					{
						Log.Warning("SteamUGC.SendQueryUGCRequest Failed with Result: " + callResultData.result.m_eResult);
					}
					SteamUGC.ReleaseQueryUGCRequest(queryHandle);
					return null;
				}
				return (_result = callResultData).GetResults(ref _settings);
			}

			public async Task<PoolKeepItemListHandle<Query.Result>> NextPageAsync()
			{
				return await GetPageAsync(++_pageNumber);
			}

			public async Task<PoolKeepItemListHandle<Query.Result>> PreviousPageAsync()
			{
				return await GetPageAsync(--_pageNumber);
			}

			public async Task PageAllAsync(Action<IEnumerable<Query.Result>> onPageResult)
			{
				int id = _id;
				int page = 1;
				do
				{
					PoolKeepItemListHandle<Query.Result> poolKeepItemListHandle = await GetPageAsync(page);
					if (_id != id)
					{
						break;
					}
					if ((bool)poolKeepItemListHandle)
					{
						int num = page + 1;
						page = num;
						if (num > 0)
						{
							onPageResult(poolKeepItemListHandle.AsEnumerable());
						}
					}
				}
				while ((bool)this && page <= maxPage);
			}

			public void Dispose()
			{
				if (!_isDisposed)
				{
					_isDisposed = true;
					SteamEx.ReleaseHandle(ref _result);
					_settings.Repool();
					_result = default(SteamUGCQueryCompleted_t);
				}
			}

			public static implicit operator bool(QueryUser query)
			{
				if (query != null)
				{
					return !query._isDisposed;
				}
				return false;
			}
		}

		public class QueryUsers : IQuery, IDisposable
		{
			public PoolListHandle<QueryUser> _queries;

			private int _id;

			public int id => _id;

			public IQueryType type => IQueryType.Users;

			static QueryUsers()
			{
				Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new QueryUsers(), delegate(QueryUsers q)
				{
					q.Dispose();
				}, delegate(QueryUsers q)
				{
					q._OnUnpool();
				});
			}

			public static QueryUsers Create(IEnumerable<CSteamID> userIds, EUserUGCList list = EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType type = EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems, EUserUGCListSortOrder sort = EUserUGCListSortOrder.k_EUserUGCListSortOrder_VoteScoreDesc, IEnumerable<KeyValuePair<string, string>> keyValueTags = null, IEnumerable<string> tags = null, bool matchAnyTag = true, bool returnMetaData = false, bool returnChildren = false, bool returnKeyValueTags = false, bool returnAdditionalPreviews = false)
			{
				QueryUsers queryUsers = Pools.Unpool<QueryUsers>();
				queryUsers._queries = Pools.UseList(userIds.Select((CSteamID id) => QueryUser.Create(id, list, type, sort, keyValueTags, tags, matchAnyTag, returnMetaData, returnChildren, returnKeyValueTags, returnAdditionalPreviews)));
				return queryUsers;
			}

			private QueryUsers()
			{
			}

			private void _OnUnpool()
			{
				_id = Interlocked.Increment(ref QueryId);
			}

			public void Dispose()
			{
				Pools.Repool(ref _queries);
			}

			public async Task PageAllAsync(Action<IEnumerable<Query.Result>> onPageResult)
			{
				await _queries.value.Select((QueryUser q) => q.PageAllAsync(onPageResult));
			}
		}

		[UIField]
		public class InspectQueryResult
		{
			private static TextBuilder _Builder;

			[UIField(filter = ItemType.Author)]
			public Query.Result creator;

			[UIField]
			public Query.Result item;

			private Transform _transform;

			private Action<ContentRef> _onUpdated;

			private static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

			[UIField(fixedSize = true, view = "UI/Input Field Multiline", collapse = UICollapseType.Open)]
			public string details
			{
				get
				{
					Builder.Append("Visibility: ").Append(EnumUtil.FriendlyName(item.visibility)).NewLine();
					Builder.Append("Mature Content: ").Append(EnumUtil.FriendlyName(item.mature)).NewLine();
					if (item.belongsToGroup)
					{
						Builder.Append("Groups: ").Append((from g in item.Groups()
							where g
							select g).ToStringSmart((Friends.Group g) => g.name)).NewLine();
					}
					if (!item.description.IsNullOrEmpty())
					{
						Builder.Append("Description: ").Append(item.description).NewLine();
					}
					if (!item.tags.IsNullOrEmpty())
					{
						Builder.Append("Tags: ").Append(item.tags.Split(SPLIT_TAGS).ToStringSmart()).NewLine();
					}
					Builder.Append("Created On: ").Append($"{MathUtil.FromUnixEpoch(item.timeCreated).ToLocalTime():f}").NewLine();
					if (item.timeUpdated > item.timeCreated)
					{
						Builder.Append("Last Updated On: ").Append($"{MathUtil.FromUnixEpoch(item.timeUpdated).ToLocalTime():f}").NewLine();
					}
					if (Application.isEditor && item.keyValueTags != null)
					{
						Builder.Append("Key-Value Tags: ").Append(item.keyValueTags.ToStringSmart()).NewLine();
					}
					Builder.Append("Install Status: ").Append(EnumUtil.FriendlyName(ContentRef.GetPublishedIdInstallStatus(item.id, item.timeUpdated))).NewLine();
					ContentRef.MetaData metaData = item.GetMetaData();
					if ((bool)metaData)
					{
						Builder.Append("Number Of UGC Dependencies: ").Append(metaData.ugcDependencyCount).Append(", Total Size In Bytes: ")
							.Append(StringUtil.FormatLargeNumber(metaData.ugcFileSizeInBytes))
							.Append("B")
							.NewLine();
					}
					if (Application.isEditor && (bool)metaData)
					{
						Builder.Append("Version: ").Append(metaData.version).NewLine();
					}
					if (Application.isEditor && (bool)metaData && metaData.hasQuickDependencies)
					{
						Builder.Append("Quick Dependencies: ").Append(metaData.GetQuickDependencies().ToStringSmart()).NewLine();
					}
					if (Application.isEditor && !item.additionalPreviews.IsNullOrEmpty())
					{
						Builder.Append("Additional Previews: ").Append(item.additionalPreviews.ToStringSmart()).NewLine();
					}
					return Builder.RemoveNewLine().ToString();
				}
				private set
				{
				}
			}

			private bool _hideUpdate => _onUpdated == null;

			public InspectQueryResult(Query.Result creatorResult, Query.Result itemResult, Transform transform, Action<ContentRef> onUpdated = null)
			{
				creator = creatorResult;
				item = itemResult;
				_transform = transform;
				_onUpdated = onUpdated;
			}

			[UIField]
			public void CopyItemCodeToClipboard()
			{
				item.details.m_nPublishedFileId.m_PublishedFileId.ToString().CopyToClipboard();
			}

			[UIField]
			[UIHideIf("_hideUpdate")]
			private void _CheckForUpdates()
			{
				UIUtil.CreateContentRefDownloadPopup(item, delegate(ContentRef cRef)
				{
					_onUpdated(cRef);
				}, _transform, _transform.GetComponentInParent<UIPopupControl>().Close);
			}
		}

		private static readonly CallbackEvent<DownloadItemResult_t> _DownloadItemEvent = new CallbackEvent<DownloadItemResult_t>();

		private static string _installFolderPath;

		public const string VISIBILITY_KEY = "V";

		public const string MATURE_KEY = "M";

		public const string GROUP_KEY = "G";

		private const string AUTHOR_KEY = "A";

		private static readonly char[] SPLIT_TAGS = new char[1] { ',' };

		public const string PREVIEW_INSPECT = "I";

		public const string PREVIEW_INSPECT_AUDIO = "IA";

		public const string PREVIEW_DATA = "D";

		public const int PENDING_REVIEW_COUNT = 10;

		private static readonly KeyValuePair<string, string>[] AuthorKeyValueTag = new KeyValuePair<string, string>[1]
		{
			new KeyValuePair<string, string>("A", "")
		};

		private static readonly Dictionary<ulong, Task<Query.Result>> _CachedAuthorFiles = new Dictionary<ulong, Task<Query.Result>>();

		private static readonly Dictionary<ulong, Task<UserItemVoteResult>> _CachedUserVotes = new Dictionary<ulong, Task<UserItemVoteResult>>();

		private static int QueryId;

		private static Dictionary<Type, KeyValuePair<string, string>[]> _ProtoMetaKeyValueTags;

		private static string _installCacheFileName
		{
			get
			{
				AppId_t appId = AppId;
				return "appworkshop_" + appId.ToString() + ".acf";
			}
		}

		public static Transform CreateItemTransform { private get; set; }

		private static Dictionary<Type, KeyValuePair<string, string>[]> ProtoMetaKeyValueTags => _ProtoMetaKeyValueTags ?? (_ProtoMetaKeyValueTags = new Dictionary<Type, KeyValuePair<string, string>[]>());

		public static event Action<PublishedFileId_t, UserItemVoteResult, UserItemVoteResult> OnItemVoteChange;

		public static async Task<PublishedFileId_t> CreateAuthorFile()
		{
			Query.Result result = await _GetAuthorFile(SteamId);
			if ((bool)result)
			{
				return result;
			}
			CreateItemResult_t createItemResult_t = await CreateItemAsync();
			if (createItemResult_t.m_eResult.Failure())
			{
				return PublishedFileId_t.Invalid;
			}
			SubmitItemUpdateResult_t submitItemUpdateResult_t = await UploadAsync(null, null, createItemResult_t.m_nPublishedFileId.m_PublishedFileId, null, null, null, null, AuthorKeyValueTag);
			return submitItemUpdateResult_t.m_eResult.Success() ? submitItemUpdateResult_t.m_nPublishedFileId : PublishedFileId_t.Invalid;
		}

		private static async Task<Query.Result> _GetAuthorFile(CSteamID userSteamId)
		{
			using QueryUser query = QueryUser.Create(userSteamId, EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc, AuthorKeyValueTag);
			using PoolKeepItemListHandle<Query.Result> poolKeepItemListHandle = await query.NextPageAsync();
			return ((bool)poolKeepItemListHandle && poolKeepItemListHandle.Count > 0) ? poolKeepItemListHandle[0] : default(Query.Result);
		}

		public static async Task<Query.Result> GetAuthorFile(CSteamID userSteamId)
		{
			if (!_CachedAuthorFiles.ContainsKey(userSteamId.m_SteamID))
			{
				_CachedAuthorFiles[userSteamId.m_SteamID] = _GetAuthorFile(userSteamId);
			}
			Query.Result obj = await _CachedAuthorFiles[userSteamId.m_SteamID];
			if (!obj)
			{
				_CachedAuthorFiles.Remove(userSteamId.m_SteamID);
			}
			return obj;
		}

		public static void ClearAuthorFileCache()
		{
			_CachedAuthorFiles.Clear();
		}

		private static async Task<bool> _ShowWorkshopLegalAgreement(EWorkshopFileType fileType)
		{
			if (DEBUG.LogError())
			{
				Log.Error("You need to accept the Steam Workshop Legal Agreement before you can submit content to the Steam Workshop. Please accept agreement and try again.");
			}
			await new AwaitCoroutine<object>(UIUtil.ShowWebBrowser("https://steamcommunity.com/sharedfiles/workshoplegalagreement", CreateItemTransform, "Steam Workshop Legal Agreement", skipConfirmation: true));
			CallResultData<CreateItemResult_t> checkIfAccepted = await SteamUGC.CreateItem(AppId, fileType).ResultAsync<CreateItemResult_t>();
			if (checkIfAccepted.result.m_eResult.Failure())
			{
				return false;
			}
			await DeleteAsync(checkIfAccepted.result.m_nPublishedFileId);
			return !checkIfAccepted.result.m_bUserNeedsToAcceptWorkshopLegalAgreement;
		}

		public static async Task<CreateItemResult_t> CreateItemAsync(EWorkshopFileType fileType = EWorkshopFileType.k_EWorkshopFileTypeGameManagedItem)
		{
			CallResultData<CreateItemResult_t> callResult = await SteamUGC.CreateItem(AppId, fileType).ResultAsync<CreateItemResult_t>();
			if (DEBUG.LogWarning() && callResult.result.m_eResult.Failure())
			{
				Log.Warning("SteamUGC.CreateItem Failed with Result: " + callResult.result.m_eResult);
			}
			else if (DEBUG.LogText())
			{
				Log.Text("SteamUGC.CreateItem Succeeded for PublishedFile: " + callResult.result.m_nPublishedFileId.m_PublishedFileId, appendToUserLog: false);
			}
			bool flag = callResult.result.m_bUserNeedsToAcceptWorkshopLegalAgreement;
			if (flag)
			{
				flag = !(await _ShowWorkshopLegalAgreement(fileType));
			}
			if (flag)
			{
				CreateItemResult_t result = default(CreateItemResult_t);
				result.m_eResult = EResult.k_EResultInsufficientPrivilege;
				return result;
			}
			return callResult;
		}

		public static async Task<SubmitItemUpdateResult_t> UploadAsync(string title, string absoluteContentFolderPath, ulong publishedFileId = 0uL, Action<ulong> onPublishedFiledIdCreated = null, string description = null, IEnumerable<string> tags = null, string previewContentPath = null, IEnumerable<KeyValuePair<string, string>> keyValueTags = null, byte[] metaData = null, EWorkshopFileType fileType = EWorkshopFileType.k_EWorkshopFileTypeGameManagedItem, ERemoteStoragePublishedFileVisibility visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic, Action<UploadInfo> onUploadInfo = null, IEnumerable<string> additionalPreviewPaths = null, string workshopTrailerVideoId = "")
		{
			if (publishedFileId == PublishedFileId_t.Invalid.m_PublishedFileId)
			{
				CreateItemResult_t createItemResult_t = await CreateItemAsync(fileType);
				if (createItemResult_t.m_eResult.Failure())
				{
					return default(SubmitItemUpdateResult_t);
				}
				publishedFileId = createItemResult_t.m_nPublishedFileId.m_PublishedFileId;
				if (onPublishedFiledIdCreated != null)
				{
					onPublishedFiledIdCreated(publishedFileId);
				}
				else if (DEBUG.LogError())
				{
					Log.Error("Steam.Ugc.Upload created new workshop item but publishedFileId was not consumed by uploading process.");
				}
			}
			UGCUpdateHandle_t updateHandle = SteamUGC.StartItemUpdate(AppId, (PublishedFileId_t)publishedFileId);
			if (title != null)
			{
				SteamUGC.SetItemTitle(updateHandle, title);
			}
			if (absoluteContentFolderPath != null)
			{
				SteamUGC.SetItemContent(updateHandle, absoluteContentFolderPath);
			}
			if (description != null)
			{
				SteamUGC.SetItemDescription(updateHandle, description);
			}
			if (tags != null)
			{
				SteamUGC.SetItemTags(updateHandle, tags.Where((string s) => !s.IsNullOrEmpty()).Distinct(StringComparer.OrdinalIgnoreCase).ToList());
			}
			if (previewContentPath != null)
			{
				SteamUGC.SetItemPreview(updateHandle, new FileInfo(previewContentPath).FullName);
			}
			if (keyValueTags != null)
			{
				SteamUGC.RemoveAllItemKeyValueTags(updateHandle);
				foreach (KeyValuePair<string, string> item in keyValueTags.Distinct())
				{
					SteamUGC.AddItemKeyValueTag(updateHandle, item.Key, item.Value);
				}
			}
			if (metaData != null && metaData.Length * 3 <= 5000)
			{
				SteamUGC.SetItemMetadata(updateHandle, Convert.ToBase64String(metaData));
			}
			SteamUGC.SetItemVisibility(updateHandle, visibility);
			if (additionalPreviewPaths != null)
			{
				using (QuerySpecific query = QuerySpecific.Create((PublishedFileId_t)publishedFileId, null, null, matchAnyTag: true, returnMetaData: false, returnChildren: false, returnKeyValueTags: false, returnAdditionalPreviews: true))
				{
					using PoolKeepItemListHandle<Query.Result> poolKeepItemListHandle2 = await query.NextPageAsync();
					using PoolKeepItemListHandle<string> poolKeepItemListHandle = Pools.UseKeepItemList(additionalPreviewPaths);
					using PoolKeepItemDictionaryHandle<string, string> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary(poolKeepItemListHandle.value.Select((string p) => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(p), p)));
					if ((bool)poolKeepItemListHandle2 && poolKeepItemListHandle2.Count == 1 && !poolKeepItemListHandle2[0].additionalPreviews.IsNullOrEmpty())
					{
						for (uint num = 0u; num < poolKeepItemListHandle2[0].additionalPreviews.Length; num++)
						{
							Query.Result.AdditionalPreview additionalPreview = poolKeepItemListHandle2[0].additionalPreviews[num];
							if (poolKeepItemDictionaryHandle.ContainsKey(additionalPreview.name))
							{
								SteamUGC.UpdateItemPreviewFile(updateHandle, num, poolKeepItemDictionaryHandle[additionalPreview.name]);
								poolKeepItemDictionaryHandle.Remove(additionalPreview.name);
							}
							else
							{
								SteamUGC.RemoveItemPreview(updateHandle, num);
							}
						}
						foreach (string value in poolKeepItemDictionaryHandle.value.Values)
						{
							SteamUGC.AddItemPreviewFile(updateHandle, value, EItemPreviewType.k_EItemPreviewType_Image);
						}
					}
					else
					{
						foreach (string item2 in poolKeepItemListHandle.value)
						{
							SteamUGC.AddItemPreviewFile(updateHandle, item2, EItemPreviewType.k_EItemPreviewType_Image);
						}
					}
				}
				if (workshopTrailerVideoId.HasVisibleCharacter())
				{
					SteamUGC.AddItemPreviewVideo(updateHandle, workshopTrailerVideoId);
				}
			}
			UploadInfo uploadInfo = new UploadInfo(updateHandle);
			CallResultData<SubmitItemUpdateResult_t> submitResult = await SteamUGC.SubmitItemUpdate(updateHandle, null).ResultAsync<SubmitItemUpdateResult_t>((onUploadInfo != null) ? ((Action)delegate
			{
				onUploadInfo(uploadInfo.Update());
			}) : null);
			if (DEBUG.LogWarning() && submitResult.result.m_eResult.Failure())
			{
				Log.Warning("SteamUGC.SubmitItemUpdate Failed with Result: " + submitResult.result.m_eResult);
			}
			else if (DEBUG.LogText())
			{
				Log.Text("SteamUGC.SubmitItemUpdate Succeeded for PublishedFile: " + submitResult.result.m_nPublishedFileId.m_PublishedFileId, appendToUserLog: false);
			}
			bool flag = submitResult.result.m_bUserNeedsToAcceptWorkshopLegalAgreement;
			if (flag)
			{
				flag = !(await _ShowWorkshopLegalAgreement(fileType));
			}
			if (flag)
			{
				SubmitItemUpdateResult_t result = default(SubmitItemUpdateResult_t);
				result.m_eResult = EResult.k_EResultInsufficientPrivilege;
				return result;
			}
			return submitResult;
		}

		public static async Task<InstallInfo> DownloadAsync(PublishedFileId_t publishedFiledId, bool highPriority = true, Action<DownloadInfo> onDownloadInfo = null)
		{
			if (!SteamUGC.DownloadItem(publishedFiledId, highPriority))
			{
				return GetInstallInfo(publishedFiledId);
			}
			if (DEBUG.LogText())
			{
				Log.Text("Steam.Ugc.DownloadAsync(" + publishedFiledId.m_PublishedFileId + ", " + highPriority + ")", appendToUserLog: false);
			}
			DownloadInfo downloadInfo = new DownloadInfo(publishedFiledId);
			DownloadItemResult_t downloadItemResult_t = await _DownloadItemEvent.WaitAsync((DownloadItemResult_t result) => result.m_nPublishedFileId == publishedFiledId && result.m_unAppID == AppId, (onDownloadInfo != null) ? ((Action)delegate
			{
				onDownloadInfo(downloadInfo.Update());
			}) : null);
			if (DEBUG.LogWarning() && downloadItemResult_t.m_eResult.Failure())
			{
				Log.Warning("SteamUGC.DownloadItem Failed with Result: " + downloadItemResult_t.m_eResult);
			}
			else if (DEBUG.LogText())
			{
				Log.Text("SteamUGC.DownloadItem Finished for [" + publishedFiledId.m_PublishedFileId + "]", appendToUserLog: false);
			}
			return GetInstallInfo(publishedFiledId);
		}

		public static InstallInfo GetInstallInfo(PublishedFileId_t publishedFileId)
		{
			InstallInfo result = publishedFileId;
			result.isInstalled = SteamUGC.GetItemInstallInfo(publishedFileId, out result.sizeInBytes, out result.filepath, 2048u, out result.lastUpdated);
			if (result.isInstalled && _installFolderPath == null && Directory.Exists(result.filepath))
			{
				_installFolderPath = new FileInfo(result.filepath).Directory.FullName;
			}
			return result;
		}

		private static async Task<InstallInfo> _DownloadAsync(PublishedFileId_t publishedFileId, string urlToPng, uint lastUpdated)
		{
			if (DEBUG.LogText())
			{
				Log.Text($"Steam.Ugc._DownloadAsync({publishedFileId}, {urlToPng}, {lastUpdated})", appendToUserLog: false);
			}
			byte[] array = await AwaitExtensions.LoadBytesFromPngAsync(urlToPng);
			DirectoryInfo directoryInfo = ProtoUtil.FromBytes<SerializedDirectory>(array).CreateDirectoryAndFiles(new DirectoryInfo(Path.Combine(IOUtil.TempDownloadPath, publishedFileId.ToString())));
			InstallInfo result = default(InstallInfo);
			result.filepath = directoryInfo.FullName;
			result.isInstalled = true;
			result.publishedFileId = publishedFileId;
			result.sizeInBytes = (ulong)array.Length;
			result.lastUpdated = lastUpdated;
			return result;
		}

		public static async Task<InstallInfo> DownloadAsync(Query.Result queryResult)
		{
			string additionalPreviewURL = queryResult.GetAdditionalPreviewURL("D");
			return (additionalPreviewURL == null) ? (await DownloadAsync(queryResult, highPriority: true, null)) : (await _DownloadAsync(queryResult, additionalPreviewURL, queryResult.timeUpdated));
		}

		private static KeyValuePair<string, string>[] _GetProtoMetaKeyValueTags<T>() where T : class
		{
			if (!ProtoMetaKeyValueTags.ContainsKey(typeof(T)))
			{
				Dictionary<Type, KeyValuePair<string, string>[]> protoMetaKeyValueTags = ProtoMetaKeyValueTags;
				Type typeFromHandle = typeof(T);
				KeyValuePair<string, string>[] obj = new KeyValuePair<string, string>[1]
				{
					new KeyValuePair<string, string>("ProtoFileMeta", typeof(T).GetUILabel())
				};
				KeyValuePair<string, string>[] result = obj;
				protoMetaKeyValueTags[typeFromHandle] = obj;
				return result;
			}
			return ProtoMetaKeyValueTags[typeof(T)];
		}

		private static QueryUser _QueryProtoFileMeta<T>(CSteamID steamId) where T : class
		{
			return QueryUser.Create(steamId, EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc, _GetProtoMetaKeyValueTags<T>(), null, matchAnyTag: true, returnMetaData: true);
		}

		public static async Task<T> GetUserProtoFileMeta<T>(CSteamID? steamId = null) where T : class
		{
			using QueryUser query = _QueryProtoFileMeta<T>(steamId ?? SteamId);
			using PoolKeepItemListHandle<Query.Result> poolKeepItemListHandle = await query.NextPageAsync();
			if ((bool)poolKeepItemListHandle && poolKeepItemListHandle.Count == 0)
			{
				return ConstructorCache<T>.Constructor();
			}
			Query.Result result = (((bool)poolKeepItemListHandle && poolKeepItemListHandle.Count > 0) ? poolKeepItemListHandle[0] : default(Query.Result));
			return result ? ProtoUtil.FromBytes<T>(result.metaData) : null;
		}

		public static async Task<bool> SetUserProtoFileMeta<T>(T value) where T : class
		{
			using QueryUser query = _QueryProtoFileMeta<T>(SteamId);
			using PoolKeepItemListHandle<Query.Result> results = await query.NextPageAsync();
			Query.Result firstResult = (((bool)results && results.Count > 0) ? results[0] : default(Query.Result));
			if (!firstResult)
			{
				firstResult.id = (await CreateItemAsync()).m_nPublishedFileId.m_PublishedFileId;
			}
			if (firstResult.id == PublishedFileId_t.Invalid.m_PublishedFileId)
			{
				return false;
			}
			return (await UploadAsync(null, null, firstResult.id, null, null, null, null, _GetProtoMetaKeyValueTags<T>(), ProtoUtil.ToByteArray(value))).m_eResult.Success();
		}

		public static async Task<PoolKeepItemListHandle<T>> GetUsersProtoMetaFiles<T>(IEnumerable<CSteamID> userIds) where T : class
		{
			PoolKeepItemListHandle<T> output = Pools.UseKeepItemList<T>();
			using (QueryUsers query = QueryUsers.Create(userIds, EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc, _GetProtoMetaKeyValueTags<T>(), null, matchAnyTag: true, returnMetaData: true))
			{
				await query.PageAllAsync(delegate(IEnumerable<Query.Result> results)
				{
					output.value.AddMany(results.Select((Query.Result result) => ProtoUtil.FromBytes<T>(result.metaData)));
				});
			}
			return output;
		}

		public static async Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeItem(PublishedFileId_t publishedFileId)
		{
			return (await SteamUGC.SubscribeItem(publishedFileId).ResultAsync<RemoteStorageSubscribePublishedFileResult_t>()).result;
		}

		public static PublishedFileId_t[] GetSubscribedItems()
		{
			PublishedFileId_t[] array = new PublishedFileId_t[SteamUGC.GetNumSubscribedItems()];
			SteamUGC.GetSubscribedItems(array, (uint)array.Length);
			return array;
		}

		public static async Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeItem(PublishedFileId_t publishedFileId)
		{
			return (await SteamUGC.UnsubscribeItem(publishedFileId).ResultAsync<RemoteStorageUnsubscribePublishedFileResult_t>()).result;
		}

		public static async Task<Query.Result> GetDetailsAsync(PublishedFileId_t publishedFileId, bool returnMetaData = false, bool returnChildren = false, bool returnKeyValueTags = false, bool returnAdditionalPreviews = false)
		{
			using QuerySpecific query = QuerySpecific.Create(publishedFileId, null, null, matchAnyTag: true, returnMetaData, returnChildren, returnKeyValueTags, returnAdditionalPreviews);
			PoolKeepItemListHandle<Query.Result> poolKeepItemListHandle = await query.GetPageAsync(1);
			if ((bool)poolKeepItemListHandle && poolKeepItemListHandle.Count > 0)
			{
				return poolKeepItemListHandle.AsEnumerable().First();
			}
			return default(Query.Result);
		}

		public static async Task<UserFavoriteItemsListChanged_t> AddItemToFavoritesAsync(PublishedFileId_t publishedFileId)
		{
			return await SteamUGC.AddItemToFavorites(AppId, publishedFileId).ResultAsync<UserFavoriteItemsListChanged_t>();
		}

		public static async Task<UserFavoriteItemsListChanged_t> RemoveItemFromFavoritesAsync(PublishedFileId_t publishedFileId)
		{
			return await SteamUGC.RemoveItemFromFavorites(AppId, publishedFileId).ResultAsync<UserFavoriteItemsListChanged_t>();
		}

		public static async Task<SetUserItemVoteResult_t> SetUserItemVoteAsync(PublishedFileId_t publishedFileId, bool upVote)
		{
			UserItemVoteResult existingVote = await GetUserItemVoteAsync(publishedFileId);
			if (existingVote == UserItemVoteResult.Failure)
			{
				return default(SetUserItemVoteResult_t);
			}
			CallResultData<SetUserItemVoteResult_t> callResultData = await SteamUGC.SetUserItemVote(publishedFileId, upVote).ResultAsync<SetUserItemVoteResult_t>();
			if (callResultData.result.m_eResult.Failure())
			{
				return callResultData;
			}
			UserItemVoteResult userItemVoteResult = (callResultData.result.m_bVoteUp ? UserItemVoteResult.UpVote : UserItemVoteResult.DownVote);
			if (userItemVoteResult != existingVote)
			{
				_CachedUserVotes.Remove(publishedFileId.m_PublishedFileId);
				if (Ugc.OnItemVoteChange != null)
				{
					Ugc.OnItemVoteChange(publishedFileId, existingVote, userItemVoteResult);
				}
			}
			return callResultData;
		}

		private static async Task<UserItemVoteResult> _GetUserItemVoteAsync(PublishedFileId_t publishedFileId)
		{
			return (await SteamUGC.GetUserItemVote(publishedFileId).ResultAsync<GetUserItemVoteResult_t>()).result.ResultType();
		}

		public static async Task<UserItemVoteResult> GetUserItemVoteAsync(PublishedFileId_t publishedFileId)
		{
			if (!_CachedUserVotes.ContainsKey(publishedFileId.m_PublishedFileId))
			{
				_CachedUserVotes[publishedFileId.m_PublishedFileId] = _GetUserItemVoteAsync(publishedFileId);
			}
			UserItemVoteResult num = await _CachedUserVotes[publishedFileId.m_PublishedFileId];
			if (num == UserItemVoteResult.Failure)
			{
				_CachedUserVotes.Remove(publishedFileId.m_PublishedFileId);
			}
			return num;
		}

		public static void ClearUserVoteCache()
		{
			_CachedUserVotes.Clear();
		}

		public static async Task<DeleteItemResult_t> DeleteAsync(PublishedFileId_t publishedFileId)
		{
			if (DEBUG.LogText())
			{
				Log.Text("Deleting Published File Id: " + publishedFileId.m_PublishedFileId);
			}
			return await SteamUGC.DeleteItem(publishedFileId).ResultAsync<DeleteItemResult_t>();
		}

		public static async Task<bool> DeleteAllPublishedByUserAsync(CSteamID userId, EUGCMatchingUGCType type = EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems)
		{
			if (!Application.isEditor || !userId.IsValid())
			{
				return false;
			}
			using QueryUser query = QueryUser.Create(userId, EUserUGCList.k_EUserUGCList_Published, type);
			PoolKeepItemListHandle<Query.Result> allResults = Pools.UseKeepItemList<Query.Result>();
			try
			{
				await query.PageAllAsync(delegate(IEnumerable<Query.Result> results)
				{
					allResults.value.AddMany(results);
				});
				return (await allResults.value.Select((Query.Result result) => DeleteAsync(result.details.m_nPublishedFileId))).All((DeleteItemResult_t delete) => delete.m_eResult.Success());
			}
			finally
			{
				if (allResults != null)
				{
					((IDisposable)allResults).Dispose();
				}
			}
		}
	}

	public static class Friends
	{
		public struct Group : IEquatable<Group>
		{
			public readonly CSteamID id;

			public string name => SteamFriends.GetClanName(this);

			public Group(CSteamID id)
			{
				this.id = id;
			}

			public override string ToString()
			{
				if (!name.HasVisibleCharacter())
				{
					return "UNKNOWN GROUP";
				}
				return name;
			}

			public static implicit operator CSteamID(Group g)
			{
				return g.id;
			}

			public static implicit operator bool(Group group)
			{
				if (group.id.BClanAccount())
				{
					return group.name.HasVisibleCharacter();
				}
				return false;
			}

			public bool Equals(Group other)
			{
				return id.Equals(other.id);
			}

			public override bool Equals(object obj)
			{
				if (obj is Group)
				{
					return Equals((Group)obj);
				}
				return false;
			}

			public override int GetHashCode()
			{
				CSteamID cSteamID = id;
				return cSteamID.GetHashCode();
			}
		}

		private static CallbackEvent<PersonaStateChange_t> _PersoneStateChangeEvent = new CallbackEvent<PersonaStateChange_t>();

		private static HashSet<CSteamID> _Friends;

		private static HashSet<Group> _Groups;

		private static Dictionary<ulong, Task<string>> _NameCache = new Dictionary<ulong, Task<string>>();

		public static HashSet<CSteamID> CachedFriends => _Friends ?? (_Friends = new HashSet<CSteamID>(GetFriends()));

		public static HashSet<Group> CachedGroups => _Groups ?? (_Groups = new HashSet<Group>(from g in GetGroups()
			where g
			select g));

		private static async Task<string> _GetPersonaNameAsync(CSteamID userId)
		{
			if (SteamFriends.RequestUserInformation(userId, bRequireNameOnly: true))
			{
				await _PersoneStateChangeEvent.WaitAsync((PersonaStateChange_t persona) => (CSteamID)persona.m_ulSteamID == userId);
			}
			return SteamFriends.GetFriendPersonaName(userId);
		}

		public static async Task<string> GetPersonaNameAsync(CSteamID userId)
		{
			if (!_NameCache.ContainsKey(userId.m_SteamID))
			{
				_NameCache.Add(userId.m_SteamID, _GetPersonaNameAsync(userId));
			}
			return await _NameCache[userId.m_SteamID];
		}

		public static void ClearNameCache()
		{
			_NameCache.Clear();
		}

		public static string GetCachedName(ulong userId)
		{
			if (!_NameCache.ContainsKey(userId) || !_NameCache[userId].IsCompleted)
			{
				return "N/A";
			}
			return _NameCache[userId].Result;
		}

		public static async Task<bool> ActivateGameOverlayToUserAsync(CSteamID userId, string openTo = "steamid")
		{
			if (!OverlayEnabled)
			{
				return false;
			}
			SteamFriends.ActivateGameOverlayToUser(openTo, userId);
			await new AwaitCondition(() => OverlayActive);
			await new AwaitCondition(() => !OverlayActive);
			return true;
		}

		public static IEnumerable<CSteamID> GetFriends(EFriendFlags friendFlags = EFriendFlags.k_EFriendFlagAll)
		{
			int count = SteamFriends.GetFriendCount(friendFlags);
			for (int x = 0; x < count; x++)
			{
				yield return SteamFriends.GetFriendByIndex(x, friendFlags);
			}
		}

		public static void ClearCachedFriends()
		{
			_Friends = null;
		}

		public static bool IsFriend(CSteamID steamId, EFriendFlags flags = EFriendFlags.k_EFriendFlagImmediate)
		{
			if (Enabled)
			{
				return SteamFriends.HasFriend(steamId, flags);
			}
			return false;
		}

		public static IEnumerable<Group> GetGroups()
		{
			int count = SteamFriends.GetClanCount();
			for (int x = 0; x < count; x++)
			{
				yield return new Group(SteamFriends.GetClanByIndex(x));
			}
		}

		public static void ClearCachedGroups()
		{
			_Groups = null;
		}

		public static async Task<HashSet<CSteamID>> GetFollowedCreatorsAsync()
		{
			HashSet<CSteamID> output = new HashSet<CSteamID>();
			uint index = 0u;
			FriendsEnumerateFollowingList_t friendsEnumerateFollowingList_t;
			do
			{
				friendsEnumerateFollowingList_t = await SteamFriends.EnumerateFollowingList(index).ResultAsync<FriendsEnumerateFollowingList_t>();
				if (friendsEnumerateFollowingList_t.m_eResult.Failure())
				{
					break;
				}
				for (int i = 0; i < friendsEnumerateFollowingList_t.m_nResultsReturned; i++)
				{
					output.Add(friendsEnumerateFollowingList_t.m_rgSteamID[i]);
				}
				index += (uint)friendsEnumerateFollowingList_t.m_nResultsReturned;
			}
			while (index < friendsEnumerateFollowingList_t.m_nTotalResultCount);
			return output;
		}

		public static void ClearFriendsAndGroupCaches()
		{
			ClearCachedFriends();
			ClearCachedGroups();
		}
	}

	public static class Stats
	{
		private static readonly CallbackEvent<UserStatsReceived_t> _UserStatsReceivedCallback = new CallbackEvent<UserStatsReceived_t>();

		private static async Task<UserStatsReceived_t> _RequestCurrentStats()
		{
			if (!SteamUserStats.RequestCurrentStats())
			{
				return default(UserStatsReceived_t);
			}
			return await _UserStatsReceivedCallback.WaitAsync((UserStatsReceived_t stats) => stats.m_steamIDUser == SteamId && stats.m_nGameID == GameId.m_GameID);
		}

		private static bool _SetAchievement(string achievementName)
		{
			if (achievementName.IsNullOrEmpty())
			{
				return false;
			}
			if (!SteamUserStats.GetAchievement(achievementName, out var pbAchieved))
			{
				if (DEBUG.LogError())
				{
					Log.Error("Steam.Stats._SetAchievement() failed to find achievement with the name: [" + achievementName + "]. Make sure corresponding achievement exists in Steam Dashboard and that changes have been published.");
				}
				return false;
			}
			if (DEBUG.LogText())
			{
				Log.Text("Steam.Stats._SetAchievement(): Achievement: " + achievementName + ", Already Achieved = " + pbAchieved);
			}
			if (!pbAchieved && SteamUserStats.SetAchievement(achievementName))
			{
				AchievementData.SignalAchievementUnlock(achievementName);
			}
			return !pbAchieved;
		}

		private static bool _IndicateAchievementProgress(string achievementName, uint current, uint max)
		{
			if (achievementName.IsNullOrEmpty())
			{
				return false;
			}
			if (!SteamUserStats.GetAchievement(achievementName, out var pbAchieved))
			{
				if (DEBUG.LogError())
				{
					Log.Error("Steam.Stats._IndicateAchievementProgress() failed to find achievement with the name: [" + achievementName + "]. Make sure corresponding achievement exists in Steam Dashboard and that changes have been published.");
				}
				return false;
			}
			if (pbAchieved)
			{
				return false;
			}
			if (DEBUG.LogText())
			{
				Log.Text($"Steam.Stats._IndicateAchievementProgress(): Achievement: {achievementName} = ({current} / {max})");
			}
			return SteamUserStats.IndicateAchievementProgress(achievementName, current, max);
		}

		private static async Task<LeaderboardFindResult_t> _FindOrCreateLeaderboard(string leaderboardName, ELeaderboardSortMethod sortMethod = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, ELeaderboardDisplayType displayType = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds)
		{
			CallResultData<LeaderboardFindResult_t> callResultData = ((!Enabled) ? default(CallResultData<LeaderboardFindResult_t>) : (await SteamUserStats.FindOrCreateLeaderboard(leaderboardName, sortMethod, displayType).ResultAsync<LeaderboardFindResult_t>()));
			return callResultData;
		}

		private static async Task<LeaderboardScoresDownloaded_t> _DownloadLeaderboardEntries(string leaderboardName, ELeaderboardDataRequest dataRequest = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, int rangeBegin = 1, int rangeEnd = int.MaxValue)
		{
			LeaderboardFindResult_t leaderboardFindResult_t = await _FindOrCreateLeaderboard(leaderboardName);
			if (leaderboardFindResult_t.m_bLeaderboardFound == 0)
			{
				return default(LeaderboardScoresDownloaded_t);
			}
			return await SteamUserStats.DownloadLeaderboardEntries(leaderboardFindResult_t.m_hSteamLeaderboard, dataRequest, rangeBegin, rangeEnd).ResultAsync<LeaderboardScoresDownloaded_t>();
		}

		private static (LeaderboardEntry_t entry, int[] details) _GetDownloadedLeaderboardEntry(LeaderboardScoresDownloaded_t downloadedScores, int index, int detailsMax)
		{
			int[] array = new int[detailsMax];
			SteamUserStats.GetDownloadedLeaderboardEntry(downloadedScores.m_hSteamLeaderboardEntries, index, out var pLeaderboardEntry, array, detailsMax);
			return (pLeaderboardEntry, array);
		}

		public static async Task<bool> IndicateAchievementProgress(string achievementName, uint current, uint max)
		{
			bool flag = Enabled;
			if (flag)
			{
				flag = (await _RequestCurrentStats()).m_eResult.Success();
			}
			return flag && _IndicateAchievementProgress(achievementName, current, max);
		}

		public static async Task<bool> UnlockAchievement(string achievementName)
		{
			bool flag = Enabled;
			if (flag)
			{
				flag = (await _RequestCurrentStats()).m_eResult.Success();
			}
			return flag && _SetAchievement(achievementName) && SteamUserStats.StoreStats();
		}

		public static async Task<bool> UnlockAchievements(IEnumerable<string> achievementNames)
		{
			bool flag = !Enabled;
			if (!flag)
			{
				flag = (await _RequestCurrentStats()).m_eResult.Failure();
			}
			if (flag)
			{
				return false;
			}
			int num = 0;
			foreach (string achievementName in achievementNames)
			{
				if (_SetAchievement(achievementName))
				{
					num++;
				}
			}
			return num == 0 || SteamUserStats.StoreStats();
		}

		public static async Task<PoolKeepItemListHandle<string>> GetAllAchievementNames()
		{
			PoolKeepItemListHandle<string> output = Pools.UseKeepItemList<string>();
			bool flag = !Enabled;
			if (!flag)
			{
				flag = (await _RequestCurrentStats()).m_eResult.Failure();
			}
			if (flag)
			{
				return output;
			}
			uint numAchievements = SteamUserStats.GetNumAchievements();
			for (uint num = 0u; num < numAchievements; num++)
			{
				output.Add(SteamUserStats.GetAchievementName(num));
			}
			return output;
		}

		public static async Task ClearAllAchievements()
		{
			if (!Enabled)
			{
				return;
			}
			foreach (string item in await GetAllAchievementNames())
			{
				SteamUserStats.ClearAchievement(item);
			}
			SteamUserStats.StoreStats();
		}

		public static async IAsyncEnumerable<(LeaderboardEntry_t entry, int[] details)> GetLeaderboardEntries(string leaderboardName, ELeaderboardDataRequest dataRequest = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, int detailsMax = 64, int rangeBegin = 1, int rangeEnd = int.MaxValue)
		{
			LeaderboardScoresDownloaded_t downloadedEntries = await _DownloadLeaderboardEntries(leaderboardName, dataRequest, rangeBegin, rangeEnd);
			for (int x = 0; x < downloadedEntries.m_cEntryCount; x++)
			{
				yield return _GetDownloadedLeaderboardEntry(downloadedEntries, x, detailsMax);
			}
		}

		public static async Task<LeaderboardScoreUploaded_t> UploadLeaderboardScore(string leaderboardName, ELeaderboardUploadScoreMethod uploadType = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, int score = 0, params int[] details)
		{
			LeaderboardFindResult_t leaderboardFindResult_t = await _FindOrCreateLeaderboard(leaderboardName);
			if (leaderboardFindResult_t.m_bLeaderboardFound == 0)
			{
				return default(LeaderboardScoreUploaded_t);
			}
			return await SteamUserStats.UploadLeaderboardScore(leaderboardFindResult_t.m_hSteamLeaderboard, uploadType, score, details, details.Length).ResultAsync<LeaderboardScoreUploaded_t>();
		}
	}

	public static class RemoteStorage
	{
		public struct FileData
		{
			public string fileName;

			public int sizeInBytes;

			public static implicit operator bool(FileData data)
			{
				return !data.fileName.IsNullOrEmpty();
			}

			public static implicit operator string(FileData data)
			{
				return data.fileName;
			}

			public override string ToString()
			{
				return $"[{fileName}: {StringUtil.FormatLargeNumber(sizeInBytes)}B]";
			}
		}

		public struct QuotaData
		{
			public ulong total;

			public ulong available;

			public ulong used => total - available;

			public override string ToString()
			{
				return $"Used Quota: {used}, Available: {available}";
			}
		}

		public static IEnumerable<FileData> GetFiles()
		{
			int count = SteamRemoteStorage.GetFileCount();
			FileData fileData = default(FileData);
			for (int x = 0; x < count; x++)
			{
				fileData.fileName = SteamRemoteStorage.GetFileNameAndSize(x, out fileData.sizeInBytes);
				if ((bool)fileData)
				{
					yield return fileData;
				}
			}
		}

		public static QuotaData GetQuota()
		{
			QuotaData result = default(QuotaData);
			SteamRemoteStorage.GetQuota(out result.total, out result.available);
			return result;
		}

		public static void FileDelete(string filename)
		{
			SteamRemoteStorage.FileDelete(filename);
		}

		public static void DeleteAll()
		{
			foreach (FileData item in Pools.UseKeepItemList(GetFiles()))
			{
				FileDelete(item);
			}
		}
	}

	public static class Apps
	{
		private static string _InstallDirectory;

		public static string InstallDirectory => _InstallDirectory ?? (_InstallDirectory = GetInstallDir(AppId));

		public static string GetInstallDir(AppId_t appId)
		{
			SteamApps.GetAppInstallDir(appId, out var pchFolder, 2048u);
			return pchFolder;
		}

		public static bool HasDLC(AppId_t dlcAppId)
		{
			if (!(dlcAppId == AppId_t.Invalid))
			{
				return SteamApps.BIsDlcInstalled(dlcAppId);
			}
			return true;
		}

		public static bool HasDLC(uint dlcAppId)
		{
			return HasDLC((AppId_t)dlcAppId);
		}
	}

	public static class Utils
	{
		public static uint? GetServerRealTime()
		{
			if (!Enabled)
			{
				return null;
			}
			return SteamUtils.GetServerRealTime();
		}
	}

	public static readonly AppId_t AppId;

	public static readonly CGameID GameId;

	public const uint FILE_PATH_SIZE_BYTES = 2048u;

	public const uint URL_PATH_SIZE_BYTES = 2048u;

	public const EWorkshopFileType UGC_TYPE = EWorkshopFileType.k_EWorkshopFileTypeGameManagedItem;

	public const EUGCMatchingUGCType UGC_MATCH = EUGCMatchingUGCType.k_EUGCMatchingUGCType_GameManagedItems;

	public static Log.LevelFlags DEBUG;

	private static Callback<GameOverlayActivated_t> _GameOverlayActivated;

	private static bool _GameOverlayActive;

	private static string _UserId;

	private static bool? _Enabled;

	public static bool Enabled
	{
		get
		{
			if (_Enabled.HasValue)
			{
				return _Enabled.Value;
			}
			try
			{
				if ((_Enabled = SteamAPI.Init()) == true)
				{
					SteamClient.SetWarningMessageHook(_WarningMessageHook);
				}
			}
			catch (Exception message)
			{
				_Enabled = false;
				Debug.LogError(message);
			}
			return _Enabled.Value;
		}
	}

	public static bool IsMainApp => AppId.m_AppId == 1815570;

	public static bool LoggedOn
	{
		get
		{
			if (Enabled)
			{
				return SteamUser.BLoggedOn();
			}
			return false;
		}
	}

	public static bool CanUseWorkshop => LoggedOn;

	public static bool OverlayEnabled
	{
		get
		{
			if (Enabled)
			{
				return SteamUtils.IsOverlayEnabled();
			}
			return false;
		}
	}

	public static bool OverlayActive => _GameOverlayActive;

	public static string UserId => _UserId ?? (_UserId = (Enabled ? SteamId.m_SteamID.ToString() : null));

	public static string UserName => SteamFriends.GetPersonaName();

	public static CSteamID SteamId
	{
		get
		{
			if (!Enabled)
			{
				return default(CSteamID);
			}
			return SteamUser.GetSteamID();
		}
	}

	public static AccountID_t AccountId => SteamId.GetAccountID();

	private static void _WarningMessageHook(int severity, StringBuilder debugText)
	{
		Debug.LogWarning(debugText.ToString());
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void _Initialize()
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void _Initialize2()
	{
		if (Enabled)
		{
			new GameObject("SteamPump").AddComponent<SteamPump>();
		}
	}

	static Steam()
	{
		AppId = (AppId_t)1815570u;
		GameId = new CGameID(AppId);
		DEBUG = EnumUtil<Log.LevelFlags>.AllFlags;
		if (Enabled)
		{
			IOUtil.InsertUserIdIntoSavePath(UserId);
			_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(delegate(GameOverlayActivated_t active)
			{
				_GameOverlayActive = active.m_bActive == 1;
			});
			if (DEBUG.LogText())
			{
				Log.Text($"Steam Enabled: AppId = {AppId.m_AppId}, UserId = {UserId}, AccountId = {AccountId.m_AccountID}, InstallDir = {Apps.InstallDirectory}");
			}
			RemoteStorage.DeleteAll();
		}
	}

	public static CSteamID? ParseSteamId(string s)
	{
		if (!s.IsNumeric())
		{
			return null;
		}
		if (!ulong.TryParse(s, out var result))
		{
			return null;
		}
		return (CSteamID)result;
	}

	public static PublishedFileId_t? ParsePublishedFileId(string s)
	{
		if (s == null || s.Length < 10 || !s.IsNumeric())
		{
			return null;
		}
		if (!ulong.TryParse(s, out var result))
		{
			return null;
		}
		return (PublishedFileId_t)result;
	}

	public static bool HasDLC(uint dlcAppId)
	{
		if (Enabled)
		{
			return Apps.HasDLC(dlcAppId);
		}
		return false;
	}
}
