using System;
using UnityEngine;

public class PendingActionSignaler : MonoBehaviour
{
	public Action action;

	private void LateUpdate()
	{
		if (action != null)
		{
			action();
			action = null;
		}
	}
}
