using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[ScriptOrder(-32768)]
public class InputManager : MonoBehaviour
{
	public class KeyBindManager
	{
		public KeyActionFlags consumedActions;

		private Dictionary<KeyAction, KeyCode[]> _keyCodes = new Dictionary<KeyAction, KeyCode[]>();

		private Dictionary<KeyAction, KeyState> _actionStates;

		private Dictionary<KeyAction, KeyState> _toggleActionStates = new Dictionary<KeyAction, KeyState>();

		private KeyActionFlags? _toggledActions;

		public KeyActionFlags toggledActions
		{
			get
			{
				KeyActionFlags? keyActionFlags = _toggledActions;
				if (!keyActionFlags.HasValue)
				{
					KeyActionFlags? keyActionFlags2 = (_toggledActions = EnumUtil<KeyAction>.ConvertToFlags<KeyActionFlags>(_toggleActionStates.Keys));
					return keyActionFlags2.Value;
				}
				return keyActionFlags.GetValueOrDefault();
			}
			private set
			{
				_toggledActions = value;
			}
		}

		public KeyState this[KeyAction action]
		{
			get
			{
				if (!_toggleActionStates.ContainsKey(action))
				{
					return _actionStates[action];
				}
				return _toggleActionStates[action];
			}
		}

		public bool this[KeyAction actionA, KeyAction actionB, KState state]
		{
			get
			{
				if (!this[actionA][state])
				{
					return this[actionB][state];
				}
				return true;
			}
		}

		public KeyBindManager()
		{
			_InitActionStates();
		}

		private void _InitActionStates()
		{
			_actionStates = EnumUtil<KeyAction>.Values.ToDictionary((KeyAction action) => action, (KeyAction action) => new KeyState());
		}

