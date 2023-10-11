using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Diacritics.Extensions;
using ProtoBuf;
using Steamworks;
using UnityEngine;

[ProtoContract]
[ProtoInclude(5, typeof(ImageRef))]
[ProtoInclude(6, typeof(AudioRef))]
[ProtoInclude(7, typeof(DataRef<AbilityData>))]
[ProtoInclude(8, typeof(DataRef<ProjectileMediaData>))]
[ProtoInclude(9, typeof(DataRef<AdventureData>))]
[ProtoInclude(10, typeof(DataRef<EnemyData>))]
[ProtoInclude(11, typeof(DataRef<CharacterData>))]
[ProtoInclude(12, typeof(DataRef<AbilityDeckData>))]
[ProtoInclude(13, typeof(DataRef<EntityAudioData>))]
[ProtoInclude(14, typeof(DataRef<CombatMediaData>))]
[ProtoInclude(15, typeof(DataRef<MusicData>))]
[ProtoInclude(16, typeof(DataRef<LightingData>))]
[ProtoInclude(17, typeof(DataRef<TutorialData>))]
[ProtoInclude(18, typeof(DataRef<GameData>))]
[ProtoInclude(19, typeof(DataRef<BonusCardData>))]
[ProtoInclude(20, typeof(DataRef<LevelUpData>))]
[ProtoInclude(21, typeof(DataRef<MessageData>))]
[ProtoInclude(22, typeof(DataRef<ProceduralNodeData>))]
[ProtoInclude(23, typeof(DataRef<ProceduralNodePackData>))]
[ProtoInclude(24, typeof(DataRef<ProceduralGraphData>))]
[ProtoInclude(25, typeof(DataRef<ResourceDeckData>))]
[ProtoInclude(26, typeof(DataRef<AchievementData>))]
[ProtoInclude(100, typeof(DataRef<NodeGraphRef>))]
[ProtoInclude(101, typeof(DataRef<BlobData>))]
[ProtoInclude(102, typeof(DataRef<EBlobData>))]
[ProtoInclude(103, typeof(DataRef<DBlobData>))]
public abstract class ContentRef : IComparable<ContentRef>, IEquatable<ContentRef>
{
	private class CachedDependencyData
	{
		public uint cachedUpdateTime = uint.MaxValue;

		public HashSet<ContentRef> dependencies = new HashSet<ContentRef>();
	}

	[ProtoContract]
	public class DependencyData
	{
		[ProtoContract]
		public struct Data : IEquatable<Data>
		{
			[ProtoMember(1)]
			public readonly ContentRef contentRef;

			[ProtoMember(2)]
			public readonly ulong publishedFileId;

			public Data(ContentRef cRef)
			{
				contentRef = cRef;
				publishedFileId = cRef.publishedFileId;
			}

			public async Task<bool> NeedsDownloadAsync(Steam.Ugc.Query.Result result = default(Steam.Ugc.Query.Result))
			{
				bool flag = !contentRef;
				if (!flag)
				{
					bool flag2 = contentRef.createdByOtherUser;
					if (flag2)
					{
						flag2 = ((!result) ? (await Steam.Ugc.GetDetailsAsync(this)) : result).timeUpdated > contentRef.lastSubmitTime;
					}
					flag = flag2;
				}
				return flag;
			}

			public static implicit operator ContentRef(Data data)
			{
				return data.contentRef;
			}

			public static implicit operator ulong(Data data)
			{
				return data.publishedFileId;
			}

			public static implicit operator PublishedFileId_t(Data data)
			{
				return (PublishedFileId_t)data.publishedFileId;
			}

			public bool Equals(Data other)
			{
				return contentRef.Equals(other.contentRef);
			}

			public override bool Equals(object obj)
			{
				if (obj is Data)
				{
					return Equals((Data)obj);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return contentRef.GetHashCode();
			}

			public override string ToString()
			{
				return $"Key: {contentRef.key}, PublishedFileId: {publishedFileId}";
			}
		}

		[ProtoMember(1)]
		private List<Data> _data;

		[ProtoMember(2)]
		private bool _isValid;

		public List<Data> data => _data ?? (_data = new List<Data>());

		private DependencyData()
		{
		}

		public DependencyData(ContentRef cRef)
		{
			if (!(_isValid = cRef.isReadyToUpload))
			{
				return;
			}
			foreach (ContentRef dependency in cRef.GetDependencies())
			{
				if (!dependency.isResource)
				{
					if (!(_isValid = dependency.isReadyToUpload))
					{
						break;
					}
					data.Add(new Data(dependency));
				}
			}
		}

		public PoolKeepItemHashSetHandle<Data> GetDependenciesDeep()
		{
			PoolKeepItemHashSetHandle<Data> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<Data>();
			using PoolKeepItemHashSetHandle<ContentRef> output = Pools.UseKeepItemHashSet<ContentRef>();
			foreach (Data datum in data)
			{
				if (!poolKeepItemHashSetHandle.Add(datum) || !datum.contentRef || !datum.contentRef.canHaveDependencies)
				{
					continue;
				}
				foreach (ContentRef item in datum.contentRef.GetDependenciesDeep(output, (ContentRef cRef) => cRef.isResource).value)
				{
					poolKeepItemHashSetHandle.Add(new Data(item));
				}
			}
			return poolKeepItemHashSetHandle;
		}

		public async Task<PoolKeepItemDictionaryHandle<ulong, Steam.Ugc.Query.Result>> QueryDependenciesThatCouldRequireDownload()
		{
			PoolKeepItemDictionaryHandle<ulong, Steam.Ugc.Query.Result> output = Pools.UseKeepItemDictionary<ulong, Steam.Ugc.Query.Result>();
			using (Steam.Ugc.QuerySpecific query = Steam.Ugc.QuerySpecific.Create(from d in GetDependenciesDeep().AsEnumerable()
				where d.publishedFileId != 0L && (!d.contentRef || d.contentRef.createdByOtherUser)
				select (PublishedFileId_t)d.publishedFileId, null, null, matchAnyTag: true, returnMetaData: true, returnChildren: false, returnKeyValueTags: true, returnAdditionalPreviews: true))
			{
				if (query.count == 0)
				{
					return output;
				}
				await query.PageAllParallelAsync(delegate(IEnumerable<Steam.Ugc.Query.Result> results)
				{
					foreach (Steam.Ugc.Query.Result result in results)
					{
						output[result.id] = result;
					}
				});
			}
			return output;
		}

		public static implicit operator bool(DependencyData dependencyData)
		{
			return dependencyData?._isValid ?? false;
		}
	}

	[UIField]
	public class UploadOptions
	{
		public enum SetChildVisibility
		{
			Never,
			IfVisibilityIncreased,
			IfVisibilityDecreased,
			Always
		}

		public enum SetChildMature
		{
			Never,
			Additive,
			Override
		}

		public enum SetChildGroups
		{
			Never,
			IfGroupDefined,
			Always
		}

		[UIField(tooltip = "Determines who will be able to see content being uploaded in searches.")]
		[UIHeader("Visibility")]
		public ContentVisibility visibility;

		[UIField(tooltip = "Have visibility set above also apply to dependencies of content being uploaded.")]
		[UIHideIf("_hideDependencyOptions")]
		public SetChildVisibility setVisibilityForDependencies;

		[UIField(maxCount = 0, tooltip = "Please mark all mature flags that may apply to content being uploaded.\nPlease use the vanilla version of Aces and Adventures as a frame of reference.\n<i>Assuming that it would not have any mature content flags enabled.</i>")]
		[UIHeader("Mature Content Flags")]
		[UIMargin(24f, false)]
		public MatureContentFlags matureContentFlags;

		[UIField(tooltip = "Have mature content flags set above also apply to dependencies of content being uploaded.")]
		[UIHideIf("_hideDependencyOptions")]
		public SetChildMature setMatureForDependencies;

		[UIField(collapse = UICollapseType.Open, tooltip = "Enables content being uploaded to only be visible to members of Steam Groups listed below.")]
		[UIHideIf("_hideGroups")]
		[UIFieldCollectionItem(validateOnChange = true)]
		[UIHeader("Steam Group Exclusivity")]
		[UIMargin(24f, false)]
		public HashSet<Steam.Friends.Group> onlyVisibleToGroups;

		[UIField(tooltip = "Have Steam Groups listed above also apply to dependencies of content being uploaded.")]
		[UIHideIf("_hideGroupDependencyOptions")]
		public SetChildGroups setGroupsForDependencies = SetChildGroups.IfGroupDefined;

		public ContentRef contentRef;

		private bool _hideDependencyOptions => !contentRef.canHaveDependencies;

		private bool _hideGroups => Steam.Friends.CachedGroups.Count == 0;

		private bool _hideGroupDependencyOptions
		{
			get
			{
				if (!_hideDependencyOptions)
				{
					return _hideGroups;
				}
				return true;
			}
		}

		public UploadOptions(ContentRef contentRef)
		{
			this.contentRef = contentRef;
			visibility = contentRef.visibility;
			matureContentFlags = contentRef.mature;
			onlyVisibleToGroups = new HashSet<Steam.Friends.Group>();
			if (this.contentRef.groupIds.IsNullOrEmpty())
			{
				return;
			}
			foreach (Steam.Friends.Group item in from id in this.contentRef.groupIds
				select new Steam.Friends.Group((CSteamID)id) into g
				where g
				select g)
			{
				onlyVisibleToGroups.Add(item);
			}
		}

