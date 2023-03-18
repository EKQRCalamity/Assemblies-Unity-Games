using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.UI.Others.MenuLogic;

public class CustomEventInput : MonoBehaviour
{
	private GameObject currentButton;

	private AxisEventData currentAxis;

	public float timeBetweenInputs = 0.15f;

	[Range(0f, 1f)]
	public float deadZone = 0.15f;

	private float timer;

	public GameObject CurrentButton => currentButton;

	private void Update()
	{
		if (timer <= 0f)
		{
			currentAxis = new AxisEventData(EventSystem.current);
			currentButton = EventSystem.current.currentSelectedGameObject;
			if (Input.GetAxisRaw("Vertical") > 0f)
			{
				currentAxis.moveDir = MoveDirection.Up;
				ExecuteEvents.Execute(currentButton, currentAxis, ExecuteEvents.moveHandler);
			}
			else if (Input.GetAxisRaw("Vertical") < 0f)
			{
				currentAxis.moveDir = MoveDirection.Down;
				ExecuteEvents.Execute(currentButton, currentAxis, ExecuteEvents.moveHandler);
			}
			else if (Input.GetAxis("Horizontal") > deadZone)
			{
				currentAxis.moveDir = MoveDirection.Right;
				ExecuteEvents.Execute(currentButton, currentAxis, ExecuteEvents.moveHandler);
			}
			else if (Input.GetAxis("Horizontal") < 0f - deadZone)
			{
				currentAxis.moveDir = MoveDirection.Left;
				ExecuteEvents.Execute(currentButton, currentAxis, ExecuteEvents.moveHandler);
			}
			timer = timeBetweenInputs;
		}
		timer -= Time.deltaTime;
	}
}
