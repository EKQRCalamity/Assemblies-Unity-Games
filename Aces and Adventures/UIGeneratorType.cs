using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIGeneratorType : MonoBehaviour
{
	public class PersistedData
	{
		private string _category;

		private int? _categoryIndex;

		private Dictionary<string, bool> _collapseData;

		private float? _scrollNormalizedPosition;

		public PersistedData(UIGeneratorType generator)
		{
			_category = generator.activeCategory;
			_categoryIndex = generator.activeCategoryIndex;
			_collapseData = generator._persistedCollapseStates.Clone();
			ScrollRect componentInParent = generator.GetComponentInParent<ScrollRect>(includeInactive: true);
			if ((bool)componentInParent)
			{
				_scrollNormalizedPosition = componentInParent.verticalNormalizedPosition;
			}
		}

		public void SetData(UIGeneratorType generator)
		{
			generator.activeCategory = _category;
			generator.activeCategoryIndex = _categoryIndex;
			generator._persistedCollapseStates = _collapseData.Clone();
			generator._persistedScrollPosition = _scrollNormalizedPosition;
		}
	}

	private static readonly Dictionary<object, UIGeneratorType> _ActiveGeneratedUIsByObject = new Dictionary<object, UIGeneratorType>(ReferenceEqualityComparer<object>.Default);

	private static Dictionary<Type, PersistedData> _PersistedDataByObject;

	public string typeToCreateName;

	public ObjectEvent OnObjectCreated;

	public ObjectEvent OnGenerate;

	public UnityEvent OnValueChanged;

	[Range(0f, 48f)]
	public int categoryIndentPixels = 24;

	public bool showDerivedTypesIfBaseTypeIsConcrete;

	public bool persistData;

	public string activeCategory;

	public string[] categoryIncludeFilter;

	public string[] categoryExcludeFilter;

	private Action rebuildRequest;

	private bool signalChangeRequest;

	private GameObject typeComboBox;

	private Type generatedType;

	private HashSet<Type> requestedTypes = new HashSet<Type>();

	private Dictionary<string, bool> _persistedCollapseStates = new Dictionary<string, bool>();

	private float? _persistedScrollPosition;

	private TreeNode<ReflectTreeData<UIFieldAttribute>> _overrideNode;

	private static Dictionary<Type, PersistedData> PersistedDataByObject => _PersistedDataByObject ?? (_PersistedDataByObject = new Dictionary<Type, PersistedData>());

	public object currentObject { get; private set; }

	public bool disableUIGameObjectCreation { get; set; }

	public TreeNode<ReflectTreeData<UIFieldAttribute>> overrideNode
	{
		get
		{
			return _overrideNode;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _overrideNode, value))
			{
				skipCollapseCaching = value != null;
			}
		}
	}

	public bool skipCollapseCaching { get; set; }

	public int? activeCategoryIndex { get; set; }

	private static event Action<Type> OnRequestValidateByType;

	private static event Action<object, object> OnRequestRefresh;

	public static event Action<object> OnObjectValueChanged;

	public static UIGeneratorType GetActiveUI(object obj)
	{
		if (!_ActiveGeneratedUIsByObject.ContainsKey(obj))
		{
			return null;
		}
		return _ActiveGeneratedUIsByObject[obj];
	}

	public static void Refresh(object objectToRefreshUIFor, object newObjectToRefreshUIFor = null)
	{
		if (UIGeneratorType.OnRequestRefresh != null)
		{
			UIGeneratorType.OnRequestRefresh(objectToRefreshUIFor, newObjectToRefreshUIFor);
		}
	}

	public static UIGeneratorType GetActiveUIForType(Type type)
	{
		foreach (object key in _ActiveGeneratedUIsByObject.Keys)
		{
			if (type == key.GetType())
			{
				return _ActiveGeneratedUIsByObject[key];
			}
		}
		return null;
	}

	public static UIGeneratorType GetActiveUIForType<T>()
	{
		return GetActiveUIForType(typeof(T));
	}

	public static T GetActiveObject<T>()
	{
		foreach (object key in _ActiveGeneratedUIsByObject.Keys)
		{
			if (typeof(T) == key.GetType())
			{
				return (T)key;
			}
		}
		return default(T);
	}

	public static void Validate(object obj)
	{
		UIGeneratorType activeUI = GetActiveUI(obj);
		if ((bool)activeUI)
		{
			activeUI.GenerateFromObject(obj);
		}
	}

	public static Transform GetActiveUITransform(object obj)
	{
		UIGeneratorType activeUI = GetActiveUI(obj);
		if (!activeUI)
		{
			return null;
		}
		return activeUI.transform;
	}

	public static Transform GetActiveUITransform<T>()
	{
		return (from obj in _ActiveGeneratedUIsByObject.Keys
			where typeof(T).IsSameOrSubclass(obj.GetType()) && (bool)_ActiveGeneratedUIsByObject[obj] && _ActiveGeneratedUIsByObject[obj].isActiveAndEnabled && _ActiveGeneratedUIsByObject[obj].transform.childCount > 0
			select _ActiveGeneratedUIsByObject[obj].transform).FirstOrDefault();
	}

	public static void ValidateAllOfType<T>()
	{
		if (UIGeneratorType.OnRequestValidateByType != null)
		{
			UIGeneratorType.OnRequestValidateByType(typeof(T));
		}
	}

	public static void RemovePersistedData(Func<Type, bool> shouldRemove)
	{
		PersistedDataByObject.RemoveKeys(shouldRemove);
	}

	private void Start()
	{
		if (!typeToCreateName.IsNullOrEmpty())
		{
			GenerateFromType(ReflectionUtil.GetTypeFromFriendlyName(typeToCreateName));
		}
	}

	private void Update()
	{
		if (rebuildRequest != null)
		{
			if (overrideNode != null)
			{
				overrideNode = overrideNode.Refresh();
				skipCollapseCaching = false;
			}
			rebuildRequest();
			rebuildRequest = null;
		}
		_ProcessSignalChangeRequest();
	}

	private void OnEnable()
	{
		OnRequestValidateByType += _OnRequestValidateByType;
		OnRequestRefresh += _OnRequestRefresh;
	}

	private void OnDisable()
	{
		OnRequestValidateByType -= _OnRequestValidateByType;
		OnRequestRefresh -= _OnRequestRefresh;
	}

	private void OnDestroy()
	{
		if (persistData)
		{
			_PersistObject(currentObject);
		}
		_RemoveCurrentObjectFromActiveUIsByObject();
	}

	public void GenerateFromType(object typeObject)
	{
		Type type = typeObject as Type;
		if (!(type == null))
		{
			object obj = ((type != typeof(string)) ? Activator.CreateInstance(type.IsConcrete() ? type : GenerateTypeComboBox(type), nonPublic: true) : "");
			OnObjectCreated.Invoke(obj);
			GenerateFromObject(obj);
		}
	}

	public void GenerateFromObject(object obj)
	{
		if (!this)
		{
			return;
		}
		_RemoveCurrentObjectFromActiveUIsByObject();
		if (obj == null)
		{
			_CachePersistedCollapses();
			base.gameObject.DestroyChildren();
			return;
		}
		currentObject = obj;
		generatedType = obj.GetType();
		RectTransform component = GetComponent<RectTransform>();
		if (!persistData || !_RestorePersistedObject(obj))
		{
			_CachePersistedCollapses();
		}
		component.gameObject.DestroyChildren();
		if (disableUIGameObjectCreation)
		{
			return;
		}
		Action onValidate = delegate
		{
			rebuildRequest = delegate
			{
				if (typeComboBox != null)
				{
					GenerateTypeComboBox(obj.GetType());
				}
				GenerateFromObject(obj);
			};
		};
		_ActiveGeneratedUIsByObject[currentObject] = this;
		UIUtil.CreateUIControlsFromObject(component, obj, _SignalChange, onValidate, categoryIndentPixels, overrideNode, activeCategory, delegate(string s)
		{
			activeCategory = s;
		}, categoryIncludeFilter, categoryExcludeFilter, activeCategoryIndex);
		activeCategoryIndex = GetComponentInChildren<CategoryTabContainerTag>()?.GetComponentInChildren<SelectedCategoryTag>()?.transform.GetSiblingIndex();
		if (typeComboBox != null)
		{
			typeComboBox.transform.SetParent(component, worldPositionStays: false);
			typeComboBox.transform.SetAsFirstSibling();
		}
		_UpdatePersistedCollapses();
		_RestorePersistedScrollPosition();
		LayoutRebuilder.ForceRebuildLayoutImmediate(component);
		OnGenerate.Invoke(obj);
		if (!_ProcessSignalChangeRequest())
		{
			_SignalObjectValueChange();
		}
	}

	public void GenerateFromObjectIfEnabled(object obj)
	{
		if (base.isActiveAndEnabled)
		{
			GenerateFromObject(obj);
		}
	}

	private bool _RestorePersistedObject(object obj)
	{
		Type type = obj.GetType();
		if (!PersistedDataByObject.ContainsKey(type))
		{
			return false;
		}
		PersistedDataByObject[type].SetData(this);
		return PersistedDataByObject.Remove(type);
	}

	private void _RestorePersistedScrollPosition()
	{
		if (_persistedScrollPosition.HasValue)
		{
			ScrollRect componentInParent = GetComponentInParent<ScrollRect>(includeInactive: true);
			if ((bool)componentInParent)
			{
				Job.Process(_RestoreScrollPosition(componentInParent));
			}
		}
	}

	private IEnumerator _RestoreScrollPosition(ScrollRect scroll)
	{
		if (_persistedScrollPosition.HasValue && (bool)scroll)
		{
			scroll.verticalNormalizedPosition = _persistedScrollPosition.Value;
		}
		_persistedScrollPosition = null;
		yield break;
	}

	private void _CachePersistedCollapses()
	{
		if (skipCollapseCaching)
		{
			return;
		}
		using PoolKeepItemListHandle<CollapseFitter> poolKeepItemListHandle = Pools.UseKeepItemList<CollapseFitter>();
		base.gameObject.GetComponentsInChildren(includeInactive: false, poolKeepItemListHandle.value);
		foreach (CollapseFitter item in poolKeepItemListHandle.value)
		{
			item.ClearPath();
		}
		foreach (CollapseFitter item2 in poolKeepItemListHandle.value)
		{
			_persistedCollapseStates[item2.GetPath()] = item2.toggleFloat;
		}
	}

	private void _UpdatePersistedCollapses()
	{
		if (skipCollapseCaching)
		{
			return;
		}
		using PoolKeepItemListHandle<CollapseFitter> poolKeepItemListHandle = Pools.UseKeepItemList<CollapseFitter>();
		base.gameObject.GetComponentsInChildren(includeInactive: false, poolKeepItemListHandle.value);
		foreach (CollapseFitter item in poolKeepItemListHandle.value)
		{
			if (_persistedCollapseStates.ContainsKey(item.GetPath()))
			{
				item.toggleFloat = (_persistedCollapseStates[item.GetPath()] ? ToggleFloatData.Open : ToggleFloatData.Close);
			}
		}
	}

	private void _PersistObject(object obj)
	{
		if (obj != null && !skipCollapseCaching)
		{
			_CachePersistedCollapses();
			PersistedDataByObject[obj.GetType()] = new PersistedData(this);
		}
	}

	private void _SignalChange()
	{
		signalChangeRequest = true;
	}

	private void _OnRequestValidateByType(Type type)
	{
		if ((bool)this && currentObject != null && base.transform.childCount > 0 && type.IsSameOrSubclass(currentObject.GetType()))
		{
			GenerateFromObject(currentObject);
		}
	}

	private void _OnRequestRefresh(object obj, object newObject)
	{
		if ((bool)this && currentObject == obj && base.transform.childCount > 0)
		{
			GenerateFromObject(newObject ?? obj);
		}
	}

	private Type GenerateTypeComboBox(Type type)
	{
		requestedTypes.Add(type);
		Type type2 = type.GetBaseClasses().LastOrDefault(requestedTypes.Contains) ?? type;
		IEnumerable<Type> inheritanceHierarchyClasses = type2.GetInheritanceHierarchyClasses(includeBaseType: true, showDerivedTypesIfBaseTypeIsConcrete);
		List<Type> list = (type2.HasAttribute<UIFieldAttribute>() ? (from t in inheritanceHierarchyClasses
			where t.HasAttribute<UIFieldAttribute>()
			orderby t.GetUIOrder()
			select t).ToList() : inheritanceHierarchyClasses.ToList());
		Type type3 = ((list.Count <= 0) ? type : (list.Contains(type) ? type : list[0]));
		if (list.Count > 1)
		{
			typeComboBox = UIUtil.CreateTypeComboBox(type2.FullName, "Select Type", delegate(Type t)
			{
				if (generatedType == null || generatedType != t)
				{
					GenerateFromType(t);
				}
			}, derivedTypesOnly: false, type3.FullName);
		}
		else
		{
			typeComboBox = null;
		}
		return type3;
	}

	public void SetUICategories(UICategorySet categorySet)
	{
		SetUICategories(categorySet.included, categorySet.excluded);
	}

	public void SetUICategories(string[] included, string[] excluded)
	{
		categoryIncludeFilter = included;
		categoryExcludeFilter = excluded;
	}

	private void _SignalObjectValueChange()
	{
		if (UIGeneratorType.OnObjectValueChanged != null && currentObject != null)
		{
			UIGeneratorType.OnObjectValueChanged(currentObject);
		}
	}

	private bool _ProcessSignalChangeRequest()
	{
		if (!signalChangeRequest)
		{
			return false;
		}
		OnValueChanged.Invoke();
		_SignalObjectValueChange();
		signalChangeRequest = false;
		return true;
	}

	private void _RemoveCurrentObjectFromActiveUIsByObject()
	{
		if (currentObject != null)
		{
			_ActiveGeneratedUIsByObject.Remove(currentObject);
		}
	}
}
