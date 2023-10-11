using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerOver3D : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	protected PointerEvent _OnEnter;

	[SerializeField]
	protected PointerEvent _OnExit;

	public PointerEvent OnEnter => _OnEnter ?? (_OnEnter = new PointerEvent());

	public PointerEvent OnExit => _OnExit ?? (_OnExit = new PointerEvent());

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnEnter.Invoke(eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnExit.Invoke(eventData);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public PointerOver3D SetEvents(UnityAction<PointerEventData> enter, UnityAction<PointerEventData> exit)
	{
		OnEnter.AddListener(enter);
		OnExit.AddListener(exit);
		return this;
	}
}