		public ContentRef CommitOptions()
		{
			contentRef._SetVisibilityAndSaveIfChanged(visibility);
			contentRef._SetMatureAndSaveIfChanged(this.matureContentFlags);
			ulong[] array = onlyVisibleToGroups.Select((Steam.Friends.Group g) => g.id.m_SteamID).ToArray();
			if (array.Length == 0)
			{
				array = null;
			}
			contentRef._SetGroupIdsSaveIfChanged(array);
			TreeNode<ContentRef> dependencyTree = contentRef.GetDependencyTree();
			PoolKeepItemListHandle<ContentRef> item = Pools.UseKeepItemList(from cRef in dependencyTree.DepthFirstEnum()
				where cRef.belongsToCurrentCreator
				select cRef);
			if (setMatureForDependencies != 0)
			{
				foreach (ContentRef item2 in item.value)
				{
					switch (setMatureForDependencies)
					{
					case SetChildMature.Additive:
						item2._SetMatureAndSaveIfChanged(EnumUtil.Add(item2.mature, this.matureContentFlags));
						break;
					case SetChildMature.Override:
						item2._SetMatureAndSaveIfChanged(this.matureContentFlags);
						break;
					}
				}
			}
			foreach (TreeNode<ContentRef> leafNode in dependencyTree.GetLeafNodes())
			{
				MatureContentFlags matureContentFlags = EnumUtil<MatureContentFlags>.NoFlags;
				for (TreeNode<ContentRef> treeNode = leafNode; treeNode != null; treeNode = treeNode.parent)
				{
					matureContentFlags |= treeNode.value.mature;
					if (treeNode.value.belongsToCurrentCreator)
					{
						treeNode.value._SetMatureAndSaveIfChanged(matureContentFlags);
					}
				}
			}
			if (setVisibilityForDependencies != 0)
			{
				foreach (ContentRef item3 in item.value)
				{
					switch (setVisibilityForDependencies)
					{
					case SetChildVisibility.Always:
						item3._SetVisibilityAndSaveIfChanged(visibility);
						break;
					case SetChildVisibility.IfVisibilityIncreased:
						item3._SetVisibilityAndSaveIfChanged(EnumUtil.Minimum(item3.visibility, visibility));
						break;
					case SetChildVisibility.IfVisibilityDecreased:
						item3._SetVisibilityAndSaveIfChanged(EnumUtil.Maximum(item3.visibility, visibility));
						break;
					}
				}
			}
			if (setGroupsForDependencies == SetChildGroups.Always || (setGroupsForDependencies == SetChildGroups.IfGroupDefined && !array.IsNullOrEmpty()))
			{
				foreach (ContentRef item4 in item.value)
				{
					item4._SetGroupIdsSaveIfChanged(array);
				}
			}
			Pools.Repool(ref item);
			return contentRef;
		}
	}

	[ProtoContract]
	public class MetaData
	{
		[ProtoMember(1)]
		public uint ugcDependencyCount;

		[ProtoMember(2)]
		public long ugcFileSizeInBytes;

		[ProtoMember(3, OverwriteList = true)]
		private Dictionary<string, DependencyData.Data> _quickDependencies;

		[ProtoMember(4)]
		public ushort version;

		public bool hasQuickDependencies => !_quickDependencies.IsNullOrEmpty();

		private MetaData()
		{
		}

		public MetaData(ContentRef contentRef, IEnumerable<Couple<string, ContentRef>> quickDependencies = null)
		{
			foreach (ContentRef item in from cRef in contentRef.GetDependenciesDeep().AsEnumerable()
				where !cRef.isResource && (bool)cRef
				select cRef)
			{
				ugcDependencyCount++;
				ugcFileSizeInBytes += item.fileSizeInBytes;
			}
			if (quickDependencies != null)
			{
				foreach (Couple<string, ContentRef> quickDependency in quickDependencies)
				{
					(_quickDependencies ?? (_quickDependencies = new Dictionary<string, DependencyData.Data>()))[quickDependency] = new DependencyData.Data(quickDependency);
				}
			}
			version = 12210;
		}

		public IEnumerable<KeyValuePair<string, DependencyData.Data>> GetQuickDependencies()
		{
			return _quickDependencies;
		}

		public DependencyData.Data? GetQuickDependency(string name)
		{
			if (_quickDependencies == null || !_quickDependencies.ContainsKey(name))
			{
				return null;
			}
			return _quickDependencies[name];
		}

		public ulong GetQuickDependencyPublishedFileId(string name)
		{
			DependencyData.Data? quickDependency = GetQuickDependency(name);
			if (!quickDependency.HasValue)
			{
				return 0uL;
			}
			return quickDependency.Value.publishedFileId;
		}

		public static implicit operator bool(MetaData metaData)
		{
			if (metaData != null)
			{
				return metaData.ugcDependencyCount != 0;
			}
			return false;
		}

		public static implicit operator uint?(MetaData metaData)
		{
			if (!metaData)
			{
				return null;
			}
			return metaData.ugcDependencyCount;
		}
	}

	public struct Key : IEquatable<Key>
	{
		public static readonly Key Null;

		public readonly string creatorId;

		public readonly uint fileId;

		public readonly byte categoryId;

		public Key(string creatorId, uint fileId, byte categoryId)
		{
			this.creatorId = creatorId;
			this.fileId = fileId;
			this.categoryId = categoryId;
		}

		public bool Equals(Key other)
		{
			if (fileId == other.fileId && categoryId == other.categoryId)
			{
				return creatorId == other.creatorId;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Key key)
			{
				return key.Equals(this);
			}
			return false;
		}

		public static bool operator ==(Key a, Key b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Key a, Key b)
		{
			return !a.Equals(b);
		}

		public static implicit operator bool(Key a)
		{
			if (a.fileId != 0)
			{
				return GetByKey<ContentRef>(a);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)fileId;
		}
	}

	private const string ContentFilename = "c";

	private const string ReferenceFilename = "r";

	private const string DependencyFilename = "d";

	private const string ReferenceFileExtension = ".bytes";

	private const string LOAD_JOB = "Load";

	private const string SAVE_JOB = "Save";

	private const string IMPORT_JOB = "Import";

	private static readonly HashSet<ContentRef> NULL_DEPENDENCIES;

	private static int UPLOAD_ATTEMPTS;

	private static int DOWNLOAD_ATTEMPTS;

	public const ContentInstallStatusFlags ALL_INSTALL_FLAGS = ContentInstallStatusFlags.Installed | ContentInstallStatusFlags.Update | ContentInstallStatusFlags.New;

	protected static Dictionary<Key, ContentRef> ContentReferences;

	private static Dictionary<Key, Dictionary<Couple<Type, Type>, Dictionary<string, object>>> GeneratedContent;

	private static Dictionary<Key, CachedDependencyData> CachedDependencies;

	private static HashSet<Key> UncommittedContent;

	private static Dictionary<Key, string> _SearchStrings;

	public static readonly Func<ContentRef, string> GetSearchStringFunc;

	private static int _ActiveSaveContentJobCount;

	protected static byte _DataKeyCategoryId;

	private static ContentRefDefaults _Defaults;

	protected static readonly string UserUID;

	protected static readonly string DeviceID;

	protected static readonly string CreatorId;

	private static TextBuilder _KeyBuilder;

	private static int _LoadAllReferencesCount;

	private static HashSet<string> _SearchStringsHash;

	private static uint _LastSubmitTimeVersion;

	private static int _SavingReferenceCount;

	private static readonly Dictionary<ulong, Key> _ContentKeysByPublishedId;

	private static readonly Dictionary<Key, HashSet<UnityEngine.Object>> _DirtyAssets;

	private static readonly HashSet<UnityEngine.Object> _DirtyAssetsToSaveWhenEnteringEditMode;

	public static bool TriggerStaticConstructor;

	private static Dictionary<Type, Func<bool, IEnumerable<ContentRef>>> _CachedSearchMethods;

	private static readonly string[] _ValidInstallFilenames;

	[ProtoMember(1)]
	private string _creatorUID;

	[ProtoMember(2, DataFormat = DataFormat.FixedSize)]
	private uint _fileId;

	[ProtoMember(3)]
	private uint _lastUpdateTime;

	private bool? _isMissingContentFile;

	private object _content;

	public static bool IsSavingContent => _ActiveSaveContentJobCount > 0;

	public static ContentRefDefaults Defaults
	{
		get
		{
			ContentRefDefaults contentRefDefaults = _Defaults;
			if (contentRefDefaults == null)
			{
				TextAsset textAsset = Resources.Load<TextAsset>(ContentRefDefaults.LoadFilepath);
				contentRefDefaults = (_Defaults = (((object)textAsset != null) ? IOUtil.FromByteArray<ContentRefDefaults>(textAsset.bytes) : new ContentRefDefaults()));
			}
			return contentRefDefaults;
		}
		private set
		{
			_Defaults = value;
		}
	}

	protected bool IsLoadingAllReferences => _LoadAllReferencesCount > 0;

	public static bool CreationEnabled => Steam.UserId != null;

	public static uint? TimeOverride { get; set; }

	public static uint Time => TimeOverride ?? DateTime.UtcNow.GetUnixEpoch();

	protected bool IsSavingReference => _SavingReferenceCount > 0;

	public static int Count => ContentReferences.Count;

	public static bool UGC => CreatorId != null;

	public abstract string categoryName { get; }

	public string creatorUID
	{
		get
		{
			return _creatorUID;
		}
		protected set
		{
			value = value ?? UserUID;
			_creatorUID = value;
		}
	}

	public CSteamID creatorSteamId
	{
		get
		{
			if (!isResource)
			{
				return (CSteamID)ulong.Parse(creatorUID);
			}
			return CSteamID.Nil;
		}
	}

	protected uint fileId
	{
		get
		{
			return _fileId;
		}
		private set
		{
			_fileId = value;
		}
	}

	private string filename
	{
		get
		{
			if (_fileId == 0)
			{
				return null;
			}
			return _fileId.ToString();
		}
	}

