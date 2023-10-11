using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[AddComponentMenu("Event/Smart Input Module")]
public class SmartInputModule : PointerInputModule
{
	[SerializeField]
	private string m_HorizontalAxis = "Horizontal";

	[SerializeField]
	private string m_VerticalAxis = "Vertical";

	[SerializeField]
	private string m_SubmitButton = "Submit";

	[SerializeField]
	private string m_CancelButton = "Cancel";

	[SerializeField]
	private float m_InputActionsPerSecond = 10f;

	[SerializeField]
	private float m_RepeatDelay = 0.5f;

	[SerializeField]
	[FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
	private bool m_ForceModuleActive;

	public bool enableDeepPointerEvents;

	private float m_PrevActionTime;

	private Vector2 m_LastMoveVector;

	private int m_ConsecutiveMoveCount;

	private Vector2 m_LastMousePosition;

	private Vector2 m_MousePosition;

	private GameObject _dragObject;

	private PointerEventData _dragPointerEventData;

	private Dictionary<PointerEventData.InputButton, List<Action<PointerEventData>>> _pointerEventPostProcesses;

	private List<RaycastResult> _raycastResults;

	private Dictionary<PointerEventData.InputButton, List<GameObject>> _deepPointerClickObjects;

	private PointerEventData dragPointerEventData
	{
		get
		{
			return _dragPointerEventData;
		}
		set
		{
			_dragPointerEventData = value;
			_dragObject = value?.pointerDrag;
		}
	}

	public bool forceModuleActive
	{
		get
		{
			return m_ForceModuleActive;
		}
		set
		{
			m_ForceModuleActive = value;
		}
	}

	public float inputActionsPerSecond
	{
		get
		{
			return m_InputActionsPerSecond;
		}
		set
		{
			m_InputActionsPerSecond = value;
		}
	}

	public float repeatDelay
	{
		get
		{
			return m_RepeatDelay;
		}
		set
		{
			m_RepeatDelay = value;
		}
	}

	public string horizontalAxis
	{
		get
		{
			return m_HorizontalAxis;
		}
		set
		{
			m_HorizontalAxis = value;
		}
	}

	public string verticalAxis
	{
		get
		{
			return m_VerticalAxis;
		}
		set
		{
			m_VerticalAxis = value;
		}
	}

	public string submitButton
	{
		get
		{
			return m_SubmitButton;
		}
		set
		{
			m_SubmitButton = value;
		}
	}

	public string cancelButton
	{
		get
		{
			return m_CancelButton;
		}
		set
		{
			m_CancelButton = value;
		}
	}

	private Dictionary<PointerEventData.InputButton, List<Action<PointerEventData>>> pointerEventPostProcesses => _pointerEventPostProcesses ?? (_pointerEventPostProcesses = EnumUtil<PointerEventData.InputButton>.Values.ToDictionary((PointerEventData.InputButton b) => b, (PointerEventData.InputButton b) => new List<Action<PointerEventData>>()));

	private Dictionary<PointerEventData.InputButton, List<GameObject>> deepPointerClickObjects => _deepPointerClickObjects ?? (_deepPointerClickObjects = EnumUtil<PointerEventData.InputButton>.Values.ToDictionary((PointerEventData.InputButton b) => b, (PointerEventData.InputButton b) => new List<GameObject>()));

	private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
	{
		if (useDragThreshold)
		{
			return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
		}
		return true;
	}

	private bool _ShouldIgnoreEventsOnNoFocus()
	{
		OperatingSystemFamily operatingSystemFamily = SystemInfo.operatingSystemFamily;
		if ((uint)(operatingSystemFamily - 1) <= 2u)
		{
			return true;
		}
		return false;
	}

	private bool _ProcessTouchEvents()
	{
		for (int i = 0; i < base.input.touchCount; i++)
		{
			Touch touch = base.input.GetTouch(i);
			if (touch.type != TouchType.Indirect)
			{
				bool pressed;
				bool released;
				PointerEventData touchPointerEventData = GetTouchPointerEventData(touch, out pressed, out released);
				_ProcessTouchPress(touchPointerEventData, pressed, released);
				if (!released)
				{
					ProcessMove(touchPointerEventData);
					ProcessDrag(touchPointerEventData);
				}
				else
				{
					RemovePointerData(touchPointerEventData);
				}
			}
		}
		return base.input.touchCount > 0;
	}

	private void _ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
	{
		GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
		if (pressed)
		{
			pointerEvent.eligibleForClick = true;
			pointerEvent.delta = Vector2.zero;
			pointerEvent.dragging = false;
			pointerEvent.useDragThreshold = true;
			pointerEvent.pressPosition = pointerEvent.position;
			pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
			DeselectIfSelectionChanged(gameObject, pointerEvent);
			if (pointerEvent.pointerEnter != gameObject)
			{
				HandlePointerExitAndEnter(pointerEvent, gameObject);
				pointerEvent.pointerEnter = gameObject;
			}
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == pointerEvent.lastPress)
			{
				if (unscaledTime - pointerEvent.clickTime < InputManager.I.DoubleClickThreshold)
				{
					int clickCount = pointerEvent.clickCount + 1;
					pointerEvent.clickCount = clickCount;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				pointerEvent.clickTime = unscaledTime;
			}
			else
			{
				pointerEvent.clickCount = 1;
			}
			pointerEvent.pointerPress = gameObject2;
			pointerEvent.rawPointerPress = gameObject;
			pointerEvent.clickTime = unscaledTime;
			pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (pointerEvent.pointerDrag != null)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (released)
		{
			ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (pointerEvent.pointerPress == eventHandler && pointerEvent.eligibleForClick)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
			}
			else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.dropHandler);
			}
			pointerEvent.eligibleForClick = false;
			pointerEvent.pointerPress = null;
			pointerEvent.rawPointerPress = null;
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
				dragPointerEventData = null;
			}
			pointerEvent.dragging = false;
			pointerEvent.pointerDrag = null;
			ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
			pointerEvent.pointerEnter = null;
		}
	}

	private bool _SendSubmitEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		if (base.input.GetButtonDown(m_SubmitButton))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
		}
		if (base.input.GetButtonDown(m_CancelButton))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
		}
		return baseEventData.used;
	}

	private Vector2 _GetRawMoveVector()
	{
		Vector2 zero = Vector2.zero;
		zero.x = base.input.GetAxisRaw(m_HorizontalAxis);
		zero.y = base.input.GetAxisRaw(m_VerticalAxis);
		if (base.input.GetButtonDown(m_HorizontalAxis))
		{
			if (zero.x < 0f)
			{
				zero.x = -1f;
			}
			if (zero.x > 0f)
			{
				zero.x = 1f;
			}
		}
		if (base.input.GetButtonDown(m_VerticalAxis))
		{
			if (zero.y < 0f)
			{
				zero.y = -1f;
			}
			if (zero.y > 0f)
			{
				zero.y = 1f;
			}
		}
		return zero;
	}

	private bool _SendMoveEventToSelectedObject()
	{
		float unscaledTime = Time.unscaledTime;
		Vector2 vector = _GetRawMoveVector();
		if (Mathf.Approximately(vector.x, 0f) && Mathf.Approximately(vector.y, 0f))
		{
			m_ConsecutiveMoveCount = 0;
			return false;
		}
		bool flag = base.input.GetButtonDown(m_HorizontalAxis) || base.input.GetButtonDown(m_VerticalAxis);
		bool flag2 = Vector2.Dot(vector, m_LastMoveVector) > 0f;
		if (!flag)
		{
			flag = ((!flag2 || m_ConsecutiveMoveCount != 1) ? (unscaledTime > m_PrevActionTime + 1f / m_InputActionsPerSecond) : (unscaledTime > m_PrevActionTime + m_RepeatDelay));
		}
		if (!flag)
		{
			return false;
		}
		AxisEventData axisEventData = GetAxisEventData(vector.x, vector.y, 0.6f);
		if (axisEventData.moveDir != MoveDirection.None)
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
			if (!flag2)
			{
				m_ConsecutiveMoveCount = 0;
			}
			m_ConsecutiveMoveCount++;
			m_PrevActionTime = unscaledTime;
			m_LastMoveVector = vector;
		}
		else
		{
			m_ConsecutiveMoveCount = 0;
		}
		return axisEventData.used;
	}

	private void _ProcessMouseEvent()
	{
		_ProcessMouseEvent(0);
	}

	private void _ProcessMouseEvent(int id)
	{
		MouseState mousePointerEventData = GetMousePointerEventData(id);
		MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
		_ProcessMousePress(eventData);
		ProcessMove(eventData.buttonData);
		ProcessDrag(eventData.buttonData);
		_ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
		ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
		_ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
		ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
		if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
		{
			ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), eventData.buttonData, ExecuteEvents.scrollHandler);
		}
	}

	private bool _SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	private bool _ShouldDeepPointerContinuePropagation(GameObject go, PointerEventData eventData)
	{
		foreach (IDeepPointerPropagation item in go.GetInterfacesPooled<IDeepPointerPropagation>())
		{
			if (!item.ShouldDeepPointerContinuePropagation(eventData))
			{
				return false;
			}
		}
		return true;
	}

	private void _ExecuteDeepPointerDown(PointerEventData eventData)
	{
		List<RaycastResult> raycastResults = GetRaycastResults(eventData);
		List<GameObject> list = deepPointerClickObjects[eventData.button];
		list.Clear();
		foreach (RaycastResult item in raycastResults)
		{
			if (ExecuteEvents.CanHandleEvent<IDeepPointerEventHandler>(item.gameObject))
			{
				list.Add(item.gameObject);
				ExecuteEvents.Execute(item.gameObject, eventData, UnityEventExtensions.deepPointerDownHandler);
				if (!_ShouldDeepPointerContinuePropagation(item.gameObject, eventData))
				{
					break;
				}
			}
		}
	}

	private void _ExecuteDeepPointerUp(PointerEventData eventData)
	{
		foreach (GameObject item in deepPointerClickObjects[eventData.button])
		{
			ExecuteEvents.Execute(item, eventData, UnityEventExtensions.deepPointerUpHandler);
		}
	}

	private void _ExecuteDeepPointerClick(PointerEventData eventData)
	{
		List<RaycastResult> raycastResults = GetRaycastResults(eventData);
		List<GameObject> list = deepPointerClickObjects[eventData.button];
		foreach (RaycastResult item in raycastResults)
		{
			if (list.Contains(item.gameObject))
			{
				ExecuteEvents.Execute(item.gameObject, eventData, UnityEventExtensions.deepPointerClickHandler);
			}
		}
		list.Clear();
	}

	private void _ProcessMousePress(MouseButtonEventData data)
	{
		PointerEventData buttonData = data.buttonData;
		GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
		if (data.PressedThisFrame())
		{
			buttonData.eligibleForClick = true;
			buttonData.delta = Vector2.zero;
			buttonData.dragging = false;
			buttonData.useDragThreshold = true;
			buttonData.pressPosition = buttonData.position;
			buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
			DeselectIfSelectionChanged(gameObject, buttonData);
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
			if (enableDeepPointerEvents)
			{
				_ExecuteDeepPointerDown(buttonData);
			}
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == buttonData.lastPress)
			{
				if (unscaledTime - buttonData.clickTime < InputManager.I.DoubleClickThreshold)
				{
					int clickCount = buttonData.clickCount + 1;
					buttonData.clickCount = clickCount;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.clickTime = unscaledTime;
			}
			else
			{
				buttonData.clickCount = 1;
			}
			buttonData.pointerPress = gameObject2;
			buttonData.rawPointerPress = gameObject;
			buttonData.clickTime = unscaledTime;
			buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
			}
			if (buttonData.pointerDrag != null)
			{
				base.eventSystem.pixelDragThreshold = InputManager.I.PixelDragThreshold;
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, UnityEventExtensions.dragThresholdSetterHandler);
			}
		}
		if (data.ReleasedThisFrame())
		{
			ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
			if (enableDeepPointerEvents)
			{
				_ExecuteDeepPointerUp(buttonData);
			}
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
			{
				ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
			}
			else if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.dropHandler);
			}
			if (enableDeepPointerEvents)
			{
				_ExecuteDeepPointerClick(buttonData);
			}
			buttonData.eligibleForClick = false;
			buttonData.pointerPress = null;
			buttonData.rawPointerPress = null;
			if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
				dragPointerEventData = null;
			}
			buttonData.dragging = false;
			buttonData.pointerDrag = null;
			if (gameObject != buttonData.pointerEnter)
			{
				HandlePointerExitAndEnter(buttonData, null);
				HandlePointerExitAndEnter(buttonData, gameObject);
			}
		}
		if (_pointerEventPostProcesses == null)
		{
			return;
		}
		List<Action<PointerEventData>> list = _pointerEventPostProcesses[buttonData.button];
		foreach (Action<PointerEventData> item in list)
		{
			item(buttonData);
		}
		list.Clear();
	}

	private bool _ForceEndDrag()
	{
		if (dragPointerEventData == null || !_dragObject)
		{
			return false;
		}
		ExecuteEvents.Execute(_dragObject, dragPointerEventData, ExecuteEvents.pointerUpHandler);
		ExecuteEvents.Execute(_dragObject, dragPointerEventData, ExecuteEvents.endDragHandler);
		ExecuteEvents.ExecuteHierarchy(_dragObject, dragPointerEventData, ExecuteEvents.pointerExitHandler);
		dragPointerEventData = null;
		return true;
	}

	private void LateUpdate()
	{
		if (dragPointerEventData != null && !Input.GetKey(dragPointerEventData.button.ToKeyCode()))
		{
			_ForceEndDrag();
		}
	}

	public override void UpdateModule()
	{
		if (!base.eventSystem.isFocused && _ShouldIgnoreEventsOnNoFocus())
		{
			if (!_ForceEndDrag())
			{
				PointerEventData buttonData = GetMousePointerEventData(-1).GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData;
				if (buttonData != null && (bool)buttonData.pointerPress)
				{
					ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
				}
				if (buttonData != null && (bool)buttonData.pointerEnter)
				{
					ExecuteEvents.ExecuteHierarchy(buttonData.pointerEnter, buttonData, ExecuteEvents.pointerExitHandler);
				}
			}
			m_PointerData.Clear();
		}
		else
		{
			m_LastMousePosition = m_MousePosition;
			m_MousePosition = base.input.mousePosition;
		}
	}

	public override bool IsModuleSupported()
	{
		if (!m_ForceModuleActive && !base.input.mousePresent)
		{
			return base.input.touchSupported;
		}
		return true;
	}

	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule())
		{
			return false;
		}
		bool flag = m_ForceModuleActive;
		flag |= base.input.GetButtonDown(m_SubmitButton);
		flag |= base.input.GetButtonDown(m_CancelButton);
		flag |= !Mathf.Approximately(base.input.GetAxisRaw(m_HorizontalAxis), 0f);
		flag |= !Mathf.Approximately(base.input.GetAxisRaw(m_VerticalAxis), 0f);
		flag |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0f;
		flag |= base.input.GetMouseButtonDown(0);
		if (base.input.touchCount > 0)
		{
			flag = true;
		}
		return flag;
	}

	public override void ActivateModule()
	{
		if (base.eventSystem.isFocused || !_ShouldIgnoreEventsOnNoFocus())
		{
			base.ActivateModule();
			m_MousePosition = base.input.mousePosition;
			m_LastMousePosition = base.input.mousePosition;
			GameObject gameObject = base.eventSystem.currentSelectedGameObject;
			if (gameObject == null)
			{
				gameObject = base.eventSystem.firstSelectedGameObject;
			}
			base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
		}
	}

	public override void DeactivateModule()
	{
		base.DeactivateModule();
		ClearSelection();
	}

	public override void Process()
	{
		if (!base.eventSystem.isFocused && _ShouldIgnoreEventsOnNoFocus())
		{
			return;
		}
		bool flag = _SendUpdateEventToSelectedObject();
		if (base.eventSystem.sendNavigationEvents)
		{
			if (!flag)
			{
				flag |= _SendMoveEventToSelectedObject();
			}
			if (!flag)
			{
				_SendSubmitEventToSelectedObject();
			}
		}
		if (!_ProcessTouchEvents() && base.input.mousePresent)
		{
			_ProcessMouseEvent();
		}
		Pools.TryRepool(ref _raycastResults);
	}

	protected override void ProcessDrag(PointerEventData pointerEvent)
	{
		if (pointerEvent.pointerDrag == null || Cursor.lockState == CursorLockMode.Locked)
		{
			return;
		}
		if (!pointerEvent.dragging && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, base.eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
		{
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
			pointerEvent.dragging = true;
			dragPointerEventData = pointerEvent;
		}
		if (pointerEvent.dragging)
		{
			if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
			}
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
		}
	}

	public GameObject GetPotentialDragObject(int pointerId)
	{
		PointerEventData lastPointerEventData = GetLastPointerEventData(pointerId);
		if (lastPointerEventData == null || !lastPointerEventData.pointerEnter)
		{
			return null;
		}
		return ExecuteEvents.GetEventHandler<IInitializePotentialDragHandler>(lastPointerEventData.pointerEnter);
	}

	public bool IsDragging(int pointerId)
	{
		return GetLastPointerEventData(pointerId)?.dragging ?? false;
	}

	public GameObject GetDragObject(int pointerId)
	{
		PointerEventData lastPointerEventData = GetLastPointerEventData(pointerId);
		if (lastPointerEventData == null || !lastPointerEventData.dragging)
		{
			return null;
		}
		return lastPointerEventData.pointerDrag;
	}

	public GameObject GetLastPointerEnter(int pointerId)
	{
		return GetLastPointerEventData(pointerId)?.pointerEnter;
	}

	public PointerEventData GetPointerData(int pointerId)
	{
		GetPointerData(pointerId, out var data, create: true);
		return data;
	}

	public void AddPointerEventPostProcess(Action<PointerEventData> postProcess, PointerEventData.InputButton mouseButton = PointerEventData.InputButton.Left)
	{
		pointerEventPostProcesses[mouseButton].Add(postProcess);
	}

	public List<RaycastResult> GetRaycastResults(PointerEventData eventData, bool rayCastAllOnUnpool = true)
	{
		if (_raycastResults == null)
		{
			_raycastResults = Pools.TryUnpoolList<RaycastResult>();
			if (rayCastAllOnUnpool)
			{
				base.eventSystem.RaycastAll(eventData, _raycastResults);
			}
		}
		return _raycastResults;
	}

	public void ForceEndDrag()
	{
		_ForceEndDrag();
	}
}
