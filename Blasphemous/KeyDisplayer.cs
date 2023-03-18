using Rewired;
using UnityEngine;

public class KeyDisplayer : MonoBehaviour
{
	private void Start()
	{
		ReInput.players.GetPlayer(0).AddInputEventDelegate(ButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
	}

	private void ButtonPressed(InputActionEventData obj)
	{
	}
}
