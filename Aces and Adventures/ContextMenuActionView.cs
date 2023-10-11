using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(PointerOver3D), typeof(PointerClick3D))]
[RequireComponent(typeof(CanvasGroup))]
public class ContextMenuActionView : MonoBehaviour
{
	private static GameObject _Blueprint;

	private static Dictionary<string, Sprite> _IconMap;

	private static readonly List<ContextMenuActionView> _EmptySiblings = new List<ContextMenuActionView>();

	[SerializeField]
	protected BoolEvent _OnIsActiveChange;

	[SerializeField]
	protected StringEvent _OnTextChange;

	[SerializeField]
	protected BoolEvent _OnHasHotKeyChange;

	[SerializeField]
	protected StringEvent _OnHotKeyTextChange;

	[SerializeField]
	protected SpriteEvent _OnIconChange;

	[SerializeField]
	protected BoolEvent _OnShowIconChange;

	[SerializeField]
	protected BoolEvent _OnIsFolderChange;

	private PointerOver3D _pointerOver;

	private PointerClick3D _pointerClick;

	private CanvasGroup _canvasGroup;

	private ContextMenuContext _context;

	private ContextMenuAction _action;

	private ContextMenuWindow _window;

	private ContextMenuWindow _subWindow;

	private PointerOverTriangle _pointerOverTriangle;

	public static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("UI/ContextMenu/ContextMenuActionView");
			}
			return _Blueprint;
		}
	}

	private static Dictionary<string, Sprite> IconMap => _IconMap ?? (_IconMap = new Dictionary<string, Sprite>());

	public RectTransform rectTransform => base.transform as RectTransform;

	public PointerOver3D pointerOver => this.CacheComponent(ref _pointerOver);

	public PointerClick3D pointerClick => this.CacheComponent(ref _pointerClick);

	public CanvasGroup canvasGroup => this.CacheComponent(ref _canvasGroup);

	private PointerOverTriangle pointerOverTriangle => this.CacheComponentInChildren(ref _pointerOverTriangle, includeInactive: true);

	public bool isValid
	{
		get
		{
			if ((bool)this && action != null)
			{
				return _context != null;
			}
			return false;
		}
	}

	public ContextMenuAction action
	{
		get
		{
			return _action;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _action, value))
			{
				_OnContextMenuActionChange();
			}
		}
	}

	public ContextMenuWindow window => this.CacheComponentInParent(ref _window);

	public List<ContextMenuActionView> siblingsActionViews
	{
		get
		{
			if (!window)
			{
				return _EmptySiblings;
			}
			return window.contextMenuActionViews;
		}
	}

	private static Sprite GetIcon(string iconName)
	{
		if (iconName.IsNullOrEmpty())
		{
			return null;
		}
		if (!IconMap.ContainsKey(iconName))
		{
			IconMap.Add(iconName, Resources.Load<Sprite>("UI/ContextMenu/Icons/" + iconName));
		}
		return IconMap[iconName];
	}

	private void Awake()
	{
		_RegisterPointerEvents();
	}

	private void OnDisable()
	{
		_context = null;
		_action = null;
		_window = null;
		_CloseSubWindow();
		_DisablePointerTriangle();
	}

	private void _RegisterPointerEvents()
	{
		pointerOver.OnEnter.AddListener(_OnPointerEnter);
		pointerOver.OnExit.AddListener(_OnPointerExit);
		pointerClick.OnClick.AddListener(_OnPointerClick);
		pointerOverTriangle.onMouseDown += delegate
		{
			_SetSiblingCanvasGroupBlockRaycasts(blockRayCasts: true);
		};
	}

	private void _OnPointerEnter(PointerEventData eventData)
	{
		foreach (ContextMenuActionView siblingsActionView in siblingsActionViews)
		{
			if (siblingsActionView != this)
			{
				siblingsActionView._CloseSubWindow();
			}
		}
		if (isValid && action.isFolder && !_subWindow)
		{
			_RegisterNewActiveMenu(eventData, AContextMenu.CreateContextMenuWindow(_context, action.subActions, rectTransform, window.transform));
		}
		_ActivatePointerTriangle(eventData);
	}

	private void _OnPointerExit(PointerEventData eventData)
	{
		_DisablePointerTriangle();
		if ((bool)_subWindow && !eventData.IsPointerOverGameObject(_subWindow.gameObject))
		{
			_CloseSubWindow();
		}
	}

	private void _OnPointerClick(PointerEventData eventData)
	{
		if (isValid && action.DoAction(_context))
		{
			window.rootWindow.gameObject.SetActive(value: false);
		}
	}

	private void _RegisterNewActiveMenu(PointerEventData eventData, ContextMenuWindow subWindow)
	{
		_CloseSubWindow();
		_subWindow = subWindow;
		_subWindow.OnClose.AddListener(_OnActiveMenuClose);
	}

	private void _OnActiveMenuClose()
	{
		_subWindow.OnClose.RemoveListener(_OnActiveMenuClose);
		_subWindow = null;
		_DisablePointerTriangle();
	}

	private void _ActivatePointerTriangle(PointerEventData eventData)
	{
		if ((bool)_subWindow)
		{
			pointerOverTriangle.SetData(eventData, _subWindow.rectTransform);
			_SetSiblingCanvasGroupBlockRaycasts(blockRayCasts: false);
		}
	}

	private void _DisablePointerTriangle()
	{
		if (pointerOverTriangle.gameObject.activeSelf)
		{
			pointerOverTriangle.gameObject.SetActive(value: false);
			_SetSiblingCanvasGroupBlockRaycasts(blockRayCasts: true);
		}
	}

	private void _SetSiblingCanvasGroupBlockRaycasts(bool blockRayCasts)
	{
		foreach (ContextMenuActionView siblingsActionView in siblingsActionViews)
		{
			if (siblingsActionView != this)
			{
				siblingsActionView.canvasGroup.blocksRaycasts = blockRayCasts;
			}
		}
	}

	private void _CloseSubWindow()
	{
		if ((bool)_subWindow)
		{
			_subWindow.gameObject.SetActive(value: false);
		}
	}

	private void _OnContextMenuActionChange()
	{
		if (isValid)
		{
			_OnIsActiveChange.Invoke(action.IsActive(_context));
			_OnTextChange.Invoke(action.name);
			_OnHasHotKeyChange.Invoke(action.hotKey.HasValue);
			_OnHotKeyTextChange.Invoke(action.hotKey.HasValue ? action.hotKey.Value.ToString() : "");
			Sprite icon = GetIcon(action.icon);
			if ((bool)icon)
			{
				_OnIconChange.Invoke(icon);
			}
			_OnShowIconChange.Invoke(icon);
			_OnIsFolderChange.Invoke(action.isFolder);
		}
	}

	public ContextMenuActionView SetData(ContextMenuContext context, ContextMenuAction action)
	{
		_context = context;
		this.action = action;
		return this;
	}
}