	public abstract string tags { get; set; }

	public ContentCreatorType creatorType
	{
		get
		{
			if (!isResource)
			{
				if (!belongsToCurrentCreator)
				{
					return ContentCreatorType.Others;
				}
				return ContentCreatorType.Yours;
			}
			return ContentCreatorType.Ours;
		}
	}

	public virtual byte maxNameLength => 32;

	public string detailedName => name + " (" + specificTypeFriendly + ")";

	protected string _downloadName
	{
		get
		{
			if (!this)
			{
				return specificType;
			}
			return detailedName;
		}
	}

	protected ContentRef r => ContentReferences[this];

	public uint lastUpdateTime
	{
		get
		{
			return r._lastUpdateTime;
		}
		private set
		{
			r._lastUpdateTime = value;
		}
	}

	private ulong publishedFileId
	{
		get
		{
			return 0uL;
		}
		set
		{
		}
	}

	private uint lastSubmitTime
	{
		get
		{
			return 0u;
		}
		set
		{
		}
	}

	private ContentVisibility visibility
	{
		get
		{
			return ContentVisibility.Public;
		}
		set
		{
		}
	}

	private MatureContentFlags mature
	{
		get
		{
			return ~(MatureContentFlags.MatureThemes | MatureContentFlags.Violence | MatureContentFlags.Blood | MatureContentFlags.Gore | MatureContentFlags.Language | MatureContentFlags.Drugs | MatureContentFlags.SexualContent | MatureContentFlags.Nudity);
		}
		set
		{
		}
	}

	private ulong[] groupIds
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public PublishedFileId_t workshopFileId => (PublishedFileId_t)publishedFileId;

	public bool hasPublishedFileId => workshopFileId.IsValid();

	private bool isReadyToUpload
	{
		get
		{
			if ((bool)this && committed && hasPublishedFileId && hasReferenceFile)
			{
				return File.Exists(loadPath);
			}
			return false;
		}
	}

	public KeyValuePair<string, string> workshopTag => new KeyValuePair<string, string>(type, categoryName);

	public long fileSizeInBytes => new FileInfo(savePath).Length;

	public Key key => this;

	protected abstract byte _keyCategoryId { get; }

	public bool isResource => creatorUID == null;

	public string savePath
	{
		get
		{
			if (!isResource)
			{
				return _ugcSavePath;
			}
			return _resourceSavePath;
		}
	}

	public string loadPath
	{
		get
		{
			if (!isResource)
			{
				return _ugcLoadPath;
			}
			return _resourceLoadPath;
		}
	}

	private string referenceSavePath => IOUtil.Combine(Path.GetDirectoryName(savePath), "r") + ".bytes";

	private string dependencySavePath => IOUtil.Combine(Path.GetDirectoryName(savePath), "d") + ".bytes";

	private bool hasReferenceFile => File.Exists(referenceSavePath);

	protected virtual string _previewFilePath => null;

	public string tempDirectoryPath => IOUtil.Combine(IOUtil.TempPath, GetFilepath(), creatorUID, filename);

	public bool usesAdditionalPreviews => true;

	public bool contentIsReady => content != null;

	protected object content
	{
		get
		{
			return r._content;
		}
		set
		{
			r._content = value;
		}
	}

	public abstract string type { get; }

	public abstract string specificType { get; }

	public virtual string specificTypeFriendly => specificType;

	public abstract Type dataType { get; }

	public abstract ContentRefType contentType { get; }

	protected virtual bool shouldCheckForUpdatedContentOnSave => false;

	public bool committed
	{
		get
		{
			return !UncommittedContent.Contains(this);
		}
		set
		{
			UncommittedContent.AddOrRemove(this, !value);
		}
	}

	public bool isTracked => ContentReferences.ContainsKey(this);

	public bool hasContent
	{
		get
		{
			if (isTracked)
			{
				if (!contentIsReady)
				{
					return !_IsMissingContentFile();
				}
				return true;
			}
			return false;
		}
	}

	public bool exists
	{
		get
		{
			if (isTracked && committed)
			{
				return hasContent;
			}
			return false;
		}
	}

	public bool hasSavedContent
	{
		get
		{
			if (isTracked)
			{
				return !_IsMissingContentFile();
			}
			return false;
		}
	}

	public bool saveCanBeOverriden
	{
		get
		{
			if (belongsToCurrentCreator)
			{
				return hasSavedContent;
			}
			return false;
		}
	}

	public bool canBeDeleted
	{
		get
		{
			if (isResource)
			{
				return belongsToCurrentCreator;
			}
			return true;
		}
	}

	public bool canUpload
	{
		get
		{
			if ((bool)this && saveCanBeOverriden)
			{
				return !isResource;
			}
			return false;
		}
	}

	public bool needsUpload
	{
		get
		{
			if (lastUpdateTime <= lastSubmitTime)
			{
				return _LastSubmitTimeVersion > lastSubmitTime;
			}
			return true;
		}
	}

	public bool belongsToCurrentCreator => creatorUID == CreatorId;

	public bool createdByOtherUser
	{
		get
		{
			if (!isResource)
			{
				return !belongsToCurrentCreator;
			}
			return false;
		}
	}

	public virtual bool isDataRef => false;

	public int sortIndex
	{
		get
		{
			if (!isDataRef)
			{
				if (!(this is ImageRef))
				{
					return 2;
				}
				return 1;
			}
			return 0;
		}
	}

	public virtual string friendlyName => filename.FriendlyFromLowerCaseUnderscore();

	public abstract string name { get; set; }

	public virtual bool canHaveDependencies => false;

	public virtual bool usesWorkshopMetaData => false;

	private string _resourceSavePath => IOUtil.Combine(_resourceSaveDir, filename, "c") + GetExtension();

	private string _resourceSaveDir => IOUtil.Combine(IOUtil.DevSavePath, "_C", GetFilepath());

	private string _resourceLoadPath => IOUtil.Combine("_C", GetFilepath(), filename, "c").ToResourcePath();

	private string _ugcSavePath => IOUtil.Combine(_ugcSaveDir, filename, "c") + GetExtension();

	private string _ugcSaveDir => IOUtil.Combine(IOUtil.UserContentPath, GetFilepath(), creatorUID);

	private string _ugcLoadPath => _ugcSavePath;

	private string _referenceLoadPathResource => Path.GetDirectoryName(_resourceLoadPath) + "/r";

	private string _referenceLoadPathUGC => IOUtil.Combine(Path.GetDirectoryName(_ugcLoadPath), "r") + ".bytes";

	protected string _assetPathContent => IOUtil.Combine("Assets/Resources", "_C", GetFilepath(), filename, "c").ToResourcePath() + GetExtension();

	private string _assetPathReference => Path.GetDirectoryName(_assetPathContent) + "/r.bytes";

	protected bool _lastUpdateTimeSpecified => IsSavingReference;

	protected bool _publishedFileIdSpecified => IsSavingReference;

	protected bool _lastSubmitTimeSpecified => IsSavingReference;

	protected bool _visibilitySpecified => IsSavingReference;

	protected bool _matureSpecified => IsSavingReference;

	protected bool _groupIdsSpecified => IsSavingReference;

	public static event Action<ContentRef> OnRequestLoad;

	static ContentRef()
	{
		NULL_DEPENDENCIES = new HashSet<ContentRef>();
		UPLOAD_ATTEMPTS = 3;
		DOWNLOAD_ATTEMPTS = 3;
		_DataKeyCategoryId = 64;
		_KeyBuilder = new TextBuilder(clearOnToString: true);
		_SearchStringsHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		_ContentKeysByPublishedId = new Dictionary<ulong, Key>();
		_DirtyAssets = new Dictionary<Key, HashSet<UnityEngine.Object>>();
		_DirtyAssetsToSaveWhenEnteringEditMode = new HashSet<UnityEngine.Object>();
		_CachedSearchMethods = new Dictionary<Type, Func<bool, IEnumerable<ContentRef>>>();
		_ValidInstallFilenames = new string[3] { "c", "r", "d" };
		ReflectionUtil.CacheMainThread();
		ProtoUtil.InitializeSurrogateTrigger = true;
		UnityEngine.Debug.Log("Game Version: " + IOUtil.VersionString);
		_InitCollections();
		GetSearchStringFunc = (ContentRef cRef) => cRef.GetSearchString();
		_LastSubmitTimeVersion = DateTime.SpecifyKind(new DateTime(2020, 5, 13, 18, 20, 0), DateTimeKind.Unspecified).GetUnixEpoch();
		DeviceID = SystemInfo.deviceUniqueIdentifier;
		UserUID = Steam.UserId ?? DeviceID;
		CreatorId = UserUID;
		IOUtil.DeleteTempFolder();
		if (Application.isPlaying)
		{
			Job.OnApplicationQuit(delegate
			{
				IOUtil.DeleteTempFolder();
			});
		}
		Directory.CreateDirectory(IOUtil.TempPath).Attributes |= FileAttributes.Hidden;
	}

	public static IEnumerable<ContentRef> SearchData(Type iDataContentType, bool mustBeCommitted = true)
	{
		if (!_CachedSearchMethods.ContainsKey(iDataContentType))
		{
			_CachedSearchMethods.Add(iDataContentType, typeof(DataRef<>).MakeGenericType(iDataContentType).CacheMethod<Func<bool, IEnumerable<ContentRef>>>("_SearchData", nonPublic: true, checkStaticMethods: true, searchOverloads: false, new Type[1] { typeof(bool) }, typeof(IEnumerable<ContentRef>)));
		}
		return _CachedSearchMethods[iDataContentType](mustBeCommitted);
	}

	public static IEnumerable<ContentRef> All()
	{
		return ContentReferences.Values.Where((ContentRef cRef) => cRef);
	}

