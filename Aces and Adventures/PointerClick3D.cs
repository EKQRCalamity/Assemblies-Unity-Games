using UnityEngine;
using UnityEngine.EventSystems;

public class PointerClick3D : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler
{
	[Header("Left Click")]
	[SerializeField]
	protected PointerEvent _OnDown;

	[SerializeField]
	protected PointerEvent _OnUp;

	[SerializeField]
	protected PointerEvent _OnClick;

	[Header("Right Click")]
	[SerializeField]
	protected PointerEvent _OnRightDown;

	[SerializeField]
	protected PointerEvent _OnRightUp;

	[SerializeField]
	protected PointerEvent _OnRightClick;

	[Header("Middle Click")]
	[SerializeField]
	protected PointerEvent _OnMiddleDown;

	[SerializeField]
	protected PointerEvent _OnMiddleUp;

	[SerializeField]
	protected PointerEvent _OnMiddleClick;

	public PointerEvent OnDown => _OnDown ?? (_OnDown = new PointerEvent());

	public PointerEvent OnUp => _OnUp ?? (_OnUp = new PointerEvent());

	public PointerEvent OnClick => _OnClick ?? (_OnClick = new PointerEvent());

	public PointerEvent OnRightDown => _OnRightDown ?? (_OnRightDown = new PointerEvent());

	public PointerEvent OnRightUp => _OnRightUp ?? (_OnRightUp = new PointerEvent());

	public PointerEvent OnRightClick => _OnRightClick ?? (_OnRightClick = new PointerEvent());

	public PointerEvent OnMiddleDown => _OnMiddleDown ?? (_OnMiddleDown = new PointerEvent());

	public PointerEvent OnMiddleUp => _OnMiddleUp ?? (_OnMiddleUp = new PointerEvent());

	public PointerEvent OnMiddleClick => _OnMiddleClick ?? (_OnMiddleClick = new PointerEvent());

	public void OnPointerDown(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			OnDown.Invoke(eventData);
			break;
		case PointerEventData.InputButton.Right:
			OnRightDown.Invoke(eventData);
			break;
		case PointerEventData.InputButton.Middle:
			OnMiddleDown.Invoke(eventData);
			break;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			OnUp.Invoke(eventData);
			break;
		case PointerEventData.InputButton.Right:
			OnRightUp.Invoke(eventData);
			break;
		case PointerEventData.InputButton.Middle:
			OnMiddleUp.Invoke(eventData);
			break;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			OnClick.Invoke(eventData);
			break;
		case PointerEventData.InputButton.Right:
			OnRightClick.Invoke(eventData);
			break;
		case PointerEventData.InputButton.Middle:
			OnMiddleClick.Invoke(eventData);
			break;
		}
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public void FakeOnClick()
	{
		OnClick.Invoke(null);
	}

	public void FakeOnClickIfActive()
	{
		if (this.IsActiveAndEnabled())
		{
			FakeOnClick();
		}
	}
}
