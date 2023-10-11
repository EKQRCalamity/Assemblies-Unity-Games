using UnityEngine;
using UnityEngine.Events;

public class StartHook : MonoBehaviour
{
	public UnityEvent OnStart;

	private void Start()
	{
		OnStart.Invoke();
	}
}
