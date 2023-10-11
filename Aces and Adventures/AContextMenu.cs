using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(CanvasInputFocus))]
public abstract class AContextMenu : MonoBehaviour, IDeepPointerClickHandler, IDeepPointerEventHandler, IEventSystemHandler
{
	private static GameObject _WindowBlueprint;

	private static GameObject _DividerBlueprint;

	[SerializeField]
	protected Camera _eventCamera;

	private List<ContextMenuAction> _rootActions;

	private List<ContextMenuAction> _rootActionsInContext;

	private List<ContextMenuAction> _hotKeyedActions;

	private ContextMenuContext _context;

	private ContextMenuWindow _activeMenu;

	private CanvasInputFocus _canvasInputFocus;

	private bool _isContextDirty;

	private static GameObject WindowBlueprint
	{
		get
		{
			if (!_WindowBlueprint)
			{
				return _WindowBlueprint = Resources.Load<GameObject>("UI/ContextMenu/ContextMenuWindow");
			}
			return _WindowBlueprint;
		}
	}

	private static GameObject DividerBlueprint
	{
		get
		{
			if (!_DividerBlueprint)
			{
				return _DividerBlueprint = Resources.Load<GameObject>("UI/ContextMenu/ContextMenuDivider");
			}
			return _DividerBlueprint;
		}
	}

	public RectTransform rectTransform => base.transform as RectTransform;

	protected List<ContextMenuAction> rootActions => _rootActions ?? (_rootActions = ContextMenuAction.ParseActions(_GetActions()));

	protected List<ContextMenuAction> rootActionsInContext
	{
		get
		{
			return _rootActionsInContext ?? _UpdateActiveActions();
		}
		set
		{
			if (value == null)
			{
				Pools.Repool(ref _rootActionsInContext);
			}
		}
	}

	protected List<ContextMenuAction> hotKeyedActions
	{
		get
		{
			return _hotKeyedActions ?? _UpdateHotKeyedActions();
		}
		set
		{
			if (value == null)
			{
				Pools.Repool(ref _hotKeyedActions);
			}
		}
	}

	public ContextMenuContext context
	{
		get
		{
			return _context ?? (_context = ContextMenuContext.Create());
		}
		private set
		{
			if (SetPropertyUtility.SetObjectEQ(ref _context, value, null, repoolCurrentValueOnChange: true))
			{
				_SetContextDirty();
			}
		}
	}

	public Camera eventCamera
	{
		get
		{
			if (!_eventCamera)
			{
				return _eventCamera = GetComponentInParent<Canvas>().worldCamera;
			}
			return _eventCamera;
		}
		set
		{
			_eventCamera = value;
		}
	}

	private CanvasInputFocus canvasInputFocus => this.CacheComponent(ref _canvasInputFocus);

	public static PoolKeepItemListHandle<GameObject> CreateUIObjects(ContextMenuContext context, List<ContextMenuAction> contextMenuActions, Transform parent = null)
	{
		PoolKeepItemListHandle<GameObject> poolKeepItemListHandle = Pools.UseKeepItemList<GameObject>();
		using PoolDictionaryValuesHandle<string, List<ContextMenuAction>> poolDictionaryValuesHandle = Pools.UseDictionaryValues<string, List<ContextMenuAction>>();
		foreach (ContextMenuAction contextMenuAction in contextMenuActions)
		{
			if (contextMenuAction.IsInContext(context))
			{
				poolDictionaryValuesHandle.value.GetOrAdd(contextMenuAction.category, Pools.TryUnpoolList<ContextMenuAction>).Add(contextMenuAction);
			}
		}
		int num = 0;
		DictionaryValueEnumerator<string, List<ContextMenuAction>>.Enumerator enumerator2 = poolDictionaryValuesHandle.Values().GetEnumerator();
		while (enumerator2.MoveNext())
		{
			foreach (ContextMenuAction item in enumerator2.Current)
			{
				poolKeepItemListHandle.Add(Pools.Unpool(ContextMenuActionView.Blueprint, parent).GetComponent<ContextMenuActionView>().SetData(context, item)
					.gameObject);
				}
				if (++num < poolDictionaryValuesHandle.Count)
				{
					poolKeepItemListHandle.Add(Pools.Unpool(DividerBlueprint, parent));
				}
			}
			return poolKeepItemListHandle;
		}