	public static bool Equal(ContentRef a, ContentRef b)
	{
		if (a == null)
		{
			return b == null;
		}
		if (b == null)
		{
			return false;
		}
		bool flag = a.IsValid();
		bool flag2 = b.IsValid();
		if (flag && flag2)
		{
			return a.Equals(b);
		}
		return flag == flag2;
	}

	public static T GetByKey<T>(Key key) where T : ContentRef
	{
		if (!ContentReferences.ContainsKey(key))
		{
			return null;
		}
		return ContentReferences[key] as T;
	}

	public static T GetByPublishedId<T>(ulong publishedId) where T : ContentRef
	{
		if (!_ContentKeysByPublishedId.ContainsKey(publishedId))
		{
			return null;
		}
		return GetByKey<T>(_ContentKeysByPublishedId[publishedId]);
	}

	public static ContentInstallStatus GetPublishedIdInstallStatus(ulong publishedId, uint timeUpdated)
	{
		return GetByPublishedId<ContentRef>(publishedId).GetInstallStatus(timeUpdated);
	}

	private static bool _IsValidDownload(string installFilepath)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(installFilepath);
		if (directoryInfo.Exists)
		{
			return (from file in directoryInfo.EnumerateFiles()
				select Path.GetFileNameWithoutExtension(file.Name)).ToHash(StringComparer.OrdinalIgnoreCase).ContainsAll(_ValidInstallFilenames);
		}
		return false;
	}

	private static async Task<Steam.Ugc.InstallInfo> _DownloadAsync(PublishedFileId_t publishedFileId, Ref<bool> cancelRequested)
	{
		int attempts = 0;
		Steam.Ugc.InstallInfo installInfo;
		while (!(installInfo = await Steam.Ugc.DownloadAsync(publishedFileId)))
		{
			int num = attempts + 1;
			attempts = num;
			if (num >= DOWNLOAD_ATTEMPTS || cancelRequested.value)
			{
				break;
			}
			UIUtil.UpdateProcessMessage("Failed to download file. Retrying…");
		}
		if (Steam.DEBUG.LogError() && !installInfo)
		{
			Log.Error("ContentRef._DownloadAsync failed to download published file id: " + publishedFileId.m_PublishedFileId + ". Aborting Download Process.");
		}
		return installInfo;
	}

	private static async Task<Steam.Ugc.InstallInfo> _DownloadAsync(Steam.Ugc.Query.Result queryResult, Ref<bool> cancelRequested)
	{
		int attempts = 0;
		Steam.Ugc.InstallInfo installInfo;
		while (!(installInfo = await Steam.Ugc.DownloadAsync(queryResult)))
		{
			int num = attempts + 1;
			attempts = num;
			if (num >= DOWNLOAD_ATTEMPTS || cancelRequested.value)
			{
				break;
			}
			UIUtil.UpdateProcessMessage("Failed to download file. Retrying…");
		}
		if (Steam.DEBUG.LogError() && !installInfo)
		{
			Log.Error("ContentRef._DownloadAsync failed to download via URL: " + queryResult.GetAdditionalPreviewURL("D"));
		}
		return installInfo;
	}

	private static async Task<HashSet<DependencyData.Data>> _GetDependenciesWhichNeedDownloadAsync(ContentRef contentRef, Dictionary<ulong, Steam.Ugc.Query.Result> queries, HashSet<DependencyData.Data> dependenciesThatNeedDownload = null, HashSet<DependencyData.Data> processedDependencies = null, Action<PublishedFileId_t> onFileProgress = null)
	{
		dependenciesThatNeedDownload = dependenciesThatNeedDownload ?? new HashSet<DependencyData.Data>();
		processedDependencies = processedDependencies ?? new HashSet<DependencyData.Data>();
		foreach (ContentRef dependency in contentRef.GetDependencies())
		{
			if (dependency.isResource)
			{
				continue;
			}
			DependencyData.Data dependencyData = new DependencyData.Data(dependency);
			if (!processedDependencies.Add(dependencyData))
			{
				return dependenciesThatNeedDownload;
			}
			if (await dependencyData.NeedsDownloadAsync(queries.GetIfExists(dependencyData)))
			{
				dependenciesThatNeedDownload.Add(dependencyData);
				continue;
			}
			onFileProgress?.Invoke(dependencyData);
			if (dependencyData.contentRef.canHaveDependencies)
			{
				UIUtil.UpdateProcessMessage("Searching " + dependencyData.contentRef._downloadName + " Dependencies.");
				await _GetDependenciesWhichNeedDownloadAsync(dependencyData, queries, dependenciesThatNeedDownload, processedDependencies, onFileProgress);
			}
		}
		return dependenciesThatNeedDownload;
	}

	private static async Task<HashSet<Steam.Ugc.InstallInfo>> _DownloadAllAsync(PublishedFileId_t publishedFileId, Ref<bool> cancelRequested, Action<PublishedFileId_t> onFileProgress = null, HashSet<Steam.Ugc.InstallInfo> downloadedFileIds = null, HashSet<PublishedFileId_t> processedFileIds = null, HashSet<DependencyData.Data> processedDependencies = null, string fileName = null, Dictionary<ulong, Steam.Ugc.Query.Result> queries = null)
	{
		downloadedFileIds = downloadedFileIds ?? new HashSet<Steam.Ugc.InstallInfo>();
		if (fileName != null)
		{
			UIUtil.UpdateProcessMessage("Downloading " + fileName + " (" + (downloadedFileIds.Count + 1) + ")");
		}
		processedFileIds = processedFileIds ?? new HashSet<PublishedFileId_t>();
		processedDependencies = processedDependencies ?? new HashSet<DependencyData.Data>();
		if (!processedFileIds.Add(publishedFileId))
		{
			return downloadedFileIds;
		}
		if (onFileProgress != null)
		{
			onFileProgress(publishedFileId);
		}
		if (downloadedFileIds.Contains(publishedFileId))
		{
			return downloadedFileIds;
		}
		Steam.Ugc.InstallInfo downloadResult = await ((queries != null && queries.ContainsKey((ulong)publishedFileId)) ? _DownloadAsync(queries[(ulong)publishedFileId], cancelRequested) : _DownloadAsync(publishedFileId, cancelRequested));
		if (!downloadResult || cancelRequested.value)
		{
			return null;
		}
		string path = Path.Combine(downloadResult.filepath, "d.bytes");
		DependencyData dependencyData = IOUtil.LoadFromBytesBackup<DependencyData>(path);
		if (!dependencyData || !_IsValidDownload(downloadResult.filepath))
		{
			if (Steam.DEBUG.LogWarning())
			{
				Log.Warning("ContentRef.DownloadAsync retrieved invalid data for published file id: " + publishedFileId.m_PublishedFileId);
			}
			downloadResult.DeleteInstallFolder();
			return null;
		}
		PoolKeepItemDictionaryHandle<ulong, Steam.Ugc.Query.Result> needsDownloadQueries = await dependencyData.QueryDependenciesThatCouldRequireDownload();
		try
		{
			foreach (DependencyData.Data datum in dependencyData.data)
			{
				if (needsDownloadQueries.ContainsKey(datum) && !needsDownloadQueries[datum].versionIsValid)
				{
					if (Steam.DEBUG.LogError())
					{
						Log.Error("ContentRef.DownloadAsync found a dependency from a higher game version. Dependency version [" + needsDownloadQueries[datum].version + "]. Current version [" + (ushort)12210 + "]. Please check if update is available for game, otherwise dependency may have been uploaded from a Test Branch.");
					}
					downloadResult.DeleteInstallFolder();
					cancelRequested.value = true;
					return null;
				}
			}
			downloadedFileIds.Add(downloadResult);
			foreach (DependencyData.Data dependency in dependencyData.data)
			{
				if (await dependency.NeedsDownloadAsync(needsDownloadQueries.value.GetIfExists(dependency)))
				{
					if (await _DownloadAllAsync(dependency, cancelRequested, onFileProgress, downloadedFileIds, processedFileIds, processedDependencies, dependency.contentRef._downloadName, needsDownloadQueries) == null)
					{
						return null;
					}
				}
				else if ((await (await _GetDependenciesWhichNeedDownloadAsync(dependency, needsDownloadQueries, null, processedDependencies, onFileProgress)).Select((DependencyData.Data child) => _DownloadAllAsync(child, cancelRequested, onFileProgress, downloadedFileIds, processedFileIds, processedDependencies, child.contentRef._downloadName, needsDownloadQueries))).Any((HashSet<Steam.Ugc.InstallInfo> i) => i == null))
				{
					return null;
				}
			}
		}
		finally
		{
			if (needsDownloadQueries != null)
			{
				((IDisposable)needsDownloadQueries).Dispose();
			}
		}
		return downloadedFileIds;
	}

