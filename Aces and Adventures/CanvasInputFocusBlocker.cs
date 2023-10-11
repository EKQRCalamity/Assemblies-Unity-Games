using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CanvasInputFocusBlocker : MonoBehaviour
{
	private static readonly HashSet<CanvasInputFocusBlocker> _Active = new HashSet<CanvasInputFocusBlocker>();

	public static bool IsBlocking => _Active.Count > 0;

	private void OnEnable()
	{
		_Active.Add(this);
	}

	private void OnDisable()
	{
		_Active.Remove(this);
	}
}
