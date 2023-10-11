using UnityEngine;
using UnityEngine.Events;

public class MenuBackHook : MonoBehaviour
{
	[SerializeField]
	protected UnityEvent _onBackRequest;

	public UnityEvent onBackRequest => _onBackRequest ?? (_onBackRequest = new UnityEvent());

	private void Update()
	{
		if ((InputManager.I[KeyCode.Escape][KState.Clicked] || InputManager.I[KeyAction.Back][KState.Clicked]) && !CanvasInputFocus.HasActiveComponents && !InputManager.IsDraggingUI)
		{
			onBackRequest.Invoke();
		}
	}
}