	public static async Task<ContentRef> DownloadAndInstallAsync(Steam.Ugc.Query.Result result)
	{
		result = await Steam.Ugc.GetDetailsAsync(result, returnMetaData: true, returnChildren: false, returnKeyValueTags: true, returnAdditionalPreviews: true);
		PublishedFileId_t publishedFileId = result;
		MetaData metaData = result.GetMetaData();
		Dictionary<ulong, Steam.Ugc.Query.Result> queries = new Dictionary<ulong, Steam.Ugc.Query.Result> { { publishedFileId.m_PublishedFileId, result } };
		Ref<bool> cancelRequested = new Ref<bool>(value: false);
		UIUtil.UpdateProcessOnCancel(delegate
		{
			cancelRequested.value = true;
		});
		HashSet<PublishedFileId_t> progressFileIds = new HashSet<PublishedFileId_t>();
		Action<PublishedFileId_t> onFileProgress = delegate(PublishedFileId_t id)
		{
			if (progressFileIds.Add(id))
			{
				UIUtil.UpdateProcessProgress((float)progressFileIds.Count / (float)((uint?)metaData).Value);
			}
		};
		HashSet<Steam.Ugc.InstallInfo> hashSet;
		while ((hashSet = await _DownloadAllAsync(publishedFileId, cancelRequested, metaData ? onFileProgress : null, null, null, null, null, queries)) == null && !cancelRequested.value)
		{
			UIUtil.UpdateProcessMessage("Failed to download all necessary files. Restarting download…");
		}
		if (hashSet == null)
		{
			if (Steam.DEBUG.LogError())
			{
				Log.Error("ContentRef.DownloadAndInstallAsync failed to download published file id: " + publishedFileId.m_PublishedFileId);
			}
			return null;
		}
		foreach (Steam.Ugc.InstallInfo item in hashSet)
		{
			ContentRef contentRef = IOUtil.LoadFromBytes<ContentRef>(Path.Combine(item.filepath, "r.bytes"));
			ContentReferences[contentRef] = contentRef;
			new FileInfo(Path.Combine(item.filepath, "c" + contentRef.GetExtension())).CopyToSafe(contentRef.savePath, overwrite: true);
			contentRef._isMissingContentFile = false;
			contentRef.lastUpdateTime = Time;
			contentRef.lastSubmitTime = item.lastUpdated;
			contentRef.committed = false;
			contentRef.committed = contentRef._SaveReference();
		}
		return GetByPublishedId<ContentRef>(publishedFileId.m_PublishedFileId);
	}

	public static async Task WaitForContentToBeReadyAsync()
	{
		await new AwaitCoroutine<object>(Job.WaitFrames(1));
		await new AwaitCondition(() => !Job.IsTrackedJobCreatedByTypeRunning<ContentRef>());
		await new AwaitCoroutine<object>(Job.WaitFrames(1));
	}

	private static HashSet<Key> _DeepDelete(ContentRef cRef, HashSet<Key> allDependencies = null)
	{
		if (cRef.canBeDeleted && (allDependencies ?? (allDependencies = new HashSet<Key>())).Add(cRef))
		{
			foreach (ContentRef dependency in cRef.GetDependencies())
			{
				_DeepDelete(dependency, allDependencies);
			}
			return allDependencies;
		}
		return allDependencies;
	}

	private static bool _DeepDelete(Key cRef, Dictionary<Key, HashSet<Key>> dependentsCache, HashSet<Key> allDependencies, HashSet<Key> deepDependants = null, Ref<bool> output = null)
	{
		if (output == null)
		{
			output = new Ref<bool>(value: true);
		}
		if (deepDependants == null)
		{
			deepDependants = new HashSet<Key>();
		}
		if (!allDependencies.Contains(cRef))
		{
			output.value = false;
		}
		if (output.value && dependentsCache.ContainsKey(cRef) && deepDependants.Add(cRef))
		{
			foreach (Key item in dependentsCache[cRef])
			{
				_DeepDelete(item, dependentsCache, allDependencies, deepDependants, output);
			}
		}
		return output.value;
	}

	public static HashSet<ContentRef> DeepDelete(ContentRef cRef, Dictionary<Key, HashSet<Key>> dependentsCache, out HashSet<Key> allDependencies)
	{
		HashSet<ContentRef> hashSet = new HashSet<ContentRef>();
		allDependencies = _DeepDelete(cRef);
		foreach (Key allDependency in allDependencies)
		{
			if (_DeepDelete(allDependency, dependentsCache, allDependencies))
			{
				hashSet.Add(GetByKey<ContentRef>(allDependency));
			}
		}
		return hashSet;
	}

	public static void UnloadAll()
	{
		foreach (ContentRef item in All())
		{
			item.Unload();
		}
		_InitCollections();
		foreach (Type item2 in from p in typeof(ContentRef).GetCustomAttributes<ProtoIncludeAttribute>()
			select p.KnownType)
		{
			item2.CacheMethod<Action>("ClearLoadedCache")();
		}
		Defaults = null;
	}

	public static void ClearSearchStringCache()
	{
		_SearchStrings.Clear();
	}

	private static void _InitCollections()
	{
		ContentReferences = new Dictionary<Key, ContentRef>();
		GeneratedContent = new Dictionary<Key, Dictionary<Couple<Type, Type>, Dictionary<string, object>>>();
		CachedDependencies = new Dictionary<Key, CachedDependencyData>();
		UncommittedContent = new HashSet<Key>();
		_SearchStrings = new Dictionary<Key, string>();
	}

	protected static void _LoadContentReferencesInFolder(string pathRelativeToContent, bool loadResources = true, bool loadUGC = true)
	{
		if (!loadResources && !loadUGC)
		{
			return;
		}
		_LoadAllReferencesCount++;
		if (loadResources)
		{
			(from o in Resources.LoadAll<TextAsset>(IOUtil.Combine("_C", pathRelativeToContent).ToResourcePath())
				where o.name == "r"
				select o into asset
				select IOUtil.FromByteArray<ContentRef>(asset.bytes)).EffectAll(delegate
			{
			});
		}
		if (loadUGC)
		{
			_LoadUGCReferencesInFolder(IOUtil.Combine(IOUtil.UserContentPath, pathRelativeToContent).ToOSPath());
		}
		_LoadAllReferencesCount--;
	}

	private static void _LoadUGCReferencesInFolder(string folderPath)
	{
		if (!Directory.Exists(folderPath))
		{
			return;
		}
		BlockingCollection<string> paths = new BlockingCollection<string>();
		Task task = Task.Run(delegate
		{
			try
			{
				foreach (string item in Directory.EnumerateFiles(folderPath, "r.bytes", SearchOption.AllDirectories))
				{
					paths.Add(item);
				}
			}
			finally
			{
				paths.CompleteAdding();
			}
		});
		Task task2 = Task.Run(delegate
		{
			foreach (string item2 in paths.GetConsumingEnumerable())
			{
				IOUtil.TryLoadFromBytes<ContentRef>(item2);
			}
		});
		Task.WaitAll(task, task2);
	}

	private static string _GetGeneratedContentJobName(Couple<Type, Type> ioPair, string name)
	{
		return "Generate<" + ioPair.a.Name + "," + ioPair.b.Name + ">:" + name;
	}

	private static void _DestroyGeneratedContent(UnityEngine.Object generatedObject)
	{
		Sprite sprite = generatedObject as Sprite;
		if (sprite != null && !sprite.texture.IsAsset())
		{
			UnityEngine.Object.Destroy(sprite.texture);
		}
		UnityEngine.Object.Destroy(generatedObject);
	}

	[Conditional("UNITY_EDITOR")]
	public static void SetDirty(Key setDirtyBy, UnityEngine.Object obj)
	{
		(_DirtyAssets.GetValueOrDefault(setDirtyBy) ?? (_DirtyAssets[setDirtyBy] = new HashSet<UnityEngine.Object>())).Add(obj);
	}

	[Conditional("UNITY_EDITOR")]
	public static void ClearDirty(Key setDirtyBy)
	{
		_DirtyAssets.Remove(setDirtyBy);
	}

	[Conditional("UNITY_EDITOR")]
	private static void _SaveDirtyAssets(Key savingKey)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void MarkAssetToBeSavedWhenExitingPlayMode(UnityEngine.Object asset)
	{
		_DirtyAssetsToSaveWhenEnteringEditMode.Add(asset);
	}

	public string GetFriendlyCategoryName()
	{
		return categoryName.FriendlyFromCamelOrPascalCase();
	}

	protected virtual Task _GeneratePreviewImageFile()
	{
		return Task.CompletedTask;
	}

	protected virtual async Task<PoolKeepItemListHandle<string>> _GenerateAdditionalPreviewImageFilesAsync()
	{
		PoolKeepItemListHandle<string> poolKeepItemListHandle = Pools.UseKeepItemList<string>();
		poolKeepItemListHandle.Add(IOUtil.ToPng(Path.Combine(tempDirectoryPath, "D.png"), ProtoUtil.ToByteArray(new SerializedDirectory(new DirectoryInfo(Path.GetDirectoryName(savePath)))), 1000000));
		return poolKeepItemListHandle;
	}

	public Job Import(string inputFile)
	{
		if ((bool)this)
		{
			Unload();
		}
		creatorUID = CreatorId;
		if (fileId == 0)
		{
			_GetUniqueFilename();
		}
		_Track();
		content = null;
		Directory.CreateDirectory(Path.GetDirectoryName(savePath));
		_MarkAsUncommitted();
		Job job = Job.Process(Import(inputFile, savePath, isResource), Department.Content).BeginTracking(r, "Import");
		job.Cleanup(delegate
		{
			_UpdateAsset(isImport: true);
		});
		return job;
	}

	public Job ReloadContent()
	{
		return Job.Process(_ReloadContent());
	}

	private IEnumerator _ReloadContent()
	{
		_UnloadContent();
		_UpdateAsset(isImport: false);
		Job retrieveContent = RetrieveContent();
		while (retrieveContent.isRunning)
		{
			yield return null;
		}
		yield return retrieveContent.result;
	}

	public Job RetrieveContent()
	{
		_Track();
		if (r.contentIsReady)
		{
			return Job.Result(() => r.content, Department.Content);
		}
		Job activeLoadJob = Job.GetJob(r, "Load");
		if (activeLoadJob != null)
		{
			return activeLoadJob;
		}
		Job.GetJob(r, "Import")?.Then();
		activeLoadJob = _BeginLoading().BeginTracking(r, "Load");
		activeLoadJob.OnStopRunning(delegate(Job j)
		{
			if (!j.wasStopped)
			{
				content = activeLoadJob.result;
				OnLoadValidation();
			}
		});
		return activeLoadJob;
	}