		public static ContextMenuWindow CreateContextMenuWindow(ContextMenuContext context, List<ContextMenuAction> contextMenuActions, Vector2 rectLerp, RectTransform anchorRect, Vector2 anchorRectLerp, Transform parent = null, bool flipRectLerpXOnOutOfBounds = true, bool flipRectLerpYOnOutOfBounds = true)
		{
			GameObject gameObject = Pools.Unpool(WindowBlueprint, parent);
			foreach (GameObject item in CreateUIObjects(context, contextMenuActions, gameObject.transform))
			{
				_ = item;
			}
			return gameObject.GetComponent<ContextMenuWindow>().SetData(rectLerp, anchorRect, anchorRectLerp, flipRectLerpXOnOutOfBounds, flipRectLerpYOnOutOfBounds);
		}

		public static ContextMenuWindow CreateContextMenuWindow(ContextMenuContext context, List<ContextMenuAction> contextMenuActions, RectTransform anchorRect, Transform parent = null)
		{
			Vector2 up = Vector2.up;
			Vector2 anchorRectLerp = Vector2.one;
			ContextMenuWindow componentInParent = anchorRect.GetComponentInParent<ContextMenuWindow>();
			bool flipRectLerpXOnOutOfBounds = componentInParent;
			if (!componentInParent)
			{
				Rect3D worldRect3D = anchorRect.GetWorldRect3D();
				anchorRectLerp = worldRect3D.GetLerpAmount(worldRect3D.GetPointOnPlaneClosestTo(context.mouseWorldPosition));
			}
			return CreateContextMenuWindow(context, contextMenuActions, up, anchorRect, anchorRectLerp, parent ? parent : anchorRect, flipRectLerpXOnOutOfBounds);
		}

		protected virtual void Update()
		{
			_UpdateDirtyContext();
			if (!canvasInputFocus.hasFocus)
			{
				return;
			}
			foreach (ContextMenuAction hotKeyedAction in hotKeyedActions)
			{
				if (InputManager.I[hotKeyedAction.hotKey] && (!hotKeyedAction.mouseMustBeInRectForHotKey || context.IsMousePositionInRect(rectTransform, eventCamera)) && hotKeyedAction.DoAction(context))
				{
					break;
				}
			}
		}

		protected void _MarkContextDirty()
		{
			_isContextDirty = true;
		}

		private void _SetContextDirty()
		{
			List<ContextMenuAction> list2 = (hotKeyedActions = null);
			rootActionsInContext = list2;
		}

		private void _UpdateDirtyContext()
		{
			if (_isContextDirty)
			{
				_SetContextDirty();
				_isContextDirty = false;
			}
		}

		private List<ContextMenuAction> _UpdateActiveActions()
		{
			Pools.TryRefresh(ref _rootActionsInContext);
			foreach (ContextMenuAction rootAction in rootActions)
			{
				if (rootAction.IsInContext(context))
				{
					_rootActionsInContext.Add(rootAction);
				}
			}
			return _rootActionsInContext;
		}

		private List<ContextMenuAction> _UpdateHotKeyedActions()
		{
			Pools.TryRefresh(ref _hotKeyedActions);
			foreach (ContextMenuAction item in rootActionsInContext)
			{
				foreach (ContextMenuAction item2 in item.GetActionsRecursive())
				{
					if (item2.hotKey.HasValue && (item == item2 || item2.IsInContext(context)) && item2.IsActive(context))
					{
						_hotKeyedActions.Add(item2);
					}
				}
			}
			_hotKeyedActions.Sort(ContextMenuActionHotKeyComparer.Default);
			return _hotKeyedActions;
		}

		private void _RegisterNewActiveMenu(ContextMenuWindow activeMenu)
		{
			Close();
			_activeMenu = activeMenu;
			_activeMenu.OnClose.AddListenerUnique(_OnActiveMenuClose);
		}

		private void _OnActiveMenuClose()
		{
			if ((bool)_activeMenu)
			{
				_activeMenu.OnClose.RemoveListener(_OnActiveMenuClose);
				_activeMenu = null;
			}
		}

		protected abstract IEnumerable<ContextMenuAction> _GetActions();

		public void Open(PointerEventData eventData)
		{
			_UpdateDirtyContext();
			_RegisterNewActiveMenu(CreateContextMenuWindow(context.SetMousePosition(eventData.dragging ? new Vector3?(eventData.GetWorldPositionOnPlane(base.transform)) : eventData.GetWorldPositionOfPress()), rootActionsInContext, (eventData.pointerPress ? eventData.pointerPress : base.gameObject).transform as RectTransform));
		}

		public void Close()
		{
			if ((bool)_activeMenu)
			{
				_activeMenu.gameObject.SetActive(value: false);
			}
		}

		public void UpdateSelection(List<Component> selection)
		{
			context = ContextMenuContext.Create(selection, context.mouseWorldPosition);
		}

		public void OnDeepPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				Open(eventData);
			}
			else if (eventData.button == PointerEventData.InputButton.Left)
			{
				Close();
			}
		}
	}