		private bool _GetAction(KeyAction action, KeyActionFlags blockedActions)
		{
			if (_keyCodes.ContainsKey(action) && !EnumUtil.HasFlagConvert(blockedActions, action))
			{
				KeyCode[] array = _keyCodes[action];
				for (int i = 0; i < array.Length; i++)
				{
					if (Input.GetKey(array[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		public KeyBindManager CopyFrom(KeyBindManager copyFrom)
		{
			_keyCodes = copyFrom._keyCodes;
			toggledActions = copyFrom.toggledActions;
			foreach (KeyAction item in _toggleActionStates.EnumerateKeysSafe())
			{
				if (!copyFrom._toggleActionStates.ContainsKey(item))
				{
					_toggleActionStates.Remove(item);
				}
			}
			foreach (KeyAction key in copyFrom._toggleActionStates.Keys)
			{
				if (!_toggleActionStates.ContainsKey(key))
				{
					_toggleActionStates.Add(key, new KeyState());
				}
				_toggleActionStates[key].CopyFrom(copyFrom._toggleActionStates[key]);
			}
			return this;
		}

		public void Clear()
		{
			consumedActions = (KeyActionFlags)0;
			_toggledActions = null;
			foreach (KeyState value in _actionStates.Values)
			{
				value.Clear();
			}
			foreach (KeyState value2 in _toggleActionStates.Values)
			{
				value2.Clear();
			}
		}

		public KeyActionFlags ClearKeyActions(KeyActionFlags clearFlags)
		{
			foreach (KeyValuePair<KeyAction, KeyState> actionState in _actionStates)
			{
				if (EnumUtil.HasFlagConvert(clearFlags, actionState.Key))
				{
					actionState.Value.Clear();
				}
			}
			foreach (KeyValuePair<KeyAction, KeyState> toggleActionState in _toggleActionStates)
			{
				if (EnumUtil.HasFlagConvert(clearFlags, toggleActionState.Key))
				{
					toggleActionState.Value.Clear();
				}
			}
			return EnumUtil.Subtract(clearFlags, consumedActions);
		}

		public void SetBindings(IEnumerable<ProfileOptions.ControlOptions.KeyBind> keyBinds)
		{
			_keyCodes.Clear();
			foreach (ProfileOptions.ControlOptions.KeyBind item in keyBinds.Where((ProfileOptions.ControlOptions.KeyBind keyBind) => keyBind))
			{
				_keyCodes.Add(item.action, item.GetKeyCodes().ToArray());
			}
		}

		public void SetActionToggleEnabled(KeyAction action, bool isToggle)
		{
			if (isToggle)
			{
				if (!_toggleActionStates.ContainsKey(action))
				{
					_toggleActionStates.Add(action, new KeyState());
				}
			}
			else
			{
				_toggleActionStates.Remove(action);
			}
			_toggledActions = null;
		}

		public KeyActionFlags Update(KeyActionFlags blockedActions)
		{
			foreach (KeyValuePair<KeyAction, KeyState> actionState in _actionStates)
			{
				actionState.Value.Update(_GetAction(actionState.Key, blockedActions));
			}
			foreach (KeyValuePair<KeyAction, KeyState> toggleActionState in _toggleActionStates)
			{
				toggleActionState.Value.Update(_actionStates[toggleActionState.Key].justPressed ? (!toggleActionState.Value.isDown) : toggleActionState.Value.isDown);
			}
			return consumedActions | blockedActions;
		}
	}

	public class KeyStateAxis
	{
		private KeyState _minAxis;

		private KeyState _maxAxis;

		private float? _acceleration;

		private KState _listenState;

		private float _currentValue;

		public float value => _currentValue;

		public float targetValue => (_minAxis[_listenState] ? (-1) : 0) + (_maxAxis[_listenState] ? 1 : 0);

		public float? acceleration
		{
			get
			{
				return _acceleration;
			}
			set
			{
				_acceleration = value;
			}
		}

		public KeyStateAxis(KeyState minAxis, KeyState maxAxis, float? acceleration = null, KState listenState = KState.Down)
		{
			_minAxis = minAxis;
			_maxAxis = maxAxis;
			_acceleration = acceleration;
			_listenState = listenState;
		}

		public void Update()
		{
			_currentValue = (_acceleration.HasValue ? MathUtil.Ease(_currentValue, targetValue, _acceleration.Value, Time.unscaledDeltaTime) : targetValue);
		}

		public static implicit operator float(KeyStateAxis axis)
		{
			return axis.value;
		}
	}

	public class KeyStateAxes
	{
		private KeyStateAxis _xAxis;

		private KeyStateAxis _yAxis;

		public Vector2 targetValue => new Vector2(_xAxis.targetValue, _yAxis.targetValue);

		public Vector2 rawValue => new Vector2(_xAxis, _yAxis);

		public Vector2 value => rawValue.ClampMagnitude();

		public float? acceleration
		{
			get
			{
				return _xAxis.acceleration;
			}
			set
			{
				_xAxis.acceleration = value;
				_yAxis.acceleration = value;
			}
		}

		public KeyStateAxes(KeyStateAxis xAxis, KeyStateAxis yAxis)
		{
			_xAxis = xAxis;
			_yAxis = yAxis;
		}

		public void Update()
		{
			_xAxis.Update();
			_yAxis.Update();
		}

		public static implicit operator Vector2(KeyStateAxes axes)
		{
			return axes.value;
		}
	}

	public const KeyActionFlags BUBBLED_ACTIONS = (KeyActionFlags)0;

	private static InputManager _instance;

	private static ResourceBlueprint<Texture2D> _CursorDisabled;

	private static ResourceBlueprint<Texture2D> _CursorDragging;

	private static ResourceBlueprint<Texture2D> _CursorDown;

	private static ResourceBlueprint<Texture2D> _CursorCanDrag;

	public static ResourceBlueprint<Texture2D> CursorAttack;

	public static ResourceBlueprint<Texture2D> CursorDefend;

	public static ResourceBlueprint<Texture2D> CursorBack;

	public float ClickThreshold = 0.25f;

	public float DragDistanceThreshold = 1f;

	public float DoubleClickThreshold = 0.5f;

	public int PixelDragThreshold = 5;

	public bool hideCursorWhileRightClickDragging = true;

	private Dictionary<KeyCode, KeyState> KeyStates;

	private KeyBindManager _keyBindManager;

	private List<KeyBindManager> _inputStack = new List<KeyBindManager>();

	private Dictionary<object, KeyBindManager> _inputStackMap = new Dictionary<object, KeyBindManager>(ObjectReferenceEqualityComparator.Default);

	private HashSet<UnityEngine.Object> _disableCameraInputRequests = new HashSet<UnityEngine.Object>();

	private HashSet<object> _disableEventSystemRequests = new HashSet<object>(ReferenceEqualityComparer<object>.Default);

	private HashSet<KeyCode> _onKeyClickedEventKeyCodes = new HashSet<KeyCode>();

	private KeyModifiers _keyModifiers;

	private bool _wasDragging;

	private List<KeyValuePair<object, SpecialCursorImage>> _cursorOverrides = new List<KeyValuePair<object, SpecialCursorImage>>();

	private HashSet<object> _cursorOverridesToClear = new HashSet<object>();

	[HideInInspector]
	public KeyActionFlags disabledKeyActions;

	private EventSystem _eventSystem;

	private static Plane _OriginPlane;

	public static InputManager I => ManagerUtil.GetSingletonInstance(ref _instance);

	public static bool EventSystemEnabled
	{
		get
		{
			if ((bool)I && (bool)I.eventSystem)
			{
				return I.eventSystem.enabled;
			}
			return false;
		}
	}

	public static EventSystem EventSystem
	{
		get
		{
			if (!I)
			{
				return null;
			}
			return I.eventSystem;
		}
	}

	public static bool IsDraggingUI
	{
		get
		{
			if ((bool)I)
			{
				return I.isDraggingUI;
			}
			return false;
		}
	}

	public bool hasFocus { get; private set; }

	public Vector3 MousePosition { get; private set; }

	public Vector3 LastMousePosition { get; private set; }

	public KeyBindManager bindings => _keyBindManager ?? (_keyBindManager = new KeyBindManager());

	public KeyModifiers keyModifiers => _keyModifiers;

	public EventSystem eventSystem
	{
		get
		{
			if (!_eventSystem)
			{
				if (!EventSystem.current)
				{
					return _eventSystem;
				}
				return _eventSystem = EventSystem.current;
			}
			return _eventSystem;
		}
	}

	public bool uiHasMouseFocus
	{
		get
		{
			if (!eventSystem.IsPointerOverGameObject())
			{
				return eventSystem.IsPointerDragging();
			}
			return true;
		}
	}

	public bool isDraggingUI => eventSystem.IsPointerDragging();

	public KeyState this[KeyCode key]
	{
		get
		{
			if (!KeyStates.ContainsKey(key))
			{
				return KeyStates[key] = new KeyState(Input.GetKey(key));
			}
			return KeyStates[key];
		}
	}

	public bool this[KeyCode keyA, KeyCode keyB, KState state]
	{
		get
		{
			if (!this[keyA][state])
			{
				return this[keyB][state];
			}
			return true;
		}
	}

	public bool this[KeyCode keyA, KeyCode keyB, KeyCode keyC, KState state]
	{
		get
		{
			if (!this[keyA][state] && !this[keyB][state])
			{
				return this[keyC][state];
			}
			return true;
		}
	}

	public KeyState this[KeyAction action] => bindings[action];

	public bool this[KeyAction actionA, KeyAction actionB, KState state] => bindings[actionA, actionB, state];

	public KeyBindManager this[object owner] => _inputStackMap[owner];

	public bool this[KeyModifiers modifiers] => (keyModifiers & modifiers) == modifiers;

	public bool this[HotKey hotKey]
	{
		get
		{
			if (this[hotKey.key][KState.JustPressed])
			{
				return this[hotKey.modifiers];
			}
			return false;
		}
	}

	public bool this[HotKey? hotKey]
	{
		get
		{
			if (hotKey.HasValue)
			{
				return this[hotKey.Value];
			}
			return false;
		}
	}

	public KeyState this[PointerEventData.InputButton button] => this[button.ToKeyCode()];

	public PoolKeepItemListHandle<KeyState> this[PointerInputButtonFlags buttonFlags]
	{
		get
		{
			PoolKeepItemListHandle<KeyState> poolKeepItemListHandle = Pools.UseKeepItemList<KeyState>();
			EnumerateFlags<PointerInputButtonFlags>.Enumerator enumerator = EnumUtil.Flags(buttonFlags).GetEnumerator();
			while (enumerator.MoveNext())
			{
				PointerInputButtonFlags current = enumerator.Current;
				poolKeepItemListHandle.Add(this[EnumUtil<PointerInputButtonFlags>.ConvertFromFlag<PointerEventData.InputButton>(current)]);
			}
			return poolKeepItemListHandle;
		}
	}

	private event Action<KeyCode> _OnKeyClicked;

	public event Action<KeyCode> OnKeyClicked
	{
		add
		{
			_OnKeyClicked += value;
		}
		remove
		{
			_OnKeyClicked -= value;
			if (this._OnKeyClicked == null)
			{
				_onKeyClickedEventKeyCodes.Clear();
			}
		}
	}

	static InputManager()
	{
		_CursorDisabled = "UI/Cursor/CursorDisabled";
		_CursorDragging = "UI/Cursor/CursorDrag";
		_CursorDown = "UI/Cursor/CursorDown";
		_CursorCanDrag = "UI/Cursor/CursorCanDrag";
		CursorAttack = "UI/Cursor/Special/CursorAttack";
		CursorDefend = "UI/Cursor/Special/CursorDefend";
		CursorBack = "UI/Cursor/Special/CursorBack";
		_OriginPlane = new Plane(Vector3.up, Vector3.zero);
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new KeyBindManager(), delegate(KeyBindManager keybinds)
		{
			keybinds.Clear();
		});
	}

	public static void RequestInput(object owner, KeyActionFlags bubbledActions = (KeyActionFlags)0)
	{
		if ((bool)I)
		{
			I._RequestInputStack(owner, bubbledActions);
		}
	}

	public static void ReleaseInput(object owner)
	{
		if ((bool)I)
		{
			I._ReleaseInputStack(owner);
		}
	}

	public static void RegisterInput(bool request, object owner, KeyActionFlags bubbledActions = (KeyActionFlags)0)
	{
		if (request)
		{
			RequestInput(owner, bubbledActions);
		}
		else
		{
			ReleaseInput(owner);
		}
	}

	public static void SetEventSystemEnabled(object owner, bool enabled)
	{
		if ((bool)I && (enabled ? I._disableEventSystemRequests.Remove(owner) : I._disableEventSystemRequests.Add(owner)) && I._disableEventSystemRequests.Count <= 1)
		{
			I.eventSystem.SetEnabled(I._disableEventSystemRequests.Count == 0);
		}
	}

	public Vector3 MouseVelocity(Vector3 referenceWorldPosition)
	{
		referenceWorldPosition = Camera.main.WorldToScreenPoint(referenceWorldPosition);
		return (Camera.main.ScreenToWorldPoint(MousePosition.SetAxis(AxisType.Z, referenceWorldPosition.z)) - Camera.main.ScreenToWorldPoint(LastMousePosition.SetAxis(AxisType.Z, referenceWorldPosition.z))) / Time.unscaledDeltaTime.InsureNonZero();
	}

	private void _GetKeyDowns(HashSet<KeyCode> keyCodes)
	{
		KeyCode[] values = EnumUtil<KeyCode>.Values;
		foreach (KeyCode keyCode in values)
		{
			if (Input.GetKeyDown(keyCode))
			{
				keyCodes.Add(keyCode);
			}
		}
	}

	private KeyCode? _GetFirstKeyUp(HashSet<KeyCode> downKeyCodes)
	{
		foreach (KeyCode downKeyCode in downKeyCodes)
		{
			if (Input.GetKeyUp(downKeyCode))
			{
				return downKeyCode;
			}
		}
		return null;
	}

	private void _UpdateOnKeyClickedEvent()
	{
		if (this._OnKeyClicked != null)
		{
			_GetKeyDowns(_onKeyClickedEventKeyCodes);
			KeyCode? keyCode = _GetFirstKeyUp(_onKeyClickedEventKeyCodes);
			if (keyCode.HasValue)
			{
				_onKeyClickedEventKeyCodes.Remove(keyCode.Value);
				this._OnKeyClicked(keyCode.Value);
			}
		}
	}

	private KeyModifiers _GetKeyModifiers()
	{
		KeyModifiers a = EnumUtil<KeyModifiers>.NoFlags;
		EnumerateFlags<KeyModifiers>.Enumerator enumerator = EnumUtil<KeyModifiers>.Flags(EnumUtil<KeyModifiers>.AllFlags).GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyModifiers current = enumerator.Current;
			KeyCode[] keyCodes = EnumUtil<KeyModifiers>.ConvertFromFlag<KeyModifier>(current).GetKeyCodes();
			foreach (KeyCode key in keyCodes)
			{
				if (this[key][KState.Down])
				{
					EnumUtil.Add(ref a, current);
					break;
				}
			}
		}
		return a;
	}

	private void _UpdateMouseContext()
	{
		Texture2D texture2D = null;
		Vector2 hotspot = Vector2.zero;
		bool flag = eventSystem.IsShowPointerDragging();
		if (!EventSystemEnabled)
		{
			texture2D = _CursorDisabled;
		}
		else
		{
			if (flag)
			{
				texture2D = _CursorDragging;
			}
			else if (eventSystem.IsOverShowCanDragItem())
			{
				texture2D = _CursorCanDrag;
			}
			if (_wasDragging && !texture2D)
			{
				texture2D = _CursorDragging;
			}
			if (this[PointerEventData.InputButton.Left].isDown || this[PointerEventData.InputButton.Right].isDown || this[PointerEventData.InputButton.Middle].isDown)
			{
				texture2D = ((texture2D == (Texture2D)_CursorCanDrag) ? ((Texture2D)_CursorDragging) : (texture2D ? texture2D : ((Texture2D)_CursorDown)));
			}
			if (texture2D == (Texture2D)_CursorCanDrag || texture2D == (Texture2D)_CursorDragging)
			{
				hotspot = new Vector2(5f, 2f);
			}
		}
		_wasDragging = flag;
		SpecialCursorImage? specialCursor = _cursorOverrides.LastValue()?.Value;
		if (new Rect(0f, 0f, Screen.width, Screen.height).Contains(Input.mousePosition))
		{
			Cursor.SetCursor(specialCursor.HasValue ? specialCursor.GetTexture() : texture2D, hotspot, CursorMode.Auto);
		}
		for (int num = _cursorOverrides.Count - 1; num >= 0; num--)
		{
			if (_cursorOverridesToClear.Contains(_cursorOverrides[num].Key))
			{
				_cursorOverrides.RemoveAt(num);
			}
		}
		_cursorOverridesToClear.Clear();
		int visible;
		if (hideCursorWhileRightClickDragging)
		{
			PointerEventData pointerData = eventSystem.GetPointerData(-2);
			visible = ((pointerData == null || !pointerData.dragging || pointerData.button != PointerEventData.InputButton.Right) ? 1 : 0);
		}
		else
		{
			visible = 1;
		}
		Cursor.visible = (byte)visible != 0;
	}

	private void Awake()
	{
		KeyStates = new Dictionary<KeyCode, KeyState>(KeyCodeEqualityComparer.Default);
	}

	private void OnEnable()
	{
		LastMousePosition = Input.mousePosition;
	}

	private void Update()
	{
		eventSystem.SetEnabled(_disableEventSystemRequests.Count == 0);
		hasFocus = !eventSystem.InputFieldHasFocus() && EventSystemEnabled;
		_disableCameraInputRequests.RemoveAllDestroyed();
		LastMousePosition = MousePosition;
		MousePosition = Input.mousePosition;
		DictionaryPairEnumerator<KeyCode, KeyState>.Enumerator enumerator = KeyStates.EnumeratePairs().GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<KeyCode, KeyState> current = enumerator.Current;
			current.Value.Update(hasFocus && Input.GetKey(current.Key));
		}
		_keyModifiers = _GetKeyModifiers();
		KeyActionFlags blockedActions = (hasFocus ? disabledKeyActions : EnumUtil<KeyActionFlags>.AllFlags);
		for (int num = _inputStack.Count - 1; num >= 0; num--)
		{
			blockedActions = _inputStack[num].Update(blockedActions);
		}
		bindings.Update(blockedActions);
		_UpdateOnKeyClickedEvent();
	}

	private void LateUpdate()
	{
		_UpdateMouseContext();
	}

	public bool IsClick(float timeHeld)
	{
		return timeHeld < ClickThreshold;
	}

	public bool IsDoubleClick(float timeSinceLastClick)
	{
		return timeSinceLastClick < DoubleClickThreshold;
	}

	public bool IsHeld(float timeHeld)
	{
		return timeHeld >= ClickThreshold;
	}

	public bool MouseInputButtonState(KState state)
	{
		return this[KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, state];
	}

	public bool AnyKeyModifierDown(KeyModifiers? specificKeyModifiersToCheck = null)
	{
		return EnumUtil.HasFlag(specificKeyModifiersToCheck ?? EnumUtil<KeyModifiers>.AllFlags, keyModifiers);
	}

	public void RequestCursorOverride(object requester, SpecialCursorImage cursor)
	{
		_cursorOverridesToClear.Remove(requester);
		for (int num = _cursorOverrides.Count - 1; num >= 0; num--)
		{
			if (_cursorOverrides[num].Key == requester)
			{
				_cursorOverrides.RemoveAt(num);
			}
		}
		_cursorOverrides.Add(new KeyValuePair<object, SpecialCursorImage>(requester, cursor));
	}

	public void ReleaseCursorOverride(object requester)
	{
		_cursorOverridesToClear.Add(requester);
	}

	public GameObject GetGameObjectAtScreenPosition(out bool blockedByUI, Vector3 screenPosition, int layerMask = -1, Camera camera = null, bool uiIsBlocking = true)
	{
		blockedByUI = false;
		if (uiIsBlocking && eventSystem.IsPointerOverGameObject())
		{
			blockedByUI = true;
			return null;
		}
		RaycastHit? raycastHit = (camera ? camera : CameraManager.Instance.mainCamera).RayCastScreenPosition(screenPosition, MathUtil.LargeNumber, layerMask);
		if (!raycastHit.HasValue)
		{
			return null;
		}
		return raycastHit.Value.transform.gameObject;
	}

	public GameObject GetGameObjectAtMouse(out bool blockedByUI, int layerMask = -1, Camera camera = null, bool uiIsBlocking = true)
	{
		return GetGameObjectAtScreenPosition(out blockedByUI, Input.mousePosition, layerMask, camera, uiIsBlocking);
	}

	public Short2 GetMouseGridCoordinates()
	{
		Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
		_OriginPlane.Raycast(ray, out var enter);
		return new Short2(ray.origin + ray.direction * enter);
	}

	public void RequestCameraInputDisabled(UnityEngine.Object obj)
	{
		_disableCameraInputRequests.Add(obj);
	}

	public void ReleaseCameraInputDisabledRequest(UnityEngine.Object obj)
	{
		_disableCameraInputRequests.Remove(obj);
	}

	public void SetCameraInputDisabledRequest(UnityEngine.Object obj, bool disableCameraInput)
	{
		if (disableCameraInput)
		{
			RequestCameraInputDisabled(obj);
		}
		else
		{
			ReleaseCameraInputDisabledRequest(obj);
		}
	}

	private void _RequestInputStack(object owner, KeyActionFlags bubbledActions)
	{
		if (!_inputStackMap.ContainsKey(owner))
		{
			KeyBindManager keyBindManager = Pools.Unpool<KeyBindManager>().CopyFrom(bindings);
			keyBindManager.consumedActions = EnumUtil<KeyActionFlags>.AllFlagsExcept(bubbledActions | bindings.toggledActions);
			_inputStackMap.Add(owner, keyBindManager);
			_inputStack.Add(keyBindManager);
			KeyActionFlags keyActionFlags = keyBindManager.consumedActions;
			int num = _inputStack.Count - 2;
			while (num >= 0 && keyActionFlags != 0)
			{
				keyActionFlags = _inputStack[num].ClearKeyActions(keyActionFlags);
				num--;
			}
			if (keyActionFlags != 0)
			{
				bindings.ClearKeyActions(keyActionFlags);
			}
		}
	}

	private void _ReleaseInputStack(object owner)
	{
		if (_inputStackMap.ContainsKey(owner))
		{
			KeyBindManager item = _inputStackMap[owner];
			_inputStack.Remove(item);
			_inputStackMap.Remove(owner);
			Pools.Repool(item);
		}
	}

	public IEnumerable<KeyBindManager> AllKeybindManagers()
	{
		for (int x = _inputStack.Count - 1; x >= 0; x--)
		{
			yield return _inputStack[x];
		}
		yield return _keyBindManager;
	}
}