	public Job Save()
	{
		return Save(forceOverwrite: false);
	}

	public Job Save(bool forceOverwrite)
	{
		if (forceOverwrite)
		{
			_MarkAsUncommitted();
		}
		return _Save();
	}

	public void SaveReferenceChanges()
	{
		lastUpdateTime = Time;
		_SaveReference();
	}

	public HashSet<ContentRef> GetDependencies()
	{
		if (!canHaveDependencies)
		{
			return NULL_DEPENDENCIES;
		}
		if (!CachedDependencies.ContainsKey(this))
		{
			CachedDependencies.Add(this, new CachedDependencyData());
		}
		if (CachedDependencies[this].cachedUpdateTime != lastUpdateTime)
		{
			CachedDependencies[this].cachedUpdateTime = lastUpdateTime;
			CachedDependencies[this].dependencies = _CalculateDependencies();
		}
		return CachedDependencies[this].dependencies;
	}

	public TreeNode<ContentRef> GetDependencyTree(TreeNode<ContentRef> dependencyTreeNode = null)
	{
		dependencyTreeNode = dependencyTreeNode ?? new TreeNode<ContentRef>(this);
		foreach (ContentRef dependency in GetDependencies())
		{
			if (!dependencyTreeNode.ContainsParent(dependency))
			{
				dependency.GetDependencyTree(dependencyTreeNode.AddChild(dependency));
			}
		}
		return dependencyTreeNode;
	}

	public PoolKeepItemHashSetHandle<ContentRef> GetDependenciesDeep(PoolKeepItemHashSetHandle<ContentRef> output = null, Func<ContentRef, bool> stopRecursionAt = null)
	{
		output = output ?? Pools.UseKeepItemHashSet<ContentRef>();
		if (output.Add(this) && canHaveDependencies && (stopRecursionAt == null || !stopRecursionAt(this)))
		{
			foreach (ContentRef dependency in GetDependencies())
			{
				dependency.GetDependenciesDeep(output, stopRecursionAt);
			}
			return output;
		}
		return output;
	}

	public void ClearCachedDependencies()
	{
		CachedDependencies.Remove(this);
	}

	public IEnumerable<DataRef<C>> GetDependants<C>() where C : IDataContent
	{
		return from dRef in DataRef<C>.Search()
			where dRef.GetDependencies().Contains(this)
			select dRef;
	}

	public void Delete()
	{
		string directoryName = Path.GetDirectoryName((!isResource) ? savePath : _assetPathContent);
		UnityEngine.Debug.Log("_Delete [" + directoryName + "]");
		_OnDelete();
		if (!isResource)
		{
			Directory.Delete(directoryName, recursive: true);
		}
		_UnloadContent();
		_Untrack();
		_isMissingContentFile = null;
	}

	public Key GetIdentifier()
	{
		if (!this.IsValid())
		{
			return Key.Null;
		}
		return this;
	}

	private string _GetSearchString()
	{
		if (!friendlyName.IsNullOrEmpty())
		{
			string[] array = friendlyName.Split(FuzzySearch.MatchSplit, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in array)
			{
				_SearchStringsHash.Add(item);
			}
		}
		if (!tags.IsNullOrEmpty())
		{
			string[] array = tags.Split(FuzzySearch.MatchSplit, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item2 in array)
			{
				_SearchStringsHash.Add(item2);
			}
		}
		List<string> automatedTags = GetAutomatedTags();
		if (automatedTags != null)
		{
			foreach (string item4 in automatedTags)
			{
				string[] array = item4.Split(FuzzySearch.MatchSplit, StringSplitOptions.RemoveEmptyEntries);
				foreach (string item3 in array)
				{
					_SearchStringsHash.Add(item3);
				}
			}
		}
		using PoolHandle<StringBuilder> poolHandle = Pools.Use<StringBuilder>();
		foreach (string item5 in _SearchStringsHash)
		{
			poolHandle.value.Append(item5);
			poolHandle.value.Append(" ");
		}
		if (poolHandle.value.Length > 0)
		{
			poolHandle.value.Remove(poolHandle.value.Length - 1, 1);
		}
		_SearchStringsHash.Clear();
		string text = poolHandle.value.ToString();
		if (LocalizationUtil.UsingDiacritics)
		{
			string text2 = text.RemoveDiacritics();
			if (text2 != text)
			{
				text = text + " " + text2;
			}
		}
		return text.ToTagString();
	}

	public string GetSearchString()
	{
		return _SearchStrings.GetValueOrDefault(this) ?? (_SearchStrings[this] = _GetSearchString());
	}

	private async Task<SubmitItemUpdateResult_t> _UploadAsync(List<string> additionalPreviewPaths = null)
	{
		return await Steam.Ugc.UploadAsync(name, new FileInfo(savePath).Directory.FullName, publishedFileId, null, GetDescription(), GetSearchString().Split(FuzzySearch.MatchSplit), _previewFilePath, new KeyValuePair<string, string>[1] { workshopTag }.Concat(_WorkshopTags()).Concat(_GetAdditionalWorkshopTags()), _GetWorkshopMetaData(), EWorkshopFileType.k_EWorkshopFileTypeGameManagedItem, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic, null, additionalPreviewPaths.IsNullOrEmpty() ? null : additionalPreviewPaths, _GetWorkshopTrailerVideoId());
	}

	public async Task<bool> UploadAsync()
	{
		if (!ProfileManager.prefs.steam.GetAuthorPublishedFileId().HasValue)
		{
			await new WaitForEndOfFrame();
			UIUtil.UpdateProcessMessage("Creating Author File");
			if (Steam.DEBUG.LogText())
			{
				Log.Text("Creating Author File For: " + Steam.UserId, appendToUserLog: false);
			}
			PublishedFileId_t publishedFileId_t = await Steam.Ugc.CreateAuthorFile();
			if (publishedFileId_t == PublishedFileId_t.Invalid)
			{
				if (Steam.DEBUG.LogError())
				{
					Log.Error("ContentRef.UploadAsync failed to create author file.");
				}
				return false;
			}
			ProfileManager.prefs.steam.SetAuthorPublishedFileId(publishedFileId_t);
			if (Steam.DEBUG.LogText())
			{
				Log.Text("Created Author File: " + publishedFileId_t.m_PublishedFileId, appendToUserLog: false);
			}
		}
		using (PoolKeepItemListHandle<ContentRef> allDependencies = Pools.UseKeepItemList((from cRef in GetDependencyTree().DepthFirstEnum()
			where cRef.canUpload && cRef.needsUpload
			select cRef).Distinct().Reverse()))
		{
			if (allDependencies.Count > 0)
			{
				await new WaitForEndOfFrame();
				allDependencies.value.AddUnique(this);
				WebRequestTextureCache.ClearCache();
			}
			foreach (ContentRef dependency2 in allDependencies.value)
			{
				if (dependency2.hasPublishedFileId)
				{
					continue;
				}
				UIUtil.UpdateProcessMessage("Creating Workshop Item For " + dependency2.detailedName);
				int attempts2 = 0;
				CreateItemResult_t createItemResult_t;
				while (true)
				{
					CreateItemResult_t createItemResult_t2 = (createItemResult_t = await Steam.Ugc.CreateItemAsync());
					if (!createItemResult_t2.m_eResult.Failure())
					{
						break;
					}
					int num = attempts2 + 1;
					attempts2 = num;
					if (num >= UPLOAD_ATTEMPTS)
					{
						break;
					}
					UIUtil.UpdateProcessMessage("Failed to create new item for " + dependency2.detailedName + ". Retrying…");
				}
				if (createItemResult_t.m_eResult.Failure())
				{
					if (Steam.DEBUG.LogError())
					{
						Log.Error("Steam.Ugc.CreateItem failed to retrieve publishedFileId after " + attempts2 + " attempts. Aborting upload of " + dependency2.detailedName);
					}
					return false;
				}
				if (Steam.DEBUG.LogText())
				{
					Log.Text("Created new workshop item id for " + dependency2.detailedName);
				}
				dependency2.publishedFileId = createItemResult_t.m_nPublishedFileId.m_PublishedFileId;
				dependency2._SaveReference();
			}
			int count = 0;
			foreach (ContentRef dependency2 in allDependencies.value)
			{
				UIUtil.UpdateProcessMessage("Submitting Update For " + dependency2.detailedName);
				int num = count + 1;
				count = num;
				UIUtil.UpdateProcessProgress((float)num / (float)allDependencies.Count);
				int attempts2 = 0;
				dependency2._CleanDirectory();
				DependencyData dependencyData = new DependencyData(dependency2);
				if (!dependencyData)
				{
					if (Steam.DEBUG.LogError())
					{
						Log.Error("ContentRef.UploadAsync has invalid dependency data for [" + dependency2.detailedName + "]. This means a dependency has failed to be assigned a published file id (This should only occur if you have altered the Content Folder outside the program).");
					}
					return false;
				}
				if (!(await dependency2._IsContentValidForUpload()))
				{
					if (Steam.DEBUG.LogError())
					{
						Log.Error("ContentRef.UploadAsync found invalid content file for [" + dependency2.detailedName + "]. This should only happen when content files have been manually altered outside the program OR when data has become invalid due to new saving rules.");
					}
					return false;
				}
				IOUtil.WriteBytes(dependencyData, dependency2.dependencySavePath);
				await dependency2._GeneratePreviewImageFile();
				PoolKeepItemListHandle<string> additionalPreviews = await dependency2._GenerateAdditionalPreviewImageFilesAsync();
				additionalPreviews.value.RemoveAll(string.IsNullOrEmpty);
				SubmitItemUpdateResult_t submitItemUpdateResult_t;
				while (true)
				{
					SubmitItemUpdateResult_t submitItemUpdateResult_t2 = (submitItemUpdateResult_t = await dependency2._UploadAsync(additionalPreviews.value));
					if (!submitItemUpdateResult_t2.m_eResult.Failure())
					{
						break;
					}
					num = attempts2 + 1;
					attempts2 = num;
					if (num >= UPLOAD_ATTEMPTS)
					{
						break;
					}
					UIUtil.UpdateProcessMessage("Failed to upload " + dependency2.detailedName + ". Retrying…");
				}
				File.Delete(dependency2.dependencySavePath);
				if (dependency2._previewFilePath != dependency2.savePath)
				{
					IOUtil.DeleteSafe(dependency2._previewFilePath);
				}
				foreach (string item in additionalPreviews)
				{
					IOUtil.DeleteSafe(item);
				}
				if (submitItemUpdateResult_t.m_eResult.Failure())
				{
					if (Steam.DEBUG.LogError())
					{
						Log.Error("Steam.Ugc.Upload failed after " + attempts2 + " attempts. Aborting upload of " + dependency2.detailedName);
					}
					return false;
				}
				dependency2.lastSubmitTime = Time;
				dependency2._SaveReference();
				if (Steam.DEBUG.LogText())
				{
					Log.Text("Successfully uploaded " + dependency2.detailedName);
				}
			}
		}
		if (Steam.DEBUG.LogText())
		{
			Log.Text("Finished Uploading " + detailedName);
		}
		return true;
	}

