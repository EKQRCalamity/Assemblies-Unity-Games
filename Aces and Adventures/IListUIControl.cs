using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IListUIControl : MonoBehaviour, IUIContentContainer
{
	public interface IIListAddWrapper
	{
		TreeNode<ReflectTreeData<UIFieldAttribute>> collectionNode { get; set; }

		object GetValue();
	}

	[UIField("IList Add Wrapper", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class IListAddWrapper<T> : IIListAddWrapper
	{
		[UIField("Item To Add", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public T itemToAdd;

		public TreeNode<ReflectTreeData<UIFieldAttribute>> collectionNode { get; set; }

		public object GetValue()
		{
			return itemToAdd;
		}

		public IListAddWrapper()
		{
			itemToAdd = (T)ReflectionUtil.CreateInstanceSmart<T>();
		}

		protected void OnValidateUI()
		{
			if (itemToAdd != null)
			{
				itemToAdd.InvokeMethod("OnValidateUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
		}
	}

	public const int DEFAULT_PAGE_SIZE = 10;

	private const string DRAG_INDICATOR_NAME = "~=[DRAG INDICATOR]=~";

	private const float DRAG_INDICATOR_HEIGHT_RATIO = 1f;

	private static Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, IListUIControl> _NodeToControlMap;

	private static Dictionary<object, Couple<string, int>> _PersistedSearchData;

	public bool showAddObjectData;

	public float disabledAddButtonAlpha = 0.666f;

	public Transform dragIndicator;

	public Transform previousPageDrag;

	public Transform nextPageDrag;

	private int _maxCount;

	private string _resourcePath;

	private int indentPixels;

	private IList _list;

	private Type _listObjectType;

	private bool _canAddAndSubtract;

	private List<Transform> _removeTransforms = new List<Transform>();

	private int _uiContentParentIndex;

	private Func<object, object> _onPrepareToAdd;

	private Action _onAddOrRemove;

	private Func<object, bool> _excludedValues;

	private object _draggedValue;

	private Color _originalDragIndicatorColor;

	private TreeNode<ReflectTreeData<UIFieldAttribute>> _collectionNode;

	private IListUIControlSearcher _searcher;

	private List<int> _indexMap;

	private List<int> _uiContentParentIndexMap;

	private static Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, IListUIControl> NodeToControlMap => _NodeToControlMap ?? (_NodeToControlMap = new Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, IListUIControl>(ReferenceEqualityComparer<TreeNode<ReflectTreeData<UIFieldAttribute>>>.Default));

	private static Dictionary<object, Couple<string, int>> PersistedSearchData => _PersistedSearchData ?? (_PersistedSearchData = new Dictionary<object, Couple<string, int>>());

	private bool _canAdd
	{
		get
		{
			if (_canAddAndSubtract)
			{
				if (_maxCount > 0)
				{
					return _list.Count < _maxCount;
				}
				return true;
			}
			return false;
		}
	}

	private string _addButtonResourecePath => _resourcePath + " Add";

	private string _removeButtonResourecePath => _resourcePath + " Remove";

	public IListUIControlSearcher searcher
	{
		get
		{
			return _searcher;
		}
		private set
		{
			_searcher = value;
		}
	}

	public TreeNode<ReflectTreeData<UIFieldAttribute>> collectionNode => _collectionNode;

	private object _persistedKey => collectionNode.value.originalCollection ?? collectionNode.value.memberInfo;

	public Transform uiContentParent
	{
		get
		{
			if (_removeTransforms.Count <= 0)
			{
				return base.transform;
			}
			return _removeTransforms[_GetNextUIContentParentIndex()];
		}
	}

	public static IListUIControl GetControlFromCollectionNode(TreeNode<ReflectTreeData<UIFieldAttribute>> collectionNode)
	{
		if (!NodeToControlMap.ContainsKey(collectionNode))
		{
			return null;
		}
		return NodeToControlMap[collectionNode];
	}

	public void Initialize(IList list, Type listObjectType, int maxCount, string resourcePath, Action onAddOrRemove = null, int indentPixels = 24, bool showAddData = false, bool fixedSize = false, Func<object, bool> excludedValues = null, Func<object, object> onPrepareToAdd = null, int? pageSizeOverride = null)
	{
		_collectionNode = UIUtil.ActiveCollectionNode;
		NodeToControlMap[_collectionNode] = this;
		_list = list;
		_listObjectType = listObjectType;
		_maxCount = maxCount;
		_resourcePath = resourcePath;
		_canAddAndSubtract = !fixedSize && !list.IsFixedSize && !list.IsReadOnly;
		if (!_canAddAndSubtract)
		{
			base.gameObject.GetOrAddComponent<IndentFitter>().indentAmountPixels = 24f;
		}
		this.indentPixels = indentPixels;
		showAddObjectData = showAddData;
		if (onAddOrRemove != null)
		{
			_onAddOrRemove = (Action)Delegate.Combine(_onAddOrRemove, onAddOrRemove);
		}
		_onAddOrRemove = (Action)Delegate.Combine(_onAddOrRemove, (Action)delegate
		{
			if ((bool)searcher)
			{
				searcher.isDirty = false;
			}
		});
		_excludedValues = excludedValues;
		_onPrepareToAdd = onPrepareToAdd;
		_RefreshAddButton();
		_RefreshSearchBar(pageSizeOverride ?? 10);
		_RefreshRemoveButtons();
	}

	private void _RefreshRemoveButtons()
	{
		if (!_canAddAndSubtract)
		{
			return;
		}
		_uiContentParentIndex = 0;
		_removeTransforms.Clear();
		bool mapIndices = _indexMap != null;
		int num = (mapIndices ? _indexMap.Count : _list.Count);
		for (int j = 0; j < num; j++)
		{
			int unmappedIndex = j;
			int i = ((!mapIndices) ? j : _indexMap[j]);
			GameObject gameObject = UIUtil.GetGameObject(_removeButtonResourecePath, base.transform);
			if (indentPixels > 0)
			{
				gameObject.AddComponent<IndentFitter>().indentAmountPixels = indentPixels;
			}
			_removeTransforms.Add(gameObject.transform);
			Transform current = gameObject.transform;
			Button[] componentsInChildren = current.gameObject.GetComponentsInChildren<Button>(includeInactive: true);
			Button obj = componentsInChildren[0];
			obj.onClick.RemoveAllListeners();
			obj.onClick.AddListener(delegate
			{
				if (InputManager.I[KeyModifiers.Alt])
				{
					if (_canAdd)
					{
						_Add(ProtoUtil.CloneNonGeneric(_list[i]), InputManager.I[KeyModifiers.Shift] ? new int?(i + 1) : null);
					}
				}
				else
				{
					_list.RemoveAt(i);
					_onAddOrRemove();
				}
			});
			Button dragButton = componentsInChildren[1];
			if (dragButton.gameObject.HasComponent<EventTrigger>())
			{
				continue;
			}
			UIUtil.AddEventHandler(dragButton.gameObject, EventTriggerType.BeginDrag, delegate(PointerEventData pointerData)
			{
				_draggedValue = _list[i];
				List<CollapseFitter> list = (from collapser in current.gameObject.GetComponentsInChildren<CollapseFitter>(includeInactive: true)
					where collapser.isOpen
					select collapser).ToList();
				if (list.Count > 0)
				{
					list.EffectAll(delegate(CollapseFitter collapse)
					{
						collapse.ForceClose();
					});
					LayoutRebuilder.ForceRebuildLayoutImmediate(current as RectTransform);
					Canvas.ForceUpdateCanvases();
				}
				GameObject gameObject2 = UIUtil.DragClone(dragButton.gameObject, current.gameObject, null, pointerData, 0.25f, 0.75f);
				if ((bool)previousPageDrag)
				{
					IListUIControlSearcher listUIControlSearcher3 = searcher;
					if ((object)listUIControlSearcher3 != null && listUIControlSearcher3.pageNumber > 1 && !searcher.searchText.HasVisibleCharacter())
					{
						previousPageDrag.gameObject.SetActiveAndReturn(active: true).transform.SetParentAndReturn(base.transform, worldPositionStays: false).SetAsLastSibling();
					}
				}
				if ((bool)nextPageDrag && searcher?.pageNumber < searcher?.maxPageNumber && !searcher.searchText.HasVisibleCharacter())
				{
					nextPageDrag.gameObject.SetActiveAndReturn(active: true).transform.SetParentAndReturn(base.transform, worldPositionStays: false).SetAsLastSibling();
				}
				if (dragIndicator != null)
				{
					dragIndicator.SetParent(base.transform, worldPositionStays: false);
					dragIndicator.GetComponent<Graphic>().color = _originalDragIndicatorColor;
					dragIndicator.gameObject.SetActive(value: true);
					dragIndicator.gameObject.GetOrAddComponent<LayoutElement>().preferredHeight = LayoutUtility.GetPreferredHeight(gameObject2.transform as RectTransform) * 1f;
				}
			});
			UIUtil.AddEventHandler(dragButton.gameObject, EventTriggerType.Drag, delegate(PointerEventData pointerData)
			{
				if (_OnDrag(unmappedIndex, pointerData))
				{
					_OnDrag(unmappedIndex, pointerData);
				}
			});
			UIUtil.AddEventHandler(dragButton.gameObject, EventTriggerType.EndDrag, delegate(PointerEventData pointerData)
			{
				if (_GetDragIListControl(pointerData) == this)
				{
					int num2 = 0;
					bool flag = mapIndices;
					Transform obj2 = previousPageDrag;
					if ((object)obj2 != null && obj2.gameObject?.activeInHierarchy == true && RectTransformUtility.RectangleContainsScreenPoint(previousPageDrag as RectTransform, Input.mousePosition, pointerData.pressEventCamera) && !(flag = false))
					{
						num2 = _indexMap[0] - 1;
						IListUIControlSearcher listUIControlSearcher = searcher;
						int pageNumber = listUIControlSearcher.pageNumber - 1;
						listUIControlSearcher.pageNumber = pageNumber;
					}
					else
					{
						Transform obj3 = nextPageDrag;
						if ((object)obj3 != null && obj3.gameObject?.activeInHierarchy == true && RectTransformUtility.RectangleContainsScreenPoint(nextPageDrag as RectTransform, Input.mousePosition, pointerData.pressEventCamera) && !(flag = false))
						{
							num2 = _indexMap[_indexMap.Count - 1] + 1;
							IListUIControlSearcher listUIControlSearcher2 = searcher;
							int pageNumber = listUIControlSearcher2.pageNumber + 1;
							listUIControlSearcher2.pageNumber = pageNumber;
						}
						else
						{
							num2 = _GetDragInsertIndex(this, unmappedIndex, pointerData);
						}
					}
					object value = _list[i];
					_list.RemoveAt(i);
					_list.Insert((!flag) ? num2 : _indexMap[num2], value);
				}
				if ((bool)previousPageDrag)
				{
					previousPageDrag.gameObject.SetActive(value: false);
				}
				if ((bool)nextPageDrag)
				{
					nextPageDrag.gameObject.SetActive(value: false);
				}
				_onAddOrRemove();
			});
		}
	}

	private void _RefreshAddButton()
	{
		if (!_canAddAndSubtract)
		{
			return;
		}
		GameObject gameObject = UIUtil.GetGameObject(_addButtonResourecePath, base.transform);
		if (_canAdd)
		{
			gameObject.GetComponent<CanvasGroup>().Disable(refreshGameObjectActive: true);
		}
		UIGeneratorType uiGeneratorType = gameObject.GetComponentInChildren<UIGeneratorType>(includeInactive: true);
		uiGeneratorType.skipCollapseCaching = true;
		uiGeneratorType.disableUIGameObjectCreation = !showAddObjectData || !_canAdd;
		IIListAddWrapper iIListAddWrapper = ReflectionUtil.CreateInstanceSmart(typeof(IListAddWrapper<>).MakeGenericType(_listObjectType)) as IIListAddWrapper;
		iIListAddWrapper.collectionNode = _collectionNode;
		uiGeneratorType.GenerateFromObject(iIListAddWrapper);
		if (!_canAdd)
		{
			return;
		}
		UnityAction onAddRequested = delegate
		{
			_Add((uiGeneratorType.currentObject as IIListAddWrapper).GetValue(), InputManager.I[KeyModifiers.Shift] ? new int?(0) : null);
		};
		UIList componentInChildren = uiGeneratorType.GetComponentInChildren<UIList>();
		if ((bool)componentInChildren)
		{
			componentInChildren.onManuallySelected += delegate
			{
				onAddRequested();
			};
		}
		else
		{
			DataRefControl componentInChildren2 = uiGeneratorType.GetComponentInChildren<DataRefControl>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.onManuallySelected += delegate
				{
					onAddRequested();
				};
				CollapseFitter componentInChildren3 = componentInChildren2.GetComponentInChildren<CollapseFitter>();
				if ((bool)componentInChildren3)
				{
					componentInChildren3.ForceOpen();
				}
			}
			else
			{
				TMP_InputField componentInChildren4 = uiGeneratorType.GetComponentInChildren<TMP_InputField>();
				if ((bool)componentInChildren4)
				{
					componentInChildren4.onSubmit.AddListener(delegate
					{
						onAddRequested();
					});
				}
			}
		}
		gameObject.GetComponentInChildrenOnly<Button>().onClick.AddListener(onAddRequested);
	}

	private IListUIControl _GetDragIListControl(PointerEventData pointerData)
	{
		GameObject gameObject = pointerData.pointerCurrentRaycast.gameObject;
		if (gameObject == null)
		{
			return null;
		}
		IListUIControlContainer componentInParent = gameObject.GetComponentInParent<IListUIControlContainer>();
		if (componentInParent == null)
		{
			return null;
		}
		IListUIControl componentInChildren = componentInParent.gameObject.GetComponentInChildren<IListUIControl>();
		if (componentInChildren == null || componentInChildren._listObjectType != _listObjectType)
		{
			return null;
		}
		return componentInChildren;
	}

	private int _GetDragInsertIndex(IListUIControl iListControl, int startDragIndex, PointerEventData pointerData)
	{
		int num = 0;
		List<Transform> removeTransforms = _removeTransforms;
		if (removeTransforms.Count == 0)
		{
			return num;
		}
		bool flag = this == iListControl;
		if (!flag)
		{
			startDragIndex = removeTransforms.Count + 1;
		}
		RectTransformUtility.ScreenPointToWorldPointInRectangle(iListControl.transform as RectTransform, pointerData.position, pointerData.pressEventCamera, out var worldPoint);
		worldPoint = iListControl.transform.worldToLocalMatrix.MultiplyPoint(worldPoint);
		for (int i = 0; i < removeTransforms.Count; i++)
		{
			num = i;
			if (worldPoint.y >= removeTransforms[i].GetSiblingTransformRelativeTo<IListUIControl>().localPosition.y)
			{
				break;
			}
		}
		Vector3 localPosition = removeTransforms[num].GetSiblingTransformRelativeTo<IListUIControl>().localPosition;
		if (num > startDragIndex && worldPoint.y > localPosition.y)
		{
			num--;
		}
		if (num < startDragIndex && worldPoint.y < localPosition.y)
		{
			num++;
		}
		return Mathf.Clamp(num, 0, removeTransforms.Count - (flag ? 1 : 0));
	}

	private bool _OnDrag(int startDragIndex, PointerEventData pointerData)
	{
		if (_GetDragIListControl(pointerData) != this)
		{
			dragIndicator.gameObject.SetActive(value: false);
			return false;
		}
		dragIndicator.gameObject.SetActive(value: true);
		int num = _GetDragInsertIndex(this, startDragIndex, pointerData);
		int num2 = Math.Sign(num - startDragIndex);
		if (num2 == 0)
		{
			dragIndicator.gameObject.SetActive(value: false);
			return false;
		}
		List<Transform> removeTransforms = _removeTransforms;
		int num3 = ((removeTransforms.Count > num) ? (removeTransforms[num].GetSiblingIndexRelativeTo<IListUIControl>() + num2) : (base.transform.childCount - 1));
		if (dragIndicator.GetSiblingIndex() != num3)
		{
			dragIndicator.GetComponent<Graphic>().color = ((!_CanAddOnDrag(this, _draggedValue)) ? Color.red : _originalDragIndicatorColor);
			dragIndicator.SetSiblingIndex(num3);
			return true;
		}
		return false;
	}

	private bool _CanAddOnDrag(IListUIControl targetIList, object objectToAdd)
	{
		if (targetIList == null)
		{
			return false;
		}
		if (this == targetIList)
		{
			return true;
		}
		if (targetIList._canAdd)
		{
			if (targetIList._excludedValues != null)
			{
				return !targetIList._excludedValues(objectToAdd);
			}
			return true;
		}
		return false;
	}

	private void _RefreshSearchBar(int pageSize)
	{
		if (_list.Count > pageSize)
		{
			searcher = IListUIControlSearcher.Create(_list, pageSize, base.transform, _GetPersistedSearchData());
			searcher.ForceUpdate();
			_OnSearchIndexMapChange(searcher.indexMap);
		}
	}

	private void _OnSearchIndexMapChange(List<int> searchIndexMap)
	{
		_indexMap = searchIndexMap;
		_uiContentParentIndexMap = (from i in searchIndexMap
			orderby i
			select searchIndexMap.IndexOf(i)).ToList();
		using PoolKeepItemHashSetHandle<int> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(_indexMap);
		for (int j = 0; j < _collectionNode.children.Count; j++)
		{
			_collectionNode.children[j].value.shouldHide = !poolKeepItemHashSetHandle.Contains(j);
		}
	}

	private int _GetNextUIContentParentIndex()
	{
		if (_uiContentParentIndexMap != null)
		{
			return _uiContentParentIndexMap[_uiContentParentIndex++];
		}
		return _uiContentParentIndex++;
	}

	private void _SetPersistedSearchData()
	{
		if ((bool)searcher && _list.Count > searcher.pageSize)
		{
			PersistedSearchData[_persistedKey] = new Couple<string, int>(searcher.searchText, searcher.pageNumber);
		}
	}

	private Couple<string, int>? _GetPersistedSearchData()
	{
		if (!PersistedSearchData.ContainsKey(_persistedKey))
		{
			return null;
		}
		Couple<string, int> value = PersistedSearchData[_persistedKey];
		PersistedSearchData.Remove(_persistedKey);
		return value;
	}

	private void _Add(object valueToAdd, int? insertIndex = null)
	{
		if (_onPrepareToAdd != null)
		{
			valueToAdd = _onPrepareToAdd(valueToAdd);
		}
		int num = insertIndex ?? _list.Count;
		_list.Insert(num, valueToAdd);
		IListUIControlSearcher listUIControlSearcher = searcher;
		if ((object)listUIControlSearcher != null && listUIControlSearcher.isActiveAndEnabled)
		{
			searcher.underlyingPageNumber = num / 10 + 1;
		}
		_onAddOrRemove();
	}

	public void RefreshForSearch()
	{
		_onAddOrRemove();
	}

	private void Awake()
	{
		if (dragIndicator != null)
		{
			dragIndicator.name = "~=[DRAG INDICATOR]=~";
			_originalDragIndicatorColor = dragIndicator.GetComponent<Graphic>().color;
		}
	}

	private void OnDisable()
	{
		_SetPersistedSearchData();
	}

	private void OnDestroy()
	{
		if (_collectionNode != null)
		{
			NodeToControlMap.Remove(_collectionNode);
		}
	}
}
