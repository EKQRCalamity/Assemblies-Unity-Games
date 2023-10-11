using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataRefControl : MonoBehaviour, IDataRefControl
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ActiveEditOverride : IDisposable
	{
		public ActiveEditOverride(ContentRef contentRef)
		{
			_ActiveEditKeyOverride = contentRef;
		}

		public void Dispose()
		{
			_ActiveEditKeyOverride = null;
		}
	}

	private static readonly List<DataRefControl> _ActiveEditKeys = new List<DataRefControl>();

	private static Dictionary<Couple<MemberInfo, ContentRef.Key>, object> _LiveEdits = new Dictionary<Couple<MemberInfo, ContentRef.Key>, object>();

	private static PoolDictionaryValuesHandle<ContentRef, PoolKeepItemHashSetHandle<IDataRefControl>> _RefToControlMap;

	private static Dictionary<object, ContentRef> _DataToRefMap;

	public Type dataType;

	[Header("Events")]
	public ObjectEvent onDataRefChanged;

	public ObjectEvent onRequestContentUI;

	public StringEvent onContentNameChanged;

	[Header("Containers")]
	public GameObject dataContainer;

	public GameObject selectButtonsContainer;

	public GameObject changesButtonContainer;

	public GameObject collapse;

	[Header("Buttons")]
	public GameObject createNewButton;

	public GameObject saveButton;

	public GameObject editButton;

	public GameObject collapseButton;

	public GameObject tagsButton;

	public GameObject uploadButton;

	public GameObject downloadButton;

	public GameObject inspectButton;

	public GameObject watchTutorialButton;

	private ContentRef _dataRef;

	private bool _dataRefHasContent;

	private object _data;

	private byte[] _snapShot = new byte[0];

	private string _previousFriendlyName;

	private string _friendlyNamePrefix = "";

	private static ContentRef.Key? _ActiveEditKeyOverride { get; set; }

	public static ContentRef.Key ActiveEditKey
	{
		get
		{
			ContentRef.Key? activeEditKeyOverride = _ActiveEditKeyOverride;
			if (!activeEditKeyOverride.HasValue)
			{
				DataRefControl activeControl = ActiveControl;
				if ((object)activeControl == null || activeControl.dataRef == null)
				{
					return ContentRef.Key.Null;
				}
				return activeControl.dataRef;
			}
			return activeEditKeyOverride.GetValueOrDefault();
		}
	}

	public static DataRefControl ActiveControl => _ActiveEditKeys.LastOrDefault();

	public static Canvas ActiveCanvas => ActiveControl?.GetComponentInParent<Canvas>();

	public static bool UseLiveEditMode { get; private set; }

	private static PoolDictionaryValuesHandle<ContentRef, PoolKeepItemHashSetHandle<IDataRefControl>> RefToControlMap => _RefToControlMap ?? (_RefToControlMap = Pools.UseDictionaryValues<ContentRef, PoolKeepItemHashSetHandle<IDataRefControl>>());

	private static Dictionary<object, ContentRef> DataToRefMap => _DataToRefMap ?? (_DataToRefMap = new Dictionary<object, ContentRef>(ReferenceEqualityComparer<object>.Default));

	public object data
	{
		get
		{
			return _data;
		}
		private set
		{
			if (_data != value)
			{
				if (_data != null)
				{
					DataToRefMap.Remove(_data);
				}
				_data = value;
				if (_data != null)
				{
					DataToRefMap[_data] = _dataRef;
				}
			}
		}
	}

	public object ownerObject { get; set; }

	public MemberInfo memberInfo { get; set; }

	public string filterMethodName { get; set; }

	public string excludedValuesMethodName { get; set; }

	public Func<object, bool> excludedValues { get; set; }

	public DataRefControlFilter filter { get; set; }

	public IEnumerable<KeyValuePair<string, string>> additionalWorkshopTags { get; set; }

	private IDataContent _iDataContent => data as IDataContent;

	public ContentRef dataRef => _dataRef;

	public bool isValid => this;

	public bool showData
	{
		get
		{
			if (filter.ShowData() && (bool)dataContainer)
			{
				return dataContainer.activeInHierarchy;
			}
			return false;
		}
	}

	public static event Action<DataRefControl> OnDataRefChanged;

	public event Action onManuallySelected;

	public static ActiveEditOverride BeginEdit(ContentRef contentRef)
	{
		return new ActiveEditOverride(contentRef);
	}

	private static ContentRef CreateNewDataRef(Type dataType, object data)
	{
		Type type = typeof(DataRef<>).MakeGenericType(dataType);
		return ((data == null) ? Activator.CreateInstance(type, nonPublic: true) : Activator.CreateInstance(type, data)) as ContentRef;
	}

	private static object GetLiveEdit(MemberInfo memberInfo, ContentRef dataRef)
	{
		if (memberInfo == null || dataRef == null)
		{
			return null;
		}
		Couple<MemberInfo, ContentRef.Key> key = new Couple<MemberInfo, ContentRef.Key>(memberInfo, dataRef.GetIdentifier());
		if (!_LiveEdits.ContainsKey(key))
		{
			return null;
		}
		return _LiveEdits[key];
	}

	private static void AddRefToControlMap(ContentRef dataRef, IDataRefControl control)
	{
		if (!RefToControlMap.ContainsKey(dataRef))
		{
			RefToControlMap.Add(dataRef, Pools.UseKeepItemHashSet<IDataRefControl>());
		}
		RefToControlMap[dataRef].Add(control);
	}

	private static void RemoveRefToControlMap(ContentRef dataRef, IDataRefControl control)
	{
		if (dataRef.IsValid() && RefToControlMap.ContainsKey(dataRef) && RefToControlMap[dataRef].Remove(control) && RefToControlMap[dataRef].Count == 0)
		{
			RefToControlMap.Remove(dataRef);
		}
	}

	public static IDataRefControl GetControl(ContentRef dataRef)
	{
		if (!dataRef.IsValid() || !RefToControlMap.ContainsKey(dataRef))
		{
			return null;
		}
		return RefToControlMap[dataRef].value.First();
	}

	public static ContentRef GetRef(object data)
	{
		if (!DataToRefMap.ContainsKey(data))
		{
			return null;
		}
		return DataToRefMap[data];
	}

	public static IDataRefControl GetMainControlForDataType(Type dataRefType)
	{
		return RefToControlMap.value.Values.SelectMany((PoolKeepItemHashSetHandle<IDataRefControl> h) => h.value).FirstOrDefault((IDataRefControl c) => c.showData && c.dataRef.IsValid() && dataRefType.IsSameOrSubclass(c.dataRef.GetType()));
	}

	public static IDataRefControl GetMainControlForDataType(ContentRef dataRef)
	{
		return GetMainControlForDataType(dataRef.GetType());
	}

	public static IDataRefControl GetMainControlForDataType<T>() where T : ContentRef
	{
		return GetMainControlForDataType(typeof(T));
	}

	public static IEnumerable<IDataRefControl> GetActiveControlsWithUnsavedChanges()
	{
		return from c in RefToControlMap.value.Values.SelectMany((PoolKeepItemHashSetHandle<IDataRefControl> h) => h.value)
			where c.HasUnsavedChanges()
			select c;
	}

	public static void SaveAllActiveControls()
	{
		Job.Process(Job.EmptyEnumerator(), Department.Content).ChainJobs(GetActiveControlsWithUnsavedChanges().Select((Func<IDataRefControl, Func<Job>>)((IDataRefControl c) => () => c.dataRef.SetDataAndSave(Serializer.DeepClone(c.data)))).ToArray());
	}

	public static void SaveActiveEditControl()
	{
		DataRefControl dataRefControl = _ActiveEditKeys.LastOrDefault();
		dataRefControl?.dataRef.SaveFromUIWithoutValidation(dataRefControl.data);
	}

	public static void CreateLiveEditPopup(ContentRef dataRef, Transform parent = null, Action<ContentRef> onClose = null)
	{
		UseLiveEditMode = true;
		Type type = typeof(Ref<>).MakeGenericType(typeof(DataRef<>).MakeGenericType(dataRef.dataType));
		object[] constructorParameters = new ContentRef[1] { dataRef };
		GameObject gameObject = UIUtil.CreateReflectedObject(ReflectionUtil.CreateInstanceSmart(type, constructorParameters), 1920f, 1080f);
		IDataRefControl control = gameObject.GetComponentInChildren<IDataRefControl>(includeInactive: true);
		UIUtil.CreatePopup("Edit " + dataRef.specificTypeFriendly + ": " + dataRef.GetFriendlyName(), gameObject, null, parent: parent, delayClose: Job.WaitForDepartmentEmpty(Department.Content).Then(delegate
		{
			if (onClose != null && control.IsValid() && (bool)control.dataRef)
			{
				onClose(control.dataRef);
			}
		}), size: null, centerReferece: null, center: null, pivot: null, onClose: delegate
		{
			if (control.IsValid() && control.HasUnsavedChanges())
			{
				control.OnSaveChanges();
			}
		}, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: null, rayCastBlockerColor: null, referenceResolution: null, buttons: Array.Empty<string>());
		UseLiveEditMode = false;
	}

	public static void SetWorkshopButtonTooltips(GameObject upload, GameObject download, GameObject inspect, GameObject tags = null, GameObject watchTutorial = null)
	{
		float waitTime = 0.5f;
		int num = 2;
		TooltipDirection direction = TooltipDirection.Vertical;
		bool trackCreator = true;
		if ((bool)upload)
		{
			TooltipCreator.CreateTextTooltip(upload.transform, "Upload To Workshop", beginShowTimer: false, waitTime, backgroundEnabled: true, TextAlignmentOptions.Center, num, 12f, direction, TooltipOrthogonalDirection.Center, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator);
		}
		if ((bool)download)
		{
			TooltipCreator.CreateTextTooltip(download.transform, "Search & Download From Workshop", beginShowTimer: false, waitTime, backgroundEnabled: true, TextAlignmentOptions.Center, num, 12f, direction, TooltipOrthogonalDirection.Center, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator);
		}
		if ((bool)inspect)
		{
			TooltipCreator.CreateTextTooltip(inspect.transform, "View Workshop Item Details", beginShowTimer: false, waitTime, backgroundEnabled: true, TextAlignmentOptions.Center, num, 12f, direction, TooltipOrthogonalDirection.Center, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator);
		}
		if ((bool)tags)
		{
			TooltipCreator.CreateTextTooltip(tags.transform, "Edit Tags", beginShowTimer: false, waitTime, backgroundEnabled: true, TextAlignmentOptions.Center, num, 12f, direction, TooltipOrthogonalDirection.Center, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator);
		}
		if ((bool)watchTutorial)
		{
			TooltipCreator.CreateTextTooltip(watchTutorial.transform, "Watch Tutorial Video", beginShowTimer: false, waitTime, backgroundEnabled: true, TextAlignmentOptions.Center, num, 12f, direction, TooltipOrthogonalDirection.Center, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator);
		}
	}

	public void SetDataRef(ContentRef dataRef, bool restoreLiveEditData = false, bool manuallySelected = false)
	{
		if (!ReflectionUtil.SafeEquals(_dataRef, dataRef))
		{
			dataRef = ProtoUtil.Clone(dataRef);
			RemoveRefToControlMap(_dataRef, this);
			_dataRef = dataRef;
			if (_dataRef.IsValid())
			{
				AddRefToControlMap(_dataRef, this);
			}
			_UpdateButtons();
			_InvokeDataEvents(null, restoreLiveEditData ? GetLiveEdit(memberInfo, _dataRef) : null);
		}
		if (filter == DataRefControlFilter.Simple)
		{
			createNewButton.SetUILabel(_dataRef.Exists() ? "Clear" : "Create New");
		}
		if (manuallySelected && this.onManuallySelected != null && _dataRef.IsValid())
		{
			this.onManuallySelected();
		}
	}

	public void SetDataRefIfValid(ContentRef dataRef)
	{
		if ((bool)this && dataRef.IsValid())
		{
			SetDataRef(dataRef);
		}
	}

	public void SetDataRefIfValidAndValidateUI(ContentRef dataRef)
	{
		if (dataRef.IsValid())
		{
			SetDataRef(dataRef);
			UIGeneratorType.ValidateAllOfType<object>();
		}
	}

	public void OnSelect()
	{
		Func<ContentRef, bool> filterMethod = null;
		if (ownerObject != null && !filterMethodName.IsNullOrEmpty())
		{
			MethodInfo filterMethodInfo = ownerObject.GetType().GetMethodInfoSmart<bool>(filterMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, searchOverloads: false, null, new object[1] { data ?? ReflectionUtil.CreateInstanceSmart(dataType) });
			if (filterMethodInfo != null)
			{
				filterMethod = (ContentRef cRef) => (bool)filterMethodInfo.Invoke(ownerObject, new object[1] { cRef.GetContentImmediate() });
			}
		}
		UIUtil.CreateDataSearchPopup(dataType, delegate(ContentRef cRef)
		{
			SetDataRef(cRef, restoreLiveEditData: false, manuallySelected: true);
		}, base.transform, (ContentRef cRef) => (filterMethod == null || filterMethod(cRef)) && (excludedValues == null || !excludedValues(cRef)), mustBeCommitted: true, !excludedValuesMethodName.IsNullOrEmpty());
	}

	public void OnEdit()
	{
		dataRef.OnEditRequest(base.transform, SetDataRefIfValidAndValidateUI);
	}

	public void OnEditTags()
	{
		if (_dataRef.IsValid())
		{
			UIUtil.CreateContentRefTagsPopup(_dataRef, base.transform, _iDataContent, _OnSaveTagChanges, filter.ShowData() ? "Confirm Changes" : null);
		}
	}

	public void OnCreate()
	{
		bool flag = _dataRef.Exists();
		if (showData && HasUnsavedChanges())
		{
			string title = "Create New " + _dataRef.specificTypeFriendly;
			UIUtil.CreatePopup(title, UIUtil.CreateMessageBox("Would you like to create a new <b>" + _dataRef.specificTypeFriendly + "</b>? All unsaved changes to <b>" + _dataRef.GetFriendlyName() + "</b> will be lost.", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: base.transform, buttons: new string[2] { "Cancel", title }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
			{
				if (s == title)
				{
					SetDataRef(_CreateRef());
				}
			});
		}
		else
		{
			SetDataRef(_CreateRef());
			if (filter == DataRefControlFilter.Simple && !flag)
			{
				OnEdit();
			}
		}
	}

	public void SetFriendlyNamePrefix(string prefix)
	{
		_friendlyNamePrefix = ((!prefix.IsNullOrEmpty()) ? (prefix.Split(':')[0] + ": ") : "");
	}

	public void OnSaveChanges()
	{
		_SaveCommon((!_dataRef.SaveCanBeOverriden()) ? SaveNameConfirmation.SaveAsNew : SaveNameConfirmation.Overwrite);
	}

	public void OnSaveChanges(Transform parent)
	{
		object originalData = data;
		_SaveCommon(SaveNameConfirmation.Overwrite, parent, delegate
		{
			data = originalData;
		});
	}

	public void OnSaveAsNew()
	{
		_SaveCommon(SaveNameConfirmation.SaveAsNew);
	}

	public void OnRevertChanges()
	{
		if (_dataRef == null)
		{
			return;
		}
		UIUtil.CreatePopup("Revert Changes To <b>" + _dataRef.GetFriendlyName(), UIUtil.CreateMessageBox("Would you like to revert all changes made to <b>" + _dataRef.GetFriendlyName() + "</b> since it was last saved?", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: base.transform, buttons: new string[2] { "Cancel", "Revert Changes" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (s == "Revert Changes")
			{
				RevertChanges();
			}
		});
	}

	public void RevertChanges()
	{
		_InvokeDataEvents();
	}

	public void OnDataValueChanged()
	{
		if (!(changesButtonContainer == null) && filter.ShowData())
		{
			changesButtonContainer.SetActive(value: true);
			bool flag = data != null && _dataRefHasContent && !IOUtil.CompareProtoBytes(data, _snapShot);
			CanvasGroup component = changesButtonContainer.GetComponent<CanvasGroup>();
			component.blocksRaycasts = flag;
			component.alpha = flag.ToFloat(1f, 0.5f);
			if ((bool)uploadButton)
			{
				CanvasGroup orAddComponent = uploadButton.GetOrAddComponent<CanvasGroup>();
				orAddComponent.blocksRaycasts = !flag;
				orAddComponent.alpha = orAddComponent.blocksRaycasts.ToFloat(1f, 0.5f);
			}
		}
	}

	public void Refresh()
	{
		UIGeneratorType.Validate(data);
		OnDataValueChanged();
	}

	public void ChangeToLiveEditUI()
	{
		collapseButton.SetActive(value: false);
		selectButtonsContainer.SetActive(value: false);
		UnityEngine.Object.Destroy(tagsButton);
		UnityEngine.Object.Destroy(collapse.GetComponent<Image>());
		RectOffset padding = collapse.GetComponent<LayoutGroup>().padding;
		int num2 = (padding.right = 0);
		int top = (padding.left = num2);
		padding.top = top;
	}

	public void Upload()
	{
		if (_dataRef.CanUpload())
		{
			UIUtil.CreateContentRefUploadPopup(_dataRef, base.transform, _OnUpload);
		}
	}

	public void Download()
	{
		UIUtil.CreateWorkshopDataSearchPopup(_dataRef ?? _CreateRef(), _OnDownload, base.transform, additionalWorkshopTags);
	}

	public void InspectWorkshopItem()
	{
		if (_dataRef.CanInspectWorkshopItem())
		{
			UIUtil.CreateSteamWorkshopItemInspectPopup(_dataRef.workshopFileId, base.transform, _dataRef.usesWorkshopMetaData, _dataRef.usesAdditionalPreviews, _OnDownload);
		}
	}

	public void WatchTutorialVideo()
	{
		dataType.GetAttribute<YouTubeVideoAttribute>().WatchVideo(base.transform);
	}

	public void SetAsActiveEdit()
	{
		_ActiveEditKeys.Add(this);
	}

	private void _InvokeDataEvents(Action beforeGeneratingDataUI = null, object liveEditData = null)
	{
		if (!this)
		{
			return;
		}
		_dataRefHasContent = _dataRef.hasContent;
		dataContainer.SetActive(_dataRefHasContent && filter.ShowData());
		saveButton.SetActive(_dataRef.SaveCanBeOverriden());
		data = (_dataRefHasContent ? Serializer.DeepClone(_dataRef.GetContentImmediate()) : ReflectionUtil.CreateInstanceSmart(dataType));
		_snapShot = IOUtil.ToByteArray(data);
		if (liveEditData != null)
		{
			data = liveEditData;
		}
		OnDataValueChanged();
		onDataRefChanged.Invoke(_dataRef);
		if (data != null)
		{
			beforeGeneratingDataUI?.Invoke();
			if (filter.ShowData())
			{
				onRequestContentUI.Invoke(data);
			}
			_InvokeContentNameChanged(_GetDataFriendlyName());
		}
	}

	private void _InvokeContentNameChanged(string friendlyName)
	{
		onContentNameChanged.Invoke(_friendlyNamePrefix + friendlyName);
		_previousFriendlyName = friendlyName;
	}

	private string _GetDataFriendlyName()
	{
		if (!(data is IDataContent dataContent))
		{
			return "";
		}
		return dataContent.GetTitle();
	}

	public bool HasUnsavedChanges()
	{
		if (filter == DataRefControlFilter.None && _iDataContent != null && _iDataContent.GetSaveErrorMessage().IsNullOrEmpty())
		{
			return !IOUtil.CompareProtoBytes(data, _snapShot);
		}
		return false;
	}

	private void _SaveCommon(SaveNameConfirmation confirmation, Transform parent = null, Action onSave = null)
	{
		if (_dataRef == null)
		{
			return;
		}
		Action onSaveFailed = null;
		ContentRef originalDataRef = _dataRef;
		object dataToSave = Serializer.DeepClone(data);
		string dataName = (dataToSave as IDataContent)?.GetTitle();
		bool localized = dataToSave.GetType().HasAttribute<LocalizeAttribute>();
		bool saveAsNew = confirmation == SaveNameConfirmation.SaveAsNew;
		if (saveAsNew)
		{
			if (localized)
			{
				foreach (LocalizedStringData item in ReflectionUtil.GetValuesFromUI<LocalizedStringData>(dataToSave))
				{
					_ = item;
				}
			}
			_dataRef = CreateNewDataRef(dataType, dataToSave);
			onSaveFailed = delegate
			{
				_dataRef = originalDataRef;
			};
		}
		ContentRef dataRefBeingSaved = _dataRef;
		ContentRef contentRef = dataRefBeingSaved;
		object dataToSave2 = dataToSave;
		Action onSaveSuccessful = delegate
		{
			if (saveAsNew && localized)
			{
				foreach (LocalizedStringData item2 in ReflectionUtil.GetValuesFromUI<LocalizedStringData>(originalDataRef.GetContentImmediate()))
				{
					_ = item2;
				}
			}
			_ = saveAsNew;
			_InvokeDataEvents(onSave);
			_UpdateButtons();
			if (localized)
			{
				UIUtil.BeginProcessJob(base.transform, null, Department.UI).Then().DoProcess(Job.WaitForDepartment(Department.Content))
					.Then()
					.DoProcess(Job.WaitForOneFrame())
					.Then()
					.Do(delegate
					{
						ReflectionUtil.LocalizeDataRef(dataRefBeingSaved, dataToSave, dataName);
					})
					.Then()
					.Do(UIUtil.EndProcess);
			}
		};
		Transform parent2 = (parent ? parent : base.transform);
		contentRef.SaveFromUI(dataToSave2, confirmation, onSaveSuccessful, onSaveFailed, parent2);
	}

	private void _UpdateButtons()
	{
		if ((bool)this)
		{
			if ((bool)editButton)
			{
				editButton.SetActive(!filter.ShowData() && (bool)_dataRef);
			}
			if ((bool)tagsButton)
			{
				tagsButton.SetActive((bool)_dataRef && _dataRef.belongsToCurrentCreator);
			}
			if ((bool)uploadButton)
			{
				uploadButton.SetActive(_dataRef.CanUpload());
			}
			if ((bool)downloadButton)
			{
				downloadButton.SetActive(Steam.CanUseWorkshop);
			}
			if ((bool)inspectButton)
			{
				inspectButton.SetActive(_dataRef.CanInspectWorkshopItem());
			}
			if ((bool)watchTutorialButton)
			{
				watchTutorialButton.SetActive(filter.ShowData() && dataType.HasAttribute<YouTubeVideoAttribute>());
			}
		}
	}

	private void _RefreshDataRef()
	{
		ContentRef arg = _dataRef;
		onDataRefChanged.Invoke(null);
		onDataRefChanged.Invoke(arg);
	}

	private void _OnSaveTagChanges()
	{
		if (filter.ShowData())
		{
			OnDataValueChanged();
		}
		else
		{
			_dataRef.SaveFromUIWithoutValidation(data);
		}
	}

	private ContentRef _CreateRef()
	{
		return ReflectionUtil.CreateInstanceSmart(typeof(DataRef<>).MakeGenericType(dataType), new object[1] { ReflectionUtil.CreateInstanceSmart(dataType) }) as ContentRef;
	}

	private void _OnUpload(bool successful)
	{
		if (successful)
		{
			_UpdateButtons();
		}
	}

	private void _OnDownload(ContentRef downloadedContentRef)
	{
		if ((bool)downloadedContentRef)
		{
			SetDataRef(_CreateRef());
			SetDataRef(downloadedContentRef, restoreLiveEditData: false, manuallySelected: true);
		}
	}

	private void Update()
	{
		if (_dataRef != null && _dataRef.isTracked)
		{
			string text = _GetDataFriendlyName();
			if (text != _previousFriendlyName)
			{
				_InvokeContentNameChanged(text);
			}
		}
	}

	private void Awake()
	{
		SetWorkshopButtonTooltips(uploadButton, downloadButton, inspectButton, tagsButton, watchTutorialButton);
		onDataRefChanged?.AddListener(delegate
		{
			DataRefControl.OnDataRefChanged?.Invoke(this);
		});
	}

	private void Start()
	{
		_UpdateButtons();
	}

	private void OnDestroy()
	{
		if (filter.ShowData())
		{
			_ActiveEditKeys.Remove(this);
		}
		if (!(memberInfo == null) && _dataRef != null && filter != DataRefControlFilter.Simple)
		{
			Couple<MemberInfo, ContentRef.Key> key = new Couple<MemberInfo, ContentRef.Key>(memberInfo, _dataRef.GetIdentifier());
			if (data != null && (!_dataRefHasContent || !ProtoUtil.Equal(data, _dataRef.GetContentImmediate())))
			{
				_LiveEdits[key] = data;
			}
			else
			{
				_LiveEdits.Remove(key);
			}
			RemoveRefToControlMap(_dataRef, this);
			data = null;
		}
	}

	[SpecialName]
	Transform IDataRefControl.get_transform()
	{
		return base.transform;
	}
}