	protected string GetFilepath()
	{
		return IOUtil.Combine(type, categoryName);
	}

	protected abstract string GetExtension();

	protected abstract IEnumerator Import(string inputFile, string contentSavePath, bool isResource);

	protected abstract IEnumerator LoadResource();

	protected abstract IEnumerator LoadUGC();

	protected virtual void OnLoadValidation()
	{
	}

	protected abstract IEnumerator SaveContent();

	protected virtual void OnSaveRefValidation()
	{
	}

	protected virtual string GetSaveErrorMessage()
	{
		return null;
	}

	protected virtual void _OnDelete()
	{
	}

	protected virtual List<string> GetAutomatedTags()
	{
		return new List<string>();
	}

	protected virtual HashSet<ContentRef> _CalculateDependencies()
	{
		return NULL_DEPENDENCIES;
	}

	public virtual byte[] GetContentBytesInMemory()
	{
		return new byte[0];
	}

	public byte[] GetContentBytesOnDisk()
	{
		if (_IsMissingContentFile())
		{
			return new byte[0];
		}
		if (!isResource)
		{
			return File.ReadAllBytes(loadPath);
		}
		return Resources.Load<TextAsset>(loadPath).bytes;
	}

	public virtual bool ContentInMemoryDistinctFromDisk()
	{
		return false;
	}

	public virtual void SaveFromUI(object dataToSave, SaveNameConfirmation confirmation, Action onSaveSuccessful, Action onSaveFailed = null, Transform parent = null)
	{
		throw new NotImplementedException();
	}

	public virtual Job SetDataAndSave(object dataToSave)
	{
		throw new NotImplementedException();
	}

	public virtual void SaveFromUIWithoutValidation(object dataToSave, Action onSaveSuccessful = null)
	{
		throw new NotImplementedException();
	}

	public virtual string GetDescription()
	{
		return null;
	}

	private IEnumerable<KeyValuePair<string, string>> _WorkshopTags()
	{
		if (visibility != EnumUtil<ContentVisibility>.Min)
		{
			yield return new KeyValuePair<string, string>("V", ((int)visibility).ToString());
		}
		if (mature != EnumUtil<MatureContentFlags>.NoFlags)
		{
			yield return new KeyValuePair<string, string>("M", ((int)mature).ToString());
		}
		if (!groupIds.IsNullOrEmpty())
		{
			ulong[] array = groupIds;
			foreach (ulong num in array)
			{
				yield return new KeyValuePair<string, string>("G", num.ToString());
			}
		}
	}

	protected virtual IEnumerable<KeyValuePair<string, string>> _GetAdditionalWorkshopTags()
	{
		yield break;
	}

	protected virtual byte[] _GetWorkshopMetaData()
	{
		return null;
	}

	protected virtual string _GetWorkshopTrailerVideoId()
	{
		return "";
	}

	protected abstract Task<bool> _IsContentValidForUpload();

	public abstract IEnumerable<ContentRef> SearchSimilar();

	protected abstract void _LoadSimilarReferences();

	protected C _LoadReferenceFile<C>() where C : ContentRef
	{
		if (isResource)
		{
			if (!ReflectionUtil.OnMainThread)
			{
				return null;
			}
			TextAsset textAsset = Resources.Load<TextAsset>(_referenceLoadPathResource);
			if (!textAsset)
			{
				return null;
			}
			return IOUtil.FromByteArray<C>(textAsset.bytes);
		}
		return IOUtil.TryLoadFromBytes<C>(_referenceLoadPathUGC);
	}

	[Conditional("UNITY_EDITOR")]
	protected void RefreshAsset(bool isImport)
	{
	}

	protected void _ClearDependencyAndSearchStringCaches()
	{
		ClearCachedDependencies();
		_SearchStrings.Remove(this);
	}

	protected void _MarkAsUncommitted()
	{
		if (UncommittedContent.Add(this))
		{
			_ClearDependencyAndSearchStringCaches();
		}
	}

	protected void _MarkAsCommitted()
	{
		if (UncommittedContent.Remove(this))
		{
			_ClearDependencyAndSearchStringCaches();
		}
	}

	private Job _BeginLoading()
	{
		if (ContentRef.OnRequestLoad != null)
		{
			ContentRef.OnRequestLoad(this);
		}
		return Job.Process(isResource ? LoadResource() : LoadUGC(), Department.Content);
	}

	private Job _Save()
	{
		_Track();
		Job job = Job.GetJob(r, "Save");
		if (job != null)
		{
			return job;
		}
		if (committed)
		{
			return Job.Process(Job.EmptyEnumerator(), Department.Content);
		}
		Job job2 = ((!contentIsReady) ? RetrieveContent() : Job.Process(Job.EmptyEnumerator(), Department.Content)).Then().DoProcess(_UpdateIfNeeded()).Then()
			.DoJob(_SaveReferenceJob())
			.Then()
			.DoJob(_SaveContent());
		job2.BeginTracking(r, "Save");
		return job2;
	}

	private Job _SaveContent()
	{
		_ActiveSaveContentJobCount++;
		Job job = Job.Process(SaveContent());
		Action action = delegate
		{
			_ActiveSaveContentJobCount--;
			r._isMissingContentFile = false;
		};
		return job.Cleanup(action);
	}

	private Job _SaveReferenceJob(bool doSaveRefValidation = true)
	{
		return Job.Action(delegate
		{
			_SaveReferenceWithValidation(doSaveRefValidation);
		});
	}

	private void _SaveReferenceWithValidation(bool doSaveRefValidation)
	{
		if (doSaveRefValidation)
		{
			OnSaveRefValidation();
		}
		if (_SaveReference())
		{
			_MarkAsCommitted();
		}
	}

	private void _Update()
	{
		lastUpdateTime = Time;
	}

	private void _UpdateAsset(bool isImport)
	{
		if (belongsToCurrentCreator)
		{
			_Update();
		}
	}

	private IEnumerator _UpdateIfNeeded()
	{
		if (!shouldCheckForUpdatedContentOnSave || !belongsToCurrentCreator)
		{
			yield break;
		}
		if (!File.Exists(loadPath))
		{
			_Update();
			yield break;
		}
		yield return ToBackgroundThread.Create();
		if (ContentInMemoryDistinctFromDisk())
		{
			_Update();
		}
	}

	private bool _SaveReference()
	{
		_SavingReferenceCount++;
		IOUtil.WriteToFile(r, referenceSavePath);
		_SavingReferenceCount--;
		return true;
	}

	protected void _GetContent<C>(Action<C> onContentRetrieved, bool forceImmediate)
	{
		if (contentIsReady)
		{
			onContentRetrieved((C)content);
			return;
		}
		Job job = RetrieveContent().Then().ResultAction(onContentRetrieved);
		if (forceImmediate)
		{
			job.ForceCompletion();
		}
	}

	protected Job _GenerateFromContent<C, O>(Func<C, IEnumerator> generateLogic, string name)
	{
		return _GenerateFromCommon<C, O>(generateLogic, name, () => RetrieveContent().Then().ResultProcess(generateLogic));
	}

	protected Job _GenerateFromRef<R, O>(Func<R, IEnumerator> generateLogic, string name) where R : ContentRef
	{
		return _GenerateFromCommon<R, O>(generateLogic, name, () => Job.Process(generateLogic(this as R), Department.Content));
	}

	private Job _GenerateFromCommon<I, O>(Func<I, IEnumerator> generateLogic, string name, Func<Job> createGenerationJob)
	{
		Couple<Type, Type> ioPair = new Couple<Type, Type>(typeof(I), typeof(O));
		if (_GeneratedContentExists(ioPair, name))
		{
			return Job.Result(() => (O)GeneratedContent[this][ioPair][name], Department.Content);
		}
		string text = _GetGeneratedContentJobName(ioPair, name);
		Job job = Job.GetJob(r, text);
		if (job != null)
		{
			return job;
		}
		Job job2 = createGenerationJob().BeginTracking(r, text);
		job2.Immediately().ResultAction(delegate(O obj)
		{
			if (obj != null)
			{
				if (!GeneratedContent.ContainsKey(this))
				{
					GeneratedContent.Add(this, new Dictionary<Couple<Type, Type>, Dictionary<string, object>>());
				}
				if (!GeneratedContent[this].ContainsKey(ioPair))
				{
					GeneratedContent[this].Add(ioPair, new Dictionary<string, object>());
				}
				if (!GeneratedContent[this][ioPair].ContainsKey(name))
				{
					GeneratedContent[this][ioPair].Add(name, null);
				}
				GeneratedContent[this][ioPair][name] = obj;
			}
		});
		return job2;
	}

