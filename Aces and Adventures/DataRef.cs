using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

[ProtoContract]
[UIField("<i>Unset Data Reference</i>", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class DataRef<C> : ContentRef where C : IDataContent
{
	private static string _CategoryName;

	private static string _CategoryFolder;

	private static byte? _KeyCategoryId;

	public static readonly C DefaultData;

	public static readonly byte[] DefaultBytes;

	public new static readonly Func<DataRef<C>, string> GetSearchStringFunc;

	private static readonly string _SpecificType;

	private static readonly string _SpecificTypeFriendly;

	private static List<DataRef<C>> _CachedSearchResults;

	private static bool _ResourcesLoaded;

	private static bool _UGCLoaded;

	private static bool _Loaded;

	private static Dictionary<Key, C> _Overrides;

	private static string CategoryName => _CategoryName ?? (_CategoryName = typeof(C).FriendlyName().Replace('<', '[').Replace('>', ']'));

	private static string CategoryFolder => _CategoryFolder ?? (_CategoryFolder = "Data/" + CategoryName);

	private static byte KeyCategoryId
	{
		get
		{
			byte valueOrDefault = _KeyCategoryId.GetValueOrDefault();
			if (!_KeyCategoryId.HasValue)
			{
				valueOrDefault = ContentRef._DataKeyCategoryId++;
				_KeyCategoryId = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public new static IEnumerable<DataRef<C>> All => _CachedSearchResults ?? (_CachedSearchResults = new List<DataRef<C>>(Search()));

	private static Dictionary<Key, C> Overrides => _Overrides ?? (_Overrides = new Dictionary<Key, C>());

	public override string categoryName => CategoryName;

	public C data => (C)GetDataImmediate();

	public bool hasUnsavedChanges => ContentInMemoryDistinctFromDisk();

	public override string type => "Data";

	protected override byte _keyCategoryId => KeyCategoryId;

	public override string specificType => _SpecificType;

	public override string specificTypeFriendly => _SpecificTypeFriendly;

	public override Type dataType => typeof(C);

	public override ContentRefType contentType => ContentRefType.Data;

	public override bool canHaveDependencies => true;

	public override string name
	{
		get
		{
			return friendlyName;
		}
		set
		{
		}
	}

	public override string tags
	{
		get
		{
			return data.tags;
		}
		set
		{
			C val = data;
			val.tags = value;
		}
	}

	public override bool isDataRef => true;

	protected override bool shouldCheckForUpdatedContentOnSave => true;

	public override string friendlyName => data.GetTitle();

	protected override string _previewFilePath
	{
		get
		{
			if (!DataRefSearchView.TypeViews.ContainsKey(GetType()))
			{
				return null;
			}
			return IOUtil.Combine(base.tempDirectoryPath, "preview.png");
		}
	}

	public override bool usesWorkshopMetaData => true;

	static DataRef()
	{
		DefaultData = ConstructorCache<C>.Constructor();
		DefaultBytes = ProtoUtil.ToByteArray(DefaultData);
		GetSearchStringFunc = (DataRef<C> dRef) => dRef.GetSearchString();
		_SpecificType = typeof(C).FriendlyName().FriendlyFromCamelOrPascalCase();
		_SpecificTypeFriendly = _SpecificType.Split(new string[1] { "Data" }, StringSplitOptions.None)[0].Trim();
	}

	public static IEnumerable<DataRef<C>> Search(bool mustBeCommitted = true, bool loadResources = true, bool loadUGC = true)
	{
		ContentRef._LoadContentReferencesInFolder(CategoryFolder, loadResources && !_ResourcesLoaded && (_ResourcesLoaded = true), loadUGC && !_UGCLoaded && (_UGCLoaded = true));
		foreach (ContentRef item in ContentRef.ContentReferences.EnumerateValuesSafe())
		{
			if (item is DataRef<C> dataRef && ((dataRef.committed && (bool)dataRef) || (!mustBeCommitted && dataRef.content != null)))
			{
				yield return dataRef;
			}
		}
	}

	private static IEnumerable<ContentRef> _SearchData(bool mustBeCommitted)
	{
		foreach (DataRef<C> item in Search(mustBeCommitted))
		{
			yield return item;
		}
	}

	public static void LoadAll()
	{
		if (!_Loaded ^ (_Loaded = true))
		{
			return;
		}
		foreach (DataRef<C> item in Search(mustBeCommitted: true, loadResources: true, loadUGC: false))
		{
			item.GetDataImmediate();
		}
	}

	public static void Warmup()
	{
		Search().FirstOrDefault()?.GetContentImmediate();
	}

	public static void ClearAllDataOverrides()
	{
		Overrides.Clear();
	}

	public static void ClearLoadedCache()
	{
		_ResourcesLoaded = (_Loaded = (_UGCLoaded = false));
		ClearAllDataOverrides();
	}

	public static Key GetKeyFromFileId(uint fileId)
	{
		return new Key(null, fileId, KeyCategoryId);
	}

	public static DataRef<C> FromFileId(uint fileId)
	{
		return ContentRef.GetByKey<DataRef<C>>(GetKeyFromFileId(fileId));
	}

	public DataRef()
	{
	}

	public DataRef(C content)
	{
		SetData(content);
	}

	protected override string GetExtension()
	{
		return ".bytes";
	}

	protected override IEnumerator Import(string inputFile, string contentSavePath, bool isResource)
	{
		yield return ToBackgroundThread.Create();
		IOUtil.CopyFile(inputFile, contentSavePath, forceOverwrite: true);
	}

	protected override IEnumerator LoadResource()
	{
		TextAsset textAsset = Resources.Load<TextAsset>(base.loadPath);
		yield return IOUtil.FromByteArray<C>(textAsset ? textAsset.bytes : DefaultBytes);
	}

	protected override IEnumerator LoadUGC()
	{
		yield return IOUtil.LoadFromBytes<C>(base.loadPath);
	}

	protected override void OnLoadValidation()
	{
		if (base.content is IDataContent)
		{
			((IDataContent)base.content).OnLoadValidation();
		}
	}

	protected override string GetSaveErrorMessage()
	{
		return data.PrepareDataAndGetSaveErrorMessage();
	}

	protected override IEnumerator SaveContent()
	{
		data.PrepareDataForSave();
		IOUtil.WriteBytes(base.content, base.savePath);
		yield break;
	}

	protected override HashSet<ContentRef> _CalculateDependencies()
	{
		return (from cRef in ReflectionUtil.FindAllInstances<ContentRef>(data)
			where cRef.isTracked
			select cRef).ToHash();
	}

	public override byte[] GetContentBytesInMemory()
	{
		return IOUtil.ToByteArray(data);
	}

	public override bool ContentInMemoryDistinctFromDisk()
	{
		if (!_IsMissingContentFile())
		{
			return !GetContentBytesOnDisk().SequenceEqual(GetContentBytesInMemory());
		}
		return true;
	}

	public bool ContentInMemoryDistinctFromDisk(byte[] contentBytes)
	{
		if (!_IsMissingContentFile())
		{
			return !GetContentBytesOnDisk().SequenceEqual(contentBytes);
		}
		return true;
	}

	public void RevertChanges()
	{
		base.content = ProtoUtil.FromBytes<C>(GetContentBytesOnDisk());
	}

	public C SetContent(C contentToSet)
	{
		object obj2 = (base.content = contentToSet);
		return (C)obj2;
	}

	public override IDataContent GetDataImmediate()
	{
		return (IDataContent)((base.content as IDataContent) ?? ((object)SetContent(base.isResource ? IOUtil.FromByteArray<C>(Resources.Load<TextAsset>(base.loadPath)?.bytes ?? DefaultBytes) : IOUtil.LoadFromBytes<C>(base.loadPath))));
	}

	protected override List<string> GetAutomatedTags()
	{
		return data.GetAutomatedTags();
	}

	public void GetGeneratedFromContent<O>(Func<C, IEnumerator> generateLogic, Action<O> onGeneratedContentRetrieved, bool forceImmediate = false, string name = "")
	{
		_GetGeneratedFromContent(generateLogic, name, onGeneratedContentRetrieved, forceImmediate);
	}

	public void GetGeneratedFromRef<O>(Func<DataRef<C>, IEnumerator> generateLogic, Action<O> onGeneratedContentRetrieved, bool forceImmediate = false, string name = "")
	{
		_GetGeneratedFromRef(generateLogic, name, onGeneratedContentRetrieved, forceImmediate);
	}

	public override void ClearGeneratedContent(bool unload = false)
	{
		base.ClearGeneratedContent(unload);
		foreach (ContentRef dependency in GetDependencies())
		{
			if (!dependency.canHaveDependencies)
			{
				dependency.ClearGeneratedContent(unload);
			}
		}
	}

	public override void UnloadGeneratedContent<T>()
	{
		base.UnloadGeneratedContent<T>();
		foreach (ContentRef dependency in GetDependencies())
		{
			if (!dependency.canHaveDependencies)
			{
				dependency.UnloadGeneratedContent<T>();
			}
		}
	}

	public override void SaveFromUI(object dataToSave, SaveNameConfirmation confirmation, Action onSaveSuccessful, Action onSaveFailed = null, Transform parent = null)
	{
		SaveFromUI((C)dataToSave, confirmation, onSaveSuccessful, onSaveFailed, parent);
	}

	public override Job SetDataAndSave(object dataObject)
	{
		C val = (C)dataObject;
		if (!val.PrepareDataAndGetSaveErrorMessage().IsNullOrEmpty())
		{
			return Job.Process(Job.EmptyEnumerator(), Department.Content);
		}
		DataRef<C> obj = (base.belongsToCurrentCreator ? this : new DataRef<C>());
		obj.SetData(val);
		return obj.Save(forceOverwrite: true);
	}

	public override void SaveFromUIWithoutValidation(object dataToSave, Action onSaveSuccessful = null)
	{
		SetData(ProtoUtil.Clone((C)dataToSave));
		InputManager.SetEventSystemEnabled(this, enabled: false);
		_CachedSearchResults = null;
		Save(forceOverwrite: true).Then().Do(delegate
		{
			onSaveSuccessful?.Invoke();
		});
		Job.Process(Job.WaitForDepartmentEmpty(Department.Content)).Immediately().Do(delegate
		{
			InputManager.SetEventSystemEnabled(this, enabled: true);
		});
	}

	public override string GetDescription()
	{
		return data.GetAutomatedDescription();
	}

	protected override IEnumerable<KeyValuePair<string, string>> _GetAdditionalWorkshopTags()
	{
		return data.GetAdditionalWorkshopTags();
	}

	protected override async Task _GeneratePreviewImageFile()
	{
		if (!DataRefSearchView.TypeViews.ContainsKey(GetType()))
		{
			return;
		}
		GameObject view = DataRefSearchView.TypeViews[GetType()](this, null);
		await ContentRef.WaitForContentToBeReadyAsync();
		foreach (ToggleFloat item in view.gameObject.GetComponentsInChildrenPooled<ToggleFloat>())
		{
			item.ForceFinish();
		}
		Texture2D texture2D = GraphicsUtil.RenderUIObject(view);
		IOUtil.WriteBytes(_previewFilePath, texture2D.EncodeToPNG());
		if (ContentRefSearcher.GetScale(this) < 1f)
		{
			NConvert.Resize(_previewFilePath, Mathf.RoundToInt((float)texture2D.PixelSize().max * ContentRefSearcher.GetScale(this)));
		}
		UnityEngine.Object.Destroy(texture2D);
	}

	protected override async Task<PoolKeepItemListHandle<string>> _GenerateAdditionalPreviewImageFilesAsync()
	{
		PoolKeepItemListHandle<string> output = await base._GenerateAdditionalPreviewImageFilesAsync();
		foreach (string item in await data.GenerateAdditionalPreviewsAsync(this))
		{
			output.Add(item);
		}
		return output;
	}

	protected override async Task<bool> _IsContentValidForUpload()
	{
		try
		{
			C val = IOUtil.LoadFromBytes<C>(base.loadPath);
			val.OnLoadValidation();
			return val.PrepareDataAndGetSaveErrorMessage().IsNullOrEmpty();
		}
		catch (Exception ex)
		{
			if (Steam.DEBUG.LogError() && ex != null)
			{
				Log.Error("Data for [" + base.detailedName + "] is invalid. This is most likely caused by new saving rules, please update data and save again in order to upload.");
			}
			return false;
		}
	}

	protected override byte[] _GetWorkshopMetaData()
	{
		return ProtoUtil.ToByteArray(new MetaData(this, data.GetQuickDependencies()));
	}

	protected override string _GetWorkshopTrailerVideoId()
	{
		return data.GetWorkshopTrailerVideoId();
	}

	protected override void _SetVisibilityAndSaveIfChanged(ContentVisibility newVisibility)
	{
		base._SetVisibilityAndSaveIfChanged(data.ForcedVisibility() ?? newVisibility);
	}

	public override IEnumerable<ContentRef> SearchSimilar()
	{
		return Search();
	}

	protected override void _LoadSimilarReferences()
	{
		ContentRef._LoadContentReferencesInFolder(CategoryFolder, !_ResourcesLoaded && (_ResourcesLoaded = true), !_UGCLoaded && (_UGCLoaded = true));
	}

	protected override void _OnDelete()
	{
		if (!typeof(C).HasAttribute<LocalizeAttribute>())
		{
			return;
		}
		SharedTableData sharedTableData = LocalizationSettings.StringDatabase.GetTable(this.GetTableReference())?.SharedData;
		if (!(sharedTableData != null))
		{
			return;
		}
		foreach (LocalizedStringData item in ReflectionUtil.GetValuesFromUI<LocalizedStringData>(data))
		{
			if ((bool)item.id)
			{
				_ = item.id.table.SharedData == sharedTableData;
			}
		}
	}

	public Job SaveFromUIJob(C dataToSave, SaveNameConfirmation confirmation, Transform parent = null)
	{
		TextAlignmentOptions alignment = TextAlignmentOptions.Left;
		int num = 24;
		string text = dataToSave.PrepareDataAndGetSaveErrorMessage();
		Transform parent2;
		Department? department;
		if (text.IsNullOrEmpty())
		{
			string newName = dataToSave.GetTitle();
			if (!base.isTracked || data == null)
			{
				string[] buttons;
				if (Search().Any((DataRef<C> dataRef) => dataRef.friendlyName == newName))
				{
					string title = "Save <b>" + newName + "</b>";
					GameObject mainContent = UIUtil.CreateMessageBox(specificTypeFriendly + " named <b>" + newName + "</b> already exists. Save anyway?", alignment, 32, 600, 300, num);
					buttons = new string[2] { "Cancel", "Save" };
					parent2 = parent;
					department = Department.Content;
					return UIUtil.CreatePopupJob(title, mainContent, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent2, buttons, department);
				}
				string title2 = "Save <b>" + newName + "</b>";
				GameObject mainContent2 = UIUtil.CreateMessageBox("Do you wish to save <b>" + newName + "</b>?", alignment, 32, 600, 300, num);
				buttons = new string[2] { "Cancel", "Save" };
				parent2 = parent;
				department = Department.Content;
				return UIUtil.CreatePopupJob(title2, mainContent2, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent2, buttons, department);
			}
			string title3 = data.GetTitle();
			switch (confirmation)
			{
			case SaveNameConfirmation.Overwrite:
			{
				string[] buttons;
				if (_IsMissingContentFile())
				{
					string title6 = "Save <b>" + newName + "</b>";
					GameObject mainContent5 = UIUtil.CreateMessageBox("Do you wish to save <b>" + newName + "</b>?", alignment, 32, 600, 300, num);
					buttons = new string[2] { "Cancel", "Save" };
					parent2 = parent;
					department = Department.Content;
					return UIUtil.CreatePopupJob(title6, mainContent5, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent2, buttons, department);
				}
				string text2 = " This will affect all data that references <b>" + title3 + "</b>.";
				if (title3 == newName)
				{
					string title7 = "Overwrite <b>" + title3 + "</b> " + specificTypeFriendly;
					GameObject mainContent6 = UIUtil.CreateMessageBox("Do you wish to overwrite <b>" + title3 + "</b>?" + text2, alignment, 32, 600, 300, num);
					buttons = new string[2] { "Cancel", "Overwrite" };
					parent2 = parent;
					department = Department.Content;
					return UIUtil.CreatePopupJob(title7, mainContent6, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent2, buttons, department);
				}
				string title8 = "Rename <b>" + title3 + "</b> " + specificTypeFriendly;
				GameObject mainContent7 = UIUtil.CreateMessageBox("Do you wish to rename <b>" + title3 + "</b> to <b>" + newName + "</b>?" + text2, alignment, 32, 600, 300, num);
				buttons = new string[2] { "Cancel", "Rename" };
				parent2 = parent;
				department = Department.Content;
				return UIUtil.CreatePopupJob(title8, mainContent7, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent2, buttons, department);
			}
			case SaveNameConfirmation.SaveAsNew:
			{
				string[] buttons;
				if (Search().Any((DataRef<C> dataRef) => dataRef.friendlyName == newName))
				{
					string title4 = "Save <b>" + newName + "</b> As New";
					GameObject mainContent3 = UIUtil.CreateMessageBox(specificTypeFriendly + " named <b>" + newName + "</b> already exists. Save as new anyway?", alignment, 32, 600, 300, num);
					buttons = new string[2] { "Cancel", "Save As New" };
					parent2 = parent;
					department = Department.Content;
					return UIUtil.CreatePopupJob(title4, mainContent3, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent2, buttons, department);
				}
				string title5 = "Save <b>" + newName + "</b> As New";
				GameObject mainContent4 = UIUtil.CreateMessageBox("Do you wish to save <b>" + newName + "</b> as a new " + specificTypeFriendly + "?", alignment, 32, 600, 300, num);
				buttons = new string[2] { "Cancel", "Save As New" };
				parent2 = parent;
				department = Department.Content;
				return UIUtil.CreatePopupJob(title5, mainContent4, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent2, buttons, department);
			}
			}
		}
		GameObject mainContent8 = UIUtil.CreateMessageBox(text, alignment, 32, 600, 300, num);
		parent2 = parent;
		department = Department.Content;
		return UIUtil.CreatePopupJob("Unable To Save", mainContent8, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent2, null, department);
	}

	public void SaveFromUI(C dataToSave, SaveNameConfirmation confirmation, Action onSaveSuccessful, Action onSaveFailed = null, Transform parent = null)
	{
		SaveFromUIJob(dataToSave, confirmation, parent).Next().ResultAction(delegate(string s)
		{
			if (s.IsNullOrEmpty() || s == "Cancel")
			{
				onSaveFailed?.Invoke();
			}
			else
			{
				SaveFromUIWithoutValidation(dataToSave, onSaveSuccessful);
			}
		});
	}

	public void SetDataOverride(C dataOverride = default(C))
	{
		if (dataOverride != null)
		{
			Overrides[this] = dataOverride;
		}
		else
		{
			Overrides.Remove(this);
		}
	}

	public C GetDataOverride()
	{
		C valueOrDefault = Overrides.GetValueOrDefault(this);
		if (valueOrDefault == null)
		{
			return data;
		}
		return valueOrDefault;
	}

	private void SetData(C data)
	{
		base.creatorUID = ContentRef.CreatorId;
		if (base.fileId == 0)
		{
			_GetUniqueFilename();
		}
		_Track();
		base.content = data;
		_MarkAsUncommitted();
	}
}