	protected void _GetGeneratedFromContent<C, O>(Func<C, IEnumerator> generateLogic, string name, Action<O> onGeneratedContentRetrieved, bool forceImmediate)
	{
		Couple<Type, Type> ioPair = new Couple<Type, Type>(typeof(C), typeof(O));
		if (_GeneratedContentExists(ioPair, name))
		{
			onGeneratedContentRetrieved((O)GeneratedContent[this][ioPair][name]);
			return;
		}
		Job job = _GenerateFromContent<C, O>(generateLogic, name).Then().ResultAction(delegate(O obj)
		{
			if (obj != null)
			{
				onGeneratedContentRetrieved(obj);
			}
		});
		if (forceImmediate)
		{
			job.ForceCompletion();
		}
	}

	protected O _GetGeneratedContent<C, O>(string name)
	{
		Couple<Type, Type> ioPair = new Couple<Type, Type>(typeof(C), typeof(O));
		if (!_GeneratedContentExists(ioPair, name))
		{
			return default(O);
		}
		return (O)GeneratedContent[this][ioPair][name];
	}

	protected void _GetGeneratedFromRef<R, O>(Func<R, IEnumerator> generateLogic, string name, Action<O> onGeneratedContentRetrieved, bool forceImmediate) where R : ContentRef
	{
		Couple<Type, Type> ioPair = new Couple<Type, Type>(typeof(R), typeof(O));
		if (_GeneratedContentExists(ioPair, name))
		{
			onGeneratedContentRetrieved((O)GeneratedContent[this][ioPair][name]);
			return;
		}
		Job job = _GenerateFromRef<R, O>(generateLogic, name).Then().ResultAction(delegate(O obj)
		{
			if (obj != null)
			{
				onGeneratedContentRetrieved(obj);
			}
		});
		if (forceImmediate)
		{
			job.ForceCompletion();
		}
	}

	private bool _GeneratedContentExists(Couple<Type, Type> ioPair, string name)
	{
		bool flag = GeneratedContent.ContainsKey(this) && GeneratedContent[this].ContainsKey(ioPair) && GeneratedContent[this][ioPair].ContainsKey(name);
		if (flag && GeneratedContent[this][ioPair][name] == null)
		{
			flag = !GeneratedContent[this][ioPair].Remove(name);
		}
		return flag;
	}

	public virtual void ClearGeneratedContent(bool unload = false)
	{
		if (unload)
		{
			UnloadGeneratedContent<UnityEngine.Object>();
		}
		GeneratedContent.Remove(this);
	}

	public virtual void UnloadGeneratedContent<C>() where C : UnityEngine.Object
	{
		if (!GeneratedContent.ContainsKey(this))
		{
			return;
		}
		foreach (KeyValuePair<Couple<Type, Type>, Dictionary<string, object>> item in GeneratedContent[this])
		{
			Couple<Type, Type> ioPair = item.Key;
			foreach (KeyValuePair<string, object> item2 in item.Value)
			{
				C val = item2.Value as C;
				Job.GetJob(r, _GetGeneratedContentJobName(ioPair, item2.Key))?.KillAndStopTracking();
				if ((bool)(UnityEngine.Object)val)
				{
					_DestroyGeneratedContent(val);
				}
			}
		}
		GeneratedContent[this].Values.EffectAll(delegate(Dictionary<string, object> d)
		{
			d.RemoveKeys((string s) => d[s] is C);
		});
	}

	protected void _Track()
	{
		if (fileId == 0)
		{
			return;
		}
		lock (ContentReferences)
		{
			if (ContentReferences.ContainsKey(this))
			{
				return;
			}
			ContentReferences[this] = this;
		}
		_ = IsLoadingAllReferences;
	}

	[Conditional("UNITY_EDITOR")]
	private void _OnFirstTracked()
	{
		ContentRef contentRef = _LoadReferenceFile<ContentRef>();
		if (contentRef == null)
		{
			if (isResource)
			{
				Job.Action(delegate
				{
					_DoFirstTrackedLogic(_LoadReferenceFile<ContentRef>());
				}, Department.Content);
			}
			else if (hasReferenceFile)
			{
				_SaveReference();
			}
		}
		else
		{
			_DoFirstTrackedLogic(contentRef);
		}
	}

	private void _DoFirstTrackedLogic(ContentRef referenceFile)
	{
		if (referenceFile != null)
		{
			lastUpdateTime = referenceFile._lastUpdateTime;
			_OnFirstTrackedUnique(referenceFile);
		}
	}

	protected virtual void _OnFirstTrackedUnique(ContentRef loadedReferenceFile)
	{
	}

	private void _Untrack()
	{
		GeneratedContent.Remove(this);
		CachedDependencies.Remove(this);
		ContentReferences.Remove(this);
		UncommittedContent.Remove(this);
	}

	private void _UnloadContent()
	{
		content = null;
	}

	public bool Unload()
	{
		if (!contentIsReady)
		{
			return false;
		}
		ClearGeneratedContent(unload: true);
		if (isResource)
		{
			Resources.UnloadAsset(content as UnityEngine.Object);
		}
		else
		{
			UnityEngine.Object.Destroy(content as UnityEngine.Object);
		}
		r._content = null;
		return true;
	}

	public object GetContentImmediate()
	{
		return content ?? RetrieveContent().ForceResult<object>();
	}

	public virtual IDataContent GetDataImmediate()
	{
		return null;
	}

	protected bool _IsMissingContentFile()
	{
		if (isResource)
		{
			return false;
		}
		ContentRef contentRef = r;
		bool? isMissingContentFile = contentRef._isMissingContentFile;
		if (!isMissingContentFile.HasValue)
		{
			bool? flag = (contentRef._isMissingContentFile = !File.Exists(savePath));
			return flag.Value;
		}
		return isMissingContentFile.GetValueOrDefault();
	}

	protected void _CleanDirectory()
	{
		string directoryName = Path.GetDirectoryName(savePath);
		if (!Directory.Exists(directoryName))
		{
			return;
		}
		foreach (string item in from s in Directory.GetFiles(directoryName)
			where s != savePath && s != referenceSavePath
			select s)
		{
			File.Delete(item);
		}
	}

	public T _Reference<T>() where T : ContentRef
	{
		return r as T;
	}

	protected void _GetUniqueFilename()
	{
		if (fileId == 0)
		{
			_LoadSimilarReferences();
			fileId = Time;
			while (isTracked)
			{
				uint num = fileId + 1;
				fileId = num;
			}
		}
	}

	protected virtual void _SetVisibilityAndSaveIfChanged(ContentVisibility newVisibility)
	{
		if (visibility != newVisibility)
		{
			if (Steam.DEBUG.LogText())
			{
				Log.Text(detailedName + " <u>Visibility</u> Changed from [" + EnumUtil.FriendlyName(visibility) + "] to [" + EnumUtil.FriendlyName(newVisibility) + "]");
			}
			visibility = newVisibility;
			SaveReferenceChanges();
		}
	}

	private void _SetMatureAndSaveIfChanged(MatureContentFlags newMature)
	{
		if (mature != newMature)
		{
			if (Steam.DEBUG.LogText())
			{
				Log.Text(detailedName + " <u>Mature Content</u> Changed from [" + EnumUtil.FriendlyName(mature) + "] to [" + EnumUtil.FriendlyName(newMature) + "]");
			}
			mature = newMature;
			SaveReferenceChanges();
		}
	}

	private void _SetGroupIdsSaveIfChanged(ulong[] newGroupIds)
	{
		if (groupIds.SequenceEqualSafe(newGroupIds))
		{
			return;
		}
		if (Steam.DEBUG.LogText())
		{
			Log.Text(detailedName + " <u>Groups</u> Changed from [" + (groupIds.IsNullOrEmpty() ? "None" : groupIds.Select((ulong id) => new Steam.Friends.Group((CSteamID)id)).ToStringSmart()) + "] to [" + (newGroupIds.IsNullOrEmpty() ? "None" : newGroupIds.Select((ulong id) => new Steam.Friends.Group((CSteamID)id)).ToStringSmart()) + "]");
		}
		groupIds = newGroupIds;
		SaveReferenceChanges();
	}

	public bool Equals(ContentRef other)
	{
		Key? key = other?.key;
		Key key2 = this.key;
		if (!key.HasValue)
		{
			return false;
		}
		if (!key.HasValue)
		{
			return true;
		}
		return key.GetValueOrDefault() == key2;
	}

	public override bool Equals(object obj)
	{
		if (obj is ContentRef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return fileId.GetHashCode();
	}

	public override string ToString()
	{
		if (!isTracked)
		{
			return GetType().GetUILabel();
		}
		return friendlyName;
	}

	public static implicit operator Key(ContentRef cRef)
	{
		return new Key(cRef.creatorUID, cRef.fileId, cRef._keyCategoryId);
	}

	public static implicit operator bool(ContentRef cRef)
	{
		return cRef.ShouldSerialize();
	}

	public static implicit operator uint(ContentRef cRef)
	{
		return cRef.fileId;
	}

	[ProtoAfterDeserialization]
	protected void _ProtoAfterDeserialization()
	{
		_Track();
	}

	public int CompareTo(ContentRef other)
	{
		return (int)(key.fileId - other.key.fileId);
	}
}
